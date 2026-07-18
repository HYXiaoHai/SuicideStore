using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

public class DragBall : MonoBehaviour
{
    public int id;
    public TMP_Text dragBallText;

    [Header("拖拽位置提醒")]
    public SpriteRenderer dragBallTargetSprite; // 目标位置提示

    [Header("拖拽粒子效果")]
    public ParticleFollower[] particles;

    private Vector3 startPos;
    private Vector3 offset;
    private bool isCollected = false;
    private bool isDragging = false;
    private bool isReturning = false;
    private bool isInBoundary = false;

    private Collider2D mycollider;
    private SpriteRenderer spriteRenderer;

    // 脉冲动画相关
    private Tween pulseTween;

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

        // 初始化引导 Sprite：透明，隐藏
        if (dragBallTargetSprite != null)
        {
            Color c = dragBallTargetSprite.color;
            c.a = 0f;
            dragBallTargetSprite.color = c;
            dragBallTargetSprite.gameObject.SetActive(false);
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
            p.StartGatherSequence();
    }

    // ------------------- 引导 Sprite 控制 -------------------
    private void ShowTargetGuide()
    {
        if (dragBallTargetSprite == null) return;
        // 停止旧动画
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        dragBallTargetSprite.gameObject.SetActive(true);
        Color c = dragBallTargetSprite.color;
        c.a = 0f;
        dragBallTargetSprite.color = c;
        // 渐显
        DOTween.To(() => dragBallTargetSprite.color.a,
                   x => { Color cc = dragBallTargetSprite.color; cc.a = x; dragBallTargetSprite.color = cc; },
                   0.6f, 0.2f).SetEase(Ease.OutQuad);

        // 循环脉冲：在 0.2 ~ 0.47 之间来回闪烁（周期 0.6s）
        pulseTween = DOTween.To(() => dragBallTargetSprite.color.a,
                               x => { Color cc = dragBallTargetSprite.color; cc.a = x; dragBallTargetSprite.color = cc; },
                               0.5f, 0.6f)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);
    }

    private void HideTargetGuide()
    {
        if (dragBallTargetSprite == null) return;
        // 停止脉冲
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        pulseTween = null;
        // 渐隐
        DOTween.To(() => dragBallTargetSprite.color.a,
                   x => { Color cc = dragBallTargetSprite.color; cc.a = x; dragBallTargetSprite.color = cc; },
                   0f, 0.2f).SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   if (dragBallTargetSprite != null)
                       dragBallTargetSprite.gameObject.SetActive(false);
               });
    }
    // -------------------------------------------------

    void OnMouseDown()
    {
        if (GameManage.Instance.isSetting) return;
        if (isCollected || isReturning) return;

        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (dragBallText != null)
            dragBallText.DOFade(0f, 0.2f);

        ShowParticles();
        ShowTargetGuide(); // 拖拽开始时显示引导
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

        // 隐藏引导（无论成功还是失败）
        HideTargetGuide();

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

        this.enabled = false;
    }

    private void ReturnToStart()
    {
        if (isReturning) return;
        isReturning = true;
        isDragging = false;

        if (mycollider != null) mycollider.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        transform.DOMove(startPos, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (dragBallText != null && !dragBallText.gameObject.activeSelf)
                dragBallText.gameObject.SetActive(true);
            if (dragBallText != null)
                dragBallText.DOFade(1f, 0.3f);
            if (mycollider != null) mycollider.enabled = true;
            isReturning = false;
        });

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