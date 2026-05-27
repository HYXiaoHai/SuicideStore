using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Rigidbody2D), typeof(Collider2D))]
public class Puzzle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int id = 0;
    public Slot currentSlot;
    public RectTransform currentSlotPosition;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Vector2 dragOffset;
    public bool isDragging;

    // 记录拖拽开始时的位置（用于边界外且未放到插槽时的回位）
    private Vector2 preDragAnchoredPos;

    [Header("每轮切换对应的图片")]
    public Transform defaultPosition;

    [Header("动画设置")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationDuration = 0.2f;
    private Vector3 originalScale;
    private Tween scaleTween;

    [Header("漂浮运动设置")]
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float speedMultiplier = 0.5f;
    [SerializeField] private string boundaryTag = "Boundary";

    [Header("音效")]
    public AudioClip clikClip;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Coroutine floatingCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        ResetDirection();
    }

    public void SetRandomRotation()
    {
        float randomZ = Random.Range(-30f, 30f);
        transform.localEulerAngles = new Vector3(0, 0, randomZ);
    }

    private void ResetDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    public void StartFloating()
    {
        if (floatingCoroutine != null) StopCoroutine(floatingCoroutine);
        floatingCoroutine = StartCoroutine(FloatingMovement());
    }

    public void StopFloating()
    {
        if (floatingCoroutine != null)
        {
            StopCoroutine(floatingCoroutine);
            floatingCoroutine = null;
        }
        if (rb != null) rb.velocity = Vector2.zero;
    }

    private IEnumerator FloatingMovement()
    {
        while (true)
        {
            int defendNum = DefendManage.Instance?.defendNum ?? 0;
            float speed = baseSpeed * (1 + defendNum * speedMultiplier);
            rb.velocity = moveDirection * speed;
            yield return new WaitForFixedUpdate();
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(boundaryTag))
        {
            Vector2 normal = collision.contacts[0].normal;
            moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
            if (floatingCoroutine != null)
            {
                int defendNum = DefendManage.Instance?.defendNum ?? 0;
                float speed = baseSpeed * (1 + defendNum * speedMultiplier);
                rb.velocity = moveDirection * speed;
            }
        }
    }

    // 拖拽逻辑
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        StopFloating();
        rb.velocity = Vector2.zero;

        // 记录拖拽开始时的 anchoredPosition
        preDragAnchoredPos = rectTransform.anchoredPosition;

        // 音效
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShortSound(clikClip, 0.8f);

        if (currentSlot != null)
        {
            currentSlot.RemovePuzzle();
            currentSlot = null;
        }

        transform.SetAsLastSibling();

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;
        Vector2 localPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out localPointerPos);
        dragOffset = localPointerPos - rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out localPointerPos);
        Vector2 newPosition = localPointerPos - dragOffset;

        // 移除了原来的边界限制，允许拖拽到任何位置
        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 检查松手时是否在边界内（相对于父级局部坐标）
        bool isInsideBoundary = IsInsideDragBoundary();

        // 尝试放到插槽上
        bool snappedToSlot = false;
        Slot[] slots = FindObjectsOfType<Slot>();
        foreach (Slot slot in slots)
        {
            if (slot.TrySnap(this))
            {
                snappedToSlot = true;
                break;
            }
        }

        // 根据条件处理
        if (snappedToSlot)
        {
            // 成功放到插槽上，插槽逻辑会处理（不会飘动）
            // 无需额外动作
        }
        else if (!isInsideBoundary)
        {
            // 在边界外且未放到插槽：回到拖拽前的位置，然后开始飘动
            Debug.Log("边界外");
            ReturnToPreDragPositionAndFloat();
        }
        else
        {
            Debug.Log("边界内");
            if (currentSlot == null)
                StartFloating();
        }

        // 检查所有插槽状态（全局）
        if (PuzzleManage.Instance != null)
            PuzzleManage.Instance.CheckAllSlots();
    }

    //判断当前拼图位置是否在允许的拖拽边界内
    private bool IsInsideDragBoundary()
    {
        if (PuzzleManage.Instance == null) return true;
        var pm = PuzzleManage.Instance;
        Vector2 pos = rectTransform.anchoredPosition;
        return pos.x >= pm.dragLeft && pos.x <= pm.dragRight &&
               pos.y >= pm.dragBottom && pos.y <= pm.dragTop;
    }

    //平滑回到拖拽前的位置，然后开始飘动
    private void ReturnToPreDragPositionAndFloat()
    {
        StopFloating();
        // 目标位置默认是拖拽前记录的位置
        Vector2 targetPos = preDragAnchoredPos;

        // 强制限制在边界内（防止因记录位置异常导致飞出边界）
        if (PuzzleManage.Instance != null)
        {
            var pm = PuzzleManage.Instance;
            targetPos.x = Mathf.Clamp(targetPos.x, pm.dragLeft, pm.dragRight);
            targetPos.y = Mathf.Clamp(targetPos.y, pm.dragBottom, pm.dragTop);
        }

        Debug.Log("移动前：" + rectTransform.anchoredPosition + " 移动到：" + targetPos);
        rectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            Debug.Log("移动后：" + rectTransform.anchoredPosition);
            if (currentSlot == null && !isDragging)
                StartFloating();
        });
    }
    // 插槽交互
    public void OnSnappedToSlot()
    {
        StopFloating();
        transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack);
        transform.DOScale(originalScale, animationDuration);
    }

    public void OnRemovedFromSlot()
    {
        if (currentSlot == null && !isDragging)
            StartFloating();
    }

    // 悬停动画
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSlot != null) return;
        // 精准终止：如果之前的缩放动画还在，就只杀掉它
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();

        // 播放新动画并保存引用
        scaleTween = transform.DOScale(originalScale * scaleMultiplier, animationDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentSlot != null) return;
        // 精准终止：如果之前的缩放动画还在，就只杀掉它
        if (scaleTween != null && scaleTween.IsActive())
            scaleTween.Kill();

        // 播放新动画并保存引用
        scaleTween = transform.DOScale(originalScale, animationDuration);
    }

    // 重置运动（每轮重置时调用）
    public void ResetMovement()
    {
        GetComponent<Collider2D>().enabled = true;
        ResetDirection();
        if (floatingCoroutine != null && currentSlot == null && !isDragging)
        {
            StopFloating();
            StartFloating();
        }
    }
}