using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("跟随参数")]
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;

    [Header("边界限制")]
    public bool useBounds = true;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    private Camera mainCamera;
    private float halfWidth;
    private float halfHeight;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        CalculateCameraBounds();
    }

    void CalculateCameraBounds()
    {
        if (mainCamera != null)
        {
            halfHeight = mainCamera.orthographicSize;
            halfWidth = halfHeight * mainCamera.aspect;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX + halfWidth, maxX - halfWidth);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY + halfHeight, maxY - halfHeight);
        }

        transform.position = smoothedPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, 0),
                new Vector3(maxX - minX, maxY - minY, 1)
            );
        }
    }
}