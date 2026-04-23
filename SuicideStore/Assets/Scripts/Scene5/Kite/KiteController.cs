using DG.Tweening;
using UnityEngine;

public class KiteController : MonoBehaviour
{
    public bool isActive = false;                      // 是否已激活（由鸟巢点击启用）
    public Transform playerFixedPosition;              // 风筝上固定玩家的位置（空物体）
    public Transform targetPosition;                     // 风筝最终移动到的目标位置
    public Vector3 targetRotation;                     // 风筝最终的旋转角度（可选，例如飘落姿态）
    public float rotateDuration = 0.8f;                // 旋转动画时长
    public float moveDuration = 2f;                    // 移动动画时长
    public Ease moveEase = Ease.InOutQuad;             // 移动曲线（可改为 InSine 等更柔和的曲线）

    private Transform player;
    private ReversalPlayerController playerController;
    private bool isFlying = false;

    public void ActivateKite()
    {
        isActive = true;
        // 可在此添加视觉提示，如改变风筝颜色或播放粒子
        Debug.Log("风筝已激活，可触碰");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || isFlying) return;      // 未激活 或 已经起飞，不再触发
        if (!other.CompareTag("Player")) return;

        player = other.transform;
        playerController = other.GetComponent<ReversalPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("玩家身上没有 ReversalPlayerController 脚本！");
            return;
        }

        isFlying = true;
        // 1. 禁用玩家操控
        player.GetComponent<Rigidbody2D>().isKinematic = true;
        player.GetComponent<Collider2D>().enabled = false;

        playerController.SetCanMove(false);

        Debug.Log("玩家控制已禁用");

        // 2. 将玩家移动到风筝上的固定点，并设为风筝的子物体（同步移动旋转）
        player.DOMove(playerFixedPosition.position, 1.5f).SetEase(Ease.OutQuad);
        player.SetParent(transform);
        // 3. 启动风筝动画序列：先旋转，后移动
        Sequence kiteSequence = DOTween.Sequence();

        // 可选：同时播放风筝自身的飘动动画（Animator），这里旋转作为“调整方向”
        kiteSequence.Append(transform.DORotate(targetRotation, rotateDuration).SetEase(Ease.InOutQuad));
        kiteSequence.Append(transform.DOMove(targetPosition.position, moveDuration).SetEase(moveEase));
        Scene5Manage.Instance.ChangeCamera(2,1f);
        // 动画完成后回调
        kiteSequence.OnComplete(() => OnFlyComplete());
    }

    private void OnFlyComplete()
    {
        Destroy(gameObject);
    }
}