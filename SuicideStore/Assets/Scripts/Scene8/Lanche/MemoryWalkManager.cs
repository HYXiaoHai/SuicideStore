using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

public class MemoryWalkManager : MonoBehaviour
{
    [Header("脚印序列（按行走顺序）")]
    public GameObject[] footprints;

    [Header("裂纹动画")]
    public SpriteRenderer crackRenderer;
    public Sprite[] crackFrames;                     // 22帧
    [Tooltip("每个脚印显示后裂纹应该到达的帧索引（0~21），数组长度必须等于脚印数量")]
    public int[] footprintToCrackFrame;              // 长度 = 26

    [Header("相机路径点")]
    public Transform[] cameraPathPoints;             // 15个路径点
    [Tooltip("每个脚印显示后相机应该跟随到的路径点索引（0~14），数组长度等于脚印数量")]
    public int[] footprintToPathPointIndex;          // 长度 = 26

    [Header("相机平滑跟随")]
    public CinemachineVirtualCamera virtualCamera;
    public GameObject followObject;                  // 实际跟随的物体（空物体）
    public float cameraSmoothTime = 0.3f;

    [Header("按键提示")]
    public SpriteRenderer ePrompt;
    public float idleTimeToShowPrompt = 0.5f;
    public Vector3 promptOffset = new Vector3(0, 1.5f, 0);

    [Header("事件")]
    public UnityEvent onComplete;

    // 运行时状态
    private int currentFootprintIndex = 0;
    private int currentCrackFrame = 0;
    private bool isActive = false;
    private Vector3 cameraVelocity = Vector3.zero;

    // 提示相关
    private Coroutine idleCoroutine = null;
    private Tween promptFadeTween = null;
    private Tween promptScaleTween = null;

    void Awake()
    {
        // 检查配置完整性
        if (footprints == null || footprints.Length == 0)
            Debug.LogError("未设置脚印数组！");
        if (crackFrames == null || crackFrames.Length == 0)
            Debug.LogError("未设置裂纹序列帧！");
        if (footprintToCrackFrame == null || footprintToCrackFrame.Length != footprints.Length)
            Debug.LogError($"footprintToCrackFrame 长度应为 {footprints.Length}，请检查！");
        if (footprintToPathPointIndex == null || footprintToPathPointIndex.Length != footprints.Length)
            Debug.LogError($"footprintToPathPointIndex 长度应为 {footprints.Length}，请检查！");
        if (cameraPathPoints == null || cameraPathPoints.Length == 0)
            Debug.LogError("未设置相机路径点！");
    }

    void Start()
    {
        // 初始隐藏所有脚印
        if (footprints != null)
        {
            foreach (var fp in footprints)
                if (fp != null) fp.SetActive(false);
        }

        // 初始裂纹第0帧
        if (crackFrames.Length > 0)
        {
            crackRenderer.sprite = crackFrames[0];
            currentCrackFrame = 0;
        }

        // 初始隐藏按键提示
        if (ePrompt != null)
        {
            ePrompt.gameObject.SetActive(false);
            ePrompt.transform.localScale = Vector3.one;
        }

        isActive = false;
    }

    public void StartGame()
    {
        // 重置状态
        currentFootprintIndex = 0;
        currentCrackFrame = 0;
        if (crackFrames.Length > 0)
            crackRenderer.sprite = crackFrames[0];
        isActive = true;
        cameraVelocity = Vector3.zero;

        // 重置所有脚印（隐藏）
        foreach (var fp in footprints)
            if (fp != null) fp.SetActive(false);

        // 重置相机位置到第一个路径点
        if (cameraPathPoints != null && cameraPathPoints.Length > 0 && followObject != null)
            followObject.transform.position = cameraPathPoints[0].position;

        // 重置按键提示
        if (ePrompt != null && ePrompt.gameObject.activeSelf)
        {
            ePrompt.DOFade(0f, 0f);
            ePrompt.gameObject.SetActive(false);
        }

        ResetIdleTimer();
        Debug.Log("MemoryWalkManager: 游戏已开始");
    }

