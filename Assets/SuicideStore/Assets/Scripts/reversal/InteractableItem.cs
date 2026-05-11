using UnityEngine;
using DG.Tweening;

public class InteractableItem : MonoBehaviour
{
    [Header("交互设置")]
    public string interactKey = "e";        //交互按键（默认 E）
    public SpriteRenderer interactableCopywriting;//显示的交互文案

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;       //记录进入范围的玩家对象
    public bool isCompleted = false; //是否已经触发
    public GameObject interactPrompt;//对应的交互提示
    void Start()
    {
    }

    void Update()
    {
        // 只有在玩家范围内才检测输入
        if (isPlayerInRange && Input.GetKeyDown(interactKey) && CanInteract())
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
    private bool CanInteract()
    {
        if (ReversalMange.Instance == null) return true;
        return ReversalMange.Instance.canInteractItems;
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
        if (isCompleted) return;
        if (!CanInteract()) return;
        Debug.Log(gameObject.name + " 被交互了！");
        isCompleted = true;

        // 通知关卡管理器，增加完成计数
        if (ReversalMange.Instance != null)
            ReversalMange.Instance.RegisterInteraction();

        if (interactableCopywriting != null)
            interactableCopywriting.DOFade(1f, 1f);
    }
}