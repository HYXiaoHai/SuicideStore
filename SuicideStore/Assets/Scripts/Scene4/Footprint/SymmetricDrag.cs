using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SymmetricDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("物体引用")]
    public RectTransform object2;//物体2的RectTransform
    public RectTransform target;//终点的RectTransform
    public float offestRedius = 0.1f;//允许的过关偏移量
    [Header("图片进度条")]
    public Transform ImageSgil;//物品索引
    public Transform startPoint;//开始位置
    public Transform endPoint;//结束位置
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
    public float currentDistance;//当前移动的距离  
    public float duration;//显隐的时间
    private Vector3 lastObject2Position;  // 物体2的上一帧位置
    private Vector3 lastPosition;            // 上一帧位置（用于计算移动距离）
    private int parentFootGroup = 0;         // 0=爸爸组, 1=妈妈组
    private int parentFootCountInGroup = 0;  // 当前组已生成的数量（最多2）


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
        if (isWin) return;
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
            rectTransform.position = newPos;
            UpdateObject2Position();

            // 计算物体1的移动方向和距离
            Vector3 moveDir1 = (rectTransform.position - lastPosition).normalized;
            float delta1 = Vector3.Distance(rectTransform.position, lastPosition);

            // 计算物体2的移动方向和距离
            Vector3 moveDir2 = (object2.position - lastObject2Position).normalized;
            float delta2 = Vector3.Distance(object2.position, lastObject2Position);
            if (delta1 > 0)
            {
                currentDistance += delta1;
                // 当累积距离达到阈值时，生成脚印（可能一次移动触发多个脚印，用 while 循环）
                while (currentDistance >= needDistance && !isWin)
                {
                    currentDistance -= needDistance;
                    SpawnLeleFootprint(moveDir1);      // 传递物体1的移动方向
                    SpawnParentFootprint(moveDir2);    // 传递物体2的移动方向
                }
                lastPosition = rectTransform.position;
                lastObject2Position = object2.position;
            }

            //胜利检测
            if (!isWin && target != null)
            {
                float distToTarget = Vector3.Distance(rectTransform.position, target.position);
                if (distToTarget <= offestRedius)
                {
                    GetTarget(); //触发胜利
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
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

    //进入下一关的转场
    private void OnGrowthComplete()
    {
        if (hasTriggeredSwitch) return;
        hasTriggeredSwitch = true;
        if (Scene4Manage.Instance != null)
        {
            Scene4Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay);
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