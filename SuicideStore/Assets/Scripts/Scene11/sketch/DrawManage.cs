using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawManage : MonoBehaviour
{
    public static DrawManage instance;
    public List<DrawingController> drawingControllers = new List<DrawingController>();

    [Header("工具引用")]
    public DrawingTool pencilTool;
    public DrawingTool eraserTool;
    public bool isSwap;

    public int completNum = 0;

    private void Awake()
    {
        instance = this;
    }
    public int GetCompletedCount()
    {
        int count = 0;
        foreach (var controller in drawingControllers)
        {
            if (controller != null && controller.IsCompleted)
            {
                Debug.Log(controller);
                count++;
            }
        }
        return count;
    }

    private void Start()
    {
        completNum = 0;
    }
    public void RefreshCompletion()
    {
        int completed = GetCompletedCount();
        if (Scene10Manage.Instance != null)
        {
            Debug.Log("completed:"+ completed);
            Scene10Manage.Instance.OnDrawComplet(completed);
        }
    }

    public void OnDrawAll()
    {
        RefreshCompletion();
    }
    public void OnEraseAll()
    {
        RefreshCompletion();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isSwap)
            {
                Debug.Log("调回工具");
                pencilTool.toolType = ToolType.Pencil;
                eraserTool.toolType = ToolType.Eraser;
                isSwap = false;
            }
            else
            {
                Debug.Log("调换工具");
                eraserTool.toolType = ToolType.Pencil;
                pencilTool.toolType = ToolType.Eraser;
                isSwap = true;
            }
        }
    }
}
