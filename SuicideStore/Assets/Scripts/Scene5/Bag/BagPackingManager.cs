using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BagPackingManager : MonoBehaviour
{
    public static BagPackingManager Instance;

    [Header("游戏开始淡入")]
    public float startDuration;
    public Image bagText1;
    public Image bagText2;
    public SpriteRenderer item1;
    public SpriteRenderer item2;
    public SpriteRenderer item3;

    [Header("物品对应UI图片(顺序:item1/item2/item3)")]
    public Image[] itemUIs;

    [Header("时间配置")]
    public float showUIDelay = 0.5f;
    public float hideDelay = 0.8f;
    public float fadeDuration = 0.4f;

    [Header("书包&外部槽位")]
    public Collider2D bagArea;
    public List<Collider2D> externalSlots;

    [Header("全部物品列表")]
    public List<DraggableItem> allItems;

    private int itemsFromInsideToOutside = 0;
    private int itemsFromOutsideToInside = 0;
    public bool gameCompleted = false;
    public bool isGameStarted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        bagText1.transform.localScale = Vector3.zero;
        bagText2.transform.localScale = Vector3.zero;

        // 初始化所有物品状态
        foreach (var item in allItems)
        {
            if (IsInBagArea(item.transform.position))
            {
                item.initialInside = true;
                item.SetOriginalPosition(item.transform.position);
            }
            else
            {
                Collider2D slot = GetSlotContainingPosition(item.transform.position);
                if (slot != null)
                {
                    item.initialInside = false;
                    Vector3 targetPos = slot.transform.position;
                    item.transform.position = targetPos;
                    item.SetOriginalPosition(targetPos);
                }
                else
                {
                    Debug.LogWarning($"物品 {item.name} 未找到外部槽位");
                }
            }
            UpdateItemStatus(item);
        }

        // 初始化所有UI：隐藏+透明
        if (itemUIs != null)
        {
            foreach (var ui in itemUIs)
            {
                if (ui != null)
                {
                    ui.gameObject.SetActive(false);
                    Color c = ui.color;
                    c.a = 0f;
                    ui.color = c;
                }
            }
        }

        Debug.Log("打包场景初始化完成");
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
            Debug.Log("游戏开始，可拖拽物品");
        });
    }

    public void CheckItemPlacement(DraggableItem item)
    {
        if (gameCompleted || item == null || item.isProcessed) return;

        bool wasInside = item.isInsideBag;
        bool wasOutsideSlot = item.isOutsideSlot;

        UpdateItemStatus(item);

        if (wasInside == item.isInsideBag && wasOutsideSlot == item.isOutsideSlot)
            return;

        // 计数逻辑
        if (item.initialInside)
        {
            if (!wasInside && item.isInsideBag)
                itemsFromInsideToOutside--;
            else if (wasInside && !item.isInsideBag && item.isOutsideSlot)
                itemsFromInsideToOutside++;
        }
        else
        {
            if (!wasInside && item.isInsideBag)
                itemsFromOutsideToInside++;
            else if (wasInside && !item.isInsideBag)
                itemsFromOutsideToInside--;
        }

        itemsFromInsideToOutside = Mathf.Max(0, itemsFromInsideToOutside);
        itemsFromOutsideToInside = Mathf.Max(0, itemsFromOutsideToInside);

        // 提示文本动画
        if ((itemsFromOutsideToInside > 0 && itemsFromInsideToOutside < 3))
        {
            bagText1.transform.DOScale(1f, startDuration).SetEase(Ease.OutExpo);
        }
        if (itemsFromInsideToOutside >= 3)
        {
            bagText1.transform.DOScale(0f, startDuration).SetEase(Ease.OutExpo);
        }

        Debug.Log($"内部→外部：{itemsFromInsideToOutside}/3 | 外部→内部：{itemsFromOutsideToInside}/3");

        // 核心逻辑：包内物品拖出书包 → 执行UI+消失流程
        if (item.initialInside && !IsInBagArea(item.transform.position) && !item.isProcessed)
        {
            StartCoroutine(ItemOutBagProcess(item));
        }

        // 通关判定
        if (itemsFromInsideToOutside >= 3 && itemsFromOutsideToInside >= 3)
        {
            OnGameComplete();
        }
    }

    /// <summary>
    /// 物品拖出书包流程：延迟弹UI → 再延迟 物品+UI 一起消失
    /// </summary>
    private IEnumerator ItemOutBagProcess(DraggableItem item)
    {
        item.isProcessed = true;
        Image targetUI = null;
        if (item.id != -1)
            targetUI = itemUIs[item.id];

        // 匹配对应UI
        //if (item.gameObject == item1.gameObject && itemUIs.Length > 0)
        //    targetUI = itemUIs[0];
        //else if (item.gameObject == item2.gameObject && itemUIs.Length > 1)
        //    targetUI = itemUIs[1];
        //else if (item.gameObject == item3.gameObject && itemUIs.Length > 2)
        //    targetUI = itemUIs[2];

        // 1. 拖出后等待反应时间
        yield return new WaitForSeconds(showUIDelay);

        // 2. 显示并淡入UI
        if (targetUI != null)
        {
            targetUI.gameObject.SetActive(true);
            targetUI.DOFade(1f, fadeDuration);
        }

        // 3. UI显示后等待消失延迟
        yield return new WaitForSeconds(hideDelay);

        // 4. 物品 + UI 同步淡出
        Sequence seq = DOTween.Sequence();
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr != null)
            seq.Append(sr.DOFade(0f, fadeDuration));

        if (targetUI != null)
            seq.Join(targetUI.DOFade(0f, fadeDuration));

        seq.OnComplete(() =>
        {
            allItems.Remove(item);
            Destroy(item.gameObject);
            if (targetUI != null)
                Destroy(targetUI.gameObject);
        });
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
            Debug.Log("打包完成，进入下一环节");
        });
    }
}