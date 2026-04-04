using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ClockController : MonoBehaviour
{
    [Header("指针")]
    public Transform minuteHand;
    public Transform hourHand;
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float handZeroOffset = 90f;  // 素材指向3点填90

    [Header("中心偏移（像素）")]
    public Vector2 handCenterOffset = Vector2.zero;

    [Header("WhiteMask 目标")]
    public Graphic maskGraphic;

    [Header("参数")]
    public float autoRotateSpeed = 6f;

    private float currentAngle = 0f;
    private bool isComplete = false;
    private bool isDragging = false;
    private Material targetMaterial;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        currentAngle = startAngle;

        if (maskGraphic != null)
            targetMaterial = maskGraphic.material;

        UpdateAspectRatio();
        UpdateHandPosition();
        UpdateHand();
        UpdateShader();
    }

    void Update()
    {
        if (isComplete || targetMaterial == null) return;

        HandleInput();

        if (!isDragging)
            currentAngle += autoRotateSpeed * Time.deltaTime;

        if (currentAngle >= 360f)
        {
            currentAngle = 360f;
            Complete();
        }

        UpdateHand();
        UpdateShader();
    }

    private void UpdateHandPosition()
    {
        if (minuteHand != null && minuteHand is RectTransform minuteRect)
            minuteRect.anchoredPosition = handCenterOffset;
        if (hourHand != null && hourHand is RectTransform hourRect)
            hourRect.anchoredPosition = handCenterOffset;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverMinuteHand())
                isDragging = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            float mouseAngle = GetAngleFromMouse();
            if (mouseAngle >= currentAngle && mouseAngle <= 360f)
                currentAngle = mouseAngle;
            else if (mouseAngle < currentAngle - 10f)
                isDragging = false;
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;
    }

    private float GetAngleFromMouse()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera, out localPoint);
        float angle = Mathf.Atan2(localPoint.x - handCenterOffset.x, localPoint.y - handCenterOffset.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    private bool IsMouseOverMinuteHand()
    {
        if (minuteHand == null) return false;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera, out localMousePos);
        if (localMousePos.magnitude < 20f) return false;

        float mouseAngle = GetAngleFromMouse();
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(mouseAngle, currentAngle));
        return angleDiff < 15f;
    }

    private void UpdateAspectRatio()
    {
        if (rectTransform != null && targetMaterial != null)
        {
            float aspect = rectTransform.rect.width / rectTransform.rect.height;
            targetMaterial.SetFloat("_AspectRatio", aspect);
        }
    }

    private void UpdateHand()
    {
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -currentAngle + handZeroOffset);
        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -currentAngle / 12f + handZeroOffset);
    }

    private void UpdateShader()
    {
        if (targetMaterial != null)
            targetMaterial.SetFloat("_Angle", currentAngle);
    }

    private void Complete()
    {
        isComplete = true;
        if (minuteHand != null) minuteHand.gameObject.SetActive(false);
        if (hourHand != null) hourHand.gameObject.SetActive(false);
        if (targetMaterial != null) targetMaterial.SetFloat("_Angle", 360f);
    }
}