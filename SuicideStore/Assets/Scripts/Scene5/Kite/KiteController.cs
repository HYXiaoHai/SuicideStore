using DG.Tweening;
using UnityEngine;

public class KiteController : MonoBehaviour
{
    public bool isActive = false;
    public Transform playerFixedPosition;
    public Transform targetPosition;
    public Vector3 targetRotation;
    public float rotateDuration = 0.8f;
    public float moveDuration = 2f;
    public Ease moveEase = Ease.InOutQuad;

    public Animator animator;

    private Transform player;
    private ReversalPlayerController playerController;
    private bool isFlying = false;
    private bool isFollowing = false;
    public void ActivateKite()
    {
        isActive = true;
        Debug.Log("风筝已激活，可触碰");
    }

    void Update()
    {
        if (isFollowing && player != null && playerFixedPosition != null)
        {
            player.position = playerFixedPosition.position;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || isFlying) return;
        if (!other.CompareTag("Player")) return;

        player = other.transform;
        playerController = other.GetComponent<ReversalPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("玩家身上没有 ReversalPlayerController 脚本！");
            return;
        }

        isFlying = true;

        // 1. 禁用玩家物理和操控
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;
        Collider2D col = player.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        playerController.SetCanMove(false);
        Debug.Log("玩家控制已禁用");

        // 2. 玩家移动到固定点，完成后启动风筝动画
        player.DOMove(playerFixedPosition.position, 1.5f).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 移动完成，翻转玩家面朝左
                Vector3 scale = player.localScale;
                scale.x = -Mathf.Abs(scale.x);
                player.localScale = scale;

                // 开启跟踪模式
                isFollowing = true;
                Debug.Log("玩家已固定，开始风筝旋转");

                // 3. 创建风筝动画序列：旋转 → 移动
                Sequence kiteSequence = DOTween.Sequence();

                kiteSequence.Append(transform.DORotate(targetRotation, rotateDuration).SetEase(Ease.InOutQuad));

                kiteSequence.AppendCallback(() => {
                    if (Scene5Manage.Instance != null)
                    {
                        Scene5Manage.Instance.ChangeCamera(2, 1f, () => {
                            // 相机切换完成后，开始跳圈游戏
                            if (JumpGameManager.Instance != null)
                                JumpGameManager.Instance.StartJumpGame();
                        });
                    }
                    else
                        Debug.LogWarning("Scene5Manage.Instance 不存在");
                });
                kiteSequence.AppendCallback(() => {
                    if (animator != null)
                        animator.SetBool("IsFlying", true);
                });
                kiteSequence.Append(transform.DOMove(targetPosition.position, moveDuration).SetEase(moveEase));
                //在这里播放动画

                kiteSequence.OnComplete(() => OnFlyComplete());
            });
    }

    private void OnFlyComplete()
    {
        isFollowing = false;
        Destroy(gameObject);
    }
}