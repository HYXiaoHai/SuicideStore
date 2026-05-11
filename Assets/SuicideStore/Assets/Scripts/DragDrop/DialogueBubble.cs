using TMPro;
using UnityEngine;
using DG.Tweening;

public class DialogueBubble : MonoBehaviour
{
    public TMP_Text _text;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private RectTransform startPosition;
    private RectTransform endPosition;
    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    //开场动画
    public void PlayEnterAnimation(TweenCallback onComplete = null,float scale = 1f)
    {
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(scale, 0.2f).SetEase(Ease.OutCirc));
        seq.Join(canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCirc));
        if (onComplete != null)
            seq.OnComplete(onComplete);
        seq.Play();
    }
    public void AnimateBubble(string content, RectTransform start, RectTransform end, float moveDuration = 1.5f,float scale = 1f)
    {
        if (_text != null)
            _text.text = content;
        rectTransform.anchoredPosition = start.anchoredPosition;
        // 入场动画结束后，自动播放上升动画
        PlayEnterAnimation(() => PlayMoveAnimation(end, moveDuration), scale);
    }
    /// 新版气泡动画：从 start 移动到 end，同时缩放到 0.5 并淡出
    public void PlayMoveAnimation(RectTransform end, float duration = 1.5f, TweenCallback onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(rectTransform.DOAnchorPos(end.anchoredPosition, duration).SetEase(Ease.InSine));
        seq.Join(transform.DOScale(0.5f, duration).SetEase(Ease.InSine));
        seq.Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.InExpo));
        if (onComplete != null)
            seq.OnComplete(onComplete);
        else
            seq.OnComplete(() => Destroy(gameObject));
        seq.Play();
    }

    // 旧方法保留（不再使用，避免干扰）
    public void ShowBubble(string content, float stayDuration = 2f, float moveDistance = 800f, float floatDuration = 4f)
    {
        // 此方法不再使用，可留空或直接调用 AnimateBubble 的默认版本
        Debug.LogWarning("ShowBubble is deprecated, use AnimateBubble instead.");
    }

    //设置文本并立即显示（无动画）
    public void SetText(string content)
    {
        if (_text != null)
            _text.text = content;
        canvasGroup.alpha = 1;
    }

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
    public void DestroyBubble()
    {
        // 停止所有正在进行的 DOTween 动画
        transform.DOKill();
        canvasGroup.DOKill();
        // 开始渐隐动画（0.5秒）
        canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
        {
            Debug.Log("消除气泡");
            Destroy(gameObject);
        });
    }
}