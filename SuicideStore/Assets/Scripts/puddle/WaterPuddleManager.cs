using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.Universal;  // 用于协程

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

    [Header("启动Timeline的按钮")]
    public Button switchButton;
    public PlayableDirector timeline;

    private int currentIndex = 0;      // 当前等待被点击的水洼索引
    private bool isComplete = false;    // 是否已完成所有交互

    void Start()
    {
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
            switchButton.gameObject.SetActive(false);

        // 开始只显示第一个水洼
        SetPuddleActive(0, true);
        for (int i = 1; i < puddles.Length; i++)
        {
            SetPuddleActive(i, false);
        }
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
        OrdinaryButton btnCtrl = puddles[clickedIndex].GetComponent<OrdinaryButton>();
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
                // 延迟0.2秒显示第二个脚印，增加顺序感
                StartCoroutine(DelayedShowFootprint(footprint2, 0.2f));
                break;
            case 1: // 点击第二个水坑 → 显示脚印3
                ShowFootprint(footprint3);
                break;
            case 2: // 点击第三个水坑 → 显示脚印4，之后显示切换按钮
                ShowFootprint(footprint4);
                StartCoroutine(ShowSwitchButtonAfterDelay(0.3f));
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

    private IEnumerator ShowSwitchButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (switchButton != null)
        {
            switchButton.gameObject.SetActive(true);
            switchButton.onClick.RemoveAllListeners(); // 避免重复添加
            switchButton.onClick.AddListener(OnSwitchButtonClick);
        }
    }

    private void OnSwitchButtonClick()
    {
        if (timeline != null)
        {
            timeline.stopped += OnTimelineStopped;
            timeline.Play();
        }
        switchButton.gameObject.SetActive(false);
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnTimelineStopped;
        DefendManage.Instance.StartScene2Dialogue();  // 你的原有逻辑
    }

    /// <summary>
    /// 重置整个流程（供外部调用）
    /// </summary>
    public void ResetSequence()
    {
        // 停止所有动画
        foreach (var btn in puddles)
        {
            if (btn != null) btn.GetComponent<Image>()?.DOKill();
        }
        footprint1?.DOKill();
        footprint2?.DOKill();
        footprint3?.DOKill();
        footprint4?.DOKill();

        currentIndex = 0;
        isComplete = false;

        // 重置所有水洼透明度为0，禁用交互
        for (int i = 0; i < puddles.Length; i++)
        {
            if (puddles[i] != null)
            {
                puddles[i].interactable = false;
                Image img = puddles[i].GetComponent<Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = 0f;
                    img.color = c;
                }
            }
        }

        // 重置所有脚印透明度为0
        SetFootprintAlpha(footprint1, 0f);
        SetFootprintAlpha(footprint2, 0f);
        SetFootprintAlpha(footprint3, 0f);
        SetFootprintAlpha(footprint4, 0f);
       
        // 隐藏切换按钮
        if (switchButton != null)
            switchButton.gameObject.SetActive(false);

        // 激活第一个水洼
        SetPuddleActive(0, true);
    }
}