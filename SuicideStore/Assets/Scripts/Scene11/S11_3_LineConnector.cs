using UnityEngine;
using DG.Tweening;
using System.Collections;

public class S11_3_LineConnector : MonoBehaviour
{
    [Header("连线设置")]
    public Transform[] waypoints;
    public float hitRadius = 0.5f;
    public MspPaint paintSystem;
    public bool isComplete = false;

    [Header("线")]
    public GameObject lineFather;

    [Header("完成后显示")]
    public CanvasGroup silhouetteObject;
    public float fadeInDuration = 0.5f;

    [Header("弱引导")]
    public Material guideMaterial;
    [Header("引导动画参数")]
    public float guideFadeInDuration = 0.3f;
    public float guideDrawDuration = 0.6f;
    public float guideHoldDuration = 0.3f;
    public float guideFadeOutDuration = 0.2f;
    public float guideLoopInterval = 1f;

    public float waypointFadeDuration = 0.3f;

    private Coroutine guideCoroutine;
    private LineRenderer guideLine;

    private int currentWaypointIndex = 0;
    private bool isWaitingForStart = true;

    private SpriteRenderer[] pointRenderers;

    void Start()
    {
        if (paintSystem == null)
            paintSystem = FindObjectOfType<MspPaint>();

        if (paintSystem != null)
        {
            paintSystem.OnDrawingPositionUpdated += OnDrawingPosition;
            paintSystem.OnDrawingFinished += OnDrawingFinished;
        }

        pointRenderers = new SpriteRenderer[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                SpriteRenderer sr = waypoints[i].GetComponent<SpriteRenderer>();
                if (sr == null)
                    sr = waypoints[i].gameObject.AddComponent<SpriteRenderer>();
                pointRenderers[i] = sr;
                Color c = sr.color;
                c.a = 0f;
                sr.color = c;
                sr.gameObject.SetActive(false);
            }
        }

        if (waypoints.Length > 0)
            FadeWaypoint(0, 1f, true, waypointFadeDuration);

