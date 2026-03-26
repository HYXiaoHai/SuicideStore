using UnityEngine;
using UnityEngine.UI; // 如果 WhiteMask 是 Image 组件
using UnityEngine.EventSystems; // 必须引用，用于检测 UI 点击

[RequireComponent(typeof(RectTransform))]
public class ClockController : MonoBehaviour
{
    [Header("指针")]
    public Transform minuteHand;
    public Transform hourHand;
    [SerializeField] private float startAngle = 0f;      // 初始角度
    [SerializeField] private float handZeroOffset = 90f;  // 贴图补偿偏移（横着指向3点则填90）

    [Header("WhiteMask 目标")]
    public Graphic maskGraphic;

    [Header("参数")]
    public float autoRotateSpeed = 6f; // 自动转动速度
    public float interactRadius = 50f; // 鼠标距离分针末端的检测半径（像素）

    private float currentAngle = 0f;
    private bool isComplete = false;
    private bool isDragging = false;
    private Material targetMaterial;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        currentAngle = startAngle;

        if (maskGraphic != null)
        {
            // 建议：在 Inspector 里给 MaskGraphic 赋一个专用的材质球，不要用默认材质
            targetMaterial = maskGraphic.material;
        }

        UpdateAspectRatio();
        UpdateHand();
        UpdateShader();
    }

    void Update()
    {
        if (isComplete || targetMaterial == null) return;

        HandleInput();

        // 只有在不拖拽时才自动转动
        if (!isDragging)
        {
            currentAngle += autoRotateSpeed * Time.deltaTime;
        }

        // 胜利条件检测
        if (currentAngle >= 360f)
        {
            currentAngle = 360f;
            Complete();
        }

        UpdateHand();
        UpdateShader();
    }

    private void HandleInput()
    {
        // 1. 按下鼠标：判断是否点中了分针
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverMinuteHand())
            {
                isDragging = true;
            }
        }

        // 2. 拖拽中：计算角度
        if (Input.GetMouseButton(0) && isDragging)
        {
            float mouseAngle = GetAngleFromMouse();

            // 逻辑限制：
            // 我们只允许顺时针增加，且不能逆向越过 0 度（12点左侧）
            // 如果玩家拖拽的角度小于当前自动转动的角度，则视为“想往回拨”，我们不予理睬或释放拖拽
            if (mouseAngle >= currentAngle && mouseAngle <= 360f)
            {
                currentAngle = mouseAngle;
            }
            else if (mouseAngle < currentAngle - 10f) // 如果拖拽的角度远小于当前角度（说明想往回拨）
            {
                isDragging = false; // 脱离玩家转动
            }
        }

        // 3. 松开鼠标
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // 获取鼠标相对于表盘中心的角度（0-360，12点为0）
    private float GetAngleFromMouse()
    {
        Vector2 localPoint;
        // 将屏幕坐标转换为 UI 本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera, out localPoint);

        // 使用 atan2(x, y) 得到 12点为0度的角度
        float angle = Mathf.Atan2(localPoint.x, localPoint.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        return angle;
    }

    // 检测鼠标是否在分针附近
    private bool IsMouseOverMinuteHand()
    {
        if (minuteHand == null) return false;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, canvas.worldCamera, out localMousePos);

        // 计算分针当前的方向向量
        float rad = (90f - (-currentAngle + handZeroOffset)) * Mathf.Deg2Rad;
        Vector2 handDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 简单的点线距离判断，或者直接判断点击位置与分针角度的差值
        float mouseAngle = GetAngleFromMouse();
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(mouseAngle, currentAngle));

        // 如果鼠标角度和分针角度差距小于 15度，且距离中心有一定距离，则认为点中
        return angleDiff < 15f && localMousePos.magnitude > 20f;
    }

    void UpdateAspectRatio()
    {
        if (rectTransform != null && targetMaterial != null)
        {
            float aspect = rectTransform.rect.width / rectTransform.rect.height;
            targetMaterial.SetFloat("_AspectRatio", aspect);
        }
    }

    void UpdateHand()
    {
        if (minuteHand != null)
        {
            float rot = -currentAngle + 90f;
            minuteHand.localRotation = Quaternion.Euler(0, 0, rot);
        }

        if (hourHand != null)
        {
            // 时针联动（可选）：12倍速度差
            float hourRot = -currentAngle / 12f + handZeroOffset;
            hourHand.localRotation = Quaternion.Euler(0, 0, hourRot+90f);
        }
    }

    void UpdateShader()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat("_Angle", currentAngle);
        }
    }

    void Complete()
    {
        isComplete = true;
        if (minuteHand != null) minuteHand.gameObject.SetActive(false);
        if (hourHand != null) hourHand.gameObject.SetActive(false);

        targetMaterial.SetFloat("_Angle", 360f);
        Debug.Log("游戏胜利！分针已转满一圈。");
    }
}