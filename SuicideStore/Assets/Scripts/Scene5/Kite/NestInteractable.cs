using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestInteractable : BaseInteractable
{
    public GameObject branchToTransform1; // 需要旋转和平移的树枝
    public Vector3 target1Rotation;
    public Vector3 target1Position;

    public GameObject branchToTransform2; // 需要旋转和平移的树枝
    public Vector3 target2Rotation;
    public Vector3 target2Position;

    public GameObject branchToTransform3; // 需要旋转和平移的树枝
    public Vector3 target3Rotation;
    public Vector3 target3Position;
    
    public float duration = 0.8f;

    public KiteController kite; // 关联的风筝
                                // 永久关闭交互提示
    [Header("交互范围（需手动拖拽）")]
    public InteractRange interactRange;   // 关联的交互区域

    [Header("音效")]
    public AudioClip interactAudioClip;
    public override void OnInteract()
    {
        AudioManager.Instance.Play2DSound(interactAudioClip, 0.5f);

        // 同时播放移动和旋转动画
        Sequence seq = DOTween.Sequence();
        seq.Join(branchToTransform1.transform.DOMove(target1Position, duration));
        seq.Join(branchToTransform1.transform.DORotate(target1Rotation, duration));
        seq.Join(branchToTransform2.transform.DOMove(target2Position, duration));
        seq.Join(branchToTransform2.transform.DORotate(target2Rotation, duration));
        seq.Join(branchToTransform3.transform.DOMove(target3Position, duration));
        seq.Join(branchToTransform3.transform.DORotate(target3Rotation, duration));


        // 启用风筝交互
        if (kite != null)
            kite.ActivateKite();

        // 可选：禁用再次点击
        GetComponent<Collider2D>().enabled = false;
        if (interactRange != null)
            interactRange.DisablePrompt();
    }
}
