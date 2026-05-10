using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    // 在当前关卡结束时调用此方法（例如剧情结束、点击某按钮、到达出口等）
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