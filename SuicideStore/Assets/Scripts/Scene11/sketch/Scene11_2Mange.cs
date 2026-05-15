using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Scene11_2Mange : MonoBehaviour
{
    public Transform startPosition;   // 起始位置（右侧屏幕外）
    public Transform drawPosition;    // 绘画位置（屏幕中央）
    public Transform endPosition;     // 结束位置（左侧屏幕外）

    [Header("每轮结束后替换的素材")]
    public Sprite sprite1; // picture1褶皱素材
    public Sprite sprite2; // picture2打湿素材
    public Sprite sprite3; // picture3黑白素材
    public Sprite sprite4; // picture4黑白素材
    public Sprite sprite5; // picture5黑白素材

    public Material defaultMaterial; // 默认材质（恢复用）

    [Header("文案")]
    public TMP_Text tmp_text;
    [TextArea] public string text3;
    [TextArea] public string text4;
    [TextArea] public string text5;

    // 状态机
    private enum RoundState { Drawing, WaitForClick, WaitForSwipe, Transitioning }
    private RoundState currentState = RoundState.Drawing;
    private int currentRound = 0;          // 0~4 对应 picture1~5
    private bool[] roundCompleted = new bool[5]; // 每轮绘画是否已完成

    // 滑动检测
    private Vector2 swipeStartPos;
    private bool isDraggingForSwipe = false;
    private float swipeThresholdPixels = 50f;
    private bool isWaitingFinal = false;  // 防止重复协程
    // 组件引用
    private SpriteRenderer currentSpriteRenderer;
    private DrawingController currentDrawingCtrl;

    void Start()
    {
        // 初始化所有画板的位置到起始点，并禁用绘画脚本
        InitAllPanels();

        // 获取第一轮并移动到绘画位置，启用绘画
        currentDrawingCtrl = GetDrawingControllerByRound(0);
        if (currentDrawingCtrl != null)
        {
            currentDrawingCtrl.gameObject.SetActive(true);
            currentSpriteRenderer = currentDrawingCtrl.targetSprite;
            // 移动到绘画位置
            currentDrawingCtrl.transform.position = drawPosition.position;
            currentDrawingCtrl.enabled = true;
        }
        else
        {
            Debug.LogError("未找到第一轮的 DrawingController！");
        }

        currentState = RoundState.Drawing;
        tmp_text.text = "";
    }

    // 初始化所有画板：置位、禁用脚本、隐藏非第一轮的（可选）
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
                //ctrl.enabled = false;
                ctrl.targetSprite.gameObject.SetActive(true);
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
        else if (currentState == RoundState.WaitForSwipe)
        {
            if (Input.GetMouseButtonDown(0) && IsMouseOverCurrentSprite())
            {
                swipeStartPos = Input.mousePosition;
                isDraggingForSwipe = true;
            }
            if (isDraggingForSwipe && Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - swipeStartPos;
                if (delta.x < -swipeThresholdPixels) // 替换原来的 -swipeThreshold
                {
                    isDraggingForSwipe = false;
                    OnSwipeComplete();
                }

            }
            if (Input.GetMouseButtonUp(0))
            {
                isDraggingForSwipe = false;
            }
        }
    }

    private void OnDrawingComplete()
    {
        currentDrawingCtrl.enabled = false;
        currentState = RoundState.WaitForClick;
        Debug.Log($"第{currentRound + 1}轮绘画完成，等待点击照片");
    }

    private void OnPhotoClick()
    {
        currentSpriteRenderer.material = defaultMaterial;
        switch (currentRound)
        {
            case 0:
                if (sprite1 != null) currentSpriteRenderer.sprite = sprite1;
                break;
            case 1:
                if (sprite2 != null) currentSpriteRenderer.sprite = sprite2;
                DrawManage.instance.SwapTool();
                break;
            case 2:
                if (sprite3 != null) currentSpriteRenderer.sprite = sprite3;
                if (!string.IsNullOrEmpty(text3)) tmp_text.text = text3;
                break;
            case 3:
                if (sprite4 != null) currentSpriteRenderer.sprite = sprite4;
                if (!string.IsNullOrEmpty(text4)) tmp_text.text = text4;
                break;
            case 4:
                if (sprite5 != null) currentSpriteRenderer.sprite = sprite5;
                if (!string.IsNullOrEmpty(text5)) tmp_text.text = text5;
                if (!isWaitingFinal)  // 防止重复启动
                {
                    isWaitingFinal = true;
                    StartCoroutine(WaitForFinalClick());
                }
                return;
        }
        currentState = RoundState.WaitForSwipe;
        Debug.Log("特效已显示，请向左滑动切换下一张照片");
    }

    IEnumerator WaitForFinalClick()
    {
        while (true)
        {
            if (Input.GetMouseButtonUp(0) && IsMouseOverCurrentSprite())
            {
                SceneManager.LoadScene("NextSceneName"); // 替换实际场景名
                yield break;
            }
            yield return null;
        }
    }

    private void OnSwipeComplete()
    {
        if (currentState != RoundState.WaitForSwipe) return;
        currentState = RoundState.Transitioning;
        StartCoroutine(TransitionToNextRound());
    }

    IEnumerator TransitionToNextRound()
    {
        int nextRound = currentRound + 1;
        if (nextRound >= 5)
        {
            Debug.LogWarning("已经是最后一轮，无法左滑");
            yield break;
        }

        DrawingController nextCtrl = GetDrawingControllerByRound(nextRound);
        SpriteRenderer nextSpriteRenderer = nextCtrl.targetSprite;

        // 确保下一张画板位于起始位置，并且可见
        nextSpriteRenderer.transform.position = startPosition.position;
        nextSpriteRenderer.gameObject.SetActive(true);
        nextCtrl.enabled = false; // 开始时不可绘画

        // 动画：当前画板从 drawPosition 移动到 endPosition，下一张从 startPosition 移动到 drawPosition
        float duration = 0.4f;
        Sequence seq = DOTween.Sequence();
        seq.Join(currentSpriteRenderer.transform.DOMove(endPosition.position, duration).SetEase(Ease.InQuad));
        seq.Join(nextSpriteRenderer.transform.DOMove(drawPosition.position, duration).SetEase(Ease.OutQuad));
        yield return seq.WaitForCompletion();

        // 隐藏当前画板（可选，但建议禁用以提高性能）
        currentDrawingCtrl.gameObject.SetActive(false);
        currentSpriteRenderer.gameObject.SetActive(false);
        // 启用下一轮的绘画脚本
        nextCtrl.enabled = true;

        // 更新全局引用
        currentSpriteRenderer = nextSpriteRenderer;
        currentDrawingCtrl = nextCtrl;
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

    private void SetAllDrawingControllersActive(bool active)
    {
        if (DrawManage.instance == null || DrawManage.instance.drawingControllers == null) return;
        foreach (var ctrl in DrawManage.instance.drawingControllers)
        {
            if (ctrl != null) ctrl.enabled = active;
        }
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