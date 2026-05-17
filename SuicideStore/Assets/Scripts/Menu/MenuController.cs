using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public CanvasGroup menuPanel;
    public CanvasGroup archivePanel;
    public CanvasGroup exitPanel;
    public CanvasGroup settingPanel;

    public bool isArchivePanel;
    public bool isExitPanel;
    public bool isSettingPanel;

    [Header("关卡选择的界面")]
    public Button[] levelButtons;

    [Header("音量设置的界面")]
    public Slider masterVolumeSlider;
    public Slider backgroundVolumeSlider;
    public Slider soundVolumeSlider;
    public TMP_Text masterVolumeText;
    public TMP_Text backgroundVolumeText;
    public TMP_Text soundVolumeText;
    public Button backSettingButton;
    public Button resetVolumeSettingButton;//重置音量设置

    private bool isAnimating = false; // 防止动画重叠

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 初始化面板透明度及状态
        archivePanel.alpha = 0f;
        archivePanel.gameObject.SetActive(false);
        exitPanel.alpha = 0f;
        exitPanel.gameObject.SetActive(false);
        settingPanel.alpha = 0f;
        settingPanel.gameObject.SetActive(false);
        menuPanel.alpha = 1f;
        menuPanel.interactable = true;
        menuPanel.blocksRaycasts = true;

        // 绑定关卡按钮
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;
            levelButtons[i].onClick.AddListener(() => OnLevelButtonClick(level));
        }

        // 音量滑条初始化
        masterVolumeSlider.minValue = 0;
        masterVolumeSlider.maxValue = 10;
        masterVolumeSlider.wholeNumbers = true;
        backgroundVolumeSlider.minValue = 0;
        backgroundVolumeSlider.maxValue = 10;
        backgroundVolumeSlider.wholeNumbers = true;
        soundVolumeSlider.minValue = 0;
        soundVolumeSlider.maxValue = 10;
        soundVolumeSlider.wholeNumbers = true;

        backSettingButton.onClick.AddListener(OnBackSettingButton);
        resetVolumeSettingButton.onClick.AddListener(OnResetVolumeSettings);
        
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        backgroundVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    // ==================== 面板切换动画 ====================

    // 打开存档面板
    public void OpenArchivePanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isArchivePanel = true;

        // 淡出主菜单
        menuPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.gameObject.SetActive(false);

            // 显示并淡入存档面板
            archivePanel.gameObject.SetActive(true);
            archivePanel.alpha = 0f;
            archivePanel.interactable = false;
            archivePanel.blocksRaycasts = false;
            archivePanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                archivePanel.interactable = true;
                archivePanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    // 关闭存档面板
    public void CloseArchivePanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isArchivePanel = false;

        archivePanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            archivePanel.interactable = false;
            archivePanel.blocksRaycasts = false;
            archivePanel.gameObject.SetActive(false);

            // 显示并淡入主菜单
            menuPanel.gameObject.SetActive(true);
            menuPanel.alpha = 0f;
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                menuPanel.interactable = true;
                menuPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    // 打开退出面板
    public void OpenExitPanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isExitPanel = true;

        menuPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.gameObject.SetActive(false);

            exitPanel.gameObject.SetActive(true);
            exitPanel.alpha = 0f;
            exitPanel.interactable = false;
            exitPanel.blocksRaycasts = false;
            exitPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                exitPanel.interactable = true;
                exitPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    // 关闭退出面板
    public void CloseExitPanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isExitPanel = false;

        exitPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            exitPanel.interactable = false;
            exitPanel.blocksRaycasts = false;
            exitPanel.gameObject.SetActive(false);

            menuPanel.gameObject.SetActive(true);
            menuPanel.alpha = 0f;
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                menuPanel.interactable = true;
                menuPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    // 打开设置面板
    public void OpenSettingPanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isSettingPanel = true;

        menuPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.gameObject.SetActive(false);
            // 刷新音量显示
            if (AudioManager.Instance != null)
            {
                masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
                backgroundVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
                soundVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                UpdateVolumeTexts();
            }
            settingPanel.gameObject.SetActive(true);
            settingPanel.alpha = 0f;
            settingPanel.interactable = false;
            settingPanel.blocksRaycasts = false;
            settingPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                settingPanel.interactable = true;
                settingPanel.blocksRaycasts = true;

                isAnimating = false;
            });
        });
    }

    // 关闭设置面板
    public void CloseSettingPanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        isSettingPanel = false;

        settingPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            settingPanel.interactable = false;
            settingPanel.blocksRaycasts = false;
            settingPanel.gameObject.SetActive(false);

            menuPanel.gameObject.SetActive(true);
            menuPanel.alpha = 0f;
            menuPanel.interactable = false;
            menuPanel.blocksRaycasts = false;
            menuPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                menuPanel.interactable = true;
                menuPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    // ==================== 音量相关 ====================
    private void UpdateVolumeTexts()
    {
        masterVolumeText.text = masterVolumeSlider.value.ToString();
        backgroundVolumeText.text = backgroundVolumeSlider.value.ToString();
        soundVolumeText.text = soundVolumeSlider.value.ToString();
    }

    private void OnMasterVolumeChanged(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        masterVolumeText.text = intVal.ToString();
        AudioManager.Instance?.SetMasterVolume(intVal);
    }

    private void OnBGMVolumeChanged(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        backgroundVolumeText.text = intVal.ToString();
        AudioManager.Instance?.SetBGMVolume(intVal);
    }

    private void OnSFXVolumeChanged(float value)
    {
        int intVal = Mathf.RoundToInt(value);
        soundVolumeText.text = intVal.ToString();
        AudioManager.Instance?.SetSFXVolume(intVal);
    }

    public void OnBackSettingButton()
    {
        CloseSettingPanel();
    }
    private void OnResetVolumeSettings()
    {
        // 直接设置 AudioManager 的值（会触发保存并应用）
        AudioManager.Instance?.SetMasterVolume(10);
        AudioManager.Instance?.SetBGMVolume(10);
        AudioManager.Instance?.SetSFXVolume(10);

        // 更新本地滑条和文本显示（如果音量面板当前是打开的）
        if (settingPanel.gameObject.activeInHierarchy)
        {
            masterVolumeSlider.value = 10;
            backgroundVolumeSlider.value = 10;
            soundVolumeSlider.value = 10;
            UpdateVolumeTexts();
        }
    }
    // ==================== 其他 ====================
    public void OnNewGame()
    {
        GameManage.Instance.NewGame();
    }

    public void OnContinue()
    {
        OpenArchivePanel();
        RefreshArchivePanel();
    }

    public void OnExit()
    {
        GameManage.Instance.QuitGame();
    }

    private void RefreshArchivePanel()
    {
        int unlocked = GameManage.Instance.GetUnlockedLevel();
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i + 1) <= unlocked;
            levelButtons[i].interactable = isUnlocked;
        }
    }

    private void OnLevelButtonClick(int level)
    {
        GameManage.Instance.StartFromLevel(level);
    }
}