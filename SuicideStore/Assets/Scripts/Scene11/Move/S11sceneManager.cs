using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class S11_1_Manager : MonoBehaviour
{
    [Header("=== 场景设置 ===")]
    public int currentRound = 1;//当前轮次 1,2,3
    public float sceneTransitionDuration = 1f;

    [Header("=== 小象设置 ===")]
    public ReversalPlayerController player;//玩家控制脚本
    public float playerSpeed1 = 3f;
    public float playerSpeed2 = 5f;
    public float playerSpeed3 = 7f;
    public float rightBound = 8f;          // 右侧边界 X 坐标
    public Transform playerStartPosition;   // 每轮起始点（最左侧）

    [Header("=== 妈妈剪影 ===")]
    public SpriteRenderer motherSilhouette;
    public Transform motherPosition;//剪影放置位置（最右侧）
    public Sprite halfSilhouetteSprite;//半身（第二轮）
    public Sprite fullSilhouetteSprite;//全身（第三轮）
    [Header("=== 交互区域 ===")]
    public SpriteRenderer ePromote;
    public Collider2D motherZoneCollider;   // 妈妈身上的触发器碰撞体

    private bool isInMotherZone = false;    // 玩家是否在交互区域内
    [Header("=== 对话设置 ===")]
    public Image[] roundImages = new Image[4]; // 0:第一轮图片,1:第二轮图片,2:第三轮图片1,3:第三轮图片2
    public float dialogDuration = 3f;

    [Header("=== 转场动画 ===")]
    public Image transitionImage;

    [Header("下一个场景")]
    public string nextSceneName;
    // 内部状态
    private bool canMove = true;            // 玩家是否可移动
    private bool isTransitioning = false;   // 是否正在转场
    private bool hasInteractedInRound3 = false; // 第三轮是否已交互
    private bool motherFading = false;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f,Color.black);

        // 初始化：从第一轮开始
        currentRound = 1;
        canMove = true;
        isTransitioning = false;
        hasInteractedInRound3 = false;

        if (player != null)
        {
            player.SetCanMoveRight(true);
            player.SetCanMove(true);
            Debug.Log("S11_1_Manager: 玩家已初始化，可以移动");
        }
        else
        {
            Debug.LogError("S11_1_Manager: player 引用未赋值！请在 Inspector 中拖入玩家对象");
        }
        // 进入第一轮
        EnterRound(1);
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!canMove || isTransitioning) return;

        // 检查是否到达右侧边界
        if (player != null && player.transform.position.x >= rightBound)
        {
            OnReachRightBound();
        }

        // 第三轮：按 E 与妈妈交互（且必须在区域内）
        if (currentRound == 3 && !hasInteractedInRound3 && isInMotherZone)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithMother();
            }
        }
    }
    /// <summary> 到达右侧边界时的处理 </summary>
    void OnReachRightBound()
    {
        // 第三轮且未交互 → 阻止前进
        if (currentRound == 3 && !hasInteractedInRound3)
        {
            Debug.Log("第三轮：必须先与妈妈交互（按E）才能继续前进");
            return;
        }

        // 所有轮次：触发切到下一轮
        canMove = false;
        StartCoroutine(TransitionToNextRound());
    }

    IEnumerator TransitionToNextRound()
    {
        isTransitioning = true;

        // 淡入黑屏
        if (transitionImage != null)
        {
            transitionImage.gameObject.SetActive(true);
            transitionImage.DOFade(1f, sceneTransitionDuration);
            yield return new WaitForSeconds(sceneTransitionDuration);
        }

        // 进入下一轮
        if (currentRound == 1)
            EnterRound(2);
        else if (currentRound == 2)
            EnterRound(3);
        else if (currentRound == 3)
        {
            OnAllComplete();
            yield break;
        }

        // 淡出黑屏，恢复移动
        if (transitionImage != null)
        {
            transitionImage.DOFade(0f, sceneTransitionDuration);
            yield return new WaitForSeconds(sceneTransitionDuration);
            transitionImage.gameObject.SetActive(false);
        }

        isTransitioning = false;
        canMove = true;
    }

    /// <summary> 进入指定轮次 (1,2,3) </summary>
    void EnterRound(int round)
    {
        currentRound = round;
        hasInteractedInRound3 = false;

        ResetPlayerPosition();

        if (player != null)
        {
            switch (round)
            {
                case 1: player.moveSpeed = playerSpeed1; break;
                case 2: player.moveSpeed = playerSpeed2; break;
                case 3: player.moveSpeed = playerSpeed3; break;
            }
            // 每轮开始时确保可以向右移动（第三轮也先允许，进入区域后再禁用）
            player.SetCanMoveRight(true);
        }

        switch (round)
            {
                case 1:
                    HideMotherSilhouette();
                    HideDialog();
                    break;
                case 2:
                    ShowMotherSilhouette(halfSilhouetteSprite);
                    ShowDialog(roundImages[0]);
                    break;
                case 3:
                    ShowMotherSilhouette(fullSilhouetteSprite);
                    HideDialog(roundImages[0]);
                    ShowDialog(roundImages[1]);
                    if (motherZoneCollider != null)
                        motherZoneCollider.enabled = true;
                    isInMotherZone = false;
                    break;
            }

        Debug.Log($"进入第 {round} 轮，速度={player?.moveSpeed}");
    }

    //与妈妈交互
    void InteractWithMother()
    {
        if (hasInteractedInRound3) return;
        hasInteractedInRound3 = true;

        // 恢复向右移动
        if (player != null)
            player.SetCanMoveRight(true);
        // 交互后隐藏提示
        HideEPrompt();
        //ShowDialog(roundImages[2], () =>
        //{

        //});
        HideDialog(roundImages[1]);
        ShowDialog(roundImages[2], null);
        ShowDialog(roundImages[3], null);
        // 2. 同时妈妈渐隐消失（与文案显示同步进行）
        if (motherSilhouette != null)
        {
            motherSilhouette.DOFade(0f, 1f).OnComplete(() =>
            {
                motherSilhouette.gameObject.SetActive(false);
                // 交互结束后禁用碰撞体
                if (motherZoneCollider != null)
                    motherZoneCollider.enabled = false;
            });
        }
    }
    // 进入交互区域
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        Debug.Log("进入区域");
        if (currentRound == 3 && !hasInteractedInRound3 && other.CompareTag("Player"))
        {
            isInMotherZone = true;
            // 禁止向右移动，但可以向左
            if (player != null)
                player.SetCanMoveRight(false);
            ShowEPrompt(); // 添加
            Debug.Log("进入妈妈区域，禁止向右移动");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (currentRound == 3 && !hasInteractedInRound3 && other.CompareTag("Player"))
        {
            isInMotherZone = false;
            if (player != null)
                player.SetCanMoveRight(true);
            HideEPrompt(); // 添加
            Debug.Log("离开妈妈区域");
        }
    }

    //所有的都完成了
    public void OnAllComplete()
    {
        SceneManager.LoadScene(nextSceneName);

    }

    // ---------- 辅助方法 ----------
    private void ShowEPrompt()
    {
        if (ePromote == null) return;
        ePromote.DOKill();
        ePromote.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);
        // 可选缩放动画
        ePromote.transform.localScale = Vector3.one * 0.8f;
        ePromote.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }

    private void HideEPrompt()
    {
        if (ePromote == null) return;
        ePromote.DOKill();
        ePromote.DOFade(0f, 0.1f);
        ePromote.transform.DOScale(0.8f, 0.1f);
    }

    void ResetPlayerPosition()
    {
        if (player != null && playerStartPosition != null)
        {
            player.transform.position = playerStartPosition.position;
            // 可选：重置刚体速度等
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }
    }

    void ShowMotherSilhouette(Sprite sprite)
    {
        if (motherSilhouette != null)
        {
            motherSilhouette.sprite = sprite;
            motherSilhouette.gameObject.SetActive(true);
            motherSilhouette.color = new Color(1, 1, 1, 0);
            motherSilhouette.DOFade(1f, 0.5f);

            // 将剪影放置在指定位置（最右侧）
            if (motherPosition != null)
                motherSilhouette.transform.position = motherPosition.position;
        }
    }

    void HideMotherSilhouette()
    {
        if (motherSilhouette != null && motherSilhouette.gameObject.activeSelf)
        {
            motherSilhouette.DOFade(0f, 0.5f).OnComplete(() =>
            {
                motherSilhouette.gameObject.SetActive(false);
            });
        }
    }

    void ShowDialog(Image image, System.Action onComplete = null)
    {
        if (image == null) return;

        image.gameObject.SetActive(true);
        Color c = image.color;
        c.a = 0;
        image.color = c;
        image.DOFade(1f, 0.3f);

        //DOVirtual.DelayedCall(dialogDuration, () =>
        //{
        //    HideDialog(image);
        //    onComplete?.Invoke();
        //});
    }

    void HideDialog(Image image)
    {
        if (image == null) return;
        image.DOFade(0f, 0.3f).OnComplete(() =>
        {
            image.gameObject.SetActive(false);
        });
    }

    void HideDialog()
    {
        foreach (Image img in roundImages)
        {
            if (img != null && img.gameObject.activeSelf)
            {
                HideDialog(img);
            }
        }
    }
}