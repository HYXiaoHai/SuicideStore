using UnityEngine;
using System.Collections;

public class FaceMoveController : MonoBehaviour
{
    [Header("移动速度范围")]
    public float minSpeed = 80f;
    public float maxSpeed = 200f;

    [Header("方向变化间隔范围")]
    public float minChangeInterval = 1f;
    public float maxChangeInterval = 3f;

    [Header("轨道引用")]
    public RectTransform trackRect;

    [Header("默认边界（找不到轨道时使用）")]
    public float defaultLeftLimit = -300f;
    public float defaultRightLimit = 300f;

    private RectTransform faceRect;
    private float leftLimit;
    private float rightLimit;
    private float currentSpeed;
    private int moveDirection = 1;
    private bool isInitialized = false;

    void Start()
    {
        faceRect = GetComponent<RectTransform>();
        
        Debug.Log("FaceMoveController: Start() 开始初始化");
        Debug.Log($"FaceTarget 初始位置: {faceRect.anchoredPosition}");
        
        // 尝试获取轨道引用
        if (trackRect == null)
        {
            GameObject trackObj = GameObject.Find("BgTrack");
            if (trackObj != null)
            {
                trackRect = trackObj.GetComponent<RectTransform>();
                Debug.Log("FaceMoveController: 找到 BgTrack");
            }
            else
            {
                Debug.LogWarning("FaceMoveController: 找不到 BgTrack，使用默认边界！请确保物体名为 'BgTrack' 或在 Inspector 中手动赋值。");
                leftLimit = defaultLeftLimit;
                rightLimit = defaultRightLimit;
            }
        }
        
        // 如果找到了轨道，计算边界
        if (trackRect != null)
        {
            float trackHalfWidth = trackRect.sizeDelta.x / 2;
            float faceHalfWidth = faceRect.sizeDelta.x / 2;
            leftLimit = -trackHalfWidth + faceHalfWidth;
            rightLimit = trackHalfWidth - faceHalfWidth;
            Debug.Log($"FaceMoveController: 轨道宽度: {trackRect.sizeDelta.x}, 笑脸宽度: {faceRect.sizeDelta.x}");
        }
        
        // 初始化速度和方向
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        moveDirection = Random.value > 0.5f ? 1 : -1;
        isInitialized = true;
        
        Debug.Log($"FaceMoveController: 初始化完成！边界: [{leftLimit}, {rightLimit}], 方向: {moveDirection}, 速度: {currentSpeed}");
        
        // 开始随机改变方向的协程
        StartCoroutine(RandomChangeDirection());
    }

    void Update()
    {
        if (isInitialized)
        {
            MoveFace();
        }
    }

    void MoveFace()
    {
        Vector2 pos = faceRect.anchoredPosition;
        float oldX = pos.x;
        
        pos.x += moveDirection * currentSpeed * Time.deltaTime;
        
        // 边界检测和反弹
        if (pos.x >= rightLimit)
        {
            pos.x = rightLimit;
            moveDirection = -1;
            Debug.Log($"FaceMoveController: 碰到右边界，反弹！");
        }
        else if (pos.x <= leftLimit)
        {
            pos.x = leftLimit;
            moveDirection = 1;
            Debug.Log($"FaceMoveController: 碰到左边界，反弹！");
        }
        
        faceRect.anchoredPosition = pos;
        
        // 每帧输出一次位置（调试用，正式可以注释掉）
        // Debug.Log($"FaceTarget 位置: {pos.x}");
    }

    IEnumerator RandomChangeDirection()
    {
        while (true)
        {
            float randomInterval = Random.Range(minChangeInterval, maxChangeInterval);
            yield return new WaitForSeconds(randomInterval);
            
            if (isInitialized)
            {
                if (Random.value > 0.5f)
                {
                    moveDirection *= -1;
                }
                
                currentSpeed = Random.Range(minSpeed, maxSpeed);
                
                Debug.Log($"FaceMoveController: 随机改变！方向: {moveDirection}, 速度: {currentSpeed}");
            }
        }
    }
}
