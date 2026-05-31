using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using DG.Tweening;   // 需要导入 DOTween

public class MemoryWalkManager : MonoBehaviour
{
    [Header("脚印序列（按行走顺序）")]
    public GameObject[] footprints;
    [Tooltip("是否开始时全部隐藏")]
    public bool hideOnStart = true;

    [Header("裂纹动画")]
    public SpriteRenderer crackRenderer;
    public Sprite[] crackFrames;
    public float baseCrackSpeed = 0.8f;
    public float maxCrackSpeed = 2.5f;
    public float minCrackSpeed = 0.2f;
    public int speedUpThreshold = 3;
    public int speedDownThreshold = 3;

    [Header("按键提示")]
    public SpriteRenderer ePrompt;
    [Tooltip("无操作多久后显示提示（秒）")]
    public float idleTimeToShowPrompt = 0.5f;
    [Tooltip("提示相对于相机跟踪点的偏移（例如上方）")]
    public Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    [Header("相机跟随")]
    public CinemachineVirtualCamera virtualCamera;
    public GameObject followObject;
    public Transform[] cameraPathPoints;
    public float cameraSmoothTime = 0.3f;

    [Header("事件")]
    public UnityEvent onComplete;    // 完全走完时调用

    private int currentFootprintIndex = 0;
    private int currentCrackFrame = 0;
    private float currentCrackSpeed;
    private float crackAccumulator = 0f;
    private bool isActive = false;
    private Vector3 cameraVelocity = Vector3.zero;

    // 按键提示相关
    private Coroutine idleCoroutine = null;
    private Tween promptFadeTween = null;
    private Tween promptScaleTween = null;

    void Start()
    {
        if (footprints == null || footprints.Length == 0)
        {
            Debug.LogError("未设置脚印数组！");
            return;
        }
        if (hideOnStart)
        {
            foreach (var fp in footprints)
                if (fp != null) fp.SetActive(false);
        }

        if (crackFrames.Length > 0)
        {
            crackRenderer.sprite = crackFrames[0];
            currentCrackFrame = 0;
        }
        currentCrackSpeed = baseCrackSpeed;
        isActive = false;

        // 初始化按键提示
        if (ePrompt != null)
        {
            ePrompt.gameObject.SetActive(false);
            ePrompt.transform.localScale = Vector3.one;
        }
    }

    public void StartGame()
    {
        // 重置状态
        currentFootprintIndex = 0;
        currentCrackFrame = 0;
        currentCrackSpeed = baseCrackSpeed;
        crackAccumulator = 0f;
        isActive = true;
        cameraVelocity = Vector3.zero;

        // 重置裂纹显示
        if (crackFrames.Length > 0)
            crackRenderer.sprite = crackFrames[0];

        // 重置所有脚印（隐藏）
        if (hideOnStart)
        {
            foreach (var fp in footprints)
                if (fp != null) fp.SetActive(false);
        }

        // 重置相机位置到第一个路径点
        if (virtualCamera != null && followObject != null && cameraPathPoints != null && cameraPathPoints.Length > 0)
        {
            followObject.transform.position = cameraPathPoints[0].position;
        }

        // 重置按键提示（隐藏）
        if (ePrompt != null)
        {
            ePrompt.gameObject.SetActive(false);
            ePrompt.transform.localScale = Vector3.one;
        }

        // 开始空闲检测
        ResetIdleTimer();

        Debug.Log("MemoryWalkManager: 游戏已开始");
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!isActive) return;

