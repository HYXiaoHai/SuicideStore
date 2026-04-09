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