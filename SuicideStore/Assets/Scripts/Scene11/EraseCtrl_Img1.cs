using UnityEngine;
using UnityEngine.UI;

public class EraseCtrl_Img1 : MonoBehaviour
{
    [Header("配置")]
    public RectTransform pencilRt;
    public Image maskBlack;
    public GameObject nextImg_Text;
    public float finishPercent = 0.8f;
    [Header("自定义擦除限定区域")]
    public RectTransform eraseLimitArea;

    private Texture2D maskTex;
    private RectTransform selfRt;
    private bool isFinish = false;

    void Start()
    {
        selfRt = GetComponent<RectTransform>();
        maskTex = new Texture2D((int)selfRt.rect.width, (int)selfRt.rect.height, TextureFormat.Alpha8, false);
        for(int x=0;x<maskTex.width;x++)
            for(int y=0;y<maskTex.height;y++)
                maskTex.SetPixel(x,y,Color.black);
        maskTex.Apply();
        maskBlack.sprite = Sprite.Create(maskTex,new Rect(0,0,maskTex.width,maskTex.height),Vector2.one*0.5f);
    }

    void Update()
    {
        if(isFinish || eraseLimitArea == null) return;

        bool inLimitArea = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            eraseLimitArea, Input.mousePosition, Camera.main, out var limitLocalPos);

        if(inLimitArea && Input.GetMouseButton(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRt,Input.mousePosition,Camera.main,out var imgLocal);
            Vector2 local = imgLocal + new Vector2(selfRt.rect.width/2,selfRt.rect.height/2);
            int px = (int)local.x;
            int py = (int)local.y;
            int brushSize = 6;
            for(int ox=-brushSize;ox<=brushSize;ox++)
                for(int oy=-brushSize;oy<=brushSize;oy++)
                {
                    int cx = Mathf.Clamp(px+ox,0,maskTex.width-1);
                    int cy = Mathf.Clamp(py+oy,0,maskTex.height-1);
                    maskTex.SetPixel(cx,cy,Color.white);
                }
            maskTex.Apply();
            UpdateMaskAlpha();
        }
    }

    void UpdateMaskAlpha()
    {
        int whiteCount=0;
        Color[] pix = maskTex.GetPixels();
        foreach(var c in pix) if(c.r>0.5f) whiteCount++;
        float rate = (float)whiteCount / pix.Length;
        
        maskBlack.color = new Color(1,1,1,1 - rate);
        
        if(rate >= finishPercent)
        {
            isFinish = true;
            maskBlack.color = new Color(1,1,1,0);
            gameObject.SetActive(false);
            nextImg_Text.SetActive(true);
        }
    }
}