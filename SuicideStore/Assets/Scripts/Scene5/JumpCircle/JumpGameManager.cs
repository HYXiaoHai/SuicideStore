using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class JumpGameManager : MonoBehaviour
{
    public static JumpGameManager Instance;

    [Header("圆圈设置")]
    public List<GameObject> circlePrefabs = new List<GameObject>();
    public Transform startPoint;
    public Transform endPoint;

    public Image[] sprites;          // 失败时随机显示的图片

    [Header("速度设置（四个圈）")]
    public float[] speeds = new float[4] { 1.5f, 2f, 2.5f, 3f };

    [Header("游戏机Sprite")]
    public SpriteRenderer gameMachineSprite;

    [Header("游戏机的裂痕Sprite")]
    public SpriteRenderer gameMashineCrack;
    public Sprite crack1;
    public Sprite crack2;
    public Sprite crack3;

    [Header("游戏结束UI")]
    public Image finishUI;

    [Header("玩家")]
    public GameObject player;
    public PlayerJumpController playerJump;

    [Header("UI")]
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI promptText;   // 提示文字
    public bool needPrompt = true;

    [Header("跳圈父物体")]
    public GameObject jumpGameFather;

    private JumpCircle currentCircle;
    private int currentIndex = 0;
    private bool isGameActive = false;
    private bool isWaitingForJump = false;
    private int successCount = 0;
    private int maxsuccessCount = 0;
    public bool gameCompleted = false;

    [Header("音效")]
    public AudioClip jumpClip;

    private Coroutine imageShowCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerJump.SetCanJump(false);
        needPrompt = true;

        if (countdownText != null)
        {
            countdownText.text = " ";
            countdownText.gameObject.SetActive(false);
        }
        if (promptText != null)
            promptText.alpha = 0f;

        gameMashineCrack.sprite = null;
    }

    public void StartJumpGame()
    {
        gameCompleted = false;
        playerJump.SetCanJump(true);

        // 提示文字淡入
        if (promptText != null)
            promptText.DOFade(1f, 0.5f);

        StartCircle(0);
    }

    public void StartCircle(int index)
    {
        if (index >= speeds.Length)
        {
            OnAllCirclesPassed();
            return;
        }

        currentIndex = index;
        GameObject newCircle = Instantiate(circlePrefabs[currentIndex], startPoint.position, Quaternion.identity);
        currentCircle = newCircle.GetComponent<JumpCircle>();
        currentCircle.moveSpeed = speeds[currentIndex];

        StartCoroutine(WatchCircleLife());
        isWaitingForJump = true;
    }

    IEnumerator WatchCircleLife()
    {
        while (currentCircle != null && currentCircle.transform.position.x < endPoint.position.x)
            yield return null;
        if (currentCircle != null && isWaitingForJump)
            OnJumpFailed();
    }

    public void OnPlayerJumpLand()
    {
        if (!isWaitingForJump) return;
        if (currentCircle == null) return;

        bool isInCenter = currentCircle.IsPlayerInCenter(player);

        if (isInCenter)
            OnJumpSuccess();
        else
            OnJumpFailed();
    }

    private void OnJumpSuccess()
    {
        isWaitingForJump = false;
        currentCircle.StopMoving();
        AudioManager.Instance.Play2DSound(jumpClip, 0.8f);
        ShakeGameMachine();
        if (currentIndex == 0)
        {
            Debug.Log("教学完成，准备倒计时...");
            Destroy(currentCircle.gameObject);
            StartCoroutine(StartCountdown());
        }
        else
        {
            successCount++;
            maxsuccessCount = Mathf.Max(successCount, maxsuccessCount);

            if (maxsuccessCount == 1)
            {
                gameMachineSprite.DOFade(0.75f, 0.5f);
                gameMashineCrack.sprite = crack1;
                gameMashineCrack.DOFade(0.75f, 0.5f);
            }
            else if (maxsuccessCount == 2)
            {
                gameMachineSprite.DOFade(0.5f, 0.5f);
                gameMashineCrack.sprite = crack2;
                gameMashineCrack.DOFade(0.5f, 0.5f);
            }
            else if (maxsuccessCount == 3)
            {
                // 第三次成功不做特殊处理，胜利时会触发完成
                gameMashineCrack.sprite = crack3;
            }

            if (countdownText != null)
                countdownText.text = successCount.ToString();

            Destroy(currentCircle.gameObject);

            if (currentIndex + 1 < speeds.Length)
                StartCircle(currentIndex + 1);
            else
                OnAllCirclesPassed();
        }
    }

    private IEnumerator StartCountdown()
    {
        playerJump.SetCanJump(false);
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
            yield return new WaitForSeconds(1f);
            countdownText.text = "2";
            yield return new WaitForSeconds(1f);
            countdownText.text = "1";
            yield return new WaitForSeconds(1f);
            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        isGameActive = true;
        successCount = 0;
        if (countdownText != null) countdownText.text = "0";
        playerJump.SetCanJump(true);
        StartCircle(1);
    }

    private void OnJumpFailed()
    {
        // 教学阶段失败
        if (!isGameActive && currentIndex == 0)
        {
            Debug.Log("教学失败，重新教学");
            if (currentCircle != null) Destroy(currentCircle.gameObject);

            //ShakeGameMachine();
            StartCircle(0);
            return;
        }

        // 正式游戏阶段失败
        if (isGameActive)
        {
            if (currentCircle != null) Destroy(currentCircle.gameObject);

            // 显示随机文案（并震动游戏机）
            ShowRandomImage();
            //ShakeGameMachine();

            // 重新开始当前圈（不重置成功计数，但maxsuccessCount保留）
            // 注意：successCount用于显示当前连续成功，但失败后应归零，以免误显示
            //successCount = 0;
            //if (countdownText != null) countdownText.text = "0";

            StartCircle(currentIndex);
        }
    }

    private void ShakeGameMachine()
    {
        if (gameMachineSprite != null)
        {
            gameMachineSprite.transform.DOKill();
            gameMachineSprite.transform.DOShakePosition(0.3f, strength: 0.3f, vibrato: 10, randomness: 90, fadeOut: true);
        }
    }

    private void ShowRandomImage()
    {
        // 停止之前的协程
        if (imageShowCoroutine != null)
        {
            StopCoroutine(imageShowCoroutine);
            // 立即隐藏所有图片
            foreach (var img in sprites)
            {
                img.DOKill();
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
        }
        imageShowCoroutine = StartCoroutine(ShowImageRoutine());
    }

    private IEnumerator ShowImageRoutine()
    {
        int index = Random.Range(0, sprites.Length);
        Image image = sprites[index];
        // 渐显
        image.DOFade(1f, 0.5f);
        // 显示1.2秒
        yield return new WaitForSeconds(1.2f);
        // 渐隐
        image.DOFade(0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        imageShowCoroutine = null;
    }

    private void OnAllCirclesPassed()
    {
        Debug.Log("全部圆圈通过");
        playerJump.SetCanJump(false);

        // 隐藏提示文字
        if (promptText != null)
        {
            promptText.DOKill();
            promptText.gameObject.SetActive(false);
        }
        if(countdownText!=null)
        {
            countdownText.gameObject.SetActive(false);
        }
        // 停止随机图片显示
        if (imageShowCoroutine != null)
        {
            StopCoroutine(imageShowCoroutine);
            imageShowCoroutine = null;
            foreach (var img in sprites)
            {
                img.DOKill();
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            }
        }

        gameCompleted = true;
        if (finishUI != null)
        {
            finishUI.gameObject.SetActive(true);
            finishUI.DOFade(1f, 0.5f);
        }

        Sequence seq = DOTween.Sequence();
        SpriteRenderer[] spriteRenderers = jumpGameFather.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            seq.Join(sr.DOFade(0f, 0.5f));
        }
        seq.OnComplete(() =>
        {
            BagPackingManager.Instance.StartGame();
        });
        seq.Play();
    }
}