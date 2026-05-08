using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PathPuzzle : MonoBehaviour
{
    public int id;
    public int trueIndex;
    public int currentIndex;

    public SpriteRenderer spriteRenderer;

    private Vector3 offset;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Collider2D itemCollider;

    private Vector3 originalPosition;   // 仅用于拖拽开始时记录，无效归位不再使用此值
    private bool isMoving = false;
    private Vector3 originalScale;
    private Tween scaleTween;

    [Header("拖拽手感")]
    public float hoverScale = 1.2f;
    public float scaleDuration = 0.1f;

    private readonly float zOffset = -0.04f;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        originalScale = transform.localScale;
        trueIndex = id;
        SetZOffset(zOffset);
    }

    private void SetZOffset(float z)
    {
        Vector3 pos = transform.position;
        pos.z = z;
        transform.position = pos;
    }

    void OnMouseDown()
    {
        if (!PathPuzzleManage.Instance.isGameStarted || isMoving) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        spriteRenderer.sortingOrder = 3;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        originalPosition = transform.position;   // 仅在拖拽开始时记录，后续无效归位不再使用
        offset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (isMoving) return;
        if (!PathPuzzleManage.Instance.isGameStarted) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
        transform.position = GetMouseWorldPos() + offset;
        spriteRenderer.sortingOrder = 3;
        SetZOffset(zOffset/2f);
    }

    void OnMouseUp()
    {
        spriteRenderer.sortingOrder = 2;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        if (isMoving) return;
        if (!PathPuzzleManage.Instance.isGameStarted) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;

        // 1. 优先检测鼠标下方的其他拼图（实现插入到目标拼图的前/后）
        PathPuzzle targetPiece = GetTargetPieceUnderMouse();
        if (targetPiece != null && targetPiece != this)
        {
            Debug.Log("检测其他拼图");
            int targetIdx = PathPuzzleManage.Instance.GetCurrentIndex(targetPiece);
            if (targetIdx != -1)
            {
                bool insertAfter = IsMouseOnRightHalf(targetPiece);
                Debug.Log("是否在右" + insertAfter);
                PathPuzzleManage.Instance.RequestInsert(this, targetIdx, insertAfter);
                return;
            }
        }

        // 2. 如果没有拼图，再检测插槽（拖拽到空白插槽）
        int slotIndex = PathPuzzleManage.Instance.GetSlotIndexOfPosition(GetMouseWorldPos());
        if (slotIndex != -1)
        {
            Debug.Log("检测插槽");
            PathPuzzleManage.Instance.RequestInsert(this, slotIndex, false);
            return;
        }

        // 3. 完全无效区域：吸附到当前正确插槽中心
        PathPuzzleManage.Instance.SnapPieceToSlot(this);
    }

    /// <summary>判断鼠标是否位于目标拼图的右半部分（世界坐标系）</summary>
    private bool IsMouseOnRightHalf(PathPuzzle target)
    {
        Vector3 mouseWorld = GetMouseWorldPos();
        Vector3 targetPos = target.transform.position;
        return mouseWorld.x > targetPos.x;
    }

    private PathPuzzle GetTargetPieceUnderMouse()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
        if (hit != null)
        {
            PathPuzzle piece = hit.GetComponent<PathPuzzle>();
            if (piece != null && piece != this)
                return piece;
        }
        return null;
    }

    public Tween MoveToPosition(Vector3 targetPos, float duration)
    {
        if (isMoving) DOTween.Kill(transform);
        isMoving = true;
        targetPos.z = zOffset;
        return transform.DOMove(targetPos, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                SetZOffset(zOffset);
                isMoving = false;
            });
    }

    public void SetOriginalPosition(Vector3 pos)
    {
        pos.z = zOffset;
        originalPosition = pos;
        transform.position = pos;
        isMoving = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}