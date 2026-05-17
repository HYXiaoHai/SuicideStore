using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class S9Decode92PuzzleSequence : MonoBehaviour
{
    public float c11BaseRadius = 1.2f;
    public float c12BaseRadius = 1.2f;
    public float sequenceLeniencyMultiplier = 2.0f;
    public float aLockBaseRadius = 1.2f;
    public bool enableCameraPan = true;
    public float cameraPanSpeed = 1.0f;
    public bool enableCameraScroll = true;
    public float cameraScrollSpeed = 4.0f;
    public float cameraClampPadding = 0f;
    public float cSnapHorizontalRange = 1.2f;
    public float cSnapVerticalRange = 2.0f;
    public float bSnapHorizontalRange = 1.2f;
    public float bSnapVerticalRange = 2.0f;
    public float a2StepDelay = 0.8f;
    public float a22SnapHorizontalRange = 1.2f;
    public float a22SnapVerticalRange = 2.0f;
    public float a24SnapHorizontalRange = 1.2f;
    public float a24SnapVerticalRange = 2.0f;
    public float blinkLightOnDuration = 1.0f;
    public float blinkLightOffDuration = 1.0f;
    public float a32SnapHorizontalRange = 1.2f;
    public float a32SnapVerticalRange = 2.0f;
    public float a33SnapHorizontalRange = 1.2f;
    public float a33SnapVerticalRange = 2.0f;
    public float a43SnapHorizontalRange = 1.2f;
    public float a43SnapVerticalRange = 2.0f;
    public float a4StepDelay = 0.8f;
    public float a22PFadeDelayAfterB2 = 0.2f;
    public float a22PFadeDuration = 0.8f;
    public float a12PFadeDelayAfterB2 = 0.2f;
    public float aLockTextDelay = 0.15f;
    public float c2FadeDuration = 0.8f;
    public float c2StepDelay = 0.8f;
    public float c21OkHorizontalRange = 1.2f;
    public float c21OkVerticalRange = 2.0f;
    public float c22MoveTolerance = 0.02f;
    public float a3StepDelay = 0.8f;
    public float b2StepDelay = 0.8f;
    public float b22MoveDuration = 0.6f;
    public float b3c3FadeDuration = 1.2f;
    public float c3StepDelay = 0.8f;
    public float b3StepDelay = 0.8f;
    public float c31MoveDuration = 0.6f;
    public float clickDragThresholdPixels = 6.0f;
    public bool enforceInitialVisibility = true;

    private Camera mainCamera;
    private SpriteRenderer bgRenderer;
    private Bounds cameraClampBounds;
    private bool hasCameraClampBounds;
    private GameObject revealC;
    private GameObject revealC1;
    private GameObject hideCC;
    private Transform pieceC11;
    private Transform pieceC12;
    private Collider2D pieceC11Collider;
    private Collider2D pieceC12Collider;
    private Transform okC11;
    private Transform okC12;
    private Vector3 cDesiredOffsetWorld;
    private Transform c11SnapTarget;
    private Transform c12SnapTarget;
    private bool c11SnappedToMarker;
    private bool c12SnappedToMarker;

    private bool c11Placed;
    private bool c12Placed;

    private GameObject c2;
    private GameObject c3;
    private Transform c21;
    private Collider2D c21Collider;
    private Transform c22;
    private Vector3 c22InitialLocalPos;
    private Vector3 c22InitialWorldPos;
    private Transform c22InitialParent;
    private GameObject c21Ok;
    private Collider2D c21OkCollider;
    private bool c2Shown;
    private bool c3Shown;
    private int c2SequenceStep;
    private float c2NextShowTime;
    private bool c21WasTouchingOk;
    private bool c21EnteredOk;

    private Transform pieceB11;
    private Transform pieceB12;
    private Collider2D pieceB12Collider;
    private Transform b12SnapTarget;
    private bool b12SnappedToMarker;
    private GameObject b2;
    private bool bSolved;
    private GameObject b21;
    private GameObject b22;
    private Transform bb22MoveTarget;
    private int b2SequenceStep;
    private float b2NextShowTime;
    private bool b21Shown;
    private bool b22Shown;
    private bool b2Revealed;
    private bool a2Triggered;
    private bool c2Triggered;
    private bool b22MovePending;
    private bool b22Moving;
    private float b22MoveStartTime;
    private Vector3 b22MoveFrom;
    private Vector3 b22MoveTo;

    private Transform pieceA12P;
    private Collider2D pieceA12PCollider;
    private SpriteRenderer a12PSpriteRenderer;
    private bool a12PRevealed;
    private Transform aLock;
    private GameObject aLockText;
    private bool aUnlocked;

    private GameObject a2;
    private GameObject a21;
    private Transform a22Lock;
    private GameObject a22Text;
    private Transform pieceA22P;
    private Collider2D pieceA22PCollider;
    private SpriteRenderer a22PSpriteRenderer;
    private bool a22PRevealed;
    private Transform pieceA24P;
    private Collider2D pieceA24PCollider;
    private bool a24PRevealed;
    private Transform lockA24;
    private GameObject a24;
    private Transform a24ShiftTarget;
    private GameObject a24Text;
    private bool a24Unlocked;
    private Transform a24TextShiftTarget;
    private bool a24Shifted;
    private GameObject a23;
    private GameObject a25;
    private GameObject a3;
    private GameObject a31;
    private GameObject lockA32;
    private GameObject lockA33;
    private GameObject b3;
    private GameObject c3Extra;
    private bool b3c3Triggered;
    private GameObject a33P;
    private GameObject c31;
    private bool c3FollowTriggered;
    private int c3FollowStep;
    private float c3FollowNextTime;
    private GameObject b31;
    private GameObject a32P;
    private bool b3FollowTriggered;
    private int b3FollowStep;
    private float b3FollowNextTime;
    private Collider2D a32PCollider;
    private Collider2D a33PCollider;
    private GameObject lockA32P;
    private GameObject lockA33P;
    private Transform lockA33PSpawn;
    private bool a33OverrideToSpawn;
    private bool a32Unlocked;
    private bool a33Unlocked;
    private GameObject a34;
    private GameObject b4;
    private GameObject c4;
    private GameObject c41;
    private GameObject a4;
    private GameObject a41;
    private GameObject b41;
    private GameObject a42;
    private Collider2D a42Collider;
    private GameObject a43P;
    private Collider2D a43PCollider;
    private Transform lockA43;
    private GameObject a43P1;
    private GameObject a44;
    private bool a43Unlocked;
    private Transform a43P1SpawnTarget;
    private Transform a44SpawnTarget;
    private Vector3 a43P1InitialPos;
    private Vector3 a44InitialPos;
    private bool hasA43P1InitialPos;
    private bool hasA44InitialPos;
    private bool a41Shown;
    private bool a42Shown;
    private bool lockA43Shown;
    private bool a43PShown;
    private int a4SequenceStep;
    private float a4NextShowTime;
    private GameObject c32;
    private bool a34b4c4Triggered;
    private float a34b4c4StartTime;
    private bool a34FadeStarted;
    private bool b4FadeStarted;
    private bool c4FadeStarted;
    private bool c41FadeStarted;
    private bool a4FadeStarted;
    private bool b41FadeStarted;
    private bool c32FadeStarted;
    private Transform c31MoveTarget;
    private bool c31MovePending;
    private bool c31Moving;
    private float c31MoveStartTime;
    private Vector3 c31MoveFrom;
    private Vector3 c31MoveTo;
    private int a2SequenceStep;
    private float a2NextShowTime;
    private bool a22Unlocked;
    private bool a23Shown;
    private bool a25Shown;
    private bool a3Shown;
    private int a3SequenceStep;
    private float a3NextShowTime;

    private bool cRevealed;

    private enum DragMode
    {
        None = 0,
        Puzzle = 1,
        Camera = 2
    }

    private Transform draggingPiece;
    private Vector3 dragOffset;
    private float draggingZ;
    private DragMode dragMode;
    private Vector3 cameraDragLastMouse;
    private Vector3 cameraDragStartMouse;
    private bool cameraDragMoved;

    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private float nextResolveTime;
    private Dictionary<string, Transform> sceneTransformCache;
    private const string LightChildObjectName = "light";
    private const string BlinkLightChildObjectName = "light (1)";
    private readonly Dictionary<int, Transform> lightChildByParentId = new Dictionary<int, Transform>(64);
    private readonly List<Transform> lightTraversalStack = new List<Transform>(256);
    private readonly List<Transform> shownLightChildren = new List<Transform>(16);
    private readonly HashSet<int> shownLightChildIds = new HashSet<int>(16);
    private readonly List<GameObject> blinkLightObjects = new List<GameObject>(64);
    private readonly Dictionary<int, GameObject> blinkLightByParentId = new Dictionary<int, GameObject>(64);
    private float blinkLightNextToggleTime;
    private bool blinkLightVisible;

    private struct FadeItem
    {
        public SpriteRenderer spriteRenderer;
        public float startTime;
        public float invDuration;
    }

    private readonly List<FadeItem> fadeItems = new List<FadeItem>(16);
    private readonly List<SpriteRenderer> spriteRendererBuffer = new List<SpriteRenderer>(32);

    private void Awake()
    {
        mainCamera = Camera.main;
        bgRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        BuildSceneTransformCache();
        revealC = GameObject.Find("C");
        revealC1 = GameObject.Find("C1");
        Transform ccTransform = FindTransformByName("CC");
        hideCC = ccTransform != null ? ccTransform.gameObject : null;
        pieceC12 = FindTransformByName("C1-2");
        okC12 = FindTransformByName("C1-2-ok");
        pieceB11 = FindTransformByName("B1-1");
        pieceB12 = FindTransformByName("B1-2");
        b12SnapTarget = FindTransformByName("B1-2 (1)");
        b2 = FindGameObjectByName("B2");
        b21 = FindGameObjectByName("B2-1");
        b22 = FindGameObjectByName("B2-2");
        bb22MoveTarget = FindTransformByName("BB2-2 (1)");
        if (bb22MoveTarget == null)
            bb22MoveTarget = FindTransformByName("B2-2 (1)");
        aLock = FindTransformByName("A锁A-1");
        Transform aLockTextTransform = FindTransformByName("A锁1-1文字");
        aLockText = aLockTextTransform != null ? aLockTextTransform.gameObject : null;
        pieceA12P = FindTransformByName("A1-2P");
        a2 = FindGameObjectByName("A2");
        a21 = FindGameObjectByName("A2-1");
        a22Lock = FindTransformByName("A2-2-锁");
        a22Text = FindGameObjectByName("A2-2-文字");
        pieceA22P = FindTransformByName("A2-2-P");
        pieceA24P = FindTransformByName("A2-4-P");
        lockA24 = FindTransformByName("锁A2-4");
        a24 = FindGameObjectByName("A2-4");
        a24ShiftTarget = FindTransformByName("A2-4 (1)");
        if (a24ShiftTarget == null)
            a24ShiftTarget = FindTransformByName("A2-4（1）");
        a24Text = FindGameObjectByName("A2-4-文字");
        a24TextShiftTarget = FindTransformByName("A2-4-文字(1)");
        a23 = FindGameObjectByName("A2-3");
        a25 = FindGameObjectByName("A2-5");
        a3 = FindGameObjectByName("A3");
        a31 = FindGameObjectByName("A3-1");
        lockA32 = FindGameObjectByName("锁A3-2");
        lockA33 = FindGameObjectByName("锁A3-3");
        lockA32P = FindGameObjectByName("锁A3-2-P");
        lockA33P = FindGameObjectByName("锁A3-3-P");
        lockA33PSpawn = FindTransformByName("锁A3-3-P (1)");
        c31MoveTarget = FindTransformByName("C3-1 (1)");
        b3 = FindGameObjectByName("B3");
        c3Extra = FindGameObjectByName("C3");
        a33P = FindGameObjectByName("A3-3-P");
        c31 = FindGameObjectByName("C3-1");
        b31 = FindGameObjectByName("B3-1");
        a32P = FindGameObjectByName("A3-2-P");
        a34 = FindGameObjectByName("A3-4");
        b4 = FindGameObjectByName("B4");
        c4 = FindGameObjectByName("C4");
        c41 = FindGameObjectByName("C4-1");
        a4 = FindGameObjectByName("A4");
        a41 = FindGameObjectByName("A4-1");
        b41 = FindGameObjectByName("B4-1");
        a42 = ResolveA42();
        a43P = FindGameObjectByName("A4-3-P");
        lockA43 = FindTransformByName("锁A4-3");
        a43P1 = FindGameObjectByName("A4-3-P 1");
        a43P1SpawnTarget = FindTransformByName("A4-3-P 1 (1)");
        a44SpawnTarget = FindTransformByName("A4-4 (1)");
        if (a44SpawnTarget == null)
            a44SpawnTarget = FindTransformByName("A4-4（1）");
        a44 = FindGameObjectByName("A4-4");
        if (a43P1 != null && !hasA43P1InitialPos)
        {
            a43P1InitialPos = a43P1.transform.position;
            hasA43P1InitialPos = true;
        }
        if (a44 != null && !hasA44InitialPos)
        {
            a44InitialPos = a44.transform.position;
            hasA44InitialPos = true;
        }
        c32 = FindGameObjectByName("C3-2");
        c2 = FindGameObjectByName("C2");
        c3 = c3Extra;
        c11SnapTarget = FindTransformByName("C1-1 (1)");
        c12SnapTarget = FindTransformByName("C1-2 (1)");
        if (c12SnapTarget == null)
        {
            c12SnapTarget = FindTransformByName("C1-2 (2)");
        }

        if (revealC != null)
        {
            revealC.SetActive(false);
        }
        if (revealC1 != null)
        {
            revealC1.SetActive(true);
        }

        if (pieceC11 != null) pieceC11Collider = EnsureClickableCollider(pieceC11);
        if (pieceC12 != null) pieceC12Collider = EnsureClickableCollider(pieceC12);
        if (pieceB12 != null) pieceB12Collider = EnsureClickableCollider(pieceB12);
        if (pieceA12P != null) pieceA12PCollider = EnsureClickableCollider(pieceA12P);
        if (pieceA22P != null) pieceA22PCollider = EnsureClickableCollider(pieceA22P);
        if (pieceA24P != null) pieceA24PCollider = EnsureClickableCollider(pieceA24P);
        if (a42 != null) a42Collider = EnsureClickableCollider(a42.transform);
        if (a43P != null)
        {
            a43PCollider = EnsureClickableCollider(a43P.transform);
            if (a43PCollider != null && !a43Unlocked)
                a43PCollider.enabled = true;
        }
        if (a32P != null) a32PCollider = EnsureClickableCollider(a32P.transform);
        if (a33P != null) a33PCollider = EnsureClickableCollider(a33P.transform);
        if (pieceA12P != null) a12PSpriteRenderer = pieceA12P.GetComponent<SpriteRenderer>();
        if (pieceA22P != null) a22PSpriteRenderer = pieceA22P.GetComponent<SpriteRenderer>();
        if (a24Text != null) a24Text.SetActive(false);

        if (pieceA12P != null)
        {
            Animator animator = pieceA12P.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = false;
            Animation animation = pieceA12P.GetComponent<Animation>();
            if (animation != null)
                animation.enabled = false;

            pieceA12P.gameObject.SetActive(true);
            if (a12PSpriteRenderer != null)
            {
                Color c = a12PSpriteRenderer.color;
                c.a = 1f;
                a12PSpriteRenderer.color = c;
            }
            a12PRevealed = true;
        }

        if (okC11 != null && okC12 != null)
        {
            cDesiredOffsetWorld = okC12.position - okC11.position;
        }
        else if (pieceC11 != null && pieceC12 != null)
        {
            cDesiredOffsetWorld = pieceC12.position - pieceC11.position;
        }

        if (aLockText != null)
        {
            aLockText.SetActive(false);
        }

        if (a2 != null) a2.SetActive(false);
        if (a21 != null) a21.SetActive(false);
        if (a22Lock != null) a22Lock.gameObject.SetActive(false);
        if (lockA24 != null) lockA24.gameObject.SetActive(false);
        if (a22Text != null) a22Text.SetActive(false);
        if (pieceA22P != null) pieceA22P.gameObject.SetActive(false);
        if (pieceA24P != null) pieceA24P.gameObject.SetActive(false);
        if (a24 != null) a24.SetActive(false);
        if (a23 != null) a23.SetActive(false);
        if (a25 != null) a25.SetActive(false);
        if (a3 != null) a3.SetActive(false);
        if (a31 != null) a31.SetActive(false);
        if (lockA32 != null) lockA32.SetActive(false);
        if (lockA33 != null) lockA33.SetActive(false);
        if (lockA32P != null) lockA32P.SetActive(false);
        if (lockA33P != null) lockA33P.SetActive(false);
        if (b3 != null) b3.SetActive(false);
        if (a33P != null) a33P.SetActive(false);
        if (c31 != null) c31.SetActive(false);
        if (b31 != null) b31.SetActive(false);
        if (a32P != null) a32P.SetActive(false);
        if (a34 != null) a34.SetActive(false);
        if (b4 != null) b4.SetActive(false);
        if (c4 != null) c4.SetActive(false);
        if (c41 != null) c41.SetActive(false);
        if (a4 != null) a4.SetActive(false);
        if (a41 != null) a41.SetActive(false);
        if (b41 != null) b41.SetActive(false);
        if (a42 != null) a42.SetActive(false);
        if (a43P != null) a43P.SetActive(false);
        if (lockA43 != null) lockA43.gameObject.SetActive(false);
        if (a43P1 != null) a43P1.SetActive(false);
        if (a44 != null) a44.SetActive(false);
        if (c32 != null) c32.SetActive(false);
        if (b2 != null) b2.SetActive(false);
        if (b21 != null) b21.SetActive(false);
        if (b22 != null) b22.SetActive(false);
        if (!aUnlocked && pieceA12P != null) pieceA12P.gameObject.SetActive(true);
        a22PRevealed = false;
        a24PRevealed = false;
        a24Unlocked = false;
        a24Shifted = false;
        a25Shown = false;
        a43Unlocked = false;
        a41Shown = false;
        a42Shown = false;
        lockA43Shown = false;
        a43PShown = false;
        a4SequenceStep = 0;
        a4NextShowTime = 0f;
        if (c2 != null) c2.SetActive(false);
        if (c3 != null) c3.SetActive(false);
        c2Shown = false;
        c3Shown = false;
        a3SequenceStep = 0;
        c2SequenceStep = 0;
        c21WasTouchingOk = false;
        c21EnteredOk = false;
        b2SequenceStep = 0;
        b21Shown = false;
        b22Shown = false;
        cRevealed = false;
        b2Revealed = false;
        a2Triggered = false;
        c2Triggered = false;
        b3c3Triggered = false;
        b22MovePending = false;
        b22Moving = false;
        c3FollowTriggered = false;
        c3FollowStep = 0;
        b3FollowTriggered = false;
        b3FollowStep = 0;
        a32Unlocked = false;
        a33Unlocked = false;
        a33OverrideToSpawn = false;
        a34b4c4Triggered = false;
        a34b4c4StartTime = 0f;
        a34FadeStarted = false;
        b4FadeStarted = false;
        c4FadeStarted = false;
        c41FadeStarted = false;
        a4FadeStarted = false;
        b41FadeStarted = false;
        c32FadeStarted = false;
        c31MovePending = false;
        c31Moving = false;

        if (enforceInitialVisibility)
        {
            ApplyInitialVisibility();
        }

        CacheAndHideLightChildren();
        RebuildCameraClampBounds();

        if (mainCamera != null)
        {
            Transform camT = mainCamera.transform;
            camT.position = ClampCameraToBg(camT.position);
        }
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                return;
        }

        if (enableCameraScroll && mainCamera.orthographic)
        {
            float wheel = Input.mouseScrollDelta.y;
            if (wheel != 0f)
            {
                Transform camT = mainCamera.transform;
                Vector3 pos = camT.position;
                pos.y += wheel * cameraScrollSpeed;
                pos = ClampCameraToBg(pos);
                camT.position = pos;
            }
        }

        if (dragMode == DragMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryBeginPrimaryDragOrPan();
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (dragMode == DragMode.Puzzle)
                    DragPuzzle();
                else if (dragMode == DragMode.Camera)
                    DragCamera();
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (Input.touchCount == 0)
                {
                    HideShownLightChildren();
                }

                if (dragMode == DragMode.Puzzle)
                {
                    EndPuzzleDrag();
                }
                else if (dragMode == DragMode.Camera)
                {
                    if (!cameraDragMoved)
                        HandleProgressClick();
                }
                dragMode = DragMode.None;
                draggingPiece = null;
            }
        }

        UpdateA2Sequence();
        UpdateA3Sequence();
        UpdateFades();
        UpdateC2Sequence();
        UpdateB2Sequence();
        UpdateB22Move();
        UpdateC3Follow();
        UpdateB3Follow();
        UpdateC31Move();
        UpdateBlinkLightCycle();
        ApplyA24ShiftIfNeeded();
        UpdateA4Sequence();

        if (Time.unscaledTime >= nextResolveTime)
        {
            nextResolveTime = Time.unscaledTime + 0.5f;
            TryResolveLateObjects();
        }
    }

    private void TryBeginPrimaryDragOrPan()
    {
        Vector3 mouseWorld = GetMouseWorldPosition();
        int count = Physics2D.OverlapPointNonAlloc(mouseWorld, overlapResults);

        if (Input.touchCount == 0)
        {
            HideShownLightChildren();
            ShowLightForOverlapHits(count);
        }

        Collider2D hit = null;
        for (int i = 0; i < count; i++)
        {
            Collider2D c = overlapResults[i];
            if (c == null)
                continue;

            if (c == pieceC11Collider && !c11Placed)
            {
                hit = c;
                break;
            }

            if (c == pieceC12Collider && !c12Placed)
            {
                hit = c;
                break;
            }

            if (c == pieceA12PCollider && !aUnlocked)
            {
                hit = c;
                break;
            }

            if (c == pieceA22PCollider && a2SequenceStep >= 3 && !a22Unlocked)
            {
                hit = c;
                break;
            }

            if (c == pieceA24PCollider && a2SequenceStep >= 3 && !a24Unlocked)
            {
                hit = c;
                break;
            }

            if (c == a32PCollider && !a32Unlocked)
            {
                hit = c;
                break;
            }

            if (c == a33PCollider && !a33Unlocked)
            {
                hit = c;
                break;
            }

            if (!a43Unlocked)
            {
                if (a43P == null)
                    a43P = FindGameObjectByName("A4-3-P");
                if (a43P != null)
                {
                    Transform root = a43P.transform;
                    if (c.transform == root || c.transform.IsChildOf(root))
                    {
                        draggingPiece = root;
                        draggingZ = draggingPiece.position.z;
                        dragOffset = draggingPiece.position - mouseWorld;
                        dragMode = DragMode.Puzzle;
                        return;
                    }
                }
            }

            if (c == pieceB12Collider)
            {
                hit = c;
                break;
            }

        }

        if (hit != null)
        {
            draggingPiece = hit.transform;
            draggingZ = draggingPiece.position.z;
            dragOffset = draggingPiece.position - mouseWorld;
            dragMode = DragMode.Puzzle;
            return;
        }

        if (enableCameraPan && mainCamera.orthographic)
        {
            cameraDragStartMouse = Input.mousePosition;
            cameraDragLastMouse = cameraDragStartMouse;
            cameraDragMoved = false;
            dragMode = DragMode.Camera;
        }
    }

    private void DragPuzzle()
    {
        if (draggingPiece == null)
        {
            dragMode = DragMode.None;
            return;
        }

        Vector3 mouseWorld = GetMouseWorldPosition();
        Vector3 pos = mouseWorld + dragOffset;
        pos.z = draggingZ;
        draggingPiece.position = pos;
    }

    private void DragCamera()
    {
        Vector3 current = Input.mousePosition;
        if (current == cameraDragLastMouse)
            return;

        if (!cameraDragMoved)
        {
            Vector3 d = current - cameraDragStartMouse;
            float thr = clickDragThresholdPixels;
            if (d.x * d.x + d.y * d.y > thr * thr)
                cameraDragMoved = true;
        }

        float zDist = -mainCamera.transform.position.z;
        Vector3 prevW = mainCamera.ScreenToWorldPoint(new Vector3(cameraDragLastMouse.x, cameraDragLastMouse.y, zDist));
        Vector3 curW = mainCamera.ScreenToWorldPoint(new Vector3(current.x, current.y, zDist));
        Vector3 delta = prevW - curW;
        delta.z = 0f;

        Transform camT = mainCamera.transform;
        Vector3 pos = camT.position;
        pos += delta * cameraPanSpeed;
        pos = ClampCameraToBg(pos);
        camT.position = pos;

        cameraDragLastMouse = current;
    }

    private void EndPuzzleDrag()
    {
        Transform released = draggingPiece;

        if (released == null)
            return;

        if (!b12SnappedToMarker && released == pieceB12 && b12SnapTarget != null)
        {
            if (IsWithinBox(pieceB12.position, b12SnapTarget.position, bSnapHorizontalRange, bSnapVerticalRange))
            {
                SnapAndLock(pieceB12, pieceB12Collider, b12SnapTarget.position);
                b12SnappedToMarker = true;
                DisableBlinkLightForParent(pieceB12);
                OnBSolved();
                return;
            }
        }

        if (!c11Placed && released == pieceC11)
        {
            if (IsWithinRadius(pieceC11.position, okC11, c11BaseRadius * sequenceLeniencyMultiplier))
            {
                SnapAndLock(pieceC11, pieceC11Collider, okC11.position);
                c11Placed = true;
                return;
            }
        }

        if (c11Placed && !c12Placed && released == pieceC12)
        {
            float r = c12BaseRadius * sequenceLeniencyMultiplier;
            if (IsWithinRadius(pieceC12.position, okC12, r) && IsOffsetMatched(pieceC12.position - pieceC11.position, cDesiredOffsetWorld, r))
            {
                SnapAndLock(pieceC12, pieceC12Collider, okC12.position);
                c12Placed = true;
                RevealC();
                if (revealC1 != null)
                {
                    revealC1.SetActive(true);
                }
                return;
            }
        }

        if (!aUnlocked && released == pieceA12P)
        {
            if (IsWithinRadius(pieceA12P.position, aLock, aLockBaseRadius * sequenceLeniencyMultiplier))
            {
                aUnlocked = true;
                if (aLock != null)
                    HideAndCancelFade(aLock.gameObject);

                HideAndCancelFade(released.gameObject);
                if (pieceA12P != null && pieceA12P.gameObject != released.gameObject)
                    HideAndCancelFade(pieceA12P.gameObject);

                FadeInGameObject(aLockText, aLockTextDelay, a22PFadeDuration);
                StartA2Sequence(aLockTextDelay);
                return;
            }
        }

        if (!a22Unlocked && released == pieceA22P && a22Lock != null)
        {
            if (IsWithinBox(pieceA22P.position, a22Lock.position, a22SnapHorizontalRange, a22SnapVerticalRange))
            {
                a22Unlocked = true;
                if (a22Lock != null) a22Lock.gameObject.SetActive(false);
                if (pieceA22P != null) pieceA22P.gameObject.SetActive(false);
                FadeInGameObject(a22Text, 0f, a22PFadeDuration);
                TryShowA25IfReady();
                ApplyA24ShiftIfNeeded();
                TriggerB22Move();
                return;
            }
        }

        if (!a24Unlocked && released == pieceA24P && lockA24 != null)
        {
            if (IsWithinBox(pieceA24P.position, lockA24.position, a24SnapHorizontalRange, a24SnapVerticalRange))
            {
                a24Unlocked = true;
                Vector3 snapPos = lockA24.position;
                snapPos.z = pieceA24P.position.z;
                pieceA24P.position = snapPos;
                if (pieceA24P != null)
                {
                    Transform localText = FindDescendantByName(pieceA24P, "A2-4-文字");
                    if (localText != null)
                        a24Text = localText.gameObject;
                }
                if (a24Text == null)
                    a24Text = FindGameObjectByName("A2-4-文字");
                if (a24Text != null)
                {
                    Transform textT = a24Text.transform;
                    if (lockA24 != null && textT.IsChildOf(lockA24))
                        textT.SetParent(lockA24.parent, true);
                    if (pieceA24P != null && textT.IsChildOf(pieceA24P))
                        textT.SetParent(pieceA24P.parent, true);
                    if (!a24Text.activeSelf)
                        a24Text.SetActive(true);
                }
                HideAndCancelFade(pieceA24P.gameObject);
                HideAndCancelFade(lockA24.gameObject);
                if (a24TextShiftTarget == null)
                {
                    if (pieceA24P != null)
                        a24TextShiftTarget = FindDescendantByName(pieceA24P, "A2-4-文字(1)");
                    if (a24TextShiftTarget == null)
                        a24TextShiftTarget = FindTransformByName("A2-4-文字(1)");
                }
                if (a22Unlocked && a24Text != null && a24TextShiftTarget != null)
                {
                    Transform t = a24Text.transform;
                    Vector3 p = a24TextShiftTarget.position;
                    p.z = t.position.z;
                    t.position = p;
                }
                FadeInGameObject(a24Text, 0f, a22PFadeDuration);
                if (a24 == null)
                    a24 = FindGameObjectByName("A2-4");
                FadeInGameObject(a24, 0f, a22PFadeDuration);
                TryShowA25IfReady();
                return;
            }
        }

        if (!a32Unlocked && a32P != null && released == a32P.transform && lockA32 != null)
        {
            if (IsWithinBox(a32P.transform.position, lockA32.transform.position, a32SnapHorizontalRange, a32SnapVerticalRange))
            {
                a32Unlocked = true;
                HideAndCancelFade(a32P);
                HideAndCancelFade(lockA32);
                if (lockA32P == null)
                    lockA32P = FindGameObjectByName("锁A3-2-P");
                FadeInGameObject(lockA32P, 0f, b3c3FadeDuration);
                if (!a33Unlocked)
                    a33OverrideToSpawn = true;
                ApplyA33SpawnPositionForce();
                TryTriggerA34B4C4AndA4();
                return;
            }
        }

        if (!a33Unlocked && a33P != null && released == a33P.transform && lockA33 != null)
        {
            if (IsWithinBox(a33P.transform.position, lockA33.transform.position, a33SnapHorizontalRange, a33SnapVerticalRange))
            {
                a33Unlocked = true;
                HideAndCancelFade(a33P);
                HideAndCancelFade(lockA33);
                if (lockA33P == null)
                    lockA33P = FindGameObjectByName("锁A3-3-P");
                ApplyA33OverridePosition();
                FadeInGameObject(lockA33P, 0f, b3c3FadeDuration);
                TriggerC31Move();
                TryTriggerA34B4C4AndA4();
                return;
            }
        }

        if (!a43Unlocked && released != null && lockA43 != null)
        {
            if (a43P == null)
                a43P = FindGameObjectByName("A4-3-P");
            if (released == a43P?.transform && IsWithinBox(released.position, lockA43.position, a43SnapHorizontalRange, a43SnapVerticalRange))
            {
                a43Unlocked = true;
                Vector3 snapPos = lockA43.position;
                snapPos.z = released.position.z;
                released.position = snapPos;
                HideAndCancelFade(released.gameObject);
                HideAndCancelFade(lockA43.gameObject);
                if (a43P1 == null)
                    a43P1 = FindGameObjectByName("A4-3-P 1");
                if (a43P1SpawnTarget == null)
                    a43P1SpawnTarget = FindTransformByName("A4-3-P 1 (1)");
                if (a43P1 != null)
                {
                    if (!hasA43P1InitialPos)
                    {
                        a43P1InitialPos = a43P1.transform.position;
                        hasA43P1InitialPos = true;
                    }
                    Vector3 p = a43P1SpawnTarget != null ? a43P1SpawnTarget.position : (hasA43P1InitialPos ? a43P1InitialPos : lockA43.position);
                    p.z = a43P1.transform.position.z;
                    a43P1.transform.position = p;
                }
                FadeInGameObject(a43P1, 0f, b3c3FadeDuration);
                if (a44 == null)
                    a44 = FindGameObjectByName("A4-4");
                if (a44SpawnTarget == null)
                {
                    a44SpawnTarget = FindTransformByName("A4-4 (1)");
                    if (a44SpawnTarget == null)
                        a44SpawnTarget = FindTransformByName("A4-4（1）");
                }
                if (a44 != null)
                {
                    if (!hasA44InitialPos)
                    {
                        a44InitialPos = a44.transform.position;
                        hasA44InitialPos = true;
                    }
                    Vector3 p = a44SpawnTarget != null ? a44SpawnTarget.position : (hasA44InitialPos ? a44InitialPos : (a43P1 != null ? a43P1.transform.position : lockA43.position));
                    if (a43P1 != null)
                    {
                        Vector3 p43 = a43P1.transform.position;
                        float dx = p.x - p43.x;
                        float dy = p.y - p43.y;
                        if (dx * dx + dy * dy < 0.000001f && hasA44InitialPos)
                        {
                            p = a44InitialPos;
                        }
                    }
                    p.z = a44.transform.position.z;
                    a44.transform.position = p;
                }
                FadeInGameObject(a44, 0f, b3c3FadeDuration);

                //跳转场景
                CompleteLevel();

                return;
            }
        }

        if (!c11SnappedToMarker && released == pieceC11 && c11SnapTarget != null)
        {
            if (IsWithinBox(pieceC11.position, c11SnapTarget.position, cSnapHorizontalRange, cSnapVerticalRange))
            {
                SnapAndLock(pieceC11, pieceC11Collider, c11SnapTarget.position);
                c11SnappedToMarker = true;
            }
        }

        if (!c12SnappedToMarker && released == pieceC12 && c12SnapTarget != null)
        {
            if (IsWithinBox(pieceC12.position, c12SnapTarget.position, cSnapHorizontalRange, cSnapVerticalRange))
            {
                SnapAndLock(pieceC12, pieceC12Collider, c12SnapTarget.position);
                c12SnappedToMarker = true;
                DisableBlinkLightForParent(pieceC12);
                RevealC();
                return;
            }
        }

        if ((c11SnappedToMarker || c11Placed) && (c12SnappedToMarker || c12Placed))
        {
            RevealC();
        }
    }

    private void RevealC()
    {
        if (hideCC != null)
        {
            hideCC.SetActive(false);
        }
        if (revealC != null)
        {
            revealC.SetActive(true);
        }

        if (!cRevealed)
        {
            cRevealed = true;
            StartC2Sequence();
        }
    }

    private void SnapAndLock(Transform piece, Collider2D pieceCollider, Vector3 targetWorldPos)
    {
        Vector3 pos = targetWorldPos;
        pos.z = piece.position.z;
        piece.position = pos;
        if (pieceCollider != null)
        {
            pieceCollider.enabled = false;
        }
    }

    private bool IsWithinRadius(Vector3 pieceWorldPos, Transform target, float radius)
    {
        if (target == null)
            return false;

        float dx = pieceWorldPos.x - target.position.x;
        float dy = pieceWorldPos.y - target.position.y;
        float rr = radius * radius;
        return dx * dx + dy * dy <= rr;
    }

    private bool IsOffsetMatched(Vector3 currentOffset, Vector3 desiredOffset, float radius)
    {
        float dx = currentOffset.x - desiredOffset.x;
        float dy = currentOffset.y - desiredOffset.y;
        float rr = radius * radius;
        return dx * dx + dy * dy <= rr;
    }

    private bool IsWithinBox(Vector3 pieceWorldPos, Vector3 targetWorldPos, float halfWidth, float halfHeight)
    {
        float dx = pieceWorldPos.x - targetWorldPos.x;
        float dy = pieceWorldPos.y - targetWorldPos.y;
        return Mathf.Abs(dx) <= halfWidth && Mathf.Abs(dy) <= halfHeight;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mp);
    }

    private Transform FindTransformByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        if (sceneTransformCache != null && sceneTransformCache.TryGetValue(objectName, out Transform cached) && cached != null)
            return cached;

        Transform t = transform.Find(objectName);
        if (t != null)
        {
            CacheTransform(objectName, t);
            return t;
        }

        GameObject go = GameObject.Find(objectName);
        if (go != null)
        {
            CacheTransform(objectName, go.transform);
            return go.transform;
        }

        return null;
    }

    private Transform FindDescendantByName(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
            return null;

        lightTraversalStack.Clear();
        lightTraversalStack.Add(root);
        while (lightTraversalStack.Count > 0)
        {
            int lastIndex = lightTraversalStack.Count - 1;
            Transform t = lightTraversalStack[lastIndex];
            lightTraversalStack.RemoveAt(lastIndex);

            if (t != null && t.name == objectName)
                return t;

            if (t == null)
                continue;

            int childCount = t.childCount;
            for (int i = 0; i < childCount; i++)
            {
                lightTraversalStack.Add(t.GetChild(i));
            }
        }

        return null;
    }

    private GameObject FindGameObjectByName(string objectName)
    {
        Transform t = FindTransformByName(objectName);
        return t != null ? t.gameObject : null;
    }

    private void TryShowA25IfReady()
    {
        if (a25Shown)
            return;

        if (a25 == null)
            a25 = FindGameObjectByName("A2-5");
        if (a25 == null)
            return;

        if (a22Text == null)
            a22Text = FindGameObjectByName("A2-2-文字");
        if (a24 == null)
            a24 = FindGameObjectByName("A2-4");

        if (a22Text == null || !a22Text.activeInHierarchy)
            return;
        if (a24 == null || !a24.activeInHierarchy)
            return;

        FadeInGameObject(a25, 0f, a22PFadeDuration);
        a25Shown = true;
    }

    private GameObject ResolveA42()
    {
        if (a4 == null)
            a4 = FindGameObjectByName("A4");

        Transform t = a4 != null ? FindDescendantByName(a4.transform, "A4-2") : null;
        if (t == null)
            t = FindTransformByName("A4-2");
        return t != null ? t.gameObject : null;
    }

    private void BuildSceneTransformCache()
    {
        Scene scene = gameObject.scene.IsValid() ? gameObject.scene : SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        int estimated = roots != null ? roots.Length * 16 : 256;
        sceneTransformCache = new Dictionary<string, Transform>(estimated);

        if (roots == null)
            return;

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
                continue;

            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < all.Length; j++)
            {
                Transform t = all[j];
                if (t == null)
                    continue;
                string n = t.name;
                if (string.IsNullOrEmpty(n))
                    continue;
                if (!sceneTransformCache.ContainsKey(n))
                {
                    sceneTransformCache.Add(n, t);
                }
            }
        }
    }

    private void CacheAndHideLightChildren()
    {
        Scene scene = gameObject.scene.IsValid() ? gameObject.scene : SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        if (roots == null || roots.Length == 0)
            return;

        lightChildByParentId.Clear();
        lightTraversalStack.Clear();
        blinkLightObjects.Clear();
        blinkLightByParentId.Clear();

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
                continue;
            lightTraversalStack.Add(root.transform);
        }

        while (lightTraversalStack.Count > 0)
        {
            int lastIndex = lightTraversalStack.Count - 1;
            Transform t = lightTraversalStack[lastIndex];
            lightTraversalStack.RemoveAt(lastIndex);
            if (t == null)
                continue;

            Transform lightChild = null;
            Transform blinkLightChild = null;
            int childCount = t.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = t.GetChild(i);
                if (child == null)
                    continue;

                if (child.name == LightChildObjectName)
                {
                    lightChild = child;
                    continue;
                }

                if (child.name == BlinkLightChildObjectName)
                {
                    blinkLightChild = child;
                    continue;
                }

                lightTraversalStack.Add(child);
            }

            if (lightChild != null)
            {
                lightChild.gameObject.SetActive(false);
                lightChildByParentId[t.GetInstanceID()] = lightChild;
            }

            if (blinkLightChild != null)
            {
                GameObject go = blinkLightChild.gameObject;
                if (go != null)
                {
                    go.SetActive(true);
                    blinkLightObjects.Add(go);
                    blinkLightByParentId[t.GetInstanceID()] = go;
                }
            }
        }

        blinkLightVisible = true;
        blinkLightNextToggleTime = Time.unscaledTime + GetBlinkLightNextInterval();
    }

    private void ShowLightForOverlapHits(int hitCount)
    {
        if (hitCount <= 0 || lightChildByParentId.Count == 0)
            return;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D c = overlapResults[i];
            if (c == null)
                continue;

            Transform t = c.transform;
            for (int depth = 0; depth < 4 && t != null; depth++)
            {
                int id = t.GetInstanceID();
                if (lightChildByParentId.TryGetValue(id, out Transform lightChild) && lightChild != null)
                {
                    GameObject go = lightChild.gameObject;
                    if (go != null && !go.activeSelf)
                        go.SetActive(true);
                    int lightId = lightChild.GetInstanceID();
                    if (shownLightChildIds.Add(lightId))
                        shownLightChildren.Add(lightChild);
                    break;
                }
                t = t.parent;
            }
        }
    }

    private void HideShownLightChildren()
    {
        if (shownLightChildren.Count == 0)
            return;

        for (int i = 0; i < shownLightChildren.Count; i++)
        {
            Transform t = shownLightChildren[i];
            if (t == null)
                continue;
            GameObject go = t.gameObject;
            if (go != null && go.activeSelf)
                go.SetActive(false);
        }

        shownLightChildren.Clear();
        shownLightChildIds.Clear();
    }

    private void UpdateBlinkLightCycle()
    {
        if (blinkLightObjects.Count == 0)
            return;

        float now = Time.unscaledTime;
        if (now < blinkLightNextToggleTime)
            return;

        bool toggled = false;
        while (now >= blinkLightNextToggleTime)
        {
            blinkLightVisible = !blinkLightVisible;
            blinkLightNextToggleTime += GetBlinkLightNextInterval();
            toggled = true;
        }

        if (!toggled)
            return;

        for (int i = blinkLightObjects.Count - 1; i >= 0; i--)
        {
            GameObject go = blinkLightObjects[i];
            if (go == null)
            {
                blinkLightObjects.RemoveAt(i);
                continue;
            }

            if (go.activeSelf != blinkLightVisible)
                go.SetActive(blinkLightVisible);
        }
    }

    private void DisableBlinkLightForParent(Transform parent)
    {
        if (parent == null)
            return;

        int id = parent.GetInstanceID();
        if (!blinkLightByParentId.TryGetValue(id, out GameObject go) || go == null)
            return;

        if (go.activeSelf)
            go.SetActive(false);

        blinkLightByParentId.Remove(id);

        for (int i = blinkLightObjects.Count - 1; i >= 0; i--)
        {
            if (blinkLightObjects[i] == go)
            {
                blinkLightObjects.RemoveAt(i);
                break;
            }
        }
    }

    private float GetBlinkLightNextInterval()
    {
        float interval = blinkLightVisible ? blinkLightOnDuration : blinkLightOffDuration;
        if (interval <= 0f)
            interval = 0.0001f;
        return interval;
    }

    private void CacheTransform(string objectName, Transform t)
    {
        if (string.IsNullOrEmpty(objectName) || t == null)
            return;
        if (sceneTransformCache == null)
            sceneTransformCache = new Dictionary<string, Transform>(128);
        sceneTransformCache[objectName] = t;
    }

    private void StartA2Sequence(float initialDelay)
    {
        if (a2SequenceStep != 0)
            return;

        a2SequenceStep = 1;
        a2NextShowTime = Time.unscaledTime + initialDelay;
        a2Triggered = true;
        TryRevealB2();
    }

    private void OnBSolved()
    {
        if (bSolved)
            return;

        bSolved = true;
        TryRevealB2();
    }

    private void TryRevealB2()
    {
        if (b2Revealed)
            return;
        if (!bSolved || !a2Triggered || !c2Triggered)
            return;

        b2Revealed = true;
        FadeInGameObject(b2, 0f, a22PFadeDuration);
        BeginA12PFadeIfPossible();
        StartB2Sequence();
    }

    private void BeginA12PFadeIfPossible()
    {
        if (a12PRevealed)
            return;
        if (!b2Revealed)
            return;

        if (pieceA12P == null)
        {
            pieceA12P = FindTransformByName("A1-2P");
            if (pieceA12P == null) pieceA12P = FindTransformByName("A1-2P(Clone)");
            if (pieceA12P != null)
            {
                pieceA12PCollider = EnsureClickableCollider(pieceA12P);
                a12PSpriteRenderer = pieceA12P.GetComponent<SpriteRenderer>();
            }
        }

        if (pieceA12P == null)
            return;

        Animator animator = pieceA12P.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;
        Animation animation = pieceA12P.GetComponent<Animation>();
        if (animation != null)
            animation.enabled = false;

        a12PRevealed = true;
        FadeInGameObject(pieceA12P.gameObject, a12PFadeDelayAfterB2, a22PFadeDuration);
    }

    private void BeginA22PFadeIfPossible()
    {
        BeginA22PFade(a22PFadeDelayAfterB2);
    }

    private void BeginA22PFade(float delay)
    {
        if (a22PRevealed)
            return;

        if (pieceA22P == null)
        {
            pieceA22P = FindTransformByName("A2-2-P");
            if (pieceA22P != null)
                pieceA22PCollider = EnsureClickableCollider(pieceA22P);
        }

        if (pieceA22P == null)
            return;

        if (a22PSpriteRenderer == null)
            a22PSpriteRenderer = pieceA22P.GetComponent<SpriteRenderer>();
        a22PRevealed = true;
        FadeInGameObject(pieceA22P.gameObject, delay, a22PFadeDuration);
    }

    private void StartB2Sequence()
    {
        if (b2SequenceStep != 0)
            return;

        b2SequenceStep = 1;
        b2NextShowTime = Time.unscaledTime + b2StepDelay;
    }

    private void UpdateB2Sequence()
    {
        if (!b2Revealed)
            return;
        if (b2SequenceStep == 0)
            return;
        if (Time.unscaledTime < b2NextShowTime)
            return;

        if (b2SequenceStep == 1)
        {
            if (b21 == null)
                b21 = FindGameObjectByName("B2-1");
            if (b21 == null)
                return;
            FadeInGameObject(b21, 0f, a22PFadeDuration);
            b21Shown = true;
            b2SequenceStep = 2;
            b2NextShowTime = Time.unscaledTime + b2StepDelay;
            return;
        }

        if (b2SequenceStep == 2)
        {
            b2SequenceStep = 3;
            b2NextShowTime = Time.unscaledTime + b2StepDelay;
            return;
        }

        if (b2SequenceStep == 3)
        {
            if (b22 == null)
                b22 = FindGameObjectByName("B2-2");
            if (b22 == null)
                return;
            BeginA22PFade(0f);
            FadeInGameObject(b22, 0f, a22PFadeDuration);
            b22Shown = true;
            TryStartB22Move();
            b2SequenceStep = 4;
        }
    }

    private void TriggerB22Move()
    {
        b22MovePending = true;
        TryStartB22Move();
    }

    private void TryStartB22Move()
    {
        if (!b22MovePending || b22Moving)
            return;

        if (b22 == null)
            b22 = FindGameObjectByName("B2-2");
        if (b22 == null || !b22.activeInHierarchy)
            return;

        if (bb22MoveTarget == null)
        {
            bb22MoveTarget = FindTransformByName("BB2-2 (1)");
            if (bb22MoveTarget == null)
                bb22MoveTarget = FindTransformByName("B2-2 (1)");
        }
        if (bb22MoveTarget == null)
            return;

        b22MovePending = false;
        b22Moving = true;
        b22MoveStartTime = Time.unscaledTime;
        b22MoveFrom = b22.transform.position;
        b22MoveTo = bb22MoveTarget.position;
        b22MoveTo.z = b22MoveFrom.z;
    }

    private void UpdateB22Move()
    {
        if (!b22Moving)
        {
            if (b22MovePending)
                TryStartB22Move();
            return;
        }

        if (b22 == null)
        {
            b22Moving = false;
            return;
        }

        float duration = b22MoveDuration > 0f ? b22MoveDuration : 0.0001f;
        float t = (Time.unscaledTime - b22MoveStartTime) / duration;
        if (t >= 1f)
        {
            b22.transform.position = b22MoveTo;
            b22Moving = false;
            return;
        }

        if (t < 0f)
            return;

        t = t * t * (3f - 2f * t);
        b22.transform.position = Vector3.LerpUnclamped(b22MoveFrom, b22MoveTo, t);
    }

    private void UpdateA2Sequence()
    {
        if (a2SequenceStep == 0)
            return;
        if (Time.unscaledTime < a2NextShowTime)
            return;

        if (a2SequenceStep == 1)
        {
            FadeInGameObject(a2, 0f, a22PFadeDuration);
            a2SequenceStep = 2;
            a2NextShowTime = Time.unscaledTime + a2StepDelay;
            return;
        }

        if (a2SequenceStep == 2)
        {
            FadeInGameObject(a21, 0f, a22PFadeDuration);
            a2SequenceStep = 3;
            a2NextShowTime = Time.unscaledTime + a2StepDelay;
            return;
        }

        if (a2SequenceStep == 3)
        {
            if (a22Lock != null) FadeInGameObject(a22Lock.gameObject, 0f, a22PFadeDuration);
            if (!a24Unlocked && lockA24 != null) FadeInGameObject(lockA24.gameObject, 0f, a22PFadeDuration);
            if (bSolved)
                BeginA22PFadeIfPossible();
            a2SequenceStep = 4;
        }
    }

    private void ApplyA24ShiftIfNeeded()
    {
        if (a24Shifted)
            return;
        if (a22Text == null || !a22Text.activeSelf)
            return;
        if (a24 == null)
            a24 = FindGameObjectByName("A2-4");
        if (a24ShiftTarget == null)
        {
            a24ShiftTarget = FindTransformByName("A2-4 (1)");
            if (a24ShiftTarget == null)
                a24ShiftTarget = FindTransformByName("A2-4（1）");
        }
        if (a24ShiftTarget == null)
            return;

        Vector3 targetPos = a24ShiftTarget.position;

        if (a24 != null)
        {
            Vector3 p = targetPos;
            p.z = a24.transform.position.z;
            a24.transform.position = p;
        }

        if (!a24Unlocked && lockA24 != null)
        {
            Vector3 p = targetPos;
            p.z = lockA24.position.z;
            lockA24.position = p;

            if (a24 != null)
            {
                float aTop = GetTopYFromSprites(a24.transform, a24.transform.position.y);
                float lockTop = GetTopYFromSprites(lockA24, lockA24.position.y);
                float dy = aTop - lockTop;
                if (dy != 0f)
                    lockA24.position += new Vector3(0f, dy, 0f);
            }
        }

        a24Shifted = true;
    }

    private void UpdateA4Sequence()
    {
        if (a4SequenceStep == 0)
        {
            if (!a41Shown && !a42Shown && !lockA43Shown)
            {
                if (a4 == null)
                    a4 = FindGameObjectByName("A4");
                if (b41 == null)
                    b41 = FindGameObjectByName("B4-1");
                if (a4 != null && a4.activeInHierarchy && b41 != null && b41.activeInHierarchy)
                {
                    a4SequenceStep = 1;
                    a4NextShowTime = Time.unscaledTime + b3c3FadeDuration;
                }
            }
        }

        if (a4SequenceStep == 0)
            return;
        if (Time.unscaledTime < a4NextShowTime)
            return;

        if (a4SequenceStep == 1)
        {
            if (a41 == null)
                a41 = FindGameObjectByName("A4-1");
            FadeInGameObject(a41, 0f, b3c3FadeDuration);
            a41Shown = true;
            a4SequenceStep = 2;
            a4NextShowTime = Time.unscaledTime + a4StepDelay;
            return;
        }

        if (a4SequenceStep == 2)
        {
            if (a42 == null)
                a42 = ResolveA42();
            FadeInGameObject(a42, 0f, b3c3FadeDuration);
            a42Shown = true;
            if (a42 != null && a42Collider == null)
                a42Collider = EnsureClickableCollider(a42.transform);
            a4SequenceStep = 3;
            a4NextShowTime = Time.unscaledTime + a4StepDelay;
            return;
        }

        if (a4SequenceStep == 3)
        {
            if (lockA43 == null)
                lockA43 = FindTransformByName("锁A4-3");
            if (lockA43 != null)
            {
                FadeInGameObject(lockA43.gameObject, 0f, b3c3FadeDuration);
                lockA43Shown = true;
            }
            if (!a43PShown)
            {
                if (a43P == null)
                    a43P = FindGameObjectByName("A4-3-P");
                if (a43P != null)
                {
                    FadeInGameObject(a43P, 0f, b3c3FadeDuration);
                    a43PShown = true;
                    if (a43PCollider == null)
                        a43PCollider = EnsureClickableCollider(a43P.transform);
                    if (a43PCollider != null && !a43Unlocked)
                        a43PCollider.enabled = true;
                }
            }
            a4SequenceStep = 0;
        }
    }

    private float GetTopYFromSprites(Transform root, float fallbackY)
    {
        if (root == null)
            return fallbackY;

        spriteRendererBuffer.Clear();
        root.GetComponentsInChildren(true, spriteRendererBuffer);
        if (spriteRendererBuffer.Count == 0)
            return fallbackY;

        float top = float.NegativeInfinity;
        for (int i = 0; i < spriteRendererBuffer.Count; i++)
        {
            SpriteRenderer sr = spriteRendererBuffer[i];
            if (sr == null)
                continue;
            float y = sr.bounds.max.y;
            if (y > top)
                top = y;
        }

        return top == float.NegativeInfinity ? fallbackY : top;
    }

    private void HandleProgressClick()
    {
        if (!a22Unlocked)
            return;

        if (!a23Shown)
        {
            FadeInGameObject(a23, 0f, a22PFadeDuration);
            a23Shown = true;
            return;
        }

        if (!a3Shown)
        {
            FadeInGameObject(a3, 0f, a22PFadeDuration);
            if (a24 == null)
                a24 = FindGameObjectByName("A2-4");
            if (a24 != null)
                DisableBlinkLightForParent(a24.transform);
            a3Shown = true;
            StartA3Sequence();
        }
    }

    private void StartA3Sequence()
    {
        if (a3SequenceStep != 0)
            return;

        a3SequenceStep = 1;
        a3NextShowTime = Time.unscaledTime + a3StepDelay;
    }

    private void UpdateA3Sequence()
    {
        if (a3SequenceStep == 0)
            return;
        if (Time.unscaledTime < a3NextShowTime)
            return;

        if (a3SequenceStep == 1)
        {
            FadeInGameObject(a31, 0f, a22PFadeDuration);
            a3SequenceStep = 2;
            a3NextShowTime = Time.unscaledTime + a3StepDelay;
            return;
        }

        if (a3SequenceStep == 2)
        {
            FadeInGameObject(lockA32, 0f, a22PFadeDuration);
            a3SequenceStep = 3;
            a3NextShowTime = Time.unscaledTime + a3StepDelay;
            return;
        }

        if (a3SequenceStep == 3)
        {
            ApplyA33OverridePosition();
            FadeInGameObject(lockA33, 0f, a22PFadeDuration);
            TriggerB3C3();
            a3SequenceStep = 4;
        }
    }

    private void TriggerB3C3()
    {
        if (b3c3Triggered)
            return;
        b3c3Triggered = true;

        if (b3 == null)
            b3 = FindGameObjectByName("B3");
        if (c3Extra == null)
            c3Extra = FindGameObjectByName("C3");

        FadeInGameObject(b3, 0f, b3c3FadeDuration);
        FadeInGameObject(c3Extra, 0f, b3c3FadeDuration);
        c3Shown = true;
        TriggerC3Follow();
        TriggerB3Follow();
    }

    private void TriggerB3Follow()
    {
        if (b3FollowTriggered)
            return;

        b3FollowTriggered = true;
        b3FollowStep = 1;
        b3FollowNextTime = Time.unscaledTime + b3StepDelay;
    }

    private void UpdateB3Follow()
    {
        if (!b3FollowTriggered)
            return;
        if (b3FollowStep == 0)
            return;
        if (Time.unscaledTime < b3FollowNextTime)
            return;

        if (b3FollowStep == 1)
        {
            if (b31 == null)
                b31 = FindGameObjectByName("B3-1");
            FadeInGameObject(b31, 0f, b3c3FadeDuration);
            b3FollowStep = 2;
            b3FollowNextTime = Time.unscaledTime + b3StepDelay;
            return;
        }

        if (b3FollowStep == 2)
        {
            if (a32P == null)
                a32P = FindGameObjectByName("A3-2-P");
            FadeInGameObject(a32P, 0f, b3c3FadeDuration);
            b3FollowStep = 0;
        }
    }

    private void ApplyA33OverridePosition()
    {
        if (!a33OverrideToSpawn || a33Unlocked)
            return;
        if (lockA33PSpawn == null)
            lockA33PSpawn = FindTransformByName("锁A3-3-P (1)");
        if (lockA33PSpawn == null)
            return;

        Vector3 p = lockA33PSpawn.position;

        if (lockA33 != null)
        {
            Vector3 pos = lockA33.transform.position;
            p.z = pos.z;
            lockA33.transform.position = p;
        }
        if (lockA33P != null)
        {
            Vector3 pos = lockA33P.transform.position;
            p.z = pos.z;
            lockA33P.transform.position = p;
        }
    }

    private void ApplyA33SpawnPositionForce()
    {
        if (lockA33PSpawn == null)
            lockA33PSpawn = FindTransformByName("锁A3-3-P (1)");
        if (lockA33PSpawn == null)
            return;

        if (lockA33 == null)
            lockA33 = FindGameObjectByName("锁A3-3");
        if (lockA33P == null)
            lockA33P = FindGameObjectByName("锁A3-3-P");

        Vector3 p = lockA33PSpawn.position;

        if (lockA33 != null)
        {
            Vector3 pos = lockA33.transform.position;
            p.z = pos.z;
            lockA33.transform.position = p;
        }
        if (lockA33P != null)
        {
            Vector3 pos = lockA33P.transform.position;
            p.z = pos.z;
            lockA33P.transform.position = p;
        }
    }

    private void TryTriggerA34B4C4AndA4()
    {
        if (!a32Unlocked || !a33Unlocked)
            return;

        if (!a34b4c4Triggered)
        {
            a34b4c4Triggered = true;
            a34b4c4StartTime = Time.unscaledTime;
        }

        if (!a34FadeStarted)
        {
            if (a34 == null)
                a34 = FindGameObjectByName("A3-4");
            if (a34 != null)
            {
                FadeInGameObject(a34, 0f, b3c3FadeDuration);
                a34FadeStarted = true;
            }
        }

        if (!b4FadeStarted)
        {
            if (b4 == null)
                b4 = FindGameObjectByName("B4");
            if (b4 != null)
            {
                FadeInGameObject(b4, 0f, b3c3FadeDuration);
                b4FadeStarted = true;
            }
        }

        if (!c4FadeStarted)
        {
            if (c4 == null)
                c4 = FindGameObjectByName("C4");
            if (c4 != null)
            {
                FadeInGameObject(c4, 0f, b3c3FadeDuration);
                if (c41 == null)
                {
                    Transform child = c4.transform.Find("C4-1");
                    if (child != null)
                        c41 = child.gameObject;
                    else
                        c41 = FindGameObjectByName("C4-1");
                }
                if (c41 != null && !c41FadeStarted)
                {
                    FadeInGameObject(c41, b3c3FadeDuration, b3c3FadeDuration);
                    c41FadeStarted = true;
                }
                c4FadeStarted = true;
            }
        }

        if (!a4FadeStarted)
        {
            if (a4 == null)
                a4 = FindGameObjectByName("A4");
            if (a4 != null)
            {
                float delay = a34b4c4StartTime + b3c3FadeDuration - Time.unscaledTime;
                if (delay < 0f)
                    delay = 0f;
                FadeInGameObject(a4, delay, b3c3FadeDuration);
                a4FadeStarted = true;
                a4SequenceStep = 1;
                a4NextShowTime = Time.unscaledTime + delay + b3c3FadeDuration;
            }
        }

        if (!b41FadeStarted)
        {
            if (b41 == null)
                b41 = FindGameObjectByName("B4-1");
            if (b41 != null)
            {
                float delay = a34b4c4StartTime + b3c3FadeDuration - Time.unscaledTime;
                if (delay < 0f)
                    delay = 0f;
                FadeInGameObject(b41, delay, b3c3FadeDuration);
                b41FadeStarted = true;
            }
        }

        if (!c32FadeStarted)
        {
            if (c32 == null)
                c32 = FindGameObjectByName("C3-2");
            if (c32 != null)
            {
                float delay = a34b4c4StartTime + b3c3FadeDuration - Time.unscaledTime;
                if (delay < 0f)
                    delay = 0f;
                FadeInGameObject(c32, delay, b3c3FadeDuration);
                c32FadeStarted = true;
            }
        }
    }

    private void TriggerC31Move()
    {
        c31MovePending = true;
        TryStartC31Move();
    }

    private void TryStartC31Move()
    {
        if (!c31MovePending || c31Moving)
            return;

        if (c31 == null)
            c31 = FindGameObjectByName("C3-1");
        if (c31 == null || !c31.activeInHierarchy)
            return;

        if (c31MoveTarget == null)
            c31MoveTarget = FindTransformByName("C3-1 (1)");
        if (c31MoveTarget == null)
            return;

        c31MovePending = false;
        c31Moving = true;
        c31MoveStartTime = Time.unscaledTime;
        c31MoveFrom = c31.transform.position;
        c31MoveTo = c31MoveTarget.position;
        c31MoveTo.z = c31MoveFrom.z;
    }

    private void UpdateC31Move()
    {
        if (!c31Moving)
        {
            if (c31MovePending)
                TryStartC31Move();
            return;
        }

        if (c31 == null)
        {
            c31Moving = false;
            return;
        }

        float duration = c31MoveDuration > 0f ? c31MoveDuration : 0.0001f;
        float t = (Time.unscaledTime - c31MoveStartTime) / duration;
        if (t >= 1f)
        {
            c31.transform.position = c31MoveTo;
            c31Moving = false;
            return;
        }

        if (t < 0f)
            return;

        t = t * t * (3f - 2f * t);
        c31.transform.position = Vector3.LerpUnclamped(c31MoveFrom, c31MoveTo, t);
    }

    private void TriggerC3Follow()
    {
        if (c3FollowTriggered)
            return;

        c3FollowTriggered = true;
        c3FollowStep = 1;
        c3FollowNextTime = Time.unscaledTime + c3StepDelay;
    }

    private void UpdateC3Follow()
    {
        if (!c3FollowTriggered)
            return;
        if (c3FollowStep == 0)
            return;
        if (Time.unscaledTime < c3FollowNextTime)
            return;

        if (c3FollowStep == 1)
        {
            if (a33P == null)
                a33P = FindGameObjectByName("A3-3-P");
            FadeInGameObject(a33P, 0f, b3c3FadeDuration);
            c3FollowStep = 2;
            c3FollowNextTime = Time.unscaledTime + c3StepDelay;
            return;
        }

        if (c3FollowStep == 2)
        {
            if (c31 == null)
                c31 = FindGameObjectByName("C3-1");
            FadeInGameObject(c31, 0f, b3c3FadeDuration);
            c3FollowStep = 0;
        }
    }

    private void FadeInGameObject(GameObject go, float delay, float duration)
    {
        if (go == null)
            return;

        if (!go.activeSelf)
            go.SetActive(true);

        spriteRendererBuffer.Clear();
        go.transform.GetComponentsInChildren(true, spriteRendererBuffer);
        if (spriteRendererBuffer.Count == 0)
            return;

        for (int i = 0; i < spriteRendererBuffer.Count; i++)
        {
            FadeInSpriteRenderer(spriteRendererBuffer[i], delay, duration);
        }
    }

    private void FadeInSpriteRenderer(SpriteRenderer sr, float delay, float duration)
    {
        if (sr == null)
            return;

        for (int i = 0; i < fadeItems.Count; i++)
        {
            if (fadeItems[i].spriteRenderer == sr)
                return;
        }

        if (!sr.enabled)
            sr.enabled = true;

        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        float d = duration > 0f ? duration : 0.0001f;
        FadeItem item;
        item.spriteRenderer = sr;
        item.startTime = Time.unscaledTime + delay;
        item.invDuration = 1f / d;
        fadeItems.Add(item);
    }

    private void HideAndCancelFade(GameObject go)
    {
        if (go == null)
            return;

        spriteRendererBuffer.Clear();
        go.transform.GetComponentsInChildren(true, spriteRendererBuffer);
        if (spriteRendererBuffer.Count > 0)
        {
            for (int i = fadeItems.Count - 1; i >= 0; i--)
            {
                SpriteRenderer sr = fadeItems[i].spriteRenderer;
                if (sr == null)
                {
                    fadeItems.RemoveAt(i);
                    continue;
                }

                for (int j = 0; j < spriteRendererBuffer.Count; j++)
                {
                    if (spriteRendererBuffer[j] == sr)
                    {
                        fadeItems.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        go.SetActive(false);
    }

    private void UpdateFades()
    {
        if (fadeItems.Count == 0)
            return;

        float now = Time.unscaledTime;
        for (int i = fadeItems.Count - 1; i >= 0; i--)
        {
            FadeItem item = fadeItems[i];
            SpriteRenderer sr = item.spriteRenderer;
            if (sr == null)
            {
                fadeItems.RemoveAt(i);
                continue;
            }

            float t = (now - item.startTime) * item.invDuration;
            if (t <= 0f)
                continue;

            if (t >= 1f)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
                fadeItems.RemoveAt(i);
                continue;
            }

            Color cc = sr.color;
            cc.a = t;
            sr.color = cc;
        }
    }

    private void StartC2Sequence()
    {
        if (c2SequenceStep != 0)
            return;
        c2Triggered = true;
        TryRevealB2();

        if (c2 == null)
            c2 = FindGameObjectByName("C2");
        if (c3 == null)
            c3 = FindGameObjectByName("C3");

        if (c2 != null)
            c2.SetActive(false);
        if (c3 != null)
            c3.SetActive(false);

        c21 = FindTransformByName("C2-1");
        c22 = FindTransformByName("C2-2");
        if (c21 != null)
            c21.gameObject.SetActive(false);
        if (c22 != null)
            c22.gameObject.SetActive(false);

        Transform existingOk = FindTransformByName("C2-1-ok");
        if (existingOk != null)
        {
            c21Ok = existingOk.gameObject;
            c21Ok.SetActive(false);
        }

        c2Shown = false;
        c3Shown = false;
        c2SequenceStep = 1;
        c2NextShowTime = Time.unscaledTime;
    }

    private void UpdateC2Sequence()
    {
        if (!cRevealed)
            return;
        if (c2SequenceStep == 0)
            return;
        if (Time.unscaledTime < c2NextShowTime)
            return;

        if (c2SequenceStep == 1)
        {
            if (c2 == null)
                c2 = FindGameObjectByName("C2");
            if (c2 == null)
                return;
            FadeInGameObject(c2, 0f, c2FadeDuration);

            c2Shown = true;

            EnsureC21Ok();

            c2SequenceStep = 2;
            c2NextShowTime = Time.unscaledTime + c2StepDelay;
            return;
        }

        if (c2SequenceStep == 2)
        {
            if (c21 == null)
                c21 = FindTransformByName("C2-1");
            if (c21 == null)
                return;
            if (c21 != null)
            {
                c21Collider = EnsureClickableCollider(c21);
                if (c21OkCollider != null)
                {
                    c21WasTouchingOk = Physics2D.Distance(c21Collider, c21OkCollider).isOverlapped;
                    c21EnteredOk = false;
                }
                FadeInGameObject(c21.gameObject, 0f, c2FadeDuration);
            }

            c2SequenceStep = 3;
            c2NextShowTime = Time.unscaledTime + c2StepDelay;
            return;
        }

        if (c2SequenceStep == 3)
        {
            if (c22 == null)
                c22 = FindTransformByName("C2-2");
            if (c22 == null)
                return;
            if (c22 != null)
            {
                FadeInGameObject(c22.gameObject, 0f, c2FadeDuration);
                c22InitialLocalPos = c22.localPosition;
                c22InitialWorldPos = c22.position;
                c22InitialParent = c22.parent;
            }

            if (!a24PRevealed && !a24Unlocked)
            {
                if (pieceA24P == null)
                {
                    pieceA24P = FindTransformByName("A2-4-P");
                    if (pieceA24P != null)
                        pieceA24PCollider = EnsureClickableCollider(pieceA24P);
                }
                if (pieceA24P != null)
                {
                    a24PRevealed = true;
                    FadeInGameObject(pieceA24P.gameObject, 0f, c2FadeDuration);
                }
            }

            c2SequenceStep = 4;
        }
    }

    private void EnsureC21Ok()
    {
        if (c21Ok == null)
        {
            Transform existingOk = FindTransformByName("C2-1-ok");
            if (existingOk != null)
            {
                c21Ok = existingOk.gameObject;
            }
            else
            {
                c21Ok = new GameObject("C2-1-ok");
                Transform parent = c2 != null ? c2.transform : transform;
                c21Ok.transform.SetParent(parent, false);
                c21Ok.transform.position = c21 != null ? c21.position : parent.position;
            }
        }

        if (!c21Ok.activeSelf)
            c21Ok.SetActive(true);

        BoxCollider2D box = c21Ok.GetComponent<BoxCollider2D>();
        if (box == null)
            box = c21Ok.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(c21OkHorizontalRange * 2f, c21OkVerticalRange * 2f);
        c21OkCollider = box;

        SpriteRenderer okSr = c21Ok.GetComponent<SpriteRenderer>();
        if (okSr != null)
            okSr.enabled = false;
    }

    private void UpdateC3Condition()
    {
        return;
    }

    private void ApplyInitialVisibility()
    {
        SetActiveIfFound("A", true);
        SetActiveIfFound("B", true);
        SetActiveIfFound("CC", true);
        SetActiveIfFound("A1", true);
        SetActiveIfFound("B1", true);
        SetActiveIfFound("C1", true);
        SetActiveIfFound("bg", true);
        SetActiveIfFound("A1-1", true);
        SetActiveIfFound("A1-1 1", true);

        if (revealC != null)
        {
            revealC.SetActive(false);
        }
    }

    private void SetActiveIfFound(string objectName, bool active)
    {
        Transform t = FindTransformByName(objectName);
        if (t != null)
        {
            t.gameObject.SetActive(active);
        }
    }

    private void TryResolveLateObjects()
    {
        if (!hasCameraClampBounds)
            RebuildCameraClampBounds();

        if (revealC1 == null)
        {
            revealC1 = GameObject.Find("C1");
        }
        if (c2 == null)
        {
            c2 = FindGameObjectByName("C2");
            if (c2 != null && !c2Shown)
                c2.SetActive(false);
        }
        if (c3 == null)
        {
            c3 = FindGameObjectByName("C3");
            if (c3 != null && !c3Shown)
                c3.SetActive(false);
        }
        if (pieceB11 == null)
        {
            pieceB11 = FindTransformByName("B1-1");
        }
        if (pieceB12 == null)
        {
            pieceB12 = FindTransformByName("B1-2");
            if (pieceB12 != null) pieceB12Collider = EnsureClickableCollider(pieceB12);
        }
        if (b12SnapTarget == null)
        {
            b12SnapTarget = FindTransformByName("B1-2 (1)");
        }
        if (b2 == null)
        {
            b2 = FindGameObjectByName("B2");
            if (b2 != null && !b2Revealed)
                b2.SetActive(false);
        }
        if (b21 == null)
        {
            b21 = FindGameObjectByName("B2-1");
            if (b21 != null && !b21Shown)
                b21.SetActive(false);
        }
        if (b22 == null)
        {
            b22 = FindGameObjectByName("B2-2");
            if (b22 != null && !b22Shown)
                b22.SetActive(false);
        }
        if (bb22MoveTarget == null)
        {
            bb22MoveTarget = FindTransformByName("BB2-2 (1)");
            if (bb22MoveTarget == null)
                bb22MoveTarget = FindTransformByName("B2-2 (1)");
        }

        if (aLock == null)
        {
            aLock = FindTransformByName("A锁A-1");
        }
        if (aLockText == null)
        {
            Transform t = FindTransformByName("A锁1-1文字");
            aLockText = t != null ? t.gameObject : null;
            if (aLockText != null && !aUnlocked)
            {
                aLockText.SetActive(false);
            }
        }

        if (pieceA12P == null)
        {
            pieceA12P = FindTransformByName("A1-2P");
            if (pieceA12P == null) pieceA12P = FindTransformByName("A1-2P(Clone)");
            if (pieceA12P != null)
            {
                pieceA12PCollider = EnsureClickableCollider(pieceA12P);
                a12PSpriteRenderer = pieceA12P.GetComponent<SpriteRenderer>();
                Animator animator = pieceA12P.GetComponent<Animator>();
                if (animator != null)
                    animator.enabled = false;
                Animation animation = pieceA12P.GetComponent<Animation>();
                if (animation != null)
                    animation.enabled = false;

                if (!aUnlocked)
                {
                    pieceA12P.gameObject.SetActive(true);
                    if (a12PSpriteRenderer != null)
                    {
                        Color c = a12PSpriteRenderer.color;
                        c.a = 1f;
                        a12PSpriteRenderer.color = c;
                    }
                    a12PRevealed = true;
                }
            }
        }
        else if (pieceA12PCollider == null)
        {
            pieceA12PCollider = EnsureClickableCollider(pieceA12P);
        }

        if (pieceA22P == null)
        {
            pieceA22P = FindTransformByName("A2-2-P");
            if (pieceA22P != null)
            {
                pieceA22PCollider = EnsureClickableCollider(pieceA22P);
                a22PSpriteRenderer = pieceA22P.GetComponent<SpriteRenderer>();
                if (!bSolved || !a22PRevealed)
                    pieceA22P.gameObject.SetActive(false);
            }
        }
        else if (pieceA22P != null && !a22PRevealed)
        {
            pieceA22P.gameObject.SetActive(false);
        }

        if (pieceA24P == null)
        {
            pieceA24P = FindTransformByName("A2-4-P");
            if (pieceA24P != null)
                pieceA24PCollider = EnsureClickableCollider(pieceA24P);
        }
        else if (pieceA24PCollider == null)
        {
            pieceA24PCollider = EnsureClickableCollider(pieceA24P);
        }
        if (pieceA24P != null && !a24PRevealed)
        {
            if (pieceA24P.gameObject.activeSelf)
                pieceA24P.gameObject.SetActive(false);
        }

        if (a24 == null)
        {
            a24 = FindGameObjectByName("A2-4");
            if (a24 != null && !a24Unlocked)
                a24.SetActive(false);
        }
        else if (!a24Unlocked && a24.activeSelf)
        {
            a24.SetActive(false);
        }

        if (a25 == null)
        {
            a25 = FindGameObjectByName("A2-5");
            if (a25 != null && !a25Shown)
                a25.SetActive(false);
        }
        else if (!a25Shown && a25.activeSelf)
        {
            a25.SetActive(false);
        }
        TryShowA25IfReady();

        if (a24ShiftTarget == null)
        {
            a24ShiftTarget = FindTransformByName("A2-4 (1)");
            if (a24ShiftTarget == null)
                a24ShiftTarget = FindTransformByName("A2-4（1）");
        }

        if (lockA24 == null)
        {
            lockA24 = FindTransformByName("锁A2-4");
        }
        if (lockA24 != null && !a24Unlocked)
        {
            if (a2SequenceStep < 3)
            {
                if (lockA24.gameObject.activeSelf)
                    lockA24.gameObject.SetActive(false);
            }
            else if (!lockA24.gameObject.activeSelf)
            {
                FadeInGameObject(lockA24.gameObject, 0f, a22PFadeDuration);
            }
        }

        if (a24TextShiftTarget == null)
        {
            if (pieceA24P != null)
                a24TextShiftTarget = FindDescendantByName(pieceA24P, "A2-4-文字(1)");
            if (a24TextShiftTarget == null)
                a24TextShiftTarget = FindTransformByName("A2-4-文字(1)");
        }

        if (a24Text == null)
        {
            if (pieceA24P != null)
            {
                Transform localText = FindDescendantByName(pieceA24P, "A2-4-文字");
                if (localText != null)
                    a24Text = localText.gameObject;
            }
            if (a24Text == null)
                a24Text = FindGameObjectByName("A2-4-文字");
            if (a24Text != null && !a24Unlocked)
                a24Text.SetActive(false);
        }
        else if (!a24Unlocked && a24Text.activeSelf)
        {
            a24Text.SetActive(false);
        }

        if (a22Unlocked)
        {
            ApplyA24ShiftIfNeeded();
        }

        if (a31 == null)
        {
            a31 = FindGameObjectByName("A3-1");
            if (a31 != null && a3SequenceStep == 0)
                a31.SetActive(false);
        }
        if (lockA32 == null)
        {
            lockA32 = FindGameObjectByName("锁A3-2");
            if (lockA32 != null && a3SequenceStep == 0)
                lockA32.SetActive(false);
        }
        if (lockA33 == null)
        {
            lockA33 = FindGameObjectByName("锁A3-3");
            if (lockA33 != null && a3SequenceStep == 0)
                lockA33.SetActive(false);
        }
        if (lockA32P == null)
        {
            lockA32P = FindGameObjectByName("锁A3-2-P");
            if (lockA32P != null && !a32Unlocked)
                lockA32P.SetActive(false);
        }
        if (lockA33P == null)
        {
            lockA33P = FindGameObjectByName("锁A3-3-P");
            if (lockA33P != null && !a33Unlocked)
                lockA33P.SetActive(false);
        }
        if (lockA33PSpawn == null)
        {
            lockA33PSpawn = FindTransformByName("锁A3-3-P (1)");
        }
        if (c31MoveTarget == null)
        {
            c31MoveTarget = FindTransformByName("C3-1 (1)");
        }
        if (b3 == null)
        {
            b3 = FindGameObjectByName("B3");
            if (b3 != null && !b3c3Triggered)
                b3.SetActive(false);
        }
        if (a33P == null)
        {
            a33P = FindGameObjectByName("A3-3-P");
            if (a33P != null && !c3FollowTriggered)
                a33P.SetActive(false);
        }
        if (c31 == null)
        {
            c31 = FindGameObjectByName("C3-1");
            if (c31 != null && !c3FollowTriggered)
                c31.SetActive(false);
        }
        if (c31MoveTarget == null)
        {
            c31MoveTarget = FindTransformByName("C3-1 (1)");
        }
        if (b31 == null)
        {
            b31 = FindGameObjectByName("B3-1");
            if (b31 != null && !b3FollowTriggered)
                b31.SetActive(false);
        }
        if (a32P == null)
        {
            a32P = FindGameObjectByName("A3-2-P");
            if (a32P != null && !b3FollowTriggered)
                a32P.SetActive(false);
        }
        if (a34 == null)
        {
            a34 = FindGameObjectByName("A3-4");
            if (a34 != null && !a34b4c4Triggered)
                a34.SetActive(false);
        }
        if (b4 == null)
        {
            b4 = FindGameObjectByName("B4");
            if (b4 != null && !a34b4c4Triggered)
                b4.SetActive(false);
        }
        if (c4 == null)
        {
            c4 = FindGameObjectByName("C4");
            if (c4 != null && !a34b4c4Triggered)
                c4.SetActive(false);
        }
        if (!c41FadeStarted && c41 != null && c41.activeSelf)
            c41.SetActive(false);
        if (!c41FadeStarted && c41 == null)
        {
            if (c4 != null)
            {
                Transform child = c4.transform.Find("C4-1");
                if (child != null)
                    c41 = child.gameObject;
            }
            if (c41 == null)
                c41 = FindGameObjectByName("C4-1");
            if (c41 != null && c41.activeSelf)
                c41.SetActive(false);
        }
        if (a4 == null)
        {
            a4 = FindGameObjectByName("A4");
            if (a4 != null && !a34b4c4Triggered)
                a4.SetActive(false);
        }
        if (a41 == null)
        {
            a41 = FindGameObjectByName("A4-1");
            if (a41 != null && !a41Shown)
                a41.SetActive(false);
        }
        else if (!a41Shown && a41.activeSelf)
        {
            a41.SetActive(false);
        }
        if (b41 == null)
        {
            b41 = FindGameObjectByName("B4-1");
            if (b41 != null && !a34b4c4Triggered)
                b41.SetActive(false);
        }
        if (a42 == null)
        {
            a42 = ResolveA42();
            if (a42 != null && !a42Shown)
                a42.SetActive(false);
        }
        else if (!a42Shown && a42.activeSelf)
        {
            a42.SetActive(false);
        }
        if (a42Collider == null && a42 != null)
        {
            a42Collider = EnsureClickableCollider(a42.transform);
        }
        if (a43P == null)
        {
            a43P = FindGameObjectByName("A4-3-P");
        }
        if (!a43PShown && !lockA43Shown && !a43Unlocked && a43P != null && a43P.activeSelf)
        {
            a43P.SetActive(false);
        }
        if (!a43PShown && lockA43Shown && !a43Unlocked && a43P != null)
        {
            FadeInGameObject(a43P, 0f, b3c3FadeDuration);
            a43PShown = true;
        }
        if (a43PCollider == null && a43P != null)
        {
            a43PCollider = EnsureClickableCollider(a43P.transform);
            if (a43PCollider != null && !a43Unlocked)
                a43PCollider.enabled = true;
        }
        if (lockA43 == null)
        {
            lockA43 = FindTransformByName("锁A4-3");
            if (lockA43 != null && (!lockA43Shown || a43Unlocked))
                lockA43.gameObject.SetActive(false);
        }
        else if (!lockA43Shown && !a43Unlocked && lockA43.gameObject.activeSelf)
        {
            lockA43.gameObject.SetActive(false);
        }
        if (a43P1 == null)
        {
            a43P1 = FindGameObjectByName("A4-3-P 1");
            if (a43P1 != null && !a43Unlocked)
                a43P1.SetActive(false);
        }
        if (a43P1SpawnTarget == null)
        {
            a43P1SpawnTarget = FindTransformByName("A4-3-P 1 (1)");
        }
        if (a44 == null)
        {
            a44 = FindGameObjectByName("A4-4");
            if (a44 != null && !a43Unlocked)
                a44.SetActive(false);
        }
        if (a44SpawnTarget == null)
        {
            a44SpawnTarget = FindTransformByName("A4-4 (1)");
            if (a44SpawnTarget == null)
                a44SpawnTarget = FindTransformByName("A4-4（1）");
        }
        if (c32 == null)
        {
            c32 = FindGameObjectByName("C3-2");
            if (c32 != null && !a34b4c4Triggered)
                c32.SetActive(false);
        }
        if (a32PCollider == null && a32P != null)
        {
            a32PCollider = EnsureClickableCollider(a32P.transform);
        }
        if (a33PCollider == null && a33P != null)
        {
            a33PCollider = EnsureClickableCollider(a33P.transform);
        }

        if (a32Unlocked)
        {
            if (a32P != null && a32P.activeSelf)
                a32P.SetActive(false);
            if (lockA32 != null && lockA32.activeSelf)
                lockA32.SetActive(false);
            if (lockA32P != null && !lockA32P.activeSelf)
                lockA32P.SetActive(true);
        }
        if (a33Unlocked)
        {
            if (a33P != null && a33P.activeSelf)
                a33P.SetActive(false);
            if (lockA33 != null && lockA33.activeSelf)
                lockA33.SetActive(false);
            if (lockA33P != null && !lockA33P.activeSelf)
                lockA33P.SetActive(true);
        }
        else
        {
            ApplyA33OverridePosition();
        }

        if (bSolved || a2Triggered || c2Triggered)
            TryRevealB2();

        if (c2Shown)
        {
            if (c21 == null)
            {
                c21 = FindTransformByName("C2-1");
                if (c21 != null)
                    c21Collider = EnsureClickableCollider(c21);
            }
            if (c22 == null)
            {
                c22 = FindTransformByName("C2-2");
                if (c22 != null)
                {
                    c22InitialLocalPos = c22.localPosition;
                    c22InitialWorldPos = c22.position;
                    c22InitialParent = c22.parent;
                }
            }
            if (c21Ok == null)
            {
                Transform existingOk = FindTransformByName("C2-1-ok");
                if (existingOk != null)
                {
                    c21Ok = existingOk.gameObject;
                    BoxCollider2D box = c21Ok.GetComponent<BoxCollider2D>();
                    if (box == null)
                        box = c21Ok.AddComponent<BoxCollider2D>();
                    box.isTrigger = true;
                    box.size = new Vector2(c21OkHorizontalRange * 2f, c21OkVerticalRange * 2f);
                    c21OkCollider = box;
                }
            }
        }

        if (c11SnapTarget == null)
        {
            c11SnapTarget = FindTransformByName("C1-1 (1)");
        }
        if (c12SnapTarget == null)
        {
            c12SnapTarget = FindTransformByName("C1-2 (1)");
            if (c12SnapTarget == null)
            {
                c12SnapTarget = FindTransformByName("C1-2 (2)");
            }
        }

        TryTriggerA34B4C4AndA4();
    }

    private Vector3 ClampCameraToBg(Vector3 cameraWorldPos)
    {
        if (mainCamera == null || !mainCamera.orthographic)
            return cameraWorldPos;

        Bounds b;
        if (hasCameraClampBounds)
        {
            b = cameraClampBounds;
        }
        else if (bgRenderer != null)
        {
            b = bgRenderer.bounds;
        }
        else
        {
            return cameraWorldPos;
        }

        float pad = cameraClampPadding;
        if (pad != 0f)
        {
            b.Expand(new Vector3(pad * 2f, pad * 2f, 0f));
        }

        float v = mainCamera.orthographicSize;
        float h = v * mainCamera.aspect;

        float minX = b.min.x + h;
        float maxX = b.max.x - h;
        float minY = b.min.y + v;
        float maxY = b.max.y - v;

        if (minX > maxX)
        {
            cameraWorldPos.x = b.center.x;
        }
        else
        {
            cameraWorldPos.x = Mathf.Clamp(cameraWorldPos.x, minX, maxX);
        }

        if (minY > maxY)
        {
            cameraWorldPos.y = b.center.y;
        }
        else
        {
            cameraWorldPos.y = Mathf.Clamp(cameraWorldPos.y, minY, maxY);
        }

        return cameraWorldPos;
    }

    private void RebuildCameraClampBounds()
    {
        hasCameraClampBounds = false;

        Scene scene = gameObject.scene.IsValid() ? gameObject.scene : SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        if (roots == null || roots.Length == 0)
            return;

        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null)
                continue;

            string n = root.name;
            if (string.IsNullOrEmpty(n))
                continue;
            n = n.Trim();
            if (n.Length < 2)
                continue;

            char c0 = n[0];
            char c1 = n[1];
            if (c0 != 'b' && c0 != 'B')
                continue;
            if (c1 != 'g' && c1 != 'G')
                continue;

            SpriteRenderer[] srs = root.GetComponentsInChildren<SpriteRenderer>(true);
            if (srs == null || srs.Length == 0)
                continue;

            for (int j = 0; j < srs.Length; j++)
            {
                SpriteRenderer sr = srs[j];
                if (sr == null || sr.sprite == null)
                    continue;
                if (!hasCameraClampBounds)
                {
                    cameraClampBounds = sr.bounds;
                    hasCameraClampBounds = true;
                }
                else
                {
                    cameraClampBounds.Encapsulate(sr.bounds);
                }
            }
        }
    }

    private Collider2D EnsureClickableCollider(Transform t)
    {
        if (t == null)
            return null;

        Collider2D existing = t.GetComponent<Collider2D>();
        if (existing is BoxCollider2D existingBox)
        {
            FitBoxColliderToSprite(existingBox, t);
            return existingBox;
        }

        if (existing != null)
            return existing;

        BoxCollider2D box = t.gameObject.AddComponent<BoxCollider2D>();
        box.isTrigger = false;
        FitBoxColliderToSprite(box, t);
        return box;
    }

    private void FitBoxColliderToSprite(BoxCollider2D box, Transform t)
    {
        if (box == null || t == null)
            return;

        SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            return;

        box.offset = sr.sprite.bounds.center;
        box.size = sr.sprite.bounds.size;
    }
    //跳转场景
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
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (okC11 != null)
        {
            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.5f);
            UnityEditor.Handles.DrawWireDisc(okC11.position, Vector3.forward, c11BaseRadius * sequenceLeniencyMultiplier);
        }
        if (okC12 != null)
        {
            UnityEditor.Handles.color = new Color(0f, 0.7f, 1f, 0.5f);
            UnityEditor.Handles.DrawWireDisc(okC12.position, Vector3.forward, c12BaseRadius * sequenceLeniencyMultiplier);
        }
    }
#endif
}
