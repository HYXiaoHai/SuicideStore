using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public Transform targetPosition;//传送的目的地
    public bool canTeleport;//是否可以传送
    public CinemachineVirtualCamera virtualCamera;//下一场景的camera
    public int nextLevel;

    [Header("下一关的传送")]
    public bool shouldUseFadeOutBGM = false;
    public string nextScence;//下一个场景的传送

    public bool isScenecsPortal = false;//是否是场景传送门
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("有碰撞");
        if (!canTeleport) return;
        
        if(collision.tag=="Player")
        {
            if(isScenecsPortal == false)
            {
                collision.transform.position = targetPosition.position;
                if (ReversalMange.Instance != null)
                {
                    ReversalMange.Instance.ChangeCinemachine(virtualCamera);
                    ReversalMange.Instance.InitLevel(nextLevel);
                }
            }
            else
            {
                //CompleteLevel();
                // 并行执行转场淡出和 BGM 淡出
                TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                {
                    // 转场完成后加载新场景
                    SceneManager.LoadScene(nextScence);
                });
            }
        }
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
                if (shouldUseFadeOutBGM)
                {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        // 转场完成后加载新场景
                        SceneManager.LoadScene(nextScene);
                    });
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
