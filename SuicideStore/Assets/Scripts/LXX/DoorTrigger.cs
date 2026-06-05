using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    public bool canLoad =false;
    [Header("场景设置")]
    public bool isScene12 = false;
    public string nextSceneName;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")&& canLoad)
        {
            EnterDoor();
        }
    }

    void EnterDoor()
    {
        if (!string.IsNullOrEmpty(nextSceneName)&& !isScene12)
        {
            //SceneManager.LoadScene(nextSceneName);
            CompleteLevel();
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
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
