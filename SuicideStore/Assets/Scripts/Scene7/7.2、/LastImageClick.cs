using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LastImageClick : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public MemorySceneManager memoryManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (memoryManager != null)
        {
            memoryManager.OnLastImageClick();
        }
    }
}
