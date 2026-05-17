using UnityEngine;
using TMPro;   // 如需显示剩余数量或胜利文字可增加

public class DragBallManager : MonoBehaviour
{
    public static DragBallManager Instance;

    [Header("游戏配置")]
    public int totalBalls = 4;        // 总小球数（也可以自动统计场景中所有 DragBall）

    public int remainingBalls;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        remainingBalls = totalBalls;
    }

    public void OnBallCollected()
    {
        if (isGameOver) return;

        remainingBalls--;

        if (remainingBalls <= 0)
        {
            GameVictory();
        }
    }

    private void GameVictory()
    {
        isGameOver = true;
        Debug.Log("游戏胜利！");
        Scene10Manage.Instance.Leve3Complete();
    }
}