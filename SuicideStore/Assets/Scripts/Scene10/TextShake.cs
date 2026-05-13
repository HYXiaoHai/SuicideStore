using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextShake : MonoBehaviour
{
    [Header("抖动设置")]
    public float shakeDuration = 0.1f;
    public float shakeIntensity = 5f;
    public int shakeVibrato = 10;
    public float shakeRandomness = 90f;

    [Header("抖动类型")]
    public ShakeType shakeType = ShakeType.Position;

    [Header("自动抖动")]
    public bool autoShake = false;
    public float autoShakeInterval = 2f;

    private RectTransform rectTransform;
    private Text text;
    private TextMeshProUGUI textMesh;

    public enum ShakeType
    {
        Position,
        Rotation,
        Scale,
        All
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        text = GetComponent<Text>();
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        if (autoShake)
        {
            InvokeRepeating("Shake", 0f, autoShakeInterval);
        }
    }

    public void Shake()
    {
        if (rectTransform == null) return;

        Sequence shakeSeq = DOTween.Sequence();

        switch (shakeType)
        {
            case ShakeType.Position:
                shakeSeq.Append(rectTransform.DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness));
                break;
            case ShakeType.Rotation:
                shakeSeq.Append(rectTransform.DOShakeRotation(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness));
                break;
            case ShakeType.Scale:
                shakeSeq.Append(rectTransform.DOShakeScale(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness));
                break;
            case ShakeType.All:
                shakeSeq.Append(rectTransform.DOShakePosition(shakeDuration, shakeIntensity, shakeVibrato, shakeRandomness));
                shakeSeq.Join(rectTransform.DOShakeRotation(shakeDuration, shakeIntensity * 0.5f, shakeVibrato, shakeRandomness));
                shakeSeq.Join(rectTransform.DOShakeScale(shakeDuration, shakeIntensity * 0.1f, shakeVibrato, shakeRandomness));
                break;
        }

        shakeSeq.Play();
    }

    public void ShakeWithParameters(float duration, float intensity, int vibrato, float randomness)
    {
        if (rectTransform == null) return;

        shakeDuration = duration;
        shakeIntensity = intensity;
        shakeVibrato = vibrato;
        shakeRandomness = randomness;

        Shake();
    }

    public void StopShake()
    {
        if (rectTransform != null)
        {
            rectTransform.DOKill();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }
    }
}