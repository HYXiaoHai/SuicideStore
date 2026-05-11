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

    /// <summary>
    /// 尝试将拼图吸附到本插槽
    /// </summary>
    /// <param name="puzzle">要吸附的拼图</param>
    /// <returns>是否成功吸附</returns>
    public bool TrySnap(Puzzle puzzle)
    {
        if (puzzle == null) return false;

        // 如果拼图已经在这个插槽上，直接返回（避免重复吸附）
        if (puzzle.currentSlot == this) return false;

        // 计算拼图与插槽的距离（世界坐标，Canvas Overlay模式下即屏幕坐标）
        float distance = Vector3.Distance(puzzle.transform.position, transform.position);
        if (distance > snapRadius) return false;

        // --- 开始吸附 ---
        // 记录拼图的原始位置和原始槽位
        Vector3 originalPos = puzzle.transform.position;
        Slot originalSlot = puzzle.currentSlot;

        // 情况1：插槽上已有拼图（且不是当前拼图本身）
        if (currentPuzzle != null && currentPuzzle != puzzle)
        {
            // 将原有拼图移动到新拼图原来的位置
            currentPuzzle.transform.position = originalPos;
            // 更新原有拼图的槽位信息
            currentPuzzle.currentSlot = originalSlot;
            // 如果原位置有一个插槽，更新那个插槽的当前拼图
            if (originalSlot != null)
            {
                originalSlot.currentPuzzle = currentPuzzle;
            }
        }

        // 情况2：插槽上没有拼图，或者已经处理完交换
        // 将新拼图吸附到本插槽
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