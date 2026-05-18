using UnityEngine;

public class DrawingController : MonoBehaviour
{
    public int id;
    public bool isBegin = false;
    private bool hasDrawn = false;
    [Header("配置")]
    public Camera mainCamera;
    public SpriteRenderer targetSprite;
    public int rtWide = 1024;
    public int rtLong = 1024;

    [Header("阈值设置")]
    [Range(0f, 1f)] public float winThreshold = 0.9f; // 涂满 90% 算过关
    [Range(0f, 1f)] public float clearThreshold = 0.05f; // 剩下不到 5% 算擦除干净

    [Header("材质引用")]
    public Material pencilMaterial;
    public Material eraserMaterial;
    public Material revealMaterial;

    private RenderTexture _maskRT;
    public MaskPainter2D _painter;
    public ToolManager _toolManager;

    // 记录上一次是否已触发过“涂满”和“擦净”
    private bool hasTriggeredWin = false;
    private bool hasTriggeredClear = false;

    private float _currentProgress = 0f;
    // 对外公开：当前画板是否已完成（涂满或擦净任一条件满足）
    public bool IsCompleted => hasTriggeredWin;
    void Start()
    {
        //基础检查：防止没拖东西导致崩溃
        if (revealMaterial == null || targetSprite == null)
        {
            Debug.LogError("请在 Inspector 面板赋值材质和 Sprite！");
            return;
        }

        _maskRT = new RenderTexture(rtLong, rtWide, 0, RenderTextureFormat.R8);
        _maskRT.Create();

        RenderTexture.active = _maskRT;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = null;

        Material instanceMaterial = Instantiate(revealMaterial);
        instanceMaterial.SetTexture("_MaskTex", _maskRT);
        targetSprite.material = instanceMaterial;

        isBegin = false;

        _painter = new MaskPainter2D(_maskRT, mainCamera, targetSprite.transform);
     
        // 订阅绘制事件：第一次绘制时标记
        _painter.OnDraw += () => hasDrawn = true;
        _toolManager = new ToolManager(pencilMaterial, eraserMaterial, _painter);
        Debug.Log(gameObject + ":" + _toolManager);
    }

    void Update()
    {
        //如果 _toolManager 还没初始化成功（比如 Start 报错了），直接返回，避免每帧刷报错
        if (_toolManager == null || _painter == null) return;
        if (Input.GetMouseButtonDown(0)) _toolManager.StartDrawing();
        if (Input.GetMouseButtonUp(0))
        {
            _toolManager.StopDrawing();
            if(hasDrawn)
                CheckProgress();
        }

        _toolManager.DoUpdate();
    }

    void CheckProgress()
    {
        //_currentProgress = _painter.GetDrawingProgress();
        //Debug.Log($"当前画面完成度: {_currentProgress * 100:F2}%");

        ////根据关卡需求判断逻辑
        //if (_currentProgress >= winThreshold)
        //{
        //    Debug.Log(gameObject+"恭喜！涂满了！");
        //    DrawManage.instance.OnDrawAll();
        //    //这里执行切关逻辑
        //}
        //else if (_currentProgress <= clearThreshold)
        //{
        //    Debug.Log(gameObject+"擦除得很干净！");
        //    DrawManage.instance.OnEraseAll();
        //    //这里执行擦除任务成功的逻辑
        //}
        float progress = _painter.GetDrawingProgress();
        Debug.Log($"当前画面完成度: {progress * 100:F2}%");

        // 检查涂满条件
        if (progress >= winThreshold)
        {
            if (!hasTriggeredWin)   // 首次满足才触发
            {
                hasTriggeredWin = true;
                Debug.Log(gameObject + " 恭喜！涂满了！");
                DrawManage.instance.OnDrawAll();
            }
        }
        else
        {
            // 如果进度回落（例如擦除了一部分），重置标记，以便下次重新满足时可再次触发
            if (hasTriggeredWin) hasTriggeredWin = false;
        }
        // 检查擦净条件（注意：擦净和涂满可能同时满足？但通常不会，因为阈值不重叠）
        if (progress <= clearThreshold)
        {
            if (!hasTriggeredClear)
            {
                hasTriggeredClear = true;
                Debug.Log(gameObject + " 擦除得很干净！");
                DrawManage.instance.OnEraseAll();
            }
        }
        else
        {
            if (hasTriggeredClear) hasTriggeredClear = false;
        }
    }

    private void OnDestroy()
    {
        // 显式释放 RT 资源
        if (_maskRT != null) _maskRT.Release();
    }
}