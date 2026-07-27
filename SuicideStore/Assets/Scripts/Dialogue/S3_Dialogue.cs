using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class S6_Dialogue : MonoBehaviour
{
    public bool shouldUseFade = true;
    public bool shouldUseAudioFade = false;
    public Image backGround1;
    public Image backGround2;
    [Header("日记本交互")]
    public Button noteButton;
    public Image noteAni;
    public bool canDialogue = false;

    [Header("=== 场景里拖进来 ===")]
    public CanvasGroup dialogueCanvas;
    public TextMeshProUGUI dialogueText;
    public GameObject continueTip;
    public Image leleImage1;
    public Image leleImage2;

    [Header("=== 打字速度 ===")]
    public float typeSpeed = 0.12f;

    private List<string> lines = new List<string>()
    {
        "我：乐乐，现在你要选择离开，去往顿丘吗？",
        "乐乐：我……我要去！",
        "我：可是你看起来很爱你的爸爸妈妈啊？",
        "乐乐：我…"
    };

    private int index = 0;
    private bool isTyping = false;
    private Animator noteAnimator;      // 帧动画 Animator

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        noteButton.onClick.AddListener(PlayAnimation);
        canDialogue = false;
        dialogueText.gameObject.SetActive(false);
        dialogueCanvas.alpha = 0f;
        if (continueTip != null) continueTip.SetActive(false);

        if (noteAni != null)
            noteAnimator = noteAni.GetComponent<Animator>();
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!canDialogue) return;

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

    void PlayAnimation()
    {
        // 按钮渐隐
        noteButton.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            noteButton.interactable = false;
        });
        // 图片渐显
        noteAni.DOFade(1f, 1f).OnComplete(() =>
        {
            if (noteAnimator != null)
            {
                // 直接播放指定名字的动画状态（需确认状态名）
                noteAnimator.Play("NoteAnimation", 0, 0f);
                // 获取当前动画片段长度
                AnimatorStateInfo stateInfo = noteAnimator.GetCurrentAnimatorStateInfo(0);
                float animLength = stateInfo.length;
                // 等待动画播放完毕
                DOVirtual.DelayedCall(2.5f, OnAnimationFinished);
            }
            else
            {
                Debug.LogError("RawImage 上没有 Animator 组件！");
                OnAnimationFinished();
            }
        });
    }

    void OnAnimationFinished()
    {
        Debug.Log("帧动画播放完毕");
        // 渐隐 RawImage
        noteAni.DOFade(0f, 1f).OnComplete(() =>
        {
            canDialogue = true;
            dialogueText.gameObject.SetActive(true);
            StartType();
        });
        dialogueCanvas.DOFade(1f, 1f);
        backGround2.DOFade(1f, 1f);
        backGround1.DOFade(0f, 1f);
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
        if (index == 2)
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
        Debug.Log("对话已全部结束！");
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