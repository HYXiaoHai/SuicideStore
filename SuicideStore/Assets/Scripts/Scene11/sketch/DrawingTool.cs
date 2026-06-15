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
    private Vector3 originalScale;          // ��¼ԭʼ����
    private Tween scaleTween;               // ���Ŷ�������
    [Header("工具类型")]
    public ToolType toolType = ToolType.None;
    [Header("音效")]
    public AudioClip tackClip;

    [Header("��ק�ָ�")]
    public float hoverScale = 1.2f;         // ���ʱ�Ŵ���
    public float scaleDuration = 0.1f;      // ���Ŷ���ʱ��

    private readonly float zOffset = -0.04f;   // �̶� Z ƫ��
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
        Debug.Log($"点击工具：{gameObject.name}");
        if (GameManage.Instance.isSetting) return;

        if (DrawManage.instance != null && DrawManage.instance.drawingControllers != null)
        {
            foreach (DrawingController dc in DrawManage.instance.drawingControllers)
            {
                if (dc != null && dc._toolManager != null)
                {
                    dc._toolManager.SetTool(toolType);
                }
            }
        }
        //拿起音效
        AudioManager.Instance.Play2DSound(tackClip,0.8f);

        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        //originalPosition = transform.position;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (GameManage.Instance.isSetting) return;
        if (isMoving) return;
        transform.position = GetMouseWorldPos() + offset;
        SetZOffset();

        if (DrawManage.instance != null && DrawManage.instance.drawingControllers != null)
        {
            foreach (DrawingController dc in DrawManage.instance.drawingControllers)
            {
                if (dc != null && dc._toolManager != null && dc._toolManager.currentType != ToolType.None)
                {
                    dc._toolManager.DrawAtToolPosition(transform.position);
                }
            }
        }
    }

    void OnMouseUp()
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);
        if (isMoving) return;
        if (DrawManage.instance != null && DrawManage.instance.drawingControllers != null)
        {
            foreach (DrawingController dc in DrawManage.instance.drawingControllers)
            {
                if (dc != null && dc._toolManager != null)
                {
                    dc._toolManager.SetTool(ToolType.None);
                }
            }
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
                SetZOffset();   // �ƶ���ɺ��ٴ�ȷ�� Z
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
