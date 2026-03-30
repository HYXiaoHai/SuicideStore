using TMPro;
using UnityEngine;
using DG.Tweening;

public class DialogueBubble : MonoBehaviour
{
    public TMP_Text _text;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void ShowBubble(string content, float stayDuration = 2f, float moveDistance = 800f, float floatDuration = 4f)
    {
        if (_text != null)
            _text.text = content;

        // 初始透明
        canvasGroup.alpha = 0f;
        // 渐显
        canvasGroup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutExpo);
        canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 停留一段时间
            DOVirtual.DelayedCall(stayDuration, () =>
            {
                // 上浮并淡出
                rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveDistance, floatDuration)
                    .SetEase(Ease.OutQuad);
                canvasGroup.DOFade(0f, floatDuration).OnComplete(() => Destroy(gameObject));
            });
        });
    }

    //设置文本并立即显示（无动画）
    public void SetText(string content)
    {
        if (_text != null)
            _text.text = content;
        canvasGroup.alpha = 1;
    }

    //仅开始上浮消失（用于已显示的气泡）
    public void StartFloat(float moveDistance = 80f, float duration = 0.5f, float delay = 0f)
    {
        canvasGroup.DOFade(0, duration).SetDelay(delay);
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveDistance, duration)
            .SetDelay(delay)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(gameObject));
    }

    //保留旧方法，但不推荐使用
    public void Initialize(string content, float moveDistance = 800f, float duration = 4f, float delay = 2f)
    {
        if (_text != null)
            _text.text = content;
        canvasGroup.alpha = 1;
        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + moveDistance, duration)
            .SetEase(Ease.OutQuad);
        canvasGroup.DOFade(0, duration - 2f).SetDelay(delay).OnComplete(() => Destroy(gameObject));
    }
}