using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S11_3_Manager : MonoBehaviour
{
    [Header("下一场景")]
    public string nextSceneName;
    public float duration = 1f;//延迟
    [Header("引用")]
    public S11_3_LineConnector lineConnector;
    public CanvasGroup buttonCanvasGroup;//包含DialogFlip按钮的CanvasGroup

    private bool lineComplete = false;

    void Start()
    {
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
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.DOFade(1f, 0.5f);
            buttonCanvasGroup.interactable = true;
            buttonCanvasGroup.blocksRaycasts = true;
        }
        Debug.Log("连线完成，按钮已显示");
    }

    public IEnumerator OnDialogFlipComplete()
    {
        yield return new WaitForSeconds(duration);
        // 翻转完成后跳转场景
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("nextSceneName 未设置！");
    }
}