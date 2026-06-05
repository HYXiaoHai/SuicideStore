using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class S11_3_DialogFlip : MonoBehaviour
{
    [Header("正反面图片")]
    public Sprite frontSprite;
    public Sprite backSprite;

    [Header("Image组件")]
    public Image targetImage;

    [Header("动画参数")]
    public float moveToCenterDuration = 0.3f;
    public float flipDuration = 0.5f;
    public float centerScale = 1.5f;

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

        Vector3 centerPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        centerPos.z = transform.position.z;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(centerPos, moveToCenterDuration).SetEase(Ease.OutQuad));
        seq.Join(transform.DOScale(originalScale * centerScale, moveToCenterDuration).SetEase(Ease.OutQuad));
        seq.OnComplete(() =>
        {
            isCentered = true;
            isAnimating = false;
        });
        seq.Play();
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
}