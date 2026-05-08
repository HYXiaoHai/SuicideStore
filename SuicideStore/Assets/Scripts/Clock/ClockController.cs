using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ClockController : MonoBehaviour
{
    [Header("转场")]
    public CanvasGroup clockCanvasGroup;
    public string nextSceneName;
    private CanvasGroup minuteHandCG;
    private CanvasGroup hourHandCG;

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
    [Header("音效")]
    public AudioClip handClip;//拨动指针的音效
    public AudioSource handAudioSource;
    // 音效触发相关
    private float lastTickAngle = 0f;        // 上次触发音效时的角度
    private readonly float tickStep = 30f;   // 每30度触发一次音效
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        currentAngle = startAngle;
        lastTickAngle = startAngle;           // 初始化上次触发角度
        if (maskGraphic != null)
            targetMaterial = maskGraphic.material;

        if (minuteHand != null)
            minuteHandCG = minuteHand.GetComponent<CanvasGroup>();
        if (hourHand != null)
            hourHandCG = hourHand.GetComponent<CanvasGroup>();

        UpdateAspectRatio();
        UpdateHandPosition();
        UpdateHand();
        UpdateShader();
    }

    void Update()
    {
        if (isComplete || targetMaterial == null) return;

        HandleInput();

        //if (!isDragging)
        //{
        //    currentAngle += autoRotateSpeed * Time.deltaTime;

        //}
        if (!isDragging)
        {
            // 自动旋转
            float oldAngle = currentAngle;
            float newAngle = currentAngle + autoRotateSpeed * Time.deltaTime;
            if (newAngle > 360f) newAngle = 360f;

            // 检查并播放音效（基于角度增量）
            CheckAndPlayTick(oldAngle, newAngle);

            currentAngle = newAngle;
        }

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
            //if (mouseAngle >= currentAngle && mouseAngle <= 360f)
            //    currentAngle = mouseAngle;
            if (mouseAngle >= currentAngle && mouseAngle <= 360f)
            {
                float oldAngle = currentAngle;
                float newAngle = mouseAngle;

                // 检查并播放音效（基于角度增量）
                CheckAndPlayTick(oldAngle, newAngle);

                currentAngle = newAngle;
            }
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
    private void CheckAndPlayTick(float oldAngle, float newAngle)
    {
        if (handAudioSource == null || handClip == null) return;
        if (Mathf.Approximately(oldAngle, newAngle)) return;

        // 计算起始和结束所在的区间索引（从0开始，每个区间宽度 tickStep）
        int startIndex = Mathf.FloorToInt(oldAngle / tickStep);
        int endIndex = Mathf.FloorToInt(newAngle / tickStep);

        // 如果区间索引不同，说明跨越了至少一个 tickStep 边界
        if (endIndex > startIndex)
        {
            int tickCount = endIndex - startIndex;
            for (int i = 0; i < tickCount; i++)
            {
                handAudioSource.PlayOneShot(handClip);
            }
        }
    }
    private void Complete()
    {
        if (isComplete) return;
        isComplete = true;

        // 停止自动旋转（已经不会再更新）
        // 确保材质角度为360
        if (targetMaterial != null)
            targetMaterial.SetFloat("_Angle", 360f);

        // 开始转场动画
        StartCoroutine(TransitionRoutine());
    }
    private IEnumerator TransitionRoutine()
    {
        // 同时淡出表盘 CanvasGroup 和指针 CanvasGroup
        float duration = 1f;

        // 使用 DOTween 并行播放动画
        if (clockCanvasGroup != null)
            clockCanvasGroup.DOFade(0f, duration);

        if (minuteHandCG != null)
            minuteHandCG.DOFade(0f, duration);

        if (hourHandCG != null)
            hourHandCG.DOFade(0f, duration);

        yield return new WaitForSeconds(duration);

        // 可选：彻底禁用指针交互（防止残留点击）
        if (minuteHandCG != null) minuteHandCG.interactable = false;
        if (hourHandCG != null) hourHandCG.interactable = false;
        if (clockCanvasGroup != null) clockCanvasGroup.interactable = false;

        // 加载场景
        LoadScene();
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }
}