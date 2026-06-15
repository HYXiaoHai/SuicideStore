using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class VideoIntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public CanvasGroup vieoCanvasGroup;
    public ElephantController player;
    public AudioClip audioClip;
    void Start()
    {
        AudioManager.Instance.Play2DSound(audioClip);

        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕，开始加载游戏场景...");
        player.GetComponent<SpriteRenderer>().DOFade(1f, 0.5f).OnComplete(() => { player.canMove = true;});
        vieoCanvasGroup.DOFade(0f, 0.6f).OnComplete(() => { vieoCanvasGroup.gameObject.SetActive(false); });
    }
}