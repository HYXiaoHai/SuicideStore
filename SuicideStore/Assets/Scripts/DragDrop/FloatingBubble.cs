using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FloatingBubble : MonoBehaviour, IPointerClickHandler
{
    [Header("漂浮设置")]
    [SerializeField] private float baseSpeed = 1.5f;
    [SerializeField] private string boundaryTag = "Boundary";

    private RectTransform rectTransform;
    private Rigidbody2D rb;
    private CanvasGroup canvasGroup;
    private Vector2 moveDirection;
    private bool isFloating = true;
    public bool isSpecial = false;
    private float targetScale = 1f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rb = GetComponent<Rigidbody2D>();
        canvasGroup = GetComponent<CanvasGroup>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 初始透明且缩放为0
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public void PlayAppearAnimation()
    {
        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1f, 1f));
        seq.OnComplete(() => StartFloating());
        seq.Play();
    }

    private void StartFloating()
    {
        ResetDirection();
        int defendNum = DefendManage.Instance?.defendNum ?? 0;
        float speed = baseSpeed * (1 + defendNum * 0.5f);
        rb.velocity = moveDirection * speed;
    }

    private void ResetDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    private void FixedUpdate()
    {
        if (!isFloating) return;

        if (PuzzleManage.Instance != null)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            var pm = PuzzleManage.Instance;
            bool changed = false;

            if (changed)
            {
                rectTransform.anchoredPosition = pos;
                int defendNum = DefendManage.Instance?.defendNum ?? 0;
                float speed = baseSpeed * (1 + defendNum * 0.5f);
                rb.velocity = moveDirection * speed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(boundaryTag))
        {
            Vector2 normal = collision.contacts[0].normal;
            moveDirection = Vector2.Reflect(moveDirection, normal).normalized;
            int defendNum = DefendManage.Instance?.defendNum ?? 0;
            float speed = baseSpeed * (1 + defendNum * 0.5f);
            rb.velocity = moveDirection * speed;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isFloating) return;
        if (isSpecial) return;
        isFloating = false;
        rb.velocity = Vector2.zero;
        canvasGroup.DOFade(0f, 0.3f).OnComplete(() =>
        {
            if (FloatingBubbleManager.Instance != null)
                FloatingBubbleManager.Instance.OnBubbleDestroyed(isSpecial);
            Destroy(gameObject);
        });
    }

    public void SetSpecial(bool special)
    {
        isSpecial = special;
        targetScale = special ? targetScale : targetScale;
    }

    public void SetInitialPosition(Vector2 anchoredPos)
    {
        rectTransform.anchoredPosition = anchoredPos;
    }
    private void OnDestroy()
    {
        DOTween.Kill(gameObject);
    }
}