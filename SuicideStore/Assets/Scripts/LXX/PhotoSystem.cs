using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PhotoSystem : MonoBehaviour
{
    [Header("照片设置")]
    public GameObject[] photos;
    public int currentPhotoIndex = 0;

    [Header("灯光效果")]
    public Light2D[] lights;
    public float[] lightTargetIntensities;
    public float[] volumetricTargetIntensities;
    public float lightAnimationDuration = 0.5f;
    [Header("心跳效果")]
    public float heartbeatAmplitude = 0.1f;      // 浮动幅度（绝对值，例如 0.1）
    public float heartbeatDuration = 1f;       // 单次浮动周期（秒）

    //[Header("对话设置")]
    //public GameObject dialoguePanel;
    //public TextMeshProUGUI dialogueText;
    //public string[] dialogueTexts;
    //public float dialogueDuration = 2f;

    private bool[] photoTriggered;
    private PhotoTrigger[] photoTriggers;
    public DoorTrigger doorTrigger;

    // 存储每个灯光的心跳动画
    private Tweener[] heartbeatIntensityTweeners;
    private Tweener[] heartbeatVolumeTweeners;
    void Start()
    {
        Initialize();
        SetLightState(0);
    }

    void Initialize()
    {
        photoTriggered = new bool[photos.Length];
        photoTriggers = new PhotoTrigger[photos.Length];

        for (int i = 0; i < photos.Length; i++)
        {
            photoTriggered[i] = false;
            if (photos[i] != null)
            {
                photoTriggers[i] = photos[i].GetComponent<PhotoTrigger>();
                if (photoTriggers[i] != null)
                {
                    photoTriggers[i].photoIndex = i;
                    photoTriggers[i].photoSystem = this;
                }
            }
        }

        //if (dialoguePanel != null)
        //    dialoguePanel.SetActive(false);

        // 初始化心跳数组
        heartbeatIntensityTweeners = new Tweener[lights.Length];
        heartbeatVolumeTweeners = new Tweener[lights.Length];

        // 所有灯光初始强度为0，并优化性能：强度为0的灯光直接禁用
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;
            lights[i].intensity = 0f;
            lights[i].volumeIntensity = 0f;
            lights[i].enabled = false;   // 先全部禁用，第一个灯光会在 SetLightState(0) 中被重新启用
        }
    }

    // 灯光控制（带动画，并自动根据强度开关 Light2D 组件）
    public void SetLightState(int activeIndex)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            int localIndex = i;
            Light2D light = lights[localIndex];
            if (light == null) continue;

            bool shouldActivate = (activeIndex >= 0 && localIndex == activeIndex);

            if (shouldActivate)
            {
                // 激活灯光：确保组件开启
                if (!light.enabled) light.enabled = true;

                float targetIntensity = (activeIndex < lightTargetIntensities.Length) ? lightTargetIntensities[activeIndex] : 0.76f;
                float targetVol = (activeIndex < volumetricTargetIntensities.Length) ? volumetricTargetIntensities[activeIndex] : 0.37f;
                // 先停止该灯光的任何现有心跳
                StopHeartbeat(localIndex);

                DOTween.To(() => light.intensity, x => light.intensity = x, targetIntensity, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { CheckAndSetLightEnabled(light);
                        if (light.enabled && light.intensity > 0.01f)
                            StartHeartbeat(localIndex, targetIntensity, targetVol);
                    });

                DOTween.To(() => light.volumeIntensity, x => light.volumeIntensity = x, targetVol, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => CheckAndSetLightEnabled(light));
            }
            else
            {
                // 熄灭灯光：停止心跳，然后渐灭
                StopHeartbeat(localIndex);
                // 熄灭灯光（动画结束后如果强度为0则禁用组件）
                DOTween.To(() => light.intensity, x => light.intensity = x, 0f, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => CheckAndSetLightEnabled(light));
                DOTween.To(() => light.volumeIntensity, x => light.volumeIntensity = x, 0f, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => CheckAndSetLightEnabled(light));
            }
        }
    }


    // 为指定灯光启动心跳效果（在基础值上周期性浮动）
    private void StartHeartbeat(int index, float baseIntensity, float baseVolume)
    {
        if (index < 0 || index >= lights.Length) return;
        Light2D light = lights[index];
        if (light == null) return;

        // 心跳动画：强度在 [baseIntensity - amplitude, baseIntensity + amplitude] 之间来回
        float lowIntensity = Mathf.Max(0, baseIntensity - heartbeatAmplitude);
        float highIntensity = baseIntensity + heartbeatAmplitude;
        float lowVolume = Mathf.Max(0, baseVolume - heartbeatAmplitude);
        float highVolume = baseVolume + heartbeatAmplitude;

        // 使用 DOTween 的 Sequence 或者循环的 Yoyo 动画
        Tweener intensityTweener = DOTween.To(() => light.intensity, x => light.intensity = x, highIntensity, heartbeatDuration / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        Tweener volumeTweener = DOTween.To(() => light.volumeIntensity, x => light.volumeIntensity = x, highVolume, heartbeatDuration / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        heartbeatIntensityTweeners[index] = intensityTweener;
        heartbeatVolumeTweeners[index] = volumeTweener;
    }

    // 停止指定灯光的心跳
    private void StopHeartbeat(int index)
    {
        if (index < 0 || index >= lights.Length) return;
        if (heartbeatIntensityTweeners[index] != null)
        {
            heartbeatIntensityTweeners[index].Kill();
            heartbeatIntensityTweeners[index] = null;
        }
        if (heartbeatVolumeTweeners[index] != null)
        {
            heartbeatVolumeTweeners[index].Kill();
            heartbeatVolumeTweeners[index] = null;
        }
    }

    // 辅助方法：根据强度是否为零来启用/禁用 Light2D 组件

    private void CheckAndSetLightEnabled(Light2D light)
    {
        if (light == null) return;
        bool shouldEnable = light.intensity > 0.01f || light.volumeIntensity > 0.01f;
        light.enabled = shouldEnable;
    }

    // 由 PhotoTrigger 调用
    public void OnPhotoTrigger(int photoIndex)
    {
        if (photoIndex != currentPhotoIndex) return;
        if (photoTriggered[photoIndex]) return;

        photoTriggered[photoIndex] = true;

        // 切换到下一个灯光
        if (photoIndex + 1 < lights.Length)
            SetLightState(photoIndex + 1);
        else
            SetLightState(-1);   // 所有照片完成，熄灭所有灯光

        StartCoroutine(NextPhoto(photoIndex));
    }

    //void ShowDialogue(int photoIndex)
    //{
    //    if (dialoguePanel != null && dialogueText != null)
    //    {
    //        int idx = Mathf.Min(photoIndex, dialogueTexts.Length - 1);
    //        dialogueText.text = dialogueTexts[idx];
    //        dialoguePanel.SetActive(true);
    //    }
    //}

    IEnumerator NextPhoto(int photoIndex)
    {
        yield return new WaitForSeconds(lightAnimationDuration);
        currentPhotoIndex = photoIndex + 1;
        if (currentPhotoIndex >= photos.Length)
        {
            if (doorTrigger != null)
                doorTrigger.canLoad = true;
        }
    }
}