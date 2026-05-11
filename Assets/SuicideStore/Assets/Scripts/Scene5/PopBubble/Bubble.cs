using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bubble : MonoBehaviour
{
    public bool canInteract = false;
    private bool isComplate = false;

    [Header("点击动画")]
    public float interactDuration = 0.2f;
    public SpriteRenderer bubbleSprite;
    public SpriteRenderer outbubbleSprite;

    [Header("气泡移动动画")]
    public float floatAmplitude = 0.3f;
    public float floatDuration = 1f;

    private Tweener floatTween;
    private Vector3 originalPosition;

    void Awake()
    {
        originalPosition = transform.position;
        // 初始透明
        SetAlpha(0f);
    }

    public void SetAlpha(float alpha)
    {
        Color c = bubbleSprite.color;
        c.a = alpha;
        bubbleSprite.color = c;
    }

    public void StartFloating()
    {
        floatTween?.Kill();
        Vector3 upPos = originalPosition + Vector3.up * floatAmplitude;
        floatTween = transform.DOMove(upPos, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void OnInteract()
    {
        if (isComplate) return;
        isComplate = true;
        bubbleSprite.DOFade(0f, interactDuration);  //点击后淡出消失
        outbubbleSprite.DOFade(1f, interactDuration);  //点击后渐显
        floatTween?.Kill();                         // 停止浮动
        PopBubbleManage.Instance?.OnBubbleClicked(this);
        Debug.Log("Bubble 被点击");
    }

    void OnMouseDown()
    {
        if (!isComplate && canInteract && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
        {
            OnInteract();
        }
    }

    public bool IsCompleted => isComplate;

    private void OnDestroy()
    {
        floatTween?.Kill();
    }
}