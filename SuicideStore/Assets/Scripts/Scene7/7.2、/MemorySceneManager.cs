using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MemorySceneManager : MonoBehaviour
{
    [Header("Memory Images")]
    public Image[] memoryImages;

    [Header("Slides")]
    public SlideController[] slides;

    [Header("Audio")]
    public AudioSource knockSound;

    [Header("Scene Settings")]
    public string nextSceneName = "Scene7.3";

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float delayBetweenItems = 0.3f;

    private int currentSlideIndex = 0;

    void Start()
    {
        InitializeScene();
    }

    void InitializeScene()
    {
        for (int i = 0; i < memoryImages.Length; i++)
        {
            if (memoryImages[i] != null)
            {
                memoryImages[i].gameObject.SetActive(false);
                memoryImages[i].canvasRenderer.SetAlpha(0);
            }
        }

        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null)
            {
                slides[i].slideIndex = i;
                slides[i].memoryManager = this;
                slides[i].gameObject.SetActive(false);
                
                CanvasGroup slideCanvasGroup = slides[i].GetComponent<CanvasGroup>();
                if (slideCanvasGroup == null)
                {
                    slideCanvasGroup = slides[i].gameObject.AddComponent<CanvasGroup>();
                }
                slideCanvasGroup.alpha = 0;
            }
        }

        StartCoroutine(ShowInitialItems());
    }

    IEnumerator ShowInitialItems()
    {
        if (memoryImages.Length > 0 && memoryImages[0] != null)
        {
            memoryImages[0].gameObject.SetActive(true);
            memoryImages[0].CrossFadeAlpha(1, fadeInDuration, false);
            yield return new WaitForSeconds(fadeInDuration);
        }

        if (slides.Length > 0 && slides[0] != null)
        {
            slides[0].gameObject.SetActive(true);
            CanvasGroup slideCanvasGroup = slides[0].GetComponent<CanvasGroup>();
            if (slideCanvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(slideCanvasGroup, 0, 1, fadeInDuration));
            }
            yield return new WaitForSeconds(fadeInDuration);
        }
    }

    public void OnSlideComplete(int slideIndex)
    {
        if (slideIndex == currentSlideIndex)
        {
            currentSlideIndex++;
            StartCoroutine(ShowNextItems());
        }
    }

    IEnumerator ShowNextItems()
    {
        if (currentSlideIndex < memoryImages.Length)
        {
            memoryImages[currentSlideIndex].gameObject.SetActive(true);
            memoryImages[currentSlideIndex].CrossFadeAlpha(1, fadeInDuration, false);
            yield return new WaitForSeconds(fadeInDuration + delayBetweenItems);
        }

        if (currentSlideIndex < slides.Length)
        {
            slides[currentSlideIndex].gameObject.SetActive(true);
            CanvasGroup slideCanvasGroup = slides[currentSlideIndex].GetComponent<CanvasGroup>();
            if (slideCanvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(slideCanvasGroup, 0, 1, fadeInDuration));
            }
            yield return new WaitForSeconds(fadeInDuration);
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    public void OnLastImageClick()
    {
        if (knockSound != null)
        {
            knockSound.Play();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Invoke("LoadNextScene", 1f);
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
