using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    [Header("物品ID")]
    public int itemId = -1;  // -1 表示不触发UI逻辑

    [Header("拖拽缩放")]
    public float hoverScale = 1.2f;
    public float scaleDuration = 0.1f;

    [Header("音效")]
    public AudioClip dragClip;

    //引用包内对应图片（由点击时设置）
    public SpriteRenderer bagItemSprite;

    private Vector3 offset;
    private Camera mainCamera;
    private Collider2D itemCollider;

    private Vector3 originalPosition;
    private Vector3 startPosition;
    private bool isMoving = false;
    public Vector3 originalScale;
    private Tween scaleTween;
    private readonly float zOffset = -0.04f;
    private Vector3 startDragPosition; //记录拖拽开始时的位置
    // 状态标记
    public bool isInsideBag;
    public bool isInExternalArea;   // 是否在合法外部区域
    public bool initialInside;      // 是否初始在包内
    public bool isProcessed = false;

    // 程序化拖拽标志
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        itemCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        originalPosition = startPosition;
        originalScale = transform.localScale;
        SetZOffset();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (GameManage.Instance.isSetting)
        {
            if (isDragging) ForceReset();
            return;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            // 跟随鼠标移动
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
            SetZOffset();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }
    }
    void OnMouseDown()
    {
        if (GameManage.Instance.isSetting) return;
        if (BagPackingManager.Instance == null || !BagPackingManager.Instance.isGameStarted || BagPackingManager.Instance.gameCompleted) return;
        if (isDragging) return;
        if (itemId >= 0) return; // 正ID不允许外部点击

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        StartDrag(mousePos, false);
    }
    private void SetZOffset()
    {
        Vector3 pos = transform.position;
        pos.z = zOffset;
        transform.position = pos;
    }

    // 程序化开始拖拽（由点击包内图片触发）
    public void StartDrag(Vector3 mouseWorldPos, bool fromBag = true)
    {
        if (isDragging) return;
        isDragging = true;
        offset = transform.position - mouseWorldPos;
        initialInside = fromBag;
        startDragPosition = transform.position; // 记录拖拽开始位置

        // 放大效果
        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale * hoverScale, scaleDuration).SetEase(Ease.OutBack);

        // 播放音效
        if (dragClip != null)
            AudioManager.Instance.Play2DSound(dragClip, 0.8f);

        // 更新状态（初始位置在包内？不，此时是包外物体）
        // 由点击产生，视为从包内拿出
        UpdateStatus();
    }

    private void EndDrag()
    {
        if (!isDragging) return;
        isDragging = false;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(originalScale, scaleDuration).SetEase(Ease.OutQuad);

        bool inBag = BagPackingManager.Instance.IsInBagArea(transform.position);
        bool inExternal = BagPackingManager.Instance.IsInExternalArea(transform.position);

        if (inBag&& !initialInside)
        {
            // 放入书包：隐藏外部物体，恢复包内图片
            Debug.Log("松开 放入书包");
            gameObject.SetActive(false);
            if (bagItemSprite != null)
            {
                bagItemSprite.gameObject.SetActive(true);
                bagItemSprite.DOKill();
                bagItemSprite.DOFade(1f, 0.2f);
            }
            BagPackingManager.Instance.OnItemReturnToBag(this);
        }
        else if (inExternal&& initialInside)
        {
            // 放到外部区域：保持外部物体显示（位置固定）
            transform.position = new Vector3(transform.position.x, transform.position.y, zOffset);
            BagPackingManager.Instance.OnItemPlacedOutside(this);
        }
        else
        {
            // 无效区域
            if (isInsideBag) //从书包拿出来的
            {
                // 回到书包
                gameObject.SetActive(false);
                Debug.Log("松开 无效区域回到书包");
                if (bagItemSprite != null)
                {
                    bagItemSprite.gameObject.SetActive(true);
                    bagItemSprite.DOKill();
                    bagItemSprite.DOFade(1f, 0.2f);
                }
            }
            else //从外部拖拽的（例如负ID物品从外部拖拽）
            {
                transform.position = startDragPosition;
            }
        }

        isDragging = false;
    }


    private void UpdateStatus()
    {
        // 更新当前状态（用于管理器）
        isInsideBag = BagPackingManager.Instance.IsInBagArea(transform.position);
        isInExternalArea = BagPackingManager.Instance.IsInExternalArea(transform.position);
    }

    // 强制复位（暂停时调用）
    private void ForceReset()
    {
        if (!isDragging) return;
        isDragging = false;

        transform.DOKill();
        scaleTween?.Kill();

        if (initialInside) // 从书包拿出来的（正ID或负ID初始在包内）
        {
            gameObject.SetActive(false);
            if (bagItemSprite != null)
            {
                bagItemSprite.gameObject.SetActive(true);
                bagItemSprite.DOKill();
                bagItemSprite.DOFade(1f, 0.2f);
            }
        }
        else // 从外部拖拽的（负ID物品初始在外部）
        {
            transform.position = startDragPosition;
            SetZOffset();
            transform.localScale = originalScale;
        }
    }
}