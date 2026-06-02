using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene7_3Manager : MonoBehaviour
{
    [Header("=== 分镜Canvas ===")]
    public Canvas comicCanvas;
    public Image[] comicPanels;

    [Header("=== 文案设置 ===")]
    public TextMeshProUGUI textDisplay;
    public Transform textPosition;
    public Color blackColor = Color.black;
    public Color blueColor = new Color(0, 0, 1, 1);
    public string text1 = "乐乐，你看那是山顶，我们一会上去拍照";
    public string text2 = "可以快点吗？我不喜欢拍照啦";

    [Header("=== 缆车轨迹设置 ===")]
    public RectTransform cableCar;
    public Vector2 startPoint = new Vector2(-250, 0);
    public Vector2 endPoint = new Vector2(250, 0);
    private Vector2 trackDirection;
    private float trackLength;

    [Header("=== 相机画面 ===")]
    public GameObject cameraScreen;

    [Header("=== 转场设置 ===")]
    public Image fadeImage;
    public string nextSceneName;
    public float fadeDuration = 0.5f;

    [Header("音效")]
    public AudioClip clickClip;//点击音效
    public AudioClip sliderClip;//滑动音效
    public AudioClip takePhotoClip;//滑动音效

    private int currentPanel = 0;
    private bool canClick = true;
    private bool isDrag = false;
    void Start()
    {
        if (comicCanvas != null)
        {
            comicCanvas.enabled = true;
        }

        for (int i = 0; i < comicPanels.Length; i++)
        {
            if (comicPanels[i] != null)
            {
                comicPanels[i].gameObject.SetActive(i == 0);
            }
        }

        if (cameraScreen != null)
        {
            cameraScreen.SetActive(false);
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }

        InitializeTrack();
        ShowText(text1, blackColor);
    }

    void InitializeTrack()
    {
        trackDirection = endPoint - startPoint;
        trackLength = trackDirection.magnitude;
        trackDirection.Normalize();

        if (cableCar != null)
        {
            cableCar.anchoredPosition = startPoint;
            
            if (!cableCar.GetComponent<CableCarDrag>())
            {
                CableCarDrag drag = cableCar.gameObject.AddComponent<CableCarDrag>();
                drag.manager = this;
            }
        }
    }

    void Update()
    {
        if (currentPanel == 1 && canClick && comicCanvas != null && comicCanvas.enabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AudioManager.Instance.Play2DSound(sliderClip, 1f);
                OnPanel2Click();
            }
        }

        if (currentPanel == 2 && canClick && comicCanvas != null && comicCanvas.enabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                AudioManager.Instance.Play2DSound(sliderClip, 1f);
                OnPanel3Click();
            }
        }

        if (cameraScreen != null && cameraScreen.activeSelf && canClick && (comicCanvas == null || !comicCanvas.enabled))
        {
            if (Input.GetMouseButtonDown(0))
            {
                AudioManager.Instance.Play2DSound(takePhotoClip, 1f);
                StartCoroutine(FadeToBlackAndLoadScene());
            }
        }
    }

    void OnPanel2Click()
    {
        canClick = false;
        ShowPanel(2);
        Invoke("EnableClick", 0.5f);
    }

    void OnPanel3Click()
    {
        canClick = false;
        SwitchToCameraScreen();
    }

    public void OnCableCarDrag(Vector2 delta)
    {
        if (currentPanel != 0 || comicCanvas == null || !comicCanvas.enabled || cableCar == null)
        {
            return;
        }
        if(!isDrag)
        {
            AudioManager.Instance.Play2DSound(clickClip, 1f);
            isDrag = true;
        }

        Vector2 currentPos = cableCar.anchoredPosition;
        Vector2 projectedDelta = Vector2.Dot(delta, trackDirection) * trackDirection;
        
        Vector2 newPos = currentPos + projectedDelta;
        
        float progress = Vector2.Dot(newPos - startPoint, trackDirection);
        progress = Mathf.Clamp(progress, 0, trackLength);
        
        cableCar.anchoredPosition = startPoint + trackDirection * progress;
    }

    public void OnCableCarDragEnd()
    {
        if (currentPanel != 0 || cableCar == null)
        {
            return;
        }
        isDrag = false;
        float progress = Vector2.Dot(cableCar.anchoredPosition - startPoint, trackDirection) / trackLength;

        if (progress >= 0.95f)
        {
            ShowPanel(1);
            AudioManager.Instance.Play2DSound(sliderClip, 1f);
            ShowText(text2, blackColor);
        }
        else
        {
            cableCar.anchoredPosition = startPoint;
        }
    }

    void ShowPanel(int index)
    {
        if (index >= 0 && index < comicPanels.Length)
        {
            comicPanels[index].gameObject.SetActive(true);
            comicPanels[index].DOFade(1f,0.5f);
            currentPanel = index;
        }
    }

    void ShowText(string text, Color color)
    {
        if (textDisplay != null)
        {
            textDisplay.gameObject.SetActive(true);
            textDisplay.text = text;
            textDisplay.color = color;
        }
    }

    void SwitchToCameraScreen()
    {
        if (comicCanvas != null)
        {
            comicCanvas.enabled = false;
        }

        if (cameraScreen != null)
        {
            cameraScreen.SetActive(true);
        }

        Invoke("EnableClick", 0.5f);
    }

    void EnableClick()
    {
        canClick = true;
    }

    IEnumerator FadeToBlackAndLoadScene()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.canvasRenderer.SetAlpha(0);
            fadeImage.CrossFadeAlpha(1, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

public class CableCarDrag : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Scene7_3Manager manager;

    public void OnDrag(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.OnCableCarDrag(eventData.delta);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.OnCableCarDragEnd();
        }
    }
}
