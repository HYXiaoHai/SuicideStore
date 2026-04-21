using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [Header("平台设置")]
    public Transform[] platforms;
    public float platformHeightOffset = 1f;

    [Header("当前状态")]
    public int currentPlatformIndex = 0;

    private Transform player;

    void Start()
    {
        FindPlayer();
        if (player != null && platforms.Length > 0)
        {
            FindCurrentPlatform();
        }
    }

    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public void FindCurrentPlatform()
    {
        if (player == null) return;

        float closestY = float.MaxValue;

        for (int i = 0; i < platforms.Length; i++)
        {
            float distanceY = Mathf.Abs(player.position.y - platforms[i].position.y);
            if (distanceY < closestY)
            {
                closestY = distanceY;
                currentPlatformIndex = i;
            }
        }
    }

    public bool TryJumpToLowerPlatform()
    {
        int lowerPlatformIndex = FindNextLowerPlatform();
        if (lowerPlatformIndex == -1)
        {
            return false;
        }

        JumpToPlatform(lowerPlatformIndex);
        return true;
    }

    int FindNextLowerPlatform()
    {
        if (player == null || platforms.Length == 0)
        {
            return -1;
        }

        // 找到所有在当前平台下方的平台
        float currentY = player.position.y;
        int closestLowerIndex = -1;
        float closestLowerY = float.MinValue;

        for (int i = 0; i < platforms.Length; i++)
        {
            float platformY = platforms[i].position.y;
            if (platformY < currentY && platformY > closestLowerY)
            {
                closestLowerY = platformY;
                closestLowerIndex = i;
            }
        }

        return closestLowerIndex;
    }

    public void JumpToPlatform(int index)
    {
        if (index < 0 || index >= platforms.Length || player == null)
        {
            return;
        }

        Transform targetPlatform = platforms[index];
        
        // 直接设置位置，忽略物理
        player.position = new Vector3(
            player.position.x,  // 保持X位置不变
            targetPlatform.position.y + platformHeightOffset,
            player.position.z
        );
        
        // 重置速度，避免物理残留
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        currentPlatformIndex = index;
    }
}
