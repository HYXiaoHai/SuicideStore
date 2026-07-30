using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene9Maneg : MonoBehaviour
{
    public static Scene9Maneg Instance;

    [Header("通用")]
    public CinemachineVirtualCamera defaultCamera;      // 默认视角
    public int currentLevel = 1;

    [Header("第一关 - 脚印")]
    public GameObject footParent;                       // 第一关所有物体父级
    public Transform footEndPosition;                   // 平移目标点
    public SpriteRenderer[] footSprites;                // 四个脚印 SpriteRenderer
    public float footFadeDuration = 0.3f;               // 脚印淡入时长
    public Image steamImage;                            // Steam 成就图片
    public Transform steamStartPos;                     // steam 起始位置
    public Transform steamEndPos;                       // steam 结束位置
    private int clickCount = 0;
    private bool isFootCompleted = false;

    [Header("穿插剧情")]
    public CanvasGroup storyCanvasGroup;               //剧情CanvasGroup
    public TextMeshProUGUI dialogueText;   //对话文字
    public GameObject continueTip;         // 继续提示（可选）
    private bool isStoryStart = false;
    private bool isStoryEnd = false;
    public float typeSpeed = 0.12f;
    private List<string> lines = new List<string>()
    {
        "我：你先休息一下，我看看发生了什么",
        "我：或许..前任列车长的记录里，会有答案..."
    };
    private int index = 0;
    private bool isTyping = false;

    [Header("第二关 - 拼图")]
    public bool isPuzzleViewOpen = false;//是否处于拼图自由视角
    public CinemachineVirtualCamera puzzleCamera;//拼图视角
    public CinemachineVirtualCamera puzzleCompleteCamera;//拼图完成视角
    public SpriteRenderer promoSpriteRender;
    //public Slider puzzleSlider;//拉杆 Slider
    public GameObject puzzleParent;//第二关所有物体父级（初始隐藏或透明）
    public PathPuzzleManage puzzleManage;//拼图逻辑管理器
    public float puzzleFadeDuration = 0.5f;//第二关渐显时长
    private SpriteRenderer[] puzzleSpriteRenderers;//第二关所有 SpriteRenderer（子物体）
    public Button puzzleButton;//切换到拼图视角的按钮

    [Header("第三关 - 文档")]
    public SpriteRenderer file;
    //public CinemachineVirtualCamera fileCamera;//文档视角
    public Button fileButton;//切换到档案的按钮
    public string fileSceneName;
    private Tween pulseTween;

    [Header("音效")]
    public AudioClip steamClip;
    public AudioClip cameraClip;//转场音效
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(TransitionManage.Instance!=null)
        {
            TransitionManage.Instance.FadeIn(1f,Color.white);
        }

        // 初始化第一关
        clickCount = 0;
        currentLevel = 1;
        isFootCompleted = false;
        //初始化举起
        storyCanvasGroup.alpha = 0f;//开启剧情

        puzzleButton.onClick.AddListener(EnterPuzzleView);
        puzzleButton.gameObject.SetActive(false);
        // 初始化第二关
        puzzleSpriteRenderers = puzzleParent.GetComponentsInChildren<SpriteRenderer>();
        SetPuzzleAlpha(0f);              // 初始完全透明

        //if (puzzleSlider != null)
        //{
        //    puzzleSlider.onValueChanged.AddListener(OnSliderValueChanged);
        //    puzzleSlider.value = 0f;
        //    puzzleSlider.gameObject.SetActive(false);
        //}

        // steam 图片复位
        steamImage.transform.position = steamStartPos.position;
        steamImage.canvasRenderer.SetAlpha(1f);

    }

    private void Update()
    {
        if (GameManage.Instance != null && GameManage.Instance.isSetting) return;

        // 第一关：鼠标点击显示脚印
        if (Input.GetMouseButtonDown(0) && currentLevel == 1 && !isFootCompleted)
        {
            clickCount++;
            ShowFootprint(clickCount - 1);
        }
        //第二关：点击鼠标左键继续
        if (currentLevel == 2&& isStoryStart&& !isStoryEnd && Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 正在打字时点击：直接显示完整句子
                StopAllCoroutines();
                if (index < lines.Count)
                    dialogueText.text = lines[index];
                isTyping = false;
                ShowContinue(true);
            }
            else
            {
                // 打完一句，点击进入下一句
                if (index < lines.Count)
                    NextLine();
            }
        }
        //// 第二关：ESC 退出拼图视角
        //if (Input.GetKeyDown(KeyCode.Escape) && isPuzzleViewOpen)
        //{
        //    ExitPuzzleView();
        //}
    }

    // ---------- 第一关逻辑 ----------
    private void ShowFootprint(int index)
    {
        // 淡入对应脚印
        footSprites[index].DOFade(1f, footFadeDuration);

        // 如果是最后一个脚印，开始转场动画
        if (clickCount == 4)
        {
            OnFootCompleted();
            StartCoroutine(TransitionToLevel2());
        }
    }

    private void OnFootCompleted()
    {
        isFootCompleted = true;
        currentLevel++;
        Debug.Log("第一关完成");

    }

    private IEnumerator TransitionToLevel2()
    {
        // 1. Steam 成就动画：从起始点飞到结束点
        Tween steamTween = steamImage.transform.DOMove(steamEndPos.position, 1f).SetEase(Ease.OutCubic);
        AudioManager.Instance.Play2DSound(steamClip, 0.8f);
        yield return steamTween.WaitForCompletion();

        //// 2. 平移第一关整体（父物体）
        footParent.gameObject.SetActive(false);

        StartLevel2FadeIn();

        yield return new WaitForSeconds(1f);
        steamImage.transform.DOMove(steamStartPos.position, 1f).SetEase(Ease.OutCubic).OnComplete(() =>{
            storyCanvasGroup.DOFade(1f, 0.5f).OnComplete(() => {
                isStoryStart = true;
                DOVirtual.DelayedCall(0.5f, () => StartType());
            });

        });
    }

    private void StartLevel2FadeIn()
    {
        // 使用 Sequence 统一控制所有 SpriteRenderer 的淡入
        AudioManager.Instance.Play2DSound(cameraClip, 0.8f);
        Sequence fadeSeq = DOTween.Sequence();
        foreach (var sr in puzzleSpriteRenderers)
        {
            fadeSeq.Join(sr.DOFade(1f, puzzleFadeDuration));
        }
        ////淡入完成后激活拼图按钮并启动拼图管理器
        //fadeSeq.OnComplete(() =>
        //{
        //    //puzzleButton.gameObject.SetActive(true);
        //    //PathPuzzleManage.Instance.StartGame();
        //});
        fadeSeq.Play();
    }
    // ----------- 剧情系统 -----------
    void StartType()
    {
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }
        isTyping = true;
        ShowContinue(false);
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        if (index >= lines.Count) yield break;
        dialogueText.text = "";
        foreach (char c in lines[index])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        ShowContinue(true);
    }

    void NextLine()
    {
        // 确保 index 合法
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }
        index++;
        if (index >= lines.Count)
        {
            // 所有对话打完，只显示结束提示，不跳转
            EndDialogue();
            return;
        }

        StartType();
    }

    void ShowContinue(bool show)
    {
        if (continueTip != null)
            continueTip.SetActive(show);
    }

    void EndDialogue()
    {
        isStoryEnd = true;
        // 对话结束：隐藏继续提示，或者你可以在这里加自己的收尾逻辑
        storyCanvasGroup.DOFade(0f, 0.5f).OnComplete(() => {
            puzzleButton.gameObject.SetActive(true);
            PathPuzzleManage.Instance.StartGame();
            storyCanvasGroup.gameObject.SetActive(false);
        });
        Debug.Log("对话已全部结束！");
        ///对话结束后激活拼图按钮并启动拼图管理器
      
        // 这里什么都不写，对话结束就停在原地，不影响场景
    }

    // ---------- 第二关逻辑 ----------
    private void EnterPuzzleView()
    {
        isPuzzleViewOpen = true;
        puzzleCamera.Priority = 20;
        promoSpriteRender.DOFade(0f, 0.5f);
        puzzleButton.gameObject.SetActive(false);
    }

    //private void ExitPuzzleView()
    //{
    //    isPuzzleViewOpen = false;
    //    puzzleCamera.Priority = 10;
    //    puzzleButton.gameObject.SetActive(true);
    //}

    // 当拼图完成时由 PathPuzzleManage 调用
    public void OnPuzzleCompleted()
    {
        isPuzzleViewOpen = false;
        //puzzleCamera.Priority = 10;
        //puzzleCompleteCamera.Priority = 20;

        //puzzleSlider.gameObject.SetActive(true);
        //// 使 Slider 渐显并可交互
        //CanvasGroup sliderCanvas = puzzleSlider.GetComponent<CanvasGroup>();
        //if (sliderCanvas == null) sliderCanvas = puzzleSlider.gameObject.AddComponent<CanvasGroup>();
        //sliderCanvas.alpha = 0f;
        //sliderCanvas.DOFade(1f, 1f);
        OnLevel2Completed();
    }

    //private void OnSliderValueChanged(float value)
    //{
    //    // 当拉杆拖到最右侧（值≈1）时触发第二关完成
    //    if (Mathf.Approximately(value, 1f))
    //    {
    //        OnLevel2Completed();
    //    }
    //}

    private void OnLevel2Completed()
    {
        currentLevel++;
        //puzzleSlider.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        //puzzleSlider.gameObject.SetActive(false);
        Debug.Log("第二关完成");
        Level3Start();
    }
    // ---------- 第三关逻辑 ----------
    public void Level3Start()
    {
        file.gameObject.SetActive(true);
        file.DOFade(0.8f,0.8f);
        fileButton.gameObject.SetActive(true);
        AudioManager.Instance.Play2DSound(cameraClip, 0.8f);
        StartPulse();
        //puzzleCompleteCamera.Priority = 10;
        //fileCamera.Priority = 20;
    }

    private void StartPulse()
    {
        // 停止现有动画
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        // 确保完全可见后开始脉冲
        file.color = new Color(file.color.r, file.color.g, file.color.b, 0.8f);
        pulseTween = file.DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }
    public void StopPulse()
    {
        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();
        if (file != null)
            file.color = new Color(file.color.r, file.color.g, file.color.b, 0f);
    }

    public void Onlevel3Button()
    {
        StopPulse();
        // 并行执行转场淡出和 BGM 淡出
        TransitionManage.Instance.FadeOut(0.3f, Color.black, () =>
        {
            // 转场完成后加载新场景
            SceneManager.LoadScene(fileSceneName);
        });
    }

    // ---------- 工具方法 ----------
    private void SetPuzzleAlpha(float alpha)
    {
        foreach (var sr in puzzleSpriteRenderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}