using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene4Manage : MonoBehaviour
{
    public static Scene4Manage Instance;
    public int currentLevel;
    [Header("关卡的控制器")]
    public GrowthComparison level1Mange;//第一关控制器（默认初始关卡）
    public RowManage level2Manage;//第二关控制器
    public PhotoMapper level3Manage;//第三关控制器
    public RowManage level4Manage;//第二关控制器

    [Header("用于切换相机")]
    public CinemachineVirtualCamera[] levelCameras;

    [Header("优先级基数")]
    [SerializeField] private int basePriority = 50;//激活相机优先级
    [SerializeField] private int step = 10;//每级递减的值
    private void Awake()
    {
        // 单例安全校验
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        if(TransitionManage.Instance!=null)
        {
            TransitionManage.Instance.FadeIn(1f,Color.white);
        }
        currentLevel = 1;

        if (levelCameras == null || levelCameras.Length < 5)
        {
            return;
        }
        ChangeCamera(1);
    }

    //转换镜头（关卡数1-5 ，延迟转换的时间）
    public void ChangeCamera(int level,float delay = 0,System.Action onComplete = null)
    {
        StartCoroutine(DelayedSwitch(level, delay,onComplete));
    }

    IEnumerator DelayedSwitch(int level, float delay,System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        if (level < 1 || level > levelCameras.Length)
        {
            yield break;
        }

        //目标相机索引（0基）
        int activeIndex = level - 1;

        //计算优先级
        for (int i = 0; i < levelCameras.Length; i++)
        {
            if (levelCameras[i] == null) continue;

            int priority;
            if (i == activeIndex)
                priority = basePriority;
            else
                priority = basePriority - (Mathf.Abs(i - activeIndex) * step);

            levelCameras[i].Priority = priority;
        }
        // 确保相机优先级设置生效（等待一帧）
        yield return new WaitForSeconds(2f);
        onComplete?.Invoke();
    }
}
