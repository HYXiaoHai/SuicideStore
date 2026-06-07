using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Scene12Manager : MonoBehaviour
{
    [Header("=== 图片设置 ===")]
    public Image firstImage;
    public Image secondImage;

    [Header("=== 转场设置 ===")]
    public float fadeDuration = 1.0f;
    public string nextSceneName;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private bool canClick = true;
    private bool isTransitioning = false;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f, Color.black);

        if (firstImage != null)
        {
            firstImage.gameObject.SetActive(true);
            firstImage.canvasRenderer.SetAlpha(1);
        }

        if (secondImage != null)
        {
            secondImage.gameObject.SetActive(false);
            secondImage.canvasRenderer.SetAlpha(0);
        }
    }

    void Update()
    {
        if (canClick && !isTransitioning && Input.GetMouseButtonDown(0))
        {
            OnScreenClick();
        }
    }

    void OnScreenClick()
    {
        if (showDebugLog)
        {
            Debug.Log("屏幕点击，开始切换图片");
        }

        canClick = false;
        isTransitioning = true;

        StartCoroutine(SwitchImages());
    }

    IEnumerator SwitchImages()
    {
        if (secondImage != null)
        {
            secondImage.gameObject.SetActive(true);
            secondImage.canvasRenderer.SetAlpha(0);
            secondImage.CrossFadeAlpha(1, fadeDuration, false);
        }

        if (firstImage != null)
        {
            firstImage.CrossFadeAlpha(0, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);

        if (firstImage != null)
        {
            firstImage.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FadeToBlackAndLoadScene());
    }

    IEnumerator FadeToBlackAndLoadScene()
    {
        Image blackFade = new GameObject("BlackFade").AddComponent<Image>();
        blackFade.rectTransform.SetParent(GetComponent<RectTransform>(), false);
        blackFade.rectTransform.anchorMin = Vector2.zero;
        blackFade.rectTransform.anchorMax = Vector2.one;
        blackFade.rectTransform.sizeDelta = Vector2.zero;
        blackFade.color = Color.black;
        blackFade.canvasRenderer.SetAlpha(0);
        blackFade.CrossFadeAlpha(1, fadeDuration, false);

        yield return new WaitForSeconds(fadeDuration);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            if (showDebugLog)
            {
                Debug.LogWarning("nextSceneName 未设置，场景不会切换");
            }
        }
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
                TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                {
                    // 转场完成后加载新场景
                    SceneManager.LoadScene(nextScene);
                });
                //AudioManager.Instance.FadeOutCurrentBGM(1f, null);
            }
        }
        else
        {
            TransitionManage.Instance.FadeOut(1f, Color.black, () =>
            {
                // 转场完成后加载新场景
                SceneManager.LoadScene("End");
            });

        }
    }
}