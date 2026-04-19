using UnityEngine;

public class ReversalPlayerController : MonoBehaviour
{
    public GameObject Player;

    [Header("�ƶ�����")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 40f;
    public float velPower = 1.2f;
    public float frictionAmount = 15f;

    [Header("��Ծ����")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;   // ���� 0.3~0.7
    public float fallGravityMultiplier = 2f;
    public float gravityScale = 1f;
    public float lastJumpTime;               // ��¼���һ����Ծ��ʱ��

    [Header("������")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("����")]
    public CoinManage coinChainManager;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int currentDirection = 1;
    private bool canControl = true;

    private float moveInput;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool isJumping = false;

    void Start()
    {
        spriteRenderer = Player.GetComponent<SpriteRenderer>();
        rb = Player.GetComponent<Rigidbody2D>();

        if (rb == null)
            Debug.LogError("��Ҫ Rigidbody2D �����");
        if (groundCheck == null)
            Debug.LogWarning("��ָ�� groundCheck (��ҽŵ׵Ŀ�����)");

        rb.gravityScale = gravityScale;
    }

    public void SetCanMove(bool canMove)
    {
        canControl = canMove;
        if (!canMove && rb != null)
            rb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (!canControl) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        //������
        UpdateGroundDetection();

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            OnJump();
        }
        HandleJumpUp();
        UpdateGravity();
        HandleFlip();

        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    private void UpdateGroundDetection()
    {
        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            isJumping = false;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void OnJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;          // ��¼��Ծʱ��
        jumpBufferTimer = 0;
        coyoteTimer = 0;
    }

    private void HandleJumpUp()
    {
        if (Input.GetButtonUp("Jump") && isJumping && rb.velocity.y > 0)
        {
            OnJumpUp();
        }
    }

    private void OnJumpUp()
    {
        if (rb.velocity.y > 0 && isJumping)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }
        lastJumpTime = 0;
        isJumping = false;
    }

    private void UpdateGravity()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void HandleFlip()
    {
        if (moveInput != 0)
        {
            int newDir = moveInput > 0 ? 1 : -1;
            if (newDir != currentDirection)
            {
                currentDirection = newDir;
                FlipPlayer();
                if (coinChainManager != null)
                    coinChainManager.UpdateDirection(currentDirection);
            }
        }
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

    private void FixedUpdate()
    {
        if (!canControl) return;

        float targetSpeed = moveInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && Mathf.Abs(moveInput) < 0.01f)
        {
            float frictionForce = Mathf.Min(Mathf.Abs(rb.velocity.x), frictionAmount);
            frictionForce *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -frictionForce, ForceMode2D.Impulse);
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