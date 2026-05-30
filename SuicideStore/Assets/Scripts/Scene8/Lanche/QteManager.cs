using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // 如果使用URP，否则根据管线调整

public class QteManager : MonoBehaviour
{
    [System.Serializable]
    public class RoundData
    {
        public string roundName;
        public RectTransform targetUIPosition;
        public int requiredClicks = 5;
        public float timeLimit = 3f;
        public bool isUnwinnable = false;        // 必死关（成功/失败都导致破碎）
    }

    [Header("轮次配置")]
    public List<RoundData> rounds = new List<RoundData>();

    [Header("QTE 组件")]
    public SingleRoundQTE singleRoundQTE;

    [Header("玻璃裂纹动画")]
    public SpriteRenderer glassImage;
    public Sprite[] crackFrames;
    public int[] crackStageFrames = { 0, 3, 6, 9, 12 };
    public float crackAnimDuration = 0.5f;
    public float pauseAfterQTE = 1f;             // 成功后的停顿
    public float pauseAfterCrack = 0.3f;

    [Header("相机噪声 (Cinemachine)")]
    public CinemachineVirtualCamera virtualCamera;
    public float defaultNoiseAmplitude = 0.3f;
    public float defaultNoiseFrequency = 1.2f;
    public float failNoiseAmplitude = 1.8f;
    public float failNoiseFrequency = 3f;
    public float failShakeDuration = 0.3f;
    public float failShakeRecoverDuration = 0.5f;//下降所需时间
    public float failShakeRiseDuration = 0.1f;//上升所需时间

    [Header("暗角效果 (Vignette)")]
    public Volume globalVolume;                  // 场景中的Global Volume
    public float failVignetteIntensity = 0.6f;
    public float vignetteFadeOutDuration = 0.5f;
    public Color failVignetteColor = Color.red;   // 仅当Vignette支持颜色时有效

    [Header("事件回调")]
    public UnityEvent onAllRoundsCompleted;
    public UnityEvent onUnwinnableFailed;

    private int currentRoundIndex = 0;
    private bool isExecuting = false;
    private int currentCrackFrame = 0;

    private CinemachineBasicMultiChannelPerlin cameraNoise;
    private Coroutine failShakeCoroutine = null;
    private Vignette vignette;
    private float originalVignetteIntensity;

