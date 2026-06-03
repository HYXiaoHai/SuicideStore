using UnityEngine;
using UnityEngine.EventSystems;

public class PencilDrag : MonoBehaviour,IBeginDragHandler,IDragHandler
{
    public RectTransform canvasRt;
    public void OnBeginDrag(PointerEventData eventData){}
    public void OnDrag(PointerEventData eventData)
    {
        GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvasRt.localScale.x;
    }
}