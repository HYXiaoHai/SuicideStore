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

    [Header("平台跳跃设置")]
    public KeyCode jumpDownKey = KeyCode.S;
    public float jumpCooldown = 0.5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canJump;
    private float lastJumpTime = 0f;
    private PlatformManager platformManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        platformManager = FindObjectOfType<PlatformManager>();
    }

    void Update()
    {
        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        canJump = isGrounded;

        // 水平移动
        float moveX = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 垂直移动（跳跃）
        if (Input.GetButtonDown("Jump") && canJump)
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            canJump = false;
        }

        // 平台跳跃（S键）
        if (Input.GetKeyDown(jumpDownKey))
        {
            TryJumpDown();
        }
    }

    void TryJumpDown()
    {
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            return;
        }

        if (platformManager == null)
        {
            return;
        }

        bool jumped = platformManager.TryJumpToLowerPlatform();
        if (jumped)
        {
            lastJumpTime = Time.time;
        }
    }
}
