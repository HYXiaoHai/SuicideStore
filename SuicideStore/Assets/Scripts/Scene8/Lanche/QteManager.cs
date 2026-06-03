using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class QteManager : MonoBehaviour
{
    [System.Serializable]
    public class RoundData
    {
        public string roundName;
        public RectTransform targetUIPosition;
        public int requiredClicks = 5;
        public float timeLimit = 3f;
        public bool isUnwinnable = false;
    }

    [Header("轮次配置")]
    public List<RoundData> rounds = new List<RoundData>();

    [Header("QTE 组件")]
    public SingleRoundQTE singleRoundQTE;
    [Header("手掌印")]
    public SpriteRenderer[] markRenderers;
    [Header("玻璃裂纹动画")]
    public SpriteRenderer glassImage;
    public Sprite[] crackFrames;
    public int[] crackStageFrames = { 0, 3, 6, 9, 12 };
    public float fastCrackDuration = 0.3f;        //快速炸裂时长（秒），更短更突然
    public float pauseAfterFastCrack = 0.1f;       //炸裂后的短停顿
    [Tooltip("QTE期间裂纹步进次数（每次增加1帧）")]
    public int crackStepsPerQTE = 3;               // 每轮QTE期间增加3帧
    [Tooltip("QTE期间每步间隔（秒）")]
    public float crackStepInterval = 0.6f;         // 每0.6秒增加一帧
    public float pauseAfterQTE = 1f;

    [Header("相机噪声")]
    public CinemachineVirtualCamera virtualCamera;
    public float defaultNoiseAmplitude = 0.3f;
    public float defaultNoiseFrequency = 1.2f;
    public float failNoiseAmplitude = 1.8f;
    public float failNoiseFrequency = 3f;
    public float failShakeDuration = 0.3f;
    public float failShakeRecoverDuration = 0.5f;
    public float failShakeRiseDuration = 0.1f;

    [Header("暗角效果")]
    public Volume globalVolume;
    public float failVignetteIntensity = 0.6f;
    public float vignetteFadeOutDuration = 0.5f;
    public Color failVignetteColor = Color.red;

    [Header("音效")]
    public AudioClip glassClip;//裂痕音效
    public AudioClip[] glassStepClips;//应力音效
    private float lastGlassCrackTime = -10f;
    private float glassCrackCooldown = 2f;

    [Header("事件回调")]
    public UnityEvent onAllRoundsCompleted;
    public UnityEvent onUnwinnableFailed;

    private int currentRoundIndex = 0;
    private bool isExecuting = false;
    private int currentCrackFrame = 0;

    private CinemachineBasicMultiChannelPerlin cameraNoise;
    private Coroutine currentSlowCrackCoroutine = null;
    private Coroutine currentCrackStepCoroutine = null;
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

        if (virtualCamera != null)
        {
            cameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (cameraNoise != null)
            {
                cameraNoise.m_AmplitudeGain = defaultNoiseAmplitude;
                cameraNoise.m_FrequencyGain = defaultNoiseFrequency;
            }
        }

        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            originalVignetteIntensity = vignette.intensity.value;
            vignette.intensity.value = 0f;
            if (vignette.color != null)
                vignette.color.value = failVignetteColor;
        }

        // 初始化手掌印（隐藏或透明）
        if (markRenderers != null)
        {
            foreach (var mark in markRenderers)
            {
                if (mark != null)
                {
                    mark.color = new Color(mark.color.r, mark.color.g, mark.color.b, 0f);
                    mark.transform.localScale = Vector3.one;
                }
            }
        }
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

        // 开场：快速裂纹从0到第一阶段结束帧
        if (crackStageFrames.Length >= 2)
        {
            int targetFrame = crackStageFrames[1];
            yield return StartCoroutine(PlayFastCrackAnimation(targetFrame));
        }

        while (currentRoundIndex < rounds.Count)
        {
            RoundData round = rounds[currentRoundIndex];
            bool isLastRound = (currentRoundIndex == rounds.Count - 1);

            int stageStartFrame = crackStageFrames[currentRoundIndex + 1];
            int stageEndFrame = (currentRoundIndex + 2 < crackStageFrames.Length) ? crackStageFrames[currentRoundIndex + 2] : crackFrames.Length - 1;

            if (currentCrackFrame < stageStartFrame)
            {
                yield return StartCoroutine(PlayFastCrackAnimation(stageStartFrame));
            }
            else if (currentCrackFrame > stageStartFrame)
            {
                currentCrackFrame = stageStartFrame;
                glassImage.sprite = crackFrames[currentCrackFrame];
            }

            singleRoundQTE.requiredClicks = round.requiredClicks;
            singleRoundQTE.timeLimit = round.timeLimit;
            singleRoundQTE.SetPosition(round.targetUIPosition);

            bool roundSuccess = false;
            bool roundFinished = false;

            UnityAction successListener = () => { roundSuccess = true; roundFinished = true; };
            UnityAction failListener = () => { roundSuccess = false; roundFinished = true; };

            singleRoundQTE.onQTESuccess.AddListener(successListener);
            singleRoundQTE.onQTEFail.AddListener(failListener);

            singleRoundQTE.ShowAndStart();
            currentCrackStepCoroutine = StartCoroutine(CrackStepDuringQTE(stageEndFrame, crackStepsPerQTE, crackStepInterval));

            yield return new WaitUntil(() => roundFinished);

            if (currentCrackStepCoroutine != null)
            {
                StopCoroutine(currentCrackStepCoroutine);
                currentCrackStepCoroutine = null;
            }

            singleRoundQTE.onQTESuccess.RemoveListener(successListener);
            singleRoundQTE.onQTEFail.RemoveListener(failListener);

            bool hidden = false;
            singleRoundQTE.Hide(() => hidden = true);
            yield return new WaitUntil(() => hidden);

            // 处理成功/失败
            if (roundSuccess)
            {
                if (currentRoundIndex < markRenderers.Length && markRenderers[currentRoundIndex] != null)
                {
                    SpriteRenderer mark = markRenderers[currentRoundIndex];
                    mark.color = new Color(mark.color.r, mark.color.g, mark.color.b, 0f);
                    mark.transform.localScale = Vector3.one;
                    mark.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
                    mark.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }
                yield return new WaitForSeconds(pauseAfterQTE);
            }
            else
            {
                // 普通失败：播放抖动和暗角
                TriggerFailShake();
                TriggerFailVignette();
            }

            // 如果是最后一轮（无论成功或失败）且是必死关，直接结束循环，触发完成事件
            if (isLastRound && round.isUnwinnable)
            {
                break; // 退出 while 循环
            }

            currentRoundIndex++;
        }

        // 所有轮次完成（包括最后一轮必死关结束）→ 触发第一关完成事件
        onAllRoundsCompleted?.Invoke();
        isExecuting = false;
    }
    private IEnumerator CrackStepDuringQTE(int maxFrame, int steps, float interval)
    {
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(interval);
            if (currentCrackFrame < maxFrame)
            {
                currentCrackFrame++;
                if (currentCrackFrame < crackFrames.Length)
                    glassImage.sprite = crackFrames[currentCrackFrame];

                // 随机播放一个应力音效
                if (glassStepClips != null && glassStepClips.Length > 0)
                {
                    AudioClip stepClip = glassStepClips[Random.Range(0, glassStepClips.Length)];
                    AudioManager.Instance.Play2DSound(stepClip, 0.5f); // 音量比例0.5可调
                }
            }
            else
            {
                break;
            }
        }
    }
    private IEnumerator PlayFastCrackAnimation(int targetFrameIndex)
    {
        if (glassImage == null || crackFrames.Length == 0)
            yield break;
        // 播放玻璃裂痕音效（带冷却，避免重叠）
        if (glassClip != null && Time.time - lastGlassCrackTime >= glassCrackCooldown)
        {
            AudioManager.Instance.Play2DSound(glassClip, 0.6f); // 音量比例0.6可调
            lastGlassCrackTime = Time.time;
        }

        targetFrameIndex = Mathf.Clamp(targetFrameIndex, 0, crackFrames.Length - 1);
        if (currentCrackFrame >= targetFrameIndex)
            yield break;

        int startFrame = currentCrackFrame;
        int frameRange = targetFrameIndex - startFrame;
        float elapsed = 0f;

        while (elapsed < fastCrackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fastCrackDuration);
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
        yield return new WaitForSeconds(pauseAfterFastCrack);
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
        float startAmp = cameraNoise.m_AmplitudeGain;
        float startFreq = cameraNoise.m_FrequencyGain;

        float riseElapsed = 0f;
        while (riseElapsed < failShakeRiseDuration)
        {
            riseElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(riseElapsed / failShakeRiseDuration);
            cameraNoise.m_AmplitudeGain = Mathf.Lerp(startAmp, failNoiseAmplitude, t);
            cameraNoise.m_FrequencyGain = Mathf.Lerp(startFreq, failNoiseFrequency, t);
            yield return null;
        }
        cameraNoise.m_AmplitudeGain = failNoiseAmplitude;
        cameraNoise.m_FrequencyGain = failNoiseFrequency;

        yield return new WaitForSeconds(failShakeDuration);

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
        DOTween.Kill(vignette);
        Sequence vignetteSeq = DOTween.Sequence();
        vignetteSeq.Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, failVignetteIntensity, 0.2f)
            .SetEase(Ease.OutCubic));
        vignetteSeq.AppendInterval(0.1f);
        vignetteSeq.Append(DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, vignetteFadeOutDuration)
            .SetEase(Ease.OutSine));
        vignetteSeq.Play();
    }
}