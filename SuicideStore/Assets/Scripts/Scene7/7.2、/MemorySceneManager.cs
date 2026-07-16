using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class MemorySceneManager : MonoBehaviour
{
    [Header("Memory Images")]
    public Image[] memoryImages;

    [Header("Slides")]
    public SlideController[] slides;

    [Header("走路音效")]
    public List<AudioClip> walkClip;

    [Header("Scene Settings")]
    public string nextSceneName = "Scene7.3";

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float delayBetweenItems = 0.3f;

    private int currentSlideIndex = 0;
    private int lastSliderIndex = 0;

    void Start()
    {
        // 并行执行转场淡出和 BGM 淡出
        if(TransitionManage.Instance!=null)
        TransitionManage.Instance.FadeIn(0.5f, Color.white);

        InitializeScene();
        lastSliderIndex = slides.Length-1;
    }
    void Update()
    {
        // 根据设置面板状态，动态禁用/启用所有滑块的交互
        bool isSetting = GameManage.Instance.isSetting;
        foreach (var slide in slides)
        {
            if (slide != null)
            {
                // 禁用脚本即可阻止拖拽事件
                slide.GetComponent<Slider>().interactable = !isSetting;
                // 当设置面板打开时，强制复位滑块（避免残留位置）
                if (isSetting)
                    slide.ResetSlide();
            }
        }
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
                slideCanvasGroup.DOFade(1f, fadeInDuration);
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
            int index = Random.Range(0, walkClip.Count);
            AudioManager.Instance.Play2DSound(walkClip[index], 1f);

            memoryImages[currentSlideIndex].gameObject.SetActive(true);
            memoryImages[currentSlideIndex].CrossFadeAlpha(1, fadeInDuration, false);
            yield return new WaitForSeconds(fadeInDuration + delayBetweenItems);
        }

        if (currentSlideIndex < slides.Length)
        {
            slides[currentSlideIndex].gameObject.SetActive(true);
            CanvasGroup slideCanvasGroup = slides[currentSlideIndex].GetComponent<CanvasGroup>();
            if (slideCanvasGroup != null)
               slideCanvasGroup.DOFade(1f,fadeInDuration);
            yield return new WaitForSeconds(fadeInDuration);
        }
        else
        {
            StartCoroutine(LoadNextScene());
        }
    }
    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(1f);
        CompleteLevel();
    }
    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                // 并行执行转场淡出和 BGM 淡出
                TransitionManage.Instance.FadeOut(0.5f, Color.white, () =>
                {
                    // 转场完成后加载新场景
                    SceneManager.LoadScene(nextScene);
                });
                AudioManager.Instance.FadeOutCurrentBGM(0.5f, null);
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
}
