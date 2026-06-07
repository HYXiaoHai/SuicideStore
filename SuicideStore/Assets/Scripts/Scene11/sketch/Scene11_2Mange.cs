using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Scene11_2Mange : MonoBehaviour
{
    [Header("每轮结束后替换的素材")]
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;
    public SpriteRenderer sprite3;
    public SpriteRenderer sprite4;

    public Material defaultMaterial; // 默认材质（恢复用）

    [Header("场景跳转")]
    public string nextSceneName;

    private enum RoundState { Drawing, WaitForClick, WaitForFinalClick }
    private RoundState currentState = RoundState.Drawing;
    private int currentRound = 0;          // 0: 第一幅绘画, 1: 第二幅绘画
    private bool[] roundCompleted = new bool[2];

    private SpriteRenderer currentSpriteRenderer;
    private DrawingController currentDrawingCtrl;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f,Color.black);

        // 初始化所有画板（隐藏、禁用）
        InitAllPanels();

        // 启用第一轮绘画
        currentDrawingCtrl = GetDrawingControllerByRound(0);
        if (currentDrawingCtrl != null)
        {
            currentDrawingCtrl.gameObject.SetActive(true);
            currentSpriteRenderer = currentDrawingCtrl.targetSprite;
            currentDrawingCtrl.enabled = true;
        }
        else
        {
            Debug.LogError("未找到第一轮的 DrawingController！");
        }

        // 设置 Sprite 显隐状态
        if (sprite1 != null) sprite1.gameObject.SetActive(true);
        if (sprite2 != null) sprite2.gameObject.SetActive(false);
        if (sprite3 != null) sprite3.gameObject.SetActive(false);

        currentState = RoundState.Drawing;
    }

    private void InitAllPanels()
    {
        if (DrawManage.instance == null || DrawManage.instance.drawingControllers == null)
        {
            Debug.LogError("DrawManage 未初始化或 drawingControllers 列表为空");
            return;
        }
        foreach (var ctrl in DrawManage.instance.drawingControllers)
        {
            if (ctrl != null)
            {
                ctrl.gameObject.SetActive(false);
                ctrl.enabled = false;
            }
        }
    }

    void Update()
    {
        if (currentState == RoundState.Drawing)
        {
            if (!roundCompleted[currentRound] && currentDrawingCtrl != null && currentDrawingCtrl.IsCompleted)
            {
                roundCompleted[currentRound] = true;
                OnDrawingComplete();
            }
        }
        else if (currentState == RoundState.WaitForClick)
        {
            if (Input.GetMouseButtonUp(0) && IsMouseOverCurrentSprite())
            {
                OnPhotoClick();
            }
        }
        else if (currentState == RoundState.WaitForFinalClick)
        {
            if (Input.GetMouseButtonUp(0) && IsMouseOverCurrentSprite())
            {
                TransitionManage.Instance.FadeOut(0.5f, Color.white, () =>
                {
                    // 转场完成后加载新场景
                    SceneManager.LoadScene(nextSceneName);
                });
            }
        }
    }

    private void OnDrawingComplete()
    {
        if (currentDrawingCtrl.currentLoopingSound != null)
        {
            AudioManager.Instance.StopLoopingSound(currentDrawingCtrl.currentLoopingSound);
            currentDrawingCtrl.currentLoopingSound = null;
        }

        currentDrawingCtrl.enabled = false;
        currentState = RoundState.WaitForClick;
        Debug.Log($"第{currentRound + 1}轮绘画完成，等待点击照片");
    }

    private void OnPhotoClick()
    {
        if (currentRound == 0)
        {
            // 第一轮：渐隐 sprite1，渐显 sprite2
            if (sprite1 != null)
                sprite1.DOFade(0f, 1f);
            if (sprite2 != null)
            {
                sprite2.gameObject.SetActive(true);
                sprite2.DOFade(1f, 1f);
            }
            if (sprite3 != null)
            {
                sprite3.gameObject.SetActive(true);
                sprite3.DOFade(1f, 1f);
            }
            StartCoroutine(SwitchToNextRoundAfterDelay(1.2f));
        }
        else if (currentRound == 1)
        {
            // 第二轮：渐隐 sprite2，渐显 sprite3，然后进入最终等待点击
            if (sprite2 != null)
                sprite2.DOFade(0f, 1f);
            if (sprite3 != null)
                sprite3.DOFade(0f, 1f);
            if (sprite4 != null)
            {
                sprite4.gameObject.SetActive(true);
                sprite4.DOFade(1f, 1f);
            }
            currentSpriteRenderer = sprite3; // 将点击检测对象改为 sprite3
            currentState = RoundState.WaitForFinalClick;
            Debug.Log("第二轮绘画完成，等待点击最终照片跳转");
        }
    }

    private IEnumerator SwitchToNextRoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        int nextRound = currentRound + 1;
        if (nextRound >= 2) // 只有两轮
        {
            Debug.LogWarning("已经是最后一轮，无法切换");
            yield break;
        }

        // 隐藏当前画板
        currentDrawingCtrl.gameObject.SetActive(false);

        // 获取并激活下一个画板
        DrawingController nextCtrl = GetDrawingControllerByRound(nextRound);
        if (nextCtrl == null)
        {
            Debug.LogError($"未找到第{nextRound + 1}轮的 DrawingController");
            yield break;
        }

        nextCtrl.gameObject.SetActive(true);
        nextCtrl.enabled = true;

        // 更新引用
        currentDrawingCtrl = nextCtrl;
        currentSpriteRenderer = nextCtrl.targetSprite;
        currentRound = nextRound;

        currentState = RoundState.Drawing;
        Debug.Log($"切换到第{currentRound + 1}轮，开始绘画");
    }

    private DrawingController GetDrawingControllerByRound(int round)
    {
        if (DrawManage.instance == null || DrawManage.instance.drawingControllers == null)
            return null;
        if (round < 0 || round >= DrawManage.instance.drawingControllers.Count)
            return null;
        return DrawManage.instance.drawingControllers[round];
    }

    private bool IsMouseOverCurrentSprite()
    {
        if (currentSpriteRenderer == null) return false;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = currentSpriteRenderer.GetComponent<Collider2D>();
        if (collider != null)
            return collider.OverlapPoint(mouseWorldPos);
        else
        {
            Bounds bounds = currentSpriteRenderer.bounds;
            return bounds.Contains(mouseWorldPos);
        }
    }
}