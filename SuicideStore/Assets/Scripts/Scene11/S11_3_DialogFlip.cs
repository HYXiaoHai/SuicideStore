using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class S11_3_DialogFlip : MonoBehaviour
{
    [Header("对话框正反面")]
    public Sprite frontSprite;
    public Sprite backSprite;
    
    [Header("引用的Image组件")]
    public Image frontImage;
    public Image backImage;

    [Header("动画设置")]
    public float flipDuration = 0.5f;
    public float scaleToCenterDuration = 0.3f;
    public float centerScale = 1.5f;

    public bool isFlipped = false;
    public bool isCentered = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Button button;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (frontImage != null && frontSprite != null)
            frontImage.sprite = frontSprite;
        if (backImage != null && backSprite != null)
            backImage.sprite = backSprite;

        if (frontImage != null) frontImage.enabled = true;
        if (backImage != null) backImage.enabled = false;

        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void OnButtonClicked()
    {
        S11_3_Manager manager = FindObjectOfType<S11_3_Manager>();
        if (manager != null && manager.IsLineComplete())
        {
            manager.OnDialogClicked(this);
        }
    }

    public void ScaleToCenter()
    {
        if (isCentered) return;

        Vector3 centerPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane + 1f));
        centerPos.z = transform.position.z;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(centerPos, scaleToCenterDuration));
        seq.Join(transform.DOScale(originalScale * centerScale, scaleToCenterDuration));
        seq.OnComplete(() => isCentered = true);
        seq.Play();
    }

    public void Flip()
    {
        if (!isCentered) return;

        Sequence seq = DOTween.Sequence();
        
        seq.Append(transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2f).OnComplete(() => {
            if (frontImage != null) frontImage.enabled = isFlipped;
            if (backImage != null) backImage.enabled = !isFlipped;
        }));
        
        seq.Append(transform.DORotate(Vector3.zero, flipDuration / 2f));
        seq.OnComplete(() => isFlipped = !isFlipped);
        seq.Play();
    }

    public void ResetToOriginal()
    {
        transform.DOMove(originalPosition, scaleToCenterDuration);
        transform.DOScale(originalScale, scaleToCenterDuration);
        transform.DORotate(Vector3.zero, scaleToCenterDuration / 2f);
        
        if (frontImage != null) frontImage.enabled = true;
        if (backImage != null) backImage.enabled = false;
        
        isFlipped = false;
        isCentered = false;
    }
}
