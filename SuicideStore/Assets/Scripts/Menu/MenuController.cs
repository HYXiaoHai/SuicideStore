using DG.Tweening;
using System.Collections;
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
    public Sprite[] levelSprites;//关卡解锁的图片（和按钮顺序对应）
    public Sprite lockSprite;//关卡未解锁的图片
    public TMP_Text lockText;//关卡未解锁的提示
    private Coroutine lockCoroutine; //用于控制未解锁提示协程
    private Vector2[] initialButtonPositions;
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
        if(TransitionManage.Instance!=null)
        {
            TransitionManage.Instance.FadeIn(0.5f,Color.black);
        }

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
        initialButtonPositions = new Vector2[levelButtons.Length];
        for (int i = 0; i < levelButtons.Length; i++)
        {
            initialButtonPositions[i] = levelButtons[i].GetComponent<RectTransform>().anchoredPosition;
        }
        RefreshArchivePanel();
        // 初始化提示文本（默认隐藏）
        if (lockText != null)
        {
            lockText.alpha = 0f;
            lockText.gameObject.SetActive(false);
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

           
        });
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
        });
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

    //private void RefreshArchivePanel()
    //{
    //    int unlocked = GameManage.Instance.GetUnlockedLevel();
    //    for (int i = 0; i < levelButtons.Length; i++)
    //    {
    //        bool isUnlocked = (i + 1) <= unlocked;
    //        levelButtons[i].interactable = isUnlocked;
    //    }
    //}
    // ==================== 关卡选择相关 ====================
    private void RefreshArchivePanel()
    {
        int unlocked = GameManage.Instance.GetUnlockedLevel();
        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool isUnlocked = (i + 1) <= unlocked;
            //levelButtons[i].interactable = isUnlocked;
            // 设置按钮图片
            Image btnImage = levelButtons[i].image;
            if (btnImage != null)
            {
                if (isUnlocked && i < levelSprites.Length)
                    btnImage.sprite = levelSprites[i];
                else
                    btnImage.sprite = lockSprite;
            }
        }
    }
    private void OnLevelButtonClick(int level)
    {
        if (level > GameManage.Instance.GetUnlockedLevel())
        {
            Button btn = levelButtons[level - 1];
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.DOKill(); // 停止所有动画
            rt.anchoredPosition = initialButtonPositions[level - 1]; //复位
                                                                     //抖动动画（左右晃动）
            rt.DOPunchAnchorPos(new Vector2(15f, 0), 0.25f).SetEase(Ease.OutBack);

            if (lockCoroutine != null)
                StopCoroutine(lockCoroutine);
            lockCoroutine = StartCoroutine(ShowLockText());
            return;
        }
        GameManage.Instance.StartFromLevel(level);
    }

    private IEnumerator ShowLockText()
    {
        if (lockText == null) yield break;
        lockText.gameObject.SetActive(true);
        lockText.alpha = 0f;
        // 渐显
        lockText.DOFade(1f, 0.5f);
        yield return new WaitForSeconds(1.5f);
        // 渐隐
        lockText.DOFade(0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        lockText.gameObject.SetActive(false);
        lockCoroutine = null; // 协程结束，置空
    }



    //private void OnLevelButtonClick(int level)
    //{
    //    GameManage.Instance.StartFromLevel(level);
    //}

}