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
    public GameObject silhouetteObject;
    public float fadeInDuration = 0.5f;

    private int currentWaypointIndex = 0;
    private bool isWaitingForStart = true;

    void Start()
    {
        if (paintSystem == null)
            paintSystem = FindObjectOfType<MspPaint>();

        if (silhouetteObject != null)
        {
            SpriteRenderer sr = silhouetteObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        }

        if (paintSystem != null)
        {
            paintSystem.OnDrawingPositionUpdated += OnDrawingPosition;
            paintSystem.OnDrawingFinished += OnDrawingFinished;
        }
    }

    void Update()
    {
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
            // 如果没连完，回退
            if (paintSystem != null)
            {
                paintSystem.RetractLine(() => {
                    currentWaypointIndex = 0;
                    isWaitingForStart = true;
                });
            }
        }
    }

    void OnConnectComplete()
    {
        isComplete = true;
        
        if (silhouetteObject != null)
        {
            SpriteRenderer sr = silhouetteObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.DOFade(1f, fadeInDuration);
            }
            else
            {
                silhouetteObject.SetActive(true);
            }
        }

        // 通知管理器
        S11_3_Manager manager = FindObjectOfType<S11_3_Manager>();
        if (manager != null)
        {
            manager.OnLineConnectComplete();
        }
    }

    private Vector3 GetMousePosition()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        return hit.collider != null ? hit.point : (Vector3)mousePos;
    }
}
