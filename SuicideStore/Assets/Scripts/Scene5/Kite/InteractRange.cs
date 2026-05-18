using UnityEngine;

public class InteractRange : MonoBehaviour
{
    public BaseInteractable targetInteractable; // 要通知的交互物品
    public SpriteRenderer spriteRenderer;
    public Sprite image1;
    public Sprite image2;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetInteractable != null)
        {
            if(spriteRenderer!=null)
            spriteRenderer.sprite = image2;
            targetInteractable.SetPlayerInRange(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetInteractable != null)
        {
            spriteRenderer.sprite = image1;
            targetInteractable.SetPlayerInRange(false);
        }
    }
}