    void Update()
    {
        if (!isActive) return;

        // 输入：显示下一个脚印
        if (Input.GetKeyDown(KeyCode.E))
        {
            HidePromptImmediately();
            ShowNextFootprint();
            ResetIdleTimer();
        }

        // 如果已完成，不再更新
        if (currentFootprintIndex >= footprints.Length)
            return;

        // 更新相机（根据当前脚印索引对应的路径点）
        UpdateCamera();

        // 更新按键提示位置
        if (ePrompt != null && ePrompt.gameObject.activeSelf && followObject != null)
            ePrompt.transform.position = followObject.transform.position + promptOffset;
    }

    void ShowNextFootprint()
    {
        if (currentFootprintIndex >= footprints.Length)
        {
            Complete();
            return;
        }

        // 显示脚印
        GameObject next = footprints[currentFootprintIndex];
        if (next != null) next.SetActive(true);

        // 根据映射设置裂纹帧
        int targetCrackFrame = footprintToCrackFrame[currentFootprintIndex];
        if (targetCrackFrame != currentCrackFrame && targetCrackFrame < crackFrames.Length)
        {
            // 平滑过渡（可选，也可直接跳转，这里直接跳转更准确）
            currentCrackFrame = targetCrackFrame;
            crackRenderer.sprite = crackFrames[currentCrackFrame];
        }

        currentFootprintIndex++;
    }

    void UpdateCamera()
    {
        if (virtualCamera == null || followObject == null) return;
        if (currentFootprintIndex >= footprints.Length) return;

        int targetPointIndex = footprintToPathPointIndex[currentFootprintIndex];
        if (targetPointIndex < 0 || targetPointIndex >= cameraPathPoints.Length)
        {
            Debug.LogWarning($"路径点索引 {targetPointIndex} 超出范围");
            return;
        }

        Vector3 targetPos = cameraPathPoints[targetPointIndex].position;
        followObject.transform.position = Vector3.SmoothDamp(followObject.transform.position, targetPos, ref cameraVelocity, cameraSmoothTime);
    }

    void Complete()
    {
        if (!isActive) return;
        isActive = false;

        if (idleCoroutine != null)
            StopCoroutine(idleCoroutine);

        if (ePrompt != null && ePrompt.gameObject.activeSelf)
            ePrompt.DOFade(0f, 0.1f).OnComplete(() => ePrompt.gameObject.SetActive(false));

        // 将相机移动到最后一个路径点
        if (cameraPathPoints != null && cameraPathPoints.Length > 0 && followObject != null)
            followObject.transform.position = cameraPathPoints[cameraPathPoints.Length - 1].position;

        onComplete?.Invoke();
        Debug.Log("Memory Walk: 成功走完");
    }

    // ---------- 按键提示 ----------
    private void ResetIdleTimer()
    {
        if (!isActive) return;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(IdleCheckCoroutine());
    }

    private IEnumerator IdleCheckCoroutine()
    {
        yield return new WaitForSeconds(idleTimeToShowPrompt);
        if (isActive && currentFootprintIndex < footprints.Length)
            ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (ePrompt == null) return;
        if (ePrompt.gameObject.activeSelf) return;

        ePrompt.gameObject.SetActive(true);
        ePrompt.color = new Color(ePrompt.color.r, ePrompt.color.g, ePrompt.color.b, 0f);
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

#if UNITY_EDITOR
    [ContextMenu("Setup Default Mapping")]
    void SetupDefaultMapping()
    {
        if (footprints == null || crackFrames == null || cameraPathPoints == null) return;

        int totalFootprints = footprints.Length;   // 应为26
        footprintToCrackFrame = new int[totalFootprints];
        footprintToPathPointIndex = new int[totalFootprints];

        for (int i = 0; i < totalFootprints; i++)
        {
            // 临时线性映射（实际请替换为正确值）
            footprintToCrackFrame[i] = Mathf.FloorToInt((float)i / totalFootprints * (crackFrames.Length - 1));
            footprintToPathPointIndex[i] = Mathf.FloorToInt((float)i / totalFootprints * (cameraPathPoints.Length - 1));
        }
        Debug.Log("已设置默认映射（占位），请根据实际需求在 Inspector 中精确调整数组！");
    }
#endif
}