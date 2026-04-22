using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
// ==================== 交互物品基类 ====================
public abstract class BaseInteractable : MonoBehaviour
{
    public bool isPlayerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }

    public abstract void OnInteract();

    void OnMouseDown()
    {
        if (isPlayerInRange && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("点击");
            OnInteract();
        }
    }
}

