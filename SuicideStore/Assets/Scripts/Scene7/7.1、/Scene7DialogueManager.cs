using UnityEngine;
using TMPro;
using System.Collections;

public class Scene7DialogueManager : MonoBehaviour
{
    public static Scene7DialogueManager Instance;

    [Header("Dialogue Settings")]
    public TMP_Text textDisplay;
    public float typingSpeed = 0.05f;
    public float displayDuration = 3f;

    [Header("Trigger Points")]
    public Transform[] dialogueTriggers;
    public string[] dialogueContent;

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
        if (dialogueTriggers.Length != dialogueContent.Length)
        {
            Debug.LogError("Dialogue trigger count does not match dialogue content count!");
            return;
        }

        triggered = new bool[dialogueTriggers.Length];
        for (int i = 0; i < triggered.Length; i++)
        {
            triggered[i] = false;
        }

        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
    }

    public void CheckTriggerPosition(Vector3 position)
    {
        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            if (!triggered[i] && position.x >= dialogueTriggers[i].position.x)
            {
                ShowDialogue(i);
                triggered[i] = true;
            }
        }
    }

    public void ShowDialogue(int index)
    {
        if (index < 0 || index >= dialogueContent.Length)
        {
            return;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(TypeDialogue(dialogueContent[index]));
    }

    IEnumerator TypeDialogue(string text)
    {
        if (textDisplay == null)
        {
            yield break;
        }

        textDisplay.text = "";
        textDisplay.gameObject.SetActive(true);

        foreach (char c in text)
        {
            textDisplay.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(displayDuration);

        textDisplay.gameObject.SetActive(false);
        currentCoroutine = null;
    }
}
