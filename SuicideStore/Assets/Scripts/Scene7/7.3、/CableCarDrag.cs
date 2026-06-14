using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CableCarDrag : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Scene7_3Manager manager;
    public Image image;
    public bool isDrag;

    private Tween pulseTween;
    private bool isPointerInside = false;

    private void Start()
    {
        if (image != null)
            StartPulse();
    }

    private void StartPulse()
    {
        // 停止现有动画
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        // 确保完全可见后开始脉冲
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
        pulseTween = image.DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    public void StopPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        if (image != null)
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }
    private void StopPulseAndHide()
    {
        if (manager.currentPanel != 0) return;
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManage.Instance.isSetting) return;
        isDrag = true;
        // 拖拽时隐藏并停止脉冲
        StopPulseAndHide();
        if (manager != null)
            manager.OnCableCarDrag(eventData.delta);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        if (!isPointerInside)
            StartPulse();
        else
            StopPulseAndHide(); // 如果鼠标还在内部，保持透明
        if (manager != null)
            manager.OnCableCarDragEnd();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManage.Instance.isSetting) return;
        isPointerInside = true;
        StopPulseAndHide();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;
        if (!isDrag)
            StartPulse();
    }
}