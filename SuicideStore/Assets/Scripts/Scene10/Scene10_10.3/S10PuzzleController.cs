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

    [Header("音效")]
    public AudioClip adsorptionClip;//吸附音效
    public AudioClip clickClip;//点击音效

    [Header("Events")]
    public UnityEvent onAllPiecesPlaced;

    [Header("S10-puzzle-10.3 (Completion)")]
    [SerializeField] public bool enableCompletionSequence = true;
    [SerializeField] private Transform puzzleMoveTarget;
    [SerializeField] private float puzzleMoveDuration = 0.6f;
    [SerializeField] private GameObject background2;

    public GameObject puzzleFather;           //拼图的父物体，需要拖拽赋值
    private bool completionAnimating = false; //防止动画重复执行

    [Header("S10-puzzle-10.3 (Camera Vertical Move)")]
    public Transform targetCamera;
    public bool enableCameraVerticalMoveAfterSolved = true;
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
    [SerializeField] private StarMessageGroup[] starMessageGroups = new StarMessageGroup[3];
    [SerializeField] private float starFadeDuration = 0.5f;

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

    private bool completionDone;


    private bool[] starsClicked = new bool[3];
    private int starsClickedCount;
    private bool starsPhaseActive;
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
        HideAllMessagesOnStart();

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

        pieceRigidbodies = new Rigidbody2D[count];
        originalKinematicStates = new bool[count];

        placedCount = 0;

        for (int i = 0; i < count; i++)
        {
            Transform p = bindings[i].piece;
            Transform t = bindings[i].target;

            if (p == null)
            {
                Debug.LogError($"S10PuzzleController: 绑定索引 {i} 的 piece 未拖拽！请检查 Inspector 中的 PieceBinding。");
                continue;
            }
            if (t == null)
            {
                Debug.LogError($"S10PuzzleController: 绑定索引 {i} 的 target 未拖拽！请检查 Inspector 中的 PieceBinding。");
                continue;
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

                pieceRigidbodies[i] = p.GetComponent<Rigidbody2D>();
                originalKinematicStates[i] = pieceRigidbodies[i] != null && pieceRigidbodies[i].isKinematic;

                if (enableDebugLogs)
                {
                    Debug.Log($"S10PuzzleController: Bind[{i}] piece={p.name}, target={t.name}, snapRadius={Mathf.Sqrt(snapRadiusSqr[i]):F3}, magnetRadius={Mathf.Sqrt(magnetRadiusSqr[i]):F3}");
                }
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

        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            if (i == idx || placed[i])
                continue;

            Transform t = pieceTransforms[i];
            if (t == null)
                continue;

            frozenPositions[i] = t.position;
            frozenRotations[i] = t.rotation;

            if (pieceRigidbodies[i] != null)
                pieceRigidbodies[i].isKinematic = true;
        }
        othersFrozen = true;
        if (pieceRigidbodies[idx] != null)
            pieceRigidbodies[idx].isKinematic = true;

        draggingIndex = idx;
        draggingZ = piece.position.z;
        dragOffsetWorld = piece.position - pointerWorld;
        if (clickClip != null)
            AudioManager.Instance.Play2DSound(clickClip, 0.5f);

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
            if (adsorptionClip != null)
                AudioManager.Instance.Play2DSound(adsorptionClip, 1f);
            placedCount++;

            if (placedCount == placed.Length)
                onAllPiecesPlaced?.Invoke();
        }
    }

    public void BeginCompletionSequence()
    {
        if (!enableCompletionSequence || completionDone || completionAnimating)
            return;

        completionAnimating = true;

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
            StartStarsPhase();
        });

        seq.Play();
    }

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
    }



    private void HandleStarClicks()
    {
        if (Input.GetMouseButtonDown(0))
        {
        
            Vector2 screenPos = Input.mousePosition;

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

    private void ShowStarMessage(int index)
    {
        if (starsClicked[index]) return;
        starsClicked[index] = true;
        starsClickedCount++;

        StarMessageGroup group = starMessageGroups[index];
        if (group.rootCanvas != null)
        {
            group.rootCanvas.gameObject.SetActive(true);
            group.rootCanvas.alpha = 0f;
            group.rootCanvas.DOFade(1f, starFadeDuration).SetEase(Ease.InSine);//显示图片+文字
            group.star.GetComponent<SpriteRenderer>().DOFade(0f, starFadeDuration).SetEase(Ease.InSine);//隐藏星星
        }

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
