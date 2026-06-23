using UnityEngine;

public class ScreenEdgeIndicator : MonoBehaviour
{
    [Header("摄像机")]
    public Camera targetCamera;          // 默认为主摄像机

    [Header("边缘偏移 & 平滑参数")]
    [SerializeField] private float edgeOffset = 0.05f;          // 边缘内缩（视口单位）
    [SerializeField] private float moveSmoothTime = 0.2f;      // 移动平滑时间（越小跟随越快）

    [Header("浮动参数（仅在原位置时生效）")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatAmplitude = 0.3f;      // 浮动幅度（世界单位）
    [SerializeField] private float floatSpeed = 2f;            // 浮动频率（周期/秒）

    // 私有变量
    public Vector3 initialPosition;      // 初始世界位置（屏幕内时的驻留点）
    private SpriteRenderer spriteRenderer;

    // SmoothDamp 使用的速度缓存
    private Vector3 moveVelocity = Vector3.zero;

    void Awake()
    {
        initialPosition = transform.position;

        if (targetCamera == null)
            targetCamera = Camera.main;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            Debug.LogWarning("未找到 SpriteRenderer，建议添加以便控制显隐。");
    }

    void Update()
    {
        if (targetCamera == null) return;

        // 1. 判断初始位置是否在屏幕内（包含 Z 轴前方检测）
        Vector3 viewportPos = targetCamera.WorldToViewportPoint(initialPosition);
        bool isInside = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                        viewportPos.y >= 0 && viewportPos.y <= 1 &&
                        viewportPos.z > 0;

        Vector3 baseTargetPos;   // 基础目标位置（不含浮动）

        if (isInside)
        {
            // ----- 屏幕内：回到初始位置 -----
            baseTargetPos = initialPosition;
        }
        else
        {
            // ----- 屏幕外：计算边缘位置（仅位置指示，无旋转） -----
            Vector3 screenCenter = new Vector3(0.5f, 0.5f, viewportPos.z);
            Vector3 dir = (viewportPos - screenCenter).normalized;

            // 计算射线与屏幕边界的交点参数 t
            float tX = float.MaxValue, tY = float.MaxValue;
            if (dir.x > 0) tX = (1 - screenCenter.x) / dir.x;
            else if (dir.x < 0) tX = (0 - screenCenter.x) / dir.x;
            if (dir.y > 0) tY = (1 - screenCenter.y) / dir.y;
            else if (dir.y < 0) tY = (0 - screenCenter.y) / dir.y;

            float t = Mathf.Min(tX, tY);
            if (float.IsInfinity(t) || float.IsNaN(t)) t = 0;
            t = Mathf.Max(0, t - edgeOffset); // 边缘内缩

            Vector3 edgeViewport = screenCenter + dir * t;
            edgeViewport.z = viewportPos.z;
            baseTargetPos = targetCamera.ViewportToWorldPoint(edgeViewport);
            baseTargetPos.z = initialPosition.z; // 保持 Z 不变
        }

        // 2. 应用浮动偏移：仅当在屏幕内（即位于原位置）时才浮动
        Vector3 finalTargetPos = baseTargetPos;
        if (enableFloat && isInside)   // 修改点：条件改为 isInside
        {
            float floatOffset = Mathf.Sin(Time.time * floatSpeed * Mathf.PI * 2f) * floatAmplitude;
            finalTargetPos += Vector3.up * floatOffset;
        }

        // 3. 平滑移动到最终目标位置（使用 SmoothDamp）
        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalTargetPos,
            ref moveVelocity,
            moveSmoothTime
        );

        // 可选：屏幕内隐藏提示（取消注释下一行）
        // if (spriteRenderer != null) spriteRenderer.enabled = !isInside;
    }

    // 编辑器下显示初始位置标记（方便调试）
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            initialPosition = transform.position;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(initialPosition, 0.3f);
    }
}