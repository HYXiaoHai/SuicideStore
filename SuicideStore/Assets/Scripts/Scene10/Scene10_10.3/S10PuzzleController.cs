using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class S10PuzzleController : MonoBehaviour
{
    [System.Serializable]
    public struct PieceBinding
    {
        public Transform piece;
        public Transform target;
        public float snapRadius;
        public float magnetRadius;
        public float magnetPositionStrength;
        public float magnetRotationSpeed;
        public bool lockRotationToTarget;
    }

    [Header("Bindings (5 pieces)")]
    [SerializeField] private PieceBinding[] bindings = new PieceBinding[5];
    [SerializeField] private string[] pieceNames = new string[5] { "P1", "P2", "P3", "P4", "P5" };
    [SerializeField] private string[] targetNames = new string[5] { "T1", "T2", "T3", "T4", "T5" };
    [SerializeField] private LayerMask pieceLayerMask = ~0;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private int dragSortingOrderBoost = 100;
    [SerializeField] private bool resetToStartIfNotSnapped = true;
    [SerializeField] private bool enableDebugLogs;
    [SerializeField] private float defaultSnapRadius = 1.5f;
    [SerializeField] private float defaultMagnetRadiusMultiplier = 3f;
    [SerializeField] private bool autoRadiusFromSpriteBounds = true;
    [SerializeField] private float autoSnapRadiusMultiplier = 0.6f;
    [SerializeField] private bool useSpriteBoundsCenterForSnap = true;
    [SerializeField] private bool useScreenSpaceDistances = true;
    [SerializeField] private float snapRadiusPixels = 90f;
    [SerializeField] private float magnetRadiusPixels = 260f;
    [SerializeField] private bool autoSnapWhileDragging = true;
    [SerializeField] private float autoSnapRadiusPixels = 140f;
    [SerializeField] private float autoSnapRadiusWorld = 1.5f;
    [SerializeField] private bool ignorePieceToPieceCollisions = true;
    [SerializeField] private bool makePieceCollidersTriggers = true;
    [SerializeField] private bool autoUnparentPiecesFromEachOther = true;
    [SerializeField] private bool freezeOtherPiecesWhileDragging = true;
    [SerializeField] private bool reparentPiecesToControllerOnStart = true;

    [Header("Events")]
    public UnityEvent onAllPiecesPlaced;

    [Header("S10-puzzle-10.3 (Completion)")]
    [SerializeField] public bool enableCompletionSequence = true;
    //[SerializeField] private bool enableGroupCompletion = true;
    //[SerializeField] private string completionGroupId = "S10-puzzle-10.3";
    //[SerializeField] private int completionGroupExpectedPieces = 5;
    //[SerializeField] private string completionGroupMasterObjectName = "P1";
    //[SerializeField] private bool autoHookOnAllPiecesPlaced = true;
    //[SerializeField] private Transform puzzleRootToMove;
    [SerializeField] private Transform puzzleMoveTarget;
    //[SerializeField] private string puzzleMoveTargetName = "1";
    //[SerializeField] private bool moveXOnly;
    //[SerializeField] private bool alignPuzzleByAnchor = true;
    //[SerializeField] private Transform puzzleAlignAnchor;
    //[SerializeField] private string puzzleAlignAnchorName = "拼图底";
    [SerializeField] private float puzzleMoveDuration = 0.6f;
    [SerializeField] private GameObject background2;
    //[SerializeField] private string background2Name = "背景2";
    //[SerializeField] private bool hideBackground2OnStart = true;
    //[SerializeField] private bool revealObjectOnClickAfterSolved = true;
    //[SerializeField] private GameObject clickRevealObject;
    //[SerializeField] private string clickRevealObjectName = "2";
    public GameObject puzzleFather;           // 拼图的父物体，需要拖拽赋值
    private bool completionAnimating = false; // 防止动画重复执行

    [Header("S10-puzzle-10.3 (Camera Vertical Move)")]
    public Transform targetCamera;
    public bool enableCameraVerticalMoveAfterSolved = true;
    //[SerializeField] private Transform cameraTransformOverride;
    //[SerializeField] private Transform cameraClampTop;
    //[SerializeField] private string cameraClampTopName = "bg";
    //[SerializeField] private Transform cameraClampBottom;
    //[SerializeField] private string cameraClampBottomName = "bg (1)";
    //[SerializeField] private float cameraScrollSpeed = 1.8f;
    //[SerializeField] private float cameraMiddleDragSpeed = 1.0f;
    //[SerializeField] private bool enableAutoCameraMoveAfterSolved = true;
    public float autoCameraMoveDuration = 1.0f;
    public Transform position1;
    public Transform position2;
    public Transform position3;


    [System.Serializable]
    public struct StarMessageGroup
    {
        public CanvasGroup rootCanvas;//文字+Image显示
        public Transform star;//交互的星星物体
    }

    [Header("Stars Click Sequence")]
    //[SerializeField] private Transform[] starClickAreas = new Transform[3];
    //[SerializeField] private string[] starNames = new string[3] { "Star1", "Star2", "Star3" };
    [SerializeField] private StarMessageGroup[] starMessageGroups = new StarMessageGroup[3];
    [SerializeField] private float starFadeDuration = 0.5f;

    [Header("Final Camera Move")]
    //[SerializeField] private bool enableFinalCameraMove = true;
    //[SerializeField] private float finalCameraTargetY = -5f;
    //[SerializeField] private float finalCameraMoveDuration = 1.0f;

    [Header("Final Auto Messages")]
    [SerializeField] private bool enableFinalAutoMessages = true;
    [SerializeField] private bool finalMessagesCompleted = false;
    [SerializeField] private TMPro.TextMeshProUGUI[] finalTexts = new TMPro.TextMeshProUGUI[3];
    [SerializeField] private float finalMessageDelay = 2.0f;
    [SerializeField] private float finalMessagesCompleteDelay = 2.0f;
    public string nextSceneName;
    private readonly Collider2D[] overlapBuffer = new Collider2D[16];

    private Transform[] pieceTransforms;
    private Transform[] targetTransforms;
    private Collider2D[] pieceColliders;
    private SpriteRenderer[] pieceSpriteRenderers;
    private Vector3[] startPositions;
    private Quaternion[] startRotations;
    private float[] snapRadiusSqr;
    private float[] magnetRadiusSqr;
    private float[] magnetInvRadiusSqr;
    private float[] magnetPositionStrength;
    private float[] magnetRotationSpeed;
    private bool[] lockRotationToTarget;
    private bool[] placed;

    // NEW: Rigidbody2D support to prevent multiple pieces from being dragged together
    private Rigidbody2D[] pieceRigidbodies;
    private bool[] originalKinematicStates;

    private int placedCount;
    private int draggingIndex = -1;
    private Vector3 dragOffsetWorld;
    private float draggingZ;
    private int draggingOriginalSortingOrder;
    private int[] cachedSortingOrder;
    private Collider2D[][] perPieceColliders;
    private Transform[] originalParents;
    private Vector3[] frozenPositions;
    private Quaternion[] frozenRotations;
    private bool othersFrozen;

    //private bool completionMoveActive;
    //private float completionMoveStartTime;
    //private float completionMoveInvDuration;
    //private Vector3 completionMoveFrom;
    //private Vector3 completionMoveTo;
    private bool completionDone;

    //private Transform cachedCameraTransform;
    //private Vector3 cameraBasePosition;
    //private float cameraMinY;
    //private float cameraMaxY;
    //private bool cameraControlEnabled;
    //private bool cameraDragging;
    //private Vector2 cameraDragLastMouse;
    //private float cameraWorldPerPixelY;
    //private float nextBackground2ResolveTime;
    //private bool clickRevealDone;
    //private bool autoCameraMoving;
    //private float autoCameraMoveStartTime;
    //private float autoCameraMoveInvDuration;
    //private Vector3 autoCameraMoveFrom;
    //private Vector3 autoCameraMoveTo;

    private bool[] starsClicked = new bool[3];
    private int starsClickedCount;
    private bool starsPhaseActive;
    //private bool finalCameraMoving;
    //private float finalCameraMoveStartTime;
    //private float finalCameraMoveInvDuration;
    //private Vector3 finalCameraMoveFrom;
    //private Vector3 finalCameraMoveTo;
    private bool finalMessagesPhase;
    private int currentFinalMessageIndex;
    private float lastFinalMessageTime;

    private static readonly System.Collections.Generic.Dictionary<string, int> groupPlacedMaskById = new System.Collections.Generic.Dictionary<string, int>(8);
    private static readonly System.Collections.Generic.Dictionary<string, bool> groupCompletedById = new System.Collections.Generic.Dictionary<string, bool>(8);
    private static readonly System.Collections.Generic.Dictionary<string, bool> groupSequenceStartedById = new System.Collections.Generic.Dictionary<string, bool>(8);
    private static int lastInitializedSceneHandle = -1;
    private static readonly System.Collections.Generic.List<Transform> sceneTraversalStack = new System.Collections.Generic.List<Transform>(256);

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

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        //if (autoHookOnAllPiecesPlaced && onAllPiecesPlaced != null)
        //    onAllPiecesPlaced.AddListener(BeginCompletionSequence);

        HideAllMessagesOnStart();

        //ResolveSequenceReferencesIfNeeded();

        int count = bindings != null ? bindings.Length : 0;
        pieceTransforms = new Transform[count];
        targetTransforms = new Transform[count];
        pieceColliders = new Collider2D[count];
        pieceSpriteRenderers = new SpriteRenderer[count];
        startPositions = new Vector3[count];
        startRotations = new Quaternion[count];
        snapRadiusSqr = new float[count];
        magnetRadiusSqr = new float[count];
        magnetInvRadiusSqr = new float[count];
        magnetPositionStrength = new float[count];
        magnetRotationSpeed = new float[count];
        lockRotationToTarget = new bool[count];
        placed = new bool[count];
        cachedSortingOrder = new int[count];
        perPieceColliders = new Collider2D[count][];
        originalParents = new Transform[count];
        frozenPositions = new Vector3[count];
        frozenRotations = new Quaternion[count];

        // NEW: Initialize Rigidbody2D arrays
        pieceRigidbodies = new Rigidbody2D[count];
        originalKinematicStates = new bool[count];

        placedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Transform p = bindings[i].piece;
            Transform t = bindings[i].target;

            if (p == null && pieceNames != null && i < pieceNames.Length)
            {
                string n = pieceNames[i];
                if (!string.IsNullOrEmpty(n))
                {
                    GameObject go = GameObject.Find(n);
                    if (go != null) p = go.transform;
                }
            }

            if (t == null && targetNames != null && i < targetNames.Length)
            {
                string n = targetNames[i];
                if (!string.IsNullOrEmpty(n))
                {
                    GameObject go = GameObject.Find(n);
                    if (go != null) t = go.transform;
                }
            }

            pieceTransforms[i] = p;
            targetTransforms[i] = t;
            lockRotationToTarget[i] = bindings[i].lockRotationToTarget;

            float r = bindings[i].snapRadius;
            if (r <= 0f)
            {
                r = defaultSnapRadius;
                if (autoRadiusFromSpriteBounds && p != null)
                {
                    SpriteRenderer sr = p.GetComponentInChildren<SpriteRenderer>(true);
                    if (sr != null)
                    {
                        Bounds b = sr.bounds;
                        float candidate = Mathf.Max(b.extents.x, b.extents.y) * autoSnapRadiusMultiplier;
                        if (candidate > r)
                            r = candidate;
                    }
                }
            }
            snapRadiusSqr[i] = r * r;

            float mr = bindings[i].magnetRadius;
            if (mr <= 0f)
                mr = r * defaultMagnetRadiusMultiplier;
            magnetRadiusSqr[i] = mr * mr;
            magnetInvRadiusSqr[i] = magnetRadiusSqr[i] > 0f ? 1f / magnetRadiusSqr[i] : 0f;

            float mps = bindings[i].magnetPositionStrength;
            if (mps <= 0f)
                mps = 18f;
            magnetPositionStrength[i] = mps;

            float mrs = bindings[i].magnetRotationSpeed;
            if (mrs <= 0f)
                mrs = 18f;
            magnetRotationSpeed[i] = mrs;

            if (p != null)
            {
                originalParents[i] = p.parent;
                if (reparentPiecesToControllerOnStart && p.parent != transform)
                    p.SetParent(transform, true);
                startPositions[i] = p.position;
                startRotations[i] = p.rotation;
                pieceSpriteRenderers[i] = p.GetComponentInChildren<SpriteRenderer>(true);
                pieceColliders[i] = EnsureClickableCollider(p);
                if (pieceSpriteRenderers[i] != null)
                    cachedSortingOrder[i] = pieceSpriteRenderers[i].sortingOrder;
                perPieceColliders[i] = GetAllColliders(p);
                EnsureCollidersEnabled(perPieceColliders[i], makePieceCollidersTriggers);

                // NEW: Cache Rigidbody2D component and its initial kinematic state
                pieceRigidbodies[i] = p.GetComponent<Rigidbody2D>();
                originalKinematicStates[i] = pieceRigidbodies[i] != null && pieceRigidbodies[i].isKinematic;

                if (enableDebugLogs)
                {
                    string tn = t != null ? t.name : "<null>";
                    Debug.Log($"S10PuzzleController: Bind[{i}] piece={p.name}, target={tn}, snapRadius={Mathf.Sqrt(snapRadiusSqr[i]):F3}, magnetRadius={Mathf.Sqrt(magnetRadiusSqr[i]):F3}");
                }
            }
            else
            {
                Debug.LogWarning($"S10PuzzleController: Missing piece binding at index {i}. Assign bindings[{i}].piece or ensure a GameObject named {GetSafeName(pieceNames, i)} exists.");
            }

            if (t == null)
            {
                Debug.LogWarning($"S10PuzzleController: Missing target binding at index {i}. Assign bindings[{i}].target or ensure a GameObject named {GetSafeName(targetNames, i)} exists.");
            }
        }

        if (autoUnparentPiecesFromEachOther)
            UnparentPiecesFromEachOther();

        if (ignorePieceToPieceCollisions)
            IgnorePieceToPieceCollisions();
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        //if (completionMoveActive)
        //{
        //    UpdateCompletionMove();
        //    return;
        //}

        //if (autoCameraMoving)
        //{
        //    UpdateAutoCameraMove();
        //    return;
        //}

        //if (finalCameraMoving)
        //{
        //    UpdateFinalCameraMove();
        //    return;
        //}

        if (starsPhaseActive)
        {
            HandleStarClicks();
            return;
        }

        if (finalMessagesPhase)
        {
            UpdateFinalMessages();
            return;
        }

        if (draggingIndex < 0)
        {
            if (TryGetPointerDown(out Vector3 pointerWorld))
                TryBeginDrag(pointerWorld);
        }
        else
        {
            bool hasPointer = TryGetPointerHeld(out Vector3 pointerWorld);
            if (hasPointer)
                Drag(pointerWorld);

            if (TryGetPointerUp() || !IsPointerActive())
                EndDrag();
        }

        //if (cameraControlEnabled)
        //    UpdateCameraControl();
    }

    private void LateUpdate()
    {
        if (!freezeOtherPiecesWhileDragging || !othersFrozen || draggingIndex < 0)
            return;

        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            if (i == draggingIndex || placed[i])
                continue;

            Transform t = pieceTransforms[i];
            if (t == null)
                continue;

            t.position = frozenPositions[i];
            t.rotation = frozenRotations[i];
        }
    }

    private void TryBeginDrag(Vector3 pointerWorld)
    {
        int hitCount = Physics2D.OverlapPointNonAlloc(pointerWorld, overlapBuffer, pieceLayerMask);
        if (hitCount <= 0)
        {
            if (TryPickBySpriteBounds(pointerWorld, out int boundsPick))
            {
                BeginDragIndex(boundsPick, pointerWorld);
            }
            else if (enableDebugLogs)
            {
                Debug.Log("S10PuzzleController: Pointer down but no piece hit (Physics2D + bounds). Check colliders/layers/names.");
            }
            return;
        }

        int bestIndex = -1;
        int bestSortingOrder = int.MinValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (hit == null)
                continue;

            int idx = IndexOfCollider(hit);
            if (idx < 0 || placed[idx])
                continue;

            int sortingOrder = 0;
            SpriteRenderer sr = pieceSpriteRenderers[idx];
            if (sr != null)
                sortingOrder = sr.sortingOrder;

            if (bestIndex < 0 || sortingOrder > bestSortingOrder)
            {
                bestIndex = idx;
                bestSortingOrder = sortingOrder;
            }
        }

        if (bestIndex < 0)
            return;

        BeginDragIndex(bestIndex, pointerWorld);
    }

    private void BeginDragIndex(int idx, Vector3 pointerWorld)
    {
        Transform piece = idx >= 0 && idx < pieceTransforms.Length ? pieceTransforms[idx] : null;
        if (piece == null)
            return;

        // NEW: Freeze other pieces' Rigidbody2D in addition to Transform
        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            if (i == idx || placed[i])
                continue;

            Transform t = pieceTransforms[i];
            if (t == null)
                continue;

            frozenPositions[i] = t.position;
            frozenRotations[i] = t.rotation;

            // Make other pieces kinematic to prevent physics interference
            if (pieceRigidbodies[i] != null)
                pieceRigidbodies[i].isKinematic = true;
        }
        othersFrozen = true;

        // Optionally make the dragged piece itself kinematic for smoother manual control
        if (pieceRigidbodies[idx] != null)
            pieceRigidbodies[idx].isKinematic = true;

        draggingIndex = idx;
        draggingZ = piece.position.z;
        dragOffsetWorld = piece.position - pointerWorld;

        SpriteRenderer psr = pieceSpriteRenderers[idx];
        if (psr != null)
        {
            draggingOriginalSortingOrder = psr.sortingOrder;
            psr.sortingOrder = draggingOriginalSortingOrder + dragSortingOrderBoost;
        }
    }

    private void Drag(Vector3 pointerWorld)
    {
        int idx = draggingIndex;
        Transform piece = pieceTransforms[idx];
        if (piece == null)
        {
            draggingIndex = -1;
            return;
        }

        Vector3 desiredPos = pointerWorld + dragOffsetWorld;
        desiredPos.z = draggingZ;

        Transform target = targetTransforms[idx];
        piece.position = desiredPos;

        if (target == null)
            return;

        Vector3 targetPos = target.position;
        targetPos.z = draggingZ;

        Vector3 snapPoint = GetPieceSnapPoint(idx);
        snapPoint.z = draggingZ;

        float proximity = GetMagnetProximity(idx, snapPoint, targetPos);
        if (proximity <= 0f)
            return;

        if (proximity > 1f)
            proximity = 1f;

        float dt = Time.deltaTime;

        if (lockRotationToTarget[idx])
        {
            float rotT = magnetRotationSpeed[idx] * proximity * dt;
            if (rotT > 1f)
                rotT = 1f;
            piece.rotation = Quaternion.Slerp(piece.rotation, target.rotation, rotT);
        }

        snapPoint = GetPieceSnapPoint(idx);
        snapPoint.z = draggingZ;

        Vector3 delta = targetPos - snapPoint;
        float posT = magnetPositionStrength[idx] * proximity * dt;
        if (posT > 1f)
            posT = 1f;

        Vector3 p = piece.position;
        p.x += delta.x * posT;
        p.y += delta.y * posT;
        p.z = draggingZ;
        piece.position = p;

        if (!autoSnapWhileDragging || placed[idx])
            return;

        Vector3 finalSnapPoint = GetPieceSnapPoint(idx);
        Vector3 finalTargetPos = target.position;
        if (IsWithinAutoSnap(idx, finalSnapPoint, finalTargetPos))
        {
            SpriteRenderer psr = pieceSpriteRenderers[idx];
            if (psr != null)
                psr.sortingOrder = draggingOriginalSortingOrder;

            SnapToTarget(idx);
            draggingIndex = -1;
            othersFrozen = false;
        }
    }

    private void EndDrag()
    {
        int idx = draggingIndex;
        draggingIndex = -1;
        othersFrozen = false;

        // NEW: Restore Rigidbody2D kinematic states for all pieces
        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            if (pieceRigidbodies[i] != null)
                pieceRigidbodies[i].isKinematic = originalKinematicStates[i];
        }

        Transform piece = idx >= 0 && idx < pieceTransforms.Length ? pieceTransforms[idx] : null;
        if (piece == null)
            return;

        SpriteRenderer psr = pieceSpriteRenderers[idx];
        if (psr != null)
            psr.sortingOrder = draggingOriginalSortingOrder;

        if (TrySnap(idx))
            return;

        if (resetToStartIfNotSnapped)
        {
            piece.position = startPositions[idx];
            piece.rotation = startRotations[idx];
        }
    }

    private bool TrySnap(int idx)
    {
        Transform piece = pieceTransforms[idx];
        Transform target = targetTransforms[idx];
        if (piece == null || target == null)
        {
            if (enableDebugLogs)
                Debug.Log($"S10PuzzleController: TrySnap failed because binding is missing. idx={idx}, piece={(piece != null ? piece.name : "<null>")}, target={(target != null ? target.name : "<null>")}");
            return false;
        }

        Vector3 targetPos = target.position;
        Vector3 snapPoint = GetPieceSnapPoint(idx);
        if (!IsWithinSnap(idx, snapPoint, targetPos, out float dist, out float threshold))
        {
            if (enableDebugLogs)
                Debug.Log($"S10PuzzleController: Not within snap radius. idx={idx}, piece={piece.name}, target={target.name}, dist={dist:F3}, threshold={threshold:F3}, mode={(useScreenSpaceDistances ? "px" : "world")}");
            return false;
        }

        SnapToTarget(idx);
        return true;
    }

    private void SnapToTarget(int idx)
    {
        Transform piece = pieceTransforms[idx];
        Transform target = targetTransforms[idx];
        if (piece == null || target == null)
            return;

        Vector3 targetPos = target.position;
        Vector3 snapPoint = GetPieceSnapPoint(idx);
        Vector3 delta = targetPos - snapPoint;
        Vector3 pos = piece.position;
        pos.x += delta.x;
        pos.y += delta.y;
        piece.position = pos;

        if (lockRotationToTarget[idx])
            piece.rotation = target.rotation;

        Collider2D col = pieceColliders[idx];
        if (col != null)
            col.enabled = false;

        if (!placed[idx])
        {
            placed[idx] = true;
            placedCount++;

            if (placedCount == placed.Length)
                onAllPiecesPlaced?.Invoke();
        }
    }

    public void BeginCompletionSequence()
    {
        //if (!enableCompletionSequence || completionDone)
        //    return;

        //if (enableGroupCompletion && !string.IsNullOrEmpty(completionGroupId))
        //{
        //    if (!IsGroupCompleted(completionGroupId))
        //        NotifyGroupPiecePlaced();

        //    if (!IsGroupCompleted(completionGroupId))
        //        return;

        //    if (IsGroupSequenceStarted(completionGroupId))
        //        return;

        //    groupSequenceStartedById[completionGroupId] = true;
        //}

        //ResolveSequenceReferencesIfNeeded();

        //if (puzzleRootToMove == null || puzzleMoveTarget == null)
        //{
        //    completionDone = true;
        //    RevealBackground2AndEnableCamera();
        //    return;
        //}

        //completionMoveActive = true;
        //completionMoveStartTime = Time.unscaledTime;
        //completionMoveInvDuration = puzzleMoveDuration > 0.0001f ? 1f / puzzleMoveDuration : 0f;
        //completionMoveFrom = puzzleRootToMove.position;
        //Vector3 to;
        //if (alignPuzzleByAnchor && puzzleAlignAnchor != null)
        //{
        //    Vector3 delta = puzzleMoveTarget.position - puzzleAlignAnchor.position;
        //    to = completionMoveFrom + delta;
        //}
        //else
        //{
        //    to = puzzleMoveTarget.position;
        //}
        //bool xOnly = moveXOnly;
        //if (!string.IsNullOrEmpty(puzzleMoveTargetName) && puzzleMoveTargetName == "1")
        //    xOnly = false;
        //if (puzzleMoveTarget != null && puzzleMoveTarget.name == "1")
        //    xOnly = false;

        //if (xOnly)
        //{
        //    to.y = completionMoveFrom.y;
        //}
        //to.z = completionMoveFrom.z;
        //completionMoveTo = to;
        if (!enableCompletionSequence || completionDone || completionAnimating)
            return;

        completionAnimating = true;

        //ResolveSequenceReferencesIfNeeded();

        Sequence seq = DOTween.Sequence();

        // 1. 移动拼图父物体
        if (puzzleFather != null && puzzleMoveTarget != null)
        {
            seq.Append(puzzleFather.transform.DOMove(puzzleMoveTarget.position, puzzleMoveDuration)
                .SetEase(Ease.InOutQuad));
        }
        else if (puzzleFather != null)
        {
            seq.Append(puzzleFather.transform.DOMoveY(puzzleFather.transform.position.y + 1f, 0.5f)
                .SetEase(Ease.OutQuad));
        }
        // 2. 移动相机到 position2（使用 targetCamera）
        if (enableCameraVerticalMoveAfterSolved && targetCamera != null && position2 != null)
        {
            seq.Append(targetCamera.DOMove(position2.position, autoCameraMoveDuration)
                .SetEase(Ease.InOutQuad));
        }

        // 3. 动画完成后，启动星星机制
        seq.OnComplete(() =>
        {
            completionDone = true;
            completionAnimating = false;
            //cameraControlEnabled = false; // 星星阶段不需要手动控制相机
            StartStarsPhase();
        });

        seq.Play();
    }

    //private void NotifyGroupPiecePlaced()
    //{
    //    int bit = GetGroupPieceBit();
    //    if (bit == 0)
    //        return;

    //    int mask = 0;
    //    //groupPlacedMaskById.TryGetValue(completionGroupId, out mask);
    //    //mask |= bit;
    //    //groupPlacedMaskById[completionGroupId] = mask;

    //    //int expected = BuildExpectedMask(completionGroupExpectedPieces);
    //    //if (expected != 0 && mask == expected)
    //    //    groupCompletedById[completionGroupId] = true;
    //}

    //private static bool IsGroupCompleted(string groupId)
    //{
    //    bool done;
    //    return groupCompletedById.TryGetValue(groupId, out done) && done;
    //}

    //private static bool IsGroupSequenceStarted(string groupId)
    //{
    //    bool started;
    //    return groupSequenceStartedById.TryGetValue(groupId, out started) && started;
    //}

    //private int GetGroupPieceBit()
    //{
    //    string n = gameObject.name;
    //    if (string.IsNullOrEmpty(n))
    //        return 0;
    //    if (n.Length >= 2 && n[0] == 'P')
    //    {
    //        int d = n[n.Length - 1] - '0';
    //        if (d >= 1 && d <= 30)
    //            return 1 << d;
    //    }
    //    return 0;
    //}

    //private static int BuildExpectedMask(int expectedPieces)
    //{
    //    int count = Mathf.Clamp(expectedPieces, 0, 30);
    //    int mask = 0;
    //    for (int i = 1; i <= count; i++)
    //        mask |= 1 << i;
    //    return mask;
    //}

    //private void UpdateCompletionMove()
    //{
    //    float t = completionMoveInvDuration > 0f ? (Time.unscaledTime - completionMoveStartTime) * completionMoveInvDuration : 1f;
    //    if (t >= 1f)
    //    {
    //        t = 1f;
    //        completionMoveActive = false;
    //        completionDone = true;
    //    }

    //    if (puzzleRootToMove != null)
    //        puzzleRootToMove.position = Vector3.LerpUnclamped(completionMoveFrom, completionMoveTo, t);

    //    if (!completionMoveActive)
    //        RevealBackground2AndEnableCamera();
    //}

    //private void RevealBackground2AndEnableCamera()
    //{
    //    if (background2 == null && !string.IsNullOrEmpty(background2Name))
    //        background2 = FindGameObjectIncludingInactive(background2Name, SceneManager.GetActiveScene().GetRootGameObjects());

    //    if (background2 != null && !background2.activeSelf)
    //        background2.SetActive(true);

    //    if (!enableCameraVerticalMoveAfterSolved || cachedCameraTransform == null)
    //        return;

    //    cameraControlEnabled = true;
    //    cameraBasePosition = cachedCameraTransform.position;

    //    if (enableAutoCameraMoveAfterSolved && cameraClampBottom != null)
    //    {
    //        autoCameraMoving = true;
    //        autoCameraMoveStartTime = Time.unscaledTime;
    //        autoCameraMoveInvDuration = autoCameraMoveDuration > 0.0001f ? 1f / autoCameraMoveDuration : 0f;
    //        autoCameraMoveFrom = cachedCameraTransform.position;
    //        autoCameraMoveTo = new Vector3(cachedCameraTransform.position.x, cameraClampBottom.position.y, cachedCameraTransform.position.z);
    //    }
    //    else
    //    {
    //        float y = Mathf.Clamp(cameraBasePosition.y, cameraMinY, cameraMaxY);
    //        cachedCameraTransform.position = new Vector3(cameraBasePosition.x, y, cameraBasePosition.z);
    //    }
    //}

    //private void UpdateAutoCameraMove()
    //{
    //    if (cachedCameraTransform == null)
    //        return;

    //    float t = autoCameraMoveInvDuration > 0f ? (Time.unscaledTime - autoCameraMoveStartTime) * autoCameraMoveInvDuration : 1f;
    //    if (t >= 1f)
    //    {
    //        t = 1f;
    //        autoCameraMoving = false;
    //        StartStarsPhase();
    //    }

    //    Vector3 pos = Vector3.LerpUnclamped(autoCameraMoveFrom, autoCameraMoveTo, t);
    //    cachedCameraTransform.position = new Vector3(cameraBasePosition.x, pos.y, cameraBasePosition.z);
    //}

    private void HideAllMessagesOnStart()
    {
        for (int i = 0; i < starMessageGroups.Length; i++)
        {
            if (starMessageGroups[i].rootCanvas != null)
            {
                starMessageGroups[i].rootCanvas.alpha = 0f;
                starMessageGroups[i].rootCanvas.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < finalTexts.Length; i++)
        {
            if (finalTexts[i] != null)
            {
                finalTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private void StartStarsPhase()
    {
        starsPhaseActive = true;
        starsClicked = new bool[3];
        starsClickedCount = 0;

        for (int i = 0; i < starMessageGroups.Length; i++)
        {
            if (starMessageGroups[i].rootCanvas != null)
            {
                starMessageGroups[i].rootCanvas.gameObject.SetActive(false);
                starMessageGroups[i].rootCanvas.alpha = 0f;
            }
        }
        //ResolveStarReferences();
    }

    //private void ResolveStarReferences()
    //{
    //    for (int i = 0; i < starNames.Length; i++)
    //    {
    //        if (starClickAreas[i] == null && !string.IsNullOrEmpty(starNames[i]))
    //        {
    //            GameObject go = GameObject.Find(starNames[i]);
    //            if (go != null)
    //                starClickAreas[i] = go.transform;
    //        }
    //    }
    //}

    private void HandleStarClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Vector3 worldPos = ScreenToWorld(Input.mousePosition);

            //for (int i = 0; i < starClickAreas.Length; i++)
            //{
            //    Debug.Log("检查点击" + starsClicked[i]);
            //    if (starsClicked[i] || starClickAreas[i] == null)
            //        continue;
            //    bool hit = CheckStarClick(i, worldPos);

            //    if (hit)
            //    {
            //        ShowStarMessage(i);
            //        break;
            //    }
            //}
            Vector2 screenPos = Input.mousePosition;
            //// 先尝试使用 EventSystem 检测 UI 点击（如果星星是UI）
            //if (EventSystem.current != null)
            //{
            //    PointerEventData pointerData = new PointerEventData(EventSystem.current);
            //    pointerData.position = screenPos;
            //    var results = new System.Collections.Generic.List<RaycastResult>();
            //    EventSystem.current.RaycastAll(pointerData, results);
            //    foreach (var result in results)
            //    {
            //        for (int i = 0; i < starMessageGroups.Length; i++)
            //        {
            //            if (starsClicked[i]) continue;
            //            Transform star = starMessageGroups[i].star;
            //            if (star != null && (result.gameObject.transform == star || result.gameObject.transform.IsChildOf(star)))
            //            {
            //                ShowStarMessage(i);
            //                return;
            //            }
            //        }
            //    }
            //}

            // 否则使用 Physics2D 检测（如果是2D物体）
            Vector3 worldPos = ScreenToWorld(screenPos);
            for (int i = 0; i < starMessageGroups.Length; i++)
            {
                if (starsClicked[i]) continue;
                Transform star = starMessageGroups[i].star;
                if (star == null) continue;
                Collider2D collider = star.GetComponent<Collider2D>();
                if (collider != null && collider.OverlapPoint(worldPos))
                {
                    ShowStarMessage(i);
                    return;
                }
            }
        }
    }

    //private bool CheckStarClick(int index, Vector3 worldPos)
    //{
    //    Transform star = starClickAreas[index];
    //    Debug.Log("检查点击"+star.gameObject);
    //    Collider2D collider = star.GetComponent<Collider2D>();
    //    if (collider != null && collider.OverlapPoint(worldPos))
    //    {
    //        return true;
    //    }

    //    Vector2 screenPos = Input.mousePosition;
    //    RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(screenPos), Vector2.zero);
    //    foreach (RaycastHit2D h in hits)
    //    {
    //        if (h.collider != null && (h.collider.transform == star || h.collider.transform.IsChildOf(star)))
    //        {
    //            return true;
    //        }
    //    }

    //    if (EventSystem.current != null)
    //    {
    //        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
    //        pointerEventData.position = Input.mousePosition;

    //        System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();
    //        EventSystem.current.RaycastAll(pointerEventData, results);

    //        foreach (RaycastResult result in results)
    //        {
    //            if (result.gameObject.transform == star || result.gameObject.transform.IsChildOf(star))
    //            {
    //                return true;
    //            }
    //        }
    //    }

    //    return false;
    //}

    private void ShowStarMessage(int index)
    {
        if (starsClicked[index]) return;
        starsClicked[index] = true;
        starsClickedCount++;

        StarMessageGroup group = starMessageGroups[index];
        //if (group.rootObject != null)
        //{
        //    group.rootObject.SetActive(true);

        //    if (group.backgroundImage != null)
        //    {
        //        group.backgroundImage.canvasRenderer.SetAlpha(0);
        //        group.backgroundImage.CrossFadeAlpha(1f, starFadeDuration, false);
        //    }

        //    if (group.text != null)
        //    {
        //        group.text.canvasRenderer.SetAlpha(0);
        //        group.text.CrossFadeAlpha(1f, starFadeDuration, false);
        //    }
        //}
        if (group.rootCanvas != null)
        {
            group.rootCanvas.gameObject.SetActive(true);
            group.rootCanvas.alpha = 0f;
            group.rootCanvas.DOFade(1f, starFadeDuration).SetEase(Ease.OutQuad);
        }


        //if (starsClickedCount >= 3)
        //{
        //    starsPhaseActive = false;
        //    if (targetCamera != null && position3 != null)
        //    {
        //        //延迟1秒后移动相机
        //        DOVirtual.DelayedCall(1f, () =>
        //        {
        //            targetCamera.DOMove(position3.position, 1.0f).SetEase(Ease.InOutQuad)
        //                .OnComplete(() => StartFinalMessagesPhase());
        //        });
        //    }
        //    else
        //    {
        //        DOVirtual.DelayedCall(1f, () => StartFinalMessagesPhase());
        //    }
        //}
        if (starsClickedCount >= 3)
        {
            starsPhaseActive = false;
            if (targetCamera != null && position3 != null)
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    targetCamera.DOMove(position3.position, 1.0f).SetEase(Ease.InOutQuad)
                        .OnComplete(() => StartFinalMessagesPhase());
                });
            }
            else
            {
                DOVirtual.DelayedCall(1f, () => StartFinalMessagesPhase());
            }
        }
    }

    //private System.Collections.IEnumerator StartFinalCameraMoveAfterDelay(float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    StartFinalCameraMove();
    //}

    //private void StartFinalCameraMove()
    //{
    //    if (!enableFinalCameraMove || cachedCameraTransform == null)
    //    {
    //        StartFinalMessagesPhase();
    //        return;
    //    }

    //    finalCameraMoving = true;
    //    finalCameraMoveStartTime = Time.unscaledTime;
    //    finalCameraMoveInvDuration = finalCameraMoveDuration > 0.0001f ? 1f / finalCameraMoveDuration : 0f;
    //    finalCameraMoveFrom = cachedCameraTransform.position;
    //    finalCameraMoveTo = new Vector3(cachedCameraTransform.position.x, finalCameraTargetY, cachedCameraTransform.position.z);
    //}

    //private void UpdateFinalCameraMove()
    //{
    //    if (cachedCameraTransform == null)
    //        return;

    //    float t = finalCameraMoveInvDuration > 0f ? (Time.unscaledTime - finalCameraMoveStartTime) * finalCameraMoveInvDuration : 1f;
    //    if (t >= 1f)
    //    {
    //        t = 1f;
    //        finalCameraMoving = false;
    //        StartFinalMessagesPhase();
    //    }

    //    Vector3 pos = Vector3.LerpUnclamped(finalCameraMoveFrom, finalCameraMoveTo, t);
    //    cachedCameraTransform.position = pos;
    //}

    private void StartFinalMessagesPhase()
    {
        if (!enableFinalAutoMessages)
            return;

        finalMessagesPhase = true;
        currentFinalMessageIndex = 0;
        lastFinalMessageTime = Time.unscaledTime;
        finalMessagesCompleted = false;      // 新增标志

        for (int i = 0; i < finalTexts.Length; i++)
        {
            if (finalTexts[i] != null)
            {
                finalTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateFinalMessages()
    {
        if (currentFinalMessageIndex >= finalTexts.Length)
        {
            // 所有文案已经显示完毕，且尚未触发完成事件
            if (!finalMessagesCompleted)
            {
                finalMessagesCompleted = true;
                finalMessagesPhase = false;   
  
                DOVirtual.DelayedCall(finalMessagesCompleteDelay, () =>
                {
                    LoadNextScene();
                });
            }
            return;
        }

        float now = Time.unscaledTime;
        if (now - lastFinalMessageTime >= finalMessageDelay)
        {
            if (finalTexts[currentFinalMessageIndex] != null)
            {
                finalTexts[currentFinalMessageIndex].gameObject.SetActive(true);
                finalTexts[currentFinalMessageIndex].canvasRenderer.SetAlpha(0);
                finalTexts[currentFinalMessageIndex].CrossFadeAlpha(1f, starFadeDuration, false);
            }
            currentFinalMessageIndex++;
            lastFinalMessageTime = now;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    //private void UpdateCameraControl()
    //{
    //    if (cachedCameraTransform == null)
    //        return;

    //    //EnsureBackground2Visible();
    //    TryRevealObjectOnClick();

    //    float dy = 0f;

    //    Vector2 scroll = Input.mouseScrollDelta;
    //    if (scroll.y != 0f)
    //        dy += -scroll.y * cameraScrollSpeed;

    //    if (Input.GetMouseButtonDown(2))
    //    {
    //        cameraDragging = true;
    //        cameraDragLastMouse = Input.mousePosition;
    //    }
    //    else if (Input.GetMouseButtonUp(2))
    //    {
    //        cameraDragging = false;
    //    }

    //    if (cameraDragging && Input.GetMouseButton(2))
    //    {
    //        Vector2 now = Input.mousePosition;
    //        float pixelDeltaY = now.y - cameraDragLastMouse.y;
    //        cameraDragLastMouse = now;
    //        dy += -pixelDeltaY * cameraWorldPerPixelY * cameraMiddleDragSpeed;
    //    }

    //    if (dy == 0f)
    //        return;

    //    Vector3 pos = cachedCameraTransform.position;
    //    float newY = Mathf.Clamp(pos.y + dy, cameraMinY, cameraMaxY);
    //    cachedCameraTransform.position = new Vector3(cameraBasePosition.x, newY, cameraBasePosition.z);
    //}

    //private void EnsureBackground2Visible()
    //{
    //    if (background2 == null )
    //    {
    //        float now = Time.unscaledTime;
    //        if (now >= nextBackground2ResolveTime)
    //        {
    //            background2 = FindGameObjectIncludingInactive(background2Name, SceneManager.GetActiveScene().GetRootGameObjects());
    //            nextBackground2ResolveTime = now + 0.5f;
    //        }
    //    }

    //    if (background2 != null && !background2.activeSelf)
    //        background2.SetActive(true);
    //}

    //private void TryRevealObjectOnClick()
    //{
    //    //if (!revealObjectOnClickAfterSolved || clickRevealDone)
    //    //    return;

    //    bool clicked = false;
    //    if (Input.touchCount > 0)
    //    {
    //        Touch t = Input.GetTouch(0);
    //        if (t.phase == TouchPhase.Began)
    //            clicked = true;
    //    }
    //    else if (Input.GetMouseButtonDown(0))
    //    {
    //        clicked = true;
    //    }

    //    if (!clicked)
    //        return;

    //    //if (clickRevealObject == null && !string.IsNullOrEmpty(clickRevealObjectName))
    //    //    clickRevealObject = FindGameObjectIncludingInactive(clickRevealObjectName, SceneManager.GetActiveScene().GetRootGameObjects());

    //    //if (clickRevealObject != null && !clickRevealObject.activeSelf)
    //    //    clickRevealObject.SetActive(true);

    //    clickRevealDone = true;
    //}

    //private void ResolveSequenceReferencesIfNeeded()
    //{
    //    if (!enableCompletionSequence)
    //        return;

        //GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();

        //if (puzzleRootToMove == null)
        //{
        //    Transform parent = transform.parent;
        //    if (parent != null && parent.name == "puzzle")
        //    {
        //        puzzleRootToMove = parent;
        //    }
        //    else
        //    {
        //        GameObject puzzle = FindGameObjectIncludingInactive("puzzle", roots);
        //        puzzleRootToMove = puzzle != null ? puzzle.transform : transform;
        //    }
        //}

        //if (puzzleMoveTarget == null && !string.IsNullOrEmpty(puzzleMoveTargetName))
        //{
        //    GameObject t = GameObject.Find(puzzleMoveTargetName);
        //    if (t != null)
        //        puzzleMoveTarget = t.transform;
        //    else
        //    {
        //        GameObject inactive = FindGameObjectIncludingInactive(puzzleMoveTargetName, roots);
        //        if (inactive != null)
        //            puzzleMoveTarget = inactive.transform;
        //    }
        //}

        //if (puzzleAlignAnchor == null && !string.IsNullOrEmpty(puzzleAlignAnchorName))
        //{
        //    if (puzzleRootToMove != null)
        //        puzzleAlignAnchor = FindTransformIncludingInactive(puzzleRootToMove, puzzleAlignAnchorName);

        //    if (puzzleAlignAnchor == null)
        //    {
        //        GameObject go = FindGameObjectIncludingInactive(puzzleAlignAnchorName, roots);
        //        if (go != null)
        //            puzzleAlignAnchor = go.transform;
        //    }
        //}

        //if (background2 == null && !string.IsNullOrEmpty(background2Name))
        //    background2 = FindGameObjectIncludingInactive(background2Name, roots);

        //if (hideBackground2OnStart && background2 != null && background2.activeSelf)
        //    background2.SetActive(false);

        //if (clickRevealObject == null && !string.IsNullOrEmpty(clickRevealObjectName))
        //    clickRevealObject = FindGameObjectIncludingInactive(clickRevealObjectName, roots);

        //if (cachedCameraTransform == null)
        //{
        //    cachedCameraTransform = cameraTransformOverride != null ? cameraTransformOverride : (mainCamera != null ? mainCamera.transform : null);
        //    if (cachedCameraTransform != null)
        //        cameraBasePosition = cachedCameraTransform.position;
        //}

        //if (cameraClampTop == null && !string.IsNullOrEmpty(cameraClampTopName))
        //{
        //    GameObject go = GameObject.Find(cameraClampTopName);
        //    if (go != null)
        //        cameraClampTop = go.transform;
        //    else
        //    {
        //        GameObject inactive = FindGameObjectIncludingInactive(cameraClampTopName, roots);
        //        if (inactive != null)
        //            cameraClampTop = inactive.transform;
        //    }
        //}

        //if (cameraClampBottom == null && !string.IsNullOrEmpty(cameraClampBottomName))
        //{
        //    GameObject go = GameObject.Find(cameraClampBottomName);
        //    if (go != null)
        //        cameraClampBottom = go.transform;
        //    else
        //    {
        //        GameObject inactive = FindGameObjectIncludingInactive(cameraClampBottomName, roots);
        //        if (inactive != null)
        //            cameraClampBottom = inactive.transform;
        //    }
        //}

        //if (cameraClampTop != null && cameraClampBottom != null)
        //{
        //    float yA = cameraClampTop.position.y;
        //    float yB = cameraClampBottom.position.y;
        //    cameraMinY = Mathf.Min(yA, yB);
        //    cameraMaxY = Mathf.Max(yA, yB);
        //}
        //else if (cachedCameraTransform != null)
        //{
        //    cameraMinY = cachedCameraTransform.position.y;
        //    cameraMaxY = cachedCameraTransform.position.y;
        //}

        //if (mainCamera != null && mainCamera.orthographic)
        //    cameraWorldPerPixelY = (mainCamera.orthographicSize * 2f) / Mathf.Max(1, Screen.height);
        //else
        //    cameraWorldPerPixelY = 0.01f;
    //}

    //private static Transform FindTransformIncludingInactive(Transform root, string name)
    //{
    //    if (root == null || string.IsNullOrEmpty(name))
    //        return null;

    //    if (root.name == name)
    //        return root;

    //    sceneTraversalStack.Clear();
    //    sceneTraversalStack.Add(root);

    //    while (sceneTraversalStack.Count > 0)
    //    {
    //        int last = sceneTraversalStack.Count - 1;
    //        Transform t = sceneTraversalStack[last];
    //        sceneTraversalStack.RemoveAt(last);
    //        if (t == null)
    //            continue;

    //        if (t.name == name)
    //            return t;

    //        int childCount = t.childCount;
    //        for (int i = 0; i < childCount; i++)
    //            sceneTraversalStack.Add(t.GetChild(i));
    //    }

    //    return null;
    //}

    //private static GameObject FindGameObjectIncludingInactive(string name, GameObject[] roots)
    //{
    //    if (string.IsNullOrEmpty(name) || roots == null)
    //        return null;

    //    sceneTraversalStack.Clear();

    //    for (int i = 0; i < roots.Length; i++)
    //    {
    //        GameObject root = roots[i];
    //        if (root == null)
    //            continue;
    //        if (root.name == name)
    //            return root;
    //        sceneTraversalStack.Add(root.transform);
    //    }

    //    while (sceneTraversalStack.Count > 0)
    //    {
    //        int last = sceneTraversalStack.Count - 1;
    //        Transform t = sceneTraversalStack[last];
    //        sceneTraversalStack.RemoveAt(last);
    //        if (t == null)
    //            continue;
    //        if (t.name == name)
    //            return t.gameObject;

    //        int childCount = t.childCount;
    //        for (int i = 0; i < childCount; i++)
    //            sceneTraversalStack.Add(t.GetChild(i));
    //    }

    //    return null;
    //}

    private int IndexOfCollider(Collider2D collider)
    {
        if (collider == null)
            return -1;

        for (int i = 0; i < pieceColliders.Length; i++)
        {
            Collider2D c = pieceColliders[i];
            if (c == collider)
                return i;
            Transform p = pieceTransforms[i];
            if (p != null && collider.transform.IsChildOf(p))
                return i;
        }
        return -1;
    }

    private Collider2D EnsureClickableCollider(Transform t)
    {
        Collider2D col = t.GetComponentInChildren<Collider2D>(true);
        if (col != null)
            return col;

        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        BoxCollider2D box = t.gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (sr != null && sr.sprite != null)
        {
            Bounds b = sr.sprite.bounds;
            box.offset = b.center;
            box.size = b.size;
        }
        return box;
    }

    private static Collider2D[] GetAllColliders(Transform piece)
    {
        return piece != null ? piece.GetComponentsInChildren<Collider2D>(true) : null;
    }

    private static void EnsureCollidersEnabled(Collider2D[] cols, bool makeTriggers)
    {
        if (cols == null)
            return;
        for (int i = 0; i < cols.Length; i++)
        {
            Collider2D c = cols[i];
            if (c == null)
                continue;
            if (!c.enabled)
                c.enabled = true;
            if (makeTriggers)
                c.isTrigger = true;
        }
    }

    private void UnparentPiecesFromEachOther()
    {
        if (pieceTransforms == null || pieceTransforms.Length == 0)
            return;

        Transform root = transform;

        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            Transform pi = pieceTransforms[i];
            if (pi == null)
                continue;

            for (int j = 0; j < pieceTransforms.Length; j++)
            {
                if (i == j)
                    continue;
                Transform pj = pieceTransforms[j];
                if (pj == null)
                    continue;

                if (pj.IsChildOf(pi))
                {
                    if (enableDebugLogs)
                        Debug.LogWarning($"S10PuzzleController: {pj.name} is a child of {pi.name}. Reparenting {pj.name} to {root.name} to prevent dragging multiple pieces together.");
                    pj.SetParent(root, true);
                }
            }
        }
    }

    private void IgnorePieceToPieceCollisions()
    {
        if (perPieceColliders == null)
            return;

        for (int i = 0; i < perPieceColliders.Length; i++)
        {
            Collider2D[] a = perPieceColliders[i];
            if (a == null || a.Length == 0)
                continue;

            for (int j = i + 1; j < perPieceColliders.Length; j++)
            {
                Collider2D[] b = perPieceColliders[j];
                if (b == null || b.Length == 0)
                    continue;

                for (int ai = 0; ai < a.Length; ai++)
                {
                    Collider2D ca = a[ai];
                    if (ca == null)
                        continue;

                    for (int bi = 0; bi < b.Length; bi++)
                    {
                        Collider2D cb = b[bi];
                        if (cb == null)
                            continue;

                        Physics2D.IgnoreCollision(ca, cb, true);
                    }
                }
            }
        }
    }

    private bool TryPickBySpriteBounds(Vector3 pointerWorld, out int bestIndex)
    {
        bestIndex = -1;
        int bestSortingOrder = int.MinValue;
        float x = pointerWorld.x;
        float y = pointerWorld.y;

        for (int i = 0; i < pieceSpriteRenderers.Length; i++)
        {
            if (placed[i])
                continue;

            SpriteRenderer sr = pieceSpriteRenderers[i];
            if (sr == null || !sr.enabled)
                continue;

            Bounds b = sr.bounds;
            if (x < b.min.x || x > b.max.x || y < b.min.y || y > b.max.y)
                continue;

            int sortingOrder = sr.sortingOrder;
            if (bestIndex < 0 || sortingOrder > bestSortingOrder)
            {
                bestIndex = i;
                bestSortingOrder = sortingOrder;
            }
        }

        return bestIndex >= 0;
    }

    private Vector3 GetPieceSnapPoint(int idx)
    {
        if (!useSpriteBoundsCenterForSnap)
        {
            Transform p = idx >= 0 && idx < pieceTransforms.Length ? pieceTransforms[idx] : null;
            return p != null ? p.position : default;
        }

        SpriteRenderer sr = idx >= 0 && idx < pieceSpriteRenderers.Length ? pieceSpriteRenderers[idx] : null;
        if (sr != null)
            return sr.bounds.center;

        Transform pt = idx >= 0 && idx < pieceTransforms.Length ? pieceTransforms[idx] : null;
        return pt != null ? pt.position : default;
    }

    private static float SqrDistanceXY(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    private float GetMagnetProximity(int idx, Vector3 snapPointWorld, Vector3 targetWorld)
    {
        if (useScreenSpaceDistances && mainCamera != null)
        {
            Vector2 a = mainCamera.WorldToScreenPoint(snapPointWorld);
            Vector2 b = mainCamera.WorldToScreenPoint(targetWorld);
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float distSqr = dx * dx + dy * dy;
            float r = magnetRadiusPixels;
            float inv = r > 0f ? 1f / (r * r) : 0f;
            return 1f - distSqr * inv;
        }

        float worldDistSqr = SqrDistanceXY(snapPointWorld, targetWorld);
        return 1f - worldDistSqr * magnetInvRadiusSqr[idx];
    }

    private bool IsWithinSnap(int idx, Vector3 snapPointWorld, Vector3 targetWorld, out float dist, out float threshold)
    {
        if (useScreenSpaceDistances && mainCamera != null)
        {
            Vector2 a = mainCamera.WorldToScreenPoint(snapPointWorld);
            Vector2 b = mainCamera.WorldToScreenPoint(targetWorld);
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float distSqr = dx * dx + dy * dy;
            dist = Mathf.Sqrt(distSqr);
            threshold = snapRadiusPixels;
            return dist <= threshold;
        }

        float worldDistSqr = SqrDistanceXY(snapPointWorld, targetWorld);
        dist = Mathf.Sqrt(worldDistSqr);
        threshold = Mathf.Sqrt(snapRadiusSqr[idx]);
        return worldDistSqr <= snapRadiusSqr[idx];
    }

    private bool IsWithinAutoSnap(int idx, Vector3 snapPointWorld, Vector3 targetWorld)
    {
        if (useScreenSpaceDistances && mainCamera != null)
        {
            Vector2 a = mainCamera.WorldToScreenPoint(snapPointWorld);
            Vector2 b = mainCamera.WorldToScreenPoint(targetWorld);
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            float distSqr = dx * dx + dy * dy;
            float r = autoSnapRadiusPixels > 0f ? autoSnapRadiusPixels : snapRadiusPixels;
            return distSqr <= r * r;
        }

        float rW = autoSnapRadiusWorld > 0f ? autoSnapRadiusWorld : Mathf.Sqrt(snapRadiusSqr[idx]);
        float worldDistSqr = SqrDistanceXY(snapPointWorld, targetWorld);
        return worldDistSqr <= rW * rW;
    }

    private void OnDrawGizmosSelected()
    {
        if (useScreenSpaceDistances)
            return;

        if (bindings == null)
            return;

        int count = bindings.Length;
        for (int i = 0; i < count; i++)
        {
            Transform t = bindings[i].target;
            if (t == null)
                continue;

            float r = bindings[i].snapRadius > 0f ? bindings[i].snapRadius : defaultSnapRadius;
            float mr = bindings[i].magnetRadius > 0f ? bindings[i].magnetRadius : r * defaultMagnetRadiusMultiplier;

            Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
            Gizmos.DrawWireSphere(t.position, mr);
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireSphere(t.position, r);
        }
    }

    private static string GetSafeName(string[] arr, int i)
    {
        if (arr == null || i < 0 || i >= arr.Length)
            return "<null>";
        return string.IsNullOrEmpty(arr[i]) ? "<empty>" : arr[i];
    }

    private bool TryGetPointerDown(out Vector3 pointerWorld)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                pointerWorld = ScreenToWorld(t.position);
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            pointerWorld = ScreenToWorld(Input.mousePosition);
            return true;
        }

        pointerWorld = default;
        return false;
    }

    private bool TryGetPointerHeld(out Vector3 pointerWorld)
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pointerWorld = ScreenToWorld(t.position);
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            pointerWorld = ScreenToWorld(Input.mousePosition);
            return true;
        }

        pointerWorld = default;
        return false;
    }

    private bool TryGetPointerUp()
    {
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            return t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
        }

        return Input.GetMouseButtonUp(0);
    }

    private bool IsPointerActive()
    {
        return Input.GetMouseButton(0) || Input.touchCount > 0;
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 p = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(p);
    }
}
