using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
// ==================== 交互物品基类 ====================
public abstract class BaseInteractable : MonoBehaviour
{
    public bool isPlayerInRange = false;

    // 供交互区触发器调用的方法
    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
    }

    public abstract void OnInteract();

    //void OnMouseDown()
    //{
    //    if (isPlayerInRange && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
    //    {
    //        Debug.Log($"点击 {gameObject.name}");
    //        OnInteract();
    //    }
    //}
    private void Update()
    {
        if (isPlayerInRange&&Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"点击 {gameObject.name}");
            OnInteract();
        }
    }
}

