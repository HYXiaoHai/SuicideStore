using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class SettingUIManage : MonoBehaviour
{
    public static SettingUIManage Instance;
    public CanvasGroup settingCanvasGroup;
    [Header("设置界面")]
    public CanvasGroup settingPanel;
    public Button continueGameButton;
    public Button musicSettingButton;
    public Button backMenuButton;
    [Header("返回Menu的界面")]
    public bool isComfirmPanel;
    public CanvasGroup confirmPanel;
    public Button trueButton;
    public Button falseButton;

    [Header("音量设置的界面")]
    public bool ismusicalPanel;
    public CanvasGroup musicalSettingPanel;
    public Slider masterVolumeSlider;//全局音量Slider(1-10)
    public Slider backgroundVolumeSlider;//背景音量Slider
    public Slider soundVolumeSlider;//音效音量Slider
    public TMP_Text masterVolumeText;//全局音量Text
    public TMP_Text backgroundVolumeText;//背景音量Text
    public TMP_Text soundVolumeText;//音效音量Text
    public Button backSettingButton;
    public Button resetVolumeSettingButton;//重置音量设置

    private bool isAnimating = false;  // 防止动画重叠

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ismusicalPanel = false;
        isComfirmPanel = false;

        continueGameButton.onClick.AddListener(OnContinueGameButton);
        musicSettingButton.onClick.AddListener(OnMusicSettingButton);
        backMenuButton.onClick.AddListener(OnBackMenuButton);
        trueButton.onClick.AddListener(OnTrueButton);
        falseButton.onClick.AddListener(OnFalseButton);

        backSettingButton.onClick.AddListener(OnBackSettingButton);
        resetVolumeSettingButton.onClick.AddListener(OnResetVolumeSettings);
        // ---------- 新增：音量滑条初始化 ----------
        // 设置滑条为整数模式 0-10
        masterVolumeSlider.minValue = 0;
        masterVolumeSlider.maxValue = 10;
        masterVolumeSlider.wholeNumbers = true;
        backgroundVolumeSlider.minValue = 0;
        backgroundVolumeSlider.maxValue = 10;
        backgroundVolumeSlider.wholeNumbers = true;
        soundVolumeSlider.minValue = 0;
        soundVolumeSlider.maxValue = 10;
        soundVolumeSlider.wholeNumbers = true;

      

        // 监听滑条值变化
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        backgroundVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    // ----------------------------------------------
    // 主设置界面
    // ----------------------------------------------
    public void OpenSettingUI()
    {
        if (isAnimating) return;
        isAnimating = true;

        settingCanvasGroup.gameObject.SetActive(true);
        settingCanvasGroup.interactable = false;
        settingCanvasGroup.blocksRaycasts = false;
        settingCanvasGroup.alpha = 0f;

        settingCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            settingCanvasGroup.interactable = true;
            settingCanvasGroup.blocksRaycasts = true;
            Time.timeScale = 0f;
            if (GameManage.Instance != null)
                GameManage.Instance.isSetting = true;
            isAnimating = false;
        });
    }

    public void CloseSettingUI()
    {
        if (isAnimating) return;
        isAnimating = true;

        settingCanvasGroup.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            settingCanvasGroup.interactable = false;
            settingCanvasGroup.blocksRaycasts = false;
            settingCanvasGroup.gameObject.SetActive(false);
            Time.timeScale = 1f;
            if (GameManage.Instance != null)
                GameManage.Instance.isSetting = false;
            isAnimating = false;
        });
    }

    private void OnContinueGameButton()
    {
        CloseSettingUI();
    }

    private void OnMusicSettingButton()
    {
        OpenMusicalSettingPanel();
    }

    private void OnBackMenuButton()
    {
        OpenConfirmPanel();
    }

    // ----------------------------------------------
    // 确认返回菜单界面
    // ----------------------------------------------
    public void OpenConfirmPanel()
    {
        isComfirmPanel = true;
        if (isAnimating) return;
        isAnimating = true;

        settingPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            settingPanel.interactable = false;
            settingPanel.blocksRaycasts = false;
            confirmPanel.gameObject.SetActive(true);
            confirmPanel.alpha = 0f;
            confirmPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                confirmPanel.interactable = true;
                confirmPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    public void CloseConfirmPanel()
    {
        if (isAnimating) return;
        isAnimating = true;

        confirmPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            confirmPanel.interactable = false;
            confirmPanel.blocksRaycasts = false;
            confirmPanel.gameObject.SetActive(false);
            settingPanel.gameObject.SetActive(true);
            settingPanel.alpha = 0f;
            settingPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                settingPanel.interactable = true;
                settingPanel.blocksRaycasts = true;
                isAnimating = false;
                isComfirmPanel = false;
            });
        });
    }

    private void OnTrueButton()
    {
        CloseConfirmPanel();
        CloseMusicalSettingPanel(); // 确保音量面板也关掉
        // 关闭整个设置UI前，先恢复时间
        Time.timeScale = 1f;
        settingCanvasGroup.gameObject.SetActive(false);
        if (GameManage.Instance != null)
        {
            AudioManager.Instance.StopAllLoopingSounds();
            GameManage.Instance.isSetting = false;
            GameManage.Instance.BackMenu();
        }
    }

    private void OnFalseButton()
    {
        CloseConfirmPanel();
    }

    // ----------------------------------------------
    // 音量设置界面
    // ----------------------------------------------
    private void UpdateVolumeTexts()
    {
        masterVolumeText.text = masterVolumeSlider.value.ToString();
        backgroundVolumeText.text = backgroundVolumeSlider.value.ToString();
        soundVolumeText.text = soundVolumeSlider.value.ToString();
    }

    // 滑条回调
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
    private void OnResetVolumeSettings()
    {
        // 直接设置 AudioManager 的值（会触发保存并应用）
        AudioManager.Instance?.SetMasterVolume(10);
        AudioManager.Instance?.SetBGMVolume(10);
        AudioManager.Instance?.SetSFXVolume(10);

        // 更新本地滑条和文本显示（如果音量面板当前是打开的）
        if (musicalSettingPanel.gameObject.activeInHierarchy)
        {
            masterVolumeSlider.value = 10;
            backgroundVolumeSlider.value = 10;
            soundVolumeSlider.value = 10;
            UpdateVolumeTexts();
        }
    }
    public void OpenMusicalSettingPanel()
    {
        if (isAnimating) return;
        isAnimating = true;
        ismusicalPanel = true;

        // 加载当前音量值（从 AudioManager 获取）
        if (AudioManager.Instance != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
            backgroundVolumeSlider.value = AudioManager.Instance.GetBGMVolume();
            soundVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            UpdateVolumeTexts();
        }

        settingPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            settingPanel.interactable = false;
            settingPanel.blocksRaycasts = false;
            musicalSettingPanel.gameObject.SetActive(true);
            musicalSettingPanel.alpha = 0f;
            musicalSettingPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                musicalSettingPanel.interactable = true;
                musicalSettingPanel.blocksRaycasts = true;
                isAnimating = false;
            });
        });
    }

    public void CloseMusicalSettingPanel()
    {
        if (isAnimating) return;
        isAnimating = true;

        musicalSettingPanel.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            musicalSettingPanel.interactable = false;
            musicalSettingPanel.blocksRaycasts = false;
            musicalSettingPanel.gameObject.SetActive(false);
            settingPanel.gameObject.SetActive(true);
            settingPanel.alpha = 0f;
            settingPanel.DOFade(1f, 0.3f).SetUpdate(true).OnComplete(() =>
            {
                settingPanel.interactable = true;
                settingPanel.blocksRaycasts = true;
                isAnimating = false;
                ismusicalPanel = false;
            });
        });
    }

    private void OnBackSettingButton()
    {
        CloseMusicalSettingPanel();
    }
}