using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PhotoConnection : MonoBehaviour
{
    [Header("照片设置")]
    public RectTransform[] photos;           // 照片数组
    public Image[] photoImages;             // 照片的 Image 组件
    public int currentTargetIndex = 0;      // 当前目标照片索引
    public float highlightIntensity = 2f;   // 高亮强度

    [Header("拉线设置")]
    public LineRenderer lineRenderer;       // 拉线
    public Transform lineStart;             // 线的起点
    public bool isDragging = false;         // 是否正在拖拽

    [Header("文本设置")]
    public GameObject dialoguePanel;        // 对话面板
    public TextMeshProUGUI dialogueText;    // 对话文本
    public string[] dialogueTexts;          // 对话数组

    [Header("参数设置")]
    public float connectionThreshold = 50f; // 连接阈值
    public float highlightDuration = 2f;     // 高亮持续时间

    private Vector2 dragStartPos;
    private int connectedPhotos = 0;

    void Start()
    {
        // 初始化线
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }

        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 初始化照片
        ResetPhotos();
        HighlightTargetPhoto();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    void StartDrag()
    {
        dragStartPos = Input.mousePosition;
        isDragging = true;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            UpdateLinePosition();
        }
    }

    void UpdateDrag()
    {
        UpdateLinePosition();
    }

    void EndDrag()
    {
        isDragging = false;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        CheckConnection();
    }

    void UpdateLinePosition()
    {
        if (lineRenderer != null && lineStart != null)
        {
            Vector3 startPos = lineStart.position;
            Vector3 endPos = Input.mousePosition;
            endPos.z = 0;

            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
    }

    void CheckConnection()
    {
        Vector2 mousePos = Input.mousePosition;

        for (int i = 0; i < photos.Length; i++)
        {
            if (i == currentTargetIndex)
            {
                Vector2 photoPos = photos[i].position;
                float distance = Vector2.Distance(mousePos, photoPos);

                if (distance <= connectionThreshold)
                {
                    // 连接成功
                    connectedPhotos++;
                    ShowDialogue();
                    StartCoroutine(NextPhoto());
                    break;
                }
            }
        }
    }

    void HighlightTargetPhoto()
    {
        // 重置所有照片
        ResetPhotos();

        // 高亮目标照片
        if (currentTargetIndex >= 0 && currentTargetIndex < photoImages.Length)
        {
            Image targetImage = photoImages[currentTargetIndex];
            if (targetImage != null)
            {
                targetImage.color = new Color(highlightIntensity, highlightIntensity, highlightIntensity);
            }
        }
    }

    void ResetPhotos()
    {
        foreach (Image image in photoImages)
        {
            if (image != null)
            {
                image.color = Color.white;
            }
        }
    }

    void ShowDialogue()
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            int dialogueIndex = Mathf.Min(connectedPhotos - 1, dialogueTexts.Length - 1);
            dialogueText.text = dialogueTexts[dialogueIndex];
            dialoguePanel.SetActive(true);
        }
    }

    IEnumerator NextPhoto()
    {
        yield return new WaitForSeconds(highlightDuration);

        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 移动到下一个目标照片
        currentTargetIndex = (currentTargetIndex + 1) % photos.Length;
        HighlightTargetPhoto();
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
}
