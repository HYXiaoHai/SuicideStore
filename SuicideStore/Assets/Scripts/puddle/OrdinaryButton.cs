using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;                  // 引入 DOTween 命名空间
using UnityEngine.EventSystems;
using Unity.VisualScripting;     // 用于 UI 事件接口

public class OrdinaryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("动画设置")]
    [SerializeField] private float scaleMultiplier = 1.2f;  // 放大倍数
    [SerializeField] private float animationDuration = 0.2f; // 动画时长（秒）
    public bool isClick = false;

    private Vector3 originalScale;  // 原始缩放

    private void Start()
    {
        // 记录初始缩放值
        originalScale = transform.localScale;
    }

    // 鼠标进入（UI 方式）
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isClick)
        {
            return;
        }
        // 停止当前正在进行的缩放动画，防止冲突
        transform.DOKill();
        // 缩放到目标大小
        transform.DOScale(originalScale * scaleMultiplier, animationDuration);
    }
    public void IsClick()
    {
        // 恢复原始大小
        transform.DOScale(originalScale, animationDuration);
        isClick = true;
    }
    // 鼠标退出（UI 方式）
    public void OnPointerExit(PointerEventData eventData)
    {
        if(isClick)
        {
            return;
        }
        // 停止当前动画
        transform.DOKill();
        // 恢复原始大小
        transform.DOScale(originalScale, animationDuration);
    }
}