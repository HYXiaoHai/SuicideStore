using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishingGameManager : MonoBehaviour
{
    [Header("UI引用")]
    public RectTransform identifyBar;      // 识别条
    public RectTransform faceTarget;       // 笑脸目标
    public Image progressFill;             // 进度条填充
    public Image characterFace;            // 人物表情
    public TextMeshProUGUI successText;    // 成功文案
    public RectTransform bgTrack;          // 轨道背景

    [Header("表情图片")]
    public Sprite happyFace;               // 开心表情
    public Sprite sadFace;                 // 不开心表情

    [Header("游戏参数")]
    public float barMoveSpeed = 300f;      // 识别条移动速度
    public float faceMoveSpeed = 150f;     // 笑脸移动速度
    public float progressIncreaseRate = 0.3f;  // 进度增长速率
    public float progressDecreaseRate = 0.2f;  // 进度减少速率
    public float winProgress = 1f;         // 胜利进度值
    public float loseProgress = 0f;        // 失败进度值

    [Header("移动范围")]
    public float leftLimit = -300f;        // 左边界
    public float rightLimit = 300f;        // 右边界

    private float currentProgress = 0.5f;  // 当前进度（0-1）
    private bool isGameActive = true;      // 游戏是否进行中
    private int faceMoveDirection = 1;     // 笑脸移动方向
    private float faceChangeDirectionTimer = 0f;  // 改变方向计时器

    void Update()
    {
        if (!isGameActive) return;

        // 控制识别条移动
        ControlIdentifyBar();

        // 控制笑脸移动
        MoveFaceTarget();

        // 检测碰撞并更新进度
        CheckCollisionAndUpdateProgress();

        // 更新表情
        UpdateCharacterFace();

        // 检查游戏结束条件
        CheckGameEnd();
    }

    // 控制识别条移动（鼠标按住右移，松开左移）
    void ControlIdentifyBar()
    {
        float moveDir = 0f;

        // 鼠标按住/点击 → 右移
        if (Input.GetMouseButton(0))
        {
            moveDir = 1f;
        }
        // 松开鼠标 → 左移
        else
        {
            moveDir = -1f;
        }

        // 移动识别条
        Vector2 pos = identifyBar.anchoredPosition;
        pos.x += moveDir * barMoveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        identifyBar.anchoredPosition = pos;
    }

    // 笑脸自动移动
    void MoveFaceTarget()
    {
        // 随机改变方向
        faceChangeDirectionTimer += Time.deltaTime;
        if (faceChangeDirectionTimer >= Random.Range(1f, 3f))
        {
            faceMoveDirection = Random.value > 0.5f ? 1 : -1;
            faceChangeDirectionTimer = 0f;
        }

        // 移动笑脸
        Vector2 pos = faceTarget.anchoredPosition;
        pos.x += faceMoveDirection * faceMoveSpeed * Time.deltaTime;

        // 边界检测和反弹
        if (pos.x >= rightLimit)
        {
            pos.x = rightLimit;
            faceMoveDirection = -1;
        }
        else if (pos.x <= leftLimit)
        {
            pos.x = leftLimit;
            faceMoveDirection = 1;
        }

        faceTarget.anchoredPosition = pos;
    }

    // 检测识别条是否框住笑脸
    bool IsFaceInIdentifyBar()
    {
        float barLeft = identifyBar.anchoredPosition.x - identifyBar.sizeDelta.x / 2;
        float barRight = identifyBar.anchoredPosition.x + identifyBar.sizeDelta.x / 2;
        float faceLeft = faceTarget.anchoredPosition.x - faceTarget.sizeDelta.x / 2;
        float faceRight = faceTarget.anchoredPosition.x + faceTarget.sizeDelta.x / 2;

        // 笑脸完全在识别条内
        return faceLeft >= barLeft && faceRight <= barRight;
    }

    // 检测碰撞并更新进度
    void CheckCollisionAndUpdateProgress()
    {
        if (IsFaceInIdentifyBar())
        {
            // 识别条框住笑脸 → 进度增长
            currentProgress += progressIncreaseRate * Time.deltaTime;
            identifyBar.GetComponent<Image>().color = new Color(0.5f, 0.9f, 0f); // 绿色
        }
        else
        {
            // 笑脸脱离识别条 → 进度减少
            currentProgress -= progressDecreaseRate * Time.deltaTime;
            identifyBar.GetComponent<Image>().color = new Color(0.4f, 0.6f, 0.45f); // 灰绿色
        }

        // 限制进度范围
        currentProgress = Mathf.Clamp01(currentProgress);

        // 更新进度条显示
        progressFill.fillAmount = currentProgress;

        // 根据进度更新颜色（从红到绿）
        Color progressColor = Color.Lerp(Color.red, Color.green, currentProgress);
        progressFill.color = progressColor;
    }

    // 更新人物表情
    void UpdateCharacterFace()
    {
        if (IsFaceInIdentifyBar())
        {
            characterFace.sprite = happyFace;
        }
        else
        {
            characterFace.sprite = sadFace;
        }
    }

    // 检查游戏结束条件
    void CheckGameEnd()
    {
        if (currentProgress >= winProgress)
        {
            // 进度填满 → 成功
            GameWin();
        }
        else if (currentProgress <= loseProgress)
        {
            // 进度归零 → 失败重置
            GameLose();
        }
    }

    // 游戏胜利
    void GameWin()
    {
        isGameActive = false;
        successText.gameObject.SetActive(true);
        successText.text = "识别成功！";
        Debug.Log("You win!");
    }

    // 游戏失败（重置）
    void GameLose()
    {
        // 重置进度
        currentProgress = 0.5f;
        progressFill.fillAmount = currentProgress;
        Debug.Log("Game reset!");
    }

    // 重新开始游戏
    public void RestartGame()
    {
        currentProgress = 0.5f;
        progressFill.fillAmount = currentProgress;
        successText.gameObject.SetActive(false);
        isGameActive = true;

        // 重置位置
        identifyBar.anchoredPosition = new Vector2(leftLimit, identifyBar.anchoredPosition.y);
        faceTarget.anchoredPosition = new Vector2(0, faceTarget.anchoredPosition.y);
    }
}
