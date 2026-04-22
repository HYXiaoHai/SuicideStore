using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    [Header("交互设置")]
    public string nextSceneName = "Scene_Memory";
    public GameObject interactionPrompt;

    [Header("音效")]
    public AudioSource doorSound;

    private bool isPlayerNear = false;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetMouseButtonDown(0))
        {
            InteractWithDoor();
        }
    }

    void InteractWithDoor()
    {
        if (doorSound != null)
        {
            doorSound.Play();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
