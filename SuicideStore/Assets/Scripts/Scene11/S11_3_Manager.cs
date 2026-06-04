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
        
        if (lineConnector == null)
            lineConnector = GetComponent<S11_3_LineConnector>();
    }

    public bool IsLineComplete()
    {
        return currentPhase != Phase.WaitConnect;
    }

    public void OnLineConnectComplete()
    {
        if (currentPhase != Phase.WaitConnect) return;
        
        currentPhase = Phase.WaitDialogClick;
        Debug.Log("连线完成！现在可以点击对话框了");
    }

    public void OnDialogClicked(S11_3_DialogFlip dialog)
    {
        if (currentPhase == Phase.WaitDialogClick)
        {
            currentDialog = dialog;
            dialog.ScaleToCenter();
            currentPhase = Phase.WaitFlip;
        }
        else if (currentPhase == Phase.WaitFlip && dialog == currentDialog)
        {
            dialog.Flip();
            currentPhase = Phase.Complete;
        }
    }

    void Update()
    {
        if (currentPhase == Phase.Complete)
        {
            if (currentDialog != null && currentDialog.isFlipped)
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                    SceneManager.LoadScene(nextSceneName);
                else
                    Debug.LogWarning("nextSceneName 未设置！");
            }
        }
    }
}
