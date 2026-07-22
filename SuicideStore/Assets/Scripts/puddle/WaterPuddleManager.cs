using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class SimpleWaterPuddleManager : MonoBehaviour
{
    [Header("水洼按钮（按顺序索引0,1,2）")]
    [SerializeField] private Button[] puddles = new Button[3];

    [Header("脚印（按显示顺序）")]
    public Image footprint1;
    public Image footprint2;
    public Image footprint3;
    public Image footprint4;

    [Header("透明度动画时长")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("乐乐跳跃动画")]
    public Image leleAniImage;          // 乐乐本体
    public Image leleREAniImage;        // 乐乐倒影
    public Sprite[] leleJumpSprites1;    // 1→2 跳跃帧（本体）
    public Sprite[] leleJumpSprites2;    // 2→3 跳跃帧（本体）
    public Sprite[] leleREJumpSprites1;  // 1→2 跳跃帧（倒影）
    public Sprite[] leleREJumpSprites2;  // 2→3 跳跃帧（倒影）
    [SerializeField] private float frameInterval = 0.08f;
    [SerializeField] private float jumpToPuddleDelay = 0.2f;

    [Header("启动Timeline的按钮")]
    public Button switchButton;
    public float switchButtonTargetScale = 1.7542f;
    public float switchButtonAnimDuration = 0.5f;
    public PlayableDirector timeline;

    [Header("音效")]
    public AudioClip puddles1Clip;
    public AudioClip puddles2Clip;
    public AudioClip puddles3Clip;

    [Header("BGM")]
    public AudioClip BGM1Clip;
    public AudioClip BGM2Clip;
    [SerializeField] private float bgmFadeDuration = 1f;

    private int currentIndex = 0;
    private bool isComplete = false;
    private bool isAnimating = false;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.white);

        if (BGM1Clip != null) BGM1Clip.LoadAudioData();
        if (BGM2Clip != null) BGM2Clip.LoadAudioData();

        // 初始化水洼
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

        for (int i = 0; i < puddles.Length; i++)
        {
            int index = i;
            puddles[i].onClick.AddListener(() => OnPuddleClicked(index));
        }

        SetFootprintAlpha(footprint1, 0f);
        SetFootprintAlpha(footprint2, 0f);
        SetFootprintAlpha(footprint3, 0f);
        SetFootprintAlpha(footprint4, 0f);

        // 乐乐本体初始隐藏
        if (leleAniImage != null)
        {
            leleAniImage.gameObject.SetActive(false);
            leleAniImage.color = new Color(leleAniImage.color.r, leleAniImage.color.g, leleAniImage.color.b, 0f);
        }

        //新增：乐乐倒影初始隐藏
        if (leleREAniImage != null)
        {
            leleREAniImage.gameObject.SetActive(false);
            leleREAniImage.color = new Color(leleREAniImage.color.r, leleREAniImage.color.g, leleREAniImage.color.b, 0f);
        }

        if (switchButton != null)
        {
            switchButton.transform.localScale = Vector3.zero;
            switchButton.gameObject.SetActive(false);
        }

        SetPuddleActive(0, true);
        for (int i = 1; i < puddles.Length; i++)
            SetPuddleActive(i, false);
    }

    // ---------- 跳跃动画（同步播放本体+倒影） ----------
    private IEnumerator PlayLeLeJumpCoroutine(Sprite[] jumpSprites, Sprite[] reJumpSprites)
    {
        if (jumpSprites == null || jumpSprites.Length == 0)
            yield break;

        // 检查倒影数组是否有效且长度一致
        bool hasRe = reJumpSprites != null && reJumpSprites.Length == jumpSprites.Length;

        for (int i = 0; i < jumpSprites.Length; i++)
        {
            // 设置本体
            leleAniImage.sprite = jumpSprites[i];
            // 如果有对应的倒影帧，同步设置
            if (hasRe)
                leleREAniImage.sprite = reJumpSprites[i];

            yield return new WaitForSeconds(frameInterval);
        }
    }

    // ---------- 播放水洼动画（封装） ----------
    private void PlayPuddleAnimation(int index)
    {
        WaterPuddleButton btnCtrl = puddles[index].GetComponent<WaterPuddleButton>();
        if (btnCtrl != null)
        {
            btnCtrl.PlayAnimation();
            btnCtrl.IsClick();
        }
    }

    // ---------- 核心点击逻辑 ----------
    private void OnPuddleClicked(int clickedIndex)
    {
        if (isComplete || clickedIndex != currentIndex || isAnimating)
            return;

        puddles[clickedIndex].interactable = false;

        isAnimating = true;
        StartCoroutine(HandleClickSequence(clickedIndex));
    }

    /// <summary>
    /// 按顺序处理点击后的所有动画和逻辑
    /// </summary>
    private IEnumerator HandleClickSequence(int index)
    {
        // ----- 水洼1：播放水洼动画，同时渐显本体和倒影 -----
        if (index == 0)
        {
            PlayPuddleAnimation(index);
            yield return new WaitForSeconds(jumpToPuddleDelay);

            // 渐显本体
            if (leleAniImage != null && !leleAniImage.gameObject.activeSelf)
            {
                leleAniImage.gameObject.SetActive(true);
                leleAniImage.DOFade(1f, 1f).SetEase(Ease.OutQuad);
            }
            // 渐显倒影
            if (leleREAniImage != null && !leleREAniImage.gameObject.activeSelf)
            {
                leleREAniImage.gameObject.SetActive(true);
                leleREAniImage.DOFade(1f, 1f).SetEase(Ease.OutQuad);
            }
        }
        // ----- 水洼2：跳跃动画1（本体+倒影） → 水洼动画 -----
        else if (index == 1)
        {
            if (leleJumpSprites1 != null && leleJumpSprites1.Length > 0)
                yield return StartCoroutine(PlayLeLeJumpCoroutine(leleJumpSprites1, leleREJumpSprites1));
            else
                Debug.LogWarning("跳跃动画1 帧数组为空");

            PlayPuddleAnimation(index);
        }
        // ----- 水洼3：跳跃动画2（本体+倒影） → 水洼动画 -----
        else if (index == 2)
        {
            if (leleJumpSprites2 != null && leleJumpSprites2.Length > 0)
                yield return StartCoroutine(PlayLeLeJumpCoroutine(leleJumpSprites2, leleREJumpSprites2));
            else
                Debug.LogWarning("跳跃动画2 帧数组为空");

            PlayPuddleAnimation(index);
        }

        ProcessClick(index);
        isAnimating = false;
    }

    /// <summary>
    /// 处理点击后的固定逻辑：显示脚印、播放音效、激活下一水洼
    /// </summary>
    private void ProcessClick(int index)
    {
        switch (index)
        {
            case 0:
                ShowFootprint(footprint1);
                AudioManager.Instance.PlayShortSound(puddles1Clip, 0.8f);
                StartCoroutine(DelayedShowFootprint(footprint2, 0.2f));
                break;
            case 1:
                AudioManager.Instance.PlayShortSound(puddles2Clip, 0.8f);
                ShowFootprint(footprint3);
                break;
            case 2:
                AudioManager.Instance.PlayShortSound(puddles3Clip, 0.8f);
                ShowFootprint(footprint4);
                StartCoroutine(ShowSwitchButtonAfterDelay(0.5f));
                break;
        }

        if (index + 1 < puddles.Length)
        {
            currentIndex = index + 1;
            SetPuddleActive(currentIndex, true);
        }
        else
        {
            isComplete = true;
        }
    }

    // ---------- 辅助方法 ----------
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

    private void SetFootprintAlpha(Image footprint, float alpha)
    {
        if (footprint != null)
        {
            Color c = footprint.color;
            c.a = alpha;
            footprint.color = c;
        }
    }

    private void ShowFootprint(Image footprint, float duration = 0.5f)
    {
        if (footprint != null)
            footprint.DOFade(1f, duration).SetEase(Ease.OutQuad);
    }

    private IEnumerator DelayedShowFootprint(Image footprint, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowFootprint(footprint);
    }

    private IEnumerator ShowSwitchButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);


        if (switchButton != null)
        {
            switchButton.gameObject.SetActive(true);
            switchButton.transform.localScale = Vector3.zero;
            switchButton.transform.DOScale(switchButtonTargetScale, switchButtonAnimDuration)
                .SetEase(Ease.OutBack);
            switchButton.onClick.RemoveAllListeners();
            switchButton.onClick.AddListener(OnSwitchButtonClick);
        }
        yield return new WaitForSeconds(1f);
        SwitchButtonMoveAniPlay();
        leleAniImage.DOFade(0f, 1f);
        leleREAniImage.DOFade(0f, 1f);
    }
    public void SwitchButtonMoveAniPlay()
    {
        //泡泡上下浮动的动画
        if (switchButton != null)
        {
            float moveDistance = 10f; //上下移动的距离
            float moveDuration = 1f; //移动的时间
            switchButton.transform.DOLocalMoveY(switchButton.transform.localPosition.y + moveDistance, moveDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    private void OnSwitchButtonClick()
    {
        switchButton.gameObject.SetActive(false);
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
            timeline.Play();
        }
        AudioManager.Instance.SwitchBGM(BGM2Clip, 1f);
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnTimelineStopped;
        DefendManage.Instance.StartScene2Dialogue();
    }
}