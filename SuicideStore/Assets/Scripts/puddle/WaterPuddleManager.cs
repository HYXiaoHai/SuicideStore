using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.Universal;  // 用于协程

public class SimpleWaterPuddleManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;//用于开局的转场

    [Header("水洼按钮（按顺序索引0,1,2）")]
    [SerializeField] private Button[] puddles = new Button[3];

    [Header("脚印（按显示顺序）")]
    public Image footprint1;
    public Image footprint2;
    public Image footprint3;
    public Image footprint4;

    [Header("透明度动画时长")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("启动Timeline的按钮")]
    public Button switchButton;
    public float switchButtonTargetScale = 1.7542f;  // 目标缩放值
    public float switchButtonAnimDuration = 0.5f;    // 动画时长
    public PlayableDirector timeline;

    [Header("音效")]
    public AudioSource puddleAudioSource;
    public AudioClip puddles1Clip;
    public AudioClip puddles2Clip;
    public AudioClip puddles3Clip;
    [Header("BGM")]
    public AudioSource BGMAudioSource;
    public AudioClip BGM1Clip;//当前关BGM
    public AudioClip BGM2Clip;//下一关的BGM
    [SerializeField] private float bgmFadeDuration = 1f;  // BGM 淡入淡出时长

    private int currentIndex = 0;      // 当前等待被点击的水洼索引
    private bool isComplete = false;    // 是否已完成所有交互
    private float originalBGMVolume;             // 记录原始 BGM 音量

    void Start()
    {
        // 记录原始 BGM 音量
        if (BGMAudioSource != null)
            originalBGMVolume = BGMAudioSource.volume;

        // 预加载所有 BGM Clip，避免切换时卡顿
        if (BGM1Clip != null) BGM1Clip.LoadAudioData();
        if (BGM2Clip != null) BGM2Clip.LoadAudioData();

        // 初始禁用所有水洼交互，透明度设为0
        foreach (var btn in puddles)
        {
            if (btn != null)
            {
                btn.interactable = false;
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f;
                    img.color = c;
                }
            }
        }

        // 添加点击监听
        for (int i = 0; i < puddles.Length; i++)
        {
            int index = i;
            puddles[i].onClick.AddListener(() => OnPuddleClicked(index));
        }

        // 初始隐藏所有脚印（透明度0）
        SetFootprintAlpha(footprint1, 0f);
        SetFootprintAlpha(footprint2, 0f);
        SetFootprintAlpha(footprint3, 0f);
        SetFootprintAlpha(footprint4, 0f);

        // 隐藏切换按钮
        if (switchButton != null)
        {
            switchButton.transform.localScale = Vector3.zero;
            switchButton.gameObject.SetActive(false);
        }

        // 开始只显示第一个水洼
        SetPuddleActive(0, true);
        for (int i = 1; i < puddles.Length; i++)
        {
            SetPuddleActive(i, false);
        }

        // 初始化 BGM（如果尚未播放）
        if (BGMAudioSource != null && BGM1Clip != null)
        {
            BGMAudioSource.clip = BGM1Clip;
            BGMAudioSource.Play();
        }

        //开局转场效果（白色渐入）
        canvasGroup.DOFade(0f, 1f).OnComplete(() => { canvasGroup.gameObject.SetActive(false); });
    }

    /// <summary>
    /// 设置水洼的可见性和交互性（透明度动画）
    /// </summary>
    private void SetPuddleActive(int index, bool active)
    {
        if (puddles[index] == null) return;

        Button btn = puddles[index];
        Image img = btn.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError($"水洼 {index} 缺少 Image 组件！");
            return;
        }

        btn.interactable = active;
        float targetAlpha = active ? 1f : 0f;
        img.DOFade(targetAlpha, fadeDuration).SetEase(Ease.OutQuad);

    }

    /// <summary>
    /// 设置单个脚印的透明度（无动画，用于初始）
    /// </summary>
    private void SetFootprintAlpha(Image footprint, float alpha)
    {
        if (footprint != null)
        {
            Color c = footprint.color;
            c.a = alpha;
            footprint.color = c;
        }
    }

    /// <summary>
    /// 渐显一个脚印
    /// </summary>
    private void ShowFootprint(Image footprint, float duration = 0.5f)
    {
        if (footprint != null)
        {
            footprint.DOFade(1f, duration).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 点击水洼的处理
    /// </summary>
    private void OnPuddleClicked(int clickedIndex)
    {
        if (isComplete || clickedIndex != currentIndex) return;

        // 1. 播放水坑动画（一次性）
        WaterPuddleButton btnCtrl = puddles[clickedIndex].GetComponent<WaterPuddleButton>();
        if (btnCtrl != null)
        {
            btnCtrl.PlayAnimation();   // 播放 Animator 动画
            btnCtrl.IsClick();         // 标记已点击，禁用悬停缩放
        }

        // 2. 当前水洼淡出消失
        //SetPuddleActive(currentIndex, false);

        // 3. 根据点击的是第几个水洼，显示对应的脚印
        switch (currentIndex)
        {
            case 0: // 点击第一个水坑 → 显示脚印1和脚印2（依次）
                ShowFootprint(footprint1);
                puddleAudioSource.PlayOneShot(puddles1Clip);
                // 延迟0.2秒显示第二个脚印，增加顺序感
                StartCoroutine(DelayedShowFootprint(footprint2, 0.2f));
                break;
            case 1: // 点击第二个水坑 → 显示脚印3
                puddleAudioSource.PlayOneShot(puddles2Clip);

                ShowFootprint(footprint3);
                break;
            case 2: // 点击第三个水坑 → 显示脚印4，之后显示切换按钮
                puddleAudioSource.PlayOneShot(puddles3Clip);

                ShowFootprint(footprint4);
                StartCoroutine(ShowSwitchButtonAfterDelay(0.5f));
                break;
        }

        // 4. 激活下一个水洼（如果还有）
        if (currentIndex + 1 < puddles.Length)
        {
            currentIndex++;
            SetPuddleActive(currentIndex, true);
        }
        else
        {
            // 所有水洼已点击完成（但第三个水坑的后续处理已在case中执行）
            isComplete = true;
        }
    }

    private IEnumerator DelayedShowFootprint(Image footprint, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowFootprint(footprint);
    }

    //
    private IEnumerator ShowSwitchButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (switchButton != null)
        {
            switchButton.gameObject.SetActive(true);
            // 设置初始缩放为 0（动画起点）
            switchButton.transform.localScale = Vector3.zero;
            // 设置初始缩放为0，然后播放放大动画
            switchButton.transform.DOScale(switchButtonTargetScale, switchButtonAnimDuration)
                .SetEase(Ease.OutBack);  // 使用弹性缓动，更有活力

            switchButton.onClick.RemoveAllListeners(); // 避免重复添加
            switchButton.onClick.AddListener(OnSwitchButtonClick);
        }
    }
    //切换下一关的按钮
    private void OnSwitchButtonClick()
    {
        switchButton.gameObject.SetActive(false);

        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
            timeline.Play();
        }
        //切换BGM
        BGMAudioSource.DOFade(0f, bgmFadeDuration).OnComplete(() =>
        {
            // 2. 切换 BGM Clip 并从头播放（音量保持 0）
            BGMAudioSource.clip = BGM2Clip;
            BGMAudioSource.Play();
            BGMAudioSource.volume = 0f;

        });
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnTimelineStopped;
        // 4. 淡入 BGM 到原始音量
        BGMAudioSource.DOFade(originalBGMVolume, 0.5f);
        DefendManage.Instance.StartScene2Dialogue();  // 你的原有逻辑
    }
}