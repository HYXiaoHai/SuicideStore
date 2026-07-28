using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoMapper : MonoBehaviour
{
    [Header("玩家")]
    public Rigidbody2D playerRb;
    public SpriteRenderer playerSprite;
    public Collider2D playerCollider;
    public Animator playerAnimator;
    public Transform playerTransform;

    [Header("角色移动")]
    public float walkSpeed = 5f;
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
    private float lastJumpTime;

    [Header("脚步生成")]
    public List<Image> footsUp;   // 上半部分脚印
    public List<Image> footsDown; // 下半部分脚印
    public float aniFadeDuration = 0.5f;

    [Header("音效")]
    public AudioClip[] walkAudioClips;
    private float walkSoundInterval = 0.3f;
    private float walkSoundTimer = 0f;
    public AudioClip landAudioClip;
    private bool wasGrounded = false;

    [Header("地面检测")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("映射边界")]
    public Transform playerLeftBound;
    public Transform playerRightBound;
    public RectTransform photo;
    public RectTransform photoLeftBound;
    public RectTransform photoRightBound;

    [Header("进场动画")]
    public Transform aniStartPosition;
    public Transform gameStartPosition;
    public Transform gameEndPosition;
    public float fadeDuration = 1f;

    [Header("平滑跟随（可选）")]
    public bool useSmoothFollow = false;
    public float smoothSpeed = 3f;

    [Header("关卡切换")]
    public int nextLevelIndex = 2;
    public float changeDelay = 1f;
    private bool hasTriggeredSwitch = false;

    // 内部状态
    private Vector2 photoInitialAnchoredPos;
    private bool isGameStarted = false;
    private bool isGameEnd = false;
    private bool canMove = true;

    private float moveInput;
    private float coyoteTimer = 0f;
    private float jumpBufferTimer = 0f;
    private bool isJumping = false;
    public bool isGrounded = false;
    private bool isFalling = false;

    void Start()
    {
        // 缓存组件
        if (playerRb == null) playerRb = GetComponent<Rigidbody2D>();
        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (playerAnimator == null) playerAnimator = GetComponent<Animator>();
        if (playerTransform == null) playerTransform = transform;

        if (photo == null)
            photo = GetComponent<RectTransform>();

        if (photo != null)
            photoInitialAnchoredPos = photo.anchoredPosition;

        // 初始状态
        if (playerCollider != null) playerCollider.enabled = false;
        playerRb.gravityScale = 0f;
        canMove = false;
        wasGrounded = isGrounded;
        // 照片初始位置（左侧边界）
        if (photoLeftBound != null && photo != null)
        {
            Vector2 pos = photo.anchoredPosition;
            pos.x = photoLeftBound.anchoredPosition.x;
            photo.anchoredPosition = pos;
        }

        // 初始化脚印：全部隐藏，然后显示第一个
        HideAllFootsteps();
        // 立即显示第一个脚印（如果列表非空）
        UpdateFootsteps(0f);

        isGameEnd = false;
    }

    void Update()
    {
        if (!isGameStarted) return;
        if (isGameEnd) return;

        // 1. 输入处理
        moveInput = Input.GetAxisRaw("Horizontal");

        // 动画
        if (playerAnimator != null)
            playerAnimator.SetBool("isMoving", Mathf.Abs(moveInput) > 0);

        // 翻转
        if (moveInput != 0)
            playerSprite.flipX = moveInput < 0;

        // 2. 音效
        if (isGrounded && Mathf.Abs(moveInput) > 0)
        {
            walkSoundTimer -= Time.deltaTime;
            if (walkSoundTimer <= 0f)
            {
                if (walkAudioClips != null && walkAudioClips.Length > 0)
                {
                    AudioClip randomClip = walkAudioClips[Random.Range(0, walkAudioClips.Length)];
                    AudioManager.Instance.Play2DSound(randomClip, 0.5f);
                }
                walkSoundTimer = walkSoundInterval;
            }
        }

        // 3. 地面检测
        UpdateGroundDetection();

        // 4. 跳跃
        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
            OnJump();
        HandleJumpUp();
        UpdateGravity();

        // 5. 落地音效
        if (!wasGrounded && isGrounded)
        {
            if (landAudioClip != null)
                AudioManager.Instance.Play2DSound(landAudioClip, 0.6f);
        }
        wasGrounded = isGrounded;

        // 6. 照片位置映射
        if (playerTransform != null && playerLeftBound != null && playerRightBound != null &&
            photoLeftBound != null && photoRightBound != null && photo != null)
        {
            float playerX = Mathf.Clamp(playerTransform.position.x, playerLeftBound.position.x, playerRightBound.position.x);
            float t = Mathf.InverseLerp(playerLeftBound.position.x, playerRightBound.position.x, playerX);
            float targetX = Mathf.Lerp(photoLeftBound.anchoredPosition.x, photoRightBound.anchoredPosition.x, t);
            Vector2 targetPos = new Vector2(targetX, photoInitialAnchoredPos.y);

            if (useSmoothFollow)
                photo.anchoredPosition = Vector2.Lerp(photo.anchoredPosition, targetPos, smoothSpeed * Time.deltaTime);
            else
                photo.anchoredPosition = targetPos;

            // 7. 更新脚印（基于相同进度 t）
            UpdateFootsteps(t);
        }

        // 8. 检测通关
        if (!hasTriggeredSwitch && playerRightBound != null && playerTransform.position.x >= playerRightBound.position.x)
        {
            TriggerLevelComplete();
        }

        // 更新计时器
        if (jumpBufferTimer > 0) jumpBufferTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!isGameStarted || !canMove) return;

        float targetSpeed = moveInput * walkSpeed;
        targetSpeed = Mathf.Clamp(targetSpeed, -walkSpeed, walkSpeed);
        playerRb.velocity = new Vector2(targetSpeed, playerRb.velocity.y);

        if (isGrounded && Mathf.Abs(moveInput) < 0.01f)
        {
            float frictionForce = Mathf.Min(Mathf.Abs(playerRb.velocity.x), frictionAmount);
            frictionForce *= Mathf.Sign(playerRb.velocity.x);
            playerRb.AddForce(Vector2.right * -frictionForce, ForceMode2D.Impulse);
        }
    }

    // ------------------- 跳跃与重力 -------------------
    private void UpdateGroundDetection()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isGrounded", isGrounded);
            if (isGrounded)
            {
                coyoteTimer = coyoteTime;
                isJumping = false;
                isFalling = false;
                playerAnimator.SetBool("isFalling", false);
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }
        }
    }

    private void OnJump()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger("JumpTrigger");
        Debug.Log("跳跃");
        playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);
        playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;
        jumpBufferTimer = 0;
        coyoteTimer = 0;
        if (playerAnimator != null)
            playerAnimator.SetBool("isFalling", false);
    }

    private void HandleJumpUp()
    {
        if (Input.GetButtonUp("Jump") && isJumping && playerRb.velocity.y > 0)
            OnJumpUp();
    }

    private void OnJumpUp()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger("JumpTrigger");
        if (playerAnimator != null)
            playerAnimator.SetBool("isFalling", false);
        if (playerRb.velocity.y > 0 && isJumping)
            playerRb.AddForce(Vector2.down * playerRb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
        lastJumpTime = 0;
        isJumping = false;
    }

    private void UpdateGravity()
    {
        if (playerRb.velocity.y < 0)
        {
            playerRb.gravityScale = gravityScale * fallGravityMultiplier;
            isFalling = true;
            if (playerAnimator != null)
                playerAnimator.SetBool("isFalling", true);
        }
        else
        {
            playerRb.gravityScale = gravityScale;
            isFalling = false;
            if (playerAnimator != null)
                playerAnimator.SetBool("isFalling", false);
        }
    }

    // ------------------- 脚印控制 -------------------
    private void HideAllFootsteps()
    {
        SetFootstepListAlpha(footsUp, 0f);
        SetFootstepListAlpha(footsDown, 0f);
    }

    private void SetFootstepListAlpha(List<Image> list, float alpha)
    {
        if (list == null) return;
        foreach (var img in list)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
                img.DOKill(); // 确保停止之前的动画
            }
        }
    }

    private void UpdateFootsteps(float progress)
    {
        if (footsUp == null && footsDown == null) return;

        // 计算目标显示数量，至少为 1（确保第一个脚印始终显示）
        int targetUp = 0, targetDown = 0;
        if (footsUp != null && footsUp.Count > 0)
            targetUp = Mathf.Clamp(Mathf.FloorToInt(progress * (footsUp.Count - 1)) + 1, 0, footsUp.Count);
        if (footsDown != null && footsDown.Count > 0)
            targetDown = Mathf.Clamp(Mathf.FloorToInt(progress * (footsDown.Count - 1)) + 1, 0, footsDown.Count);

        UpdateFootstepList(footsUp, targetUp);
        UpdateFootstepList(footsDown, targetDown);
    }

    private void UpdateFootstepList(List<Image> list, int targetCount)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            Image img = list[i];
            if (img == null) continue;
            float targetAlpha = (i < targetCount) ? 1f : 0f;
            if (Mathf.Abs(img.color.a - targetAlpha) > 0.01f)
            {
                img.DOKill();
                img.DOFade(targetAlpha, aniFadeDuration).SetEase(Ease.Linear);
            }
        }
    }

    // ------------------- 通关与切换 -------------------
    private void TriggerLevelComplete()
    {
        if (hasTriggeredSwitch) return;
        hasTriggeredSwitch = true;

        if (isGameEnd) return;
            isGameEnd = true;

        playerRb.velocity = Vector2.zero;
        if (playerCollider != null) playerCollider.enabled = false;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(playerRb.DOMove(gameEndPosition.position, 2f));
        sequence.Join(playerSprite.DOFade(0f, 2f));
        sequence.OnComplete(() => OnGrowthComplete());
    }

    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");
        if (Scene4Manage.Instance != null)
        {
            Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                if (nextLevelIndex == 4 && Scene4Manage.Instance.level4Manage != null)
                {
                    Scene4Manage.Instance.level4Manage.BeginGame();
                }
                else
                {
                    Debug.LogError("目标关卡管理器未赋值！");
                }
            });
        }
    }

    // ------------------- 公开方法 -------------------
    public void StartGame()
    {
        if (playerCollider != null)
            playerCollider.enabled = false;

        playerRb.position = aniStartPosition.position;
        playerRb.velocity = Vector2.zero;

        Color color = playerSprite.color;
        color.a = 0f;
        playerSprite.color = color;

        if (playerAnimator != null)
            playerAnimator.SetBool("isMoving", true);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(playerRb.DOMove(gameStartPosition.position, fadeDuration).SetEase(Ease.Linear));
        sequence.Join(playerSprite.DOFade(1f, fadeDuration));

        sequence.OnComplete(() =>
        {
            if (playerCollider != null)
                playerCollider.enabled = true;
            playerRb.gravityScale = 1f;
            if (playerAnimator != null)
                playerAnimator.SetBool("isMoving", false);
            isGameStarted = true;
            canMove = true;
            Debug.Log("进场动画结束，游戏开始");
        });
    }
}