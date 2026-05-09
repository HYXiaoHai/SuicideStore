using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
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

    [Header("拖拽边界（相对于拼图父级局部坐标）")]
    public float dragLeft = -500f;
    public float dragRight = 500f;
    public float dragTop = 300f;
    public float dragBottom = -300f;
    // 需要手动指定拼图的父级 RectTransform（用于可视化，如果不指定则无法显示边界）
    [Header("可视化参考（拼图的父级 RectTransform）")]
    public RectTransform puzzleParentReference;

    [Header("插槽")]
    public Slot slot0;
    public Slot slot1;
    public Slot slot2;

    [Header("默认出生位置")]
    public Transform rectTransform1;//第一个默认出生位置
    public Transform rectTransform2;//第二个默认出生位置
    public Transform rectTransform3;//第三个默认出生位置



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

        // 准备三个默认出生点（必须已赋值）
        Transform[] spawnPoints = { rectTransform1, rectTransform2, rectTransform3 };
        // 随机打乱出生点顺序（用于随机分配）
        System.Random rng = new System.Random();
        for (int i = spawnPoints.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[j];
            spawnPoints[j] = temp;
        }

        // 建立 id -> Slot 的映射
        var slotMap = new Dictionary<int, Slot>
    {
        { 0, slot0 },
        { 1, slot1 },
        { 2, slot2 }
    };

        // 遍历当前轮次的拼图，根据其 id 设置对应插槽的位置
        for (int i = 0; i < currentPuzzles.Length; i++)
        {
            Puzzle puzzle = currentPuzzles[i];
            if (puzzle == null) continue;

            // 根据拼图的 id 找到对应的插槽
            if (slotMap.TryGetValue(puzzle.id, out Slot targetSlot) && targetSlot != null)
            {
                if (puzzle.currentSlotPosition != null)
                {
                    targetSlot.transform.position = puzzle.currentSlotPosition.position;
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

            // 分配到随机出生点（使用打乱后的 spawnPoints[i]）
            Transform spawnPoint = spawnPoints[i];
            if (spawnPoint != null)
            {
                RectTransform targetRect = spawnPoint.GetComponent<RectTransform>();
                if (targetRect != null)
                    puzzle.GetComponent<RectTransform>().anchoredPosition = targetRect.anchoredPosition;
                else
                    puzzle.transform.position = spawnPoint.position;
            }
            else
            {
                Debug.LogWarning($"出生点 {i} 未赋值！使用拼图自身的 defaultPosition 作为后备");
                if (puzzle.defaultPosition != null)
                {
                    RectTransform targetRect = puzzle.defaultPosition.GetComponent<RectTransform>();
                    if (targetRect != null)
                        puzzle.GetComponent<RectTransform>().anchoredPosition = targetRect.anchoredPosition;
                    else
                        puzzle.transform.position = puzzle.defaultPosition.position;
                }
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (puzzleParentReference == null) return;

        // 定义局部坐标下的四个角（左下、左上、右上、右下）
        Vector3 localMin = new Vector3(dragLeft, dragBottom, 0);
        Vector3 localMax = new Vector3(dragRight, dragTop, 0);
        Vector3[] localCorners = new Vector3[]
        {
            new Vector3(localMin.x, localMin.y, 0), // 左下
            new Vector3(localMin.x, localMax.y, 0), // 左上
            new Vector3(localMax.x, localMax.y, 0), // 右上
            new Vector3(localMax.x, localMin.y, 0)  // 右下
        };

        // 转换到世界坐标
        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = puzzleParentReference.TransformPoint(localCorners[i]);
        }

        Gizmos.color = Color.green;
        // 绘制线框
        Gizmos.DrawLine(worldCorners[0], worldCorners[1]);
        Gizmos.DrawLine(worldCorners[1], worldCorners[2]);
        Gizmos.DrawLine(worldCorners[2], worldCorners[3]);
        Gizmos.DrawLine(worldCorners[3], worldCorners[0]);

        // 绘制半透明填充（需要 Handles）
        Handles.DrawSolidRectangleWithOutline(worldCorners, new Color(0, 1, 0, 0.1f), Color.green);
    }
#endif
}