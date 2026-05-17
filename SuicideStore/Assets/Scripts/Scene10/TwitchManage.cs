using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class TwitchManage : MonoBehaviour
{
    public SpriteRenderer twitchTarget;//目标物体
    public Slider twitchSlider;//slider
    public float twitchMin = 0f;//最小值
    public float twitchMax = 1f;//最大值
    private Material twitchMaterial;
    [Header("第一阶段")]
    public CanvasGroup stage1Canvas;
    public TMP_Text twitchText1;
    public TMP_Text twitchText2;
    [Header("第二阶段")]
    public CanvasGroup stage2Canvas;
    public TMP_Text twitchText3;
    public TMP_Text twitchText4;
    public Image twitchImage3;
    public Image twitchImage4;
    [Header("第三阶段")]
    public CanvasGroup stage3Canvas;
    public TMP_Text twitchText5;

    [Header("关卡切换")]
    public string nextScene;//第2关
    public float changeDelay = 1f;//跳转延迟
    private bool hasTriggeredSwitch = false; //防止重复触发

    // 用于管理所有活跃的动画，防止重叠冲突
    private Tween activeTween_CG1;
    private Tween activeTween_CG2;
    private Tween activeTween_CG3;
    private Tween activeTween_Text2;
    private Tween activeTween_Text4;
    private Tween activeTween_Image4;
    // Start is called before the first frame update
    void Start()
    {
        if (twitchTarget != null)
            twitchMaterial = twitchTarget.material;

        if (twitchSlider != null)
        {
            twitchSlider.onValueChanged.AddListener(OnSliderChanged);
            twitchSlider.value = 0f;
        }
        UpdateDisplay(0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnSliderChanged(float value)
    {
        UpdateDisplay(value);
    }
    private void UpdateDisplay(float value)
    {
        float targetAlpha1, targetAlpha2, targetAlpha3;
        float threshold1 = 1f / 3f;
        float threshold2 = 2f / 3f;

        // 阶段判定及整体 CanvasGroup 透明度目标
        if (value <= threshold1)
        {
            targetAlpha1 = 1f;
            targetAlpha2 = 0f;
            targetAlpha3 = 0f;

            // 第一阶段内的子动画：滑块值超过 1/6 时淡入 twitchText2
            float subThreshold = threshold1 / 2f; // 0.16667
            bool shouldShowText2 = value >= subThreshold;
            AnimateAlpha(twitchText2, shouldShowText2 ? 1f : 0f, 0.2f, ref activeTween_Text2);
        }
        else if (value <= threshold2)
        {
            targetAlpha1 = 0f;
            targetAlpha2 = 1f;
            targetAlpha3 = 0f;

            // 第二阶段内的子动画：滑块值超过 0.5 时淡入 twitchText4 和 twitchImage4
            float subThreshold = threshold1 + (threshold2 - threshold1) / 2f; // 0.5
            bool shouldShowText4 = value >= subThreshold;
            AnimateAlpha(twitchText4, shouldShowText4 ? 1f : 0f, 0.2f, ref activeTween_Text4);
            AnimateAlpha(twitchImage4, shouldShowText4 ? 1f : 0f, 0.2f, ref activeTween_Image4);
        }
        else
        {
            targetAlpha1 = 0f;
            targetAlpha2 = 0f;
            targetAlpha3 = 1f;
            // 第三阶段无额外子动画
        }

        // 执行整体 CanvasGroup 渐变动画（安全托管）
        AnimateCanvasGroup(stage1Canvas, targetAlpha1, 0.2f, ref activeTween_CG1);
        AnimateCanvasGroup(stage2Canvas, targetAlpha2, 0.2f, ref activeTween_CG2);
        AnimateCanvasGroup(stage3Canvas, targetAlpha3, 0.2f, ref activeTween_CG3);

        UpdateMaterial(value);

        // 检测完成条件（滑块拉满）
        if (!hasTriggeredSwitch && Mathf.Approximately(value, 1f))
        {
            hasTriggeredSwitch = true;
            StartCoroutine(OnGrowthComplete());
        }
    }

    private void UpdateMaterial(float value)
    {
        //float mappedValue = value == 0f ? 0f : value * (twitchMax - twitchMin) + twitchMin;
        float mappedValue = value *(twitchMax - twitchMin) + twitchMin;
        // 移除频繁的 Debug.Log，避免性能损耗
        // Debug.Log(mappedValue);
        if (twitchMaterial != null)
            twitchMaterial.SetFloat("_Frequency", mappedValue);
    }

    /// <summary>
    /// 安全的 CanvasGroup 透明度动画，自动中断旧动画
    /// </summary>
    private void AnimateCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, ref Tween activeTween)
    {
        if (cg == null) return;

        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();

        activeTween = cg.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }

    /// <summary>
    /// 通用的 Graphic（Text/Image）透明度动画，自动中断旧动画
    /// </summary>
    private void AnimateAlpha(Graphic graphic, float targetAlpha, float duration, ref Tween activeTween)
    {
        if (graphic == null) return;

        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();

        activeTween = graphic.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }

    /// <summary>
    /// 清理所有正在播放的动画
    /// </summary>
    private void KillAllTweens()
    {
        KillTween(ref activeTween_CG1);
        KillTween(ref activeTween_CG2);
        KillTween(ref activeTween_CG3);
        KillTween(ref activeTween_Text2);
        KillTween(ref activeTween_Text4);
        KillTween(ref activeTween_Image4);
    }

    private void KillTween(ref Tween tween)
    {
        if (tween != null && tween.IsActive())
            tween.Kill();
        tween = null;
    }
    //进入下一关
    private IEnumerator OnGrowthComplete()
    {
        //禁用滑块交互，防止再次拖动
        if (twitchSlider != null)
            twitchSlider.interactable = false;

        Debug.Log("成长完成，切换至下一关卡");
        yield return new WaitForSeconds(changeDelay);

        SceneManager.LoadScene(nextScene);
    }
    private void OnDestroy()
    {
        // 清理所有动画，防止内存泄漏或跨场景残留
        KillAllTweens();
        if (twitchSlider != null)
            twitchSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

}
