using TMPro;
using UnityEngine;
using DG.Tweening;

public class DialogueBubble : MonoBehaviour
{
    public TMP_Text _text;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isSpecialBubble = true; // 新增
    private bool shouldGenerateFloating = true;
    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void PlayEnterAnimation(TweenCallback onComplete = null, float scale = 1f)
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

    public void AnimateBubble(string content, RectTransform start, RectTransform end, float moveDuration = 1.5f, float scale = 1f, bool isSpecial = false, bool generateFloating = true)
    {
        isSpecialBubble = isSpecial;
        shouldGenerateFloating = generateFloating;
        if (_text != null)
            _text.text = content;
        rectTransform.anchoredPosition = start.anchoredPosition;
        PlayEnterAnimation(() => PlayMoveAnimation(end, moveDuration), scale);
    }

    public void PlayMoveAnimation(RectTransform end, float duration = 1.5f, TweenCallback onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(rectTransform.DOAnchorPos(end.anchoredPosition, duration).SetEase(Ease.InSine));
        seq.Join(transform.DOScale(0.5f, duration).SetEase(Ease.InSine));
        seq.Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.InExpo));
        seq.OnComplete(() => {
            // 气泡消失后，根据需要生成漂浮气泡
            if (shouldGenerateFloating && FloatingBubbleManager.Instance != null && _text != null && !string.IsNullOrEmpty(_text.text))
            {
                FloatingBubbleManager.Instance.TryAddFloatingBubble(_text.text, isSpecialBubble);
            }
            Destroy(gameObject);
            onComplete?.Invoke();
        });
        seq.Play();
    }

    public void DestroyBubble()
    {
        transform.DOKill();
        canvasGroup.DOKill();
        canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InExpo).OnComplete(() =>
        {
            if (FloatingBubbleManager.Instance != null && _text != null && !string.IsNullOrEmpty(_text.text))
            {
                FloatingBubbleManager.Instance.TryAddFloatingBubble(_text.text, isSpecialBubble);
            }
            Destroy(gameObject);
        });
    }

    //// 以下旧方法可保留但不再使用
    //public void ShowBubble(string content, float stayDuration = 2f, float moveDistance = 800f, float floatDuration = 4f) { }
    //public void SetText(string content) { if (_text != null) _text.text = content; canvasGroup.alpha = 1; }
    //public void StartFloat(float moveDistance = 80f, float duration = 0.5f, float delay = 0f) { }
    //public void Initialize(string content, float moveDistance = 800f, float duration = 4f, float delay = 2f) { }
}