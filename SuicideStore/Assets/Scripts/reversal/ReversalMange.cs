using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReversalMange : MonoBehaviour
{
    public static ReversalMange Instance;
    public int currentLevel = 1;//当前的关卡
    
    [Header("交互相关")]
    public GameObject showInteractionObject1;//第一关获得所有金币后的呈现物品
    public GameObject showInteractionObject2;//第二关获得所有金币后的呈现物品
    public GameObject showInteractionObject3;//第三关获得所有金币后的呈现物品

    [Header("传送门")]
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

    void Update()
    {
        
    }
}
