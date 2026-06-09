using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class S11_3_DialogFlip : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    [Header("中心位置")]
    public RectTransform centerPosition;
    [Header("正反面图片")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Image组件")]
    public Image targetImage;
    
    [Header("动画参数")]
    public float moveToCenterDuration = 0.3f;
    public float flipDuration = 0.5f;
    public float centerScale = 1.5f;

    [Header("气泡移动动画")]
    public float floatAmplitude = 0.3f;
    public float floatDuration = 1f;

    private Tweener floatTween;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Button button;
    private bool isAnimating = false;
    private bool isCentered = false;       // 是否已放大到中心
    private bool isFlipped = false;        // 是否已翻转（背面）
    private S11_3_Manager manager;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (targetImage != null && frontSprite != null)
            targetImage.sprite = frontSprite;

        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);

        manager = FindObjectOfType<S11_3_Manager>();
    }

    private void OnButtonClicked()
    {
        if (GameManage.Instance.isSetting) return;
        if (manager == null || !manager.IsLineComplete()) return;

        if (!isCentered)
        {
            // 第一次点击：放大到屏幕中央
            StartMoveToCenter();
        }
        else if (!isFlipped)
        {
            // 第二次点击：执行翻转
            StartFlip();
        }
    }

    private void StartMoveToCenter()
    {
        if (isAnimating) return;
        isAnimating = true;

        floatTween?.Kill();                         // 停止浮动
        rectTransform.pivot = new Vector2(0.5f,0.5f);
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(centerPosition.position, moveToCenterDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(centerScale, moveToCenterDuration).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            isCentered = true;
            isAnimating = false;
        });
        seq.Play();
    }
    public void PlayEnterAnimation(TweenCallback onComplete = null, float scale = 1f)
    {
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOScale(scale, 0.2f).SetEase(Ease.OutCirc));
        seq.Join(canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCirc));
        seq.OnComplete(StartFloating);
        seq.Play();
    }
    public void StartFloating()
    {
        floatTween?.Kill();
        Vector3 upPos = originalPosition + Vector3.up * floatAmplitude;
        floatTween = transform.DOMove(upPos, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    private void StartFlip()
    {
        if (isAnimating) return;
        isAnimating = true;

        // 强制将物体旋转归零（如果之前有旋转残留）
        transform.rotation = Quaternion.identity;

        // 第一步：旋转到 90 度
        transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2f)
            .OnComplete(() =>
            {
                Debug.Log("旋转到90度，切换图片");
                if (targetImage != null)
                {
                    targetImage.color = Color.white;
                    targetImage.sprite = backSprite;
                }
                isFlipped = true;

                // 第二步：旋转回 0 度
                transform.DORotate(Vector3.zero, flipDuration / 2f)
                    .OnComplete(() =>
                    {
                        isAnimating = false;
                        StartCoroutine(manager.OnDialogFlipComplete());
                    });
            });
    }
    private void OnDestroy()
    {
        floatTween?.Kill();
    }
}