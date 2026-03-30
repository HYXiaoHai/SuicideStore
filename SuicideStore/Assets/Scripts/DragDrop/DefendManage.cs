using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class DefendManage : MonoBehaviour
{
    [Header("气泡对话")]
    public Transform leftFather;//左气泡出生父物体
    public Transform rightFather;//右气泡出生父物体
    public GameObject bubblePrefab_right;//气泡预制体
    public GameObject bubblePrefab_left;//气泡预制体
    public float minInterval = 1f;//右侧持续生成气泡的最小时间间隔
    public float maxInterval = 1.5f;//右侧持续生成气泡的最大时间间隔
    public string[] dialogues;//右侧随机对话的库存
    public string specicalDialogue0 = "学点好";
    public string specicalDialogue1 = "懒得跟你犟";
    public string specicalDialogue2 = "好了，别讲了！赶紧回家";

    [Header("开场气泡")]
    public GameObject initialBubble;    //预先放在右侧的“乐乐！！！”气泡
    public GameObject initialBubble2;    //预先放在右侧的"你怎么右踩”气泡

    [Header("辩解模块")]
    public static DefendManage Instance;
    public Button defendButton;          // 普通辩解按钮
    public Button specialDefendButton;   // 特殊辩解按钮（第三次出现）
    public int defendNum = 0;

    private Coroutine randomBubbleCoroutine; // 右侧随机气泡协程
    private bool isScene2Started = false; // 是否已开始场景2流程
    private GameObject finalBubble;     // 存储最终气泡引用，用于排除清除
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        // 初始化按钮
        if (defendButton != null)
        {
            defendButton.image.color = new Color(defendButton.image.color.r, defendButton.image.color.g, defendButton.image.color.b, 0);
            defendButton.interactable = false;
            defendButton.gameObject.SetActive(false);
            defendButton.onClick.AddListener(OnDefendButtonClick);
        }
        if (specialDefendButton != null)
        {
            specialDefendButton.image.color = new Color(specialDefendButton.image.color.r, specialDefendButton.image.color.g, specialDefendButton.image.color.b, 0);
            specialDefendButton.interactable = false;
            specialDefendButton.gameObject.SetActive(false);
            specialDefendButton.onClick.AddListener(OnSpecialDefendButtonClick);
        }
    }

    public void StartScene2Dialogue()
    {
        if (isScene2Started) return;
        isScene2Started = true;

        // 1. 让初始“乐乐！！！”气泡飘动并消失
        if (initialBubble != null)
        {
            DialogueBubble bubble = initialBubble.GetComponent<DialogueBubble>();
            if (bubble != null)
            {
                Debug.Log("开场气泡");
                initialBubble.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);

            }
            else
            {
                // 如果没有脚本，直接销毁
                Destroy(initialBubble, 0.5f);
            }
        }

        // 2. 延迟一段时间后生成第二个气泡“你这孩子...”
        StartCoroutine(SpawnSecondBubbleAndStartGame());
    }
    private IEnumerator SpawnSecondBubbleAndStartGame()
    {
        // 等待第一个气泡飘动结束（可根据动画时长适当调整）
        yield return new WaitForSeconds(0.6f);

        // 生成第二个气泡（静止显示）
        initialBubble2.GetComponent<CanvasGroup>().DOFade(1f,1f);
        DialogueBubble bubble = initialBubble2.GetComponent<DialogueBubble>();

        // 等待2秒，让玩家看到文字
        yield return new WaitForSeconds(1f);

        initialBubble2.GetComponent<CanvasGroup>().DOFade(0f, 1f);

        yield return new WaitForSeconds(1.5f);
        // 启用拼图区域
        if (PuzzleManage.Instance != null)
        {
            PuzzleManage.Instance.ShowPuzzleArea();
        }

        // 启动右侧随机对话（从第二个气泡之后开始持续发送）
        StartRandomRightBubblesAfterFirstTwo();
    }

    //启动随机气泡（跳过开头的两个特定对话）
    private void StartRandomRightBubblesAfterFirstTwo()
    {
        if (randomBubbleCoroutine != null)
            StopCoroutine(randomBubbleCoroutine);
        randomBubbleCoroutine = StartCoroutine(RandomBubbleCoroutineAfterFirstTwo());
    }

    private IEnumerator RandomBubbleCoroutineAfterFirstTwo()
    {
        
        while (true)
        {
            if (dialogues != null && dialogues.Length > 0)
            {
                string randomMsg = dialogues[Random.Range(0, dialogues.Length)];
                SendBubble(rightFather, randomMsg,true);
            }
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }


    //生成气泡
    public DialogueBubble SendBubble(Transform parent, string content,bool isright)
    {
        if ((isright && bubblePrefab_right == null) || (!isright && bubblePrefab_left == null) || parent == null)
            return null;

        GameObject bubbleObj = isright ? Instantiate(bubblePrefab_right, parent) : Instantiate(bubblePrefab_left, parent);
        DialogueBubble bubble = bubbleObj.GetComponent<DialogueBubble>();
        if (bubble != null)
        {
            bubble.ShowBubble(content, 0.5f); // 停留0.5秒，上浮4秒（默认参数）
        }
        else
        {
            Destroy(bubbleObj, 2f);
        }
        return bubble;
    }
    //停止右侧随机对话
    public void StopRandomRightBubbles()
    {
        if (randomBubbleCoroutine != null)
        {
            StopCoroutine(randomBubbleCoroutine);
            randomBubbleCoroutine = null;
        }
    }

    //显示普通辩解按钮
    public void ShowDefendButton()
    {
        if (defendButton == null) return;
        defendButton.gameObject.SetActive(true);
        defendButton.interactable = true;
        defendButton.image.DOFade(1f, 0.3f);
    }

    //隐藏普通辩解按钮
    public void HideDefendButton()
    {
        if (defendButton == null) return;
        defendButton.interactable = false;
        defendButton.image.DOFade(0f, 0.3f).OnComplete(() =>
        {
            if (defendButton.gameObject.activeSelf)
                defendButton.gameObject.SetActive(false);
        });
    }

    //显示特殊辩解按钮
    public void ShowSpecialDefendButton()
    {
        if (specialDefendButton == null) return;
        specialDefendButton.gameObject.SetActive(true);
        specialDefendButton.interactable = true;
        specialDefendButton.image.DOFade(1f, 0.3f);
    }

    //隐藏特殊辩解按钮
    public void HideSpecialDefendButton()
    {
        if (specialDefendButton == null) return;
        specialDefendButton.interactable = false;
        specialDefendButton.image.DOFade(0f, 0.3f).OnComplete(() =>
        {
            if (specialDefendButton.gameObject.activeSelf)
                specialDefendButton.gameObject.SetActive(false);
        });
    }

    //普通按钮点击（前两次）
    public void OnDefendButtonClick()
    {
        // 1. 停止随机气泡生成
        StopRandomRightBubbles();

        // 2. 隐藏普通按钮
        HideDefendButton();

        // 3. 发送左侧打断气泡（不等待，因为它不影响右侧）
        SendLeftBubbleWithInterruption();

        // 4. 发送右侧特殊语句，并记录气泡对象
        DialogueBubble specialBubble = null;
        if (defendNum == 0) // 第一次辩解
        {
            specialBubble = SendBubble(rightFather, specicalDialogue0, true);
        }
        else if (defendNum == 1) // 第二次辩解
        {
            specialBubble = SendBubble(rightFather, specicalDialogue1, true);
        }

        // 增加辩解次数
        defendNum++;
        // 6. 启动协程，等待特殊语句消失后，重启随机气泡
        StartCoroutine(WaitForSpecialBubbleAndRestartRandom(specialBubble));
        // 通知拼图系统重置拼图
        if (PuzzleManage.Instance != null)
        {
            PuzzleManage.Instance.ResetPuzzlesForNextRound(defendNum);
        }
    }
    private IEnumerator WaitForSpecialBubbleAndRestartRandom(DialogueBubble specialBubble)
    {

        yield return new WaitForSeconds(2f);

        // 重新启动随机气泡生成（跳过前两个固定对话，直接从随机库开始）
        StartRandomRightBubblesAfterFirstTwo();
    }
    // 特殊按钮点击（第三次）
    public void OnSpecialDefendButtonClick()
    {
        Debug.Log("特殊辩解按钮被点击");

        // 播放音效（预留）
        // AudioSource.PlayClipAtPoint(successClip, Camera.main.transform.position);

        // 发送左侧对话框 "....."
        SendBubble(leftFather, ".....",false);

        // 游戏结束或不再重置拼图，可禁用所有交互
        HideSpecialDefendButton();
        StopRandomRightBubbles(); // 停止右侧随机对话

        // 可在此触发结局或禁用拼图拖拽等
        // 例如：禁用所有拼图的拖拽
        Puzzle[] puzzles = FindObjectsOfType<Puzzle>();
        foreach (Puzzle p in puzzles)
        {
            p.enabled = false; // 禁用脚本，阻止拖拽
        }
    }
    public void ShowFinalBubble()
    {
        // 1. 停止随机对话
        StopRandomRightBubbles();

        // 2. 清除所有现有气泡（包括左右两侧的动态气泡，以及预设的 initialBubble/initialBubble2）
        ClearAllBubbles();

        // 3. 动态生成最终气泡
        if (bubblePrefab_right != null && rightFather != null)
        {
            GameObject finalBubbleObj = Instantiate(bubblePrefab_right, rightFather);
            DialogueBubble bubble = finalBubbleObj.GetComponent<DialogueBubble>();
            if (bubble != null)
            {
                // 使用新的显示流程：渐显 → 停留2秒 → 上浮
                bubble.ShowBubble(specicalDialogue2, stayDuration: 2f, moveDistance: 80f, floatDuration: 0.5f);
            }
            else
            {
                Debug.LogError("bubblePrefab 没有 DialogueBubble 脚本！");
                Destroy(finalBubbleObj, 2f);
            }
        }
        else
        {
            Debug.LogWarning("bubblePrefab 或 rightFather 未赋值，无法生成最终气泡！");
        }
    }
    ///清除所有在场的气泡（动态生成的 + 预设的 initialBubble, initialBubble2）
    private void ClearAllBubbles()
    {
        // 清除动态生成的气泡（在 leftFather 和 rightFather 下的动态气泡）
        ClearChildrenBubbles(leftFather);
        ClearChildrenBubbles(rightFather);
    }

    //清除指定父物体下所有子物体中带 DialogueBubble 脚本的对象
    private void ClearChildrenBubbles(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent)
        {
            DialogueBubble bubble = child.GetComponent<DialogueBubble>();
            if (bubble != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    //发送一个表示被打断的左侧气泡（可选）
    private void SendLeftBubbleWithInterruption()
    {
        string[] interruptionTexts = { "我...", "可是...", "那个...", "但是..." };
        string msg = interruptionTexts[Random.Range(0, interruptionTexts.Length)];
        SendBubble(leftFather, msg,false);
    }
}