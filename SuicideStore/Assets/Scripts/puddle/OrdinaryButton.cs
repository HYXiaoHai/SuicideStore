using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class OrdinaryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator animator;               // 改名为 animator，避免和 Unity 的 Animation 类混淆
    public string animationName = "Click";  // 动画状态名称（请确保与 Animator Controller 中一致）

    [Header("动画设置")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationDuration = 0.2f;
    public bool isClick = false;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isClick) return;
        transform.DOKill();
        transform.DOScale(originalScale * scaleMultiplier, animationDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isClick) return;
        transform.DOKill();
        transform.DOScale(originalScale, animationDuration);
    }

    /// <summary>
    /// 播放一次性的 Animator 动画
    /// </summary>
    public void PlayAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(animationName))
        {
            // 重置触发器（如果有），然后播放指定动画
            animator.ResetTrigger(animationName);
            animator.SetTrigger(animationName);
            // 或者直接播放（不推荐，因为会打断过渡）
            // animator.Play(animationName, 0, 0f);
        }
    }

    /// <summary>
    /// 标记按钮已被点击（缩放恢复 + 禁用悬停效果）
    /// </summary>
    public void IsClick()
    {
        transform.DOScale(originalScale, animationDuration);
        isClick = true;
    }
}