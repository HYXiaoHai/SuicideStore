using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

/// <summary>
/// 水洼顺序交互管理器（透明度控制版）
/// 通过 Image 的 Alpha 值控制水洼的显示与隐藏，点击当前水洼后淡出，下一个水洼淡入
/// </summary>
public class SimpleWaterPuddleManager : MonoBehaviour
{
    [Header("水洼按钮（按顺序索引0,1,2）")]
    [SerializeField] private Button[] puddles = new Button[3];

    [Header("脚印")]
    public Image footprint1;
    public Image footprint2;
    public Image footprint3;
    public Image footprint4;

    [Header("透明度动画时长")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("启动Timeline的按钮")]
    public Button switchButton;//切换画面的按钮
    public PlayableDirector timeline;


    private int currentIndex = 0;      // 当前等待被点击的水洼索引
    private bool isComplete = false;    // 是否已完成所有交互

    void Start()
    {
        // 初始全部禁用交互，并设置透明度为0（不可见）
        foreach (var btn in puddles)
        {
            if (btn != null)
            {
                btn.interactable = false;
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    Color color = img.color;
                    color.a = 0f;
                    img.color = color;
                }
            }
        }

        // 添加点击监听
        for (int i = 0; i < puddles.Length; i++)
        {
            int index = i;
            puddles[i].onClick.AddListener(() => OnPuddleClicked(index));
        }

        // 开始时只显示第一个水洼（淡入并启用交互）
        SetPuddleActive(0, true);
        for (int i = 1; i < puddles.Length; i++)
        {
            SetPuddleActive(i, false);
        }
    }

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

        // 控制交互性
        btn.interactable = active;
    
        // 控制透明度动画
        if(active)
        {
            img.DOFade(1f, fadeDuration).SetEase(Ease.OutQuad);

        }
    }

    /// <summary>
    /// 点击水洼的处理
    /// </summary>
    private void OnPuddleClicked(int clickedIndex)
    {
        // 忽略无效点击（已完成或顺序不对）
        if (isComplete || clickedIndex != currentIndex) return;

        // 当前水洼淡出并禁用交互（防止重复点击）
        SetPuddleActive(currentIndex, false);
        puddles[currentIndex].GetComponent<OrdinaryButton>().IsClick();
        // 判断是否还有下一个水洼
        if (currentIndex + 1 < puddles.Length)
        {
            // 激活下一个水洼（淡入）
            currentIndex++;
            SetPuddleActive(currentIndex, true);
        }
        else
        {
            // 所有水洼都已点击完成
            isComplete = true;
            OnAllPuddlesCompleted();
            // 可在此触发完成事件
        }
    }
    public void OnAllPuddlesCompleted()
    {
        if (switchButton != null)
        {
            switchButton.gameObject.SetActive(true);
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
        // 隐藏按钮或其他操作
        switchButton.gameObject.SetActive(false);
    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        director.stopped -= OnTimelineStopped;
        // 调用场景2对话启动
        DefendManage.Instance.StartScene2Dialogue();
    }

}