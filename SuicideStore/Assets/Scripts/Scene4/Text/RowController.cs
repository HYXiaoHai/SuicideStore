using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class RowController : MonoBehaviour
{
    [Header("碰撞体")]
    public Collider2D rowCollider;//文字的碰撞体
    public Collider2D lineCollider;//线的碰撞体

    [Header("蓝色图片（上层）")]
    public Image blueImage;
    public Image blackImage;

    [Header("左右边界（世界坐标）")]
    public Transform leftBound;
    public Transform rightBound;
    public Transform lineLeftBound;  // 横线左边界（用于左移终点）
    public Transform lineRightBound; // 横线右边界（下落起始点）
    private float leftX, rightX, lineLeftX, lineRightX;
    private float width;

    void Start()
    {
        if (blueImage == null)
            blueImage = GetComponentInChildren<Image>(); // 自动查找，但最好手动拖

        // 确保 BlueImage 是 Filled 模式且 Origin Left
        blueImage.type = Image.Type.Filled;
        blueImage.fillMethod = Image.FillMethod.Horizontal;
        blueImage.fillOrigin = (int)Image.OriginHorizontal.Right;
        blueImage.fillAmount = 1f; // 全蓝

        // 确保 BlueImage 是 Filled 模式且 Origin Left
        blackImage.type = Image.Type.Filled;
        blackImage.fillMethod = Image.FillMethod.Horizontal;
        blackImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        blackImage.fillAmount = 0f; // 全蓝



        // 获取边界世界坐标
        leftX = leftBound.position.x;
        rightX = rightBound.position.x;
        lineLeftX = lineLeftBound.position.x;
        lineRightX = lineRightBound.position.x;

        // 初始状态：开启文字碰撞体，关闭横线碰撞体
        rowCollider.enabled = true;
        lineCollider.enabled = false;
        width = rightX - leftX;
    }
    // 获取横线左边界 X
    public float GetLineLeftX() => lineLeftX;
    // 获取横线右边界 X
    public float GetLineRightX() => lineRightX;
    // 根据玩家 X 坐标更新 fillAmount
    public void UpdateFillByPlayerX(float playerX)
    {
        float progress = Mathf.Clamp01((playerX - leftX) / (rightX - leftX));
        blueImage.fillAmount = 1f - progress;   // 蓝色从右向左消失
        blackImage.fillAmount = progress;       // 黑色从左向右填充
        Debug.Log("更新:"+ blackImage.fillAmount);
    }

    // 可选：强制完成（全黑）
    public void CompleteRow()
    {
        blueImage.fillAmount = 0f;
        blackImage.fillAmount = 1f;

        // 关闭文字碰撞体，开启横线碰撞体（准备横线行走）
        lineCollider.GetComponent<Image>().DOFade(1f, 0.5f);
        rowCollider.enabled = false;
        lineCollider.enabled = true;
    }
    public void CompleteLine()
    {
        lineCollider.enabled = false;   // 关闭横线碰撞体，让玩家掉落到下一行
    }
    public void ResetRow()
    {
        blueImage.fillAmount = 1f;
        blackImage.fillAmount = 0f;
        rowCollider.enabled = true;
        lineCollider.enabled = false;
    }

}