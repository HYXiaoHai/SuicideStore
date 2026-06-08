using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlideController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Slide Settings")]
    public RectTransform sliderHandle;
    public RectTransform sliderBackground;
    public float completionThreshold = 0.9f;

    [Header("References")]
    public MemorySceneManager memoryManager;
    public int slideIndex = 0;

    private Vector2 startPosition;
    private Vector2 handleStartPosition;
    private bool isCompleted = false;
    private bool isDragging = false;

    void Start()
    {
        if (sliderHandle == null)
        {
            sliderHandle = transform.Find("Handle")?.GetComponent<RectTransform>();
        }

        if (sliderBackground == null)
        {
            sliderBackground = transform.Find("Background")?.GetComponent<RectTransform>();
        }

        ResetSlide();
    }
    public void ResetSlide()
    {
        isCompleted = false;
        isDragging = false;
        if (sliderHandle != null && sliderBackground != null)
        {
            float handleWidth = sliderHandle.rect.width;
            float backgroundWidth = sliderBackground.rect.width;
            float centeredX = (backgroundWidth - handleWidth) / 2f;
            
            // 初始位置在滑轨最底部
            handleStartPosition = new Vector2(centeredX, 0);
            sliderHandle.anchoredPosition = handleStartPosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (GameManage.Instance.isSetting) return;
        if (isCompleted) return;

        startPosition = eventData.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManage.Instance.isSetting) return;
        if (isCompleted || !isDragging || sliderHandle == null || sliderBackground == null)
        {
            return;
        }

        float deltaY = eventData.position.y - startPosition.y;
        float backgroundHeight = sliderBackground.rect.height;

        float newY = handleStartPosition.y + deltaY;
        newY = Mathf.Clamp(newY, 0, backgroundHeight);

        sliderHandle.anchoredPosition = new Vector2(handleStartPosition.x, newY);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isCompleted && sliderHandle != null && sliderBackground != null)
        {
            isDragging = false;
            float backgroundHeight = sliderBackground.rect.height;
            float currentProgress = sliderHandle.anchoredPosition.y / backgroundHeight;

            if (currentProgress >= completionThreshold)
            {
                CompleteSlide();
            }
            else
            {
                sliderHandle.anchoredPosition = handleStartPosition;
            }
        }
    }

    void CompleteSlide()
    {
        isCompleted = true;
        isDragging = false;
        if (memoryManager != null)
        {
            memoryManager.OnSlideComplete(slideIndex);
        }
    }
}
