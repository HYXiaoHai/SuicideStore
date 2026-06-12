using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// 卡槽：单个固定正确位置
[Serializable]
public class CardSlot
{
    [Tooltip("场景里的点位空物体")]
    public Transform slotTrans;
    [HideInInspector] public Vector3 correctPos;
}

// 卡片：绑定自己应该去哪个卡槽
[Serializable]
public class SortCard
{
    [Tooltip("卡片 SpriteRenderer")]
    public SpriteRenderer mainSprite;
    [Tooltip("这张卡片对应的正确卡槽下标(从0开始)")]
    public int targetSlotIndex;
    [Tooltip("初始是否可拖拽")]
    public bool isUnlocked = true;

    [HideInInspector] public Color originColor;
}

// 反馈图片
[Serializable]
public class FeedbackImage
{
    [Tooltip("反馈图 SpriteRenderer")]
    public SpriteRenderer spriteRenderer;
}

public class Scene9UIManager : MonoBehaviour
{
    [Header("==== 所有固定卡槽（按顺序摆放） ====")]
    public List<CardSlot> allSlots = new List<CardSlot>();

    [Header("==== 所有卡片 ====")]
    public List<SortCard> allCards = new List<SortCard>();

    [Header("==== 所有反馈图（与分组一一对应） ====")]
    public List<FeedbackImage> feedbackImages = new List<FeedbackImage>();

    [Header("==== 分组数量设置 ====")]
    public int group1_CardNum = 3;
    public int group2_CardNum = 3;
    public int group3_CardNum = 2;

    [Header("==== 解锁按钮（一共2个） ====")]
    public GameObject lock_Group1;  // 第一组通关 → 开启第二组
    public GameObject lock_Group2;  // 第二组通关 → 开启第三组

    [Header("==== 相机移动参数 ====")]
    public Camera mainCam;
    public float camStartY;
    public float camTargetY1;
    public float camTargetY2;
    public float camMoveSpeed = 2f;

    [Header("==== 拖拽、吸附、发光设置 ====")]
    [Tooltip("两张卡片靠近多远算互换")]
    public float swapDistance = 1.2f;
    [Tooltip("卡片离卡槽多远自动吸附")]
    public float snapRange = 0.8f;
    [Tooltip("判定归位正确的误差范围")]
    public float judgeRange = 0.3f;
    [Tooltip("拖拽透明度")]
    public float dragAlpha = 0.7f;
    [Tooltip("闪烁速度")]
    public float flickSpeed = 2f;
    [Tooltip("发光颜色")]
    public Color glowColor = new Color(0f, 0.7f, 1f);

    private int currentGroup = 1;
    private float currentCamTargetY;
    private SortCard dragCard;

    void Start()
    {
        // 1. 记录所有卡槽正确坐标
        foreach (var slot in allSlots)
        {
            if (slot.slotTrans != null)
                slot.correctPos = slot.slotTrans.position;
        }

        // 2. 初始化卡片颜色、发光
        foreach (var card in allCards)
        {
            if (card.mainSprite == null) continue;
            card.originColor = card.mainSprite.color;

            if (card.isUnlocked)
                StartCoroutine(GlowFlicker(card));
            else
                card.mainSprite.color = Color.gray;
        }

        // 3. 隐藏所有反馈图
        foreach (var img in feedbackImages)
        {
            if (img.spriteRenderer != null)
                img.spriteRenderer.enabled = false;
        }

        // 4. 相机初始位置
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(
                mainCam.transform.position.x,
                camStartY,
                mainCam.transform.position.z
            );
            currentCamTargetY = camStartY;
        }

