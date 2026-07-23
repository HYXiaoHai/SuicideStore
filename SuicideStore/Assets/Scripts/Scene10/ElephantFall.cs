using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElephantFall : MonoBehaviour
{
    [Header("=== 角色设置 ===")]
    // 移除Rigidbody2D重力相关，改用匀速下落
    public Transform elephantTrans;
    public float fallSpeed = 2f; // 小象匀速下落速度

    [Header("=== 气球设置 ===")]
    public SpriteRenderer balloonSprite;
    public float balloonUpwardDistance = 3f;
    public float balloonMoveDuration = 0.5f;

    [Header("文案")]
    public TMP_Text text1;
    public TMP_Text text2;

    [Header("音效")]
    public AudioClip balloonSound;

    [Header("=== 场景设置 ===")]
    public string nextSceneName = "S10-10.6-door";
    public bool shouldUseFade = true;
    public float fallDelay = 1.5f;

    [Header("=== 落地平台设置 ===")]
    public Transform groundPlatform; // 目标落地平台
    private bool isFalling = false;
    private bool isLanded = false;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private bool isBalloonClicked = false;
    private Camera mainCamera;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.white);

        mainCamera = Camera.main;

        // 移除刚体重力/运动学设置，不再使用Rigidbody2D控制下落
        if (elephantTrans == null)
            elephantTrans = transform;

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

        // 匀速下落逻辑 + 落地判断
        if (isFalling && !isLanded && groundPlatform != null)
        {
            float targetY = groundPlatform.position.y;
            // 向下匀速移动
            elephantTrans.Translate(Vector2.down * fallSpeed * Time.deltaTime);

            // 到达平台高度，停止下落，保持直立
            if (elephantTrans.position.y <= targetY)
            {
                isLanded = true;
                elephantTrans.position = new Vector3(elephantTrans.position.x, targetY, elephantTrans.position.z);
                if (showDebugLog)
                    Debug.Log("小象已平稳落到平台上");
            }
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
        text1.DOFade(0f, 0.5f);
        text2.DOFade(1f, 0.5f);
        // 气球上升动画 保留
        if (balloonSprite != null)
        {
            Vector3 originalPos = balloonSprite.transform.position;
            balloonSprite.transform.DOMoveY(originalPos.y + balloonUpwardDistance, balloonMoveDuration).SetEase(Ease.OutQuad);
        }

        // 播放气球音效 保留
        AudioManager.Instance.Play2DSound(balloonSound, 0.8f);

        // 开启下落状态
        isFalling = true;

        // 延迟加载场景 保留原逻辑
        Invoke(nameof(LoadNextScene), fallDelay);
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            CompleteLevel();
        }
        else if (showDebugLog)
        {
            Debug.LogWarning("nextSceneName 未设置");
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
                if (shouldUseFade)
                {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(0.5f, Color.black, () =>
                    {
                        // 转场完成后加载新场景
                        SceneManager.LoadScene(nextScene);
                    });
                    AudioManager.Instance.FadeOutCurrentBGM(0.5f, null);
                }
                else
                {
                    SceneManager.LoadScene(nextScene);
                }
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }

    // 原有碰撞检测保留
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (showDebugLog)
        {
            Debug.Log("小象碰撞到: " + collision.gameObject.name);
        }
    }
}