using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ClockController_reverse : MonoBehaviour
{
    [Header("表盘UI")]
    public Image fillImage;
    public Transform minuteHand;
    public Transform hourHand;

    [Header("完成动画")]
    public CanvasGroup clockCanvasGroup;
    public string nextSceneName;
    public bool shouldUseFade = true;

    [Header("参数")]
    public float autoRotateSpeed = 6f;
    public Vector2 handCenterOffset = Vector2.zero;
    [SerializeField] private float handZeroOffset = 0f;

    private float currentAngle = 0f;          // 0~180度（9点=0, 12点=90, 3点=180）
    private bool isComplete = false;
    private bool isDragging = false;
    private RectTransform clockRect;
    private Canvas canvas;
    private Vector2 centerPoint;
    private float aspect;

    [Header("音效")]
    public AudioClip handClip;
    private float lastTickAngle = 0f;
    public float tickStep = 30f;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        clockRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        centerPoint = clockRect.rect.center;
        aspect = clockRect.rect.width / clockRect.rect.height;

        currentAngle = 0f;
        lastTickAngle = currentAngle;
        UpdateDisplay();
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (isComplete) return;

        HandleInput();

        if (!isDragging)
        {
            float old = currentAngle;
            float newAngle = currentAngle + autoRotateSpeed * Time.deltaTime;
            if (newAngle > 180f) newAngle = 180f;
            CheckAndPlayTick(old, newAngle);
            currentAngle = newAngle;
            UpdateDisplay();
        }

        if (currentAngle >= 180f && !isComplete)
        {
            currentAngle = 180f;
            UpdateDisplay();
            Complete();
        }
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
            if (mouseAngle >= currentAngle && mouseAngle <= 180f)
            {
                float old = currentAngle;
                currentAngle = mouseAngle;
                CheckAndPlayTick(old, currentAngle);
                UpdateDisplay();
            }
            else if (mouseAngle < currentAngle - 10f)
                isDragging = false;
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;
    }

    // 鼠标位置 → 角度（0~180）
    private float GetAngleFromMouse()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(clockRect, Input.mousePosition, canvas.worldCamera, out localPoint);
        float dx = localPoint.x - centerPoint.x;
        float dy = localPoint.y - centerPoint.y;
        dx /= aspect;
        float angle = Mathf.Atan2(dx, -dy) * Mathf.Rad2Deg;
        float t = (angle + 90f) / 180f;
        return Mathf.Clamp(t, 0f, 1f) * 180f;
    }

    // 获取当前分针的视觉角度（0~180）
    private float GetHandAngle()
    {
        if (minuteHand == null) return 0f;
        float angle = minuteHand.localEulerAngles.z - handZeroOffset;
        if (angle > 180f) angle -= 360f;
        // 顺时针旋转的UI角度是负值，取反得到视觉角度（-90~90）
        float visualAngle = -angle;
        return visualAngle + 90f;   // 映射到 0~180
    }

    // 原版角度差检测（阈值15度）
    private bool IsMouseOverMinuteHand()
    {
        if (minuteHand == null) return false;
        float mouseAngle = GetAngleFromMouse();
        float handAngle = GetHandAngle();
        float diff = Mathf.Abs(mouseAngle - handAngle);
        // 调试输出（正式使用时注释）
        Debug.Log($"鼠标角度: {mouseAngle:F1}, 分针角度: {handAngle:F1}, 差值: {diff:F1}");
        return diff < 15f;
    }

    private void UpdateDisplay()
    {
        float progress = currentAngle / 180f;
        if (fillImage != null)
            fillImage.fillAmount = progress;

        // 指针旋转：视觉角度 = -90 + progress*180
        float visualAngle = -90f + progress * 180f;
        float uiAngle = -visualAngle;          // UI坐标系顺时针为负
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, uiAngle + handZeroOffset);
        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, uiAngle / 12f + handZeroOffset);
    }

    private void CheckAndPlayTick(float oldAngle, float newAngle)
    {
        if (Mathf.Approximately(oldAngle, newAngle)) return;
        int startIndex = Mathf.FloorToInt(oldAngle / tickStep);
        int endIndex = Mathf.FloorToInt(newAngle / tickStep);
        int stepCount = Mathf.Abs(endIndex - startIndex);
        if (stepCount > 0)
        {
            for (int i = 0; i < stepCount; i++)
                AudioManager.Instance.PlayShortSound(handClip, 0.8f);
        }
    }

    private void Complete()
    {
        if (isComplete) return;
        isComplete = true;
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        float duration = 1f;
        if (clockCanvasGroup != null)
            clockCanvasGroup.DOFade(0f, duration);
        yield return new WaitForSeconds(duration);
        LoadScene();
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            CompleteLevel();
        else
            Debug.LogWarning("Next scene name not set!");
    }

    public void CompleteLevel()
    {
        GameManage.Instance.CompleteCurrentLevel();
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (shouldUseFade)
                {
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        SceneManager.LoadScene(nextScene);
                    });
                    AudioManager.Instance.FadeOutCurrentBGM(1f, null);
                }
                else
                {
                    SceneManager.LoadScene(nextScene);
                }
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
}