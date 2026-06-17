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

    [Header("UI 组件")]
    public Image bgImage;
    public CanvasGroup timCanvas; // 制作组面板（可选，用于整体控制）
    public TextMeshProUGUI endLabel;
    public TextMeshProUGUI gameTitle;
    public CanvasGroup[] timNameCanvas; // 人名单组（按数组顺序逐个展示）
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

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.black);

        // 初始透明
        if (endLabel != null) endLabel.alpha = 0;
        if (gameTitle != null) gameTitle.alpha = 0;
        if (thanksText != null)
        {
            thanksText.alpha = 0;
            thanksText.text = "感谢您的体验与游玩！\r\n\r\n                      -- 顿丘 制作组";
        }
        if (continuePrompt != null)
        {
            continuePrompt.alpha = 0;
            continuePrompt.text = "[SPACE]";
        }
        // 初始化所有人名 CanvasGroup 为透明
        foreach (var cg in timNameCanvas)
        {
            if (cg != null) cg.alpha = 0;
        }

        // 初始隐藏视频
        if (videoCanvas != null)
        {
            videoCanvas.alpha = 0;
        }

        activeCoroutine = StartCoroutine(PlayVideoThenCredits());
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
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
            // 等待视频播放结束或跳过
            while (videoPlayer.isPlaying && !videoSkipped)
                yield return null;

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
        // 整体面板淡入（如果有）
        if (timCanvas != null)
            timCanvas.DOFade(1f, fadeDuration);

        yield return new WaitForSeconds(fadeDuration);

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

        // 2. 逐条显示制作名单（按数组顺序）
        foreach (CanvasGroup cg in timNameCanvas)
        {
            if (cg == null) continue;
            cg.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(displayDuration);
            cg.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 3. 显示感谢语
        if (thanksText != null)
        {
            thanksText.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(displayDuration);
            thanksText.DOFade(0f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);

        }

        // 4. 背景图淡入（如果有）
        if (bgImage != null)
        {
            bgImage.DOFade(1f, fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        // 5. 显示按键提示
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

        videoSkipped = true;

        // 杀死所有动画
        if (endLabel != null) endLabel.DOKill();
        if (gameTitle != null) gameTitle.DOKill();
        if (thanksText != null) thanksText.DOKill();
        if (continuePrompt != null) continuePrompt.DOKill();
        if (videoCanvas != null)
        {
            videoCanvas.DOKill();
            videoCanvas.gameObject.SetActive(false);
        }
        // 杀死所有人名 CanvasGroup 的动画
        foreach (var cg in timNameCanvas)
        {
            if (cg != null) cg.DOKill();
        }

        // 隐藏非必要的文本
        if (endLabel != null) endLabel.gameObject.SetActive(false);
        if (gameTitle != null) gameTitle.gameObject.SetActive(false);
        // 隐藏所有人名 CanvasGroup
        foreach (var cg in timNameCanvas)
        {
            if (cg != null) cg.gameObject.SetActive(false);
        }

        // 直接显示感谢语和提示
        if (thanksText != null) thanksText.alpha = 0;

        if (bgImage != null) bgImage.color = new Color(1,1,1,1);
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
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeOut(1f, Color.black, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        else
            SceneManager.LoadScene(nextSceneName);
    }
}