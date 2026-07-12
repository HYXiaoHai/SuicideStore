using DG.Tweening;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public int slotId;                // 插槽ID，用于匹配拼图（后续用）
    public float snapRadius = 50f;    // 吸附半径（像素）
    public Puzzle currentPuzzle;     // 当前吸附在此插槽上的拼图
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    //尝试将拼图吸附到本插槽
    public bool TrySnap(Puzzle puzzle)
    {
        if (puzzle == null) return false;
        if (puzzle.currentSlot == this) return false;

        float distance = Vector3.Distance(puzzle.transform.position, transform.position);
        if (distance > snapRadius) return false;

        Slot originalSlot = puzzle.currentSlot;

        // 如果当前插槽已有拼图，且不是新拼图本身（替换逻辑）
        if (currentPuzzle != null && currentPuzzle != puzzle)
        {
            Puzzle oldPuzzle = currentPuzzle;
            oldPuzzle.currentSlot = null;

            if (PuzzleManage.Instance != null)
            {
                RectTransform oldRect = oldPuzzle.GetComponent<RectTransform>();
                Vector2 oldPos = oldRect.anchoredPosition;
                Vector2 targetPos = PuzzleManage.Instance.GetClosestBoundaryPosition(oldPos);
                oldPuzzle.StopFloating();
                oldRect.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    oldPuzzle.StartFloating();
                });
            }
            else
            {
                oldPuzzle.StartFloating();
            }
        }

        if (originalSlot != null && originalSlot.currentPuzzle == puzzle)
        {
            originalSlot.currentPuzzle = null;
        }

        puzzle.transform.position = transform.position;
        puzzle.currentSlot = this;
        currentPuzzle = puzzle;
        puzzle.OnSnappedToSlot();

        return true;
    }

    /// <summary>
    /// 移除当前插槽上的拼图（拖拽开始时调用）
    /// </summary>
    public void RemovePuzzle()
    {
        if (currentPuzzle != null && currentPuzzle.currentSlot == this)
        {
            currentPuzzle.currentSlot = null;
            currentPuzzle.OnRemovedFromSlot();   // 新增
            currentPuzzle = null;
        }
    }

    // 可选：在编辑器中显示吸附半径
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }
}