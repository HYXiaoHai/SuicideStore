using UnityEngine;

public class LanternPortal : MonoBehaviour
{
    [Header("传送目标")]
    public Transform exitPoint;           // 出口位置（建议放在另一个空物体上）

    [Header("推出力度")]
    public Vector2 launchForce = new Vector2(5f, 5f); // 力的方向和大小（X:水平, Y:垂直）

    [Header("传送冷却")]
    public float teleportCooldown = 0.5f;  // 每次传送后的冷却时间（秒）
    private float lastTeleportTime = -999f;

    [HideInInspector]
    public bool canTeleport = false;       // 由灯笼交互脚本激活
    [Header("音效")]
    public AudioClip portalAudioClip;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 条件检查：传送门未激活、不是玩家、还在冷却中 → 返回
        if (!canTeleport) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time < lastTeleportTime + teleportCooldown) return;

        lastTeleportTime = Time.time;

        // 获取玩家的 Rigidbody2D（用于施加力）
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("玩家身上没有 Rigidbody2D，无法施加力！");
            return;
        }

        // 传送位置
        if (exitPoint != null)
        {
            AudioManager.Instance.Play2DSound(portalAudioClip, 0.5f);
            other.transform.position = exitPoint.position;
        }
        else
            Debug.LogWarning("传送门出口未设置，位置不变");

        // 施加力：先清零原有速度（可选，让推力效果更纯粹）
        rb.velocity = Vector2.zero;
        rb.AddForce(launchForce, ForceMode2D.Impulse);

        Debug.Log($"传送完成，施加推力：{launchForce}");
    }

    // ==================== Scene 视图调试 ====================
    private void OnDrawGizmos()
    {
        if (exitPoint == null) return;

        // 1. 绘制入口→出口的连线（黄色）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, exitPoint.position);

        // 2. 在出口位置绘制推力方向箭头（绿色）
        Gizmos.color = Color.green;
        Vector3 start = exitPoint.position;
        Vector3 direction = new Vector3(launchForce.x, launchForce.y, 0).normalized;
        float arrowLength = Mathf.Min(launchForce.magnitude * 0.2f, 2f); // 长度自适应，最大2

        if (launchForce.magnitude > 0.01f)
        {
            // 主线
            Gizmos.DrawRay(start, direction * arrowLength);
            // 箭头头部
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 0, 135) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 0, -135) * Vector3.forward;
            Gizmos.DrawRay(start + direction * arrowLength, right * 0.2f);
            Gizmos.DrawRay(start + direction * arrowLength, left * 0.2f);
        }

        // 3. 可选：在入口位置绘制一个小球标（方便识别）
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}