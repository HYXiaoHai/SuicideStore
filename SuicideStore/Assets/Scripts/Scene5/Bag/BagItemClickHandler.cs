using DG.Tweening;
using UnityEngine;

public class BagItemClickHandler : MonoBehaviour
{
    public int itemId;                      // 对应 DraggableItem 的 itemId
    public DraggableItem externalItem;      // 对应的包外拖拽物体
    public SpriteRenderer bagSprite;        // 自身的 SpriteRenderer

    private void Start()
    {
        if (bagSprite == null)
            bagSprite = GetComponent<SpriteRenderer>();
        //// 确保包外物品初始隐藏
        //if (externalItem != null)
        //    externalItem.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (BagPackingManager.Instance == null || !BagPackingManager.Instance.isGameStarted || BagPackingManager.Instance.gameCompleted)
            return;

        // 如果物品已被拿出（包内图片隐藏），则忽略点击
        if (!bagSprite.gameObject.activeSelf)
            return;

        // 渐隐包内图片
        bagSprite.DOKill();
        bagSprite.DOFade(0f, 0.2f).OnComplete(() =>
        {
            bagSprite.gameObject.SetActive(false);
        });

        // 激活包外物品，并设置位置到鼠标位置，然后开始拖拽
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        externalItem.transform.position = mousePos;
        externalItem.gameObject.SetActive(true);
        Debug.Log("开始拖拽"+externalItem.gameObject.name);
        // 设置对应的包内图片引用，以便放回时恢复
        externalItem.bagItemSprite = bagSprite;
        externalItem.StartDrag(mousePos);
    }
}