using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class S6_Dialogue : MonoBehaviour
{
    public bool shouldUseFade = true;
    public bool shouldUseAudioFade = false;

    [Header("日记本交互")]
    public Button noteButton;
    public RawImage noteAni;
    public bool canDialogue = false;

    [Header("=== 场景里拖进来 ===")]
    public CanvasGroup dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public GameObject continueTip;
    public Image leleImage1;
    public Image leleImage2;

    [Header("=== 打字速度 ===")]
    public float typeSpeed = 0.12f;
    [Header("交付日记本音效")]

    private List<string> lines = new List<string>()
    {
        "列车员（我）：乐乐，现在你要选择离开，去往顿丘吗？",
        "象乐乐：我……我要去！",
        "列车员（我）：可是你看起来很爱你的爸爸妈妈啊？",
        "象乐乐：我…"
    };

    private int index = 0;
    private bool isTyping = false;
    private VideoPlayer videoPlayer;     // 缓存 VideoPlayer 组件

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        noteButton.onClick.AddListener(PlayVideo);
        // 初始状态：不能对话，等待视频播放完
        canDialogue = false;
        dialogueText.gameObject.SetActive(false);
        dialogueCanvas.alpha = 0f;
        if (continueTip != null) continueTip.SetActive(false);

        // 获取 RawImage 上的 VideoPlayer 组件
        if (noteAni != null)
            videoPlayer = noteAni.GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;

            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Prepare();
        }
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!canDialogue) return;  // 未开启对话时禁止点击

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
                ShowContinue(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    void PlayVideo()
    {
        // 开始播放视频：按钮渐隐，RawImage 渐显，播放视频
        noteButton.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            noteButton.interactable = false;
        });
        noteAni.DOFade(1f, 1f).OnComplete(() =>
        {
            if (videoPlayer != null)
                videoPlayer.Play();
            else
                Debug.LogError("RawImage 上没有 VideoPlayer 组件！");
        });
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        Debug.Log("视频已准备就绪");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完毕");
        // 渐隐 RawImage
        noteAni.DOFade(0f, 1f).OnComplete(() =>
        {
            // 开启对话
            canDialogue = true;
            dialogueText.gameObject.SetActive(true);
            StartType();  // 开始第一句对话
        });
        dialogueCanvas.DOFade(1f, 1f);
    }

    void StartType()
    {
        isTyping = true;
        ShowContinue(false);
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        dialogueText.text = "";
        foreach (char c in lines[index])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        ShowContinue(true);
    }

    void NextLine()
    {
        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }
        if(index==2)
        {
            leleImage1.DOFade(0f, 0.5f).SetEase(Ease.InQuart);
            leleImage2.DOFade(1f, 0.5f).SetEase(Ease.OutQuart);
        }
        StartType();
    }

    void ShowContinue(bool show)
    {
        if (continueTip != null)
            continueTip.SetActive(show);
    }

    void EndDialogue()
    {
        ShowContinue(false);
        Debug.Log("S3 对话已全部结束！");
        CompleteLevel();
    }

    public void CompleteLevel()
    {
        GameManage.Instance.CompleteCurrentLevel();
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (shouldUseFade)
                {
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        SceneManager.LoadScene(nextScene);
                    });
                    if (shouldUseAudioFade)
                        AudioManager.Instance.FadeOutCurrentBGM(1f, null);
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