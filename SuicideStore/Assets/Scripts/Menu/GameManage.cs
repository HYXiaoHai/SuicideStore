using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    None,
    Menu,
    Game,
}

public class GameManage : MonoBehaviour
{
    public static GameManage Instance;

    public GameState currentState = GameState.None;
    public int currentLevel;//当前正在游玩的关卡（1~12）
    [Header("设置相关")]
    public bool isSetting = false;
    public string menuSceneName;//
    //存档数据
    private int unlockedLevel = 1;//已解锁的最高关卡，初始为1
    [Header("暂停 返回主界面相关")]

    // 场景映射表：关卡号 -> 该关的第一个场景名
    private Dictionary<int, string> levelFirstScene = new Dictionary<int, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);   // 先设单例，再标记跨场景
            InitSceneMapping();
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);   // 已经存在实例，销毁当前对象
            return;
        }
    }

    void Start()
    {
        isSetting = false;
        currentState = GameState.Menu;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Menu)
            {
                if (MenuController.instance.isArchivePanel)
                {
                    MenuController.instance.CloseArchivePanel();
                }
                else if (MenuController.instance.isExitPanel)
                {
                    MenuController.instance.CloseExitPanel();
                }
                else if (MenuController.instance.isSettingPanel)
                {
                    MenuController.instance.CloseSettingPanel();
                }
                else
                {
                    MenuController.instance.OpenExitPanel();
                }
            }
            else if (currentState == GameState.Game)
            {
                if (!isSetting)
                {
                    isSetting = true;
                    SettingUIManage.Instance?.OpenSettingUI();
                }
                else
                {
                    if (SettingUIManage.Instance.ismusicalPanel)
                    {
                        SettingUIManage.Instance.CloseMusicalSettingPanel();
                    }
                    else if (SettingUIManage.Instance.isComfirmPanel)
                    {
                        SettingUIManage.Instance.CloseConfirmPanel();
                    }
                    else
                    {
                        isSetting = false;
                        SettingUIManage.Instance?.CloseSettingUI();
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------
    // 场景映射表（根据你提供的数据）
    // ------------------------------------------------------------
    private void InitSceneMapping()
    {
        // level 1
        levelFirstScene[1] = "S1-1.1-text";//blak
        // level 2
        levelFirstScene[2] = "S2-2.1-clock";//white
        // level 3
        levelFirstScene[3] = "S3";//black
        // level 4
        levelFirstScene[4] = "S4";//white
        // level 5
        levelFirstScene[5] = "S5";//white
        // level 6
        levelFirstScene[6] = "S6_reversal";//blak
        // level 7
        //levelFirstScene[7] = "S7-7.1-puzzle";
        levelFirstScene[7] = "S8-8.1-story";//blak
        // level 8
        levelFirstScene[8] = "S7-7.2-walk";//blak
        // level 9
        levelFirstScene[9] = "S9-9.1-exchange";//white
        // level 10
        levelFirstScene[10] = "S10-10.1-dialogue";//white
        // level 11
        levelFirstScene[11] = "S11-11.1-move";//blak
        // level 12
        levelFirstScene[12] = "S12";
    }

    // 根据关卡号获取第一个场景名
    public string GetFirstSceneOfLevel(int level)
    {
        if (levelFirstScene.ContainsKey(level))
            return levelFirstScene[level];
        else
        {
            Debug.LogError($"未找到关卡 {level} 的映射场景！");
            return null;
        }
    }

    // ------------------------------------------------------------
    // 存档 / 读档（使用 PlayerPrefs 简单存储）
    // ------------------------------------------------------------
    private void SaveGameData()
    {
        PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
        PlayerPrefs.Save();
        Debug.Log($"游戏已保存，当前已解锁关卡：{unlockedLevel}");
    }

    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey("UnlockedLevel"))
            unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel");
        else
            unlockedLevel = 1;

        Debug.Log($"加载存档，已解锁关卡：{unlockedLevel}");
    }

    //新游戏：重置存档
    public void NewGame()
    {
        unlockedLevel = 1;
        SaveGameData();
        currentLevel = 1;
        // 加载第一关第一个场景
        string firstScene = GetFirstSceneOfLevel(1);
        if (!string.IsNullOrEmpty(firstScene))
        {
            currentState = GameState.Game;
            TransitionManage.Instance.FadeOut(1f, Color.black,null);
            if (AudioManager.Instance.CurrentBGMClip != null)
            {
                AudioManager.Instance.FadeOutCurrentBGM(1f, ()=>
                    {
                    SceneManager.LoadScene(firstScene);
                });

            }
            //SceneManager.LoadScene(firstScene);
        }
    }

    // 获取当前已解锁的最高关卡（供UI使用）
    public int GetUnlockedLevel() => unlockedLevel;

    // 通关当前关卡时调用（在关卡最后一个场景结束时触发）
    public void CompleteCurrentLevel()
    {
        // 假设当前关卡为 currentLevel（需要在进入关卡时设置）
        if (currentLevel + 1 > unlockedLevel && currentLevel + 1 <= 12)
        {
            unlockedLevel = currentLevel + 1;
            SaveGameData();
            Debug.Log($"恭喜通关第{currentLevel}关，解锁第{unlockedLevel}关！");
        }
    }

    // 从某一关开始游戏（供存档面板的按钮调用）
    public void StartFromLevel(int level)
    {
        if (level > unlockedLevel)
        {
            Debug.LogWarning($"关卡 {level} 尚未解锁，无法开始");
            return;
        }

        Color fadeColor = Color.white;
        if(level == 1||level==3|| level == 6|| level == 7|| level == 8) fadeColor = Color.black;

        currentLevel = level;
        string targetScene = GetFirstSceneOfLevel(level);
        if (!string.IsNullOrEmpty(targetScene))
        {
            currentState = GameState.Game;
            TransitionManage.Instance.FadeOut(1f, fadeColor);
            if (AudioManager.Instance.CurrentBGMClip != null)
            {
                AudioManager.Instance.FadeOutCurrentBGM(1f, () =>
                {
                    SceneManager.LoadScene(targetScene);
                });

            }
        }
    }
    public void BackMenu()
    {
        currentState = GameState.Menu;
        Time.timeScale = 1f;      //恢复时间
        //并行执行转场淡出和 BGM 淡出
        TransitionManage.Instance.FadeOut(1f, Color.black, () =>
        {
            // 转场完成后加载新场景
            SceneManager.LoadScene("Menu");
        });
        if(AudioManager.Instance.CurrentBGMClip!=null)
        AudioManager.Instance.FadeOutCurrentBGM(1f, null);
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
