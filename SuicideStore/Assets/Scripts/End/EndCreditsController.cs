using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class CreditsController : MonoBehaviour
{
    [Header("视频")]
    public CanvasGroup videoCanvas;
    public VideoPlayer videoPlayer;
    public float videoFadeDuration = 1f;
    public float blackScreenDelay = 2f;

    [Header("背景切换")]
    public Image bgImage;                // 背景图
    public Sprite[] bgSprites;           // 背景图片数组
    public float bgSwitchDuration = 3f;  // 每张图停留时间
    public float bgFadeDuration = 0.8f;  // 淡入淡出时长

    [Header("UI 组件")]
    public CanvasGroup timCanvas;
    public TextMeshProUGUI endLabel;
    public TextMeshProUGUI gameTitle;
    public TextMeshProUGUI groupTitle;
    public TextMeshProUGUI memberNames;
    public TextMeshProUGUI thanksText;
    public TextMeshProUGUI continuePrompt;

    [Header("显示时长")]
    public float endDisplayDuration = 2f;
    public float titleDisplayDuration = 3f;
    public float fadeDuration = 0.8f;
    public float displayDuration = 3.5f;

    [Header("跳过功能")]
    public KeyCode skipKey = KeyCode.Space;
    public bool canSkip = true;
    public string nextSceneName;

    private bool isSkipping = false;
    private Coroutine activeCoroutine;
    private bool videoSkipped = false;
    private Coroutine bgCoroutine;

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

        entries = new CreditsEntry[]
        {
            new CreditsEntry("-程序-", "24游技 董唤洋\r\n24游技 梁馨兮"),
            new CreditsEntry("-美术-", "24游艺 周珈怡\r\n24游艺 陈柔心"),
            new CreditsEntry("-策划-", "24数娱 白芮宁\r\n24数娱 刘艺浓"),
            new CreditsEntry("-特别感谢!-", "24动画 张文哲\r\n24计广 金子熙")
        };

        // 初始透明
        if (endLabel != null) endLabel.alpha = 0;
        if (gameTitle != null) gameTitle.alpha = 0;
        if (groupTitle != null) groupTitle.alpha = 0;
        if (memberNames != null) memberNames.alpha = 0;
        if (thanksText != null)
        {
            thanksText.alpha = 0;
            thanksText.text = "感谢你的体验与游玩！\r\n\r\n                      -- 顿丘 制作组";
        }
        if (continuePrompt != null)
        {
            continuePrompt.alpha = 0;
            continuePrompt.text = "[SPACE]";
        }
        //if (bgImage != null)
        //{
        //    bgImage.color = new Color(1, 1, 1, 0);
        //    if (bgSprites != null && bgSprites.Length > 0)
        //        bgImage.sprite = bgSprites[0];
        //}
        if (videoCanvas != null)
        {
            videoCanvas.alpha = 0;
        }

        activeCoroutine = StartCoroutine(PlayVideoThenCredits());
    }

    void Update()
    {
        if (canSkip && Input.GetKeyDown(skipKey) && !isSkipping)
        {
            SkipCredits();
        }
    }

    IEnumerator PlayVideoThenCredits()
    {
        // ----- 1. 播放视频 -----
        if (videoCanvas != null && videoPlayer != null)
        {
            if (!videoPlayer.isPrepared)
            {
                videoPlayer.Prepare();
                while (!videoPlayer.isPrepared)
                    yield return null;
            }

            videoCanvas.gameObject.SetActive(true);
            videoCanvas.alpha = 0;
            videoCanvas.DOFade(1f, videoFadeDuration).SetEase(Ease.Linear);
            yield return new WaitForSeconds(videoFadeDuration);

            videoPlayer.Play();
            yield return new WaitForSeconds(4f);
            // 如需等待视频完整播放，请取消下面注释
            // while (videoPlayer.isPlaying && !videoSkipped)
            //     yield return null;

            if (videoSkipped)
                videoPlayer.Stop();

            videoCanvas.DOFade(0f, videoFadeDuration).SetEase(Ease.Linear);
            yield return new WaitForSeconds(videoFadeDuration);
            videoCanvas.gameObject.SetActive(false);
            yield return new WaitForSeconds(blackScreenDelay);
        }
        else
        {
            Debug.Log("没有配置视频，直接进入字幕");
        }

        // ----- 2. 执行字幕逻辑 -----
        yield return StartCoroutine(PlayCreditsSubRoutine());
    }

    IEnumerator PlayCreditsSubRoutine()
    {
        //// 启动背景切换
        //if (bgImage != null && bgSprites != null && bgSprites.Length > 0)
        //{
        //    bgCoroutine = StartCoroutine(SwitchBackground());
        //}

        timCanvas.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration+1f);

        //// 0. END
        //if (endLabel != null)
        //{
        //    endLabel.text = "--END--";
        //    endLabel.DOFade(1f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //    yield return new WaitForSeconds(endDisplayDuration);
        //    endLabel.DOFade(0f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //}

        //// 1. 标题
        //if (gameTitle != null)
        //{
        //    gameTitle.text = "-顿丘-";
        //    gameTitle.DOFade(1f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //    yield return new WaitForSeconds(titleDisplayDuration);
        //    gameTitle.DOFade(0f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //}

        //// 2. 名单
        //foreach (var entry in entries)
        //{
        //    groupTitle.text = entry.title;
        //    memberNames.text = entry.names;

        //    groupTitle.DOFade(1f, fadeDuration);
        //    memberNames.DOFade(1f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //    yield return new WaitForSeconds(displayDuration);
        //    groupTitle.DOFade(0f, fadeDuration);
        //    memberNames.DOFade(0f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //}

        //// 3. 感谢语
        //if (thanksText != null)
        //{
        //    thanksText.DOFade(1f, fadeDuration);
        //    yield return new WaitForSeconds(fadeDuration);
        //}

        // 4. 按键提示
        if (continuePrompt != null)
        {
            continuePrompt.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        //// 停止背景切换并淡出
        //if (bgCoroutine != null)
        //{
        //    StopCoroutine(bgCoroutine);
        //    bgCoroutine = null;
        //}
        //if (bgImage != null)
        //{
        //    bgImage.DOFade(0f, fadeDuration);
        //}

        // 等待玩家按键
        yield return new WaitUntil(() => Input.GetKeyDown(skipKey));
        yield return null;

        OnCreditsFinished();
    }

    IEnumerator SwitchBackground()
    {
        if (bgSprites == null || bgSprites.Length == 0) yield break;
        int index = 0;
        while (true)
        {
            bgImage.sprite = bgSprites[index];
            bgImage.DOFade(1f, bgFadeDuration);
            yield return new WaitForSeconds(bgFadeDuration);
            yield return new WaitForSeconds(bgSwitchDuration);
            bgImage.DOFade(0f, bgFadeDuration);
            yield return new WaitForSeconds(bgFadeDuration);
            index = (index + 1) % bgSprites.Length;
        }
    }

    void SkipCredits()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        if (isSkipping) return;
        isSkipping = true;

        videoSkipped = true;

        // 杀死所有动画
        if (endLabel != null) endLabel.DOKill();
        if (gameTitle != null) gameTitle.DOKill();
        if (groupTitle != null) groupTitle.DOKill();
        if (memberNames != null) memberNames.DOKill();
        if (thanksText != null) thanksText.DOKill();
        if (continuePrompt != null) continuePrompt.DOKill();
        if (videoCanvas != null)
        {
            videoCanvas.DOKill();
            videoCanvas.gameObject.SetActive(false);
        }
        //if (bgImage != null)
        //{
        //    bgImage.DOKill();
        //    bgImage.color = new Color(1, 1, 1, 0);
        //}
        //if (bgCoroutine != null)
        //{
        //    StopCoroutine(bgCoroutine);
        //    bgCoroutine = null;
        //}

        if (endLabel != null) endLabel.gameObject.SetActive(false);
        if (gameTitle != null) gameTitle.gameObject.SetActive(false);
        if (groupTitle != null) groupTitle.gameObject.SetActive(false);
        if (memberNames != null) memberNames.gameObject.SetActive(false);

        if (thanksText != null) thanksText.alpha = 1;
        if (continuePrompt != null) continuePrompt.alpha = 1;

        //StartCoroutine(QuickFinish());
        OnCreditsFinished();
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