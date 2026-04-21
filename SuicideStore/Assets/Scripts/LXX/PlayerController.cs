using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("重生设置")]
    public Transform spawnPoint;
    public float groundY = -10f;

    [Header("自由落体设置")]
    public KeyCode fallDownKey = KeyCode.S;
    public float fallDuration = 0.5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canJump;
    private bool isFalling = false;
    private float fallTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spawnPoint == null)
        {
            GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
            if (spawn != null)
            {
                spawnPoint = spawn.transform;
            }
            else
            {
                spawnPoint = new GameObject("SpawnPoint").transform;
                spawnPoint.position = transform.position;
                spawnPoint.gameObject.tag = "SpawnPoint";
            }
        }
    }

    void Update()
    {
        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        canJump = isGrounded;

        // 检查是否掉落到底部
        if (transform.position.y < groundY)
        {
            Respawn();
        }

        // 水平移动
        float moveX = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 垂直移动（跳跃）
        if (Input.GetButtonDown("Jump") && canJump)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            canJump = false;
        }

        // 自由落体（S键）
        if (Input.GetKeyDown(fallDownKey) && !isFalling)
        {
            StartFalling();
        }

        // 处理自由落体
        if (isFalling)
        {
            HandleFalling();
        }
    }

    void StartFalling()
    {
        isFalling = true;
        fallTimer = 0f;
        
        // 暂时禁用碰撞（可选）
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    void HandleFalling()
    {
        fallTimer += Time.deltaTime;
        
        if (fallTimer >= fallDuration)
        {
            StopFalling();
        }
    }

    void StopFalling()
    {
        isFalling = false;
        
        // 重新启用碰撞
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
    }

    void Respawn()
    {
        transform.position = spawnPoint.position;
        transform.rotation = Quaternion.identity;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
