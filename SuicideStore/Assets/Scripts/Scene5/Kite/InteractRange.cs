using UnityEngine;

public class InteractRange : MonoBehaviour
{
    public BaseInteractable targetInteractable; // 要通知的交互物品
    public SpriteRenderer spriteRenderer;
    public Sprite image1;
    public Sprite image2;
    public GameObject prompt;//按键提示
    private bool isDisabled = false;   // 交互后永久禁用提示
    public void DisablePrompt()
    {
        isDisabled = true;
        if (prompt != null)
            prompt.SetActive(false);
        if (spriteRenderer != null)
            spriteRenderer.sprite = image1;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDisabled) return;
        if (other.CompareTag("Player") && targetInteractable != null)
        {
            prompt.SetActive(true);
            if (spriteRenderer!=null)
            spriteRenderer.sprite = image2;
            targetInteractable.SetPlayerInRange(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (isDisabled) return;
        if (other.CompareTag("Player") && targetInteractable != null)
        {
            prompt.SetActive(false);
            if (spriteRenderer != null)
                spriteRenderer.sprite = image1;
            targetInteractable.SetPlayerInRange(false);
        }
    }
}