using UnityEngine;

public class ElephantController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public KeyCode moveRightKey = KeyCode.D;
    public KeyCode moveLeftKey = KeyCode.A;

    [Header("斜坡方向")]
    public Vector2 slopeUpRight = new Vector2(1f, 1f).normalized;

    [Header("边界限制")]
    public bool useBounds = true;
    public Vector2 minBounds;   // 左下边界 (x, y)
    public Vector2 maxBounds;   // 右上边界 (x, y)

    [Header("引用")]
    public Animator animator;
    public Scene7DialogueManager dialogueManager;

    private Vector2 moveDirection = Vector2.zero;
    private bool isMoving = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<Scene7DialogueManager>();
    }

    void Update()
    {
        // 计算移动方向
        moveDirection = Vector2.zero;
        if (Input.GetKey(moveRightKey))
            moveDirection += slopeUpRight;
        if (Input.GetKey(moveLeftKey))
            moveDirection -= slopeUpRight;

        isMoving = moveDirection != Vector2.zero;

        if (isMoving)
        {
            moveDirection.Normalize();
            Vector3 newPos = transform.position + (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

            // 应用边界限制
            if (useBounds)
            {
                newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
            }

            transform.position = newPos;
        }

        UpdateAnimation();

        if (dialogueManager != null)
            dialogueManager.CheckTriggerPosition(transform.position);
    }

    void UpdateAnimation()
    {
        if (animator != null)
            animator.SetBool("isWalking", isMoving);
    }

    // 显示移动方向和边界框（Scene视图）
    private void OnDrawGizmos()
    {
        // 绘制移动方向
        if (slopeUpRight != Vector2.zero)
        {
            Gizmos.color = Color.green;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + (Vector3)slopeUpRight * 20f;
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireSphere(startPos, 0.1f);
        }

        // 绘制边界框
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}