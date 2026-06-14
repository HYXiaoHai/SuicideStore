using DG.Tweening;
using UnityEngine;

public class DragBallManager : MonoBehaviour
{
    public static DragBallManager Instance;

    [Header("游戏配置")]
    public int totalBalls = 4;

    [Header("背景动画")]
    public SpriteRenderer bgSpriteRender1;      // 主背景
    public SpriteRenderer bgSpriteRender_next;  // 辅助过渡背景
    public Sprite[] bgSprites;                  // 按顺序对应第1~4次收集

    [Header("背景涂鸦")]
    public SpriteRenderer[] graffitiSprites;    // 顺序对应小球id (0~3)

    public int remainingBalls;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        remainingBalls = 0;
        HideAllGraffiti();
        // 设置第一张背景
        if (bgSprites.Length > 0)
            bgSpriteRender1.sprite = bgSprites[0];
        // 辅助背景初始透明
        if (bgSpriteRender_next != null)
        {
            bgSpriteRender_next.color = new Color(1, 1, 1, 0);
            bgSpriteRender_next.gameObject.SetActive(true);
        }
    }

    public void OnBallCollected(int id)
    {
        if (isGameOver) return;

        remainingBalls++;
        ChangeBG();
        ShowGraffiti(id);

        if (remainingBalls >= totalBalls)
            GameVictory();
    }

    private void ChangeBG()
    {
        if (bgSprites == null || bgSprites.Length == 0) return;
        int nextIndex = remainingBalls;
        if (nextIndex >= bgSprites.Length) return;

        bgSpriteRender_next.sprite = bgSprites[nextIndex];
        bgSpriteRender_next.color = new Color(1, 1, 1, 1);
        bgSpriteRender_next.DOFade(1f, 0.5f).OnComplete(() =>
        {
            bgSpriteRender1.sprite = bgSprites[nextIndex];
            bgSpriteRender_next.DOFade(0f, 0.3f);
        });
    }

    private void ShowGraffiti(int index)
    {
        if (graffitiSprites == null || index < 0 || index >= graffitiSprites.Length) return;
        graffitiSprites[index].gameObject.SetActive(true);
        graffitiSprites[index].DOFade(1f, 0.5f);
    }

    private void HideAllGraffiti()
    {
        if (graffitiSprites == null) return;
        foreach (var g in graffitiSprites)
        {
            if (g != null)
            {
                g.color = new Color(1, 1, 1, 0);
                g.gameObject.SetActive(false);
            }
        }
    }
    public Vector3 GetGraffitiPosition(int id)
    {
        if (graffitiSprites == null || id < 0 || id >= graffitiSprites.Length)
            return Vector3.zero;
        return graffitiSprites[id].transform.position;
    }
    private void GameVictory()
    {
        isGameOver = true;
        Debug.Log("游戏胜利！");
        Scene10Manage.Instance?.Leve3Complete();
    }
}