using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   // ��Ҫ���� TextMeshPro
using DG.Tweening;
using UnityEngine.UI;

public class JumpGameManager : MonoBehaviour
{
    public static JumpGameManager Instance;

    [Header("Ȧ����")]
    public List<GameObject> circlePrefabs = new List<GameObject>();
    public Transform startPoint;
    public Transform endPoint;

    public Image sprite1;
    public Image sprite2;
    public Image sprite3;

    [Header("�ٶ����ã��ĸ�Ȧ��")]
    public float[] speeds = new float[4] { 1.5f, 2f, 2.5f, 3f };

    [Header("游戏结束UI")]
    public Image finishUI; // 通关弹窗/图片

    [Header("���")]
    public GameObject player;
    public PlayerJumpController playerJump;

    [Header("UI")]
    public TextMeshProUGUI countdownText;   // ����ʱ��ʾ���ı���3,2,1,GO��

    private JumpCircle currentCircle;
    private int currentIndex = 0;           // ��ǰ���ڽ��е�Ȧ������0��ѧ��1,2,3��ʽ��
    private bool isGameActive = false;      // ����ʱ������Ϊtrue����ʾ��ʽ��Ϸ������
    private bool isWaitingForJump = false;
    private int successCount = 0;           // �ɹ���������ʽȦ������0~3��
    private int maxsuccessCount = 0;           // �ɹ���������ʽȦ������0~3��
    public bool gameCompleted = false;
    [Header("��Ч")]
    public AudioClip jumpClip;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerJump.SetCanJump(false);
        // ��ʼ���ػ����UI
        if (countdownText != null) countdownText.text = " ";
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    // �ⲿ���ã�������Ȧ��Ϸ���ɷ�������л��󴥷���
    public void StartJumpGame()
    {
        gameCompleted = false;
        playerJump.SetCanJump(true);
        // ��ʼ��ѧȦ������0��
        StartCircle(0);
    }

    // ��ʼָ��������Ȧ
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
            OnJumpFailed("Ȧ�ѳ�����Χ");
    }

    // ����Ҷ����¼����ã����˲�䣩
    public void OnPlayerJumpLand()
    {
        if (!isWaitingForJump) return;
        if (currentCircle == null) return;

        bool isInCenter = currentCircle.IsPlayerInCenter(player);

        if (isInCenter)
            OnJumpSuccess();
        else
            OnJumpFailed("������������");
    }

    private void OnJumpSuccess()
    {
        isWaitingForJump = false;
        currentCircle.StopMoving();
        AudioManager.Instance.Play2DSound(jumpClip, 0.8f);

        if (currentIndex == 0)
        {
            // 教学圈通过，开启倒计时，不直接进入下一个圈
            Debug.Log("教学完成，准备倒计时...");
            Destroy(currentCircle.gameObject);
            StartCoroutine(StartCountdown());
        }
        else
        {
            // 正式圈通过，计数+1，更新UI
            successCount++;
            maxsuccessCount = Mathf.Max(successCount, maxsuccessCount);
            if (maxsuccessCount == 1)
            {
                sprite1.DOFade(1f, 1f);
            }
            else if (maxsuccessCount == 2)
            {
                sprite1.DOFade(0f, 0.5f);
                sprite2.DOFade(1f, 0.5f);
            }
            else if (maxsuccessCount == 3)
            {
                sprite2.DOFade(0f, 0.5f);
                sprite3.DOFade(1f, 0.5f);
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
            yield return new WaitForSeconds(3f); // ���û��UI���ȴ�3��
        }

        // ����ʱ��������ʽ������Ϸ
        isGameActive = true;
        // ���ü���������Ϊ��ʽ��Ϸ��ʼ��֮ǰ��ѧȦ�����룩
        successCount = 0;
        sprite1.color = new Color(1f, 1f, 1f, 0f);
        sprite2.color = new Color(1f, 1f, 1f, 0f);
        sprite3.color = new Color(1f, 1f, 1f, 0f);
        if (countdownText != null) countdownText.text = "0";
        // �ָ������Ծ����
        playerJump.SetCanJump(true);
        // ��ʼ��ʽ��Ϸ��һ��Ȧ������1�����ڶ���Ȧ��
        StartCircle(1);
    }

    private void OnJumpFailed(string reason)
    {
        if (!isGameActive && currentIndex == 0)
        {
            // ��ѧȦʧ�ܣ���������ѧȦ�������UI����������ʽ��Ϸ��
            Debug.Log("��ѧȦʧ�ܣ����¿�ʼ��ѧȦ");
            if (currentCircle != null) Destroy(currentCircle.gameObject);
            StartCircle(0);
            return;
        }

        if (isGameActive)
        {
            // ������ǰȦ
            if (currentCircle != null) Destroy(currentCircle.gameObject);
            // ���ü���UI
            successCount = 0;
            if (countdownText != null) countdownText.text = "0";
            // ���õ���2��Ȧ������1���������µ���ʱ
            StartCircle(1);
        }
    }

    private void OnAllCirclesPassed()
    {
        Debug.Log("全部圆圈通过");
        playerJump.SetCanJump(false);
        gameCompleted = true;

        // 直接显示UI
        if (finishUI != null)
        {
            finishUI.gameObject.SetActive(true);
            finishUI.DOFade(1f,0.5f);
        }

        // 立刻执行下一个流程
        BagPackingManager.Instance.StartGame();
    }
}