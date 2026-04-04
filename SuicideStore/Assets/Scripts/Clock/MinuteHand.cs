using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;                  // 引入 DOTween 命名空间
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using UnityEngine.UI;     // 用于 UI 事件接口

public class MinuteHand : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image mineteHand;
    [Header("动画设置")]
    [SerializeField] private float scaleMultiplier = 1.2f;  // 放大倍数
    [SerializeField] private float animationDuration = 0.2f; // 动画时长（秒）
    public bool isClick = false;

    public Vector3 originalScale;  // 原始缩放

    private void Start()
    {
        // 记录初始缩放值
        originalScale = mineteHand.transform.localScale;
    }

    // 鼠标进入（UI 方式）
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("鼠标移入");
        if(isClick)
        {
            return;
        }
        // 停止当前正在进行的缩放动画，防止冲突
        mineteHand.transform.DOKill();
        // 缩放到目标大小
        mineteHand.transform.DOScale(originalScale * scaleMultiplier, animationDuration);
    }
    public void IsClick()
    {
        // 恢复原始大小
        mineteHand.transform.DOScale(originalScale, animationDuration);
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
        mineteHand.transform.DOKill();
        // 恢复原始大小
        mineteHand.transform.DOScale(originalScale, animationDuration);
    }
}