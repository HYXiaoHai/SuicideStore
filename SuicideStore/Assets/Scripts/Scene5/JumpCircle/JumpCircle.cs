using UnityEngine;

public class JumpCircle : MonoBehaviour
{
    public float moveSpeed = 2f;          // 移动速度
    public Transform centerTrigger;       // 中心判定区域的Transform（通常是一个子物体Collider）
    private bool isMoving = true;

    void Update()
    {
        if (isMoving)
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
    }

    // 供外部调用停止移动（如判定后）
    public void StopMoving() { isMoving = false; }

    // 获取中心区域的位置（用于视觉反馈等）
    public Vector3 GetCenterPosition() { return centerTrigger.position; }

    // 判断玩家是否落在中心区域（由管理器调用，通过物理检测）
    public bool IsPlayerInCenter(GameObject player)
    {
        // 注意：中心触发器需要有Collider2D且为IsTrigger
        Collider2D centerCollider = centerTrigger.GetComponent<Collider2D>();
        if (centerCollider == null) return false;

        // 检测玩家是否与该触发器重叠
        return centerCollider.bounds.Contains(player.transform.position);
    }
}