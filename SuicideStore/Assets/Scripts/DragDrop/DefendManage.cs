using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class DefendManage : MonoBehaviour
{
    [Header("气泡对话")]
    public Transform leftFather;//左气泡出生父物体
    public RectTransform leftDialogueShotStart;//左气泡出生位置
    public RectTransform leftDialogueShotEnd;//左气泡到达位置
    public Transform rightFather;//右气泡出生父物体
    public RectTransform rightDialogueShotStart;//左气泡出生位置
    public RectTransform rightDialogueShotEnd;//左气泡到达位置

    public GameObject bubblePrefab_right;//气泡预制体
    public GameObject bubblePrefab_left;//气泡预制体
    public float minInterval = 1f;//右侧持续生成气泡的最小时间间隔
    public float maxInterval = 1.5f;//右侧持续生成气泡的最大时间间隔
    public string[] dialogues;//右侧随机对话的库存
    public string specicalDialogue0 = "学点好";
    public string specicalDialogue1 = "懒得跟你犟";
    public string specicalDialogue2 = "好了，别讲了！赶紧回家";

    [Header("特殊气泡")]
    public GameObject initialBubble;    //预先放在右侧的气泡
    public GameObject initialBubble2;    //预先放在右侧的气泡
    public GameObject finalBubblePrefab_right;//最终气泡预制体


    [Header("辩解模块")]
    public static DefendManage Instance;
    public Button defendButton;          // 普通辩解按钮
    public Button specialDefendButton;   // 特殊辩解按钮（第三次出现）
    public int defendNum = 0;

    [Header("转场")]
    public string nextSceneName;
    public CanvasGroup defendCanvasGroup;

    [Header("音效")]
    public AudioClip defendClip;

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

        StartCoroutine(OpeningBubblesSequence());
    }
    private IEnumerator OpeningBubblesSequence()
    {
        yield return new WaitForSeconds(0.5f);

        if (initialBubble != null)
        {
            DialogueBubble bubble1 = initialBubble.GetComponent<DialogueBubble>();
            if (bubble1 != null)
            {
                bool moveComplete = false;
                bubble1.PlayMoveAnimation(rightDialogueShotEnd, 2f, () => moveComplete = true);
                yield return new WaitUntil(() => moveComplete);
            }
            else
            {
                initialBubble.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
                yield return new WaitForSeconds(0.5f);
                Destroy(initialBubble);
            }
        }

        //显示 initialBubble2（入场动画 → 停留1秒 → 上升）
        if (initialBubble2 != null)
        {
            RectTransform start = rightDialogueShotStart;
            RectTransform end = rightDialogueShotEnd;
            RectTransform bubble2Rect = initialBubble2.GetComponent<RectTransform>();
            if (bubble2Rect != null && start != null)
            {
                bubble2Rect.anchoredPosition = start.anchoredPosition;
            }

            DialogueBubble bubble2 = initialBubble2.GetComponent<DialogueBubble>();
            if (bubble2 != null)
            {
                bubble2.PlayEnterAnimation(() =>
                {
                    // 停留1秒后开始上升
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        bubble2.PlayMoveAnimation(end, 2f);
                    });
                });
                // 等待 initialBubble2 被销毁（动画结束）
                yield return new WaitUntil(() => initialBubble2 == null);
            }
            else
            {
             
                CanvasGroup cg = initialBubble2.GetComponent<CanvasGroup>();
                cg.alpha = 0;
                cg.DOFade(1, 0.3f);
                yield return new WaitForSeconds(1f);
                cg.DOFade(0, 0.5f);
                yield return new WaitForSeconds(0.5f);
                Destroy(initialBubble2);
            }
        }

        // 3. 后续流程：启用拼图区域、启动随机对话
        if (PuzzleManage.Instance != null)
        {
            PuzzleManage.Instance.ShowPuzzleArea();
        }
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
                SendBubble(rightFather, randomMsg,true,1f);
            }
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }


    //生成气泡
    public DialogueBubble SendBubble(Transform parent, string content,bool isright,float scale,float time = 2f)
    {
        if ((isright && bubblePrefab_right == null) || (!isright && bubblePrefab_left == null) || parent == null)
            return null;

        GameObject bubbleObj = isright ? Instantiate(bubblePrefab_right, parent) : Instantiate(bubblePrefab_left, parent);
        DialogueBubble bubble = bubbleObj.GetComponent<DialogueBubble>();
        if (bubble != null)
        {
            // 根据左右选择起始点和终点
            RectTransform start = isright ? rightDialogueShotStart : leftDialogueShotStart;
            RectTransform end = isright ? rightDialogueShotEnd : leftDialogueShotEnd;
            // 动画时长可调，这里设为 1 秒
            bubble.AnimateBubble(content, start, end, time, scale);
        }
        else
        {
            Destroy(bubbleObj, 3f);
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
        //点击音效
        AudioManager.Instance.PlayShortSound(defendClip, 0.8f);

        // 1. 停止随机气泡生成
        StopRandomRightBubbles();

        // 2. 隐藏普通按钮
        HideDefendButton();

        // 3. 发送左侧打断气泡（不等待，因为它不影响右侧）
        SendLeftBubbleWithInterruption(defendNum);

        // 4. 发送右侧特殊语句，并记录气泡对象
        DialogueBubble specialBubble = null;
        if (defendNum == 0) // 第一次辩解
        {
            specialBubble = SendBubble(rightFather, specicalDialogue0, true,1.3f);
        }
        else if (defendNum == 1) // 第二次辩解
        {
            specialBubble = SendBubble(rightFather, specicalDialogue1, true,1.4f);
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
        AudioManager.Instance.PlayShortSound(defendClip, 0.8f);

        StartCoroutine(loadScence());
    }
    public IEnumerator loadScence()
    {
        SendBubble(leftFather, ".....", false, 1f,3.5f);

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
        defendCanvasGroup.DOFade(0f,3.5f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(4.5f);
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }
    public void ShowFinalBubble()
    {
        // 1. 停止随机对话
        StopRandomRightBubbles();

        // 2. 清除所有现有气泡（包括左右两侧的动态气泡，以及预设的 initialBubble/initialBubble2）
        ClearAllBubbles();

        // 3. 使用 finalBubblePrefab_right 生成最终气泡
        if (finalBubblePrefab_right != null && rightFather != null)
        {
            GameObject finalBubbleObj = Instantiate(finalBubblePrefab_right, rightFather);
            DialogueBubble bubble = finalBubbleObj.GetComponent<DialogueBubble>();
            if (bubble != null)
            {
                bubble.AnimateBubble(specicalDialogue2, rightDialogueShotStart, rightDialogueShotEnd, 3f,1.5f);
            }
            else
            {
                Destroy(finalBubbleObj, 2f);
            }
        }
        else
        {
            Debug.LogWarning("finalBubblePrefab_right 或 rightFather 未赋值，无法生成最终气泡！");
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
                bubble.DestroyBubble();

                //Destroy(child.gameObject);
            }
        }
    }
    //发送一个表示被打断的左侧气泡（可选）
    private void SendLeftBubbleWithInterruption(int n)
    {
        string[] interruptionTexts = { "动画片里....","妈妈我....", "......"};
        string msg = interruptionTexts[n];
        SendBubble(leftFather, msg,false, 1f);
    }

}