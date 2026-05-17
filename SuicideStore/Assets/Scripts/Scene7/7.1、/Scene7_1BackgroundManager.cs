using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Scene7_1BackgroundManager : MonoBehaviour
{
    [Header("背景图片")]
    public Image[] backgroundImages;

    [Header("碰撞触发器")]
    public Collider2D[] triggerColliders;

    [Header("淡入淡出设置")]
    public float fadeDuration = 1f;

    private bool[] triggered;
    private int currentImageIndex = -1;

    void Start()
    {
        triggered = new bool[triggerColliders.Length];

        for (int i = 0; i < backgroundImages.Length; i++)
        {
            if (backgroundImages[i] != null)
            {
                backgroundImages[i].gameObject.SetActive(true);
                backgroundImages[i].canvasRenderer.SetAlpha(0);
            }
        }
    }

    void Update()
    {
        CheckTriggers();
    }

    void CheckTriggers()
    {
        for (int i = 0; i < triggerColliders.Length; i++)
        {
            if (!triggered[i] && triggerColliders[i] != null)
            {
                Collider2D playerCollider = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider2D>();

                if (playerCollider != null && triggerColliders[i].bounds.Intersects(playerCollider.bounds))
                {
                    triggered[i] = true;
                    _OnTriggerEnter(i);
                }
            }
        }
    }
    //修改：OnTriggerEnter ->_OnTriggerEnter
    void _OnTriggerEnter(int index)
    {
        if (index == 0)
        {
            StartCoroutine(FadeInImage(0));
        }
        else if (index == 1)
        {
            StartCoroutine(FadeOutImageAndFadeInNext(0, 1));
        }
    }

    IEnumerator FadeInImage(int imageIndex)
    {
        if (imageIndex < 0 || imageIndex >= backgroundImages.Length || backgroundImages[imageIndex] == null)
        {
            yield break;
        }

        currentImageIndex = imageIndex;
        backgroundImages[imageIndex].CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }

    IEnumerator FadeOutImageAndFadeInNext(int imageToFadeOut, int imageToFadeIn)
    {
        if (imageToFadeOut >= 0 && imageToFadeOut < backgroundImages.Length && backgroundImages[imageToFadeOut] != null)
        {
            backgroundImages[imageToFadeOut].CrossFadeAlpha(0f, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);

        if (imageToFadeIn >= 0 && imageToFadeIn < backgroundImages.Length && backgroundImages[imageToFadeIn] != null)
        {
            currentImageIndex = imageToFadeIn;
            backgroundImages[imageToFadeIn].CrossFadeAlpha(1f, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);
    }
}