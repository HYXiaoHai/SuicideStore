using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [Header("UI 组件")]
    public TextMeshProUGUI endLabel;            // 新增：显示 "--END--"
    public TextMeshProUGUI gameTitle;          // 显示“顿丘”
    public TextMeshProUGUI groupTitle;         // 显示“程序组”等标题
    public TextMeshProUGUI memberNames;        // 显示具体成员姓名+职责
    public TextMeshProUGUI thanksText;         // 单独的感谢语
    public TextMeshProUGUI continuePrompt;     // 按键提示（例如“按 空格 继续”）

    [Header("显示时长")]
    public float endDisplayDuration = 2f;       // END 标语停留时长
    public float titleDisplayDuration = 3f;
    public float fadeDuration = 0.8f;
    public float displayDuration = 3.5f;

    [Header("跳过功能")]
    public KeyCode skipKey = KeyCode.Space;
    public bool canSkip = true;
    public string nextSceneName;

    private bool isSkipping = false;
    private Coroutine activeCoroutine;

    private struct CreditsEntry
    {
        public string title;
        public string names;
        public CreditsEntry(string t, string n) { title = t; names = n; }
    }

    private CreditsEntry[] entries;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.black);

        // 初始化名单
        entries = new CreditsEntry[]
        {
            new CreditsEntry("-程序-", "24游技 董唤洋\r\n24游技 梁馨兮 "),
            new CreditsEntry("-美术-", "24游艺 周珈怡\r\n24游艺 陈柔心\r\n24动画 张文哲 "),
            new CreditsEntry("-策划-", "24数娱 白芮宁\r\n24数娱 刘艺浓")
        };

        // 初始透明
        if (endLabel != null) endLabel.alpha = 0;
        if (gameTitle != null) gameTitle.alpha = 0;
        if (groupTitle != null) groupTitle.alpha = 0;
        if (memberNames != null) memberNames.alpha = 0;
        if (thanksText != null)
        {
            thanksText.alpha = 0;
            thanksText.text = "感谢你的游玩！\r\n\r\n                      -- 顿丘 制作组";
        }
        if (continuePrompt != null)
        {
            continuePrompt.alpha = 0;
            continuePrompt.text = "[SPACE]";
        }

        activeCoroutine = StartCoroutine(PlayCredits());
    }

    void Update()
    {
        if (canSkip && Input.GetKeyDown(skipKey) && !isSkipping)
        {
            SkipCredits();
        }
    }

    IEnumerator PlayCredits()
    {
        // 0. 显示 "--END--"
        if (endLabel != null)
        {
            endLabel.text = "--END--";
            endLabel.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(endDisplayDuration);
            endLabel.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 1. 显示标题“顿丘”
        if (gameTitle != null)
        {
            gameTitle.text = "-顿丘-";
            gameTitle.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(titleDisplayDuration);
            gameTitle.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 2. 逐条显示制作名单
        foreach (var entry in entries)
        {
            groupTitle.text = entry.title;
            memberNames.text = entry.names;

            groupTitle.DOFade(1f, fadeDuration);
            memberNames.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(displayDuration);
            groupTitle.DOFade(0f, fadeDuration);
            memberNames.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3. 显示感谢语
        if (thanksText != null)
        {
            thanksText.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 4. 显示按键提示
        if (continuePrompt != null)
        {
            continuePrompt.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 等待玩家按键
        yield return new WaitUntil(() => Input.GetKeyDown(skipKey));
        yield return null;

        OnCreditsFinished();
    }

    void SkipCredits()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        if (isSkipping) return;
        isSkipping = true;

        // 杀死所有动画
        if (endLabel != null) endLabel.DOKill();
        if (gameTitle != null) gameTitle.DOKill();
        if (groupTitle != null) groupTitle.DOKill();
        if (memberNames != null) memberNames.DOKill();
        if (thanksText != null) thanksText.DOKill();
        if (continuePrompt != null) continuePrompt.DOKill();

        // 隐藏非必要的文本
        if (endLabel != null) endLabel.gameObject.SetActive(false);
        if (gameTitle != null) gameTitle.gameObject.SetActive(false);
        if (groupTitle != null) groupTitle.gameObject.SetActive(false);
        if (memberNames != null) memberNames.gameObject.SetActive(false);

        // 直接显示感谢语和提示
        if (thanksText != null) thanksText.alpha = 1;
        if (continuePrompt != null) continuePrompt.alpha = 1;

        StartCoroutine(QuickFinish());
    }

    IEnumerator QuickFinish()
    {
        yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(skipKey));
        OnCreditsFinished();
    }

    void OnCreditsFinished()
    {
        TransitionManage.Instance.FadeOut(1f, Color.black, () =>
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }
}