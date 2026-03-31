using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrowthComparison : MonoBehaviour
{
    [Header("图片引用")]
    public CanvasGroup childImage;
    public CanvasGroup adultImage;

    [Header("滑块引用")]
    public Slider growthSlider;

    [Header("身高文本")]
    public TextMeshProUGUI heightText;

    [Header("参数设置")]
    public float minHeight = 80f;
    public float maxHeight = 120f;
    public float minScale = 1f;
    public float maxScale = 1.3f;

    private Vector3 childInitialScale;
    private Vector3 adultInitialScale;

    void Start()
    {
        if (childImage != null)
        {
            childInitialScale = childImage.transform.localScale;
            childImage.alpha = 1f;
        }
        
        if (adultImage != null)
        {
            adultInitialScale = adultImage.transform.localScale;
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
        if (childImage != null)
        {
            childImage.alpha = 1f - value;
            float scale = Mathf.Lerp(minScale, maxScale, value);
            childImage.transform.localScale = childInitialScale * scale;
        }
        
        if (adultImage != null)
        {
            adultImage.alpha = value;
            float scale = Mathf.Lerp(minScale, maxScale, value);
            adultImage.transform.localScale = adultInitialScale * scale;
        }
        
        if (heightText != null)
        {
            float height = Mathf.Lerp(minHeight, maxHeight, value);
            heightText.text = $"{height:F0}cm";
        }
    }
}
