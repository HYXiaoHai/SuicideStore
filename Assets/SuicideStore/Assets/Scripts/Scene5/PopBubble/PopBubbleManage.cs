using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopBubbleManage : MonoBehaviour
{
    public static PopBubbleManage Instance;

    [Header("气泡")]
    public List<Bubble> bubbles = new List<Bubble>();

    [Header("开局对话框")]
    public float startDuration = 1f;
    public Image paopaoText1;
    public Image paopaoText2;

    private bool gameCompleted = false;
    public bool isGameStarted = false;
    private int completedCount = 0;        // 已点击完成的气泡数

    [Header("关卡切换")]
    public int nextLevelIndex = 2;//第2关
    public float changeDelay = 1f;//完成后多久切换镜头（延迟）
    private bool hasTriggeredSwitch = false; //防止重复触发

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化：不可交互，透明度0，不浮动
        foreach (var bubble in bubbles)
        {
            bubble.SetAlpha(0f);
            bubble.canInteract = false;
        }
        // 对话框初始缩放为0
        paopaoText1.transform.localScale = Vector3.zero;
        paopaoText2.transform.localScale = Vector3.zero;
    }

    public void StartGame()
    {
        Sequence seq = DOTween.Sequence();
        // 对话框弹出动画
        seq.Join(paopaoText1.transform.DOScale(1f, startDuration).SetEase(Ease.OutBack));
        seq.Join(paopaoText2.transform.DOScale(1f, startDuration).SetEase(Ease.OutBack));

        // 为每个气泡添加渐显动画（随机时长 0.3~0.8 秒）
        foreach (var bubble in bubbles)
        {
            float randomDuration = Random.Range(0.3f, 0.8f);
            seq.Join(bubble.bubbleSprite.DOFade(1f, randomDuration)
                .OnComplete(() => {
                    // 渐显完成后启用交互并开始浮动
                    bubble.canInteract = true;
                    bubble.StartFloating();
                }));
        }

        seq.OnComplete(() =>
        {
            isGameStarted = true;
            Debug.Log("气泡游戏已开始，可以点击");
        });
    }

    public void OnBubbleClicked(Bubble bubble)
    {
        if (gameCompleted) return;
        completedCount++;
        Debug.Log($"已点击 {completedCount}/{bubbles.Count} 个气泡");
        if (completedCount >= bubbles.Count)
        {
            OnGameComplete();
        }
    }
    //进入下一关
    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");

        if (Scene5Manage.Instance != null)
        {
            // 假设 changeDelay = 2f，相机切换完成后启动第二关
            Scene5Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                if (Scene5Manage.Instance.level3manage != null)
                {
                    Scene5Manage.Instance.level3manage.BeginGame();
                }
                else
                {
                    Debug.LogError("level2Manage 未在 Scene4Manage 中赋值！");
                }
            });
        }
        else
        {
            Debug.LogError("Scene4Manage.Instance 不存在，请确保场景中有 Scene4Manage 组件");
        }
    }
    private void OnGameComplete()
    {
        gameCompleted = true;
        isGameStarted = false;
        OnGrowthComplete();
        Debug.Log("所有气泡点击完成，游戏胜利！");
        // 暂时空函数，以后可扩展转场或奖励
        // 例如：Scene5Manage.Instance.ChangeCamera(4, 1f);
    }
}