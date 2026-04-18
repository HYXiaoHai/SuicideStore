using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoinManage : MonoBehaviour
{
    public static CoinManage instance;
    [Header("引用")]
    public GameObject player;          // 玩家物体
    public List<Coin>coins = new List<Coin>();               // 场景中所有金币（可手动拖拽，也可自动查找）

    [Header("偏移设置")]
    public Vector3 baseOffset = new Vector3(-1.5f, 0f, 0f);   // 金币1相对于玩家的偏移（如左侧1.5单位）
    public Vector3 chainOffset = new Vector3(-0.5f, 0f, 0f);  // 后续金币相对于前一个金币的额外偏移（可选）

    private Vector3 originalBaseOffset;
    private Vector3 originalChainOffset;

    public int originalDic;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (coins.Count == 0)
        {
            return;
        }

        originalBaseOffset = baseOffset;
        originalChainOffset = chainOffset;

        //初始朝向为右，使用方向 = 1
        originalDic = 1;
        SetupChains(1);
    }
    private void SetupChains(int direction)
    {
        // 方向为 1（右）时偏移保持原样（负X在左边）
        // 方向为 -1（左）时偏移X取反（正X在右边）
        Debug.Log(originalBaseOffset + " " + originalChainOffset);
        Vector3 actualBaseOffset = new Vector3(baseOffset.x * direction, originalBaseOffset.y, originalBaseOffset.z);
        Vector3 actualChainOffset = new Vector3(chainOffset.x * direction, originalChainOffset.y, originalChainOffset.z);
        for (int i = 0; i < coins.Count; i++)
        {
            if (i == 0)
            {
                coins[i].trackObject = player;
                coins[i].offset = actualBaseOffset;
            }
            else
            {
                coins[i].trackObject = coins[i - 1].gameObject;
                coins[i].offset = actualChainOffset;
            }
        }
    }
    public void AddCoin(Coin coin)
    {
        coins.Add(coin);
        UpdateDirection(originalDic);
    }
    public void UpdateDirection(int direction)
    {
        originalDic = direction;
        SetupChains(direction);
    }
}
