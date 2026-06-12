using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [HideInInspector] public bool isLocked = false; // 放对后锁定
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
    public GameObject lock_Group1;
    public GameObject lock_Group2;

    [Header("==== 相机移动参数 ====")]
    public Camera mainCam;
    public float camStartY;
    public float camTargetY1;
    public float camTargetY2;
    public float camMoveSpeed = 2f;

    [Header("==== 拖拽、吸附设置 ====")]
    [Tooltip("两张卡片靠近多远算互换")]
    public float swapDistance = 1.2f;
    [Tooltip("卡片离卡槽多远自动吸附")]
    public float snapRange = 0.8f;
    [Tooltip("判定归位正确的误差范围")]
    public float judgeRange = 0.3f;

    private int currentGroup = 1;
    private float currentCamTargetY;
    private SortCard dragCard;

    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(1f,Color.black);
            // 记录卡槽坐标
            foreach (var slot in allSlots)
        {
            if (slot.slotTrans != null)
                slot.correctPos = slot.slotTrans.position;
        }

        // 隐藏所有反馈图
        foreach (var img in feedbackImages)
        {
            if (img.spriteRenderer != null)
                img.spriteRenderer.enabled = false;
        }

        // 初始化相机
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(
                mainCam.transform.position.x,
                camStartY,
                mainCam.transform.position.z
            );
            currentCamTargetY = camStartY;
        }

        // 隐藏分组按钮
        if (lock_Group1) lock_Group1.SetActive(false);
        if (lock_Group2) lock_Group2.SetActive(false);

        // 初始所有卡片解锁，可拖拽
        foreach (var card in allCards)
        {
            card.isLocked = false;
        }
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

        // 鼠标抬起结束拖拽
        if (Input.GetMouseButtonUp(0) && dragCard != null)
        {
            DragEnd();
            dragCard = null;
        }
    }

    void DragEnd()
    {
        // 恢复渲染层级
        if (dragCard.mainSprite != null)
            dragCard.mainSprite.sortingOrder = 0;

        // 1. 卡片互换（已锁定卡片不参与互换）
        bool isSwap = false;
        foreach (var otherCard in allCards)
        {
            if (otherCard == dragCard || otherCard.isLocked) continue;

            float dist = Vector3.Distance(
                dragCard.mainSprite.transform.position,
                otherCard.mainSprite.transform.position
            );

            if (dist < swapDistance)
            {
                Vector3 tempPos = dragCard.mainSprite.transform.position;
                dragCard.mainSprite.transform.position = otherCard.mainSprite.transform.position;
                otherCard.mainSprite.transform.position = tempPos;
                isSwap = true;
                break;
            }
        }

        // 2. 自动吸附到最近卡槽
        if (!isSwap)
        {
            int nearestSlotIdx = -1;
            float minDis = float.MaxValue;
            for (int i = 0; i < allSlots.Count; i++)
            {
                float d = Vector3.Distance(dragCard.mainSprite.transform.position, allSlots[i].correctPos);
                if (d < minDis)
                {
                    minDis = d;
                    nearestSlotIdx = i;
                }
            }
            if (nearestSlotIdx != -1 && minDis < snapRange)
            {
                dragCard.mainSprite.transform.position = allSlots[nearestSlotIdx].correctPos;
            }
        }

        // 拖拽结束后统一检查分组完成状态
        CheckGroupComplete();
    }

    // 鼠标点击检测（修复射线，保证2D卡片可点击）
    private void OnMouseDown()
    {
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        // 兼容2D碰撞体，优先获取碰撞体
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit)
        {
            // 遍历卡片，只拾取【未锁定】的卡片
            foreach (var card in allCards)
            {
                if (card.isLocked || card.mainSprite == null) continue;
                Collider2D col = card.mainSprite.GetComponent<Collider2D>();
                if (col != null && col == hit.collider)
                {
                    dragCard = card;
                    card.mainSprite.sortingOrder = 999; // 拖拽置顶
                    break;
                }
            }
        }
    }

    // 检查当前分组是否全部摆放正确
    void CheckGroupComplete()
    {
        int startIndex = 0;
        int endIndex = 0;

        // 划分当前分组卡片范围
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

        bool allCorrect = true;
        // 先批量判定每张卡片是否归位，只在拖拽结束时锁定，不再动态解锁
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i >= allCards.Count) break;
            SortCard card = allCards[i];
            if (card.mainSprite == null || card.targetSlotIndex >= allSlots.Count) continue;

            Vector3 cardPos = card.mainSprite.transform.position;
            Vector3 rightPos = allSlots[card.targetSlotIndex].correctPos;
            float dis = Vector3.Distance(cardPos, rightPos);

            // 放对位置 → 锁定（一旦锁定，不再解锁）
            if (dis <= judgeRange)
            {
                card.isLocked = true;
            }
            else
            {
                allCorrect = false;
            }
        }

        // 整组全部正确 → 触发通关（显示反馈图+按钮）
        if (allCorrect)
        {
            GroupSuccess();
        }
    }

    // 分组通关：显示对应反馈图 + 弹出切换按钮
    void GroupSuccess()
    {
        Debug.Log("分组完成");
        if (currentGroup == 1)
        {
            // 显示第一组反馈图
            for (int i = 0; i < group1_CardNum; i++)
            {
                if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
                    feedbackImages[i].spriteRenderer.enabled = true;
            }
            // 显示下一组按钮
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

            CompleteLevel();
        }
    }

    // 切换下一组，重置当前组以外卡片状态
    void NextGroup()
    {
        currentGroup++;
        if (currentGroup == 2)
        {
            currentCamTargetY = camTargetY1;
            // 第二组卡片全部解锁可拖拽
            int start = group1_CardNum;
            int end = group1_CardNum + group2_CardNum - 1;
            for (int i = start; i <= end; i++)
            {
                if (i < allCards.Count)
                    allCards[i].isLocked = false;
            }
        }
        else if (currentGroup == 3)
        {
            currentCamTargetY = camTargetY2;
            // 第三组卡片全部解锁可拖拽
            int start = group1_CardNum + group2_CardNum;
            int end = start + group3_CardNum - 1;
            for (int i = start; i <= end; i++)
            {
                if (i < allCards.Count)
                    allCards[i].isLocked = false;
            }
        }
        Debug.Log("进入下一组");
    }

    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
            {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        // 转场完成后加载新场景
                        SceneManager.LoadScene(nextScene);
                    });
                    AudioManager.Instance.FadeOutCurrentBGM(1f, null);
               
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            OnMouseDown();
    }
}