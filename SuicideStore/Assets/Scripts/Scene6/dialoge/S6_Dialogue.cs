using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S3_Dialogue : MonoBehaviour
{
    public bool shouldUseFade = true;
    public bool shouldUseAudioFade = false;
    [Header("=== 场景里拖进来 ===")]
    public TextMeshProUGUI dialogueText;   // 对话文字
    public GameObject continueTip;         // 继续提示（可选）

    [Header("=== 打字速度 ===")]
    public float typeSpeed = 0.12f;
    [Header("交付日记本音效")]
    public AudioClip audioClip;
    // ======================
    // 你的 S3 对话内容 直接写这里
    // ======================
    private List<string> lines = new List<string>()
    {
        "乐乐：故事到这里就结束了，我的东西在哪里？",
        "列车员（我）：这是一本专属于你的日记本，你可以用它看到爸爸妈妈在想什么..."
    };

    private int index = 0;
    private bool isTyping = false;

    void Start()
    {
        // 游戏开始自动播放第一句对话
        if (TransitionManage.Instance != null)
        {
            TransitionManage.Instance.FadeIn(0.5f, Color.black);
        }
        StartType();
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        // 点击鼠标左键继续
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 正在打字时点击：直接显示完整句子
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
                ShowContinue(true);
            }
            else
            {
                // 打完一句，点击进入下一句
                NextLine();
            }
        }
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
            // 所有对话打完，只显示结束提示，不跳转
            EndDialogue();
            return;
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
        // 对话结束：隐藏继续提示，或者你可以在这里加自己的收尾逻辑
        AudioManager.Instance.Play2DSound(audioClip, 0.8f);
        ShowContinue(false);
        Debug.Log("S3 对话已全部结束！");
        CompleteLevel();
        // 这里什么都不写，对话结束就停在原地，不影响场景
    }
    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (shouldUseFade)
                {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(0.5f, Color.white, () =>
                    {
                        // 转场完成后加载新场景
                        SceneManager.LoadScene(nextScene);
                    });
                    if (shouldUseAudioFade)
                        AudioManager.Instance.FadeOutCurrentBGM(0.5f, null);
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
