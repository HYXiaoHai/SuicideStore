using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PencilDrawMask : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform pencilRt;
    public Image maskImage;
    public Text storyText;
    public BrokenPhotoClick photoClick;

    private Texture2D maskTex;
    private Vector2 lastPos;
    private bool isDrawing = false;

    void Start()
    {
        maskTex = new Texture2D(Screen.width, Screen.height, TextureFormat.Alpha8, false);
        maskImage.sprite = Sprite.Create(maskTex, new Rect(0, 0, maskTex.width, maskTex.height), Vector2.one * 0.5f);

        Color clear = Color.clear;
        for (int x = 0; x < maskTex.width; x++)
            for (int y = 0; y < maskTex.height; y++)
                maskTex.SetPixel(x, y, clear);
        maskTex.Apply();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastPos = eventData.position;
        isDrawing = true;
        if (photoClick != null)
            photoClick.OnDrawingStarted();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDrawing) return;

        pencilRt.anchoredPosition += eventData.delta;
        DrawLine(lastPos, eventData.position, 8, Color.white);
        lastPos = eventData.position;
        maskTex.Apply();
        storyText.maskable = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrawing = false;
        if (photoClick != null)
            photoClick.OnDrawingComplete();
    }

    void DrawLine(Vector2 p1, Vector2 p2, int width, Color col)
    {
        float t = 0;
        while (t < 1)
        {
            Vector2 pos = Vector2.Lerp(p1, p2, t);
            int x = (int)pos.x;
            int y = (int)pos.y;
            for (int ox = -width; ox <= width; ox++)
                for (int oy = -width; oy <= width; oy++)
                    maskTex.SetPixel(x + ox, y + oy, col);
            t += 0.01f;
        }
    }
}