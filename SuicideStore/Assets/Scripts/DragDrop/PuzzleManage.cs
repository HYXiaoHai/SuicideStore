using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
//拼图系统
public class PuzzleManage : MonoBehaviour
{
    public static PuzzleManage Instance;
    public CanvasGroup puzzleArea;//用来控制拼图区域显示的

    
    [Header("拼图")]
    public Puzzle[] puzzles1;//第一轮拼图
    public Puzzle[] puzzles2;//第二轮拼图
    public Puzzle[] puzzles3;//第三轮拼图

    [Header("插槽")]
    public Slot slot0;
    public Slot slot1;
    public Slot slot2;

    private Puzzle[] currentPuzzles;   // 当前激活的拼图数组
    private int currentRound = 0;      // 0=第一轮, 1=第二轮, 2=第三轮
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // 初始隐藏拼图区域（透明度0，不可交互）
        if (puzzleArea != null)
        {
            puzzleArea.alpha = 0f;
            puzzleArea.interactable = false;
            puzzleArea.blocksRaycasts = false;
        }
        // 初始隐藏所有拼图
        HideAllPuzzles();
    }

    //检查所有插槽，如果每个插槽都有拼图且 ID 匹配，则显示按钮；否则隐藏
    public void CheckAllSlots()
    {
        Slot[] slots = { slot0, slot1, slot2 };
        bool allCorrect = true;

        foreach (Slot slot in slots)
        {
            if (slot == null || slot.currentPuzzle == null || slot.currentPuzzle.id != slot.slotId)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            if (DefendManage.Instance.defendNum == 2) // 第三次完成
            {
                DefendManage.Instance.ShowFinalBubble();
                DefendManage.Instance.ShowSpecialDefendButton();
            }
            else
            {
                DefendManage.Instance.ShowDefendButton();
            }
        }
        else
        {
            DefendManage.Instance.HideDefendButton();
            DefendManage.Instance.HideSpecialDefendButton();
        }
    }
    public void ShowPuzzleArea()
    {
        if (puzzleArea == null) return;
        // 淡入拼图区域
        puzzleArea.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
        puzzleArea.interactable = true;
        puzzleArea.blocksRaycasts = true;

        // 激活第一轮拼图
        ActivateRound(0);

    }
    //重置
    public void ResetPuzzlesForNextRound(int roundIndex)
    {
        Debug.Log($"重置拼图，进入第 {roundIndex + 1} 轮");

        // 清除所有插槽的引用（因为旧拼图会被隐藏）
        Slot[] slots = { slot0, slot1, slot2 };
        foreach (Slot slot in slots)
        {
            if (slot != null && slot.currentPuzzle != null)
            {
                slot.currentPuzzle.currentSlot = null;
                slot.currentPuzzle = null;
            }
        }

        // 激活下一轮拼图
        int nextRound = Mathf.Clamp(roundIndex, 0, 2);
        ActivateRound(nextRound);

        // 重新检查插槽状态（此时所有插槽应无拼图，按钮会隐藏）
        CheckAllSlots();

        if (roundIndex >= 2)
        {
            Debug.Log("所有轮次结束，游戏进入最终阶段");
        }
    }
    //激活
    private void ActivateRound(int round)
    {
        // 隐藏当前所有拼图
        HideAllPuzzles();

        // 获取新轮次的拼图数组
        Puzzle[] newPuzzles = null;
        switch (round)
        {
            case 0: newPuzzles = puzzles1; break;
            case 1: newPuzzles = puzzles2; break;
            case 2: newPuzzles = puzzles3; break;
        }

        if (newPuzzles == null || newPuzzles.Length != 3)
        {
            Debug.LogError($"第{round + 1}轮拼图配置错误，需要3个拼图！");
            return;
        }

        currentPuzzles = newPuzzles;
        currentRound = round;

        // 建立 id -> Slot 的映射
        var slotMap = new Dictionary<int, Slot>
    {
        { 0, slot0 },
        { 1, slot1 },
        { 2, slot2 }
    };

        // 遍历当前轮次的拼图，根据其 id 设置对应插槽的位置
        foreach (Puzzle puzzle in currentPuzzles)
        {
            if (puzzle == null) continue;

            // 根据拼图的 id 找到对应的插槽
            if (slotMap.TryGetValue(puzzle.id, out Slot targetSlot) && targetSlot != null)
            {
                // 如果拼图配置了 currentSlotPosition，则将插槽移动到该位置
                if (puzzle.currentSlotPosition != null)
                {
                    targetSlot.transform.position = puzzle.currentSlotPosition.position;
                    // 如果需要同步旋转，可以取消注释下一行
                    // targetSlot.transform.rotation = puzzle.currentSlotPosition.rotation;
                    Debug.Log($"设置插槽 {puzzle.id} 的位置到 {puzzle.currentSlotPosition.position}");
                }
                else
                {
                    Debug.LogWarning($"拼图 {puzzle.name} (id={puzzle.id}) 的 currentSlotPosition 未赋值！");
                }
            }
            else
            {
                Debug.LogError($"找不到 id={puzzle.id} 对应的插槽！");
            }

            // 激活拼图并初始化
            puzzle.gameObject.SetActive(true);
            puzzle.currentSlot = null;

            // 移动到默认位置（漂浮起始点）
            if (puzzle.defaultPosition != null)
            {
                RectTransform targetRect = puzzle.defaultPosition.GetComponent<RectTransform>();
                if (targetRect != null)
                    puzzle.GetComponent<RectTransform>().anchoredPosition = targetRect.anchoredPosition;
                else
                    puzzle.transform.position = puzzle.defaultPosition.position;
            }
            // 设置随机旋转
            puzzle.SetRandomRotation();
            // 重置运动方向并开始漂浮
            puzzle.ResetMovement();
            puzzle.StartFloating();
        }
    }
    private void HideAllPuzzles()
    {
        HidePuzzlesArray(puzzles1);
        HidePuzzlesArray(puzzles2);
        HidePuzzlesArray(puzzles3);
    }

    private void HidePuzzlesArray(Puzzle[] puzzles)
    {
        if (puzzles == null) return;
        foreach (Puzzle p in puzzles)
        {
            if (p != null)
            {
                // 停止漂浮，防止隐藏后仍运行协程
                p.StopFloating();
                p.gameObject.SetActive(false);
            }
        }
    }
}