using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternInteractable : BaseInteractable
{
    [Header("传送门设置")]
    public LanternPortal p; 

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
}