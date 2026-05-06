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
                SceneManager.LoadScene(nextScence);
            }
        }
    }
}
