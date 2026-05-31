using DG.Tweening;
using UnityEngine;

public class CableCarManage : MonoBehaviour
{
    public static CableCarManage instance;
    public CanvasGroup transitionCanvasGroup;//转场用
    public Camera mycamera;
    public GameObject cameraFollow;//相机跟踪带点
    [Header("第一关")]
    public Color level1BGColor;//第一关相机背景颜色
    public QteManager level1manager;
    public Transform level1CameraPosition;//第一关相机位置
    [Header("第二关")]
    public Color level2BGColor;//第二关相机背景颜色
    public MemoryWalkManager level2Manage;
    public Transform level2CameraPosition;//第二关相机位置（剩下的靠MemoryWalkManager管理）
    [Header("跳转场景")]
    public float loadScenesDuration;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // 设置第一关背景和相机位置
        mycamera.backgroundColor = level1BGColor;
        cameraFollow.transform.position = level1CameraPosition.position;

        // 开场动画：从黑屏渐显，完成后启动第一关
        if (transitionCanvasGroup != null)
        {
            transitionCanvasGroup.alpha = 1f;
            transitionCanvasGroup.blocksRaycasts = true;
            transitionCanvasGroup.DOFade(0f, 1f).OnComplete(() =>
            {
                transitionCanvasGroup.blocksRaycasts = false;
                OnLevel1Start();
            });
        }
        else
        {
            OnLevel1Start();
        }
    }
    public void OnLevel1Start()
    {
        Debug.Log("第一关开启");
        level1manager.StartQTESequence();
    }
    public void OnLevel1Complete()
    {
        Debug.Log("第一关完成");
        // 渐隐到黑
        transitionCanvasGroup.DOFade(1f, 1f).OnComplete(() =>
        {
            // 切换背景色和相机位置
            mycamera.backgroundColor = level2BGColor;
            cameraFollow.transform.position = level2CameraPosition.position;

            // 等待1秒后渐显，并启动第二关
            DOVirtual.DelayedCall(2f, () =>
            {
                transitionCanvasGroup.DOFade(0f, 1f).OnComplete(() =>
                {
                    OnLevel2Start();
                });
            });
        });
    }
    public void OnLevel2Start()
    {
        Debug.Log("第二关开启");
        if (level2Manage != null)
            level2Manage.StartGame();
        else
            Debug.LogError("第二关管理器未绑定！");
    }
    public void OnLevel2Complete()
    {
        // 第二关完成，渐隐并加载下一场景
        transitionCanvasGroup.DOFade(1f, 1f).OnComplete(() =>
        {
            // 可以调用 CompleteLevel 加载下一关卡
            NextScene();
        });
    }
    public void NextScene()
    {
        CompleteLevel();
    }
    public void CompleteLevel()
    {
        // 通知 GameManage 当前关卡通关
        GameManage.Instance.CompleteCurrentLevel();
        // 可选：自动进入下一关第一场景（如果希望无缝衔接）
        int nextLevel = GameManage.Instance.currentLevel + 1;
        if (nextLevel <= 12)
        {
            string nextScene = GameManage.Instance.GetFirstSceneOfLevel(nextLevel);
            if (!string.IsNullOrEmpty(nextScene))
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
}
