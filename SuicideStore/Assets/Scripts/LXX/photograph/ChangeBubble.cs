using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChangeBubble : MonoBehaviour
{
    public Image bubble;                // 气泡图片
    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite3;
    public Sprite spriteSuccess;        // 获胜图片

    public float aniDuration = 0.5f;    // 缩放/淡入淡出动画时长
    public float showDuration = 1f;     // 每张图片显示停留时间

    public bool isSuccess = false;      // 是否获胜（获胜后停止循环）
    public bool isStart = false;        // 是否开始

    private int currentIndex = 0;      
    private Coroutine loopCoroutine;
    private Sequence currentSequence;   //当前正在播放的序列

    void Start()
    {
        // 初始隐藏
        bubble.transform.localScale = Vector3.zero;
        bubble.sprite = sprite1;
    }

    // 外部调用：开始游戏，启动循环
    public void StartGame()
    {
        if (isStart) return;
        isStart = true;
        isSuccess = false;

        // 重置当前索引
        currentIndex = 0;
        // 取消之前的协程
        if (loopCoroutine != null) StopCoroutine(loopCoroutine);
        // 停止当前动画
        if (currentSequence != null && currentSequence.IsActive()) currentSequence.Kill();

        // 开始循环协程
        loopCoroutine = StartCoroutine(BubbleLoop());
    }

    // 外部调用：停止循环（例如游戏结束）
    public void StopGame()
    {
        isStart = false;
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        if (currentSequence != null && currentSequence.IsActive()) currentSequence.Kill();
        // 不立即隐藏，由调用方决定
    }

    public void GameWin()
    {
        if (isSuccess) return;
        isSuccess = true;
        isStart = false;

        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        if (currentSequence != null && currentSequence.IsActive()) currentSequence.Kill();

        if (bubble.gameObject.activeSelf)
        {
            bubble.transform.DOKill();
            bubble.transform.DOScale(Vector3.zero, aniDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                bubble.gameObject.SetActive(false);
            });
        }
        else
        {
            bubble.gameObject.SetActive(false);
        }
    }

    IEnumerator BubbleLoop()
    {
        // 先显示第一张（sprite1）
        yield return ShowCurrentBubble();

        // 然后循环切换
        while (isStart && !isSuccess)
        {
            // 切换到下一张（索引循环 0->1->2->0...）
            currentIndex = (currentIndex + 1) % 3;
            yield return ShowCurrentBubble();
        }
    }

    IEnumerator ShowCurrentBubble()
    {
        // 根据当前索引设置图片
        switch (currentIndex)
        {
            case 0: bubble.sprite = sprite1; break;
            case 1: bubble.sprite = sprite2; break;
            case 2: bubble.sprite = sprite3; break;
            default: bubble.sprite = sprite1; break;
        }

        // 1. 缩放从0到1（出现）
        bubble.transform.localScale = Vector3.zero;
        // 使用 DOTween 序列控制顺序：缩放出现 → 等待 → 淡出
        Sequence seq = DOTween.Sequence();
        seq.Append(bubble.transform.DOScale(Vector3.one, aniDuration).SetEase(Ease.OutBack));
        seq.AppendInterval(showDuration);
        seq.Append(bubble.DOFade(0f, aniDuration).SetEase(Ease.InQuad));
        seq.OnComplete(() =>
        {
            // 重置透明度为1（为下次显示准备）
            Color c = bubble.color;
            c.a = 1f;
            bubble.color = c;
        });

        // 保存当前序列引用以便外部中止
        currentSequence = seq;
        yield return seq.WaitForCompletion();
        currentSequence = null;
    }

    // 立即隐藏（用于停止时）
    void HideBubbleImmediate()
    {
        if (bubble.gameObject.activeSelf)
        {
            bubble.transform.localScale = Vector3.zero;
            Color c = bubble.color;
            c.a = 1f;
            bubble.color = c;
        }
    }
}