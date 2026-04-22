using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KiteController : MonoBehaviour
{
    public bool isActive = false;
    public float flySpeed = 3f;
    public Vector2 flyDirection = Vector2.up;
    public float flyDistance = 15f;
    private Transform player;
    private bool isFlying = false;

    public void ActivateKite()
    {
        isActive = true;
        // 可选：添加视觉提示
        GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActive && !isFlying && other.CompareTag("Player"))
        {
            player = other.transform;
            isFlying = true;

            // 玩家与风筝同步移动
            player.SetParent(transform);
            StartCoroutine(FlyAway());
        }
    }

    System.Collections.IEnumerator FlyAway()
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + flyDirection * flyDistance;
        float elapsed = 0f;

        while (elapsed < flyDistance / flySpeed)
        {
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed * flySpeed / flyDistance);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log("场景飞出完成 - 可在此处加载下一关");
        // 这里可以触发关卡结束或转场动画
    }
}