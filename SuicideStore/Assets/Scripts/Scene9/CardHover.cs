using DG.Tweening;
using UnityEngine;

public class CardHover : MonoBehaviour
{
    public SortCard sortCard;
    private Vector3 originalScale;
    private bool isHovering = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnMouseEnter()
    {
        if (sortCard == null || sortCard.isLocked) return;
        isHovering = true;
        transform.DOScale(originalScale * 1.05f, 0.1f).SetEase(Ease.OutQuad);
    }

    void OnMouseExit()
    {
        if (sortCard == null) return;
        isHovering = false;
        transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutQuad);
    }
}