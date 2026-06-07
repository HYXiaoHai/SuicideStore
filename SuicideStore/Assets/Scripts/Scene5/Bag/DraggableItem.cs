using DG.Tweening;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("绑定对应UI图片")]
    public UnityEngine.UI.Image itemUI;

    [Header("时间配置")]
    public float showUIDelay = 0.5f;    // 拖出书包后，延迟多久弹出UI
    public float hideDelay = 0.8f;       // UI显示后，延迟多久一起消失
    public float fadeDuration = 0.4f;    // 淡出动画时长

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

    private bool hasProcessed = false; // 标记该物品是否已经执行过弹出+消失逻辑

    void Start()
    {
        mainCamera = Camera.main;
        itemCollider = GetComponent<Collider2D>();

        originalPosition = transform.position;
        originalScale = transform.localScale;
        SetZOffset();

        // 初始化UI：隐藏+透明
        if (itemUI != null)
        {
            itemUI.gameObject.SetActive(false);
            Color c = itemUI.color;
            c.a = 0f;
            itemUI.color = c;
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
        if (isMoving || hasProcessed) return;
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
        if (isMoving || hasProcessed) return;
        if (BagPackingManager.Instance == null ||
            !BagPackingManager.Instance.isGameStarted ||
            BagPackingManager.Instance.gameCompleted)
            return;

        transform.position = GetMouseWorldPos() + offset;
        SetZOffset();
    }

    void OnMouseUp()
    {
        if (isMoving || hasProcessed) return;

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

                // 核心判断：仅【初始在包里】的物品 + 现在不在包里 + 未执行过逻辑
                if (initialInside && !BagPackingManager.Instance.IsInBagArea(transform.position) && !hasProcessed)
                {
                    OutBagProcess();
                }

                BagPackingManager.Instance.CheckItemPlacement(this);
            });
    }

    /// <summary>
    /// 物品拖出书包后的流程：延迟弹UI → 再延迟一起消失
    /// </summary>
    private void OutBagProcess()
    {
        hasProcessed = true;

        DOTween.Sequence()
            // 1. 拖出后等待反应时间
            .AppendInterval(showUIDelay)
            // 2. 弹出UI并淡入
            .AppendCallback(() =>
            {
                if (itemUI != null)
                {
                    itemUI.gameObject.SetActive(true);
                    itemUI.DOFade(1f, fadeDuration);
                }
            })
            // 3. UI显示后再等待反应时间
            .AppendInterval(hideDelay)
            // 4. 物品+UI 同步淡出
            .Append(transform.DOFade(0f, fadeDuration))
            .Join(itemUI?.DOFade(0f, fadeDuration))
            // 5. 动画结束销毁
            .OnComplete(() =>
            {
                BagPackingManager.Instance.allItems.Remove(this);
                Destroy(gameObject);
                if (itemUI != null)
                    Destroy(itemUI.gameObject);
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