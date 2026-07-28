using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

// 卡槽：单个固定正确位置
[Serializable]
public class CardSlot
{
    [Tooltip("场景里的点位空物体")]
    public Transform slotTrans;
    [HideInInspector] public bool isOccupied = false; // 是否已被正确卡片占用
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
    public int currentSlotIndex = -1;
    public bool isLocked = false; // 放对后锁定
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

    [Header("==== 图片显示设置 ====")]
    [Tooltip("每张反馈图出现的间隔时间（秒）")]
    public float imageShowDelay = 0.2f;
    [Tooltip("图片淡入动画时长（秒）")]
    public float fadeInDuration = 0.3f;

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
            TransitionManage.Instance.FadeIn(1f, Color.black);
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
        //为每个卡片物体挂载悬停脚本
        foreach (var card in allCards)
        {
            if (card.mainSprite != null)
            {
                CardHover hover = card.mainSprite.gameObject.GetComponent<CardHover>();
                if (hover == null)
                    hover = card.mainSprite.gameObject.AddComponent<CardHover>();
                hover.sortCard = card;
                hover.sortCard.currentSlotIndex = -1;
            }
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
            dragCard.mainSprite.sortingOrder = 1;

        // 1. 卡片互换（已锁定卡片不参与互换）
        bool isSwap = false;
        foreach (var otherCard in allCards)
        {
            if (otherCard == dragCard || otherCard.isLocked) continue;

            float dist = Vector3.Distance(
                dragCard.mainSprite.transform.position,
                otherCard.mainSprite.transform.position
            );

            //if (dist < swapDistance)
            //{
            //    Vector3 tempPos = dragCard.mainSprite.transform.position;
            //    dragCard.mainSprite.transform.position = otherCard.mainSprite.transform.position;
            //    otherCard.mainSprite.transform.position = tempPos;
            //    isSwap = true;
            //    break;
            //}
            if (dist < swapDistance)
            {
                // 交换位置
                Vector3 tempPos = dragCard.mainSprite.transform.position;
                dragCard.mainSprite.transform.position = otherCard.mainSprite.transform.position;
                otherCard.mainSprite.transform.position = tempPos;

                // ★ 交换 currentSlotIndex
                int tempSlot = dragCard.currentSlotIndex;
                dragCard.currentSlotIndex = otherCard.currentSlotIndex;
                otherCard.currentSlotIndex = -1;

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
                if (allSlots[i].isOccupied) continue;
                float d = Vector3.Distance(dragCard.mainSprite.transform.position, allSlots[i].correctPos);
                if (d < minDis)
                {
                    minDis = d;
                    nearestSlotIdx = i;
                }
            }
            if (nearestSlotIdx != -1 && minDis < snapRange)
            {
                //dragCard.mainSprite.transform.position = allSlots[nearestSlotIdx].correctPos;
                dragCard.mainSprite.transform.DOMove(
                      allSlots[nearestSlotIdx].correctPos,
                      0.12f
                  ).SetEase(Ease.OutQuad);
                dragCard.currentSlotIndex = nearestSlotIdx;
                Debug.Log("吸附");
            }
        }

        // 拖拽结束后统一检查分组完成状态
        CheckGroupComplete();
    }

    // 鼠标点击检测
    private void OnMouseDown()
    {
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit)
        {
            foreach (var card in allCards)
            {
                if (card.isLocked || card.mainSprite == null) continue;
                Collider2D col = card.mainSprite.GetComponent<Collider2D>();
                if (col != null && col == hit.collider)
                {
                    dragCard = card;
                    card.mainSprite.sortingOrder = 999;
                    // 将卡片旋转归零（带动画）
                    card.mainSprite.transform.DOKill(); //停止可能正在播放的旋转动画
                    card.mainSprite.transform.DORotate(Vector3.zero, 0.15f).SetEase(Ease.OutQuad);
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

            //锁定
            //if (dis <= judgeRange)
            if (card.currentSlotIndex == card.targetSlotIndex)
            {
                card.isLocked = true;
                card.mainSprite.sortingOrder = 0;
                Collider2D col = card.mainSprite.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                // 标记卡槽被占用
                allSlots[card.targetSlotIndex].isOccupied = true;
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
            //StartCoroutine(ShowImagesWithButton(0, group1_CardNum, lock_Group1));
            ShowImagesWithButton(0, group1_CardNum, lock_Group1);
        }
        else if (currentGroup == 2)
        {
            int start = group1_CardNum;
            //StartCoroutine(ShowImagesWithButton(start, group2_CardNum, lock_Group2));
            ShowImagesWithButton(start, group2_CardNum, lock_Group2);
        }
        else if (currentGroup == 3)
        {
            int start = group1_CardNum + group2_CardNum;
            //StartCoroutine(ShowImagesAndComplete(start, group3_CardNum));
            ShowImagesAndComplete(start, 2);
        }
    }
    void ShowImagesWithButton(int startIndex, int count, GameObject buttonObj)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = startIndex; i < startIndex + count; i++)
        {
            bool isLast = (i == startIndex + count - 1);
            if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
            {
                SpriteRenderer sr = feedbackImages[i].spriteRenderer;
                sr.enabled = true;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

                //照片淡入（顺序执行）
                seq.Append(sr.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));

                //如果是最后一张，同时淡入按钮
                if (isLast && buttonObj)
                {
                    buttonObj.SetActive(true);
                    Image btnImage = buttonObj.GetComponent<Image>();
                    if (btnImage != null)
                    {
                        Color c = btnImage.color;
                        c.a = 0f;
                        btnImage.color = c;
                        seq.Join(btnImage.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
                    }
                }
                if (!isLast)
                    seq.AppendInterval(imageShowDelay);
            }
        }
        seq.OnComplete(() =>
        {
            if (buttonObj)
            {
                Button btn = buttonObj.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(NextGroup);
                }
            }
        });

        seq.Play();
    }


    void ShowImagesAndComplete(int startIndex, int count)
    {
        Sequence seq = DOTween.Sequence();

        for (int i = startIndex; i < startIndex + count; i++)
        {
            if (i < feedbackImages.Count && feedbackImages[i].spriteRenderer != null)
            {
                SpriteRenderer sr = feedbackImages[i].spriteRenderer;
                sr.enabled = true;
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
                seq.Append(sr.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad));
                if (i < startIndex + count - 1)
                    seq.AppendInterval(imageShowDelay);
            }
        }

        seq.OnComplete(() => CompleteLevel());
        seq.Play();
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
                {
                    allCards[i].isLocked = false;
                    allCards[i].currentSlotIndex = -1; // 重置
                    Collider2D col = allCards[i].mainSprite.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true;
                    allCards[i].mainSprite.sortingOrder = 1;
                }
            }
        }
        else if (currentGroup == 3)
        {
            currentCamTargetY = camTargetY2;
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