using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class LanternInteractable : BaseInteractable
{
    [Header("传送门设置")]
    public LanternPortal p;
    [Header("灯")]
    public Light2D mylight;
    [Header("传送冷却")]
    public float teleportCooldown = 0.5f;
    private float lastTeleportTime;
    private bool isTeleportSystemActive = false;  // 是否已激活

    [Header("交互范围（需手动拖拽）")]
    public InteractRange interactRange;   // 关联的交互区域
    private void Start()
    {
        mylight.intensity = 0f;
    }
    public override void OnInteract()
    {
        if (isTeleportSystemActive) return; // 已激活，不再重复激活

        isTeleportSystemActive = true;
        p.canTeleport = true;
        // 灯光渐显
        DOTween.Kill(mylight);
        DOTween.To(() => mylight.intensity,
                   x => mylight.intensity = x,
                   2f,   // 目标值，这里改为1
                   0.5f) // 持续时间
               .SetEase(Ease.OutQuad); 
        // 关闭灯笼自身的交互，防止重复点击
        GetComponent<Collider2D>().enabled = false;
        // 永久关闭交互提示
        if (interactRange != null)
            interactRange.DisablePrompt();
        Debug.Log("传送系统已激活！现在触碰灯笼（入口）即可传送");
    }
}