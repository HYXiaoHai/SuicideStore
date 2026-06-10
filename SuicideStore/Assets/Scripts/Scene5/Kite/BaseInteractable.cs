using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
// ==================== 交互物品基类 ====================
public abstract class BaseInteractable : MonoBehaviour
{
    public bool isPlayerInRange = false;
    public bool closeRange = false;//不启用范围
    public bool useMouse = false;//使用鼠标交互
    // 供交互区触发器调用的方法
    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
    }

    public abstract void OnInteract();

    void OnMouseDown()
    {
        if (useMouse&&(closeRange||isPlayerInRange) && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log($"点击 {gameObject.name}");
            OnInteract();
        }
    }
    private void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (!useMouse&&(closeRange || isPlayerInRange) && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"点击 {gameObject.name}");
            OnInteract();
        }
    }
}

