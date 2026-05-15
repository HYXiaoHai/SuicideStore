using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MspPaint : MonoBehaviour
{
    [Header("绘画设置")]
    public Color paintColor = Color.red;
    public float paintSize = 0.1f;
    public Material lineMaterial;

    [Header("回退设置")]
    public float retractDuration = 1f;

    private LineRenderer currentLine;
    private List<Vector3> positions = new List<Vector3>();
    private bool isDrawing = false;
    private Vector3 lastMousePos;
    private Tween retractTween;

    // 事件
    public System.Action<Vector3> OnDrawingPositionUpdated;
    public System.Action OnDrawingFinished;

    private void Update()
    {
        // 只在绘制中处理鼠标移动和抬起（不再自动检测按下）
        if (isDrawing && currentLine != null)
        {
            Vector3 currentPos = GetMousePosition();
            if (Vector3.Distance(currentPos, lastMousePos) > 0.1f)
                AddPosition(currentPos);

            OnDrawingPositionUpdated?.Invoke(currentPos);
        }

        if (isDrawing && Input.GetMouseButtonUp(0))
        {
            EndDrawing();
        }
    }

    /// <summary>
    /// 外部调用：开始绘制（必须传入有效的起点位置）
    /// </summary>
    public void StartDrawing(Vector3 startPos)
    {
        if (isDrawing) return;
        if (retractTween != null && retractTween.IsActive())
        {
            retractTween.Kill();
            if (currentLine != null) Destroy(currentLine.gameObject);
            currentLine = null;
            positions.Clear();
        }

        GameObject go = new GameObject("PencilLine");
        go.transform.SetParent(transform);
        currentLine = go.AddComponent<LineRenderer>();
        currentLine.textureMode = LineTextureMode.Tile;
        currentLine.material = lineMaterial;
        currentLine.startWidth = paintSize;
        currentLine.endWidth = paintSize;
        currentLine.startColor = paintColor;
        currentLine.endColor = paintColor;
        currentLine.numCornerVertices = 5;
        currentLine.numCapVertices = 5;

        AddPosition(startPos);
        lastMousePos = startPos;
        isDrawing = true;
    }

    private void AddPosition(Vector3 pos)
    {
        lastMousePos = pos;
        if (pos != Vector3.zero)
        {
            positions.Add(pos);
        }
        currentLine.positionCount = positions.Count;
        currentLine.SetPositions(positions.ToArray());
    }

    private void EndDrawing()
    {
        if (!isDrawing) return;
        isDrawing = false;
        OnDrawingFinished?.Invoke();
    }

    /// <summary>
    /// 回退整条线（外部调用，例如顺序错误或漏连）
    /// </summary>
    public void RetractLine(System.Action onComplete = null)
    {
        if (currentLine == null) return;
        if (retractTween != null && retractTween.IsActive())
            retractTween.Kill();

        int startCount = positions.Count;
        if (startCount == 0)
        {
            onComplete?.Invoke();
            return;
        }

        retractTween = DOTween.To(
            () => startCount,
            x =>
            {
                int newCount = Mathf.RoundToInt(x);
                currentLine.positionCount = newCount;
                if (positions.Count > newCount)
                    positions.RemoveRange(newCount, positions.Count - newCount);
                if (newCount > 0)
                    currentLine.SetPositions(positions.ToArray());
            },
            0,
            retractDuration
        ).SetEase(Ease.Linear);

        retractTween.OnComplete(() =>
        {
            if (currentLine != null) Destroy(currentLine.gameObject);
            currentLine = null;
            positions.Clear();
            retractTween = null;
            onComplete?.Invoke();
        });
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        return hit.collider != null ? hit.point : (Vector3)mousePos;
    }

    public bool IsDrawing => isDrawing;
}