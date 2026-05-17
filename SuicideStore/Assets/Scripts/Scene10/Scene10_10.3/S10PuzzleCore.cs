using UnityEngine;
using UnityEngine.Events;

public class S10PuzzleCore : MonoBehaviour
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

    // Private fields
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

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        ResolveReferences();
        InitializeArrays();
        PreparePieces();
    }

    private void Update()
    {
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

    private void ResolveReferences()
    {
        int count = bindings != null ? bindings.Length : 0;
        pieceTransforms = new Transform[count];
        targetTransforms = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            Transform p = bindings[i].piece;
            Transform t = bindings[i].target;

            if (p == null && pieceNames != null && i < pieceNames.Length)
            {
                GameObject go = GameObject.Find(pieceNames[i]);
                if (go != null) p = go.transform;
            }

            if (t == null && targetNames != null && i < targetNames.Length)
            {
                GameObject go = GameObject.Find(targetNames[i]);
                if (go != null) t = go.transform;
            }

            pieceTransforms[i] = p;
            targetTransforms[i] = t;
        }
    }

    private void InitializeArrays()
    {
        int count = pieceTransforms.Length;
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
            Transform p = pieceTransforms[i];
            Transform t = targetTransforms[i];
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

            magnetPositionStrength[i] = bindings[i].magnetPositionStrength > 0f ? bindings[i].magnetPositionStrength : 18f;
            magnetRotationSpeed[i] = bindings[i].magnetRotationSpeed > 0f ? bindings[i].magnetRotationSpeed : 18f;

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
                    string tn = t != null ? t.name : "<null>";
                    Debug.Log($"S10PuzzleCore: Bind[{i}] piece={p.name}, target={tn}, snapRadius={Mathf.Sqrt(snapRadiusSqr[i]):F3}, magnetRadius={Mathf.Sqrt(magnetRadiusSqr[i]):F3}");
                }
            }
            else
            {
                Debug.LogWarning($"S10PuzzleCore: Missing piece binding at index {i}.");
            }
        }
    }

    private void PreparePieces()
    {
        if (autoUnparentPiecesFromEachOther)
            UnparentPiecesFromEachOther();

        if (ignorePieceToPieceCollisions)
            IgnorePieceToPieceCollisions();
    }

    // ---------- Drag & Snap (核心逻辑与原代码相同，只保留拼图部分) ----------
    private void TryBeginDrag(Vector3 pointerWorld)
    {
        int hitCount = Physics2D.OverlapPointNonAlloc(pointerWorld, overlapBuffer, pieceLayerMask);
        int bestIndex = -1;
        int bestSortingOrder = int.MinValue;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (hit == null) continue;
            int idx = IndexOfCollider(hit);
            if (idx < 0 || placed[idx]) continue;
            int sortingOrder = pieceSpriteRenderers[idx] != null ? pieceSpriteRenderers[idx].sortingOrder : 0;
            if (bestIndex < 0 || sortingOrder > bestSortingOrder)
            {
                bestIndex = idx;
                bestSortingOrder = sortingOrder;
            }
        }

        if (bestIndex < 0)
        {
            if (TryPickBySpriteBounds(pointerWorld, out int boundsPick))
                bestIndex = boundsPick;
            else
                return;
        }

        BeginDragIndex(bestIndex, pointerWorld);
    }

    private void BeginDragIndex(int idx, Vector3 pointerWorld)
    {
        Transform piece = pieceTransforms[idx];
        if (piece == null) return;

        // Freeze others
        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            if (i == idx || placed[i]) continue;
            Transform t = pieceTransforms[i];
            if (t == null) continue;
            frozenPositions[i] = t.position;
            frozenRotations[i] = t.rotation;
            if (pieceRigidbodies[i] != null) pieceRigidbodies[i].isKinematic = true;
        }
        othersFrozen = true;
        if (pieceRigidbodies[idx] != null) pieceRigidbodies[idx].isKinematic = true;

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
        if (piece == null) { draggingIndex = -1; return; }

        Vector3 desiredPos = pointerWorld + dragOffsetWorld;
        desiredPos.z = draggingZ;
        piece.position = desiredPos;

        Transform target = targetTransforms[idx];
        if (target == null) return;

        Vector3 targetPos = target.position;
        targetPos.z = draggingZ;
        Vector3 snapPoint = GetPieceSnapPoint(idx);
        snapPoint.z = draggingZ;

        float proximity = GetMagnetProximity(idx, snapPoint, targetPos);
        if (proximity <= 0f) return;
        if (proximity > 1f) proximity = 1f;

        float dt = Time.deltaTime;

        if (lockRotationToTarget[idx])
        {
            float rotT = magnetRotationSpeed[idx] * proximity * dt;
            if (rotT > 1f) rotT = 1f;
            piece.rotation = Quaternion.Slerp(piece.rotation, target.rotation, rotT);
        }

        snapPoint = GetPieceSnapPoint(idx);
        snapPoint.z = draggingZ;
        Vector3 delta = targetPos - snapPoint;
        float posT = magnetPositionStrength[idx] * proximity * dt;
        if (posT > 1f) posT = 1f;
        Vector3 p = piece.position;
        p.x += delta.x * posT;
        p.y += delta.y * posT;
        p.z = draggingZ;
        piece.position = p;

        if (!autoSnapWhileDragging || placed[idx]) return;

        Vector3 finalSnapPoint = GetPieceSnapPoint(idx);
        Vector3 finalTargetPos = target.position;
        if (IsWithinAutoSnap(idx, finalSnapPoint, finalTargetPos))
        {
            SpriteRenderer psr = pieceSpriteRenderers[idx];
            if (psr != null) psr.sortingOrder = draggingOriginalSortingOrder;
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

        Transform piece = idx >= 0 ? pieceTransforms[idx] : null;
        if (piece == null) return;

        SpriteRenderer psr = pieceSpriteRenderers[idx];
        if (psr != null) psr.sortingOrder = draggingOriginalSortingOrder;

        if (TrySnap(idx)) return;

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
        if (piece == null || target == null) return false;

        Vector3 snapPoint = GetPieceSnapPoint(idx);
        if (!IsWithinSnap(idx, snapPoint, target.position, out float dist, out float threshold))
        {
            if (enableDebugLogs)
                Debug.Log($"S10PuzzleCore: Not within snap radius. idx={idx}, dist={dist:F3}, threshold={threshold:F3}");
            return false;
        }

        SnapToTarget(idx);
        return true;
    }

    private void SnapToTarget(int idx)
    {
        Transform piece = pieceTransforms[idx];
        Transform target = targetTransforms[idx];
        if (piece == null || target == null) return;

        Vector3 targetPos = target.position;
        Vector3 snapPoint = GetPieceSnapPoint(idx);
        Vector3 delta = targetPos - snapPoint;
        Vector3 pos = piece.position;
        pos.x += delta.x;
        pos.y += delta.y;
        piece.position = pos;

        if (lockRotationToTarget[idx])
            piece.rotation = target.rotation;

        if (pieceColliders[idx] != null)
            pieceColliders[idx].enabled = false;

        if (!placed[idx])
        {
            placed[idx] = true;
            placedCount++;
            if (placedCount == placed.Length)
                onAllPiecesPlaced?.Invoke();
        }
    }

    // ---------- Helper methods (与原代码相同) ----------
    private int IndexOfCollider(Collider2D collider)
    {
        if (collider == null) return -1;
        for (int i = 0; i < pieceColliders.Length; i++)
        {
            if (pieceColliders[i] == collider) return i;
            Transform p = pieceTransforms[i];
            if (p != null && collider.transform.IsChildOf(p)) return i;
        }
        return -1;
    }

    private Collider2D EnsureClickableCollider(Transform t)
    {
        Collider2D col = t.GetComponentInChildren<Collider2D>(true);
        if (col != null) return col;
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

    private static Collider2D[] GetAllColliders(Transform piece) => piece?.GetComponentsInChildren<Collider2D>(true);

    private static void EnsureCollidersEnabled(Collider2D[] cols, bool makeTriggers)
    {
        if (cols == null) return;
        foreach (var c in cols)
        {
            if (c == null) continue;
            if (!c.enabled) c.enabled = true;
            if (makeTriggers) c.isTrigger = true;
        }
    }

    private void UnparentPiecesFromEachOther()
    {
        Transform root = transform;
        for (int i = 0; i < pieceTransforms.Length; i++)
        {
            Transform pi = pieceTransforms[i];
            if (pi == null) continue;
            for (int j = 0; j < pieceTransforms.Length; j++)
            {
                if (i == j) continue;
                Transform pj = pieceTransforms[j];
                if (pj == null) continue;
                if (pj.IsChildOf(pi))
                {
                    if (enableDebugLogs) Debug.LogWarning($"S10PuzzleCore: {pj.name} is child of {pi.name}. Reparenting.");
                    pj.SetParent(root, true);
                }
            }
        }
    }

    private void IgnorePieceToPieceCollisions()
    {
        if (perPieceColliders == null) return;
        for (int i = 0; i < perPieceColliders.Length; i++)
        {
            var a = perPieceColliders[i];
            if (a == null) continue;
            for (int j = i + 1; j < perPieceColliders.Length; j++)
            {
                var b = perPieceColliders[j];
                if (b == null) continue;
                foreach (var ca in a)
                    foreach (var cb in b)
                        if (ca != null && cb != null)
                            Physics2D.IgnoreCollision(ca, cb, true);
            }
        }
    }

    private bool TryPickBySpriteBounds(Vector3 pointerWorld, out int bestIndex)
    {
        bestIndex = -1;
        int bestSortingOrder = int.MinValue;
        for (int i = 0; i < pieceSpriteRenderers.Length; i++)
        {
            if (placed[i]) continue;
            SpriteRenderer sr = pieceSpriteRenderers[i];
            if (sr == null || !sr.enabled) continue;
            Bounds b = sr.bounds;
            if (pointerWorld.x >= b.min.x && pointerWorld.x <= b.max.x && pointerWorld.y >= b.min.y && pointerWorld.y <= b.max.y)
            {
                int so = sr.sortingOrder;
                if (bestIndex < 0 || so > bestSortingOrder)
                {
                    bestIndex = i;
                    bestSortingOrder = so;
                }
            }
        }
        return bestIndex >= 0;
    }

    private Vector3 GetPieceSnapPoint(int idx)
    {
        if (!useSpriteBoundsCenterForSnap)
        {
            Transform p = pieceTransforms[idx];
            return p != null ? p.position : default;
        }
        SpriteRenderer sr = pieceSpriteRenderers[idx];
        if (sr != null) return sr.bounds.center;
        Transform pt = pieceTransforms[idx];
        return pt != null ? pt.position : default;
    }

    private static float SqrDistanceXY(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    private float GetMagnetProximity(int idx, Vector3 snapPointWorld, Vector3 targetWorld)
    {
        if (useScreenSpaceDistances && mainCamera != null)
        {
            Vector2 a = mainCamera.WorldToScreenPoint(snapPointWorld);
            Vector2 b = mainCamera.WorldToScreenPoint(targetWorld);
            float distSqr = (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
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
            float distSqr = (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
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
            float distSqr = (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
            float r = autoSnapRadiusPixels > 0f ? autoSnapRadiusPixels : snapRadiusPixels;
            return distSqr <= r * r;
        }
        float rW = autoSnapRadiusWorld > 0f ? autoSnapRadiusWorld : Mathf.Sqrt(snapRadiusSqr[idx]);
        float worldDistSqr = SqrDistanceXY(snapPointWorld, targetWorld);
        return worldDistSqr <= rW * rW;
    }

    private bool TryGetPointerDown(out Vector3 pointerWorld)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pointerWorld = ScreenToWorld(Input.GetTouch(0).position);
            return true;
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

    private bool IsPointerActive() => Input.GetMouseButton(0) || Input.touchCount > 0;

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 p = new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(p);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (useScreenSpaceDistances) return;
        if (bindings == null) return;
        for (int i = 0; i < bindings.Length; i++)
        {
            Transform t = bindings[i].target;
            if (t == null) continue;
            float r = bindings[i].snapRadius > 0f ? bindings[i].snapRadius : defaultSnapRadius;
            float mr = bindings[i].magnetRadius > 0f ? bindings[i].magnetRadius : r * defaultMagnetRadiusMultiplier;
            Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
            Gizmos.DrawWireSphere(t.position, mr);
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
            Gizmos.DrawWireSphere(t.position, r);
        }
    }
#endif
}