using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
public class GrowthComparison : MonoBehaviour
{
    [Header("图片引用")]
    public CanvasGroup babyImage;
    public CanvasGroup childImage;
    public CanvasGroup adultImage;

    [Header("滑块引用")]
    public Slider growthSlider;

    [Header("参数设置")]
    public float minHeight = 80f;
    public float maxHeight = 120f;

    [Header("音效")]
    public AudioClip growthClip;//身高切换音效

    //public float minScale = 1f;
    //public float maxScale = 1.3f;

    //private Vector3 childInitialScale;
    //private Vector3 adultInitialScale;

    [Header("关卡切换")]
    public int nextLevelIndex = 2;//第2关
    public float changeDelay = 1f;//完成后多久切换镜头（延迟）
    private bool hasTriggeredSwitch = false; //防止重复触发

    private Tween activeTween1, activeTween2, activeTween3;
    void Start()
    {
        if (babyImage != null)
        {
            //childInitialScale = babyImage.transform.localScale;
            babyImage.alpha = 1f;
        }
        if (childImage != null)
        {
            //childInitialScale = childImage.transform.localScale;
            childImage.alpha = 0f;
        }
        
        if (adultImage != null)
        {
            //adultInitialScale = adultImage.transform.localScale;
            adultImage.alpha = 0f;
        }
        
        if (growthSlider != null)
        {
            growthSlider.onValueChanged.AddListener(OnSliderChanged);
            growthSlider.value = 0f;
        }
        
        UpdateDisplay(0f);
    }
    void Update()
    {
        if (growthSlider != null && growthSlider.interactable == GameManage.Instance.isSetting)
        {
            growthSlider.interactable = !GameManage.Instance.isSetting;
        }
    }
    void OnSliderChanged(float value)
    {
        if (GameManage.Instance.isSetting) return;
        UpdateDisplay(value);
    }
    void UpdateDisplay(float value)
    {
        float targetAlpha1, targetAlpha2, targetAlpha3;

        if (value <= 0.5f)
        {
            //阶段1
            float t = value / 0.5f;
            targetAlpha1 = 1f - t;
            targetAlpha2 = t;
            targetAlpha3 = 0f;
        }
        else
        {
            //阶段2
            float t = (value - 0.5f) / 0.5f; 
            targetAlpha1 = 0f;
            targetAlpha2 = 1f - t;
            targetAlpha3 = t;
        }

        //渐变过度
        AnimateAlpha(babyImage, targetAlpha1, ref activeTween1);
        AnimateAlpha(childImage, targetAlpha2, ref activeTween2);
        AnimateAlpha(adultImage, targetAlpha3, ref activeTween3);

        // 检查是否完成（value 达到 1）
        if (!hasTriggeredSwitch && Mathf.Approximately(value, 1f))
        {
            AudioManager.Instance.Play2DSound(growthClip, 0.5f);
            hasTriggeredSwitch = true;
            OnGrowthComplete();
        }

    }

    //进入下一关
    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");

        if (Scene4Manage.Instance != null)
        {
            // 假设 changeDelay = 2f，相机切换完成后启动第二关
            Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                if (Scene4Manage.Instance.level2Manage != null)
                {
                    Scene4Manage.Instance.level2Manage.BeginGame();
                }
                else
                {
                    Debug.LogError("level2Manage 未在 Scene4Manage 中赋值！");
                }
            });
        }
        else
        {
            Debug.LogError("Scene4Manage.Instance 不存在，请确保场景中有 Scene4Manage 组件");
        }

        //禁用滑块交互，防止再次拖动
        if (growthSlider != null)
            growthSlider.interactable = false;
    }

    private void AnimateAlpha(CanvasGroup cg, float targetAlpha, ref Tween activeTween)
    {
        if (cg == null) return;

        //临界状态
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();

        //开始新的透明度动画
        activeTween = cg.DOFade(targetAlpha, 0.2f).SetEase(Ease.Linear);
    }
}
