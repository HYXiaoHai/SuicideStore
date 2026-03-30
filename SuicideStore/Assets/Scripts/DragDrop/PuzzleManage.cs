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
    public Puzzle puzzle0;
    public Puzzle puzzle1;
    public Puzzle puzzle2;

    [Header("插槽")]
    public Slot slot0;
    public Slot slot1;
    public Slot slot2;

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
                // 显示最终气泡（使用预设）
                DefendManage.Instance.ShowFinalBubble();
                // 显示特殊按钮
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
        puzzleArea.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
        puzzleArea.interactable = true;
        puzzleArea.blocksRaycasts = true;
        Puzzle[] puzzles = { puzzle0, puzzle1, puzzle2 };
        // 第一步：清除所有插槽的引用（彻底断开）
        foreach (Puzzle puzzle in puzzles)
        {
            if (puzzle == null) continue;
            puzzle.StartFloating();
        }

    }
    //重置
    public void ResetPuzzlesForNextRound(int roundIndex)
    {
        Debug.Log("重置开始，roundIndex=" + roundIndex);
        int spriteIndex = Mathf.Clamp(roundIndex, 0, 2); // 根据轮次选择图片

        Puzzle[] puzzles = { puzzle0, puzzle1, puzzle2 };
        Slot[] slots = { slot0, slot1, slot2 };

        // 第一步：清除所有插槽的引用（彻底断开）
        foreach (Slot slot in slots)
        {
            if (slot != null && slot.currentPuzzle != null)
            {
                slot.currentPuzzle.currentSlot = null;
                slot.currentPuzzle = null;
            }
        }

        // 第二步：移动拼图到默认位置并更换图片
        foreach (Puzzle puzzle in puzzles)
        {
            if (puzzle == null) continue;

            // 清除拼图自身的槽位引用（以防万一）
            puzzle.currentSlot = null;

            // 移动到默认位置
            if (puzzle.defaultPosition != null)
            {
                RectTransform targetRect = puzzle.defaultPosition.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    puzzle.GetComponent<RectTransform>().anchoredPosition = targetRect.anchoredPosition;
                }
                else
                {
                    puzzle.transform.position = puzzle.defaultPosition.position;
                }
                Debug.Log($"拼图 {puzzle.id} 移动到 {puzzle.defaultPosition.position}");
            }
            else
            {
                Debug.LogWarning($"拼图 {puzzle.id} 的 defaultPosition 未赋值！");
            }

            // 更换图片
            Image img = puzzle.GetComponent<Image>();
            if (img != null && puzzle.sprites != null && puzzle.sprites.Length > spriteIndex)
            {
                img.sprite = puzzle.sprites[spriteIndex];
            }
            puzzle.ResetMovement();   // 添加这一行
            puzzle.StartFloating();
        }


        CheckAllSlots();
      
        if (roundIndex >= 2)
        {
            Debug.Log("所有轮次结束，游戏进入最终阶段");
            // 可在此禁用拼图拖拽等功能
        }
    }
}