using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElephantFall : MonoBehaviour
{
    [Header("=== 角色设置 ===")]
    public Rigidbody2D elephantRb;

    [Header("=== 气球设置 ===")]
    public SpriteRenderer balloonSprite;
    public float balloonUpwardDistance = 3f;
    public float balloonMoveDuration = 0.5f;

    [Header("=== 场景设置 ===")]
    public string nextSceneName = "S10-10.6-door";
    public float fallDelay = 1.5f;

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
            Debug.Log("气球被点击，气球上升，小象开始下落");
        }

        if (balloonSprite != null)
        {
            Vector3 originalPos = balloonSprite.transform.position;
            balloonSprite.transform.DOMoveY(originalPos.y + balloonUpwardDistance, balloonMoveDuration).SetEase(Ease.OutQuad);
        }

        if (elephantRb != null)
        {
            elephantRb.isKinematic = false;
            elephantRb.gravityScale = 1;
        }

        Invoke(nameof(LoadNextScene), fallDelay);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else if (showDebugLog)
        {
            Debug.LogWarning("nextSceneName 未设置");
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