using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    public bool canLoad =false;
    [Header("场景设置")]
    public string nextSceneName;
    private void Start()
    {
        canLoad = false;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& canLoad)
        {
            EnterDoor();
        }
    }

    void EnterDoor()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            //SceneManager.LoadScene(nextSceneName);
            CompleteLevel();
        }
        else
        {
            Debug.Log("DoorTrigger: 未设置下一场景名称！");
        }
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