    private void Awake()
    {
        if (singleRoundQTE == null)
        {
            Debug.LogError("QteManager: 未绑定 SingleRoundQTE！");
            return;
        }

        singleRoundQTE.onQTESuccess.RemoveAllListeners();
        singleRoundQTE.onQTEFail.RemoveAllListeners();

        if (glassImage != null && crackFrames.Length > 0)
        {
            glassImage.sprite = crackFrames[0];
            currentCrackFrame = 0;
        }

        // 相机噪声
        if (virtualCamera != null)
        {
            cameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (cameraNoise != null)
            {
                cameraNoise.m_AmplitudeGain = defaultNoiseAmplitude;
                cameraNoise.m_FrequencyGain = defaultNoiseFrequency;
            }
        }

        // 暗角效果
        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            originalVignetteIntensity = vignette.intensity.value;
            vignette.intensity.value = 0f;          // 默认无暗角
            // 如果支持颜色，可在此设置颜色
            if (vignette.color != null)
                vignette.color.value = failVignetteColor;
        }
    }

    private void Start()
    {
         StartQTESequence(); // 外部调用
    }

    public void StartQTESequence()
    {
        if (isExecuting) return;
        StartCoroutine(RunFullSequence());
    }

    private IEnumerator RunFullSequence()
    {
        isExecuting = true;
        currentRoundIndex = 0;

        SetNoiseToDefault();
        ResetVignette();

        // 第1轮QTE前播放裂纹动画
        if (crackStageFrames.Length >= 2)
        {
            int targetFrame = crackStageFrames[1];
            yield return StartCoroutine(PlayCrackAnimation(targetFrame));
        }

        while (currentRoundIndex < rounds.Count)
        {
            RoundData round = rounds[currentRoundIndex];
            bool isLastRound = (currentRoundIndex == rounds.Count - 1);

            singleRoundQTE.requiredClicks = round.requiredClicks;
            singleRoundQTE.timeLimit = round.timeLimit;
            singleRoundQTE.SetPosition(round.targetUIPosition);

            bool roundSuccess = false;
            bool roundFinished = false;

            UnityAction successListener = null;
            UnityAction failListener = null;
            successListener = () => { roundSuccess = true; roundFinished = true; };
            failListener = () => { roundSuccess = false; roundFinished = true; };

            singleRoundQTE.onQTESuccess.AddListener(successListener);
            singleRoundQTE.onQTEFail.AddListener(failListener);

            singleRoundQTE.ShowAndStart();

            yield return new WaitUntil(() => roundFinished);

            singleRoundQTE.onQTESuccess.RemoveListener(successListener);
            singleRoundQTE.onQTEFail.RemoveListener(failListener);

            // 先隐藏面板（成功和失败都需要）
            bool hidden = false;
            singleRoundQTE.Hide(() => hidden = true);
            yield return new WaitUntil(() => hidden);

            // 处理成功/失败
            if (roundSuccess)
            {
                yield return new WaitForSeconds(pauseAfterQTE);
            }
            else
            {
                // 失败：触发震动 + 暗角效果
                TriggerFailShake();
                TriggerFailVignette();

                // 如果是必死关且是最后一轮，直接破碎结局
                if (isLastRound && round.isUnwinnable)
                {
                    yield return StartCoroutine(PlayBreakAndEnd());
                    yield break;
                }
            }

            // 如果不是最后一轮，播放下一段裂纹动画
            if (!isLastRound)
            {
                int nextStageIndex = currentRoundIndex + 2;
                if (nextStageIndex < crackStageFrames.Length)
                {
                    int targetFrame = crackStageFrames[nextStageIndex];
                    yield return StartCoroutine(PlayCrackAnimation(targetFrame));
                }
                else
                {
                    Debug.LogWarning("裂纹阶段配置不足");
                }
            }
            else
            {
                if (round.isUnwinnable)
                {
                    // 必死关：最后一步已经完成（成功或失败都走到这里），触发破碎
                    yield return StartCoroutine(PlayBreakAndEnd());
                    yield break;
                }
                else
                {
                    // 非必死最后一轮（理论上不会有）正常结束
                    int finalStage = crackStageFrames[crackStageFrames.Length - 1];
                    yield return StartCoroutine(PlayCrackAnimation(finalStage));
                    onAllRoundsCompleted?.Invoke();
                    isExecuting = false;
                    yield break;
                }
            }

            currentRoundIndex++;
        }

        onAllRoundsCompleted?.Invoke();
        isExecuting = false;
    }

    // 播放裂纹推进动画（保持原有逻辑）
    private IEnumerator PlayCrackAnimation(int targetFrameIndex)
    {
        if (glassImage == null || crackFrames.Length == 0)
            yield break;

        targetFrameIndex = Mathf.Clamp(targetFrameIndex, 0, crackFrames.Length - 1);
        if (currentCrackFrame >= targetFrameIndex)
            yield break;

        int startFrame = currentCrackFrame;
        int frameRange = targetFrameIndex - startFrame;
        float elapsed = 0f;

        while (elapsed < crackAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crackAnimDuration);
            int frameIdx = startFrame + Mathf.FloorToInt(t * frameRange);
            frameIdx = Mathf.Clamp(frameIdx, startFrame, targetFrameIndex);
            if (frameIdx != currentCrackFrame)
            {
                currentCrackFrame = frameIdx;
                glassImage.sprite = crackFrames[currentCrackFrame];
            }
            yield return null;
        }

        currentCrackFrame = targetFrameIndex;
        glassImage.sprite = crackFrames[currentCrackFrame];
        yield return new WaitForSeconds(pauseAfterCrack);
    }

    // 最终破碎动画（完全破碎）
    private IEnumerator PlayBreakAndEnd()
    {
        if (glassImage == null || crackFrames.Length == 0)
        {
            onUnwinnableFailed?.Invoke();
            isExecuting = false;
            yield break;
        }

        int lastFrame = crackFrames.Length - 1;
        if (currentCrackFrame < lastFrame)
        {
            float quickDuration = 0.4f;
            float elapsed = 0f;
            int startFrame = currentCrackFrame;
            int range = lastFrame - startFrame;

            while (elapsed < quickDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / quickDuration);
                int frameIdx = startFrame + Mathf.FloorToInt(t * range);
                frameIdx = Mathf.Clamp(frameIdx, startFrame, lastFrame);
                if (frameIdx != currentCrackFrame)
                {
                    currentCrackFrame = frameIdx;
                    glassImage.sprite = crackFrames[currentCrackFrame];
                }
                yield return null;
            }
        }

        currentCrackFrame = lastFrame;
        glassImage.sprite = crackFrames[currentCrackFrame];
        SetNoiseToDefault();
        ResetVignette();

        yield return new WaitForSeconds(0.3f);
        onUnwinnableFailed?.Invoke();
        isExecuting = false;
    }

    // ---------- 相机噪声 ----------
    private void SetNoiseToDefault()
    {
        if (cameraNoise != null)
        {
            cameraNoise.m_AmplitudeGain = defaultNoiseAmplitude;
            cameraNoise.m_FrequencyGain = defaultNoiseFrequency;
        }
    }

    private void TriggerFailShake()
    {
        if (cameraNoise == null) return;
        if (failShakeCoroutine != null)
            StopCoroutine(failShakeCoroutine);
        failShakeCoroutine = StartCoroutine(FailShakeRoutine());
    }

    private IEnumerator FailShakeRoutine()
    {
        if (cameraNoise == null) yield break;

        // 记录起始值（可能是默认值或当前值）
        float startAmp = cameraNoise.m_AmplitudeGain;
        float startFreq = cameraNoise.m_FrequencyGain;

        // 快速上升到峰值
        float riseElapsed = 0f;
        while (riseElapsed < failShakeRiseDuration)
        {
            riseElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(riseElapsed / failShakeRiseDuration);
            cameraNoise.m_AmplitudeGain = Mathf.Lerp(startAmp, failNoiseAmplitude, t);
            cameraNoise.m_FrequencyGain = Mathf.Lerp(startFreq, failNoiseFrequency, t);
            yield return null;
        }
        // 确保到达峰值
        cameraNoise.m_AmplitudeGain = failNoiseAmplitude;
        cameraNoise.m_FrequencyGain = failNoiseFrequency;

        // 保持峰值一小段时间
        yield return new WaitForSeconds(failShakeDuration);

        // 平滑恢复到默认值
        float recoverElapsed = 0f;
        startAmp = cameraNoise.m_AmplitudeGain;
        startFreq = cameraNoise.m_FrequencyGain;
        while (recoverElapsed < failShakeRecoverDuration)
        {
            recoverElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(recoverElapsed / failShakeRecoverDuration);
            cameraNoise.m_AmplitudeGain = Mathf.Lerp(startAmp, defaultNoiseAmplitude, t);
            cameraNoise.m_FrequencyGain = Mathf.Lerp(startFreq, defaultNoiseFrequency, t);
            yield return null;
        }

        cameraNoise.m_AmplitudeGain = defaultNoiseAmplitude;
        cameraNoise.m_FrequencyGain = defaultNoiseFrequency;

        failShakeCoroutine = null;
    }

    // ---------- 暗角效果 ----------
    private void ResetVignette()
    {
        if (vignette != null)
            vignette.intensity.value = 0f;
    }

    private void TriggerFailVignette()
    {
        if (vignette == null) return;
        // 停止之前的动画
        DOTween.Kill(vignette);

        Sequence vignetteSeq = DOTween.Sequence();
        // 第一阶段：从当前值（假设为0）上升到峰值
        vignetteSeq.Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, failVignetteIntensity, 0.2f)
            .SetEase(Ease.OutCubic));
        // 第二阶段：等待峰值停留 0.1 秒（可选）
        vignetteSeq.AppendInterval(0.1f);
        // 第三阶段：下降回 0
        vignetteSeq.Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, vignetteFadeOutDuration)
            .SetEase(Ease.OutSine));

        vignetteSeq.Play();
    }
}