using UnityEngine;

public enum ToolType {None,Pencil, Eraser }

public class ToolManager
{
    private Material _pencilMat;
    private Material _eraserMat;
    private MaskPainter2D _painter;

    private Material _currentBrushMat;
    private bool _isPressing = false;
    public ToolType currentType;
    public ToolManager(Material pencil, Material eraser, MaskPainter2D painter)
    {
        _pencilMat = pencil;
        _eraserMat = eraser;
        _painter = painter;

        //Ä¬ÈÏ
        SetTool(ToolType.None);
    }

    public void SetTool(ToolType type)
    {
        switch (type)
        {
            case ToolType.None:
                currentType = ToolType.None;
                break;
            case ToolType.Pencil:
                currentType = ToolType.Pencil;
                _currentBrushMat = _pencilMat;
                Debug.Log("ÒÑÇÐ»»µ½Ç¦±Ê");
                break;
            case ToolType.Eraser:
                currentType = ToolType.Eraser;
                _currentBrushMat = _eraserMat;
                Debug.Log("ÒÑÇÐ»»µ½ÏðÆ¤");
                break;
        }
    }

    public void StartDrawing() => _isPressing = true;
    public void StopDrawing() => _isPressing = false;

    public void DoUpdate()
    { 
        if (_isPressing&&currentType != ToolType.None)
        {
            _painter.DrawAtMousePosition(_currentBrushMat);
        }
    }
}