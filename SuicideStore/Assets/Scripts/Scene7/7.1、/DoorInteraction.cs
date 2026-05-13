using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    [Header("交互设置")]
    public string nextSceneName = "Scene7.2";
    public GameObject interactionPrompt;
    public bool showDebugLog = true;

    [Header("交互方式")]
    public bool useMouseClick = true;
    public bool useKeyPress = false;
    public KeyCode interactKey = KeyCode.Space;

    [Header("音效")]
    public AudioSource doorSound;

    [Header("视觉提示")]
    public SpriteRenderer doorSprite;
    public Color highlightColor = Color.yellow;
    private Color originalColor;

    private bool isPlayerNear = false;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (doorSprite != null)
        {
            originalColor = doorSprite.color;
        }

        if (showDebugLog)
        {
            Debug.Log("DoorInteraction 已初始化，目标场景: " + nextSceneName);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            if (showDebugLog)
            {
                Debug.Log("玩家靠近门了！");
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            if (doorSprite != null)
            {
                doorSprite.color = highlightColor;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            
            if (showDebugLog)
            {
                Debug.Log("玩家离开门了");
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (doorSprite != null)
            {
                doorSprite.color = originalColor;
            }
        }
    }

    void Update()
    {
        if (isPlayerNear)
        {
            if (useMouseClick && Input.GetMouseButtonDown(0))
            {
                if (showDebugLog)
                {
                    Debug.Log("点击交互！");
                }
                InteractWithDoor();
            }

            if (useKeyPress && Input.GetKeyDown(interactKey))
            {
                if (showDebugLog)
                {
                    Debug.Log("按键交互！");
                }
                InteractWithDoor();
            }
        }
    }

    void InteractWithDoor()
    {
        if (showDebugLog)
        {
            Debug.Log("正在切换到场景: " + nextSceneName);
        }

        if (doorSound != null)
        {
            doorSound.Play();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("场景名称为空！请检查Next Scene Name设置");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
}