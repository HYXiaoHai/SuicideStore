using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestInteractable : BaseInteractable
{
    public SpriteRenderer nestRenderer;
    public Sprite newNestSprite; // 新贴图，如未指定则改变颜色
    public Color newColor = Color.green;
    public KiteController kite; // 关联的风筝

    public override void OnInteract()
    {
        // 修改鸟巢贴图
        if (nestRenderer != null)
        {
            if (newNestSprite != null)
                nestRenderer.sprite = newNestSprite;
            else
                nestRenderer.color = newColor;
        }

        // 启用风筝交互
        if (kite != null)
            kite.ActivateKite();

        // 可选：禁用再次点击
        GetComponent<Collider2D>().enabled = false;
    }
}
