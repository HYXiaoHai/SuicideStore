using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

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

    [Header("每轮切换对应的图片")]
    public Transform defaultPosition;

    [Header("动画设置")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationDuration = 0.2f;
    private Vector3 originalScale;

    [Header("漂浮运动设置")]
    [SerializeField] private float baseSpeed = 2f;           // 基础速度（单位/秒）
    [SerializeField] private float speedMultiplier = 0.5f;    // 每增加一次辩解，速度提升乘数
    [SerializeField] private string boundaryTag = "Boundary"; // 边界物体的Tag

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
        Debug.Log(id+"碰到了");
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
        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Slot[] slots = FindObjectsOfType<Slot>();
        foreach (Slot slot in slots)
        {
            if (slot.TrySnap(this)) break;
        }

        if (PuzzleManage.Instance != null)
            PuzzleManage.Instance.CheckAllSlots();

        if (currentSlot == null)
            StartFloating();
    }

    // 插槽交互
    public void OnSnappedToSlot()
    {
        StopFloating();
        // 使用 DOTween 动画旋转回正
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
        if (currentSlot != null)
        {
            return;
        }
        transform.DOKill();
        transform.DOScale(originalScale * scaleMultiplier, animationDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(currentSlot!=null)
        {
            return;
        }
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration);
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