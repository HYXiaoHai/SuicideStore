using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipMirrorController : MonoBehaviour
{
    [Header("蒙版层")]
    public GameObject currentMidleObject;//当前的翻转层
    public GameObject currentUpObject;//当前的翻转层
    public bool isUP;
    [Header("翻转镜移动")]
    private Vector3 offset;
    private Camera mainCamera;
    public bool canMove = true;

    public int isDefaultState  = 1;//当前翻转镜模式 1：默认模式  2：拍照模式
    void Start()
    {
        mainCamera = Camera.main;
    }

    //跟随鼠标移动
    void Update()
    {
        MirrorMove();
        if(Input.GetMouseButtonDown(0))
        {
            
            if (isUP)
            {
                currentUpObject.SetActive(false);
                currentMidleObject.SetActive(true);
                isUP = false;
                canMove = false;
                Time.timeScale = 0f;
            }
            else
            {
                currentUpObject.SetActive(true);
                currentMidleObject.SetActive(false);
                isUP = true;
                canMove = true;
                Time.timeScale = 1f;
            }
        }
    }

    public void MirrorMove()
    {
        if(canMove == false)
        {
            return;
        }
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        transform.position = mouseWorld + offset;
    }
}