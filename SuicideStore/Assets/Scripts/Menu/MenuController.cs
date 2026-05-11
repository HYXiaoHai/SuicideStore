using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public CanvasGroup menuPanel;
    public CanvasGroup archivePanel;
    public CanvasGroup exitPanel;

    public bool isArchivePanel;
    public bool isExitPanel;

    // 存放12个关卡按钮的数组（在Inspector中拖入）
    public Button[] levelButtons;   // 长度12，索引0代表第1关，1代表第2关...

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        archivePanel.alpha = 0f;
        archivePanel.gameObject.SetActive(false);

        // 为每个按钮绑定点击事件（也可以手动在Inspector绑定，这里自动绑定）
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;   // 关卡号
            levelButtons[i].onClick.AddListener(() => OnLevelButtonClick(level));
        }
    }

    // 新游戏按钮
    public void OnNewGame()
    {
        GameManage.Instance.NewGame();
    }

    // 继续游戏按钮 -> 打开存档面板
    public void OnContinue()
    {
        OpenArchivePanel();
        RefreshArchivePanel();   // 刷新按钮状态（根据存档禁用未解锁关卡）
    }

    // 退出按钮
    public void OnExit()
    {
        CloseExitPanel();
        GameManage.Instance.QuitGame();
    }

    // 打开存档面板
    public void OpenArchivePanel()
    {
        isArchivePanel = true;
        archivePanel.gameObject.SetActive(true);
        menuPanel.alpha = 0f;
        archivePanel.alpha = 1f;
        menuPanel.gameObject.SetActive(false);
    }

    // 关闭存档面板
    public void CloseArchivePanel()
    {
        isArchivePanel = false;
        menuPanel.gameObject.SetActive(true);
        menuPanel.alpha = 1f;
        archivePanel.alpha = 0f;
        archivePanel.gameObject.SetActive(false);
    }
    //打开存档面板
    public void OpenExitPanel()
    {
        isExitPanel = true;
        exitPanel.gameObject.SetActive(true);
        menuPanel.alpha = 0f;
        exitPanel.alpha = 1f;
        menuPanel.gameObject.SetActive(false);
    }

    // 关闭存档面板
    public void CloseExitPanel()
    {
        isExitPanel = false;
        menuPanel.gameObject.SetActive(true);
        menuPanel.alpha = 1f;
        exitPanel.alpha = 0f;
        exitPanel.gameObject.SetActive(false);
    }



    // 刷新存档面板：根据已解锁关卡，禁用未解锁的按钮
    private void RefreshArchivePanel()
    {
        int unlocked = GameManage.Instance.GetUnlockedLevel();
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i + 1) <= unlocked;
            levelButtons[i].interactable = isUnlocked;
            // 可选：修改按钮文本颜色或显示锁图标
        }
    }

    // 存档面板中某个关卡按钮被点击
    private void OnLevelButtonClick(int level)
    {
        GameManage.Instance.StartFromLevel(level);
    }
}