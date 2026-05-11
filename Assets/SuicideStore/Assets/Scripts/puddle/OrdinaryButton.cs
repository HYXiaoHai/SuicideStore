using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrdinaryButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [Header("∂Øª≠…Ë÷√")]
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private float animationDuration = 0.2f;
    public bool isClick = false;

    public Vector3 originalScale;

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
    public void IsClick()
    {
        transform.DOScale(originalScale, animationDuration);
        isClick = true;
    }
}
