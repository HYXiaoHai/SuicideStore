using UnityEngine;

public class IdentifyBarController : MonoBehaviour
{
    [Header("移动参数")]
    public float normalSpeed = 400f;
    public float boostSpeed = 600f;

    private RectTransform barRect;
    private float leftLimit;
    private float rightLimit;

    [Header("轨道引用")]
    public RectTransform trackRect;

    void Start()
    {
        barRect = GetComponent<RectTransform>();
        
        if (trackRect == null)
        {
            GameObject trackObj = GameObject.Find("BgTrack");
            if (trackObj != null)
            {
                trackRect = trackObj.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError("IdentifyBarController: 找不到 BgTrack 物体！请在Inspector中手动赋值。");
                return;
            }
        }
        
        float trackHalfWidth = trackRect.sizeDelta.x / 2;
        float barHalfWidth = barRect.sizeDelta.x / 2;
        leftLimit = -trackHalfWidth + barHalfWidth;
        rightLimit = trackHalfWidth - barHalfWidth;
        
        Debug.Log($"识别条边界: left={leftLimit}, right={rightLimit}");
    }

    void Update()
    {
        float currentSpeed = normalSpeed;
        bool isBoost = Input.GetKey(KeyCode.Space) && Input.GetMouseButton(0);
        
        if (isBoost)
        {
            currentSpeed = boostSpeed;
        }

        if (Input.GetMouseButton(0))
        {
            MoveBar(1, currentSpeed);
        }
        else
        {
            MoveBar(-1, normalSpeed);
        }
    }

    void MoveBar(int dir, float speed)
    {
        Vector2 pos = barRect.anchoredPosition;
        pos.x += dir * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        barRect.anchoredPosition = pos;
    }
}
