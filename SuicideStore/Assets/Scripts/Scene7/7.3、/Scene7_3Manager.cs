using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

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

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private int currentPanel = 0;
    private bool canClick = true;

    void Start()
    {
        if (comicCanvas != null)
        {
            comicCanvas.enabled = true;
        }
        else if (showDebugLog)
        {
            Debug.LogWarning("comicCanvas 未设置！");
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
        else if (showDebugLog)
        {
            Debug.LogWarning("cableCar 未设置！");
        }
    }

    void Update()
    {
        if (showDebugLog && Input.GetMouseButtonDown(0))
        {
            Debug.Log("当前状态 - currentPanel: " + currentPanel + ", canClick: " + canClick + ", comicCanvas.enabled: " + (comicCanvas != null && comicCanvas.enabled));
        }

        if (currentPanel == 1 && canClick && comicCanvas != null && comicCanvas.enabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPanel2Click();
            }
        }

        if (currentPanel == 2 && canClick && comicCanvas != null && comicCanvas.enabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnPanel3Click();
            }
        }

        if (cameraScreen != null && cameraScreen.activeSelf && canClick && (comicCanvas == null || !comicCanvas.enabled))
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCoroutine(FadeToBlackAndLoadScene());
            }
        }
    }

    void OnPanel2Click()
    {
        if (showDebugLog)
        {
            Debug.Log("分镜2点击，切换到分镜3");
        }
        canClick = false;
        ShowPanel(2);
        Invoke("EnableClick", 0.5f);
    }

    void OnPanel3Click()
    {
        if (showDebugLog)
        {
            Debug.Log("分镜3点击，切换到相机画面");
        }
        canClick = false;
        SwitchToCameraScreen();
    }

    public void OnCableCarDrag(Vector2 delta)
    {
        if (currentPanel != 0 || comicCanvas == null || !comicCanvas.enabled || cableCar == null)
        {
            return;
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

        float progress = Vector2.Dot(cableCar.anchoredPosition - startPoint, trackDirection) / trackLength;

        if (progress >= 0.95f)
        {
            ShowPanel(1);
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
            for (int i = 0; i < comicPanels.Length; i++)
            {
                if (comicPanels[i] != null)
                {
                    comicPanels[i].gameObject.SetActive(i == index);
                }
            }
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

            //if (textPosition != null)
            //{
            //    textDisplay.rectTransform.position = textPosition.position;
            //}
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
