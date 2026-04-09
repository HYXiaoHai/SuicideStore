using UnityEngine;

public class Move : MonoBehaviour
{
    public GameObject Player;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;          // 跳跃力度
    public LayerMask groundLayer;           // 地面层（用于检测是否在地面）
    public Transform groundCheck;           // 地面检测点（通常放在玩家脚底）
    public float groundCheckRadius = 0.2f;  // 检测半径

    public CoinManage coinChainManager;     // 金币管理器（注意类名与你代码中一致）

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int currentDirection = 1;       // 1: 右, -1: 左
    private bool isGrounded;

    void Start()
    {
        // 获取组件
        spriteRenderer = Player.GetComponent<SpriteRenderer>();
        rb = Player.GetComponent<Rigidbody2D>();

        if (rb == null)
            Debug.LogError("Player 需要添加 Rigidbody2D 组件！");
        if (spriteRenderer == null)
            Debug.LogWarning("Player 没有 SpriteRenderer，无法翻转");
        if (groundCheck == null)
            Debug.LogWarning("请为 groundCheck 赋值一个 Transform（玩家脚底的空物体）");
    }

    void Update()
    {
        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 水平输入
        float h = Input.GetAxis("Horizontal");

        // 跳跃输入（只有在地面时才能跳跃）
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 处理方向翻转（左右翻转）
        if (h != 0)
        {
            int newDir = h > 0 ? 1 : -1;
            if (newDir != currentDirection)
            {
                currentDirection = newDir;
                FlipPlayer();
                if (coinChainManager != null)
                    coinChainManager.UpdateDirection(currentDirection);
            }
        }
    }

    private void FixedUpdate()
    {
        // 水平移动（使用物理速度，保持平滑）
        float h = Input.GetAxis("Horizontal");
        Vector2 targetVelocity = new Vector2(h * moveSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;
    }

    private void FlipPlayer()
    {
        if (spriteRenderer != null)
        {
            Vector3 scale = Player.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * currentDirection;
            Player.transform.localScale = scale;
        }
    }

    // 可选：在 Scene 视图中可视化地面检测半径
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}