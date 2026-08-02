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
    public GameObject bagPackingFather;
    //public Image bagText1;
    //public Image bagText2;

    [Header("物品对应UI图片(顺序:item0/item1/item2)")]
    public Image[] itemUIs;

    [Header("书包区域")]
    public Collider2D bagArea;
    public Collider2D externalArea;   // 合法外部区域（多边形碰撞体）

    [Header("全部包外物品（DraggableItem）")]
    public List<DraggableItem> allExternalItems;

    [Header("计数")]
    public int itemsFromInsideToOutside = 0;  // 从包内拿出到外部的数量
    public int itemsFromOutsideToInside = 0;  // 从外部放入包内的数量
    public bool gameCompleted = false;
    public bool isGameStarted = false;

    [Header("关卡切换")]
    public int nextLevelIndex = 2;//第2关
    public float changeDelay = 1f;//完成后多久切换镜头（延迟）
    private bool hasTriggeredSwitch = false; //防止重复触发

    [Header("下一关环境音")]
    public AudioClip envClip;
    public AudioSource envSourse;
    public Transform envClipPos;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        itemsFromInsideToOutside = 0;
        itemsFromOutsideToInside = 3;
        Debug.Log("打包场景初始化完成");
    }

    public void StartGame()
    {
        Sequence seq = DOTween.Sequence();
        SpriteRenderer[] spriteRenderers = bagPackingFather.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            seq.Join(sr.DOFade(1f, startDuration));
        }
        seq.OnComplete(() =>
        {
            isGameStarted = true;
            Debug.Log("游戏开始，可点击包内物品");
        });
        seq.Play();
    }

    public void OnItemPlacedOutside(DraggableItem item)
    {
        if (gameCompleted) return;
        if (item.itemId >= 0 && item.itemId <= 2)
        {
            StartCoroutine(ShowUIAndDisappear(item,itemsFromInsideToOutside)); // 显示UI并销毁物品
            itemsFromInsideToOutside = Mathf.Min(3, itemsFromInsideToOutside + 1);

        }
        else if (item.itemId < 0)
        {
            itemsFromOutsideToInside = Mathf.Max(0, itemsFromOutsideToInside - 1);
            Debug.Log(item.gameObject.name+"拿出");
        }
        CheckVictory();
    }

    public void OnItemReturnToBag(DraggableItem item)
    {
        if (gameCompleted) return;

        if (item.itemId < 0)
        {
            itemsFromOutsideToInside = Mathf.Min(3, itemsFromOutsideToInside + 1);
            Debug.Log(item.gameObject.name + "拿进");
        }
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (itemsFromInsideToOutside >= 3 && itemsFromOutsideToInside >= 3)
        {
            OnGameComplete();
        }
    }
    // UI显示并消失流程（原逻辑）
    private IEnumerator ShowUIAndDisappear(DraggableItem item,int index)
    {
        Image targetUI = null;
        if (item.itemId >= 0 && item.itemId < itemUIs.Length)
            targetUI = itemUIs[index];

        // 1. 延迟显示UI
        yield return new WaitForSeconds(0.5f);

        if (targetUI != null)
        {
            targetUI.gameObject.SetActive(true);
            targetUI.DOFade(1f, 0.4f);
        }

        yield return new WaitForSeconds(0.8f);

        // 物品+UI 同步淡出
        Sequence seq = DOTween.Sequence();
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr != null)
            seq.Append(sr.DOFade(0f, 0.4f));
        if (targetUI != null)
            seq.Join(targetUI.DOFade(0f, 0.4f));

        seq.OnComplete(() =>
        {
            allExternalItems.Remove(item);
            Destroy(item.gameObject);
            if (targetUI != null)
                Destroy(targetUI.gameObject);
        });
    }

    private void OnGameComplete()
    {
        gameCompleted = true;
        Sequence seq = DOTween.Sequence();
        //seq.Join(bagText1.transform.DOScale(0f, startDuration).SetEase(Ease.OutExpo));
        //seq.Join(bagText2.transform.DOScale(1f, startDuration).SetEase(Ease.OutExpo));

        //下一关
        OnGrowthComplete();
    }
    private void OnGrowthComplete()
    {
        Debug.Log("成长完成，切换至下一关卡");

        if (Scene5Manage.Instance != null)
        {
            // 假设 changeDelay = 2f，相机切换完成后启动第二关
            Scene5Manage.Instance.ChangeCamera(nextLevelIndex, changeDelay, () =>
            {
                // 在关卡初始化时播放环境音
                envSourse = AudioManager.Instance.PlayLoopingSound(envClip, loop: true, volumeScale: 0.3f);

                envSourse.transform.position = envClipPos.position;  // 例如 new Vector3(10, 5, 0)
                envSourse.minDistance = 8f;   //距离音源 5 米内音量最大
                envSourse.maxDistance = 17f;  //30米外几乎听不到
                envSourse.rolloffMode = AudioRolloffMode.Linear;
            });
        }
        else
        {
            Debug.LogError("Scene4Manage.Instance 不存在，请确保场景中有 Scene4Manage 组件");
        }
    }

    // 检测是否在书包内
    public bool IsInBagArea(Vector3 pos)
    {
        return bagArea != null && bagArea.OverlapPoint(pos);
    }

    // 检测是否在合法外部区域
    public bool IsInExternalArea(Vector3 pos)
    {
        return externalArea != null && externalArea.OverlapPoint(pos);
    }
    private void OnDestroy()
    {
        AudioManager.Instance.StopLoopingSound(envSourse);
    }
}