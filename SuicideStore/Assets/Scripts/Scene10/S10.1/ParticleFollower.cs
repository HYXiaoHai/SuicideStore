using DG.Tweening;
using UnityEngine;

public class ParticleFollower : MonoBehaviour
{
    [Header("追踪设置")]
    public Transform target;
    public float followSpeed = 8f;
    public float spreadRadius = 0.8f;
    public float gatherSpeed = 5f;

    [Header("面向目标")]
    public bool alwaysFaceTarget = true;
    public float rotateSpeed = 360f;

    private SpriteRenderer spriteRenderer;
    private Vector3 targetOffset;
    private bool isActive = false;
    private bool isGathering = false;
    private Vector3 gatherTargetPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    void Update()
    {
        if (target == null || !isActive) return;

        Vector3 desiredPos = target.position + targetOffset;

        if (isGathering)
        {
            transform.position = Vector3.Lerp(transform.position, gatherTargetPos, gatherSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, gatherTargetPos) < 0.05f)
            {
                gameObject.SetActive(false);
                isActive = false;
                isGathering = false;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        }

        if (alwaysFaceTarget && !isGathering)
        {
            Vector3 direction = target.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }
    }

    public void StartFollowing(Transform targetObj)
    {
        target = targetObj;
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        targetOffset = (Vector3)randomDir * spreadRadius;
        gatherTargetPos = targetObj.position;
        transform.position = targetObj.position;
        spriteRenderer.DOFade(1f, 0.1f);
        isActive = true;
        isGathering = false;
        gameObject.SetActive(true);

        if (alwaysFaceTarget)
        {
            Vector3 dir = target.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void StartGatherSequence(float delay = 0f)
    {
        if (!isActive) return;
        if (delay > 0)
            DOVirtual.DelayedCall(delay, () => GatherAndHide());
        else
            GatherAndHide();
    }

    private void GatherAndHide()
    {
        if (!isActive) return;
        isGathering = true;
        gatherTargetPos = target.position;
        spriteRenderer.DOFade(0f, 0.2f);
    }
}