        // 5. 隐藏两个解锁按钮
        if (lock_Group1) lock_Group1.SetActive(false);
        if (lock_Group2) lock_Group2.SetActive(false);
    }

    void Update()
    {
        // 相机平滑移动
        if (mainCam != null)
        {
            Vector3 camPos = mainCam.transform.position;
            camPos.y = Mathf.Lerp(camPos.y, currentCamTargetY, Time.deltaTime * camMoveSpeed);
            mainCam.transform.position = camPos;
        }

        // 拖拽跟随鼠标
        if (dragCard != null && dragCard.mainSprite != null)
        {
            Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = dragCard.mainSprite.transform.position.z;
            dragCard.mainSprite.transform.position = mouseWorld;
        }

        // 鼠标抬起：结束拖拽
        if (Input.GetMouseButtonUp(0) && dragCard != null)
        {
            DragEnd();
            dragCard = null;
        }
    }

    void DragEnd()
    {
        // 恢复拖拽透明度
        dragCard.mainSprite.color = dragCard.originColor;

        // 第一步：检测是否和其他卡片相撞 → 互换位置
        bool isSwap = false;
        foreach (var otherCard in allCards)
        {
            if (otherCard == dragCard || !otherCard.isUnlocked) continue;

            float dist = Vector3.Distance(
                dragCard.mainSprite.transform.position,
                otherCard.mainSprite.transform.position
            );

            if (dist < swapDistance)
            {
                // 互换坐标
                Vector3 tempPos = dragCard.mainSprite.transform.position;
                dragCard.mainSprite.transform.position = otherCard.mainSprite.transform.position;
                otherCard.mainSprite.transform.position = tempPos;
                isSwap = true;
                break;
            }
        }

        // 第二步：没有互换 → 自动吸附到最近卡槽
        if (!isSwap)
        {
            int nearestSlotIdx = -1;
            float minDis = float.MaxValue;

            for (int i = 0; i < allSlots.Count; i++)
            {
                float d = Vector3.Distance(
                    dragCard.mainSprite.transform.position,
                    allSlots[i].correctPos
                );
                if (d < minDis)
                {
                    minDis = d;
                    nearestSlotIdx = i;
                }
            }

            // 在吸附范围内 → 卡入卡槽
            if (nearestSlotIdx != -1 && minDis < snapRange)
            {
                dragCard.mainSprite.transform.position = allSlots[nearestSlotIdx].correctPos;
            }
        }

        // 第三步：检查当前分组是否全部归位正确
        CheckGroupComplete();
    }

    // 鼠标按下 → 开始拖拽
    private void OnMouseDown()
    {
        if (mainCam == null) return;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (!Physics2D.Raycast(ray.origin, ray.direction)) return;

        foreach (var card in allCards)
        {
            if (!card.isUnlocked || card.mainSprite == null) continue;

            Collider2D col = card.mainSprite.GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(ray.origin))
            {
                dragCard = card;
                // 拖拽半透明
                Color c = card.mainSprite.color;
                c.a = dragAlpha;
                card.mainSprite.color = c;
                // 层级置顶
                card.mainSprite.sortingOrder = 999;
                break;
            }
        }
    }

    // 检查当前分组是否全部拼对（每张卡落到自己指定卡槽）
    void CheckGroupComplete()
    {
        int startIndex = 0;
        int endIndex = 0;

        // 根据当前分组，确定卡片范围
        if (currentGroup == 1)
        {
            startIndex = 0;
            endIndex = group1_CardNum - 1;
        }
        else if (currentGroup == 2)
        {
            startIndex = group1_CardNum;
            endIndex = group1_CardNum + group2_CardNum - 1;
        }
        else if (currentGroup == 3)
        {
            startIndex = group1_CardNum + group2_CardNum;
            endIndex = startIndex + group3_CardNum - 1;
        }

        bool allRight = true;
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= allCards.Count) break;

            SortCard card = allCards[i];
            if (card.mainSprite == null) continue;
            if (card.targetSlotIndex >= allSlots.Count) continue;

            Vector3 cardPos = card.mainSprite.transform.position;
            Vector3 rightPos = allSlots[card.targetSlotIndex].correctPos;

            // 坐标不在误差范围内 → 错误
            if (Vector3.Distance(cardPos, rightPos) > judgeRange)
            {
                allRight = false;
                break;
            }
        }

        if (allRight)
            GroupSuccess();
    }

    // 当前分组拼对成功
    void GroupSuccess()
    {
        Debug.Log("当前分组拼对完成！");

        // 显示当前分组对应的反馈图
        if (currentGroup == 1)
        {
            for (int i = 0; i < group1_CardNum; i++)
            {
                if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
                    feedbackImages[i].spriteRenderer.enabled = true;
            }
            // 第一组通关 → 显示第一个锁
            if (lock_Group1)
            {
                lock_Group1.SetActive(true);
                Button btn = lock_Group1.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(NextGroup);
                }
            }
        }
        else if (currentGroup == 2)
        {
            int start = group1_CardNum;
            int count = group2_CardNum;
            for (int i = start; i < start + count; i++)
            {
                if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
                    feedbackImages[i].spriteRenderer.enabled = true;
            }
            // 第二组通关 → 显示第二个锁
            if (lock_Group2)
            {
                lock_Group2.SetActive(true);
                Button btn = lock_Group2.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(NextGroup);
                }
            }
        }
        else if (currentGroup == 3)
        {
            int start = group1_CardNum + group2_CardNum;
            int count = group3_CardNum;
            for (int i = start; i < start + count; i++)
            {
                if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
                    feedbackImages[i].spriteRenderer.enabled = true;
            }
            // 第三组是最后一组，不再弹出锁
        }
    }

    // 进入下一组、解锁卡片、移动相机
    void NextGroup()
    {
        currentGroup++;
        if (currentGroup == 2)
        {
            currentCamTargetY = camTargetY1;
            // 解锁第二组卡片
            int start = group1_CardNum;
            int end = group1_CardNum + group2_CardNum - 1;
            for (int i = start; i <= end; i++)
            {
                if (i >= allCards.Count) break;
                allCards[i].isUnlocked = true;
                allCards[i].mainSprite.color = allCards[i].originColor;
                StartCoroutine(GlowFlicker(allCards[i]));
            }
        }
        else if (currentGroup == 3)
        {
            currentCamTargetY = camTargetY2;
            // 解锁第三组卡片
            int start = group1_CardNum + group2_CardNum;
            int end = start + group3_CardNum - 1;
            for (int i = start; i <= end; i++)
            {
                if (i >= allCards.Count) break;
                allCards[i].isUnlocked = true;
                allCards[i].mainSprite.color = allCards[i].originColor;
                StartCoroutine(GlowFlicker(allCards[i]));
            }
        }
        Debug.Log("进入下一组");
    }

    // 卡片颜色闪烁发光（纯代码，不用材质/Outline）
    System.Collections.IEnumerator GlowFlicker(SortCard card)
    {
        float time = 0f;
        while (card.isUnlocked)
        {
            time += Time.deltaTime * flickSpeed;
            float bright = (Mathf.Sin(time) + 1f) * 0.5f;
            card.mainSprite.color = Color.Lerp(card.originColor, glowColor, bright);
            yield return null;
        }
        card.mainSprite.color = card.originColor;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseDown();
    }
}