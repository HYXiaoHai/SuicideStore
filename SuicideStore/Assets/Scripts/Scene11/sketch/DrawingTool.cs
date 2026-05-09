using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingTool : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D itemCollider;

    private Vector3 originalPosition;
    private bool isMoving = false;
    private Vector3 originalScale;          // 记录原始缩放
    private Tween scaleTween;               // 缩放动画引用
    [Header("当前工具")]
    public ToolType toolType = ToolType.None;
    
    [Header("拖拽手感")]
    public float hoverScale = 1.2f;         // 点击时放大倍数
    public float scaleDuration = 0.1f;      // 缩放动画时长

    private readonly float zOffset = -0.04f;   // 固定 Z 偏移
    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        originalScale = transform.localScale;
        SetZOffset();
    }
    private void SetZOffset()
    {
        Vector3 pos = transform.position;
        pos.z = zOffset;
        transform.position = pos;
    }

    void OnMouseDown()
    {
        // 调试：输出点击信息，帮助判断是否命中
        Debug.Log($"鼠标点击物品：{gameObject.name}");
        

        foreach (DrawingController dc in DrawManage.instance.drawingControllers)
        {
            dc._toolManager.SetTool(toolType);
        }
        // 播放放大动画
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        originalPosition = transform.position;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isMoving) return;
        transform.position = GetMouseWorldPos() + offset;
        SetZOffset();   // 拖拽时保持 Z
    }

    void OnMouseUp()
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        if (isMoving) return;
        foreach (DrawingController dc in DrawManage.instance.drawingControllers)
        {
            dc._toolManager.SetTool(ToolType.None);
        }
        MoveToTarget(originalPosition);
    }

    private void MoveToTarget(Vector3 targetPos)
    {
        isMoving = true;
        transform.DOMove(targetPos, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                SetZOffset();   // 移动完成后再次确保 Z
                isMoving = false;
            });
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        pos.z = zOffset;
        originalPosition = pos;
        transform.position = pos;
    }
}
