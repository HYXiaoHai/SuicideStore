using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class BagPackingManager : MonoBehaviour
{
    public static BagPackingManager Instance;

    [Header("游戏开始时展现的物品")]
    public float startDuration;//开始游戏的动画时长
    public Image bagText1;//对话框1
    public Image bagText2;//对话框2
    public SpriteRenderer item1;//物品1
    public SpriteRenderer item2;//物品2
    public SpriteRenderer item3;//物品3

    [Header("区域设置")]
    public Collider2D bagArea;
    public List<Collider2D> externalSlots;  // 外部展示区域触发器列表（6个）

    [Header("物品列表")]
    public List<DraggableItem> allItems;

    private int itemsFromInsideToOutside = 0;
    private int itemsFromOutsideToInside = 0;
    public bool gameCompleted = false;
    public bool isGameStarted = false;   // 游戏是否已开始（可拖拽）
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        bagText1.transform.localScale = Vector3.zero;
        bagText2.transform.localScale = Vector3.zero;
        // 初始化每个物品的状态和初始位置
        foreach (var item in allItems)
        {
            // 判断初始位置归属
            if (IsInBagArea(item.transform.position))
            {
                item.initialInside = true;
                // 书包内物品的初始位置就是当前位置，不需要额外设置
                item.SetOriginalPosition(item.transform.position);
            }
            else
            {
                Collider2D slot = GetSlotContainingPosition(item.transform.position);
                if (slot != null)
                {
                    item.initialInside = false;
                    // 确保外部物品精确吸附到展示区域中心（如果初始位置偏差）
                    Vector3 targetPos = slot.transform.position;
                    item.transform.position = targetPos;
                    item.SetOriginalPosition(targetPos);
                }
                else
                {
                    Debug.LogWarning($"物品 {item.name} 没有放在任何展示区域内，请调整初始位置");
                }
            }
            UpdateItemStatus(item);
        }
        Debug.Log("收拾书包游戏开始");
    }

    public void StartGame()
    {
        Sequence seq = DOTween.Sequence();
        seq.Join(item1.DOFade(1f, startDuration));
        seq.Join(item2.DOFade(1f, startDuration));
        seq.Join(item3.DOFade(1f, startDuration));
        seq.OnComplete(() =>
        {
            isGameStarted = true;
            Debug.Log("游戏已开始，可以拖拽物品");
        });
    }
    public void CheckItemPlacement(DraggableItem item)
    {
        if (gameCompleted) return;

        bool wasInside = item.isInsideBag;
        bool wasOutsideSlot = item.isOutsideSlot;

        UpdateItemStatus(item);

        if (wasInside == item.isInsideBag && wasOutsideSlot == item.isOutsideSlot)
            return;

        // 处理计数变化
        if (item.initialInside)
        {
            // 内部物品：移出书包到外部槽位则增加，从外部移回书包则减少
            if (!wasInside && item.isInsideBag)
                itemsFromInsideToOutside--;
            else if (wasInside && !item.isInsideBag && item.isOutsideSlot)
                itemsFromInsideToOutside++;
        }
        else // 原本在书包外的物品
        {
            // 外部物品：移入书包内增加，移出书包（到任何外部区域）减少
            if (!wasInside && item.isInsideBag)
            {
                itemsFromOutsideToInside++;
            }
            else if (wasInside && !item.isInsideBag)
                itemsFromOutsideToInside--;
        }

        itemsFromInsideToOutside = Mathf.Max(0, itemsFromInsideToOutside);
        itemsFromOutsideToInside = Mathf.Max(0, itemsFromOutsideToInside);
        if((itemsFromOutsideToInside>0&& itemsFromInsideToOutside<3))
        {
            bagText1.transform.DOScale(1f, startDuration).SetEase(Ease.OutExpo);
        }
        if(itemsFromInsideToOutside>=3)
        {
            bagText1.transform.DOScale(0f, startDuration).SetEase(Ease.OutExpo);
        }
        Debug.Log($"内部→外部：{itemsFromInsideToOutside}/3，外部→内部：{itemsFromOutsideToInside}/3");

        if (itemsFromInsideToOutside >= 3 && itemsFromOutsideToInside >= 3)
            OnGameComplete();
    }

    private void UpdateItemStatus(DraggableItem item)
    {
        item.isInsideBag = IsInBagArea(item.transform.position);
        item.isOutsideSlot = GetSlotContainingPosition(item.transform.position) != null;
    }

    public bool IsInBagArea(Vector3 pos)
    {
        return bagArea != null && bagArea.OverlapPoint(pos);
    }

    public Collider2D GetSlotContainingPosition(Vector3 pos)
    {
        foreach (var slot in externalSlots)
        {
            if (slot != null && slot.OverlapPoint(pos))
                return slot;
        }
        return null;
    }
    private void OnGameComplete()
    {
        gameCompleted = true;
        Sequence seq = DOTween.Sequence();

        seq.Join(bagText1.transform.DOScale(0f, startDuration).SetEase(Ease.OutExpo));
        seq.Join(bagText2.transform.DOScale(1f, startDuration).SetEase(Ease.OutExpo));
        seq.OnComplete(() =>
        {
            PopBubbleManage.Instance.StartGame();
            Debug.Log("收拾书包完成！开启下一关");
        });
    }
}