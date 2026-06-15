using UnityEngine;

public class DrawingController : MonoBehaviour
{
    public int id;
    public bool isBegin = false;
    private bool hasDrawn = false;
    [Header("����")]
    public Camera mainCamera;
    public SpriteRenderer targetSprite;
    public int rtWide = 1024;
    public int rtLong = 1024;

    [Header("��ֵ����")]
    [Range(0f, 1f)] public float winThreshold = 0.9f; // Ϳ�� 90% �����
    [Range(0f, 1f)] public float clearThreshold = 0.05f; // ʣ�²��� 5% ������ɾ�

    [Header("��Ч")]
    public AudioClip drawingLoopClip;      // ����Ч��ѭ�����ţ�
    public AudioSource currentLoopingSound;
    private bool isActuallyDrawing; // ��Ǳ��λ��������Ƿ�����������
    [Header("��������")]
    public Material pencilMaterial;
    public Material eraserMaterial;
    public Material revealMaterial;

    [Header("画笔大小")]
    [Range(0.05f, 1f)] public float pencilSize = 0.2f;
    [Range(0.05f, 1f)] public float eraserSize = 0.4f;

    private RenderTexture _maskRT;
    public MaskPainter2D _painter;
    public ToolManager _toolManager;

    // ��¼��һ���Ƿ��Ѵ�������Ϳ�����͡�������
    private bool hasTriggeredWin = false;
    private bool hasTriggeredClear = false;

    private float _currentProgress = 0f;
    // ���⹫������ǰ�����Ƿ�����ɣ�Ϳ���������һ�������㣩
    public bool IsCompleted => hasTriggeredWin;
    void Start()
    {
        //������飺��ֹû�϶������±���
        if (revealMaterial == null || targetSprite == null)
        {
            Debug.LogError("���� Inspector ��帳ֵ���ʺ� Sprite��");
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

        // 设置画笔大小到材质
        if (pencilMaterial != null)
            pencilMaterial.SetFloat("_Size", pencilSize);
        if (eraserMaterial != null)
            eraserMaterial.SetFloat("_Size", eraserSize);

        // ���Ļ����¼�����һ�λ���ʱ���
        _painter.OnDraw += () =>
        {
            hasDrawn = true;
            // ��������ʱ�����ѭ����Чδ�����ҵ�ǰ������Ч��������
            if (drawingLoopClip != null && currentLoopingSound == null && _toolManager != null && _toolManager.currentType != ToolType.None)
            {
                currentLoopingSound = AudioManager.Instance.PlayLoopingSound(drawingLoopClip, true, 0.6f);
            }
        };
        _toolManager = new ToolManager(pencilMaterial, eraserMaterial, _painter);
        Debug.Log(gameObject + ":" + _toolManager);
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;

        if (_toolManager == null || _painter == null) return;
        if (Input.GetMouseButtonDown(0))
        {
            _toolManager.StartDrawing();
            isActuallyDrawing = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _toolManager.StopDrawing();
            if (currentLoopingSound != null)
            {
                AudioManager.Instance.StopLoopingSound(currentLoopingSound);
                currentLoopingSound = null;
            }
            hasDrawn = false;
        }
        if (hasDrawn)
            CheckProgress();
    }

    void CheckProgress()
    {
        //_currentProgress = _painter.GetDrawingProgress();
        //Debug.Log($"��ǰ������ɶ�: {_currentProgress * 100:F2}%");

        ////���ݹؿ������ж��߼�
        //if (_currentProgress >= winThreshold)
        //{
        //    Debug.Log(gameObject+"��ϲ��Ϳ���ˣ�");
        //    DrawManage.instance.OnDrawAll();
        //    //����ִ���й��߼�
        //}
        //else if (_currentProgress <= clearThreshold)
        //{
        //    Debug.Log(gameObject+"�����úܸɾ���");
        //    DrawManage.instance.OnEraseAll();
        //    //����ִ�в�������ɹ����߼�
        //}
        float progress = _painter.GetDrawingProgress();
        Debug.Log($"��ǰ������ɶ�: {progress * 100:F2}%");

        // ���Ϳ������
        if (progress >= winThreshold)
        {
            if (!hasTriggeredWin)   // �״�����Ŵ���
            {
                hasTriggeredWin = true;
                Debug.Log(gameObject + " ��ϲ��Ϳ���ˣ�");
                DrawManage.instance.OnDrawAll();
            }
        }
        else
        {
            // ������Ȼ��䣨���������һ���֣������ñ�ǣ��Ա��´���������ʱ���ٴδ���
            if (hasTriggeredWin) hasTriggeredWin = false;
        }
        // ������������ע�⣺������Ϳ������ͬʱ���㣿��ͨ�����ᣬ��Ϊ��ֵ���ص���
        if (progress <= clearThreshold)
        {
            if (!hasTriggeredClear)
            {
                hasTriggeredClear = true;
                Debug.Log(gameObject + " �����úܸɾ���");
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
        // ��ʽ�ͷ� RT ��Դ
        if (_maskRT != null) _maskRT.Release();
    }
}