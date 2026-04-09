using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragByRaycast : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private bool isTouchPlayer = false;   // 是否与 Player 接触

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            transform.position = mouseWorld + offset;
    }

    //// 碰撞进入时，记录接触状态
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    //if (collision.gameObject.CompareTag("Player"))
    //    //{
    //    //    isTouchPlayer = true;
    //    //    Debug.Log("碰到了" + isTouchPlayer);
    //    //    // 如果拖动过程中碰到 Player，强制停止拖动（可选）
    //    //    if (isDragging)
    //    //    {
    //    //        isDragging = false;
    //    //    }
    //    //}
    //}

    //// 碰撞退出时，清除接触状态
    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    //Debug.Log("出去了");
    //    //if (collision.gameObject.CompareTag("Player"))
    //    //{
    //    //    isTouchPlayer = false;
    //    //}
    //}
}