        // 输入：显示下一个脚印
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 立即隐藏提示
            HidePromptImmediately();
            ShowNextFootprint();
            // 重置空闲计时器
            ResetIdleTimer();
        }

        // 如果已经完成（所有脚印已显示），不再更新裂纹和相机
        if (currentFootprintIndex >= footprints.Length)
            return;

        // 裂纹自动推进
        crackAccumulator += Time.deltaTime * currentCrackSpeed;
        if (crackAccumulator >= 1f)
        {
            int steps = Mathf.FloorToInt(crackAccumulator);
            crackAccumulator -= steps;
            for (int i = 0; i < steps; i++)
            {
                if (currentCrackFrame < crackFrames.Length - 1)
                {
                    currentCrackFrame++;
                    crackRenderer.sprite = crackFrames[currentCrackFrame];
                }
                else
                {
                    // 裂纹完全破碎，游戏结束（此处可根据需要触发失败事件）
                    GameOver();
                    return;
                }
            }
        }

        // 动态调整裂纹速度
        int diff = currentFootprintIndex - currentCrackFrame;
        if (diff > 0)
        {
            float t = Mathf.Clamp01((float)diff / speedUpThreshold);
            currentCrackSpeed = Mathf.Lerp(baseCrackSpeed, maxCrackSpeed, t);
        }
        else if (diff < 0)
        {
            float t = Mathf.Clamp01((float)(-diff) / speedDownThreshold);
            currentCrackSpeed = Mathf.Lerp(baseCrackSpeed, minCrackSpeed, t);
        }
        else
        {
            currentCrackSpeed = baseCrackSpeed;
        }

        // 更新相机位置
        UpdateCamera();

        // 更新按键提示位置（使其跟随相机跟踪点）
        if (ePrompt != null && ePrompt.gameObject.activeSelf && followObject != null)
        {
            ePrompt.transform.position = followObject.transform.position + promptOffset;
        }
    }

    void ShowNextFootprint()
    {
        if (currentFootprintIndex >= footprints.Length)
        {
            Complete();
            return;
        }

        GameObject next = footprints[currentFootprintIndex];
        if (next != null) next.SetActive(true);
        currentFootprintIndex++;
    }

    void UpdateCamera()
    {
        if (virtualCamera == null || followObject == null) return;
        if (currentFootprintIndex >= footprints.Length) return; // 已完成，不再移动相机

        float progress = (float)currentFootprintIndex / footprints.Length;
        Vector3 targetPos = GetPositionOnCameraPath(progress);
        if (float.IsNaN(targetPos.x) || float.IsNaN(targetPos.y) || float.IsNaN(targetPos.z))
        {
            Debug.LogWarning("相机路径计算返回 NaN，跳过本帧");
            return;
        }
        followObject.transform.position = Vector3.SmoothDamp(followObject.transform.position, targetPos, ref cameraVelocity, cameraSmoothTime);
    }

    Vector3 GetPositionOnCameraPath(float t)
    {
        if (cameraPathPoints == null || cameraPathPoints.Length == 0)
            return followObject.transform.position;
        if (cameraPathPoints.Length == 1)
            return cameraPathPoints[0].position;

        float totalLength = 0f;
        for (int i = 0; i < cameraPathPoints.Length - 1; i++)
            totalLength += Vector3.Distance(cameraPathPoints[i].position, cameraPathPoints[i + 1].position);

        if (totalLength <= 0f) return cameraPathPoints[0].position;

        float targetDist = t * totalLength;
        float currentDist = 0f;
        for (int i = 0; i < cameraPathPoints.Length - 1; i++)
        {
            float segDist = Vector3.Distance(cameraPathPoints[i].position, cameraPathPoints[i + 1].position);
            if (targetDist <= currentDist + segDist)
            {
                float segT = (targetDist - currentDist) / segDist;
                return Vector3.Lerp(cameraPathPoints[i].position, cameraPathPoints[i + 1].position, segT);
            }
            currentDist += segDist;
        }
        return cameraPathPoints[cameraPathPoints.Length - 1].position;
    }

    void Complete()
    {
        if (!isActive) return;
        isActive = false;

        // 停止空闲检测
        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        // 隐藏提示
        if (ePrompt != null && ePrompt.gameObject.activeSelf)
        {
            ePrompt.DOFade(0f, 0.1f).OnComplete(() => ePrompt.gameObject.SetActive(false));
        }

        // 将相机移动到最后一个路径点
        if (cameraPathPoints != null && cameraPathPoints.Length > 0 && followObject != null)
        {
            followObject.transform.position = cameraPathPoints[cameraPathPoints.Length - 1].position;
        }

        onComplete?.Invoke();
        Debug.Log("Memory Walk: 成功走完");
    }

    private void GameOver()
    {
        if (!isActive) return;
        isActive = false;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        if (ePrompt != null && ePrompt.gameObject.activeSelf)
            ePrompt.DOFade(0f, 0.1f).OnComplete(() => ePrompt.gameObject.SetActive(false));

        Debug.Log("Memory Walk: 游戏结束（裂纹完全破碎）");
        // 可根据需要触发失败事件
    }

    // ---------- 按键提示逻辑 ----------
    private void ResetIdleTimer()
    {
        if (!isActive) return;
        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(IdleCheckCoroutine());
    }

    private System.Collections.IEnumerator IdleCheckCoroutine()
    {
        yield return new WaitForSeconds(idleTimeToShowPrompt);
        if (isActive && currentFootprintIndex < footprints.Length)
        {
            ShowPrompt();
        }
    }

    private void ShowPrompt()
    {
        if (ePrompt == null) return;
        if (ePrompt.gameObject.activeSelf) return;

        ePrompt.gameObject.SetActive(true);
        ePrompt.transform.localScale = Vector3.zero;

        promptFadeTween?.Kill();
        promptScaleTween?.Kill();
        promptFadeTween = ePrompt.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);
        promptScaleTween = ePrompt.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutElastic, 0.8f, 0.5f);
    }

    private void HidePromptImmediately()
    {
        if (ePrompt == null) return;
        if (!ePrompt.gameObject.activeSelf) return;

        promptFadeTween?.Kill();
        promptScaleTween?.Kill();
        ePrompt.DOFade(0f, 0.1f).OnComplete(() =>
        {
            if (ePrompt != null) ePrompt.gameObject.SetActive(false);
        });
        ePrompt.transform.DOScale(Vector3.one, 0.1f);
    }
}