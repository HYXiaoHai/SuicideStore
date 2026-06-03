using UnityEngine;
using UnityEngine.SceneManagement;

public class S11_3_Manager : MonoBehaviour
{
    [Header("下一场景")]
    public string nextSceneName;

    [Header("引用")]
    public S11_3_LineConnector lineConnector;
    public S11_3_DialogFlip[] dialogBoxes;

    private enum Phase { WaitConnect, WaitDialogClick, WaitFlip, Complete }
    private Phase currentPhase = Phase.WaitConnect;
    private S11_3_DialogFlip currentDialog;

    void Start()
    {
        currentPhase = Phase.WaitConnect;
        
        // 初始化对话框
        if (dialogBoxes != null)
        {
            foreach (var dialog in dialogBoxes)
            {
                if (dialog != null)
                    dialog.ResetToOriginal();
            }
        }
    }

    // 连线完成回调
    public void OnLineConnectComplete()
    {
        if (currentPhase != Phase.WaitConnect) return;
        
        currentPhase = Phase.WaitDialogClick;
        Debug.Log("连线完成！现在可以点击对话框了");
    }

    // 对话框被点击回调
    public void OnDialogClicked(S11_3_DialogFlip dialog)
    {
        if (currentPhase == Phase.WaitDialogClick)
        {
            // 第一次点击：放大到中央
            currentDialog = dialog;
            dialog.ScaleToCenter();
            currentPhase = Phase.WaitFlip;
        }
        else if (currentPhase == Phase.WaitFlip && dialog == currentDialog)
        {
            // 第二次点击：翻转
            dialog.Flip();
            currentPhase = Phase.Complete;
        }
    }

    // 检查是否完成
    void Update()
    {
        if (currentPhase == Phase.Complete)
        {
            // 翻转完成后等一会儿跳转
            if (currentDialog != null && currentDialog.isFlipped)
            {
                // 可以加个延迟再跳转
                // 这里直接跳转
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                else
                {
                    Debug.LogWarning("nextSceneName 未设置！");
                }
            }
        }
    }
}
