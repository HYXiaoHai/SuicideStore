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

    [Header("对话设置")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogueTexts;
    public float dialogueDuration = 2f;

    private bool[] photoTriggered;
    private PhotoTrigger[] photoTriggers;
    public DoorTrigger doorTrigger;

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

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

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

                DOTween.To(() => light.intensity, x => light.intensity = x, targetIntensity, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => CheckAndSetLightEnabled(light));
                DOTween.To(() => light.volumeIntensity, x => light.volumeIntensity = x, targetVol, lightAnimationDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => CheckAndSetLightEnabled(light));
            }
            else
            {
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
        ShowDialogue(photoIndex);

        // 切换到下一个灯光
        if (photoIndex + 1 < lights.Length)
            SetLightState(photoIndex + 1);
        else
            SetLightState(-1);   // 所有照片完成，熄灭所有灯光

        StartCoroutine(NextPhoto(photoIndex));
    }

    void ShowDialogue(int photoIndex)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            int idx = Mathf.Min(photoIndex, dialogueTexts.Length - 1);
            dialogueText.text = dialogueTexts[idx];
            dialoguePanel.SetActive(true);
        }
    }

    IEnumerator NextPhoto(int photoIndex)
    {
        yield return new WaitForSeconds(dialogueDuration);
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentPhotoIndex = photoIndex + 1;
        if (currentPhotoIndex >= photos.Length)
        {
            if (doorTrigger != null)
                doorTrigger.canLoad = true;
        }
    }
}