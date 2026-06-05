using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TransitionManage : MonoBehaviour
{
    public static TransitionManage Instance;
    public Canvas canvas;
   
    [Header("Panel")]
    public CanvasGroup transitionCanvasGroup;
    public Image transitionImage;

    private bool isTransitioning = false;
    private Tween activeTween;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 初始确保完全透明且不可交互
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }
    private void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
    //淡出（进入黑屏/白屏等），完成后可选回调
    public void FadeOut(float duration, Color color, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("转场动画正在进行中，请等待完成后再调用");
            return;
        }
        isTransitioning = true;

        if (transitionImage != null)
            transitionImage.color = color;

        // 启用交互阻挡
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.interactable = true;
            transitionCanvasGroup.blocksRaycasts = true;
        }

        activeTween?.Kill();
        transitionCanvasGroup.alpha = 0f;
        activeTween = transitionCanvasGroup.DOFade(1f, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                activeTween = null;
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    //淡入（从黑屏/白屏恢复正常），完成后可选回调
    public void FadeIn(float duration, Color color, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("转场动画正在进行中，请等待完成后再调用");
            return;
        }
        isTransitioning = true;

        if (transitionImage != null)
            transitionImage.color = color;
        activeTween?.Kill();
        transitionCanvasGroup.alpha = 1f;
        activeTween = transitionCanvasGroup.DOFade(0f, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 动画完成后移除交互阻挡
                if (transitionCanvasGroup != null)
                {
                    transitionCanvasGroup.interactable = false;
                    transitionCanvasGroup.blocksRaycasts = false;
                }
                activeTween = null;
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    //立即重置转场状态（完全透明且不可交互），用于场景加载前的强制重置
    public void ResetImmediate()
    {
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();
        isTransitioning = false;
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.interactable = false;
            transitionCanvasGroup.blocksRaycasts = false;
        }
    }
}