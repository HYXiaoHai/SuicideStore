using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   // 需要引入 TextMeshPro

public class JumpGameManager : MonoBehaviour
{
    public static JumpGameManager Instance;

    [Header("圈配置")]
    public List<GameObject> circlePrefabs = new List<GameObject>();
    public Transform startPoint;
    public Transform endPoint;

    [Header("速度设置（四个圈）")]
    public float[] speeds = new float[4] { 1.5f, 2f, 2.5f, 3f };

    [Header("玩家")]
    public GameObject player;
    public PlayerJumpController playerJump;

    [Header("UI")]
    public TextMeshProUGUI countdownText;   // 倒计时显示的文本（3,2,1,GO）

    private JumpCircle currentCircle;
    private int currentIndex = 0;           // 当前正在进行的圈索引（0教学，1,2,3正式）
    private bool isGameActive = false;      // 倒计时结束后为true，表示正式游戏进行中
    private bool isWaitingForJump = false;
    private int successCount = 0;           // 成功跳过的正式圈个数（0~3）
    public bool gameCompleted =false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerJump.SetCanJump(false);
        // 初始隐藏或清空UI
        if (countdownText != null) countdownText.text = " ";
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    // 外部调用：开启跳圈游戏（由风筝相机切换后触发）
    public void StartJumpGame()
    {
        gameCompleted = false;
        playerJump.SetCanJump(true);
        // 开始教学圈（索引0）
        StartCircle(0);
    }

    // 开始指定索引的圈
    public void StartCircle(int index)
    {
        if (index >= speeds.Length)
        {
            OnAllCirclesPassed();
            return;
        }

        currentIndex = index;
        GameObject newCircle = Instantiate(circlePrefabs[currentIndex], startPoint.position, Quaternion.identity);
        currentCircle = newCircle.GetComponent<JumpCircle>();
        currentCircle.moveSpeed = speeds[currentIndex];

        StartCoroutine(WatchCircleLife());
        isWaitingForJump = true;
    }

    IEnumerator WatchCircleLife()
    {
        while (currentCircle != null && currentCircle.transform.position.x < endPoint.position.x)
            yield return null;
        if (currentCircle != null && isWaitingForJump)
            OnJumpFailed("圈已超出范围");
    }

    // 由玩家动画事件调用（落地瞬间）
    public void OnPlayerJumpLand()
    {
        if (!isWaitingForJump) return;
        if (currentCircle == null) return;

        bool isInCenter = currentCircle.IsPlayerInCenter(player);

        if (isInCenter)
            OnJumpSuccess();
        else
            OnJumpFailed("不在中心区域");
    }

    private void OnJumpSuccess()
    {
        isWaitingForJump = false;
        currentCircle.StopMoving();

        if (currentIndex == 0)
        {
            // 教学圈通过，开启倒计时，不直接进入下一个圈
            Debug.Log("教学完成，准备倒计时...");
            Destroy(currentCircle.gameObject);
            StartCoroutine(StartCountdown());
        }
        else
        {
            // 正式圈通过，计数+1，更新UI
            successCount++;
            if (countdownText != null) countdownText.text = successCount.ToString();

            Destroy(currentCircle.gameObject);

            if (currentIndex + 1 < speeds.Length)
                StartCircle(currentIndex + 1);
            else
                OnAllCirclesPassed();
        }
    }

    // 倒计时协程
    private IEnumerator StartCountdown()
    {
        // 倒计时期间禁止玩家跳跃（也可以让玩家不能跳跃，但教学圈后玩家跳跃已启用，我们暂时禁用跳跃输入感知，但为了安全，可以再关闭一次跳跃权限）
        playerJump.SetCanJump(false);
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(3f); // 如果没有UI，等待3秒
        }

        // 倒计时结束，正式激活游戏
        isGameActive = true;
        // 重置计数器（因为正式游戏开始，之前教学圈不计入）
        successCount = 0;
        if (countdownText != null) countdownText.text = "0";
        // 恢复玩家跳跃能力
        playerJump.SetCanJump(true);
        // 开始正式游戏第一个圈（索引1，即第二个圈）
        StartCircle(1);
    }

    private void OnJumpFailed(string reason)
    {
        if (!isGameActive && currentIndex == 0)
        {
            // 教学圈失败：重新来教学圈（不清除UI，不启用正式游戏）
            Debug.Log("教学圈失败，重新开始教学圈");
            if (currentCircle != null) Destroy(currentCircle.gameObject);
            StartCircle(0);
            return;
        }

        if (isGameActive)
        {
            Debug.Log($"失败原因：{reason}。回到第2个圈，分数归零");
            // 清理当前圈
            if (currentCircle != null) Destroy(currentCircle.gameObject);
            // 重置计数UI
            successCount = 0;
            if (countdownText != null) countdownText.text = "0";
            // 重置到第2个圈（索引1），不重新倒计时
            StartCircle(1);
        }
    }

    private void OnAllCirclesPassed()
    {
        Debug.Log("胜利！所有圈通过");
        playerJump.SetCanJump(false);
        gameCompleted = true;
        BagPackingManager.Instance.StartGame();
    }
}