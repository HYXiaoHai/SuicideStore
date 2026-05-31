using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SingleRoundQTE : MonoBehaviour
{
    [Header("QTE 参数")]
    public int requiredClicks = 8;
    public float timeLimit = 3f;
    public KeyCode targetKey = KeyCode.E;

    [Header("动画特效")]
    public CanvasGroup qteCanvasGroup;      // 整个QTE面板的CanvasGroup（用于淡入淡出）
    public float appearDuration = 0.3f;     // 出现/消失动画时长
    public Image ePrompt;                   // 按键提示图
    public float pressScaleDuration = 0.1f;
    public float pressScaleFactor = 1.1f;
    private Tweener scaleTweener;
    private Vector3 originalScale;

    [Header("UI 绑定")]
    public Image progressFill;//进度条
    public Image timerFill;         // 倒计时进度（剩余时间）

    [Header("事件回调")]
    public UnityEvent onQTESuccess;
    public UnityEvent onQTEFail;

    // 内部状态
    private int currentClicks = 0;
    private float currentTime = 0f;
    private bool isActive = false;
    private bool completed = false;

    private void Awake()
    {
        if (ePrompt != null)
            originalScale = ePrompt.transform.localScale;

        // 初始隐藏面板
        if (qteCanvasGroup != null)
        {
            qteCanvasGroup.alpha = 0f;
            qteCanvasGroup.interactable = false;
            qteCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!isActive || completed) return;

        currentTime -= Time.deltaTime;
        // 更新倒计时进度条
        if (timerFill != null)
        {
            float fill = currentTime / timeLimit;
            timerFill.fillAmount = 1f-fill;
        }
        if (currentTime <= 0f)
        {
            FailQTE("Time out");
            return;
        }
        if (Input.GetKeyDown(targetKey))
        {
            OnValidPress();
        }
    }

    private void OnValidPress()
    {
        if (!isActive || completed) return;

        currentClicks++;
        UpdateProgressUI();

        // 按键反馈动画
        if (ePrompt != null)
        {
            scaleTweener?.Kill();
            ePrompt.transform.localScale = originalScale;
            ePrompt.transform.DOPunchScale(originalScale * 0.3f, 0.1f, 1, 0);
        }

        if (currentClicks >= requiredClicks)
            SuccessQTE();
    }

    private void SuccessQTE()
    {
        if (completed) return;
        completed = true;
        isActive = false;
        Debug.Log("QTE Success!");
        onQTESuccess?.Invoke();
    }

    private void FailQTE(string reason)
    {
        if (completed) return;
        completed = true;
        isActive = false;
        Debug.Log($"QTE Failed: {reason}");
        onQTEFail?.Invoke();
    }

    ///设置本轮QTE的位置（会移动整个面板到目标物体位置）
    public void SetPosition(RectTransform targetRect)
    {
        if (targetRect == null) return;
        RectTransform myRect = GetComponent<RectTransform>();
        if (myRect != null)
        {
            myRect.anchoredPosition = targetRect.anchoredPosition;
        }
    }

    public void ShowAndStart()
    {
        if (qteCanvasGroup == null)
        {
            InitializeAndStart();
            return;
        }

        // 停止所有动画
        qteCanvasGroup.DOKill();

        // 初始状态：透明、极小缩放、可交互
        qteCanvasGroup.alpha = 0f;
        qteCanvasGroup.interactable = true;
        qteCanvasGroup.blocksRaycasts = true;

        RectTransform rect = qteCanvasGroup.GetComponent<RectTransform>();
        Vector3 originalPos = rect != null ? rect.anchoredPosition : Vector3.zero;

        if (rect != null)
        {
            rect.localScale = Vector3.zero;
            rect.anchoredPosition = originalPos; // 重置位置
        }

        // 创建一个并行动画组
        Sequence urgentSeq = DOTween.Sequence();

        // 1. 淡入（快速）
        urgentSeq.Join(qteCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));

        if (rect != null)
        {
            // 2. 缩放（弹性曲线，自带轻微震荡）
            urgentSeq.Join(rect.DOScale(2.374178f, 0.3f).SetEase(Ease.OutElastic, 0.6f, 0.4f));

            // 3. 位置抖动（与缩放完全同步，融为一体）
            Tween shakeTween = rect.DOShakePosition(0.35f, strength: 8f, vibrato: 25, randomness: 90, fadeOut: true);
            urgentSeq.Join(shakeTween);
        }

        // 动画结束时确保位置回归原值（防止微小偏移）
        urgentSeq.OnComplete(() =>
        {
            if (rect != null) rect.anchoredPosition = originalPos;
            InitializeAndStart();
        });

        urgentSeq.Play();
    }

    /// 隐藏面板（可在回调后执行下一轮）
    public void Hide(System.Action onComplete = null)
    {
        if (qteCanvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        qteCanvasGroup.DOKill();
        qteCanvasGroup.DOFade(0f, appearDuration).OnComplete(() =>
        {
            qteCanvasGroup.interactable = false;
            qteCanvasGroup.blocksRaycasts = false;
            onComplete?.Invoke();
        });
    }

    /// 重置参数并激活QTE（内部调用，不负责动画）
    public void InitializeAndStart()
    {
        currentClicks = 0;
        currentTime = timeLimit;
        completed = false;
        isActive = true;
        UpdateProgressUI();

        // 重置倒计时进度条为满格，颜色恢复白色
        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
        }
    }

    private void UpdateProgressUI()
    {
        if (progressFill != null)
            progressFill.fillAmount = 1f - (float)currentClicks / requiredClicks;
    }
}