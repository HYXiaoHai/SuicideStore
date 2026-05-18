using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S11_1_Manager : MonoBehaviour
{
    [Header("=== 场景设置 ===")]
    public int currentSceneIndex = 0;
    public string[] nextSceneNames;
    public float sceneTransitionDuration = 1f;

    [Header("=== 小象设置 ===")]
    public Transform leLe;
    public float[] moveSpeeds;
    public float rightBound = 8f;

    [Header("=== 雾气设置 ===")]
    public Image fogImage;
    public float fogRiseDuration = 2f;
    public float fogAlpha = 0.6f;

    [Header("=== 妈妈剪影设置 ===")]
    public SpriteRenderer motherSilhouette;
    public Sprite halfSilhouetteSprite;
    public Sprite fullSilhouetteSprite;

    [Header("=== 对话设置 ===")]
    public GameObject dialogPanel;
    public TextMeshProUGUI dialogText;
    public float dialogDuration = 3f;

    [Header("=== 转场覆盖层 ===")]
    public Image transitionImage;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private SceneState currentState;
    private bool canMove = false;
    private bool canInteract = false;
    private bool isTransitioning = false;
    private float currentSpeed;
    private Vector3 originalLeLePos;

    public enum SceneState
    {
        Scene1_Start,
        Scene2_WithHalfSilhouette,
        Scene3_MoveFaster,
        Scene4_WithFullSilhouette,
        Scene5_Interaction
    }

    void Start()
    {
        originalLeLePos = leLe.position;
        currentState = SceneState.Scene1_Start;
        currentSpeed = moveSpeeds[0];

        SetupFog();
        SetupTransition();

        EnterScene1();
    }

    void SetupFog()
    {
        if (fogImage != null)
        {
            fogImage.color = new Color(fogImage.color.r, fogImage.color.g, fogImage.color.b, 0);
        }
    }

    void SetupTransition()
    {
        if (transitionImage != null)
        {
            transitionImage.gameObject.SetActive(true);
            transitionImage.color = Color.black;
            transitionImage.DOFade(0f, 1f).OnComplete(() =>
            {
                transitionImage.gameObject.SetActive(false);
            });
        }
    }

    void EnterScene1()
    {
        currentState = SceneState.Scene1_Start;
        currentSceneIndex = 0;
        currentSpeed = moveSpeeds[0];

        ResetLeLePosition();
        HideMotherSilhouette();
        HideDialog();

        if (fogImage != null)
        {
            fogImage.DOFade(fogAlpha, fogRiseDuration);
        }

        canMove = true;

        if (showDebugLog)
        {
            Debug.Log("场景1：乐乐在空白场景中移动");
        }
    }

    void EnterScene2()
    {
        currentState = SceneState.Scene2_WithHalfSilhouette;
        currentSceneIndex = 1;
        currentSpeed = moveSpeeds[1];

        ResetLeLePosition();
        ShowMotherSilhouette(true);
        ShowDialog("列车员（我）：你父母.......还没有走远");

        canMove = true;

        if (showDebugLog)
        {
            Debug.Log("场景2：出现妈妈剪影（半个），移动速度加快");
        }
    }

    void EnterScene3()
    {
        currentState = SceneState.Scene3_MoveFaster;
        currentSceneIndex = 2;
        currentSpeed = moveSpeeds[2];

        ResetLeLePosition();
        HideDialog();

        canMove = true;

        if (showDebugLog)
        {
            Debug.Log("场景3：移动速度更快");
        }
    }

    void EnterScene4()
    {
        currentState = SceneState.Scene4_WithFullSilhouette;
        currentSceneIndex = 3;
        currentSpeed = moveSpeeds[3];

        ResetLeLePosition();
        ShowMotherSilhouette(false);
        ShowDialog("列车员（我）：他们的记忆，快要消失了");

        canMove = true;

        if (showDebugLog)
        {
            Debug.Log("场景4：出现完整妈妈剪影");
        }
    }

    void EnterScene5()
    {
        currentState = SceneState.Scene5_Interaction;
        currentSceneIndex = 4;
        currentSpeed = moveSpeeds[4];

        ResetLeLePosition();
        HideDialog();

        canMove = true;
        canInteract = true;

        if (showDebugLog)
        {
            Debug.Log("场景5：可以与妈妈交互");
        }
    }

    void ResetLeLePosition()
    {
        if (leLe != null)
        {
            leLe.position = originalLeLePos;
        }
    }

    void ShowMotherSilhouette(bool isHalf)
    {
        if (motherSilhouette != null)
        {
            motherSilhouette.gameObject.SetActive(true);
            motherSilhouette.sprite = isHalf ? halfSilhouetteSprite : fullSilhouetteSprite;
            motherSilhouette.DOFade(1f, 0.5f);
        }
    }

    void HideMotherSilhouette()
    {
        if (motherSilhouette != null)
        {
            motherSilhouette.DOFade(0f, 1f).OnComplete(() =>
            {
                motherSilhouette.gameObject.SetActive(false);
            });
        }
    }

    void ShowDialog(string text)
    {
        if (dialogPanel != null && dialogText != null)
        {
            dialogPanel.SetActive(true);
            dialogText.text = text;
            dialogText.alpha = 0;
            dialogText.DOFade(1f, 0.3f);
        }
    }

    void HideDialog()
    {
        if (dialogPanel != null && dialogText != null)
        {
            dialogText.DOFade(0f, 0.3f).OnComplete(() =>
            {
                dialogPanel.SetActive(false);
            });
        }
    }

    void Update()
    {
        if (!canMove || isTransitioning) return;

        HandleMovement();

        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            OnInteractWithMother();
        }
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.D))
        {
            if (leLe != null)
            {
                leLe.position += Vector3.right * currentSpeed * Time.deltaTime;

                if (leLe.position.x >= rightBound)
                {
                    OnReachRightBound();
                }
            }
        }
    }

    void OnReachRightBound()
    {
        canMove = false;

        if (showDebugLog)
        {
            Debug.Log("到达最右侧，切换场景");
        }

        StartCoroutine(TransitionToNextScene());
    }

    System.Collections.IEnumerator TransitionToNextScene()
    {
        isTransitioning = true;

        if (transitionImage != null)
        {
            transitionImage.gameObject.SetActive(true);
            transitionImage.DOFade(1f, sceneTransitionDuration);

            yield return new WaitForSeconds(sceneTransitionDuration);

            TransitionToNextState();

            yield return new WaitForSeconds(0.5f);

            transitionImage.DOFade(0f, sceneTransitionDuration).OnComplete(() =>
            {
                transitionImage.gameObject.SetActive(false);
                isTransitioning = false;
            });
        }
        else
        {
            TransitionToNextState();
            isTransitioning = false;
        }
    }

    void TransitionToNextState()
    {
        switch (currentState)
        {
            case SceneState.Scene1_Start:
                EnterScene2();
                break;
            case SceneState.Scene3_MoveFaster:
                EnterScene4();
                break;
        }
    }

    void OnInteractWithMother()
    {
        canInteract = false;

        if (showDebugLog)
        {
            Debug.Log("与小象妈妈交互");
        }

        ShowDialog("象妈妈：乐乐...你怎么跑到这里来了?你不该来的，不该来这里啊...", () =>
        {
            HideMotherSilhouette();
            canMove = true;
        });
    }

    void ShowDialog(string text, System.Action onComplete = null)
    {
        if (dialogPanel != null && dialogText != null)
        {
            dialogPanel.SetActive(true);
            dialogText.text = text;
            dialogText.alpha = 0;
            dialogText.DOFade(1f, 0.3f);

            Invoke(nameof(HideDialog), dialogDuration);

            if (onComplete != null)
            {
                Invoke("ExecuteOnComplete", dialogDuration);
                pendingOnComplete = onComplete;
            }
        }
    }

    private System.Action pendingOnComplete;
    void ExecuteOnComplete()
    {
        pendingOnComplete?.Invoke();
        pendingOnComplete = null;
    }

    public void ResetScene()
    {
        currentState = SceneState.Scene1_Start;
        canMove = false;
        canInteract = false;
        isTransitioning = false;
        pendingOnComplete = null;

        EnterScene1();
    }
}