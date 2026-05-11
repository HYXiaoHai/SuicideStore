using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene7_3Manager : MonoBehaviour
{
    [Header("分镜Canvas")]
    public Canvas comicCanvas;
    public Image[] comicPanels;

    [Header("文案设置")]
    public Text textDisplay;
    public Color blackColor = Color.black;
    public Color blueColor = new Color(0, 0, 1, 1);

    [Header("缆车设置")]
    public RectTransform cableCar;
    public float dragThreshold = 200f;
    private Vector2 startDragPos;
    private bool isDragging = false;

    [Header("相机画面")]
    public GameObject cameraScreen;

    [Header("转场设置")]
    public Image fadeImage;
    public string nextSceneName;
    public float fadeDuration = 0.5f;

    [Header("文案内容")]
    public string text1 = "乐乐，你看那是山顶，我们一会上去拍照";
    public string text2 = "可以快点吗？我不喜欢拍照啦";

    private int currentPanel = 0;
    private bool canClick = true;

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

        ShowText(text1, blackColor);
    }

    void Update()
    {
        if (currentPanel == 0 && !isDragging)
        {
            CheckDragStart();
        }

        if (currentPanel == 2 && canClick && comicCanvas.enabled)
        {
            CheckCableCarClick();
        }

        if (!comicCanvas.enabled && cameraScreen.activeSelf && canClick)
        {
            CheckCameraClick();
        }
    }

    void CheckDragStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startDragPos = Input.mousePosition;
            isDragging = true;
        }
    }

    void CheckCableCarClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (cableCar != null)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    cableCar.parent.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    null,
                    out localPos
                );

                if (cableCar.rect.Contains(localPos))
                {
                    canClick = false;
                    SwitchToCameraScreen();
                }
            }
        }
    }

    void CheckCameraClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            canClick = false;
            StartCoroutine(FadeToBlackAndLoadScene());
        }
    }

    void FixedUpdate()
    {
        if (isDragging && currentPanel == 0 && comicCanvas.enabled)
        {
            HandleDrag();
        }
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Vector2 currentPos = Input.mousePosition;
            float deltaX = currentPos.x - startDragPos.x;

            if (deltaX > dragThreshold)
            {
                ShowPanel(1);
                ShowText(text2, blueColor);
            }

            isDragging = false;
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
            textDisplay.text = text;
            textDisplay.color = color;
        }
    }

    public void OnPanelClick()
    {
        if (currentPanel == 1 && canClick && comicCanvas.enabled)
        {
            canClick = false;
            ShowPanel(2);
            Invoke("EnableClick", 0.5f);
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