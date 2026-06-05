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

    private bool lastSettingState = false;
    private bool lastMouseState = false;
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
        bool isSetting = GameManage.Instance.isSetting;

        // 检测设置面板关闭的瞬间，根据当前鼠标状态强行更正模式
        if (lastSettingState && !isSetting)
        {
            // 刚刚从设置中恢复，同步鼠标状态
            bool mousePressed = Input.GetMouseButton(0);
            if (mousePressed && isDefaultMode)  // 应该处于拍照模式
            {
                ForceToPhotoMode();
            }
            else if (!mousePressed && !isDefaultMode) // 应该处于默认模式
            {
                ForceToDefaultMode();
            }
        }
        lastSettingState = isSetting;

        // 设置面板打开时，所有交互暂停
        if (isSetting) return;

        if (canMove)
            MirrorMove();

        // 实时检测鼠标按下/抬起，切换模式
        if (Input.GetMouseButtonDown(0))
        {
            SwitchMode();
        }
        else if (Input.GetMouseButtonUp(0))
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
    void ForceToDefaultMode()
    {
        if (!isDefaultMode)
        {
            isDefaultMode = true;
            ApplyMode(true);
            canMove = true;
            if (playerController != null)
                playerController.SetCanMove(true);
            Time.timeScale = 1f;
        }
    }

    void ForceToPhotoMode()
    {
        if (isDefaultMode)
        {
            isDefaultMode = false;
            ApplyMode(false);
            canMove = false;
            if (playerController != null)
                playerController.SetCanMove(false);
            Time.timeScale = 0f;
        }
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