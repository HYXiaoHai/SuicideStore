using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Scene10_6Manager : MonoBehaviour
{
    [Header("=== 点击区域设置 ===")]
    public RectTransform doorClickArea;

    [Header("=== 全屏背景图设置 ===")]
    public Image backgroundImage;
    public Sprite initialImage;
    public Sprite newImage;
    public float imageFadeDuration = 0.5f;

    [Header("=== 摄像头设置 ===")]
    public Camera mainCamera;
    public float cameraZoomOutSize = 10f;
    public float cameraZoomDuration = 1.5f;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private bool isClicked = false;
    private float originalCameraSize;
    private Vector3 originalCameraPosition;
    private Camera mainCam;

    void Start()
    {
        mainCam = mainCamera != null ? mainCamera : Camera.main;
        originalCameraSize = mainCam.orthographicSize;
        originalCameraPosition = mainCam.transform.position;

        SetupFullScreenBackground();
        UpdateBackgroundImage(initialImage);
    }

    void SetupFullScreenBackground()
    {
        if (backgroundImage == null) return;

        RectTransform imageRect = backgroundImage.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0, 0);
        imageRect.anchorMax = new Vector2(1, 1);
        imageRect.sizeDelta = Vector2.zero;
        imageRect.anchoredPosition = Vector2.zero;
    }

    void UpdateBackgroundImage(Sprite sprite)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    void Update()
    {
        if (!isClicked && Input.GetMouseButtonDown(0))
        {
            CheckDoorClick();
        }
    }

    void CheckDoorClick()
    {
        if (doorClickArea == null) return;

        Vector2 mousePos = Input.mousePosition;

        if (RectTransformUtility.RectangleContainsScreenPoint(doorClickArea, mousePos))
        {
            OnDoorClicked();
        }
    }

    void OnDoorClicked()
    {
        isClicked = true;

        if (showDebugLog)
        {
            Debug.Log("门被点击，切换背景图并拉远摄像头");
        }

        Sequence sequence = DOTween.Sequence();

        if (backgroundImage != null && newImage != null)
        {
            sequence.Append(backgroundImage.DOFade(0f, imageFadeDuration).OnComplete(() =>
            {
                UpdateBackgroundImage(newImage);
                backgroundImage.DOFade(1f, imageFadeDuration);
            }));
        }

        if (mainCam != null)
        {
            sequence.Join(mainCam.DOOrthoSize(cameraZoomOutSize, cameraZoomDuration).SetEase(Ease.OutQuad));
        }

        sequence.Play();
    }

    public void ResetScene()
    {
        isClicked = false;

        if (backgroundImage != null && initialImage != null)
        {
            UpdateBackgroundImage(initialImage);
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
        }

        if (mainCam != null)
        {
            mainCam.orthographicSize = originalCameraSize;
            mainCam.transform.position = originalCameraPosition;
        }
    }
}