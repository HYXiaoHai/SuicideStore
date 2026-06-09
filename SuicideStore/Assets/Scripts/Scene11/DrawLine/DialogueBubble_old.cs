using TMPro;
using UnityEngine;
using DG.Tweening;
//气泡动画脚本
//调用：AnimateBubble(文字，起始位置，结束位置，动画时长(默认1.5),气泡最大scale(默认1))
public class DialogueBubble_old : MonoBehaviour
{
    public TMP_Text _text;
    public RectTransform startPosition;//起始点
    public RectTransform endPosition;//结束点
    public float duration;//时间间隔
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

    private void Start()
    {
        //AnimateBubble("", startPosition, endPosition);
    }
    //启动自动动画（出生+上浮）
    public void AnimateBubble(string content, RectTransform start, RectTransform end, float moveDuration = 1.5f, float scale = 1f)
    {
        if (_text != null)
            _text.text = content;
        rectTransform.anchoredPosition = start.anchoredPosition;
        //入场动画结束后，自动播放上升动画
        PlayEnterAnimation(() => PlayMoveAnimation(end, moveDuration), scale);
    }
    //气泡出生动画
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

    //气泡上浮动画：从 start 移动到 end，同时缩放到0.5并淡出
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