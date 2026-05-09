using DG.Tweening;
using TMPro;
using UnityEngine;

public class DragBall : MonoBehaviour
{
    public int id;                     // 唯一ID
    public TMP_Text dragBallText;      // 对应的UI文本

    private Vector3 startPos;          // 起始位置
    private Vector3 offset;            // 拖拽偏移量
    private bool isCollected = false;  // 是否已收集
    private bool isDragging = false;
    private bool isReturning = false;  // 是否正在回位动画中
    private bool isInBoundary = false; // 当前是否在右侧区域内

    private Collider2D collider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;

        // 刚体设为运动学，避免物理影响
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        collider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 确保文字初始可见
        if (dragBallText != null)
        {
            dragBallText.alpha = 1f;
            dragBallText.gameObject.SetActive(true);
        }
    }

    void OnMouseDown()
    {
        if (isCollected || isReturning) return; // 已收集或正在回位时不可拖拽

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 文字渐隐
        if (dragBallText != null)
        {
            dragBallText.DOFade(0f, 0.2f);
        }
    }

    void OnMouseDrag()
    {
        if (isCollected || !isDragging || isReturning) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
    }

    void OnMouseUp()
    {
        if (isCollected || isReturning) return;
        isDragging = false;

        // 抬起时检测是否在右侧区域内
        if (isInBoundary)
        {
            Collect(); // 收集小球
        }
        else
        {
            // 未在区域内：移回起点，并让文字渐显
            ReturnToStart();
        }
    }

    // 检测进入右侧区域（tag = "Boundary"）
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boundary"))
        {
            isInBoundary = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boundary"))
        {
            isInBoundary = false;
        }
    }

    // 收集小球
    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        isDragging = false;

        // 通知管理器
        DragBallManager.Instance.OnBallCollected();

        // 处理文字：淡出并禁用
        if (dragBallText != null)
        {
            dragBallText.DOFade(0f, 0.2f).OnComplete(() =>
            {
                dragBallText.gameObject.SetActive(false);
            });
        }

        // 小球自身：例如淡出并销毁（也可直接销毁）
        // 为了平滑，先禁用碰撞体和拖拽，再执行缩放/淡出动画
        collider.enabled = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, 0.2f).OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }

        // 禁用脚本，避免残留事件
        this.enabled = false;
    }

    // 回到起点并恢复文字
    private void ReturnToStart()
    {
        if (isReturning) return;
        isReturning = true;
        isDragging = false;

        // 移动小球回起始点
        transform.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            isReturning = false;
        });

        // 文字渐显恢复
        if (dragBallText != null)
        {
            dragBallText.DOFade(1f, 0.3f);
        }
    }
}