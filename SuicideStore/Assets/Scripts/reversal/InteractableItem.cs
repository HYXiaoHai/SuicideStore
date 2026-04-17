using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("交互设置")]
    public string interactKey = "e";        // 交互按键（默认 E）

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;       // 记录进入范围的玩家对象

    void Start()
    {
    }

    void Update()
    {
        // 只有在玩家范围内才检测输入
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    // 玩家进入交互范围
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            currentPlayer = other.gameObject;

        }
    }

    // 玩家离开交互范围
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            currentPlayer = null;

        }
    }

    // 交互逻辑（具体功能在此实现，目前为空）
    public virtual void Interact()
    {
        Debug.Log(gameObject.name + " 被交互了！");
    }
}