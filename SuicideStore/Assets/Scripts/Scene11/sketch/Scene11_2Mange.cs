using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class Scene11_2Mange : MonoBehaviour
{
    [Header("每轮结束后替换的素材")]
    public SpriteRenderer sprite1;
    public SpriteRenderer sprite2;
    public SpriteRenderer sprite3;
    public SpriteRenderer sprite4;

    public Material defaultMaterial;

    [Header("场景跳转")]
    public string nextSceneName;

    [Header("空格提示")]
    public TMP_Text waitPrompt;

    private enum RoundState { Drawing, WaitForClick, WaitForFinalClick }
    private RoundState currentState = RoundState.Drawing;
    private int currentRound = 0;
    private bool[] roundCompleted = new bool[2];

    private SpriteRenderer currentSpriteRenderer;
    public DrawingController currentDrawingCtrl;

    private bool isWaitingForSpace = false;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        InitAllPanels();

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

        if (sprite1 != null) sprite1.gameObject.SetActive(true);
        if (sprite2 != null) sprite2.gameObject.SetActive(false);
        if (sprite3 != null) sprite3.gameObject.SetActive(false);

        if (waitPrompt != null)
        {
            waitPrompt.alpha = 0f;
            waitPrompt.gameObject.SetActive(false);
        }

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
            if (Input.GetKeyDown(KeyCode.Space) && !isWaitingForSpace)
            {
                OnPhotoClick();
            }
        }
        else if (currentState == RoundState.WaitForFinalClick)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isWaitingForSpace)
            {
                TransitionManage.Instance.FadeOut(0.5f, Color.white, () =>
                {
                    SceneManager.LoadScene(nextSceneName);
                });
            }
        }
    }

    private void OnDrawingComplete()
    {
        Debug.Log("绘画完成");
        if (currentDrawingCtrl != null)
            currentDrawingCtrl.StopDrawingSound();

        currentState = RoundState.WaitForClick;
        ShowPrompt();
        Debug.Log($"第{currentRound + 1}轮绘画完成，按空格继续");
    }

    private void OnPhotoClick()
    {
        HidePrompt();

        if (currentRound == 0)
        {
            if (sprite1 != null)
                sprite1.DOFade(0f, 1f).OnComplete(() => { sprite1.gameObject.SetActive(false); });
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
            // 第二轮：渐隐 sprite2 和 sprite3，渐显 sprite4
            if (sprite2 != null)
                sprite2.DOFade(0f, 1f);
            if (sprite3 != null)
                sprite3.DOFade(0f, 1f);
            if (sprite4 != null)
            {
                sprite4.gameObject.SetActive(true);
                sprite4.DOFade(1f, 1f);
            }
            currentSpriteRenderer = sprite3;
            currentState = RoundState.WaitForFinalClick;

            // 延迟后再显示最终提示，避免与 HidePrompt 冲突
            StartCoroutine(DelayedShowPrompt(0.5f));
            Debug.Log("第二轮绘画完成，按空格跳转场景");
        }
    }

    private IEnumerator SwitchToNextRoundAfterDelay(float delay)
    {
        if (currentDrawingCtrl != null)
            currentDrawingCtrl.StopDrawingSound();

        yield return new WaitForSeconds(delay);
        int nextRound = currentRound + 1;
        if (nextRound >= 2)
        {
            Debug.LogWarning("已经是最后一轮，无法切换");
            yield break;
        }

        currentDrawingCtrl.gameObject.SetActive(false);

        DrawingController nextCtrl = GetDrawingControllerByRound(nextRound);
        if (nextCtrl == null)
        {
            Debug.LogError($"未找到第{nextRound + 1}轮的 DrawingController");
            yield break;
        }

        nextCtrl.gameObject.SetActive(true);
        nextCtrl.enabled = true;

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

    private void ShowPrompt()
    {
        if (waitPrompt == null) return;
        waitPrompt.gameObject.SetActive(true);
        waitPrompt.alpha = 0f;
        waitPrompt.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
    }

    private void HidePrompt()
    {
        if (waitPrompt == null) return;
        waitPrompt.DOFade(0f, 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            waitPrompt.gameObject.SetActive(false);
        });
    }

    private IEnumerator DelayedShowPrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowPrompt();
    }
}