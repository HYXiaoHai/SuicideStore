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

    [Header("身高文本")]
    public TextMeshProUGUI heightText;

    [Header("参数设置")]
    public float minHeight = 80f;
    public float maxHeight = 120f;
    //public float minScale = 1f;
    //public float maxScale = 1.3f;

    //private Vector3 childInitialScale;
    //private Vector3 adultInitialScale;
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

    void OnSliderChanged(float value)
    {
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

        //更新身高文本
        if (heightText != null)
        {
            float height = Mathf.Lerp(minHeight, maxHeight, value);
            heightText.text = $"{height:F0}cm";
        }
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

    //旧脚本
    //void UpdateDisplay(float value)
    //{
    //    if (childImage != null)
    //    {
    //        childImage.alpha = 1f - value;
    //        //float scale = Mathf.Lerp(minScale, maxScale, value);
    //        //childImage.transform.localScale = childInitialScale * scale;
    //    }

    //    if (adultImage != null)
    //    {
    //        adultImage.alpha = value;
    //        //float scale = Mathf.Lerp(minScale, maxScale, value);
    //        //adultImage.transform.localScale = adultInitialScale * scale;
    //    }

    //    if (heightText != null)
    //    {
    //        float height = Mathf.Lerp(minHeight, maxHeight, value);
    //        heightText.text = $"{height:F0}cm";
    //    }
    //}
}
