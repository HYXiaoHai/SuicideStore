using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FloatingBubbleManager : MonoBehaviour
{
    public static FloatingBubbleManager Instance;

    [Header("漂浮气泡预制体")]
    public GameObject[] floatingBubblePrefabs;
    [Header("漂浮气泡父物体")]
    public Transform floatingBubbleFather;

    [Header("漂浮气泡生成边界（相对于父节点局部坐标）")]
    public float dragLeft = -500f;
    public float dragRight = 500f;
    public float dragBottom = -300f;
    public float dragTop = 300f;

    [Header("可视化参考（用于Scene视图显示边界）")]
    public RectTransform visualizationParent;

    private int currentRound = 0;
    private int maxBubbleCount = 5;      // 普通气泡最大数量
    private int currentBubbleCount = 0;  // 当前普通气泡数量（特殊气泡不计数）
    private int specialBubbleCount = 0;      // 当前特殊气泡数量（仅统计，不限制）
    private List<GameObject> activeBubbles = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetRound(int round)
    {
        currentRound = round;
        switch (round)
        {
            case 0: maxBubbleCount = 5; break;
            case 1: maxBubbleCount = 8; break;
            case 2: maxBubbleCount = 12; break;
            default: maxBubbleCount = 5; break;
        }
    }

    private Vector2 GetRandomPositionInsideBoundary()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Range(dragLeft, dragRight);
            float randY = Random.Range(dragBottom, dragTop);
            Vector2 pos = new Vector2(randX, randY);
            if (pos.x >= dragLeft && pos.x <= dragRight &&
                pos.y >= dragBottom && pos.y <= dragTop)
                return pos;
        }
        float centerX = (dragLeft + dragRight) / 2f;
        float centerY = (dragBottom + dragTop) / 2f;
        return new Vector2(centerX, centerY);
    }

    public bool TryAddFloatingBubble(string content, bool isSpecial)
    {
        // 普通气泡：检查数量上限
        if (!isSpecial && currentBubbleCount >= maxBubbleCount)
        {
            Debug.Log($"本轮普通气泡已达上限 {maxBubbleCount}，无法生成新气泡");
            return false;
        }

        if (floatingBubblePrefabs == null)
        {
            Debug.LogError("FloatingBubbleManager: floatingBubblePrefab 未赋值");
            return false;
        }
        Transform parent = floatingBubbleFather != null ? floatingBubbleFather : transform;
        GameObject randObject = floatingBubblePrefabs[Random.Range(0, floatingBubblePrefabs.Length)];
        GameObject bubbleObj = Instantiate(randObject, parent);
        FloatingBubble bubble = bubbleObj.GetComponent<FloatingBubble>();
        if (bubble == null)
        {
            Debug.LogError("漂浮气泡预制体缺少 FloatingBubble 组件");
            Destroy(bubbleObj);
            return false;
        }

        Vector2 randomPos = GetRandomPositionInsideBoundary();
        bubble.SetInitialPosition(randomPos);
        bubble.SetSpecial(isSpecial);
        bubble.PlayAppearAnimation();

        activeBubbles.Add(bubbleObj);
        if (isSpecial)
            specialBubbleCount++;
        else
            currentBubbleCount++;

        return true;
    }

    public void OnBubbleDestroyed(bool wasSpecial)
    {
        if (wasSpecial)
            specialBubbleCount--;
        else
            currentBubbleCount--;
    }

    public void ClearAllBubbles()
    {
        foreach (var bubble in activeBubbles)
        {
            if (bubble != null) Destroy(bubble);
        }
        activeBubbles.Clear();
        currentBubbleCount = 0;
        specialBubbleCount = 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (visualizationParent == null) return;

        Vector3 localMin = new Vector3(dragLeft, dragBottom, 0);
        Vector3 localMax = new Vector3(dragRight, dragTop, 0);
        Vector3[] localCorners = new Vector3[]
        {
            new Vector3(localMin.x, localMin.y, 0),
            new Vector3(localMin.x, localMax.y, 0),
            new Vector3(localMax.x, localMax.y, 0),
            new Vector3(localMax.x, localMin.y, 0)
        };

        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
            worldCorners[i] = visualizationParent.TransformPoint(localCorners[i]);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < 4; i++)
            Gizmos.DrawLine(worldCorners[i], worldCorners[(i + 1) % 4]);

        Handles.DrawSolidRectangleWithOutline(worldCorners, new Color(0, 1, 1, 0.1f), Color.cyan);
    }
#endif
}