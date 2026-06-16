using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ClockController_reverse : MonoBehaviour
{
    [Header("表盘")]
    public CanvasGroup clockCanvasGroup;
    public string nextSceneName;
    public bool shouldUseFade = true;
    private CanvasGroup minuteHandCG;
    private CanvasGroup hourHandCG;

    [Header("指针")]
    public Transform minuteHand;
    public Transform hourHand;
    [SerializeField] private float startAngle = 0f;
    [SerializeField] private float handZeroOffset = 90f;

    [Header("中心偏移（像素）")]
    public Vector2 handCenterOffset = Vector2.zero;

    [Header("WhiteMask 目标")]
    public Graphic maskGraphic;

    [Header("旋转速度")]
    public float autoRotateSpeed = 6f;

    private float currentAngle = 0f;      // 连续角度，范围 [270, 450]
    private bool isComplete = false;
    private bool isDragging = false;
    private Material targetMaterial;
    private RectTransform rectTransform;
    private Canvas canvas;

    [Header("音效")]
    public AudioClip handClip;
    private float lastTickAngle = 0f;
    public float tickStep = 30f;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // 起始角度 270°（9点），目标 450°（3点，即 90° + 360°）
        currentAngle = 270f;
        lastTickAngle = currentAngle;

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
        if (GameManage.Instance.isSetting) return;
        if (isComplete || targetMaterial == null) return;

        HandleInput();

        if (!isDragging)
        {
            float oldAngle = currentAngle;
            // 顺时针增加角度
            float newAngle = currentAngle + autoRotateSpeed * Time.deltaTime;
            if (newAngle > 450f) newAngle = 450f;
            CheckAndPlayTick(oldAngle, newAngle);
            currentAngle = newAngle;
        }

        // 完成条件：达到 450°
        if (currentAngle >= 450f)
        {
            currentAngle = 450f;
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
            float mouseAngle = GetAngleFromMouse(); // 范围 0~360
            // 将鼠标角度映射到与 currentAngle 连续的值域 [270, 450]
            float mappedMouseAngle = mouseAngle;
            if (mappedMouseAngle < 270f && currentAngle > 270f)
                mappedMouseAngle += 360f; // 跨过 0° 时加一圈
            // 限制在允许范围 [270, 450]
            mappedMouseAngle = Mathf.Clamp(mappedMouseAngle, 270f, 450f);

            // 顺时针拖拽：新角度必须大于等于当前角度
            if (mappedMouseAngle >= currentAngle && mappedMouseAngle <= 450f)
            {
                float oldAngle = currentAngle;
                float newAngle = mappedMouseAngle;
                CheckAndPlayTick(oldAngle, newAngle);
                currentAngle = newAngle;
            }
            else if (mappedMouseAngle < currentAngle - 10f) // 明显逆时针则取消拖拽
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
        // 将鼠标角度映射到接近 currentAngle 的连续值
        float mappedMouse = mouseAngle;
        if (currentAngle > 270f && mouseAngle < 270f)
            mappedMouse += 360f;
        // 计算角度差（考虑连续域）
        float angleDiff = Mathf.Abs(mappedMouse - currentAngle);
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
        // 使用模360的角度用于实际旋转（0~360）
        float displayAngle = currentAngle % 360f;
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -displayAngle + handZeroOffset);
        if (hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -displayAngle / 12f + handZeroOffset);
    }

    private void UpdateShader()
    {
        if (targetMaterial != null)
        {
            // 当前指针角度（基于12点为0°，顺时针）
            float rawAngle = currentAngle % 360f;
            // 转换为基于9点为0°的角度（供Shader使用）
            float shaderAngle = (rawAngle - 270f + 360f) % 360f;
            targetMaterial.SetFloat("_Angle", shaderAngle);
        }
    }

    private void CheckAndPlayTick(float oldAngle, float newAngle)
    {
        if (Mathf.Approximately(oldAngle, newAngle)) return;
        // 将角度映射到 0~360 区间计算刻度跨越
        float oldMod = oldAngle % 360f;
        float newMod = newAngle % 360f;
        // 处理跨 360 的情况
        if (newAngle > oldAngle && oldMod > newMod) // 跨过0°
        {
            // 先播放从 oldMod 到 360 的刻度，再从 0 到 newMod 的刻度
            int startIdx1 = Mathf.FloorToInt(oldMod / tickStep);
            int endIdx1 = Mathf.FloorToInt(360f / tickStep);
            int steps1 = endIdx1 - startIdx1;
            int startIdx2 = 0;
            int endIdx2 = Mathf.FloorToInt(newMod / tickStep);
            int steps2 = endIdx2 - startIdx2;
            int totalSteps = steps1 + steps2;
            for (int i = 0; i < totalSteps; i++)
                AudioManager.Instance.PlayShortSound(handClip, 0.8f);
        }
        else
        {
            int startIndex = Mathf.FloorToInt(oldMod / tickStep);
            int endIndex = Mathf.FloorToInt(newMod / tickStep);
            int stepCount = Mathf.Abs(endIndex - startIndex);
            if (stepCount > 0)
            {
                for (int i = 0; i < stepCount; i++)
                    AudioManager.Instance.PlayShortSound(handClip, 0.8f);
            }
        }
    }

    private void Complete()
    {
        if (isComplete) return;
        isComplete = true;

        if (targetMaterial != null)
            targetMaterial.SetFloat("_Angle", 0f);

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        float duration = 1f;

        if (clockCanvasGroup != null)
            clockCanvasGroup.DOFade(0f, duration);
        if (minuteHandCG != null)
            minuteHandCG.DOFade(0f, duration);
        if (hourHandCG != null)
            hourHandCG.DOFade(0f, duration);

        yield return new WaitForSeconds(duration);

        if (minuteHandCG != null) minuteHandCG.interactable = false;
        if (hourHandCG != null) hourHandCG.interactable = false;
        if (clockCanvasGroup != null) clockCanvasGroup.interactable = false;

        LoadScene();
    }

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            CompleteLevel();
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
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