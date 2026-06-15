using UnityEngine;

public class MaskPainter2D
{
    private RenderTexture _targetRT;
    private Camera _cam;
    private Transform _spriteTransform;

    public System.Action OnDraw;

    // ���ڶ�ȡ���ݵ���ʱС��ͼ������
    private Texture2D _readTexture;
    private const int CheckResolution = 64; // ���ֱ��ʣ�64x64 �㹻��׼�����ܺ�

    public MaskPainter2D(RenderTexture rt, Camera cam, Transform spriteTF)
    {
        _targetRT = rt;
        _cam = cam;
        _spriteTransform = spriteTF;
        _readTexture = new Texture2D(CheckResolution, CheckResolution, TextureFormat.R8, false);
    }

    public void DrawAtMousePosition(Material brushMat)
    {
        DrawAtWorldPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition), brushMat);
    }

    public void DrawAtWorldPosition(Vector3 worldPos, Material brushMat)
    {
        worldPos.z = _spriteTransform.position.z;
        
        Vector3 localPos = _spriteTransform.InverseTransformPoint(worldPos);

        SpriteRenderer sr = _spriteTransform.GetComponent<SpriteRenderer>();
        Vector2 size = sr.sprite.bounds.size;

        float u = (localPos.x / size.x) + 0.5f;
        float v = (localPos.y / size.y) + 0.5f;

        Vector2 uv = new Vector2(u, v);

        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            DrawToRT(uv, brushMat);
            OnDraw?.Invoke();
        }
    }

    private void DrawToRT(Vector2 uv, Material brushMat)
    {
        // ���Ĳ��裺ʹ�� GL �� RT ��ָ�� UV λ�û���һ�� Quad

        // ���õ�ǰ��Ⱦ��Ŀ�� RT
        RenderTexture.active = _targetRT;

        // ���浱ǰ�����л�������ͶӰ��UV �ռ䣩
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 0, 1); // ӳ�䵽 0 �� 1 �����ؾ���

        // ��ȡ���������õı�ˢ��С��ȷ�� Quad �Ļ�������
        float size = brushMat.GetFloat("_Size");
        Rect rect = new Rect(uv.x - size, uv.y - size, size * 2, size * 2);

        // ʹ�ñ�ˢ���ʣ��Ѱ׵��������Ƶ�ָ�����򣨹ؼ��� Shader ��� Blend ģʽ��
        Graphics.DrawTexture(rect, Texture2D.whiteTexture, brushMat);

        // �ָ�������յ�ǰ RT
        GL.PopMatrix();
        RenderTexture.active = null;
    }

    /// </summary>
    public float GetDrawingProgress()
    {
        // 1. ����һ����ʱ�ļ�С RT �����²���
        RenderTexture tempRT = RenderTexture.GetTemporary(CheckResolution, CheckResolution, 0, RenderTextureFormat.R8);

        // 2. ������ɰ� RT ѹ����С�� RT �� (GPU ��ɣ�����)
        Graphics.Blit(_targetRT, tempRT);

        // 3. ��С RT �����ݶ�ȡ�� CPU ���Է��ʵ� Texture2D
        RenderTexture.active = tempRT;
        _readTexture.ReadPixels(new Rect(0, 0, CheckResolution, CheckResolution), 0, 0);
        _readTexture.Apply();
        RenderTexture.active = null;

        // 4. �ͷ���ʱ��Դ
        RenderTexture.ReleaseTemporary(tempRT);

        // 5. �������ؼ����ɫռ��
        Color32[] pixels = _readTexture.GetPixels32();
        int filledCount = 0;

        // ����� 128 ����ֵ��Rͨ��ֵ���� 128 ��Ϊ�Ǳ�Ϳ����
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 128) filledCount++;
        }

        return (float)filledCount / pixels.Length;
    }
}