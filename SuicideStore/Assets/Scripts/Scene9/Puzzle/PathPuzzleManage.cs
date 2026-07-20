using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class PathPuzzleManage : MonoBehaviour
{
    public static PathPuzzleManage Instance;

    [Header("区域设置")]
    public List<Collider2D> externalSlots;
    private List<PathPuzzle> orderedPieces;

    [Header("拼图列表")]
    public List<PathPuzzle> allItems;

    private bool gameCompleted = false;
    public bool isGameStarted = false;
    private bool isMoving = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        orderedPieces = new List<PathPuzzle>(new PathPuzzle[externalSlots.Count]);
        foreach (var item in allItems)
        {
            int slotIndex = GetSlotIndexOfPosition(item.transform.position);
            if (slotIndex != -1)
            {
                orderedPieces[slotIndex] = item;
                item.currentIndex = slotIndex;
            }
            else
                Debug.LogWarning($"拼图 {item.name} 不在任何卡槽内！");
            item.trueIndex = item.id;
        }
        ShufflePieces();
    }

    public void StartGame()
    {
        if (gameCompleted || !isGameStarted)
        {
            gameCompleted = false;
            isGameStarted = true;
            Debug.Log("游戏已开始");
        }
    }

    public void ResetGame()
    {
        gameCompleted = false;
        isGameStarted = false;
        ShufflePieces();
        Debug.Log("游戏已重置");
    }

    public int GetSlotIndexOfPosition(Vector3 pos)
    {
        for (int i = 0; i < externalSlots.Count; i++)
            if (externalSlots[i] != null && externalSlots[i].OverlapPoint(pos))
                return i;
        return -1;
    }

    public Vector3 GetSlotPosition(int index) => externalSlots[index].transform.position;

    public int GetCurrentIndex(PathPuzzle piece) => orderedPieces.IndexOf(piece);

    // 核心插入方法
    public void RequestInsert(PathPuzzle piece, int targetPieceIndex, bool insertAfter = false, bool excludeDragged = false)
    {
        if (!isGameStarted || gameCompleted) return;

        // 如果正在移动，停止其他拼图的动画
        if (isMoving)
        {
            foreach (var p in orderedPieces)
            {
                if (p != null && p != piece)
                    p.StopAllTweens();
            }
            isMoving = false;
        }

        int fromIndex = GetCurrentIndex(piece);
        if (fromIndex == -1) return;

        int targetSlotIndex = insertAfter ? targetPieceIndex + 1 : targetPieceIndex;
        targetSlotIndex = Mathf.Clamp(targetSlotIndex, 0, orderedPieces.Count);

        // 移除并插入到新位置
        orderedPieces.RemoveAt(fromIndex);
        int insertIndex = (targetSlotIndex > fromIndex) ? targetSlotIndex - 1 : targetSlotIndex;
        orderedPieces.Insert(insertIndex, piece);
        piece.currentIndex = insertIndex;

        isMoving = true;
        MoveAllPiecesToSlots(excludeDragged ? piece : null, () =>
        {
            isMoving = false;
            if (!excludeDragged)
                CheckVictory();
        });
    }

    // 移动所有拼图到插槽，排除指定的拼图
    private void MoveAllPiecesToSlots(PathPuzzle excludePiece = null, TweenCallback onComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < orderedPieces.Count; i++)
        {
            PathPuzzle piece = orderedPieces[i];
            if (piece == null || piece == excludePiece) continue;
            piece.currentIndex = i;
            Vector3 targetPos = externalSlots[i].transform.position;
            piece.StopAllTweens();          // 强制停止旧动画
            seq.Join(piece.MoveToPosition(targetPos, 0.2f));
        }
        if (onComplete != null) seq.OnComplete(onComplete);
        seq.Play();
    }

    // 吸附单个拼图到对应插槽
    public void SnapPieceToSlot(PathPuzzle piece)
    {
        int idx = GetCurrentIndex(piece);
        if (idx != -1)
        {
            Vector3 slotPos = GetSlotPosition(idx);
            piece.StopAllTweens();
            piece.MoveToPosition(slotPos, 0.2f);
        }
    }

    private void CheckVictory()
    {
        for (int i = 0; i < orderedPieces.Count; i++)
            if (orderedPieces[i].trueIndex != orderedPieces[i].currentIndex) return;
        gameCompleted = true;
        isGameStarted = false;
        OnGameComplete();
    }

    private void OnGameComplete()
    {
        gameCompleted = true;
        Scene9Maneg.Instance.OnPuzzleCompleted();
    }

    public void ShufflePieces()
    {
        for (int i = 0; i < orderedPieces.Count; i++)
        {
            int rand = Random.Range(i, orderedPieces.Count);
            var temp = orderedPieces[i];
            orderedPieces[i] = orderedPieces[rand];
            orderedPieces[rand] = temp;
        }
        MoveAllPiecesToSlots(null, () => { });
    }

    //public static PathPuzzleManage Instance;

    //[Header("区域设置")]
    //public List<Collider2D> externalSlots;
    //private List<PathPuzzle> orderedPieces;

    //[Header("拼图列表")]
    //public List<PathPuzzle> allItems;

    //private bool gameCompleted = false;
    //public bool isGameStarted = false;   // 初始不可拖拽
    //private bool isMoving = false;


    //void Awake()
    //{
    //    Instance = this;
    //}

    //void Start()
    //{
    //    orderedPieces = new List<PathPuzzle>(new PathPuzzle[externalSlots.Count]);
    //    foreach (var item in allItems)
    //    {
    //        int slotIndex = GetSlotIndexOfPosition(item.transform.position);
    //        if (slotIndex != -1)
    //        {
    //            orderedPieces[slotIndex] = item;
    //            item.currentIndex = slotIndex;
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"拼图 {item.name} 不在任何卡槽内！");
    //        }
    //        item.trueIndex = item.id;
    //    }
    //    // 初始打乱但不可拖拽（仅显示乱序）
    //    ShufflePieces();
    //}

    ///// <summary>
    ///// 开始游戏：重置状态、打乱顺序、允许拖拽
    ///// </summary>
    //public void StartGame()
    //{
    //    if (gameCompleted || !isGameStarted)
    //    {
    //        gameCompleted = false;
    //        isGameStarted = true;
    //        // 重新打乱顺序（也可以选择不打乱，根据需求）
    //        //ShufflePieces();
    //        Debug.Log("游戏已开始，可以拖拽拼图");
    //    }
    //}

    ///// <summary>
    ///// 完全重置游戏（可用于重新开始）
    ///// </summary>
    //public void ResetGame()
    //{
    //    gameCompleted = false;
    //    isGameStarted = false;
    //    ShufflePieces();
    //    Debug.Log("游戏已重置，需要调用 StartGame() 才能开始");
    //}

    //public int GetSlotIndexOfPosition(Vector3 pos)
    //{
    //    for (int i = 0; i < externalSlots.Count; i++)
    //        if (externalSlots[i] != null && externalSlots[i].OverlapPoint(pos))
    //            return i;
    //    return -1;
    //}

    //public Vector3 GetSlotPosition(int index) => externalSlots[index].transform.position;

    //public int GetCurrentIndex(PathPuzzle piece) => orderedPieces.IndexOf(piece);

    //public void RequestInsert(PathPuzzle piece, int targetPieceIndex, bool insertAfter = false)
    //{
    //    if (!isGameStarted || gameCompleted || isMoving) return;

    //    int fromIndex = GetCurrentIndex(piece);
    //    if (fromIndex == -1) return;

    //    int targetSlotIndex = insertAfter ? targetPieceIndex + 1 : targetPieceIndex;
    //    targetSlotIndex = Mathf.Clamp(targetSlotIndex, 0, orderedPieces.Count);

    //    orderedPieces.RemoveAt(fromIndex);
    //    int newIndex = (fromIndex < targetSlotIndex) ? targetSlotIndex - 1 : targetSlotIndex;
    //    orderedPieces.Insert(newIndex, piece);

    //    isMoving = true;
    //    MoveAllPiecesToSlots(() =>
    //    {
    //        isMoving = false;
    //        CheckVictory();
    //    });
    //}

    //private void MoveAllPiecesToSlots(TweenCallback onComplete = null)
    //{
    //    Sequence seq = DOTween.Sequence();
    //    for (int i = 0; i < orderedPieces.Count; i++)
    //    {
    //        PathPuzzle piece = orderedPieces[i];
    //        if (piece == null) continue;
    //        piece.currentIndex = i;
    //        Vector3 targetPos = externalSlots[i].transform.position;
    //        if (Vector3.Distance(piece.transform.position, targetPos) > 0.01f)
    //            seq.Join(piece.MoveToPosition(targetPos, 0.2f));
    //        else
    //            piece.SetOriginalPosition(targetPos);
    //    }
    //    if (onComplete != null) seq.OnComplete(onComplete);
    //    seq.Play();
    //}

    //public void SnapPieceToSlot(PathPuzzle piece)
    //{
    //    int idx = GetCurrentIndex(piece);
    //    if (idx != -1)
    //    {
    //        Vector3 slotPos = GetSlotPosition(idx);
    //        piece.MoveToPosition(slotPos, 0.2f);
    //    }
    //}

    //private void CheckVictory()
    //{
    //    for (int i = 0; i < orderedPieces.Count; i++)
    //        if (orderedPieces[i].trueIndex != orderedPieces[i].currentIndex) return;
    //    gameCompleted = true;
    //    isGameStarted = false;
    //    OnGameComplete();
    //}

    //private void OnGameComplete()
    //{
    //    gameCompleted = true;
    //    Scene9Maneg.Instance.OnPuzzleCompleted();
    //}

    //public void ShufflePieces()
    //{
    //    for (int i = 0; i < orderedPieces.Count; i++)
    //    {
    //        int rand = Random.Range(i, orderedPieces.Count);
    //        var temp = orderedPieces[i];
    //        orderedPieces[i] = orderedPieces[rand];
    //        orderedPieces[rand] = temp;
    //    }
    //    MoveAllPiecesToSlots(() => { });
    //}
}