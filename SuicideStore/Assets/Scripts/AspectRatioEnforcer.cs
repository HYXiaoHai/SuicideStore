using UnityEngine;

//屏幕自适应

[RequireComponent(typeof(Camera))]
public class ViewportLetterboxOnly : MonoBehaviour
{
    [Header("目标宽高比 (例如 16:9 = 1.777...)")]
    public float targetAspect = 16.0f / 9.0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        ApplyViewport();
    }

#if UNITY_EDITOR
    void Update()
    {
        ApplyViewport(); // 方便在编辑器 Game 视图中实时预览
    }
#endif

    void ApplyViewport()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        Rect rect = cam.rect;

        if (currentAspect < targetAspect)
        {
            //Debug.Log("切换成16：10");
            // 屏幕比16:10、4:3，上下加黑边
            float scale = currentAspect / targetAspect; // 例如 1.6 / 1.777 = 0.9
            rect.x = 0;
            rect.y = (1 - scale) * 0.5f; // 垂直居中
            rect.width = 1;
            rect.height = scale;
        }
        else
        {
            Debug.Log("切换成16：9");
            // 屏幕比刚好 16:9，全屏显示
            rect.x = 0;
            rect.y = 0;
            rect.width = 1;
            rect.height = 1;
        }

        cam.rect = rect;
    }
}