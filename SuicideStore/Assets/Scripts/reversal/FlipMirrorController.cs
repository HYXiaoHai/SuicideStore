using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipMirrorController : MonoBehaviour
{
    [Header("蒙版层")]
    public GameObject currentOuterLayer;// 外层线稿（有碰撞体，不可见但碰撞体有效）
    public GameObject currentMiddleLayer;// 中层正常场景（障碍物）
    public GameObject currentInnerLayer;// 内层透视提示（文字等）
    public bool isDefaultMode = true;//当前翻转镜模式
    [Header("翻转镜移动")]
    private Vector3 offset;
    private Camera mainCamera;
    public bool canMove = true;
    [Header("音效")]
    public AudioClip changeClip;
    [Header("玩家控制")]
    public ReversalPlayerController playerController;
    void Start()
    {
        mainCamera = Camera.main;

        isDefaultMode = true;
        canMove = isDefaultMode;
        ApplyMode(isDefaultMode);
    }
    public void InitMirror(GameObject outlayer,GameObject middlelayer,GameObject innerlayer)
    {
        currentOuterLayer = outlayer;
        currentMiddleLayer = middlelayer;
        currentInnerLayer = innerlayer;

        isDefaultMode = true;
        canMove = isDefaultMode;
        ApplyMode(isDefaultMode);
    }
    //跟随鼠标移动
    void Update()
    {
        //if (GameManage.Instance.isSetting) return;
        // 普通模式下镜子跟随鼠标
        if (canMove)
        {
            MirrorMove();
        }

        //切换模式
        if (Input.GetMouseButtonDown(0))
        {
            SwitchMode();
        }
        if (Input.GetMouseButtonUp(0))
        {
            SwitchMode();
        }
    }

    public void MirrorMove()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        transform.position = mouseWorld + offset;
    }
    public void SwitchMode()
    {
        AudioManager.Instance.Play2DSound(changeClip, 1f);
        isDefaultMode = !isDefaultMode;
        ApplyMode(isDefaultMode);

        canMove = isDefaultMode;

        // 控制玩家移动权限（拍照模式玩家不可操控）
        if (playerController != null)
            playerController.SetCanMove(isDefaultMode);

        // 拍照模式时间暂停（也可只禁用玩家输入，这里按需求暂停时间）
        Time.timeScale = isDefaultMode ? 1f : 0f;
    }

    void ApplyMode(bool defaultMode)
    {
        if (defaultMode == true)
        {
            //切回默认模式
            currentOuterLayer.SetActive(true);//开启外层图层
            currentOuterLayer.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

            currentMiddleLayer.SetActive(true);
            foreach (var item in currentMiddleLayer.GetComponentsInChildren<SpriteRenderer>())
            {
                item.maskInteraction = SpriteMaskInteraction.None;
            }
            //currentMiddleLayer.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;

            currentInnerLayer.SetActive(false);//关闭内层图层以及文字提示

        }
        else
        {
            //切回拍照模式
            currentOuterLayer.SetActive(false);//关闭外层图层

            currentMiddleLayer.SetActive(true);
            foreach (var item in currentMiddleLayer.GetComponentsInChildren<SpriteRenderer>())
            {
                item.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
            //currentMiddleLayer.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

            currentInnerLayer.SetActive(true);//
        }
    }
}