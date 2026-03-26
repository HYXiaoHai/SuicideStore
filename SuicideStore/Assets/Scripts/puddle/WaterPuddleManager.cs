using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 水洼顺序交互管理器（透明度控制版）
/// 通过 Image 的 Alpha 值控制水洼的显示与隐藏，点击当前水洼后淡出，下一个水洼淡入
/// </summary>
public class SimpleWaterPuddleManager : MonoBehaviour
{
    [Header("水洼按钮（按顺序索引0,1,2）")]
    [SerializeField] private Button[] puddles = new Button[3];

    [Header("透明度动画时长")]
    [SerializeField] private float fadeDuration = 1f;

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
            Debug.Log("所有水洼已踩完！");
            // 可在此触发完成事件
        }
    }

    /// <summary>
    /// 重置流程（外部调用，用于重新开始）
    /// </summary>
    public void ResetSequence()
    {
        // 停止所有进行中的淡入淡出动画，避免冲突
        foreach (var btn in puddles)
        {
            if (btn != null)
            {
                Image img = btn.GetComponent<Image>();
                if (img != null) img.DOKill();
            }
        }

        currentIndex = 0;
        isComplete = false;

        // 重置所有水洼透明度为0，交互关闭
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

        // 激活第一个水洼
        SetPuddleActive(0, true);
    }
}