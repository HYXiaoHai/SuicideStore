using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class RowManage : MonoBehaviour
{
    [Header("玩家")]
    public Rigidbody2D playerRb;        // 玩家刚体
    public SpriteRenderer playerSprite;   // 用于透明度控制
    public Collider2D playerCollider;
    public Animator playerAnimator;       // 用于播放行走动画
    public float walkSpeed = 3f;

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
    public Transform aniStartPosition;     // 开场起始位置
    public Transform gameStartPosition;    // 游戏正式开始位置
    public Transform gameEndPosition;    // 游戏正式开始位置
    public float fadeDuration = 1f;        // 渐显时间（与移动时间同步）
    private bool isFalling = false;
    [Header("线")]
    public Image[] lineImage;

    [Header("行数据")]
    public RowController[] rows;         // 按顺序拖入三行的 RowController
    public int currentRowIndex = 0;
    [Header("关卡切换")]
    public int nextLevelIndex = 2;//第2关
    public float changeDelay = 1f;//完成后多久切换镜头（延迟）
    private bool hasTriggeredSwitch = false; //防止重复触发

    // 状态机
    private enum GameState { OnWords, OnLine }
    private GameState currentState;

    // 当前行的边界（文字行走时用）
    private float leftBoundX, rightBoundX;
    // 当前行的横线边界（横线行走时用）
    private float lineLeftX, lineRightX;

    // 控制是否允许输入
    public bool canMove = true;

    private float moveInput;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool isJumping = false;
    public bool isGrounded = false;                //缓存地面状态
    private SpriteRenderer spriteRenderer;
    private int currentDirection = 1;

    void Start()
    {
        if (playerRb == null) playerRb = GetComponent<Rigidbody2D>();
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();

        // 初始状态：禁用碰撞体，防止提前交互
        if (playerCollider != null) playerCollider.enabled = false;
        playerRb.gravityScale = 0f;

        canMove = false;
        wasGrounded = isGrounded;
    }
    // 供外部调用的公开方法，开始第二关
    public void BeginGame()
    {
        // 重置游戏状态（可多次调用）
        currentRowIndex = 0;
        canMove = false;
        playerRb.velocity = Vector2.zero;

        // 重置所有行的状态（填充、碰撞体等）
        foreach (var row in rows)
        {
            row.ResetRow();
        }
        // 重置所有行的状态（填充、碰撞体等）
        foreach (var row in lineImage)
        {
            row.DOFade(0.9f, fadeDuration);
        }
        SetupCurrentRowForWords();

        // 开始开场动画
        PlayOpeningAnimation();
    }
    public void  PlayOpeningAnimation()
    {
        // 禁用玩家碰撞体，防止动画期间触发物理碰撞
        if (playerCollider != null) playerCollider.enabled = false;

        // 设置起始位置和透明度
        playerRb.position = aniStartPosition.position;
        playerRb.velocity = Vector2.zero;
        Color color = playerSprite.color;
        color.a = 0f;
        playerSprite.color = color;

        //// 播放行走动画（假设动画状态名为 "Xiang_Walk"）
        //if (playerAnimator != null)
        Debug.Log("设置开场行走动画");
        playerAnimator.SetBool("isMoving", true);

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 同时移动位置和渐显
        sequence.Join(playerRb.DOMove(gameStartPosition.position, fadeDuration).SetEase(Ease.Linear));
        sequence.Join(playerSprite.DOFade(1f, fadeDuration));

        // 动画结束后执行初始化
        sequence.OnComplete(() =>
        {
            // 动画结束，启用碰撞体，允许移动
            if (playerCollider != null) playerCollider.enabled = true;
            playerRb.gravityScale = 1f;
            currentState = GameState.OnWords;
            canMove = true;
        });
    }
    void Update()
    {
        if (!canMove) return;

        // 获取水平输入（A/D 或 左/右箭头）
        float horizontal = Input.GetAxisRaw("Horizontal");
        Debug.Log("设置行走动画");
        playerAnimator.SetBool("isMoving", Mathf.Abs(horizontal) > 0);
        moveInput = horizontal;
        // 玩家转向（根据输入方向）
        if (horizontal != 0)
            playerSprite.flipX = horizontal < 0;
        
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

        //++++++++++
        //地面检测
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

        // 落地音效检测
        if (!wasGrounded && isGrounded)
        {
            if (landAudioClip != null)
            {
                AudioManager.Instance.Play2DSound(landAudioClip, 0.6f); // 音量可调整
            }
        }
        wasGrounded = isGrounded;
        // 根据当前状态处理移动
        switch (currentState)
        {
            case GameState.OnWords:
                HandleMovementOnWords(horizontal);
                break;
            case GameState.OnLine:
                HandleMovementOnLine(horizontal);
                break;
        }
    }

    // 在文字上行走时的逻辑
    private void HandleMovementOnWords(float horizontal)
    {
        // 刚体移动（水平速度）
        float targetSpeed = horizontal * walkSpeed;
        playerRb.velocity = new Vector2(targetSpeed, playerRb.velocity.y);

      
        // 更新当前行的填充（根据玩家实际 X）
        rows[currentRowIndex].UpdateFillByPlayerX(playerRb.position.x);

        // 到达右边界 → 触发完成文字行
        if (playerRb.position.x >= rightBoundX)
        {
            CompleteCurrentRow();
        }
    }

    // 在横线上行走时的逻辑
    private void HandleMovementOnLine(float horizontal)
    {
        // 横线上只允许向左移动（按 A 或 左箭头），也可以允许双向，但到达左边界才切换
        // 这里为了符合需求，允许双向移动，但到达左边界后自动切换
        float targetSpeed = horizontal * walkSpeed;
        playerRb.velocity = new Vector2(targetSpeed, playerRb.velocity.y);

        // 限制在横线边界内
        Vector2 pos = playerRb.position;
        float newX = Mathf.Clamp(pos.x, lineLeftX, lineRightX);
        if (newX != pos.x)
            playerRb.position = new Vector2(newX, pos.y);

        // 到达左边界 → 触发完成横线行走，掉落到下一行
        if (playerRb.position.x <= lineLeftX)
        {
            CompleteCurrentLine();
        }
    }

    // 完成当前文字行（右边界触发）
    private void CompleteCurrentRow()
    {
        canMove = false;
        playerRb.velocity = Vector2.zero;   // 停止移动
        // 通知当前行：文字部分完成，开启横线碰撞体
        rows[currentRowIndex].CompleteRow();
        if (currentRowIndex == rows.Length - 1)
        {
            Debug.Log("通关！");
            playerRb.gravityScale = 0f;
            canMove = false;
            TriggerLevelComplete();  // 触发通关完成
            return;
        }
        Invoke(nameof(StartLinePhase), 0.5f);
    }

    private void StartLinePhase()
    {
        // 更新横线边界
        lineLeftX = rows[currentRowIndex].GetLineLeftX();
        lineRightX = rows[currentRowIndex].GetLineRightX();

        currentState = GameState.OnLine;
        canMove = true;
    }

    // 完成当前横线行走（左边界触发）
    private void CompleteCurrentLine()
    {
        canMove = false;
        playerRb.velocity = Vector2.zero;

        // 通知当前行：横线阶段完成，关闭横线碰撞体（让玩家掉落）
        rows[currentRowIndex].CompleteLine();

      
        // 切换到下一行
        SwitchToNextRow();
    }

    private void SwitchToNextRow()
    {
        currentRowIndex++;

        // 重置新行的状态（文字碰撞体开启，横线碰撞体关闭，填充重置）
        rows[currentRowIndex].ResetRow();

        // 重新设置文字行走的边界
        SetupCurrentRowForWords();

        currentState = GameState.OnWords;
        canMove = true;
    }
    //触发通关完成（渐隐+切换）
    private void TriggerLevelComplete()
    {
        if (hasTriggeredSwitch) return;
        hasTriggeredSwitch = true;
        // 停止移动
        playerRb.velocity = Vector2.zero;
        // 禁用碰撞体
        if (playerCollider != null) playerCollider.enabled = false;
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 同时移动位置和渐隐藏
        sequence.Join(playerRb.DOMove(gameEndPosition.position, 2f));
        sequence.Join(playerSprite.DOFade(0f, 2f));//渐隐
        //OnGrowthComplete();
        //动画结束后执行初始化
        sequence.OnComplete(() =>
        {
            OnGrowthComplete();
        });
    }
    //进入下一关
    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");

        if (Scene4Manage.Instance != null)
        {
            // 假设 changeDelay = 2f，相机切换完成后启动第二关
            Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                if (nextLevelIndex==3&&Scene4Manage.Instance.level3Manage != null)
                {
                    //下一关的manage
                    Scene4Manage.Instance.level3Manage.StartGame();
                }
                else if (nextLevelIndex == 5)
                {
                    CompleteLevel();
                }
                else
                {
                    Debug.LogError("level2Manage 未在 Scene4Manage 中赋值！");
                }
            });
        }
        else if(Scene5Manage.Instance != null)
        {
            CompleteLevel();
        }
    }
    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
    private void SetupCurrentRowForWords()
    {
        RowController row = rows[currentRowIndex];
        leftBoundX = row.leftBound.position.x;
        rightBoundX = row.rightBound.position.x;
    }

    //角色跳跃
    private void UpdateGroundDetection()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        playerAnimator.SetBool("isGrounded", isGrounded);
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            isJumping = false;
            isFalling = false;          // 落地重置
            playerAnimator.SetBool("isFalling", false);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void OnJump()
    {
        //动画切换
        playerAnimator.SetTrigger("JumpTrigger");
        Debug.Log("跳跃");
        //施加力
        playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);
        playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;          // ��¼��Ծʱ��
        jumpBufferTimer = 0;
        coyoteTimer = 0;

        // 重置下降标志（动画器会等待 isFalling 变为 true）
        playerAnimator.SetBool("isFalling", false);
    }

    private void HandleJumpUp()
    {
        if (Input.GetButtonUp("Jump") && isJumping && playerRb.velocity.y > 0)
        {
            OnJumpUp();
        }
    }

    private void OnJumpUp()
    {
        playerAnimator.SetTrigger("JumpTrigger");
        // 重置下降标志（动画器会等待 isFalling 变为 true）
        playerAnimator.SetBool("isFalling", false);
        if (playerRb.velocity.y > 0 && isJumping)
        {
            playerRb.AddForce(Vector2.down * playerRb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        }
        lastJumpTime = 0;
        isJumping = false;
    }

    private void UpdateGravity()
    {
        if (playerRb.velocity.y < 0)
        {
            Debug.Log("下落");
            playerRb.gravityScale = gravityScale * fallGravityMultiplier;
            isFalling = true;
            playerAnimator.SetBool("isFalling", true);
        }
        else
        {
            playerRb.gravityScale = gravityScale;
            isFalling = false;
            playerAnimator.SetBool("isFalling", false);
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
               
            }
        }
    }

    private void FlipPlayer()
    {
        if (spriteRenderer != null)
        {
            Vector3 scale = playerRb.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * currentDirection;
            playerRb.transform.localScale = scale;
        }
    }
}