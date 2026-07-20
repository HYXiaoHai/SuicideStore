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
    private Vector3 originalPosition;
    private bool isMoving = false;
    private Vector3 originalScale;
    private Tween scaleTween;

    [Header("拖拽手感")]
    public float hoverScale = 1.2f;
    public float scaleDuration = 0.1f;
    private bool isDragging = false;
    private Tween currentMoveTween;
    [Header("悬浮晃动")]
    public float hoverShakeAngle = 3f;
    public float hoverShakeDuration = 0.2f;
    private Tween currentShakeTween;

    [Header("音效")]
    public AudioClip clickClip;
    public AudioClip checkClip;
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

    void OnMouseEnter()
    {
        if (GameManage.Instance.isSetting) return;
        if (!PathPuzzleManage.Instance.isGameStarted) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
        if (isMoving) return;
        ShakeOnce();
    }

    void OnMouseExit()
    {
        if (GameManage.Instance.isSetting) return;
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
            transform.localRotation = Quaternion.identity;
        }
    }

    void OnMouseDown()
    {
        if (GameManage.Instance.isSetting) return;
        if (!PathPuzzleManage.Instance.isGameStarted || isMoving) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        AudioManager.Instance.Play2DSound(clickClip, 0.8f);

        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
            transform.localRotation = Quaternion.identity;
        }

        spriteRenderer.sortingOrder = 3;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        originalPosition = transform.position;
        offset = transform.position - GetMouseWorldPos();
        isDragging = true;
    }

    void OnMouseDrag()
    {
        if (GameManage.Instance.isSetting) return;
        if (isMoving) return;
        if (!PathPuzzleManage.Instance.isGameStarted) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;

        transform.position = GetMouseWorldPos() + offset;
        spriteRenderer.sortingOrder = 3;
        SetZOffset(zOffset / 2f);

        // 拖拽过程中检测目标
        PathPuzzle targetPiece = GetTargetPieceUnderMouse();
        if (targetPiece != null && targetPiece != this)
        {
            int targetIdx = PathPuzzleManage.Instance.GetCurrentIndex(targetPiece);
            if (targetIdx != -1)
            {
                bool insertAfter = IsMouseOnRightHalf(targetPiece);
                PathPuzzleManage.Instance.RequestInsert(this, targetIdx, insertAfter, true);
                return;
            }
        }
    }

    void OnMouseUp()
    {
        if (GameManage.Instance.isSetting)
        {
            if (isDragging) ForceRelease();
            return;
        }

        spriteRenderer.sortingOrder = 2;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        if (!PathPuzzleManage.Instance.isGameStarted) return;
        if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;

        // 1. 检测其他拼图
        PathPuzzle targetPiece = GetTargetPieceUnderMouse();
        if (targetPiece != null && targetPiece != this)
        {
            int targetIdx = PathPuzzleManage.Instance.GetCurrentIndex(targetPiece);
            if (targetIdx != -1)
            {
                Debug.Log("检测其他拼图");
                bool insertAfter = IsMouseOnRightHalf(targetPiece);
                PathPuzzleManage.Instance.RequestInsert(this, targetIdx, insertAfter, false);
                AudioManager.Instance.Play2DSound(checkClip, 0.8f);
                return;
            }
        }

        // 2. 检测空白插槽
        int slotIndex = PathPuzzleManage.Instance.GetSlotIndexOfPosition(GetMouseWorldPos());
        if (slotIndex != -1)
        {
            Debug.Log("检测空白插槽");
            PathPuzzleManage.Instance.RequestInsert(this, slotIndex, false, false);
            AudioManager.Instance.Play2DSound(checkClip, 0.8f);
            return;
        }

        // 3. 无效区域：吸附到当前插槽
        Debug.Log("无效区域");
        PathPuzzleManage.Instance.SnapPieceToSlot(this);
        isDragging = false;
    }

    private void ForceRelease()
    {
        scaleTween?.Kill();
        spriteRenderer.sortingOrder = 2;
        transform.localScale = originalScale;
        PathPuzzleManage.Instance.SnapPieceToSlot(this);
        isDragging = false;
    }

    private void ShakeOnce()
    {
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
            transform.localRotation = Quaternion.identity;
        }
        currentShakeTween = transform.DOPunchRotation(
            new Vector3(0, 0, hoverShakeAngle),
            hoverShakeDuration,
            vibrato: 2,
            elasticity: 0
        ).SetEase(Ease.OutQuad);
    }

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
        // 如果已有移动动画，先停止
        if (currentMoveTween != null && currentMoveTween.IsActive())
            currentMoveTween.Kill();

        isMoving = true;
        targetPos.z = zOffset;

        // 创建动画并保存引用
        currentMoveTween = transform.DOMove(targetPos, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                SetZOffset(zOffset);
                isMoving = false;
                currentMoveTween = null;
            });

        return currentMoveTween;
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
    public void StopAllTweens()
    {
        if (currentMoveTween != null && currentMoveTween.IsActive())
            currentMoveTween.Kill();
        isMoving = false;
    }

    //public int id;
    //public int trueIndex;
    //public int currentIndex;

    //public SpriteRenderer spriteRenderer;

    //private Vector3 offset;
    //private Camera mainCamera;
    //private Rigidbody2D rb;
    //private Collider2D itemCollider;

    //private Vector3 originalPosition;   // 仅用于拖拽开始时记录，无效归位不再使用此值
    //private bool isMoving = false;
    //private Vector3 originalScale;
    //private Tween scaleTween;

    //[Header("拖拽手感")]
    //public float hoverScale = 1.2f;
    //public float scaleDuration = 0.1f;
    //private bool isDragging = false;
    //[Header("悬浮晃动")]
    //public float hoverShakeAngle = 3f;      // 晃动幅度（角度）
    //public float hoverShakeDuration = 0.2f; // 晃动持续时间
    //private Tween currentShakeTween;

    //[Header("音效")]
    //public AudioClip clickClip;
    //public AudioClip checkClip;
    //private readonly float zOffset = -0.04f;

    //void Start()
    //{
    //    mainCamera = Camera.main;
    //    rb = GetComponent<Rigidbody2D>();
    //    spriteRenderer = GetComponent<SpriteRenderer>();
    //    itemCollider = GetComponent<Collider2D>();
    //    originalPosition = transform.position;
    //    originalScale = transform.localScale;
    //    trueIndex = id;
    //    SetZOffset(zOffset);
    //}

    //private void SetZOffset(float z)
    //{
    //    Vector3 pos = transform.position;
    //    pos.z = z;
    //    transform.position = pos;
    //}
    //void OnMouseEnter()
    //{
    //    // 只有在游戏已开始、视图打开、没有正在移动/拖拽时才触发晃动
    //    if (GameManage.Instance.isSetting) return;
    //    if (!PathPuzzleManage.Instance.isGameStarted) return;
    //    if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
    //    if (isMoving) return;

    //    ShakeOnce();
    //}

    //void OnMouseExit()
    //{
    //    // 鼠标离开时，立即停止并归零旋转，避免残留
    //    if (GameManage.Instance.isSetting) return;
    //    if (currentShakeTween != null && currentShakeTween.IsActive())
    //    {
    //        currentShakeTween.Kill();
    //        transform.localRotation = Quaternion.identity;
    //    }
    //}

    //void OnMouseDown()
    //{
    //    if (GameManage.Instance.isSetting) return;
    //    if (!PathPuzzleManage.Instance.isGameStarted || isMoving) return;
    //    if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
    //    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

    //    AudioManager.Instance.Play2DSound(clickClip, 0.8f);

    //    // 停止任何正在晃动的动画，并强制归零
    //    if (currentShakeTween != null && currentShakeTween.IsActive())
    //    {
    //        currentShakeTween.Kill();
    //        transform.localRotation = Quaternion.identity;
    //    }

    //    spriteRenderer.sortingOrder = 3;

    //    scaleTween?.Kill();
    //    scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

    //    originalPosition = transform.position;   // 仅在拖拽开始时记录，后续无效归位不再使用
    //    offset = transform.position - GetMouseWorldPos();
    //    isDragging = true;
    //}

    //void OnMouseDrag()
    //{
    //    if (GameManage.Instance.isSetting) return;
    //    if (isMoving) return;
    //    if (!PathPuzzleManage.Instance.isGameStarted) return;
    //    if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;
    //    transform.position = GetMouseWorldPos() + offset;
    //    spriteRenderer.sortingOrder = 3;
    //    SetZOffset(zOffset/2f);
    //}

    //void OnMouseUp()
    //{
    //    if (GameManage.Instance.isSetting)
    //    {
    //        // 如果暂停期间抬起鼠标，我们不做任何处理，但需要重置拖拽标志，否则恢复后会卡住
    //        if (isDragging)
    //        {
    //            ForceRelease();
    //        }
    //        return;
    //    }

    //    spriteRenderer.sortingOrder = 2;
    //    scaleTween?.Kill();
    //    scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

    //    if (isMoving) return;
    //    if (!PathPuzzleManage.Instance.isGameStarted) return;
    //    if (!Scene9Maneg.Instance.isPuzzleViewOpen) return;

    //    // 1. 优先检测鼠标下方的其他拼图（实现插入到目标拼图的前/后）
    //    PathPuzzle targetPiece = GetTargetPieceUnderMouse();
    //    if (targetPiece != null && targetPiece != this)
    //    {
    //        Debug.Log("检测其他拼图");
    //        int targetIdx = PathPuzzleManage.Instance.GetCurrentIndex(targetPiece);
    //        if (targetIdx != -1)
    //        {
    //            bool insertAfter = IsMouseOnRightHalf(targetPiece);
    //            Debug.Log("是否在右" + insertAfter);
    //            PathPuzzleManage.Instance.RequestInsert(this, targetIdx, insertAfter);
    //            AudioManager.Instance.Play2DSound(checkClip, 0.8f);
    //            return;
    //        }
    //    }

    //    // 2. 如果没有拼图，再检测插槽（拖拽到空白插槽）
    //    int slotIndex = PathPuzzleManage.Instance.GetSlotIndexOfPosition(GetMouseWorldPos());
    //    if (slotIndex != -1)
    //    {
    //        Debug.Log("检测插槽");
    //        AudioManager.Instance.Play2DSound(checkClip, 0.8f);
    //        PathPuzzleManage.Instance.RequestInsert(this, slotIndex, false);
    //        return;
    //    }

    //    // 3. 完全无效区域：吸附到当前正确插槽中心
    //    PathPuzzleManage.Instance.SnapPieceToSlot(this);

    //    isDragging = false;
    //}
    //private void ForceRelease()
    //{
    //    // 停止拖拽动画
    //    scaleTween?.Kill();
    //    spriteRenderer.sortingOrder = 2;
    //    transform.localScale = originalScale;
    //    // 归位到正确插槽
    //    PathPuzzleManage.Instance.SnapPieceToSlot(this);
    //    isDragging = false;
    //}
    ////晃动效果
    //private void ShakeOnce()
    //{
    //    // 取消之前的晃动动画
    //    if (currentShakeTween != null && currentShakeTween.IsActive())
    //    {
    //        currentShakeTween.Kill();
    //        transform.localRotation = Quaternion.identity;
    //    }

    //    // 使用 PunchRotation 实现快速晃动并自动归位
    //    currentShakeTween = transform.DOPunchRotation(
    //        new Vector3(0, 0, hoverShakeAngle),
    //        hoverShakeDuration,
    //        vibrato: 2,      // 晃动次数
    //        elasticity: 0    // 不弹性回弹，直接归零
    //    ).SetEase(Ease.OutQuad);
    //}
    ///// <summary>判断鼠标是否位于目标拼图的右半部分（世界坐标系）</summary>
    //private bool IsMouseOnRightHalf(PathPuzzle target)
    //{
    //    Vector3 mouseWorld = GetMouseWorldPos();
    //    Vector3 targetPos = target.transform.position;
    //    return mouseWorld.x > targetPos.x;
    //}

    //private PathPuzzle GetTargetPieceUnderMouse()
    //{
    //    Vector3 mouseWorldPos = GetMouseWorldPos();
    //    Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
    //    if (hit != null)
    //    {
    //        PathPuzzle piece = hit.GetComponent<PathPuzzle>();
    //        if (piece != null && piece != this)
    //            return piece;
    //    }
    //    return null;
    //}

    //public Tween MoveToPosition(Vector3 targetPos, float duration)
    //{
    //    if (isMoving) DOTween.Kill(transform);
    //    isMoving = true;
    //    targetPos.z = zOffset;
    //    return transform.DOMove(targetPos, duration)
    //        .SetEase(Ease.OutQuad)
    //        .OnComplete(() =>
    //        {
    //            SetZOffset(zOffset);
    //            isMoving = false;
    //        });
    //}

    //public void SetOriginalPosition(Vector3 pos)
    //{
    //    pos.z = zOffset;
    //    originalPosition = pos;
    //    transform.position = pos;
    //    isMoving = false;
    //}

    //private Vector3 GetMouseWorldPos()
    //{
    //    Vector3 mousePoint = Input.mousePosition;
    //    mousePoint.z = -mainCamera.transform.position.z;
    //    return mainCamera.ScreenToWorldPoint(mousePoint);
    //}
}