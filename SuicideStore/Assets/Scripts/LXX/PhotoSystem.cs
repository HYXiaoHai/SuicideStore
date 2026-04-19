using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PhotoSystem : MonoBehaviour
{
    [Header("照片设置")]
    public GameObject[] photos;
    public int currentPhotoIndex = 0;
    public float highlightIntensity = 2f;

    [Header("对话设置")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogueTexts;
    public float dialogueDuration = 2f;

    private SpriteRenderer[] photoRenderers;
    private PhotoGlow[] photoGlowComponents;
    private bool[] photoTriggered;

    void Start()
    {
        InitializePhotos();
        HighlightCurrentPhoto();
    }

    void InitializePhotos()
    {
        photoRenderers = new SpriteRenderer[photos.Length];
        photoGlowComponents = new PhotoGlow[photos.Length];
        photoTriggered = new bool[photos.Length];

        for (int i = 0; i < photos.Length; i++)
        {
            if (photos[i] != null)
            {
                photoRenderers[i] = photos[i].GetComponent<SpriteRenderer>();
                photoGlowComponents[i] = photos[i].GetComponent<PhotoGlow>();
                photoTriggered[i] = false;

                if (photoGlowComponents[i] != null)
                {
                    photoGlowComponents[i].StartGlow();
                }
            }
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void HighlightCurrentPhoto()
    {
        for (int i = 0; i < photos.Length; i++)
        {
            if (photoRenderers[i] != null)
            {
                photoRenderers[i].color = Color.white;
            }
        }

        if (currentPhotoIndex < photos.Length && photoRenderers[currentPhotoIndex] != null)
        {
            photoRenderers[currentPhotoIndex].color = new Color(highlightIntensity, highlightIntensity, highlightIntensity);
        }
    }

    public void OnPhotoTrigger(int photoIndex)
    {
        if (photoIndex == currentPhotoIndex && !photoTriggered[photoIndex])
        {
            photoTriggered[photoIndex] = true;
            ShowDialogue(photoIndex);
            StartCoroutine(NextPhoto());
        }
    }

    void ShowDialogue(int photoIndex)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            int dialogueIndex = Mathf.Min(photoIndex, dialogueTexts.Length - 1);
            dialogueText.text = dialogueTexts[dialogueIndex];
            dialoguePanel.SetActive(true);
        }

        if (photoGlowComponents[photoIndex] != null)
        {
            photoGlowComponents[photoIndex].StopGlow();
        }

        if (photoRenderers[photoIndex] != null)
        {
            photoRenderers[photoIndex].color = Color.white;
        }
    }

    IEnumerator NextPhoto()
    {
        yield return new WaitForSeconds(dialogueDuration);

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        currentPhotoIndex = (currentPhotoIndex + 1) % photos.Length;
        HighlightCurrentPhoto();
    }
}
