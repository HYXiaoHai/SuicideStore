using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TwitchManage : MonoBehaviour
{
    public SpriteRenderer twitchTarget;
    public Slider twitchSlider;
    public float twitchMin = 0f;
    public float twitchMax = 1f;
    private Material twitchMaterial;

    [Header("第一阶段")]
    public CanvasGroup stage1Canvas;
    public TMP_Text twitchText1;
    public TMP_Text twitchText2;
    public TMP_Text twitchText3;

    [Header("第二阶段")]
    public CanvasGroup stage2Canvas;
    public TMP_Text twitchText4;
    public TMP_Text twitchText5;
    public TMP_Text twitchText6;

    [Header("第三阶段")]
    public CanvasGroup stage3Canvas;
    public TMP_Text twitchText7;

    [Header("关卡切换")]
    public string nextScene;
    public float changeDelay = 1f;
    private bool hasTriggeredSwitch = false;

    // 卡槽节点设置（对应三个阶段的分界点）
    private float[] snapPoints = { 1f / 3f, 2f / 3f };
    private float snapThreshold = 0.025f;  // 吸附阈值，可调
    private bool isSnapping = false;

    // 动画管理
    private Tween activeTween_CG1;
    private Tween activeTween_CG2;
    private Tween activeTween_CG3;
    private Tween activeTween_Text2;
    private Tween activeTween_Text4;
    private Tween activeTween_Text3;
    private Tween activeTween_Text5;
    private Tween activeTween_Text6;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.white);

        if (twitchTarget != null)
            twitchMaterial = twitchTarget.material;

        if (twitchSlider != null)
        {
            twitchSlider.onValueChanged.AddListener(OnSliderChanged);
            twitchSlider.value = 0f;
            twitchSlider.interactable = !GameManage.Instance.isSetting;
        }
        UpdateDisplay(0f);
    }

    void Update()
    {
        if (twitchSlider != null && twitchSlider.interactable == GameManage.Instance.isSetting)
        {
            twitchSlider.interactable = !GameManage.Instance.isSetting;
        }
    }

    void OnSliderChanged(float value)
    {
        if (GameManage.Instance.isSetting) return;
        if (isSnapping)
        {
            isSnapping = false;
            return;
        }

        // 检查是否靠近某个卡槽节点（只处理中间两个节点，0和1不吸附）
        float nearestSnap = -1f;
        float minDiff = Mathf.Infinity;
        foreach (var snap in snapPoints)
        {
            float diff = Mathf.Abs(value - snap);
            if (diff < minDiff)
            {
                minDiff = diff;
                nearestSnap = snap;
            }
        }

        if (minDiff < snapThreshold && !Mathf.Approximately(value, nearestSnap))
        {
            // 吸附到最近的节点
            isSnapping = true;
            twitchSlider.value = nearestSnap;
            // 直接更新UI（因为value变化会再次触发OnSliderChanged，但isSnapping会阻止循环）
            UpdateDisplay(nearestSnap);
            // 可在此添加轻微震动或音效
            // AudioManager.Instance.PlayShortSound(snapClip, 0.5f);
            return;
        }

        // 正常更新
        UpdateDisplay(value);
    }

    private void UpdateDisplay(float value)
    {
        float targetAlpha1, targetAlpha2, targetAlpha3;
        float threshold1 = 1f / 3f;
        float threshold2 = 2f / 3f;

        if (value <= threshold1)
        {
            targetAlpha1 = 1f;
            targetAlpha2 = 0f;
            targetAlpha3 = 0f;

            float sub1 = threshold1 / 3f;
            float sub2 = threshold1 * 2f / 3f;
            float alphaText2 = Mathf.Clamp01((value - sub1) / (sub2 - sub1));
            float alphaText3 = Mathf.Clamp01((value - sub2) / (threshold1 - sub2));

            AnimateAlpha(twitchText2, alphaText2, 0.2f, ref activeTween_Text2);
            AnimateAlpha(twitchText3, alphaText3, 0.2f, ref activeTween_Text3);

            if (twitchText1 != null && twitchText1.color.a != 1f)
                twitchText1.CrossFadeAlpha(1f, 0.1f, false);
        }
        else if (value <= threshold2)
        {
            targetAlpha1 = 0f;
            targetAlpha2 = 1f;
            targetAlpha3 = 0f;

            float stage2Start = threshold1;
            float stage2End = threshold2;
            float sub1 = stage2Start + (stage2End - stage2Start) / 3f;
            float sub2 = stage2Start + (stage2End - stage2Start) * 2f / 3f;

            float alphaText5 = Mathf.Clamp01((value - sub1) / (sub2 - sub1));
            float alphaText6 = Mathf.Clamp01((value - sub2) / (stage2End - sub2));

            if (twitchText4 != null && twitchText4.color.a != 1f)
                twitchText4.CrossFadeAlpha(1f, 0.1f, false);

            AnimateAlpha(twitchText5, alphaText5, 0.2f, ref activeTween_Text5);
            AnimateAlpha(twitchText6, alphaText6, 0.2f, ref activeTween_Text6);
        }
        else
        {
            targetAlpha1 = 0f;
            targetAlpha2 = 0f;
            targetAlpha3 = 1f;
        }

        AnimateCanvasGroup(stage1Canvas, targetAlpha1, 0.2f, ref activeTween_CG1);
        AnimateCanvasGroup(stage2Canvas, targetAlpha2, 0.2f, ref activeTween_CG2);
        AnimateCanvasGroup(stage3Canvas, targetAlpha3, 0.2f, ref activeTween_CG3);

        UpdateMaterial(value);

        if (!hasTriggeredSwitch && Mathf.Approximately(value, 1f))
        {
            hasTriggeredSwitch = true;
            StartCoroutine(OnGrowthComplete());
        }
    }

    private void UpdateMaterial(float value)
    {
        float mappedValue = value * (twitchMax - twitchMin) + twitchMin;
        if (twitchMaterial != null)
            twitchMaterial.SetFloat("_Frequency", mappedValue);
    }

    private void AnimateCanvasGroup(CanvasGroup cg, float targetAlpha, float duration, ref Tween activeTween)
    {
        if (cg == null) return;
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();
        activeTween = cg.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }

    private void AnimateAlpha(Graphic graphic, float targetAlpha, float duration, ref Tween activeTween)
    {
        if (graphic == null) return;
        if (activeTween != null && activeTween.IsActive())
            activeTween.Kill();
        activeTween = graphic.DOFade(targetAlpha, duration).SetEase(Ease.Linear);
    }

    private void KillAllTweens()
    {
        KillTween(ref activeTween_CG1);
        KillTween(ref activeTween_CG2);
        KillTween(ref activeTween_CG3);
        KillTween(ref activeTween_Text2);
        KillTween(ref activeTween_Text3);
        KillTween(ref activeTween_Text4);
        KillTween(ref activeTween_Text5);
        KillTween(ref activeTween_Text6);
    }

    private void KillTween(ref Tween tween)
    {
        if (tween != null && tween.IsActive())
            tween.Kill();
        tween = null;
    }

    private IEnumerator OnGrowthComplete()
    {
        if (twitchSlider != null)
            twitchSlider.interactable = false;

        Debug.Log("成长完成，切换至下一关卡");
        yield return new WaitForSeconds(changeDelay);
        TransitionManage.Instance.FadeOut(1f, Color.black, () =>
        {
            SceneManager.LoadScene(nextScene);
        });
    }

    private void OnDestroy()
    {
        KillAllTweens();
        if (twitchSlider != null)
            twitchSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}