using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class S10CameraControl : MonoBehaviour
{
    [Header("S10-puzzle-10.3 (Completion Sequence)")]
    [SerializeField] private bool enableCompletionSequence = true;
    [SerializeField] private bool enableGroupCompletion = true;
    [SerializeField] private string completionGroupId = "S10-puzzle-10.3";
    [SerializeField] private int completionGroupExpectedPieces = 5;
    [SerializeField] private string completionGroupMasterObjectName = "P1";

    [Header("Puzzle Move Animation")]
    [SerializeField] private Transform puzzleRootToMove;
    [SerializeField] private Transform puzzleMoveTarget;
    [SerializeField] private string puzzleMoveTargetName = "1";
    [SerializeField] private bool moveXOnly;
    [SerializeField] private bool alignPuzzleByAnchor = true;
    [SerializeField] private Transform puzzleAlignAnchor;
    [SerializeField] private string puzzleAlignAnchorName = "拼图底";
    [SerializeField] private float puzzleMoveDuration = 0.6f;

    [Header("Background & Click Reveal")]
    [SerializeField] private GameObject background2;
    [SerializeField] private string background2Name = "背景2";
    [SerializeField] private bool hideBackground2OnStart = true;
    [SerializeField] private bool revealObjectOnClickAfterSolved = true;
    [SerializeField] private GameObject clickRevealObject;
    [SerializeField] private string clickRevealObjectName = "2";

    [Header("Camera Vertical Move")]
    [SerializeField] private bool enableCameraVerticalMoveAfterSolved = true;
    [SerializeField] private Transform cameraTransformOverride;
    [SerializeField] private Transform cameraClampTop;
    [SerializeField] private string cameraClampTopName = "bg";
    [SerializeField] private Transform cameraClampBottom;
    [SerializeField] private string cameraClampBottomName = "bg (1)";
    [SerializeField] private float cameraScrollSpeed = 1.8f;
    [SerializeField] private float cameraMiddleDragSpeed = 1.0f;

    // Private state
    private Camera mainCamera;
    private Transform cachedCameraTransform;
    private Vector3 cameraBasePosition;
    private float cameraMinY;
    private float cameraMaxY;
    private bool cameraControlEnabled;
    private bool cameraDragging;
    private Vector2 cameraDragLastMouse;
    private float cameraWorldPerPixelY;
    private float nextBackground2ResolveTime;
    private bool clickRevealDone;
    private bool completionMoveActive;
    private float completionMoveStartTime;
    private float completionMoveInvDuration;
    private Vector3 completionMoveFrom;
    private Vector3 completionMoveTo;
    private bool completionDone;

    private static readonly Dictionary<string, int> groupPlacedMaskById = new Dictionary<string, int>(8);
    private static readonly Dictionary<string, bool> groupCompletedById = new Dictionary<string, bool>(8);
    private static readonly Dictionary<string, bool> groupSequenceStartedById = new Dictionary<string, bool>(8);
    private static int lastInitializedSceneHandle = -1;
    private static readonly List<Transform> sceneTraversalStack = new List<Transform>(256);

    private void Awake()
    {
        int sceneHandle = SceneManager.GetActiveScene().handle;
        if (sceneHandle != lastInitializedSceneHandle)
        {
            groupPlacedMaskById.Clear();
            groupCompletedById.Clear();
            groupSequenceStartedById.Clear();
            lastInitializedSceneHandle = sceneHandle;
        }

        mainCamera = Camera.main;
        if (cameraTransformOverride != null)
            cachedCameraTransform = cameraTransformOverride;
        else if (mainCamera != null)
            cachedCameraTransform = mainCamera.transform;

        if (cachedCameraTransform != null)
            cameraBasePosition = cachedCameraTransform.position;
    }

    private void Start()
    {
        ResolveReferences();
        if (hideBackground2OnStart && background2 != null && background2.activeSelf)
            background2.SetActive(false);

        // 监听拼图核心的完成事件 (需要在 Inspector 中手动绑定，或者通过代码查找)
        S10PuzzleCore puzzleCore = FindObjectOfType<S10PuzzleCore>();
        if (puzzleCore != null)
            puzzleCore.onAllPiecesPlaced.AddListener(BeginCompletionSequence);
        else
            Debug.LogWarning("S10CameraControl: No S10PuzzleCore found in scene.");
    }

    private void Update()
    {
        if (completionMoveActive)
            UpdateCompletionMove();

        if (cameraControlEnabled)
            UpdateCameraControl();
    }

    private void ResolveReferences()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        if (puzzleRootToMove == null)
        {
            Transform parent = transform.parent;
            if (parent != null && parent.name == "puzzle")
                puzzleRootToMove = parent;
            else
            {
                GameObject puzzle = FindGameObjectIncludingInactive("puzzle", roots);
                puzzleRootToMove = puzzle != null ? puzzle.transform : transform;
            }
        }

        if (puzzleMoveTarget == null && !string.IsNullOrEmpty(puzzleMoveTargetName))
        {
            GameObject t = GameObject.Find(puzzleMoveTargetName);
            if (t != null)
                puzzleMoveTarget = t.transform;
            else
            {
                GameObject inactive = FindGameObjectIncludingInactive(puzzleMoveTargetName, roots);
                if (inactive != null)
                    puzzleMoveTarget = inactive.transform;
            }
        }

        if (puzzleAlignAnchor == null && !string.IsNullOrEmpty(puzzleAlignAnchorName))
        {
            if (puzzleRootToMove != null)
                puzzleAlignAnchor = FindTransformIncludingInactive(puzzleRootToMove, puzzleAlignAnchorName);
            if (puzzleAlignAnchor == null)
            {
                GameObject go = FindGameObjectIncludingInactive(puzzleAlignAnchorName, roots);
                if (go != null)
                    puzzleAlignAnchor = go.transform;
            }
        }

        if (background2 == null && !string.IsNullOrEmpty(background2Name))
            background2 = FindGameObjectIncludingInactive(background2Name, roots);

        if (clickRevealObject == null && !string.IsNullOrEmpty(clickRevealObjectName))
            clickRevealObject = FindGameObjectIncludingInactive(clickRevealObjectName, roots);

        if (cameraClampTop == null && !string.IsNullOrEmpty(cameraClampTopName))
        {
            GameObject go = GameObject.Find(cameraClampTopName);
            if (go == null)
                go = FindGameObjectIncludingInactive(cameraClampTopName, roots);
            if (go != null) cameraClampTop = go.transform;
        }

        if (cameraClampBottom == null && !string.IsNullOrEmpty(cameraClampBottomName))
        {
            GameObject go = GameObject.Find(cameraClampBottomName);
            if (go == null)
                go = FindGameObjectIncludingInactive(cameraClampBottomName, roots);
            if (go != null) cameraClampBottom = go.transform;
        }

        if (cameraClampTop != null && cameraClampBottom != null)
        {
            float yA = cameraClampTop.position.y;
            float yB = cameraClampBottom.position.y;
            cameraMinY = Mathf.Min(yA, yB);
            cameraMaxY = Mathf.Max(yA, yB);
        }
        else if (cachedCameraTransform != null)
        {
            cameraMinY = cachedCameraTransform.position.y;
            cameraMaxY = cachedCameraTransform.position.y;
        }

        if (mainCamera != null && mainCamera.orthographic)
            cameraWorldPerPixelY = (mainCamera.orthographicSize * 2f) / Mathf.Max(1, Screen.height);
        else
            cameraWorldPerPixelY = 0.01f;
    }

    private void BeginCompletionSequence()
    {
        if (!enableCompletionSequence || completionDone) return;

        if (enableGroupCompletion && !string.IsNullOrEmpty(completionGroupId))
        {
            if (!IsGroupCompleted(completionGroupId))
                NotifyGroupPiecePlaced();
            if (!IsGroupCompleted(completionGroupId))
                return;
            if (IsGroupSequenceStarted(completionGroupId))
                return;
            groupSequenceStartedById[completionGroupId] = true;
        }

        if (puzzleRootToMove == null || puzzleMoveTarget == null)
        {
            completionDone = true;
            RevealBackground2AndEnableCamera();
            return;
        }

        completionMoveActive = true;
        completionMoveStartTime = Time.unscaledTime;
        completionMoveInvDuration = puzzleMoveDuration > 0.0001f ? 1f / puzzleMoveDuration : 0f;
        completionMoveFrom = puzzleRootToMove.position;
        Vector3 to;
        if (alignPuzzleByAnchor && puzzleAlignAnchor != null)
        {
            Vector3 delta = puzzleMoveTarget.position - puzzleAlignAnchor.position;
            to = completionMoveFrom + delta;
        }
        else
        {
            to = puzzleMoveTarget.position;
        }
        if (moveXOnly)
            to.y = completionMoveFrom.y;
        to.z = completionMoveFrom.z;
        completionMoveTo = to;
    }

    private void NotifyGroupPiecePlaced()
    {
        int bit = GetGroupPieceBit();
        if (bit == 0) return;

        int mask = 0;
        groupPlacedMaskById.TryGetValue(completionGroupId, out mask);
        mask |= bit;
        groupPlacedMaskById[completionGroupId] = mask;

        int expected = BuildExpectedMask(completionGroupExpectedPieces);
        if (expected != 0 && mask == expected)
            groupCompletedById[completionGroupId] = true;
    }

    private int GetGroupPieceBit()
    {
        string n = gameObject.name;
        if (string.IsNullOrEmpty(n)) return 0;
        if (n.Length >= 2 && n[0] == 'P')
        {
            int d = n[n.Length - 1] - '0';
            if (d >= 1 && d <= 30) return 1 << d;
        }
        return 0;
    }

    private static int BuildExpectedMask(int expectedPieces)
    {
        int mask = 0;
        for (int i = 1; i <= Mathf.Clamp(expectedPieces, 0, 30); i++)
            mask |= 1 << i;
        return mask;
    }

    private static bool IsGroupCompleted(string groupId)
    {
        return groupCompletedById.TryGetValue(groupId, out bool done) && done;
    }

    private static bool IsGroupSequenceStarted(string groupId)
    {
        return groupSequenceStartedById.TryGetValue(groupId, out bool started) && started;
    }

    private void UpdateCompletionMove()
    {
        float t = completionMoveInvDuration > 0f ? (Time.unscaledTime - completionMoveStartTime) * completionMoveInvDuration : 1f;
        if (t >= 1f)
        {
            t = 1f;
            completionMoveActive = false;
            completionDone = true;
        }
        if (puzzleRootToMove != null)
            puzzleRootToMove.position = Vector3.LerpUnclamped(completionMoveFrom, completionMoveTo, t);

        if (!completionMoveActive)
            RevealBackground2AndEnableCamera();
    }

    private void RevealBackground2AndEnableCamera()
    {
        if (background2 != null && !background2.activeSelf)
            background2.SetActive(true);

        if (!enableCameraVerticalMoveAfterSolved || cachedCameraTransform == null)
            return;

        cameraControlEnabled = true;
        float y = Mathf.Clamp(cameraBasePosition.y, cameraMinY, cameraMaxY);
        cachedCameraTransform.position = new Vector3(cameraBasePosition.x, y, cameraBasePosition.z);
    }

    private void UpdateCameraControl()
    {
        if (cachedCameraTransform == null) return;

        EnsureBackground2Visible();
        TryRevealObjectOnClick();

        float dy = 0f;

        Vector2 scroll = Input.mouseScrollDelta;
        if (scroll.y != 0f)
            dy += -scroll.y * cameraScrollSpeed;

        if (Input.GetMouseButtonDown(2))
        {
            cameraDragging = true;
            cameraDragLastMouse = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            cameraDragging = false;
        }

        if (cameraDragging && Input.GetMouseButton(2))
        {
            Vector2 now = Input.mousePosition;
            float pixelDeltaY = now.y - cameraDragLastMouse.y;
            cameraDragLastMouse = now;
            dy += -pixelDeltaY * cameraWorldPerPixelY * cameraMiddleDragSpeed;
        }

        if (dy == 0f) return;

        Vector3 pos = cachedCameraTransform.position;
        float newY = Mathf.Clamp(pos.y + dy, cameraMinY, cameraMaxY);
        cachedCameraTransform.position = new Vector3(cameraBasePosition.x, newY, cameraBasePosition.z);
    }

    private void EnsureBackground2Visible()
    {
        if (background2 == null && !string.IsNullOrEmpty(background2Name))
        {
            float now = Time.unscaledTime;
            if (now >= nextBackground2ResolveTime)
            {
                background2 = FindGameObjectIncludingInactive(background2Name, SceneManager.GetActiveScene().GetRootGameObjects());
                nextBackground2ResolveTime = now + 0.5f;
            }
        }
        if (background2 != null && !background2.activeSelf)
            background2.SetActive(true);
    }

    private void TryRevealObjectOnClick()
    {
        if (!revealObjectOnClickAfterSolved || clickRevealDone) return;

        bool clicked = Input.GetMouseButtonDown(0);
        if (!clicked && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            clicked = true;

        if (!clicked) return;

        if (clickRevealObject == null && !string.IsNullOrEmpty(clickRevealObjectName))
            clickRevealObject = FindGameObjectIncludingInactive(clickRevealObjectName, SceneManager.GetActiveScene().GetRootGameObjects());

        if (clickRevealObject != null && !clickRevealObject.activeSelf)
            clickRevealObject.SetActive(true);

        clickRevealDone = true;
    }

    private static Transform FindTransformIncludingInactive(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        if (root.name == name) return root;
        sceneTraversalStack.Clear();
        sceneTraversalStack.Add(root);
        while (sceneTraversalStack.Count > 0)
        {
            int last = sceneTraversalStack.Count - 1;
            Transform t = sceneTraversalStack[last];
            sceneTraversalStack.RemoveAt(last);
            if (t == null) continue;
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
                sceneTraversalStack.Add(t.GetChild(i));
        }
        return null;
    }

    private static GameObject FindGameObjectIncludingInactive(string name, GameObject[] roots)
    {
        if (string.IsNullOrEmpty(name) || roots == null) return null;
        sceneTraversalStack.Clear();
        foreach (GameObject root in roots)
        {
            if (root == null) continue;
            if (root.name == name) return root;
            sceneTraversalStack.Add(root.transform);
        }
        while (sceneTraversalStack.Count > 0)
        {
            int last = sceneTraversalStack.Count - 1;
            Transform t = sceneTraversalStack[last];
            sceneTraversalStack.RemoveAt(last);
            if (t == null) continue;
            if (t.name == name) return t.gameObject;
            for (int i = 0; i < t.childCount; i++)
                sceneTraversalStack.Add(t.GetChild(i));
        }
        return null;
    }
}