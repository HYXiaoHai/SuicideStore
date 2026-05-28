using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Scene7InteractableItem : MonoBehaviour
{
    [Header("交互设置")]
    public string interactKey = "e";        //交互按键（默认 E）
    public SpriteRenderer interactableCopywriting;//显示的交互文案
    public bool canInteractable = false;//是否可以交互
    private bool isPlayerInRange = false;
    public bool isCompleted = false; //是否已经触发
    public SpriteRenderer interactPrompt;//对应的交互提示
    [Header("下一个")]
    public Scene7InteractableItem nexInteractableItem;
    public DoorInteraction nexDoorInteraction;
    void Start()
    {

    }
    public void SetInteractable(bool enable)
    {
        if(enable)
        {
            interactPrompt.gameObject.SetActive(true);
            interactPrompt.DOFade(1f, 0.5f).OnComplete(() => {
                canInteractable = true;
                interactPrompt.GetComponent<EFloating>().StartFloating(true);
            });
        }
        else
        {
            interactPrompt.gameObject.SetActive(false);
            canInteractable = false;
        }
    }

    void Update()
    {
        // 只有在玩家范围内才检测输入
        if (canInteractable&&isPlayerInRange && Input.GetKeyDown(interactKey))
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

        }
    }
    // 玩家离开交互范围
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    // 交互逻辑（具体功能在此实现，目前为空）
    public void Interact()
    {
        if (isCompleted) return;
        isCompleted = true;

        if (interactPrompt != null)
        {
            interactPrompt.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() => {
                interactPrompt.GetComponent<EFloating>().StartFloating(false);
                interactPrompt.gameObject.SetActive(false);
            });
        }
        
        if(nexInteractableItem != null)
        {
            nexInteractableItem.SetInteractable(true);
        }
        if(nexDoorInteraction != null)
        {
            nexDoorInteraction.SetInteractable(true);
        }

        if (interactableCopywriting != null)
            interactableCopywriting.DOFade(1f, 1f);
    }
}