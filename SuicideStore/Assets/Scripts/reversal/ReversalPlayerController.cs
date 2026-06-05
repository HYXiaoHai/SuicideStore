using UnityEngine;

public class ReversalPlayerController : MonoBehaviour
{
    public GameObject Player;

    [Header("角色移动")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 40f;
    public float velPower = 1.2f;
    public float frictionAmount = 15f;

    [Header("移动限制")]
    public bool canMoveRight = true;   // 是否允许向右移动

    [Header("角色跳跃")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;   // ���� 0.3~0.7
    public float fallGravityMultiplier = 2f;
    public float gravityScale = 1f;
    public float lastJumpTime;               // ��¼���һ����Ծ��ʱ��

    [Header("音效")]
    public AudioClip[] walkAudioClips;
    //public AudioClip jumpAudioClip;
    private float walkSoundInterval = 0.3f;
    private float walkSoundTimer = 0f;
    public AudioClip landAudioClip;   // 落地音效
    private bool wasGrounded = false; // 上一帧地面状态

    [Header("地面检测")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("动画")]
    public Animator playerAnimitor;
    private bool isFalling = false;
    [Header("金币")]
    public CoinManage coinChainManager;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private int currentDirection = 1;
    private bool canControl = true;

    private float moveInput;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool isJumping = false;
    private bool isGrounded = false;

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
    public void SetCanMoveRight(bool canMove)
    {
        canMoveRight = canMove;
    }
    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!canControl) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        UpdateGroundDetection();
        //移动动画
        playerAnimitor.SetBool("isMoving", Mathf.Abs(moveInput) > 0);

        //走路音效
        if (isGrounded && Mathf.Abs(moveInput) > 0)
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
        }

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

        // 落地音效检测
        if (!wasGrounded && isGrounded)
        {
            if (landAudioClip != null)
            {
                AudioManager.Instance.Play2DSound(landAudioClip, 0.6f); // 音量可调整
            }
        }
        wasGrounded = isGrounded;

        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    private void UpdateGroundDetection()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        playerAnimitor.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            isJumping = false;
            isFalling = false;          // 落地重置
            playerAnimitor.SetBool("isFalling", false);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void OnJump()
    {
        //动画切换
        playerAnimitor.SetTrigger("JumpTrigger");
        Debug.Log("跳跃");
        //施加力
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;          // ��¼��Ծʱ��
        jumpBufferTimer = 0;
        coyoteTimer = 0;

        // 重置下降标志（动画器会等待 isFalling 变为 true）
        playerAnimitor.SetBool("isFalling", false);
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
        playerAnimitor.SetTrigger("JumpTrigger");
        // 重置下降标志（动画器会等待 isFalling 变为 true）
        playerAnimitor.SetBool("isFalling", false);
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
            Debug.Log("下落");
            rb.gravityScale = gravityScale * fallGravityMultiplier;
            isFalling = true;
            playerAnimitor.SetBool("isFalling", true);
        }
        else
        {
            rb.gravityScale = gravityScale;
            isFalling = false;
            playerAnimitor.SetBool("isFalling", false);
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
        float rawInput = moveInput;
        // 如果不允许向右移动且当前输入是向右，则强制置零
        if (!canMoveRight && rawInput > 0)
            rawInput = 0;
        float targetSpeed = rawInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && Mathf.Abs(rawInput) < 0.01f)
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