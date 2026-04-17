using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipMirrorController : MonoBehaviour
{
    [Header("蒙版层")]
    public GameObject currentMidleObject;//中间翻转层
    public GameObject currentUnderObject;//底部翻转层
    public GameObject currentUpObject;//上层翻转层
    public int isDefaultState = 1;//当前翻转镜模式 1：默认模式  -1：拍照模式
    [Header("翻转镜移动")]
    private Vector3 offset;
    private Camera mainCamera;
    public bool canMove = true;

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
            
            if (isDefaultState == -1)
            {
                //切回默认模式
                currentUpObject.SetActive(true);
                currentUpObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                currentMidleObject.SetActive(true);
                currentMidleObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
                currentUnderObject.SetActive(false);
                isDefaultState = 1;
                canMove = true;
                Time.timeScale = 1f;
            }
            else
            {
                //切回拍照模式
                currentUpObject.SetActive(false);
                currentMidleObject.SetActive(true);
                currentMidleObject.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                currentUnderObject.SetActive(true);
                isDefaultState = -1;
                canMove = false;
                Time.timeScale = 0f;
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