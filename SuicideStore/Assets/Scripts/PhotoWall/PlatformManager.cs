using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    [Header("平台设置")]
    public Transform[] platforms;
    public Transform player;
    public float platformHeightOffset = 1f;

    [Header("当前状态")]
    public int currentPlatformIndex = 0;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (player != null && platforms.Length > 0)
        {
            FindCurrentPlatform();
        }
    }

    public void FindCurrentPlatform()
    {
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
        float currentY = platforms[currentPlatformIndex].position.y;
        int closestLowerIndex = -1;
        float closestLowerY = float.MinValue;

        for (int i = 0; i < platforms.Length; i++)
        {
            if (i == currentPlatformIndex) continue;

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
        if (index < 0 || index >= platforms.Length)
        {
            return;
        }

        Transform targetPlatform = platforms[index];
        player.position = new Vector3(
            player.position.x,
            targetPlatform.position.y + platformHeightOffset,
            player.position.z
        );
        currentPlatformIndex = index;
    }
}
