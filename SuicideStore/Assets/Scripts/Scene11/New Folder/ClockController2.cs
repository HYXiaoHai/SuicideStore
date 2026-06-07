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

    [Header("ָ��")]
    public Transform minuteHand;
    public Transform hourHand;
    [SerializeField] private float startAngle = 0f;    
    [SerializeField] private float handZeroOffset = 90f;

    [Header("����ƫ�ƣ����أ�")]
    public Vector2 handCenterOffset = Vector2.zero;

    [Header("WhiteMask Ŀ��")]
    public Graphic maskGraphic;

    [Header("����")]
    public float autoRotateSpeed = 6f;

    private float currentAngle = 0f;
    private bool isComplete = false;
    private bool isDragging = false;
    private Material targetMaterial;
    private RectTransform rectTransform;
    private Canvas canvas;
    [Header("��Ч")]
    public AudioClip handClip;
    private float lastTickAngle = 0f;
    public float tickStep = 30f;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        // ��ʱ�룺�� 360�� ��ʼ���𽥼��� 0��
        currentAngle = 360f;
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
            float newAngle = currentAngle - autoRotateSpeed * Time.deltaTime;
            if (newAngle < 0f) newAngle = 0f;
            CheckAndPlayTick(oldAngle, newAngle);
            currentAngle = newAngle;
        }

        if (currentAngle <= 0f)
        {
            currentAngle = 0f;
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
            // ��ʱ�룺ֻ�����Ƕȼ�С
            if (mouseAngle <= currentAngle && mouseAngle >= 0f)
            {
                float oldAngle = currentAngle;
                float newAngle = mouseAngle;
                CheckAndPlayTick(oldAngle, newAngle);
                currentAngle = newAngle;
            }
            else if (mouseAngle > currentAngle + 10f)
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
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                if (shouldUseFade)
                {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        // 转场完成后加载新场景
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