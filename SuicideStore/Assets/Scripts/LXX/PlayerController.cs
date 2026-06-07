using UnityEngine;
//注释的都是旧版的
// 带//++的是新加的
public class PlayerController : MonoBehaviour
{
    //[Header("移动参数")]
    //public float moveSpeed = 5f;
    //public float jumpForce = 8f;

    public GameObject Player;
    [Header("角色移动")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 40f;
    public float velPower = 1.2f;
    public float frictionAmount = 15f;

    [Header("角色跳跃")]
    public float jumpForce = 12f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 2f;
    public float gravityScale = 1f;
    public float lastJumpTime;

    //旧版
    [Header("地面检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("动画")]
    public Animator playerAnimitor;
    private bool isFalling = false;

    [Header("重生设置")]
    public Transform spawnPoint;
    public float groundY = -10f;

    //[Header("自由落体设置")]
    //public KeyCode fallDownKey = KeyCode.S;
    //public float fallDuration = 0.5f;
    //private bool isFalling = false;

    [Header("平台穿透 (S键)")]
    public bool canPenetrate = true;
    public KeyCode penetrateKey = KeyCode.S;        // 触发穿透的按键
    public float penetrateDuration = 0.3f;          // 碰撞体禁用时长（秒）
    public float penetrateCooldown = 0.5f;          // 穿透冷却时间

    [Header("音效")]
    public AudioClip[] walkAudioClips;
    //public AudioClip jumpAudioClip;
    private float walkSoundInterval = 0.3f;
    private float walkSoundTimer = 0f;
    public AudioClip landAudioClip;   // 落地音效
    private bool wasGrounded = false; // 上一帧地面状态

    //private Rigidbody2D rb;
    //private bool isGrounded;
    //private bool canJump;
    //private float fallTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D playerCollider;              //玩家的碰撞体
    private int currentDirection = 1;
    private bool canControl = true;

    private float moveInput;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool isJumping = false;
    public bool isGrounded = true;                //缓存地面状态

    // 穿透状态
    private bool isPenetrating = false;
    private float penetrateTimer = 0f;
    private float lastPenetrateTime = -10f;         //上次穿透时间（用于冷却）

    void Start()
    {
        if(TransitionManage.Instance!=null)
        {
            TransitionManage.Instance.FadeIn(1f,Color.white);
        }

        rb = Player.GetComponent<Rigidbody2D>();
        spriteRenderer = Player.GetComponent<SpriteRenderer>();//++
        playerCollider = Player.GetComponent<Collider2D>();//++
        rb.gravityScale = gravityScale;//++

        //初始化重生点（旧）
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
        wasGrounded = isGrounded;
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;

        // 检查是否掉落到底部  旧
        if (transform.position.y < groundY)
        {
            Respawn();
        }
        if (!canControl) return;

        //水平按键输入
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

        //跳跃输入
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferTimer = jumpBufferTime;
        }
        if (jumpBufferTimer > 0 && coyoteTimer > 0 && !isPenetrating) //穿透时禁止跳跃
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


        //平台穿透 S键
        if (canPenetrate&&Input.GetKeyDown(penetrateKey) && !isPenetrating && isGrounded && Time.time - lastPenetrateTime >= penetrateCooldown)
        {
            StartPenetrate();
        }

        //更新穿透计时
        if (isPenetrating)
        {
            penetrateTimer -= Time.deltaTime;
            if (penetrateTimer <= 0f)
            {
                EndPenetrate();
            }
        }

        if (jumpBufferTimer > 0)
            jumpBufferTimer -= Time.deltaTime;
    }

    //旧逻辑
    //void StartFalling()
    //{
    //    isFalling = true;
    //    fallTimer = 0f;
        
    //    // 暂时禁用碰撞（可选）
    //    Collider2D collider = GetComponent<Collider2D>();
    //    if (collider != null)
    //    {
    //        collider.enabled = false;
    //    }
    //}

    //void HandleFalling()
    //{
    //    fallTimer += Time.deltaTime;
        
    //    if (fallTimer >= fallDuration)
    //    {
    //        StopFalling();
    //    }
    //}

    //void StopFalling()
    //{
    //    isFalling = false;
        
    //    // 重新启用碰撞
    //    Collider2D collider = GetComponent<Collider2D>();
    //    if (collider != null)
    //    {
    //        collider.enabled = true;
    //    }
    //}

    //回到出生点 旧
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
    //更新地面检测状态
    private void UpdateGroundDetection()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        playerAnimitor.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            isJumping = false;
            isFalling = false;
            playerAnimitor.SetBool("isFalling", false);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    //跳跃
    private void OnJump()
    {
        playerAnimitor.SetTrigger("JumpTrigger");
        Debug.Log("跳跃");
        //AudioManager.Instance.PlayShortSound(jumpAudioClip, 0.8f);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        playerAnimitor.SetBool("isFalling", false);
    }

    //
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
    //调转方向
    private void HandleFlip()
    {
        if (moveInput != 0)
        {
            int newDir = moveInput > 0 ? 1 : -1;
            if (newDir != currentDirection)
            {
                currentDirection = newDir;
                FlipPlayer();
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
    //角色移动逻辑
    private void FixedUpdate()
    {
        if (!canControl) return;

        float targetSpeed = moveInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right);

        // 地面摩擦力（穿透期间也生效，无额外影响）
        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (grounded && Mathf.Abs(moveInput) < 0.01f)
        {
            float frictionForce = Mathf.Min(Mathf.Abs(rb.velocity.x), frictionAmount);
            frictionForce *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -frictionForce, ForceMode2D.Impulse);
        }
    }

    //开始穿透：禁用碰撞体，让角色受重力自然下落
    private void StartPenetrate()
    {
        if (playerCollider == null) return;

        playerCollider.enabled = false;
        isPenetrating = true;
        penetrateTimer = penetrateDuration;
        lastPenetrateTime = Time.time;

        // 可选：给一点向下的初速度，加快下落（不强制，注释掉）
        // rb.velocity = new Vector2(rb.velocity.x, -2f);

        Debug.Log("穿透开始，碰撞体禁用");
    }

    //结束穿透：重新启用碰撞体
    private void EndPenetrate()
    {
        if (playerCollider == null) return;

        playerCollider.enabled = true;
        isPenetrating = false;

        Debug.Log("穿透结束，碰撞体恢复");
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
