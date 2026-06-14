using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

public class DragBall : MonoBehaviour
{
    public int id;
    public TMP_Text dragBallText;

    [Header("拖拽粒子效果")]
    public ParticleFollower[] particles;

    private Vector3 startPos;
    private Vector3 offset;
    private bool isCollected = false;      // 是否已收集（成功）
    private bool isDragging = false;
    private bool isReturning = false;      // 是否正在回到起点（移动动画中）
    private bool isInBoundary = false;

    private Collider2D mycollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        mycollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (dragBallText != null)
        {
            dragBallText.alpha = 1f;
            dragBallText.gameObject.SetActive(true);
        }

        if (particles != null)
        {
            foreach (var p in particles)
            {
                if (p != null) p.gameObject.SetActive(false);
            }
        }
    }

    private void ShowParticles()
    {
        if (particles == null) return;
        foreach (var p in particles)
            p.StartFollowing(transform);
    }

    private void HideParticles()
    {
        if (particles == null) return;
        foreach (var p in particles)
            p.StartGatherSequence(); // 立即聚集
    }

    void OnMouseDown()
    {
        if (GameManage.Instance.isSetting) return;
        if (isCollected || isReturning) return;

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (dragBallText != null)
            dragBallText.DOFade(0f, 0.2f);

        ShowParticles();
    }

    void OnMouseDrag()
    {
        if (GameManage.Instance.isSetting) return;
        if (isCollected || !isDragging || isReturning) return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
    }

    void OnMouseUp()
    {
        if (isCollected || isReturning) return;
        isDragging = false;

        if (isInBoundary)
        {
            Collect();
        }
        else
        {
            ReturnToStart();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boundary")) isInBoundary = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Boundary")) isInBoundary = false;
    }

    // 成功收集
    private void Collect()
    {
        if (isCollected) return;
        isCollected = true;
        isDragging = false;

        DragBallManager.Instance.OnBallCollected(id);
        Vector3 targetPos = DragBallManager.Instance.GetGraffitiPosition(id);
        if (targetPos == Vector3.zero) targetPos = transform.position;
        if (mycollider != null) mycollider.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
        if (dragBallText != null)
        {
            dragBallText.DOFade(0f, 0.2f).OnComplete(() => dragBallText.gameObject.SetActive(false));
        }

        transform.DOMove(targetPos, 0.4f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            if (spriteRenderer != null)
                spriteRenderer.DOFade(0f, 0.2f);
            StartCoroutine(StartParticleGatherSequence(() => Destroy(gameObject)));
        });

        this.enabled = false;  // 成功后禁用，因为小球会被销毁
    }

    // 未成功：回到起点，文字恢复，粒子聚集消失（但小球不销毁）
    private void ReturnToStart()
    {
        if (isReturning) return;
        isReturning = true;
        isDragging = false;

        // 禁用碰撞体避免干扰
        if (mycollider != null) mycollider.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // 移动小球回起点
        transform.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 恢复文字渐显
            if (dragBallText != null && !dragBallText.gameObject.activeSelf)
                dragBallText.gameObject.SetActive(true);
            if (dragBallText != null)
                dragBallText.DOFade(1f, 0.3f);
            // 重新启用碰撞体
            if (mycollider != null) mycollider.enabled = true;
            // 重置状态标志
            isReturning = false;
        });

        // 粒子和渐隐逻辑：先聚集消失，但小球不销毁
        StartCoroutine(StartParticleGatherSequence(null));
    }

    private IEnumerator StartParticleGatherSequence(System.Action onComplete = null)
    {
        if (particles == null || particles.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float delayStep = 0.05f;
        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] != null)
                particles[i].StartGatherSequence(delayStep * i);
        }
        float totalWait = delayStep * (particles.Length - 1) + 0.3f;
        yield return new WaitForSeconds(totalWait);
        onComplete?.Invoke();
    }
}