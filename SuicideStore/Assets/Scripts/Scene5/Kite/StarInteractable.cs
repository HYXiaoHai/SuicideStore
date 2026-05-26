using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarInteractable : BaseInteractable
{
    public GameObject branchToTransform; // 需要旋转和平移的树枝
    public Vector3 targetRotation;
    public float duration = 0.8f;

    [Header("交互范围（需手动拖拽）")]
    public InteractRange interactRange;   // 关联的交互区域
    public override void OnInteract()
    {
        if (branchToTransform == null)
        {
            Debug.LogError("StarInteractable: branchToTransform 未赋值！请在 Inspector 中指定需要变换的树枝。");
            return;
        }
        // 星星消失
        gameObject.SetActive(false);
        DisableBranchCollider();
        // 同时播放移动和旋转动画
        Sequence seq = DOTween.Sequence();
        //seq.Join(branchToTransform.transform.DOMove(targetPosition, duration));
        seq.Join(branchToTransform.transform.DORotate(targetRotation, duration));

        // 动画完成后关闭碰撞体
        // 永久关闭交互提示
        if (interactRange != null)
            interactRange.DisablePrompt();
    }
    private void DisableBranchCollider()
    {
            Collider2D col = branchToTransform.GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
                Debug.Log($"自动禁用树枝上的碰撞体：{col.name}");
            }
    }
}