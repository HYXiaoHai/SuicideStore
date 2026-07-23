using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Scene12Manager : MonoBehaviour
{
    [Header("=== 图片设置 ===")]
    public Image firstImage;
    public Image secondImage;

    [Header("=== 泛光设置 ===")]
    public Volume globalVolume;
    [SerializeField] private float bloomStep1 = 0.3f;    // 第一次点击后的泛光强度
    [SerializeField] private float bloomStep2 = 0.7f;    // 第二次点击后的泛光强度
    [SerializeField] private float bloomStep3 = 10f;     // 第三次点击后的泛光强度（过曝）
    [SerializeField] private float bloomFadeDuration = 1.2f; // 每次泛光渐变的时长

    [Header("=== 转场设置 ===")]
    public float fadeDuration = 1.0f;
    public string nextSceneName;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private int currentStep = 0;          // 0=初始, 1=第一次点击后, 2=第二次点击后, 3=完成
    private bool isProcessing = false;
    private Bloom bloom;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.black);

        // 获取 Bloom
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out bloom);
            if (bloom != null)
                bloom.intensity.value = 0f;
            else
                Debug.LogWarning("Global Volume 中未找到 Bloom 组件");
        }
        else
        {
            Debug.LogWarning("请将 Global Volume 赋值给 globalVolume 字段");
        }

        // 初始状态：只显示图一
        if (firstImage != null)
        {
            firstImage.gameObject.SetActive(true);
        }

        if (secondImage != null)
        {
            secondImage.gameObject.SetActive(false);
        }

        currentStep = 0;
        isProcessing = false;
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (isProcessing) return;
        if (currentStep >= 3) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.A))
        {
            OnScreenClick();
        }
    }

    void OnScreenClick()
    {
        if (isProcessing) return;

        currentStep++;
        isProcessing = true;

        if (showDebugLog)
            Debug.Log($"第 {currentStep} 次点击");

        switch (currentStep)
        {
            case 1:
                StartCoroutine(Step1_ShowFirstImage());
                break;
            case 2:
                StartCoroutine(Step2_SwitchToSecondImage());
                break;
            case 3:
                StartCoroutine(Step3_OverexposeAndFinish());
                break;
            default:
                isProcessing = false;
                break;
        }
    }

    // ==================== 第一步：渐显图一，泛光到 0.3 ====================
    IEnumerator Step1_ShowFirstImage()
    {
        if (showDebugLog) Debug.Log("Step1: 渐显图一，泛光 0.3");

        // 图一已经在 Start 中显示了，确保它可见
        if (firstImage != null)
        {
            firstImage.gameObject.SetActive(true);
            firstImage.DOFade(1f, fadeDuration);
        }

        // 泛光渐变到 Step1
        if (bloom != null)
        {
            DOTween.To(
                () => bloom.intensity.value,
                x => bloom.intensity.value = x,
                bloomStep1,
                bloomFadeDuration
            ).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(bloomFadeDuration);
        isProcessing = false;
    }

    // ==================== 第二步：渐隐图一，渐显图二，泛光到 0.7 ====================
    IEnumerator Step2_SwitchToSecondImage()
    {
        if (showDebugLog) Debug.Log("Step2: 渐隐图一，渐显图二，泛光 0.7");

        // 渐显图二
        if (secondImage != null)
        {
            secondImage.gameObject.SetActive(true);
            secondImage.DOFade(1f, fadeDuration);
        }

        // 渐隐图一
        if (firstImage != null)
        {
            firstImage.CrossFadeAlpha(0f, fadeDuration, false);
        }

        // 泛光渐变到 Step2
        if (bloom != null)
        {
            DOTween.To(
                () => bloom.intensity.value,
                x => bloom.intensity.value = x,
                bloomStep2,
                bloomFadeDuration
            ).SetEase(Ease.OutQuad);
        }

        yield return new WaitForSeconds(fadeDuration);

        // 彻底隐藏图一
        if (firstImage != null)
        {
            firstImage.gameObject.SetActive(false);
        }

        isProcessing = false;
    }

    // ==================== 第三步：泛光拉到目标值（InExpo）→ 黑屏 → 跳转 ====================
    IEnumerator Step3_OverexposeAndFinish()
    {
        if (showDebugLog) Debug.Log("Step3: 泛光过曝 InExpo → 黑屏跳转");

        // 泛光从当前值拉到目标值，使用 InExpo 曲线
        if (bloom != null)
        {
            DOTween.To(
                () => bloom.intensity.value,
                x => bloom.intensity.value = x,
                bloomStep3,
                bloomFadeDuration
            ).SetEase(Ease.InExpo);
        }

        // 等待泛光动画完成
        yield return new WaitForSeconds(bloomFadeDuration);

        // 再停留 0.3 秒，让过曝效果充分展现
        yield return new WaitForSeconds(0.3f);

        // 执行转场跳转
        CompleteLevel();
    }

    // ==================== 跳转逻辑（原样保留） ====================
    public void CompleteLevel()
    {
        GameManage.Instance.CompleteCurrentLevel();
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                TransitionManage.Instance.FadeOut(1.5f, Color.black, () =>
                {
                    SceneManager.LoadScene(nextScene);
                });
            }
        }
        else
        {
            TransitionManage.Instance.FadeOut(1f, Color.black, () =>
            {
                SceneManager.LoadScene("End");
            });
        }
    }
}