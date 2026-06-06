using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ReversalMange : MonoBehaviour
{
    public static ReversalMange Instance;
    public int currentLevel = 1;//当前的关卡

    public FlipMirrorController flipMirrorController;
    [Header("地图层级1")]
    public GameObject level1OuterLayer;// 外层线稿（有碰撞体，不可见但碰撞体有效）
    public GameObject level1MiddleLayer;// 中层正常场景（障碍物）
    public GameObject level1InnerLayer;// 内层透视提示（文字等）
    [Header("地图层级2")]
    public GameObject level2OuterLayer;// 外层线稿（有碰撞体，不可见但碰撞体有效）
    public GameObject level2MiddleLayer;// 中层正常场景（障碍物）
    public GameObject level2InnerLayer;//内层透视提示（文字等）
    [Header("地图层级3")]
    public GameObject level3OuterLayer;//外层线稿（有碰撞体，不可见但碰撞体有效）
    public GameObject level3MiddleLayer;//中层正常场景（障碍物）
    public GameObject level3InnerLayer;//内层透视提示（文字等）


    [Header("交互相关")]
    public SpriteRenderer showInteractionObject1;//第一关获得金币后的呈现物品（修改图片）
    public Sprite sprite1;//切换的图片
    public SpriteRenderer showInteractionObject2;//第二关获得金币后的呈现物品（修改图片）
    public Sprite sprite2;
    public SpriteRenderer showInteractionObject3;//第三关获得金币后的呈现物品（修改图片）
    public Sprite sprite3;
    [Header("交互检测")]
    public int requiredInteractions = 3;//本关所需交互数量
    private int completedInteractions = 0;//已完成交互数
    public bool canInteractItems = false;
    [Header("交互显示")]
    public GameObject[] interactPrompt1;//第一层提示
    public GameObject[] interactPrompt2;//第二层提示
    public GameObject[] interactPrompt3;//第三层提示

    [Header("金币")]
    public GameObject coinPrefab;                 //金币预制体
    public Transform currentcoinSpawnPoint;       //金币生成位置（可在场景中放一个空物体）
    public Transform coinSpawnPoint1;             //金币生成位置（可在场景中放一个空物体）
    public Transform coinSpawnPoint2;             //金币生成位置（可在场景中放一个空物体）
    public Transform coinSpawnPoint3;             //金币生成位置（可在场景中放一个空物体）
    private GameObject spawnedCoin;               //生成的金币实例
    private bool coinSpawned = false;             //是否已生成金币

    [Header("传送门")]
    public Portal currentPortal;//当前关卡的传送门
    public Portal portal1;//传送门1 完成交互后开启
    public Portal portal2;//传送门2 完成交互后开启
    public Portal portal3;//传送门3 完成交互后开启

    [Header("摄像机")]
    public CinemachineVirtualCamera currentCinemachine;
    public CinemachineVirtualCamera nextCinemachine;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if(TransitionManage.Instance!=null)
        {
            TransitionManage.Instance.FadeIn(1f,Color.black);
        }
        currentLevel = 1;
        InitLevel(currentLevel);
    }
    //初始化配置
    public void InitLevel(int level)
    {
        Debug.Log("切换关卡"+level);
        // 销毁上一层的金币（如果存在）
        if (spawnedCoin != null)
        {
            Destroy(spawnedCoin);
            spawnedCoin = null;
        }
        // 清空 CoinManage 中的金币列表，避免残留
        if (CoinManage.instance != null)
        {
            CoinManage.instance.ClearCoins();
        }
        canInteractItems = false;   // 重置物品交互权限
        currentLevel = level;   // 更新当前关卡编号
        completedInteractions = 0;
        coinSpawned = false;
        EnableItemHints(currentLevel,false);
        switch (level)
        {
            case 1:
                currentPortal = portal1;
                currentcoinSpawnPoint = coinSpawnPoint1;
                flipMirrorController.InitMirror(level1OuterLayer,level1MiddleLayer,level1InnerLayer);
                break;
            case 2:
                currentPortal = portal2;
                currentcoinSpawnPoint = coinSpawnPoint2;
                flipMirrorController.InitMirror(level2OuterLayer, level2MiddleLayer, level2InnerLayer);

                break;
            case 3:
                currentPortal = portal3;
                currentcoinSpawnPoint = coinSpawnPoint3;
                flipMirrorController.InitMirror(level3OuterLayer, level3MiddleLayer, level3InnerLayer);
                requiredInteractions = 2;
                canInteractItems = true;
                // 可选：同步显示/隐藏所有物品的交互提示（见下方说明）
                EnableItemHints(currentLevel, true);
                break;
        }
        if (currentPortal != null)
            currentPortal.canTeleport = false;
    }
    public IEnumerator SetItemsInteractable(bool value)
    {
        yield return new WaitForSeconds(1f);
        canInteractItems = value;
        // 可选：同步显示/隐藏所有物品的交互提示（见下方说明）
        EnableItemHints(currentLevel,value);
    }
    // 可选：控制场景中所有物品的提示显示（如果每个物品都挂载了 InteractableItem）
    private void EnableItemHints(int level,bool enable)
    {
        switch (level)
        {
            case 1:
               for(int i=0;i<3;i++)
                {
                    interactPrompt1[i].SetActive(enable);
                    interactPrompt1[i].GetComponent<EFloating>().StartFloating(enable);
                }
                break;
            case 2:
                for (int i = 0; i < 3; i++)
                {
                    interactPrompt2[i].SetActive(enable);
                    interactPrompt2[i].GetComponent<EFloating>().StartFloating(enable);
                }
                break;
            case 3:
                for (int i = 0; i < 2; i++)
                {
                    interactPrompt3[i].SetActive(enable);
                    interactPrompt3[i].GetComponent<EFloating>().StartFloating(enable);
                }
                break;
        }
    }
    public void RegisterInteraction()
    {
        if (completedInteractions >= requiredInteractions) return;
        completedInteractions++;
        Debug.Log($"交互进度：{completedInteractions}/{requiredInteractions}");

        // 达到指定数量，生成金币
        if (completedInteractions == requiredInteractions && !coinSpawned)
        {
            //完成数量改变外观
            //UpdateInteractionAppearance();
            SpawnCoin();
        }
    }
    //private void UpdateInteractionAppearance()
    //{
    //    switch(currentLevel)
    //    {
    //        case 1://切换第一层的交互图片
    //            showInteractionObject1.sprite = sprite1; break;
    //        case 2://切换第二层的交互图片
    //            showInteractionObject2.sprite = sprite2; break;
    //        case 3://切换第三层的交互图片
    //            showInteractionObject3.sprite = sprite3; break;
    //    }
    //}
    private void SpawnCoin()
    {
        SpriteRenderer gameObject = null;
        Sprite sprite = null;
        switch(currentLevel)
        {
            case 1://切换第一层的交互图片
                gameObject = showInteractionObject1;
                    sprite = sprite1; break;
            case 2://切换第二层的交互图片
                gameObject = showInteractionObject2;
                    sprite = sprite2; break;
            case 3://切换第三层的交互图片
                gameObject = showInteractionObject3;
                    sprite = sprite3; break;
        }

        coinSpawned = true;
        if (coinPrefab != null && currentcoinSpawnPoint != null)
        {
            spawnedCoin = Instantiate(coinPrefab, currentcoinSpawnPoint.position, Quaternion.identity);
            //确保金币的 Coin 脚本能正确通知管理器
            Coin coinScript = spawnedCoin.GetComponent<Coin>();
            if (coinScript != null)
            {
                coinScript.currentInteractionObject = gameObject;
                coinScript.nextSprite = sprite;
                coinScript.SetIsLevelCoin(true); // 标记这是关卡完成金币（新增方法）
            }
        }
        else
        {
            Debug.LogWarning("未设置金币预制体或生成点，无法生成金币！");
        }
    }

    // 玩家拾取金币后调用
    public void OnLevelCoinCollected()
    {
        if (currentPortal != null)
        {
            currentPortal.canTeleport = true;
            Debug.Log("金币已拾取，传送门已开启！");
        }
        else
        {
            Debug.LogWarning("未设置当前传送门！");
        }
    }
    public void ChangeCinemachine(CinemachineVirtualCamera camera)
    {
        nextCinemachine = camera;
        currentCinemachine.Priority = 10;
        nextCinemachine.Priority = 20;
        //切换成功
        currentCinemachine = nextCinemachine;
        nextCinemachine = null;
    }

}
