using DG.Tweening;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("拖拽缩放")]
    public float hoverScale = 1.2f;
    public float scaleDuration = 0.1f;

    [Header("音效")]
    public AudioClip dragClip;

    private Vector3 offset;
    private Camera mainCamera;
    private Collider2D itemCollider;

    private Vector3 originalPosition;
    private bool wasInsideBagAtDragStart;
    private bool isMoving = false;
    private Vector3 originalScale;
    private Tween scaleTween;
    private readonly float zOffset = -0.04f;

    // 状态标记
    public bool isInsideBag;
    public bool isOutsideSlot;
    public bool initialInside;
    public bool isProcessed = false; // 标记是否已执行消失逻辑

    void Start()
    {
        mainCamera = Camera.main;
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
        if (isMoving || isProcessed) return;
        if (BagPackingManager.Instance == null ||
            !BagPackingManager.Instance.isGameStarted ||
            BagPackingManager.Instance.gameCompleted)
            return;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        originalPosition = transform.position;
        wasInsideBagAtDragStart = isInsideBag;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isMoving || isProcessed) return;
        if (BagPackingManager.Instance == null ||
            !BagPackingManager.Instance.isGameStarted ||
            BagPackingManager.Instance.gameCompleted)
            return;

        transform.position = GetMouseWorldPos() + offset;
        SetZOffset();
    }

    void OnMouseUp()
    {
        if (isMoving || isProcessed) return;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        bool inBag = BagPackingManager.Instance.IsInBagArea(transform.position);
        Collider2D targetSlot = BagPackingManager.Instance.GetSlotContainingPosition(transform.position);

        if (targetSlot != null)
        {
            Vector3 targetPos = targetSlot.transform.position;
            targetPos.z = zOffset;
            MoveToTarget(targetPos);
            AudioManager.Instance.Play2DSound(dragClip, 0.8f);
        }
        else if (inBag)
        {
            AudioManager.Instance.Play2DSound(dragClip, 0.8f);
            SetZOffset();
            BagPackingManager.Instance.CheckItemPlacement(this);
        }
        else
        {
            MoveToTarget(originalPosition);
        }
    }

    private void MoveToTarget(Vector3 targetPos)
    {
        isMoving = true;
        transform.DOMove(targetPos, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                SetZOffset();
                isMoving = false;
                // 移动结束后，通知管理器检查是否需要触发消失逻辑
                BagPackingManager.Instance.CheckItemPlacement(this);
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