using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour
{
    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D itemCollider;

    private Vector3 originalPosition;      // 拖拽前记录的位置（用于无效区域回退）
    private bool wasInsideBagAtDragStart;  // 拖拽开始时是否在书包内（暂未使用但保留）
    private bool isMoving = false;         // 防止动画期间重复拖拽

    public bool isInsideBag;      // 当前是否在书包内
    public bool isOutsideSlot;    // 当前是否在外部展示区
    public bool initialInside;    // 初始是否在书包内（由管理器设置）

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
    }

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (isMoving) return; // 动画中禁止拖拽

        originalPosition = transform.position;
        wasInsideBagAtDragStart = isInsideBag;
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isMoving) return;
        transform.position = GetMouseWorldPos() + offset;
    }

    void OnMouseUp()
    {
        if (isMoving) return;

        bool inBag = BagPackingManager.Instance.IsInBagArea(transform.position);
        Collider2D targetSlot = BagPackingManager.Instance.GetSlotContainingPosition(transform.position);

        if (targetSlot != null)
        {
            // 吸附到外部展示区域中心（带动画）
            Vector3 targetPos = targetSlot.transform.position;
            Debug.Log($"物品吸附到展示区域：{targetSlot.name}");
            MoveToTarget(targetPos);
        }
        else if (inBag)
        {
            // 在书包内，直接更新状态，不移动
            BagPackingManager.Instance.CheckItemPlacement(this);
        }
        else
        {
            // 无效区域：回到拖拽前位置（带动画）
            Debug.Log("物品回到原位置");
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

    // 供管理器调用的初始位置设置（可选，用于初始吸附外部物品到槽位）
    public void SetOriginalPosition(Vector3 pos)
    {
        originalPosition = pos;
        transform.position = pos;
    }
}