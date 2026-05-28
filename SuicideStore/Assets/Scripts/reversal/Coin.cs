using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [Header("链条设置")]
    public int id;                 // 用于排序，0=第一个金币
    public GameObject trackObject; // 要跟随的目标（玩家或前一个金币）
    public Vector3 offset;         // 相对于目标的偏移量

    [Header("跟随参数")]
    public float followDuration = 0.2f;   // 移动动画时长（秒）
    public Ease followEase = Ease.Linear; // 缓动曲线

    [Header("捡起金币后修改的图片")]
    public SpriteRenderer currentInteractionObject;//第一关获得金币后的呈现物品（修改图片）
    public Sprite nextSprite;//切换的图片

    [Header("状态")]
    public bool canTrack = false;

    private Tweener moveTweener;   // 当前移动动画的引用
    private bool isCollected = false;          // 防止重复触发
    private bool isLevelCoin = false;           // 是否是关卡完成金币
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !canTrack)
        {
            isCollected = true;

            // 如果是关卡完成金币，通知管理器
            if (isLevelCoin && ReversalMange.Instance != null)
            {
                currentInteractionObject.sprite = nextSprite;
                ReversalMange.Instance.OnLevelCoinCollected();
            }
            Add();
            StartTrack();
        }
    }

    public void Add()
    {
        CoinManage.instance.AddCoin(gameObject.GetComponent<Coin>());
    }
    public void StartTrack()
    {
        canTrack = true;
    }

    /// <summary>
    /// 外部调用：设置追踪目标和偏移，并开始追踪
    /// </summary>
    public void SetTrackTarget(GameObject target, Vector3 off)
    {
        trackObject = target;
        offset = off;
    }
    // 外部调用，标记这是关卡完成金币
    public void SetIsLevelCoin(bool value)
    {
        isLevelCoin = value;
    }
    void Update()
    {
        if (!canTrack || trackObject == null) return;

        // 目标位置 = 追踪对象的位置 + 偏移
        Vector3 targetPos = trackObject.transform.position + offset;
        // 如果已有移动动画，先杀死，避免冲突
        if (moveTweener != null && moveTweener.IsActive())
            moveTweener.Kill();

        // 用 DOTween 平滑移动到目标位置
        moveTweener = transform.DOMove(targetPos, followDuration)
                              .SetEase(followEase);
    }
    private void OnDestroy()
    {
        if (moveTweener != null && moveTweener.IsActive())
            moveTweener.Kill();
    }
}