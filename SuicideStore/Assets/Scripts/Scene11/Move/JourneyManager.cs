using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JourneyManager : MonoBehaviour
{
    public string nextSceneName;
    public bool isFinal;
    public Color fadeColor = Color.white;
    void Start()
    {
        if (TransitionManage.Instance != null)
            TransitionManage.Instance.FadeIn(0.5f, Color.black);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if(Input.GetMouseButtonDown(0))
        {
            if (isFinal)
                CompleteLevel();
            else
                NextScene();
        }
    }
    public void NextScene()
    {
        // 并行执行转场淡出和 BGM 淡出
        TransitionManage.Instance.FadeOut(1f, fadeColor, () =>
        {
            // 转场完成后加载新场景
            SceneManager.LoadScene(nextSceneName);
        });
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
