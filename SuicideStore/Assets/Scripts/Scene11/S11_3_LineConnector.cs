using UnityEngine;
using DG.Tweening;

public class S11_3_LineConnector : MonoBehaviour
{
    [Header("连线设置")]
    public Transform[] waypoints;
    public float hitRadius = 0.5f;
    public MspPaint paintSystem;
    public bool isComplete = false;

    [Header("完成后显示")]
    public CanvasGroup silhouetteObject;
    public float fadeInDuration = 0.5f;

    private int currentWaypointIndex = 0;
    private bool isWaitingForStart = true;

    void Start()
    {
        if (paintSystem == null)
            paintSystem = FindObjectOfType<MspPaint>();

        if (paintSystem != null)
        {
            paintSystem.OnDrawingPositionUpdated += OnDrawingPosition;
            paintSystem.OnDrawingFinished += OnDrawingFinished;
        }

        // 初始只显示第一个点
        HideAllWaypoints();
        if (waypoints.Length > 0)
            ShowWaypoint(0);
    }

    void HideAllWaypoints()
    {
        foreach (var wp in waypoints)
        {
            if (wp != null)
                wp.gameObject.SetActive(false);
        }
    }

    void ShowWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Length && waypoints[index] != null)
            waypoints[index].gameObject.SetActive(true);
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
            paintSystem.RetractLine(() => {
                currentWaypointIndex = 0;
                isWaitingForStart = true;
                HideAllWaypoints();
                ShowWaypoint(0);
            });
        }
    }

    void OnConnectComplete()
    {
        isComplete = true;

        if (silhouetteObject != null)
        {
            silhouetteObject.DOFade(1f, fadeInDuration);
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