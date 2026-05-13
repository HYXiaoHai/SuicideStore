using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElephantFall : MonoBehaviour
{
    [Header("=== 角色设置 ===")]
    public Rigidbody2D elephantRb;

    [Header("=== 气球设置 ===")]
    public SpriteRenderer balloonSprite;
    public Sprite normalBalloonSprite;
    public Sprite poppedBalloonSprite;

    [Header("=== 转场设置 ===")]
    public CanvasGroup whiteFadePanel;
    public string nextSceneName;
    public float fallDelay = 1.5f;
    public float fadeDuration = 1.0f;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private bool isBalloonClicked = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (elephantRb != null)
        {
            elephantRb.gravityScale = 0;
            elephantRb.isKinematic = true;
        }

        if (whiteFadePanel != null)
        {
            whiteFadePanel.alpha = 0;
            whiteFadePanel.gameObject.SetActive(false);
        }

        if (balloonSprite != null && balloonSprite.GetComponent<Collider2D>() == null)
        {
            balloonSprite.gameObject.AddComponent<CircleCollider2D>();
        }
    }

    void Update()
    {
        if (!isBalloonClicked && Input.GetMouseButtonDown(0))
        {
            CheckBalloonClick();
        }
    }

    void CheckBalloonClick()
    {
        if (balloonSprite == null) return;

        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos);

        if (hitCollider != null && hitCollider.gameObject == balloonSprite.gameObject)
        {
            OnBalloonClicked();
        }
    }

    public void OnBalloonClicked()
    {
        if (isBalloonClicked) return;
        isBalloonClicked = true;

        if (showDebugLog)
        {
            Debug.Log("气球被点击，小象开始下落");
        }

        if (balloonSprite != null && poppedBalloonSprite != null)
        {
            balloonSprite.sprite = poppedBalloonSprite;
        }

        if (elephantRb != null)
        {
            elephantRb.isKinematic = false;
            elephantRb.gravityScale = 1;
        }

        Invoke(nameof(WhiteFadeAndTransition), fallDelay);
    }

    void WhiteFadeAndTransition()
    {
        if (whiteFadePanel != null)
        {
            whiteFadePanel.gameObject.SetActive(true);
            whiteFadePanel.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    SceneManager.LoadScene(nextSceneName);
                }
                else if (showDebugLog)
                {
                    Debug.LogWarning("nextSceneName 未设置");
                }
            });
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (showDebugLog)
        {
            Debug.Log("小象碰撞到: " + collision.gameObject.name);
        }
    }
}