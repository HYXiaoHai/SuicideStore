using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class BrokenPhotoClick : MonoBehaviour
{
    public Image[] photoStates;
    public Text[] dialogueTexts;
    public string nextSceneName;

    private int clickStep = 0;
    private bool isDrawingComplete = false;

    void Start()
    {
        foreach (var text in dialogueTexts)
        {
            text.canvasRenderer.SetAlpha(0);
        }

        for (int i = 0; i < photoStates.Length; i++)
        {
            photoStates[i].gameObject.SetActive(i == 0);
        }
    }

    public void OnDrawingStarted()
    {
    }

    public void OnDrawingComplete()
    {
        isDrawingComplete = true;
        dialogueTexts[0].DOFade(1, 0.6f);
    }

    public void OnPhotoClick()
    {
        switch (clickStep)
        {
            case 0:
                photoStates[0].gameObject.SetActive(false);
                photoStates[1].gameObject.SetActive(true);
                dialogueTexts[1].DOFade(1, 0.6f);
                clickStep++;
                break;

            case 1:
                photoStates[1].gameObject.SetActive(false);
                photoStates[2].gameObject.SetActive(true);
                dialogueTexts[2].DOFade(1, 0.6f);
                photoStates[2].rectTransform.DOScale(1.05f, 0.3f).SetLoops(2, LoopType.Yoyo);
                clickStep++;
                break;

            case 2:
                StartCoroutine(FadeToBlackAndLoadScene());
                break;
        }
    }

    System.Collections.IEnumerator FadeToBlackAndLoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}