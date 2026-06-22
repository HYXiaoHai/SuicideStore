using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class DoorTrigger : MonoBehaviour
{
    public bool canLoad = false;
    [Header("灯光")]
    public Light2D[] lights;
    [Header("场景设置")]
    public bool isScene12 = false;
    public string nextSceneName;
    public bool shouldUseFade = false;
    public bool shouldUseAudioFade = false;
    public float fadeDuration = 0.5f;
    public Color fadeColor = Color.white;

    [Header("视频")]
    public CanvasGroup videoCanvasGroup;   // 包含 VideoPlayer 的 CanvasGroup
    public VideoPlayer videoPlayer;         // 视频播放器组件
    public float videoFadeDuration = 0.5f; // 视频淡入淡出时长

    private bool isProcessing = false;      // 防止重复触发

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canLoad && !isProcessing)
        {
            EnterDoor();
        }
    }

    void EnterDoor()
    {
        isProcessing = true;

        // 如果配置了视频，则播放视频，否则直接跳转
        if (videoCanvasGroup != null && videoPlayer != null)
        {
            StartCoroutine(PlayVideoAndTransition());
        }
        else
        {
            // 无视频：直接执行跳转逻辑
            PerformTransition();
        }
    }

    private System.Collections.IEnumerator PlayVideoAndTransition()
    {
        // 准备视频（如果未准备）
        if (!videoPlayer.isPrepared)
        {
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;
        }

        // 渐显视频
        videoCanvasGroup.alpha = 0;
        videoCanvasGroup.gameObject.SetActive(true);
        videoCanvasGroup.DOFade(1f, videoFadeDuration).SetEase(Ease.Linear);
        yield return new WaitForSeconds(videoFadeDuration);

        // 播放视频
        videoPlayer.Play();

        // 等待视频播放结束
        while (videoPlayer.isPlaying)
            yield return null;

        //// 视频结束，淡出视频
        //videoCanvasGroup.DOFade(0f, videoFadeDuration).SetEase(Ease.Linear);
        //yield return new WaitForSeconds(videoFadeDuration);
        //videoCanvasGroup.gameObject.SetActive(false);

        // 执行场景跳转逻辑
        PerformTransition();
    }

    private void PerformTransition()
    {

        foreach (var l in lights)
        {
            if (l != null)
            {
                Debug.Log("隐藏" + l.gameObject);
                l.gameObject.SetActive(false);
            }
        }

        // 根据 isScene12 或其他条件决定跳转方式
        //if (isScene12 || !string.IsNullOrEmpty(nextSceneName))
        if (isScene12)
        {
            if (shouldUseFade)
            {
                TransitionManage.Instance.FadeOut(fadeDuration, fadeColor, () =>
                {
                    if (!string.IsNullOrEmpty(nextSceneName))
                        SceneManager.LoadScene(nextSceneName);
                });
                if (shouldUseAudioFade)
                    AudioManager.Instance.FadeOutCurrentBGM(fadeDuration, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            // 默认下一关逻辑（兼容旧版 CompleteLevel）
            CompleteLevel();
        }
    }

    public void CompleteLevel()
    {
        Debug.Log("跳转");
        GameManage.Instance.CompleteCurrentLevel();
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (shouldUseFade)
                {
                    TransitionManage.Instance.FadeOut(fadeDuration, fadeColor, () =>
                    {
                        SceneManager.LoadScene(nextScene);
                    });
                    if (shouldUseAudioFade)
                        AudioManager.Instance.FadeOutCurrentBGM(fadeDuration, null);
                }
                else
                {
                    SceneManager.LoadScene(nextScene);
                }
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
}