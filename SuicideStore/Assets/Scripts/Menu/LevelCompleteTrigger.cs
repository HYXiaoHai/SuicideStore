using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteTrigger : MonoBehaviour
{
    public bool shouldUseFade;
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
            {
                if (shouldUseFade)
                {
                    // 并行执行转场淡出和 BGM 淡出
                    TransitionManage.Instance.FadeOut(1f, Color.black, () =>
                    {
                        // 转场完成后加载新场景
                        SceneManager.LoadScene(nextScene);
                    });
                    AudioManager.Instance.FadeOutCurrentBGM(1f, null);
                }
                else
                {
                    SceneManager.LoadScene(nextScene);
                }
            }
        }
        else
        {
            Debug.Log("恭喜通关全部12大关！");
        }
    }
}