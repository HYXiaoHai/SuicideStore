using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D itemCollider;

    private Vector3 originalPosition;
    private bool wasInsideBagAtDragStart;
    private bool isMoving = false;
    private Vector3 originalScale;          // 记录原始缩放
    private Tween scaleTween;               // 缩放动画引用

    [Header("拖拽手感")]
    public float hoverScale = 1.2f;         // 点击时放大倍数
    public float scaleDuration = 0.1f;      // 缩放动画时长

    [Header("音效")]
    public AudioClip dragClip;

    private readonly float zOffset = -0.04f;   // 固定 Z 偏移

    public bool isInsideBag;
    public bool isOutsideSlot;
    public bool initialInside;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        originalScale = transform.localScale;
        SetZOffset();
        // 可选：自动调整碰撞体大小，使其略大于视觉范围（建议手动调整，这里仅作提示）
        if (itemCollider != null && itemCollider is BoxCollider2D box)
        {
            // 如果你希望自动扩大碰撞体，可以取消注释，但一般建议在编辑器中手动调整
            // box.size = Vector2.one * 0.8f;
        }
    }
    private void SetZOffset()
    {
        Vector3 pos = transform.position;
        pos.z = zOffset;
        transform.position = pos;
    }

    void OnMouseDown()
    {
        if (isMoving)
        {
            Debug.Log("物品动画中，禁止拖拽");
            return;
        }
        if (BagPackingManager.Instance == null || !BagPackingManager.Instance.isGameStarted||BagPackingManager.Instance.gameCompleted)
        {
            return;
        }

        // 播放放大动画
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        originalPosition = transform.position;
        wasInsideBagAtDragStart = isInsideBag;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isMoving) return;
        if (BagPackingManager.Instance == null || !BagPackingManager.Instance.isGameStarted || BagPackingManager.Instance.gameCompleted)
        {
            return;
        }
        transform.position = GetMouseWorldPos() + offset;
        SetZOffset();   // 拖拽时保持 Z
    }

    void OnMouseUp()
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        if (isMoving) return;

        bool inBag = BagPackingManager.Instance.IsInBagArea(transform.position);
        Collider2D targetSlot = BagPackingManager.Instance.GetSlotContainingPosition(transform.position);

        if (targetSlot != null)
        {
            Vector3 targetPos = targetSlot.transform.position;
            targetPos.z = zOffset;   // 目标位置也修正 Z
            MoveToTarget(targetPos);
            AudioManager.Instance.Play2DSound(dragClip, 0.8f);
        }
        else if (inBag)
        {
            AudioManager.Instance.Play2DSound(dragClip,0.8f);
            SetZOffset();   // 书包内也确保 Z
            BagPackingManager.Instance.CheckItemPlacement(this);
        }
        else
        {
            // 无效区域：回到拖拽前位置（原位置已经包含正确 Z）
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
                SetZOffset();   // 移动完成后再次确保 Z
                isMoving = false;
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