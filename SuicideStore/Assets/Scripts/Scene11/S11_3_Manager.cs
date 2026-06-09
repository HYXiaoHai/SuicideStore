using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S11_3_Manager : MonoBehaviour
{
    [Header("下一场景")]
    public string nextSceneName;
    public float duration = 1f;//延迟
    [Header("引用")]
    public S11_3_LineConnector lineConnector;
    public SpriteRenderer lineBGSprite;
    public CanvasGroup buttonCanvasGroup;//包含DialogFlip按钮的CanvasGroup
    [Header("气泡")]
    public DialogueBubble_old[] dialogueBubbles;
    public S11_3_DialogFlip dialogFlip;

    private bool lineComplete = false;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.white);

        if (lineConnector == null)
            lineConnector = GetComponent<S11_3_LineConnector>();

        // 初始隐藏按钮
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 0f;
            buttonCanvasGroup.interactable = false;
            buttonCanvasGroup.blocksRaycasts = false;
        }
    }

    public bool IsLineComplete()
    {
        return lineComplete;
    }

    public void OnLineConnectComplete()
    {
        lineComplete = true;
        // 显示按钮（渐显 + 可点击）
        //lineBGSprite.DOColor(Color.white, 0.5f);
        lineBGSprite.DOFade(0f,0.5f);
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.DOFade(1f, 0.5f);
            buttonCanvasGroup.interactable = true;
            buttonCanvasGroup.blocksRaycasts = true;
        }
        Debug.Log("连线完成，按钮已显示");
        StartCoroutine(FloatBubble());
    }
    public IEnumerator FloatBubble()
    {
        yield return new WaitForSeconds(1f);
        dialogFlip.PlayEnterAnimation();
        foreach (var bubble in dialogueBubbles)
        {
            bubble.AnimateBubble("",bubble.startPosition,bubble.endPosition,bubble.duration);
        }
    }
    public IEnumerator OnDialogFlipComplete()
    {
        yield return new WaitForSeconds(duration);
        // 翻转完成后跳转场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            TransitionManage.Instance.FadeOut(0.5f, Color.black, () =>
            {
                // 转场完成后加载新场景
                SceneManager.LoadScene(nextSceneName);

            });
        }
        else
            Debug.LogWarning("nextSceneName 未设置！");
    }
}