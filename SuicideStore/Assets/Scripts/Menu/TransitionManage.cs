using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManage : MonoBehaviour
{
    public static TransitionManage Instance;
    public Canvas canvas;
   
    [Header("Panel")]
    public CanvasGroup transitionCanvasGroup;
    public Image transitionImage;
    public float transitionDuration = 1f;
    public bool isTransitioning = false;
    private Tween activeTween;
    private Coroutine autoFadeCoroutine;
    private bool isLoadingWithAutoFade = false;
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
        BindCamera();
    }
    // 绑定相机：确保 Canvas 的 worldCamera 为当前主相机
    public void BindCamera()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;
    }
    //淡出（进入黑屏/白屏等），完成后可选回调
    public void FadeOut(float duration, Color color, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            return;
        }
        isTransitioning = true;
        BindCamera();

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
        BindCamera();

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
    //自动跳转
    public void LoadSceneWithAutoFade(string sceneName, float fadeOutDuration, float fadeInDuration, Color color)
    {
        if (isLoadingWithAutoFade)
        {
            Debug.LogWarning("已有自动淡入淡出加载流程进行中，请勿重复调用");
            return;
        }

        isLoadingWithAutoFade = true;
        BindCamera();

        // 先淡出
        FadeOut(fadeOutDuration, color, () =>
        {
            // 注册场景加载完成事件（仅一次）
            SceneManager.sceneLoaded += OnSceneLoadedForAutoFade;

            // 加载场景
            SceneManager.LoadScene(sceneName);
        });
    }

    private void OnSceneLoadedForAutoFade(Scene scene, LoadSceneMode mode)
    {
        // 移除事件，避免影响后续场景切换
        SceneManager.sceneLoaded -= OnSceneLoadedForAutoFade;
        BindCamera();

        // 开始淡入
        FadeIn(transitionDuration, transitionImage != null ? transitionImage.color : Color.black, () =>
        {
            isLoadingWithAutoFade = false;
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