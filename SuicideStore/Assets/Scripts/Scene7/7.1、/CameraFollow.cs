using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("跟随参数")]
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;

    [Header("背景边界限制")]
    public bool useBackgroundBounds = true;
    public Transform backgroundBoundsObject;
    public Vector2 backgroundSize = new Vector2(20f, 10f);

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
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        if (useBackgroundBounds)
        {
            float minX, maxX, minY, maxY;

            if (backgroundBoundsObject != null)
            {
                Vector3 boundsCenter = backgroundBoundsObject.position;
                Vector3 boundsScale = backgroundBoundsObject.localScale;
                
                minX = boundsCenter.x - boundsScale.x / 2f + halfWidth;
                maxX = boundsCenter.x + boundsScale.x / 2f - halfWidth;
                minY = boundsCenter.y - boundsScale.y / 2f + halfHeight;
                maxY = boundsCenter.y + boundsScale.y / 2f - halfHeight;
            }
            else
            {
                minX = -backgroundSize.x / 2f + halfWidth;
                maxX = backgroundSize.x / 2f - halfWidth;
                minY = -backgroundSize.y / 2f + halfHeight;
                maxY = backgroundSize.y / 2f - halfHeight;
            }

            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
        }

        transform.position = smoothedPosition;
    }
}