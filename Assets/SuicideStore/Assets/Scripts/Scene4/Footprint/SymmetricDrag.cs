using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class SymmetricDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;   // 起始位置
    private bool gameStarted = false; // 是否已开始

    [Header("物体引用")]
    public RectTransform object2;//物体2的RectTransform
    public RectTransform target;//终点的RectTransform
    public float offestRedius = 0.1f;//允许的过关偏移量
    [Header("图片进度条")]
    public RectTransform ImageSlider;//物品索引
    public RectTransform startPoint;//开始位置
    public RectTransform endPoint;//结束位置
    public float maxValue;//终点到起始点的距离
    public float currentValue;//当前移动的距离
    private float startX;          // 物品1起始 X 坐标
    private bool moveRight;        // 物品1是否需要向右移动才能到达终点

    [Header("脚印偏移")]
    public float lateralOffsetDistance = 0.2f;   // 横向偏移距离
    private int currentOffsetSign_lele = 1;           // 1=右, -1=左（交替）
    private int currentOffsetSign_parent = 1;           // 1=右, -1=左（交替）
    [Header("脚印")]
    public GameObject footFather;//脚印的父物体，方便管理
    public GameObject leleFoot_Prefab;//乐乐的脚印
    public GameObject momFoot_Prefab;//妈妈的脚印
    public GameObject dadFoot_Prefab;//父亲的脚印
    public float needDistance;//每移动多少距离就会诞生一个脚印
    public float needTime;//诞生一个脚印的时间限制
    private float lastSpawnTime;  // 新增
    public float currentDistance;//当前移动的距离  
    public float duration;//显隐的时间
    private Vector3 lastObject2Position;  //物体2的上一帧位置
    private Vector3 lastPosition;            //上一帧位置（用于计算移动距离）
    private int parentFootGroup = 0;         //0=爸爸组, 1=妈妈组
    private int parentFootCountInGroup = 0;  //当前组已生成的数量（最多2）


    [Header("对称轴设置")]
    public Transform symmetryAxis;//对称轴
    public float symmetryY = 0f;//备用

    [Header("Canvas设置")]
    public Canvas canvas;

    [Header("边界限制（世界坐标）")]
    public float boundaryLeft = -10f;
    public float boundaryRight = 10f;
    public float boundaryBottom = -5f;
    public float boundaryTop = 5f;

    [Header("障碍物设置")]
    public bool enableObstacle = true;
    public LayerMask obstacleLayer;      // 可选，用于过滤（不再需要物理检测，但保留以防万一）
    public float object2Radius = 0.2f;   // 物品2的半径（手动设置，与物品1大小匹配）
    private List<Obstacle> obstacles;    // 场景中的所有障碍物
    public float object1Radius;         // 物品1的半径（从 CircleCollider2D 自动获取）

    [Header("关卡切换")]
    public int nextLevelIndex = 4;//第2关
    public float changeDelay = 1f;//完成后多久切换镜头（延迟）
    private bool hasTriggeredSwitch = false; //防止重复触发
    private bool isWin = false;

    private RectTransform rectTransform;
    private Plane canvasPlane;//Canvas 所在平面
    private Vector3 dragOffset;//物体世界位置与鼠标世界位置的偏移
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("找不到 Canvas 组件！");

    }

    void Start()
    {
        if (object2 != null)
            object2.position = rectTransform.position;
        lastObject2Position = object2.position;
        startPosition = rectTransform.position; 
                                                
        gameStarted = false; // 初始时禁止交互，等待 StartGame 调用
        // 初始化进度条位置
        ImageSlider.position = startPoint.position;



        // 记录起始 X 和目标 X
        startX = rectTransform.position.x;
        float targetX = target.position.x;
        moveRight = targetX > startX;               // 终点在右侧则为 true
        maxValue = Mathf.Abs(targetX - startX);     // 总水平移动距离（正数）
        currentValue = 0f;

        obstacles = new List<Obstacle>(FindObjectsOfType<Obstacle>());
    }
    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        // 重置胜利标志
        isWin = false;
        hasTriggeredSwitch = false;

        // 重置物体位置
        rectTransform.position = startPosition;
        if (object2 != null)
            object2.position = GetObject2Position(startPosition);

        // 重置进度条
        currentValue = 0f;
        UpdateImageSlider();


        // 重置障碍物可见性（如果障碍物脚本有 ResetVisibility 方法）
        if (enableObstacle && obstacles != null)
        {
            foreach (var obs in obstacles)
            {
                    obs.ResetVisibility();
            }
        }
        lastSpawnTime = Time.time - needTime;
        // 确保拖拽标志重置
        isDragging = false;
    }
    //计算鼠标在 Canvas 平面上的世界坐标
    private bool GetMouseWorldPositionOnCanvas(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        if (canvas == null) return false;

        Plane plane = new Plane(canvas.transform.forward, canvas.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter;
        if (plane.Raycast(ray, out enter))
        {
            worldPos = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!gameStarted || isWin) return;
        isDragging = true;
        Vector3 mouseWorld;
        if (GetMouseWorldPositionOnCanvas(out mouseWorld))
        {
            dragOffset = rectTransform.position - mouseWorld;
        }
        else
        {
            dragOffset = Vector3.zero;
            Debug.LogWarning("无法将鼠标投射到 Canvas 平面");
        }
        // 记录起始位置，用于距离计算
        lastPosition = rectTransform.position;
        lastObject2Position = object2.position;

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector3 mouseWorld;
        if (GetMouseWorldPositionOnCanvas(out mouseWorld))
        {
            Vector3 newPos = mouseWorld + dragOffset;
            newPos.x = Mathf.Clamp(newPos.x, boundaryLeft, boundaryRight);
            newPos.y = Mathf.Clamp(newPos.y, boundaryBottom, boundaryTop);

            if (enableObstacle && obstacles != null && obstacles.Count > 0)
            {
                newPos = PushOutOfObstacles(newPos);
            }

            rectTransform.position = newPos;
            UpdateObject2Position();

            // 计算移动方向和脚印生成（略）
            Vector3 moveDir1 = (rectTransform.position - lastPosition).normalized;
            float delta1 = Vector3.Distance(rectTransform.position, lastPosition);
            Vector3 moveDir2 = (object2.position - lastObject2Position).normalized;
            float delta2 = Vector3.Distance(object2.position, lastObject2Position);
            if (delta1 > 0)
            {
                currentDistance += delta1;
                while (currentDistance >= needDistance && Time.time - lastSpawnTime >= needTime &&!isWin)
                {
                    currentDistance -= needDistance;
                    SpawnLeleFootprint(moveDir1);
                    SpawnParentFootprint(moveDir2);
                    lastSpawnTime = Time.time;
                }
                lastPosition = rectTransform.position;
                lastObject2Position = object2.position;
            }

            // ========== 进度条更新（新代码） ==========
            float deltaX = rectTransform.position.x - startX;
            currentValue = moveRight ? deltaX : -deltaX;
            currentValue = Mathf.Clamp(currentValue, 0f, maxValue);
            UpdateImageSlider();
            // ======================================
            // ========== 障碍物显隐检测（新增） ==========
            if (enableObstacle && obstacles != null)
            {
                foreach (var obs in obstacles)
                {
                    if (obs == null) continue;
                    obs.CheckAndReveal(rectTransform.position, object1Radius);
                    obs.CheckAndReveal(object2.position, object2Radius);
                }
            }
            // 胜利检测
            if (!isWin && target != null)
            {
                float distToTarget = Vector3.Distance(rectTransform.position, target.position);
                if (distToTarget <= offestRedius)
                {
                    GetTarget();
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    void UpdateImageSlider()
    {
        // 计算进度比例（0~1）
        float progress = currentValue / maxValue;
        progress = Mathf.Clamp01(progress);

        // 根据 startPoint 和 endPoint 进行线性插值（同时改变 X 和 Y）
        Vector3 newPos = Vector3.Lerp(startPoint.position, endPoint.position, progress);
        // 保持 Z 轴与 startPoint 一致（通常为 0）
        newPos.z = startPoint.position.z;
        ImageSlider.position = newPos;
    }

    private void UpdateObject2Position()
    {
        if (object2 == null) return;
        if (isWin) return; 
        float axisY = (symmetryAxis != null) ? symmetryAxis.position.y : symmetryY;

        Vector3 pos1 = rectTransform.position;
        float newX = pos1.x;
        float newY = 2f * axisY - pos1.y;
        object2.position = new Vector3(newX, newY, pos1.z);
    }
    //到达终点
    public void GetTarget()
    {
        if (isWin) return;
        isWin = true;
        isDragging = false; // 立即停止拖拽
        
        //将物体1和物体2丝滑移动到终点位置
        rectTransform.DOMove(target.position, 0.3f).SetEase(Ease.OutQuad);
        if (object2 != null)
            object2.DOMove(target.position, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                OnGrowthComplete(); // 移动完成后切换关卡
            });
        else
        {
            OnGrowthComplete();
        }
    }

    ////进入下一关的转场
    //private void OnGrowthComplete()
    //{
    //    if (hasTriggeredSwitch) return;
    //    hasTriggeredSwitch = true;
    //    if (Scene4Manage.Instance != null)
    //    {
    //        Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay);
    //    }
    //    else
    //    {
    //        Debug.LogError("Scene4Manage.Instance 不存在，请确保场景中有 Scene4Manage 组件");
    //    }
    //}
    //进入下一关
    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");

        if (Scene4Manage.Instance != null)
        {
            Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                if (Scene4Manage.Instance.level4Manage != null)
                {
                    Scene4Manage.Instance.level4Manage.BeginGame();
                }
                else
                {
                    Debug.LogError("level4Manage 未在 Scene4Manage 中赋值！");
                }
            });
        }
        else
        {
            Debug.LogError("Scene4Manage.Instance 不存在，请确保场景中有 Scene4Manage 组件");
        }
    }

    // 生成乐乐脚印（在物体1位置）
    private void SpawnLeleFootprint(Vector3 direction)
    {
        if (leleFoot_Prefab == null) return;

        Vector3 spawnPos = rectTransform.position;
        if (direction != Vector3.zero)
        {
            //计算垂直于移动方向的右侧向量 (y, -x)
            Vector3 right = new Vector3(direction.y, -direction.x, 0).normalized;
            spawnPos += right * lateralOffsetDistance * currentOffsetSign_lele;
            //翻转偏移方向（左右交替）
            currentOffsetSign_lele *= -1;
        }

        GameObject foot = Instantiate(leleFoot_Prefab, spawnPos, Quaternion.identity);
        foot.transform.SetParent(footFather.transform);

        foot.transform.localScale = Vector3.one;
        // 设置旋转：使脚印的默认向上方向（Vector3.up）转向移动方向
        if (direction != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            foot.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        Destroy(foot, duration);
    }

    // 生成爸爸/妈妈脚印（在物体2位置，按顺序：爸爸2个，妈妈2个，循环）
    private void SpawnParentFootprint(Vector3 direction)
    {
        if (dadFoot_Prefab == null || momFoot_Prefab == null) return;

        GameObject prefabToUse = (parentFootGroup == 0) ? dadFoot_Prefab : momFoot_Prefab;

        Vector3 spawnPos = object2.position;
        if (direction != Vector3.zero)
        {
            Vector3 right = new Vector3(direction.y, -direction.x, 0).normalized;
            spawnPos += right * lateralOffsetDistance * currentOffsetSign_parent;
            currentOffsetSign_parent *= -1;   // 每次生成都翻转，保证全局交替
        }
        Debug.Log(spawnPos);

        // 生成脚印
        GameObject foot = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
        foot.transform.SetParent(footFather.transform);
        foot.transform.localScale = Vector3.one;

        // 设置旋转
        if (direction != Vector3.zero)
        {
            float angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            foot.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        Destroy(foot, duration);

        // 更新计数和组
        parentFootCountInGroup++;
        if (parentFootCountInGroup >= 2)
        {
            // 切换组（爸爸<->妈妈）
            parentFootGroup = 1 - parentFootGroup;
            parentFootCountInGroup = 0;
        }
    }

    // 根据物品1位置计算物品2位置（与 UpdateObject2Position 逻辑一致）
    private Vector3 GetObject2Position(Vector3 pos1)
    {
        float axisY = (symmetryAxis != null) ? symmetryAxis.position.y : symmetryY;
        return new Vector3(pos1.x, 2f * axisY - pos1.y, pos1.z);
    }


    // 核心：将物品1位置推离所有障碍物（迭代5次）
    private Vector3 PushOutOfObstacles(Vector3 pos1, int iterations = 5)
    {
        for (int iter = 0; iter < iterations; iter++)
        {
            bool anyPush = false;
            Vector3 pos2 = GetObject2Position(pos1);
            foreach (var obs in obstacles)
            {
                if (obs == null) continue;
                // 处理物品1
                Vector3 push1 = PushOutSingle(pos1, obs, object1Radius);
                if (push1 != Vector3.zero)
                {
                    pos1 += push1;
                    anyPush = true;
                }
                // 重新计算物品2位置（因为物品1变了）
                pos2 = GetObject2Position(pos1);
                // 处理物品2：物品2的推离需要转化为物品1的移动
                Vector3 push2 = PushOutSingle(pos2, obs, object2Radius);
                if (push2 != Vector3.zero)
                {
                    // 物品2的移动方向与物品1相反（因为对称轴是水平线，Y轴镜像）
                    // 物品2的推离向量 (dx, dy) 对应物品1的推离向量为 (dx, -dy)
                    Vector3 push1From2 = new Vector3(push2.x, -push2.y, 0);
                    pos1 += push1From2;
                    anyPush = true;
                }
            }
            if (!anyPush) break;
        }
        return pos1;
    }
    // 单个物体推离单个障碍物（返回需要移动的向量）
    private Vector3 PushOutSingle(Vector3 objPos, Obstacle obs, float radius)
    {
        float dist = obs.SignedDistanceToBoundary(objPos, radius);
        if (dist >= 0) return Vector3.zero; // 未进入或刚好在边界

        // 进入深度为 -dist，需要向外推离 -dist 距离
        // 推离方向：从障碍物中心指向物体中心（对于圆形）或最近边法向（对于矩形）
        Vector3 direction;
        if (obs.shape == Obstacle.ShapeType.Circle)
        {
            direction = (objPos - obs.Center).normalized;
        }
        else // 矩形
        {
            // 找到矩形上离物体最近的点，方向为物体指向该点的反方向？更简单：直接用物体位置减去矩形中心，然后根据符号决定方向
            Vector3 localPos = objPos - obs.Center;
            Vector3 halfSize = new Vector3(obs.size.x * 0.5f, obs.size.y * 0.5f, 0);
            // 计算物体在矩形局部坐标中的溢出量
            float dx = Mathf.Abs(localPos.x) - halfSize.x;
            float dy = Mathf.Abs(localPos.y) - halfSize.y;
            if (dx > dy)
                direction = new Vector3(Mathf.Sign(localPos.x), 0, 0);
            else
                direction = new Vector3(0, Mathf.Sign(localPos.y), 0);
        }
        return direction * (-dist);
    }
   

    // 可选：在编辑器中绘制边界辅助线
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3((boundaryLeft + boundaryRight) / 2, (boundaryBottom + boundaryTop) / 2, 0);
        Vector3 size = new Vector3(boundaryRight - boundaryLeft, boundaryTop - boundaryBottom, 0);
        Gizmos.DrawWireCube(center, size);

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(target.position, offestRedius);
        }
    }
}