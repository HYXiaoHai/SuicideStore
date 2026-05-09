using UnityEngine;

public class MaskPainter2D
{
    private RenderTexture _targetRT;
    private Camera _cam;
    private Transform _spriteTransform;

    public System.Action OnDraw;

    // 用于读取数据的临时小贴图和数组
    private Texture2D _readTexture;
    private const int CheckResolution = 64; // 检测分辨率，64x64 足够精准且性能好

    public MaskPainter2D(RenderTexture rt, Camera cam, Transform spriteTF)
    {
        _targetRT = rt;
        _cam = cam;
        _spriteTransform = spriteTF;
        _readTexture = new Texture2D(CheckResolution, CheckResolution, TextureFormat.R8, false);
    }

    public void DrawAtMousePosition(Material brushMat)
    {
        // 1. 获取鼠标屏幕坐标 (Pixels)
        Vector3 mousePos = Input.mousePosition;

        // 2. 转换到世界空间 (World Space)
        // 注意：2D 下 z 轴通常设为相对于相机的距离
        mousePos.z = Mathf.Abs(_cam.transform.position.z - _spriteTransform.position.z);
        Vector3 worldPos = _cam.ScreenToWorldPoint(mousePos);

        // 3. 转换到 Sprite 的本地坐标系 (Local Space)
        // 这一步非常关键，它会自动处理物体的 Position, Rotation, Scale
        Vector3 localPos = _spriteTransform.InverseTransformPoint(worldPos);

        // 4. 获取 Sprite 的实际尺寸 (以 Unit 为单位)
        // 如果你用的是默认的 Square，它的大小是 1x1，中心在 0,0
        // 如果你用了自定义图片，这里需要考虑 Sprite 的 bounds
        SpriteRenderer sr = _spriteTransform.GetComponent<SpriteRenderer>();
        Vector2 size = sr.sprite.bounds.size; // 获取图片的原始宽高

        // 5. 映射到 UV 空间 [0, 1]
        // 算法：(本地位置 / 总尺寸) + 中心偏移 (0.5)
        float u = (localPos.x / size.x) + 0.5f;
        float v = (localPos.y / size.y) + 0.5f;

        Vector2 uv = new Vector2(u, v);

        // 6. 范围检测与绘制
        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            DrawToRT(uv, brushMat);
            OnDraw?.Invoke();
        }
    }

    private void DrawToRT(Vector2 uv, Material brushMat)
    {
        // 核心步骤：使用 GL 在 RT 的指定 UV 位置绘制一个 Quad

        // 设置当前渲染的目标 RT
        RenderTexture.active = _targetRT;

        // 保存当前矩阵，切换到正交投影（UV 空间）
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, 1, 0, 1); // 映射到 0 到 1 的像素矩阵

        // 获取材质中设置的笔刷大小，确定 Quad 的绘制区域
        float size = brushMat.GetFloat("_Size");
        Rect rect = new Rect(uv.x - size, uv.y - size, size * 2, size * 2);

        // 使用笔刷材质，把白底纹理绘制到指定区域（关键是 Shader 里的 Blend 模式）
        Graphics.DrawTexture(rect, Texture2D.whiteTexture, brushMat);

        // 恢复矩阵，清空当前 RT
        GL.PopMatrix();
        RenderTexture.active = null;
    }

    /// </summary>
    public float GetDrawingProgress()
    {
        // 1. 创建一个临时的极小 RT 用于下采样
        RenderTexture tempRT = RenderTexture.GetTemporary(CheckResolution, CheckResolution, 0, RenderTextureFormat.R8);

        // 2. 将大的蒙版 RT 压缩到小的 RT 中 (GPU 完成，极快)
        Graphics.Blit(_targetRT, tempRT);

        // 3. 将小 RT 的数据读取到 CPU 可以访问的 Texture2D
        RenderTexture.active = tempRT;
        _readTexture.ReadPixels(new Rect(0, 0, CheckResolution, CheckResolution), 0, 0);
        _readTexture.Apply();
        RenderTexture.active = null;

        // 4. 释放临时资源
        RenderTexture.ReleaseTemporary(tempRT);

        // 5. 遍历像素计算白色占比
        Color32[] pixels = _readTexture.GetPixels32();
        int filledCount = 0;

        // 这里的 128 是阈值，R通道值大于 128 认为是被涂过了
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].r > 128) filledCount++;
        }

        return (float)filledCount / pixels.Length;
    }
}