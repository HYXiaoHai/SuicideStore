using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Scene10Manage : MonoBehaviour
{
    public static Scene10Manage Instance;
    public int currentLevel = 1;
    //关卡1、2需要用到的左右区域
    public GameObject rightRoundFather;
    public GameObject leftRoundFather;
    public SpriteRenderer levelBg;//关卡总背景
    public Sprite level1Bg_Sprit;//
    public Sprite level2Bg_Sprit;
    [Header("level1")]
    public CanvasGroup level1GameCanvas;
    public GameObject rightLevel1Father;
    public GameObject leftLevel1Father;//左父物体
    public SpriteRenderer l1bubble1;//左侧气泡1
    public SpriteRenderer l2bubble2;//左侧气泡2
    public SpriteRenderer dialogBox1;//右侧对话框
    public SpriteRenderer dialogBox2;//右侧对话框
    public Button button1;
    public Button button2;
    public int buttonNum = 0;
    private SpriteRenderer[] level1Renderers;

    [Header("level2")]
    public GameObject rightLevel2Father;//第二关右父物体
    public GameObject leftLevel2Father;//第二关左父物体
    public SpriteRenderer level2BG;//交互背景
    public Sprite bg_sprit1;//背景图片
    public Sprite bg_sprit2;//背景图片
    public SpriteRenderer mother;//妈妈
    public SpriteRenderer father;//爸爸
    public SpriteRenderer son;//儿子 乐乐
    public SpriteRenderer pencil;//铅笔
    public SpriteRenderer eraser;//橡皮
    public SpriteRenderer l2Bubble1;//第二关 左侧气泡1
    public SpriteRenderer l2Bubble2;//气泡2
    public SpriteRenderer l2Bubble3;//气泡3
    public SpriteRenderer[] level2Renderers;
    public Sprite image1;//切换图片1
    public Sprite image2;//切换图片1
    public Sprite image3;//切换图片2
    //public Rigidbody2D LeleRb;//乐乐的刚体
    public CanvasGroup transitionCanvas;//转场canvas
    public Transform sonPositon1;//切换位置1
    public Transform sonPositon2;//切换位置2
    [Header("level3")]
    public GameObject level3Father;//关卡3的背景
    public CanvasGroup level3GameCanvas;

    [Header("跳转场景")]
    public string nextSceneName;//
    public float duration;//跳转间隔
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        level1GameCanvas.gameObject.SetActive(true);
        level3GameCanvas.gameObject.SetActive(false);

        level1Renderers = new SpriteRenderer[] { dialogBox1, dialogBox2, l1bubble1, l2bubble2 };//用于结束时隐藏的
        level2Renderers = new SpriteRenderer[] { mother, father, son ,pencil, eraser, l2Bubble1,level2BG };//用于开始时显现的

        button1.onClick.AddListener(OnButton1Click);
        button2.onClick.AddListener(OnButton2Click);
        StartLevel1();
    }
    //开启第一关
    public void StartLevel1()
    {
        currentLevel = 1;
        buttonNum = 0;
        button2.gameObject.SetActive(false);
    }
    //第一关 按钮1
    public void OnButton1Click()
    {
        //先点击button1
        l1bubble1.DOFade(1f, 1f);
        button1.gameObject.SetActive(false);
        buttonNum++;//1
        dialogBox2.DOFade(1f, 1f).OnComplete(() => {
            button2.gameObject.SetActive(true);//开启button2
        });
    }
    //第一关 按钮2
    public void OnButton2Click()
    {
        buttonNum++;
        if (buttonNum == 2)//第1次点击button2显示文案
        {
            l2bubble2.DOFade(1f, 1f);
        }
        else if (buttonNum >= 3)//第2次点击button2完成关卡
        {
            button2.gameObject.SetActive(false);
            Level1Complect();
        }
    }
    //完成第一关
    public void Level1Complect()
    {
        Sequence fadeSeq = DOTween.Sequence();
        foreach (var item in level1Renderers)
        {
            fadeSeq.Join(item.DOFade(0f, 1f));
        }
        fadeSeq.Join(levelBg.DOFade(0f, 1f));
        fadeSeq.OnComplete(() =>
        {
            rightLevel1Father.SetActive(false);
            leftLevel1Father.SetActive(false);
            level1GameCanvas.gameObject.SetActive(false);
            StartLevel2();
        });
        fadeSeq.Play();
    }
    //开启第二关
    public void StartLevel2()
    {
        rightLevel2Father.SetActive(true);
        foreach (var item in level2Renderers)
        {
            item.DOFade(1f, 1f);
        }
        levelBg.sprite = level2Bg_Sprit;
        levelBg.DOFade(1f, 1f);
    }
    //
    public void OnDrawComplet(int _completeNum)
    {
        switch (_completeNum)
        {
            case 0://第一阶段
                son.sprite = image1;
                son.transform.position = sonPositon1.position;
                level2BG.sprite = bg_sprit1;
                l2Bubble2.DOFade(0f, 1f);//隐藏对话2
                l2Bubble3.DOFade(0f, 1f);//隐藏对话3
                break;
            case 1://第二阶段
                son.sprite = image2;
                son.transform.position = sonPositon2.position;
                l2Bubble2.DOFade(1f, 1f);//显示对话2
                l2Bubble3.DOFade(0f, 1f);//隐藏对话3
                break;
            case 2://第三阶段
                son.sprite = image3;
                level2BG.sprite = bg_sprit2;
                l2Bubble3.DOFade(1f, 1f);//显示对话3
                Level2Complete();
                break;
        }
    }
    public void Level2Complete()
    {
        //LeleRb.bodyType = RigidbodyType2D.Dynamic;
        transitionCanvas.DOFade(1f, 2f).SetEase(Ease.InQuart).OnComplete(() => {
            //LeleRb.gameObject.SetActive(false);
            leftLevel2Father.SetActive(false);
            rightLevel2Father.SetActive(false);
            rightRoundFather.SetActive(false);
            leftRoundFather.SetActive(false);
            Leve3Start();
        });
    }

    public void Leve3Start()
    {
        level3Father.SetActive(true);
        level3GameCanvas.gameObject.SetActive(true);
        transitionCanvas.DOFade(0f, 1f);//渐显
    }
    public void Leve3Complete()
    {
        //跳转
    }
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
