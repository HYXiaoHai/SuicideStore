using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform targetPosition;//传送的目的地
    public bool canTeleport;//是否可以传送
    public CinemachineVirtualCamera virtualCamera;//
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("有碰撞");
        if (!canTeleport) return;
        
        if(collision.tag=="Player")
        {
            collision.transform.position = targetPosition.position;
            ReversalMange.Instance.ChangeCinemachine(virtualCamera);
        }
    }
}
