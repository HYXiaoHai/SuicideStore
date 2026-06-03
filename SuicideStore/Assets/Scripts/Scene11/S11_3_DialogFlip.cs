using UnityEngine;
using DG.Tweening;

public class S11_3_DialogFlip : MonoBehaviour
{
    [Header("对话框正反面")]
    public GameObject frontSide;
    public GameObject backSide;

    [Header("动画设置")]
    public float flipDuration = 0.5f;
    public float scaleToCenterDuration = 0.3f;
    public float centerScale = 1.5f;

    [Header("状态")]
    public bool isFlipped = false;
    public bool isCentered = false;

    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 originalRotation;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.eulerAngles;

        if (frontSide != null) frontSide.SetActive(true);
        if (backSide != null) backSide.SetActive(false);
    }

    // 放大到屏幕中央
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

    // 翻转对话框
    public void Flip()
    {
        if (!isCentered) return;

        Sequence seq = DOTween.Sequence();
        
        // 先转到 90 度
        seq.Append(transform.DORotate(originalRotation + new Vector3(0, 90, 0), flipDuration / 2f).OnComplete(() => {
            if (frontSide != null) frontSide.SetActive(isFlipped);
            if (backSide != null) backSide.SetActive(!isFlipped);
        }));
        
        // 再转回来
        seq.Append(transform.DORotate(originalRotation, flipDuration / 2f));
        seq.OnComplete(() => isFlipped = !isFlipped);
        seq.Play();
    }

    // 点击检测
    void OnMouseDown()
    {
        S11_3_Manager manager = FindObjectOfType<S11_3_Manager>();
        if (manager != null)
        {
            manager.OnDialogClicked(this);
        }
    }

    // 重置到原始状态
    public void ResetToOriginal()
    {
        transform.DOMove(originalPosition, scaleToCenterDuration);
        transform.DOScale(originalScale, scaleToCenterDuration);
        transform.DORotate(originalRotation, scaleToCenterDuration / 2f);
        
        if (frontSide != null) frontSide.SetActive(true);
        if (backSide != null) backSide.SetActive(false);
        
        isFlipped = false;
        isCentered = false;
    }
}
