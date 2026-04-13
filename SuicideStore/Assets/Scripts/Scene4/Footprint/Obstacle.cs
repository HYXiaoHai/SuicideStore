using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;   // 添加 DOTween 命名空间

public class Obstacle : MonoBehaviour
{
    public enum ShapeType { Circle, Rect }

    [Header("区域形状")]
    public ShapeType shape = ShapeType.Circle;

    [Header("圆形参数")]
    public float radius = 1f;

    [Header("矩形参数")]
    public Vector2 size = new Vector2(2f, 2f);

    [Header("显隐设置")]
    public bool appearOnce = true;
    public float appearAlpha = 1f;
    public float disappearAlpha = 0f;
    public float fadeDuration = 0.5f;   // 动画时长

    private SpriteRenderer spriteRenderer;
    private Image image;
    private bool hasAppeared = false;

    public Vector3 Center => transform.position;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        // 初始设置为完全透明（不带动画）
        SetAlphaImmediate(disappearAlpha);
    }

    // 供外部调用：检查物体是否进入区域，如果是则显现（带动画）
    public void CheckAndReveal(Vector3 objPos, float objRadius)
    {
        if (!enabled) return;
        if (appearOnce && hasAppeared) return;

        float dist = SignedDistanceToBoundary(objPos, objRadius);
        if (dist < 0)
        {
            FadeTo(appearAlpha);
            hasAppeared = true;
        }
    }

    // 淡入淡出动画
    private void FadeTo(float targetAlpha)
    {
        if (spriteRenderer != null)
        {
            // 如果 DOTween 动画正在运行，先杀死避免冲突
            spriteRenderer.DOKill();
            spriteRenderer.DOFade(targetAlpha, fadeDuration);
        }
        else if (image != null)
        {
            image.DOKill();
            image.DOFade(targetAlpha, fadeDuration);
        }
    }

    // 立即设置透明度（无动画）
    private void SetAlphaImmediate(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
        else if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }

    // 返回点到区域边界的有符号距离（负表示点位于区域内，并考虑了物体半径外扩）
    public float SignedDistanceToBoundary(Vector3 p, float objectRadius = 0f)
    {
        Vector3 localPos = p - transform.position;
        if (shape == ShapeType.Circle)
        {
            float distToCenter = localPos.magnitude;
            return distToCenter - (radius + objectRadius);
        }
        else // Rect
        {
            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;
            float dx = Mathf.Abs(localPos.x) - halfW;
            float dy = Mathf.Abs(localPos.y) - halfH;
            if (dx > 0 || dy > 0)
            {
                return Mathf.Max(dx, dy) - objectRadius;
            }
            else
            {
                return Mathf.Max(dx, dy) - objectRadius;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (shape == ShapeType.Circle)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0));
        }
    }
}