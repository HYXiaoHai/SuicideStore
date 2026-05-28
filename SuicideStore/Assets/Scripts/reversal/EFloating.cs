using DG.Tweening;
using UnityEngine;

public class EFloating : MonoBehaviour
{
    [Header("ÒÆ¶¯¶¯»­")]
    public float floatAmplitude = 0.3f;
    public float floatDuration = 1f;

    private Tweener floatTween;
    private Vector3 originalPosition;

    public bool startFloat = false;

    void Awake()
    {
        originalPosition = transform.position;
        Debug.Log(gameObject + "Awake");
        StartFloating(startFloat);
    }
    private void Start()
    {

    }
    public void StartFloating(bool enable)
    {
        if (this == null || gameObject == null)
            return;
        if (!enable)
        {
             floatTween?.Kill();
        }
        else
        {
            floatTween?.Kill();
            Vector3 upPos = originalPosition + Vector3.up * floatAmplitude;
            floatTween = transform.DOMove(upPos, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);//
        }
    }
    private void OnDestroy()
    {
        if (floatTween != null && floatTween.IsActive())
            floatTween.Kill();
    }
}