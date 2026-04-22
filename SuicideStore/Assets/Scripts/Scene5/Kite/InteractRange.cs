using UnityEngine;

public class InteractRange : MonoBehaviour
{
    public BaseInteractable targetInteractable; // 要通知的交互物品

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetInteractable != null)
            targetInteractable.SetPlayerInRange(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetInteractable != null)
            targetInteractable.SetPlayerInRange(false);
    }
}