        if (waypoints.Length >= 3 && paintSystem != null)
        {
            CreateGuideLine();
            guideCoroutine = StartCoroutine(PlayGuideLoop());
        }
    }

    void CreateGuideLine()
    {
        if (paintSystem == null) return;

        GameObject go = new GameObject("GuideLine");
        go.transform.SetParent(transform);
        guideLine = go.AddComponent<LineRenderer>();

        Material mat = guideMaterial != null ? guideMaterial : paintSystem.lineMaterial;
        if (mat == null)
            mat = new Material(Shader.Find("Sprites/Default"));

        guideLine.material = mat;
        guideLine.startWidth = paintSystem.paintSize;
        guideLine.endWidth = paintSystem.paintSize;
        guideLine.startColor = paintSystem.paintColor;
        guideLine.endColor = paintSystem.paintColor;
        guideLine.numCornerVertices = 5;
        guideLine.numCapVertices = 5;
        guideLine.useWorldSpace = true;
        guideLine.gameObject.SetActive(false);
    }

    IEnumerator PlayGuideLoop()
    {
        while (isWaitingForStart && !isComplete && guideLine != null)
        {
            // 直接隐藏点1、2（无动画）
            HideWaypoint(1);
            HideWaypoint(2);
            // 确保点0可见（淡入）
            FadeWaypoint(0, 1f, false, waypointFadeDuration);

            yield return StartCoroutine(PlayGuideOnce());

            HideWaypoint(1);
            HideWaypoint(2);
            yield return new WaitForSeconds(guideLoopInterval);
        }
        if (guideLine != null)
        {
            guideLine.gameObject.SetActive(false);
            Destroy(guideLine.gameObject);
            guideLine = null;
        }
    }

    IEnumerator PlayGuideOnce()
    {
        if (guideLine == null || waypoints.Length < 3 || paintSystem == null)
            yield break;

        float lineWidth = paintSystem.paintSize;
        Color lineColor = paintSystem.paintColor;

        guideLine.startWidth = lineWidth;
        guideLine.endWidth = lineWidth;
        guideLine.startColor = lineColor;
        guideLine.endColor = lineColor;

        guideLine.positionCount = 2;
        Vector3[] positions = new Vector3[3];
        positions[0] = waypoints[0].position;
        positions[1] = positions[0];
        guideLine.SetPositions(new Vector3[] { positions[0], positions[1] });

        Color col = lineColor;
        col.a = 0;
        guideLine.startColor = col;
        guideLine.endColor = col;
        guideLine.gameObject.SetActive(true);

        yield return DOTween.To(() => 0f, x => { col.a = x; guideLine.startColor = col; guideLine.endColor = col; }, 1f, guideFadeInDuration).WaitForCompletion();

        // 点0确保可见
        FadeWaypoint(0, 1f, false, 0.1f);

        // 第一段
        Vector3 startPos = waypoints[0].position;
        Vector3 endPos = waypoints[1].position;
        float elapsed = 0f;
        while (elapsed < guideDrawDuration)
        {
            if (guideLine == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / guideDrawDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            positions[1] = currentPos;
            guideLine.SetPositions(new Vector3[] { positions[0], positions[1] });
            yield return null;
        }
        if (guideLine == null) yield break;
        positions[1] = endPos;
        guideLine.SetPositions(new Vector3[] { positions[0], positions[1] });

        FadeWaypoint(1, 1f, true, waypointFadeDuration);

        // 第二段
        if (guideLine == null) yield break;
        guideLine.positionCount = 3;
        positions[0] = waypoints[0].position;
        positions[1] = waypoints[1].position;
        positions[2] = positions[1];
        guideLine.SetPositions(positions);

        startPos = waypoints[1].position;
        endPos = waypoints[2].position;
        elapsed = 0f;
        while (elapsed < guideDrawDuration)
        {
            if (guideLine == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / guideDrawDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            positions[2] = currentPos;
            guideLine.SetPositions(positions);
            yield return null;
        }
        if (guideLine == null) yield break;
        positions[2] = endPos;
        guideLine.SetPositions(positions);

        FadeWaypoint(2, 1f, true, waypointFadeDuration);

        yield return new WaitForSeconds(guideHoldDuration);
        if (guideLine == null) yield break;
        yield return DOTween.To(() => 1f, x => { col.a = x; guideLine.startColor = col; guideLine.endColor = col; }, 0f, guideFadeOutDuration).WaitForCompletion();
        if (guideLine != null)
            guideLine.gameObject.SetActive(false);
    }

    private void FadeWaypoint(int index, float targetAlpha, bool activeAfterFade, float duration)
    {
        if (index < 0 || index >= pointRenderers.Length) return;
        SpriteRenderer sr = pointRenderers[index];
        if (sr == null) return;

        sr.DOKill();

        if (targetAlpha == 0f && !activeAfterFade)
        {
            sr.gameObject.SetActive(false);
        }
        else if (targetAlpha == 1f && activeAfterFade)
        {
            sr.gameObject.SetActive(true);
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
            sr.DOFade(1f, duration);
        }
        else
        {
            sr.DOFade(targetAlpha, duration);
            if (activeAfterFade && !sr.gameObject.activeSelf)
                sr.gameObject.SetActive(true);
            else if (!activeAfterFade && targetAlpha == 0f)
                sr.gameObject.SetActive(false);
        }
    }

    private void HideWaypoint(int index)
    {
        if (index < 0 || index >= pointRenderers.Length) return;
        if (pointRenderers[index] != null)
            pointRenderers[index].gameObject.SetActive(false);
    }

    private void ShowWaypoint(int index)
    {
        FadeWaypoint(index, 1f, true, waypointFadeDuration);
    }

    private void HideAllWaypoints()
    {
        for (int i = 0; i < pointRenderers.Length; i++)
        {
            if (pointRenderers[i] != null)
                pointRenderers[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (isComplete || paintSystem == null) return;

        if (isWaitingForStart && !paintSystem.IsDrawing && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = GetMousePosition();
            if (waypoints.Length > 0 && Vector3.Distance(mousePos, waypoints[0].position) <= hitRadius)
            {
                if (guideCoroutine != null)
                {
                    StopCoroutine(guideCoroutine);
                    guideCoroutine = null;
                }
                if (guideLine != null)
                {
                    guideLine.gameObject.SetActive(false);
                    Destroy(guideLine.gameObject);
                    guideLine = null;
                }

                HideAllWaypoints();
                ShowWaypoint(0);

                isWaitingForStart = false;
                paintSystem.StartDrawing(mousePos);
            }
        }
    }

    void OnDrawingPosition(Vector3 currentPos)
    {
        if (isComplete || currentWaypointIndex >= waypoints.Length) return;

        Transform target = waypoints[currentWaypointIndex];
        if (Vector3.Distance(currentPos, target.position) <= hitRadius)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex < waypoints.Length)
                ShowWaypoint(currentWaypointIndex);

            if (currentWaypointIndex >= waypoints.Length)
            {
                OnConnectComplete();
            }
        }
    }

    void OnDrawingFinished()
    {
        if (currentWaypointIndex < waypoints.Length && !isComplete)
        {
            // 如果只到达起点（没有其他点亮），直接重置
            if (currentWaypointIndex <= 1)
            {
                paintSystem.RetractLine(() => {
                    currentWaypointIndex = 0;
                    isWaitingForStart = true;
                    HideAllWaypoints();
                    ShowWaypoint(0);
                });
                return;
            }

            // 启动协程，在 retractDuration 内依次隐藏已亮起的点（从最后一个开始）
            StartCoroutine(HidePointsDuringRetract(currentWaypointIndex + 1, paintSystem.retractDuration, () =>
            {
                // 回退完成后的重置操作
                currentWaypointIndex = 0;
                isWaitingForStart = true;
                HideAllWaypoints();
                ShowWaypoint(0);
            }));

            // 同时启动线的回退（不需要额外回调）
            paintSystem.RetractLine();
        }
    }
    IEnumerator HidePointsDuringRetract(int reachedCount, float duration, System.Action onComplete)
    {
        // reachedCount 是已经点亮的点数量（包括起点）
        // 需要隐藏的点是索引 1 到 reachedCount-1
        int pointCount = reachedCount - 1; // 不包括起点
        if (pointCount <= 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float interval = duration / pointCount;
        // 从最后一个点开始隐藏（索引从大到小）
        for (int i = reachedCount - 1; i >= 1; i--)
        {
            if (i < pointRenderers.Length && pointRenderers[i] != null)
                pointRenderers[i].gameObject.SetActive(false);
            yield return new WaitForSeconds(interval);
        }

        onComplete?.Invoke();
    }
    void OnConnectComplete()
    {
        isComplete = true;

        if (silhouetteObject != null)
        {
            silhouetteObject.DOFade(1f, fadeInDuration).OnComplete(() => { lineFather.SetActive(false); });
        }

        S11_3_Manager manager = FindObjectOfType<S11_3_Manager>();
        if (manager != null)
            manager.OnLineConnectComplete();
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        return hit.collider != null ? hit.point : (Vector3)mousePos;
    }
}