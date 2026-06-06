using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene10Manage : MonoBehaviour
{
    public static Scene10Manage Instance;
    public int currentLevel = 1;
    //关卡1、2需要用到的左右区域
    public GameObject rightRoundFather;
    public GameObject leftRoundFather;
    public SpriteRenderer levelBg;//关卡总背景
    //public Sprite level1Bg_Sprit;//
    public Sprite level2Bg_Sprit;

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
        //level1GameCanvas.gameObject.SetActive(true);
        level3GameCanvas.gameObject.SetActive(false);

        //level1Renderers = new SpriteRenderer[] { dialogBox1, dialogBox2, l1bubble1, l2bubble2 };//用于结束时隐藏的
        level2Renderers = new SpriteRenderer[] { mother, father, son ,pencil, eraser, l2Bubble1,level2BG };//用于开始时显现的

        ////button1.onClick.AddListener(OnButton1Click);
        //button2.onClick.AddListener(OnButton2Click);
        //StartLevel1();
        StartLevel2();
    }
    //开启第二关
    public void StartLevel2()
    {
        rightLevel2Father.SetActive(true);
        //foreach (var item in level2Renderers)
        //{
        //    item.DOFade(1f, 1f);
        //}
        //levelBg.sprite = level2Bg_Sprit;
        //levelBg.DOFade(1f, 1f);
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
        StartCoroutine(LoadNextScene());
    }
    public IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(nextSceneName);
    }
}
