using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternInteractable : BaseInteractable
{
    [Header("传送门设置")]
    public Portal p; 

    [Header("传送冷却")]
    public float teleportCooldown = 0.5f;
    private float lastTeleportTime;
    private bool isTeleportSystemActive = false;  // 是否已激活

    public override void OnInteract()
    {
        if (isTeleportSystemActive) return; // 已激活，不再重复激活

        isTeleportSystemActive = true;
        p.canTeleport = true;
        // 关闭灯笼自身的交互，防止重复点击
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("传送系统已激活！现在触碰灯笼（入口）即可传送");
    }

    // 传送逻辑：直接挂载在灯笼物体上（作为入口传送门）
    //void OnTriggerEnter2D(Collider2D other)
    //{
    //    if (!isTeleportSystemActive) return;          // 未激活时不可传送
    //    if (exitPortal == null) return;
    //    if (!other.CompareTag("Player")) return;
    //    if (Time.time < lastTeleportTime + teleportCooldown) return;

    //    lastTeleportTime = Time.time;
    //    other.transform.position = exitPortal.position;
    //    Debug.Log("玩家已传送至出口");
    //}
}