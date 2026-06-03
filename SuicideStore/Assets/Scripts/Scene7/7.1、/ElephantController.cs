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
    [Header("音效")]
    public AudioClip[] walkAudioClips;
    private float walkSoundInterval = 0.3f;
    private float walkSoundTimer = 0f;

    private Vector2 moveDirection = Vector2.zero;
    private bool isMoving = false;
    public bool canMove = false;
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!canMove) return;

        // 计算移动方向
        moveDirection = Vector2.zero;
        if (Input.GetKey(moveRightKey))
        {
            moveDirection += slopeUpRight;

        }
        if (Input.GetKey(moveLeftKey))
        {
            moveDirection -= slopeUpRight;
        }

        isMoving = moveDirection != Vector2.zero;

        if (isMoving)
        {
            walkSoundTimer -= Time.deltaTime;
            if (walkSoundTimer <= 0f)
            {
                // 从数组随机选择一个移动音效
                if (walkAudioClips != null && walkAudioClips.Length > 0)
                {
                    AudioClip randomClip = walkAudioClips[Random.Range(0, walkAudioClips.Length)];
                    AudioManager.Instance.Play2DSound(randomClip, 0.5f);
                }
                walkSoundTimer = walkSoundInterval;
            }


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
        FlipCharacter(moveDirection.x);
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator != null)
            animator.SetBool("isWalking", isMoving);
    }
    private void FlipCharacter(float horizontal)
    {
        if (horizontal > 0) // 向右移动
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontal < 0) // 向左移动
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        // horizontal == 0 时不翻转，保持上次方向（可选）
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