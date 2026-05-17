using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Scene7_01Manager : MonoBehaviour
{
    [Header("=== 场景对象 ===")]
    public GameObject deskBg;
    public GameObject deskStaticItems;
    public GameObject diaryClose;
    public GameObject diaryPuzzleUI;
    public GameObject photoItem;
    public GameObject xiangLeLe;
    public GameObject dialogPanel;
    public Camera mainCamera;

    [Header("=== 相机位置 ===")]
    public Transform cameraDeskView;
    public Transform cameraPhotoView;
    public Transform cameraHandView;
    public Transform cameraLeLeFaceView;

    [Header("=== 泪滴设置 ===")]
    public GameObject tearPrefab;
    public Transform[] tearSpawnPoints;

    [Header("=== 对话设置 ===")]
    public TextMeshProUGUI dialogText;
    public float dialogDuration = 2f;

    [Header("=== 拖拽转场设置 ===")]
    public float dragThreshold = 0.2f;
    public float itemMoveDuration = 1f;
    public Transform[] itemsToMove;

    [Header("=== 调试设置 ===")]
    public bool showDebugLog = true;

    private SceneState currentState;
    private Camera mainCam;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private float originalCameraSize;
    private GameObject[] activeTears;
    private int currentTearIndex;
    private int currentDialogIndex;

    public enum SceneState
    {
        State0_Idle,
        State1_DiaryPuzzle,
        State2_PhotoInteract,
        State3_TearWipe,
        State4_Transition
    }

    void Awake()
    {
        mainCam = mainCamera != null ? mainCamera : Camera.main;
        originalCameraPos = mainCam.transform.position;
        originalCameraRot = mainCam.transform.rotation;
        originalCameraSize = mainCam.orthographicSize;
    }

    void Start()
    {
        InitializeScene();
        SetState(SceneState.State0_Idle);
    }

    void InitializeScene()
    {
        SetActive(deskBg, true);
        SetActive(deskStaticItems, true);
        SetActive(diaryClose, true);
        SetActive(diaryPuzzleUI, false);
        SetActive(photoItem, false);
        SetActive(xiangLeLe, false);
        SetActive(dialogPanel, false);

        if (diaryClose != null)
        {
            AddClickHandler(diaryClose, OnDiaryClicked);
        }
    }

    void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }

    void AddClickHandler(GameObject obj, System.Action callback)
    {
        if (obj == null) return;

        Button btn = obj.GetComponent<Button>();
        if (btn == null)
        {
            btn = obj.AddComponent<Button>();
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => callback?.Invoke());
    }

    void SetState(SceneState newState)
    {
        currentState = newState;
        if (showDebugLog)
        {
            Debug.Log("切换状态: " + newState.ToString());
        }

        switch (newState)
        {
            case SceneState.State0_Idle:
                HandleState0();
                break;
            case SceneState.State1_DiaryPuzzle:
                HandleState1();
                break;
            case SceneState.State2_PhotoInteract:
                HandleState2();
                break;
            case SceneState.State3_TearWipe:
                HandleState3();
                break;
            case SceneState.State4_Transition:
                HandleState4();
                break;
        }
    }

    void HandleState0()
    {
        SetActive(diaryClose, true);
    }

    void HandleState1()
    {
        SetActive(diaryClose, false);
        SetActive(diaryPuzzleUI, true);
    }

    void HandleState2()
    {
        SetActive(diaryPuzzleUI, false);
        SetActive(photoItem, true);

        if (photoItem != null)
        {
            AddClickHandler(photoItem, OnPhotoClicked);
        }
    }

    void HandleState3()
    {
        SetActive(photoItem, false);
        SetActive(xiangLeLe, true);

        CameraToPosition(cameraLeLeFaceView);

        SpawnTears();

        if (xiangLeLe != null)
        {
            AddClickHandler(xiangLeLe, OnLeLeClicked);
        }
    }

    void HandleState4()
    {
        SetActive(xiangLeLe, false);
    }

    void OnDiaryClicked()
    {
        if (currentState != SceneState.State0_Idle) return;

        SetState(SceneState.State1_DiaryPuzzle);
    }

    public void OnLeftPuzzleComplete()
    {
        if (currentState != SceneState.State1_DiaryPuzzle) return;

        ShowDialog("列车员（我）：乐乐，你已经看到了那些记忆中被忽略的真相。你要选择离开，去往顿丘吗？");
    }

    public void OnRightPuzzleComplete()
    {
        if (currentState != SceneState.State1_DiaryPuzzle) return;

        ShowDialog("象乐乐：我……我要去！我必须去顿丘！", () =>
        {
            SetState(SceneState.State2_PhotoInteract);
        });
    }

    private int photoClickCount = 0;
    void OnPhotoClicked()
    {
        if (currentState != SceneState.State2_PhotoInteract) return;

        photoClickCount++;

        switch (photoClickCount)
        {
            case 1:
                CameraToPosition(cameraPhotoView);
                break;
            case 2:
                PlayLeLePullAnimation();
                break;
            case 3:
                CameraToPosition(cameraHandView);
                PlaySkirtAnimation();
                break;
            case 4:
            case 5:
                ShowDialog(currentDialogIndex == 0 ?
                    "日记本里明明那么希望爸爸妈妈陪你，可为什么还是执意想要去往顿丘？" :
                    "能告诉我原因吗？", () =>
                    {
                        currentDialogIndex++;
                        if (currentDialogIndex >= 2)
                        {
                            currentDialogIndex = 0;
                        }
                    });
                break;
            default:
                CameraToPosition(cameraDeskView);
                SetState(SceneState.State3_TearWipe);
                break;
        }
    }

    void PlayLeLePullAnimation()
    {
        if (showDebugLog)
        {
            Debug.Log("播放象乐乐拉妈妈衣角动画");
        }
    }

    void PlaySkirtAnimation()
    {
        if (showDebugLog)
        {
            Debug.Log("播放妈妈裙摆褶皱动画");
        }
    }

    void OnLeLeClicked()
    {
        if (currentState != SceneState.State3_TearWipe) return;

        ShowDialog("列车员安抚对话...");
    }

    void SpawnTears()
    {
        activeTears = new GameObject[tearSpawnPoints.Length];

        for (int i = 0; i < tearSpawnPoints.Length; i++)
        {
            if (tearSpawnPoints[i] != null && tearPrefab != null)
            {
                GameObject tear = Instantiate(tearPrefab, tearSpawnPoints[i].position, Quaternion.identity);
                activeTears[i] = tear;

                int index = i;
                Button tearBtn = tear.GetComponent<Button>();
                if (tearBtn == null)
                {
                    tearBtn = tear.AddComponent<Button>();
                }
                tearBtn.onClick.RemoveAllListeners();
                tearBtn.onClick.AddListener(() => OnTearClicked(index));
            }
        }

        currentTearIndex = 0;
    }

    private int[] tearClickCount = new int[3];
    void OnTearClicked(int tearIndex)
    {
        if (currentState != SceneState.State3_TearWipe) return;
        if (activeTears[tearIndex] == null) return;

        tearClickCount[tearIndex]++;

        if (tearClickCount[tearIndex] == 1)
        {
            Image tearImage = activeTears[tearIndex].GetComponent<Image>();
            if (tearImage != null)
            {
                tearImage.DOFade(0.3f, 0.3f);
            }
        }
        else if (tearClickCount[tearIndex] >= 2)
        {
            Destroy(activeTears[tearIndex]);
            activeTears[tearIndex] = null;

            string[] tearDialogs = {
                "没关系，乐乐，不要害怕",
                "把真实的情况告诉我，好吗？",
                "不管是什么，我都会陪着你。"
            };

            ShowDialog(tearDialogs[tearIndex], () =>
            {
                CheckAllTearsWiped();
            });
        }
    }

    void CheckAllTearsWiped()
    {
        bool allWiped = true;
        foreach (var tear in activeTears)
        {
            if (tear != null)
            {
                allWiped = false;
                break;
            }
        }

        if (allWiped)
        {
            CameraToPosition(cameraDeskView);
            SetState(SceneState.State4_Transition);
        }
    }

    void ShowDialog(string text, System.Action onComplete = null)
    {
        if (dialogPanel == null || dialogText == null) return;

        SetActive(dialogPanel, true);
        dialogText.text = text;
        dialogText.DOFade(1f, 0.3f);

        Invoke(nameof(HideDialog), dialogDuration);

        if (onComplete != null)
        {
            Invoke("ExecuteOnComplete", dialogDuration);
            pendingOnComplete = onComplete;
        }
    }

    private System.Action pendingOnComplete;
    void ExecuteOnComplete()
    {
        pendingOnComplete?.Invoke();
        pendingOnComplete = null;
    }

    void HideDialog()
    {
        if (dialogPanel != null && dialogText != null)
        {
            dialogText.DOFade(0f, 0.3f).OnComplete(() =>
            {
                SetActive(dialogPanel, false);
            });
        }
    }

    void CameraToPosition(Transform targetPos)
    {
        if (mainCam == null || targetPos == null) return;

        mainCam.transform.DOMove(targetPos.position, 1f).SetEase(Ease.OutQuad);
        mainCam.transform.DORotate(targetPos.rotation.eulerAngles, 1f).SetEase(Ease.OutQuad);
    }

    void Update()
    {
        if (currentState == SceneState.State4_Transition)
        {
            CheckDragTransition();
        }

        if (currentState == SceneState.State2_PhotoInteract && Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI())
            {
                photoClickCount = 6;
                OnPhotoClicked();
            }
        }
    }

    private Vector2 startTouchPos;
    void CheckDragTransition()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 currentPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 delta = currentPos - startTouchPos;
            float screenWidth = Screen.width;

            if (delta.x < -screenWidth * dragThreshold)
            {
                StartTransition();
            }
        }
    }

    void StartTransition()
    {
        foreach (var item in itemsToMove)
        {
            if (item != null)
            {
                item.DOMoveX(-Screen.width, itemMoveDuration).SetEase(Ease.InQuad);
            }
        }

        Invoke(nameof(LoadNextScene), itemMoveDuration + 0.5f);
    }

    void LoadNextScene()
    {
        if (showDebugLog)
        {
            Debug.Log("加载下一关卡");
        }
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public void ResetScene()
    {
        photoClickCount = 0;
        currentDialogIndex = 0;
        tearClickCount = new int[3];
        currentTearIndex = 0;

        mainCam.transform.position = originalCameraPos;
        mainCam.transform.rotation = originalCameraRot;
        mainCam.orthographicSize = originalCameraSize;

        InitializeScene();
        SetState(SceneState.State0_Idle);
    }
}