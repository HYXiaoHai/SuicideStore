using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scene7DialogueManager : MonoBehaviour
{
    public static Scene7DialogueManager Instance;

    [Header("图片淡入淡出设置")]
    public float fadeDuration = 0.5f;

    [Header("触发点")]
    public Transform[] dialogueTriggers;
    
    [Header("交互按键")]
    public KeyCode interactKey = KeyCode.E;

    [Header("图片内容")]
    public Image[] images;

    private bool[] triggered;
    private Coroutine currentCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (dialogueTriggers.Length != images.Length)
        {
            Debug.LogError("触发点数量与图片数量不匹配！");
            return;
        }

        triggered = new bool[dialogueTriggers.Length];
        for (int i = 0; i < triggered.Length; i++)
        {
            triggered[i] = false;
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null)
            {
                images[i].gameObject.SetActive(false);
                images[i].canvasRenderer.SetAlpha(0);
            }
        }
    }

    public void CheckTriggerPosition(Vector3 position)
    {
        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            if (!triggered[i] && position.x >= dialogueTriggers[i].position.x)
            {
                ShowImage(i);
                triggered[i] = true;
            }
        }
    }

    public void ShowImage(int index)
    {
        if (index < 0 || index >= images.Length)
        {
            return;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(FadeInImage(index));
    }

    IEnumerator FadeInImage(int index)
    {
        if (index > 0 && images[index - 1] != null)
        {
            images[index - 1].CrossFadeAlpha(0, fadeDuration, false);
            yield return new WaitForSeconds(fadeDuration);
            images[index - 1].gameObject.SetActive(false);
        }

        if (images[index] != null)
        {
            images[index].gameObject.SetActive(true);
            images[index].CrossFadeAlpha(1, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);
        currentCoroutine = null;
    }
}