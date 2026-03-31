using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FaceGameManager : MonoBehaviour
{
    [Header("UI引用")]
    public RectTransform identifyBar;
    public RectTransform faceTarget;
    public Image progressFill;
    public Image characterFace;
    public GameObject successText;
    public Canvas gameCanvas;

    [Header("表情图片")]
    public Sprite happyFace;
    public Sprite sadFace;

    [Header("进度参数")]
    public float progressAddSpeed = 0.25f;
    public float progressReduceSpeed = 0.15f;
    public float initialProgress = 0.5f;

    [Header("收获效果")]
    public ParticleSystem harvestEffect;
    public float harvestDelay = 1f;

    private float currentProgress = 0f;
    private float barHalfWidth;
    private float faceHalfWidth;
    private bool isGameActive = true;

    void Start()
    {
        if (identifyBar == null) Debug.LogError("FaceGameManager: identifyBar 未赋值！");
        if (faceTarget == null) Debug.LogError("FaceGameManager: faceTarget 未赋值！");
        if (progressFill == null) Debug.LogError("FaceGameManager: progressFill 未赋值！");
        if (characterFace == null) Debug.LogError("FaceGameManager: characterFace 未赋值！");
        if (successText == null) Debug.LogError("FaceGameManager: successText 未赋值！");
        if (happyFace == null) Debug.LogError("FaceGameManager: happyFace 未赋值！");
        if (sadFace == null) Debug.LogError("FaceGameManager: sadFace 未赋值！");
        
        if (identifyBar != null) barHalfWidth = identifyBar.sizeDelta.x / 2;
        if (faceTarget != null) faceHalfWidth = faceTarget.sizeDelta.x / 2;
        
        successText.SetActive(false);
        currentProgress = initialProgress;
        if (progressFill != null) progressFill.fillAmount = currentProgress;
        
        Debug.Log("FaceGameManager 初始化完成");
    }

    void Update()
    {
        if (!isGameActive) return;

        bool isCaught = CheckCatch();
        UpdateCharacterFace(isCaught);
        UpdateProgress(isCaught);
        CheckGameEnd();
    }

    bool CheckCatch()
    {
        float barCenter = identifyBar.anchoredPosition.x;
        float faceCenter = faceTarget.anchoredPosition.x;
        
        float barLeft = barCenter - barHalfWidth;
        float barRight = barCenter + barHalfWidth;
        float faceLeft = faceCenter - faceHalfWidth;
        float faceRight = faceCenter + faceHalfWidth;
        
        return faceLeft >= barLeft && faceRight <= barRight;
    }

    void UpdateCharacterFace(bool isCaught)
    {
        characterFace.sprite = isCaught ? happyFace : sadFace;
    }

    void UpdateProgress(bool isCaught)
    {
        if (isCaught)
        {
            currentProgress += progressAddSpeed * Time.deltaTime;
        }
        else
        {
            currentProgress -= progressReduceSpeed * Time.deltaTime;
        }
        
        currentProgress = Mathf.Clamp01(currentProgress);
        progressFill.fillAmount = currentProgress;
        
        Color progressColor = Color.Lerp(Color.red, Color.green, currentProgress);
        progressFill.color = progressColor;
    }

    void CheckGameEnd()
    {
        if (currentProgress >= 1f)
        {
            GameWin();
        }
        else if (currentProgress <= 0f)
        {
            ResetGame();
        }
    }

    void GameWin()
    {
        isGameActive = false;
        successText.SetActive(true);
        Debug.Log("钓鱼成功！");
        StartCoroutine(PlayHarvestEffectAndCloseUI());
    }

    IEnumerator PlayHarvestEffectAndCloseUI()
    {
        if (harvestEffect != null)
        {
            harvestEffect.Play();
        }
        
        yield return new WaitForSeconds(harvestDelay);
        
        if (gameCanvas != null)
        {
            gameCanvas.gameObject.SetActive(false);
        }
        
        Debug.Log("UI已关闭");
    }

    void ResetGame()
    {
        Debug.Log("钓鱼失败，重置游戏");
        currentProgress = initialProgress;
        progressFill.fillAmount = currentProgress;
        
        RectTransform trackRect = GameObject.Find("BgTrack")?.GetComponent<RectTransform>();
        if (trackRect != null)
        {
            float trackHalfWidth = trackRect.sizeDelta.x / 2;
            float barPosX = -trackHalfWidth + barHalfWidth;
            identifyBar.anchoredPosition = new Vector2(barPosX, identifyBar.anchoredPosition.y);
        }
        
        faceTarget.anchoredPosition = new Vector2(0, faceTarget.anchoredPosition.y);
    }

    public void RestartGame()
    {
        if (gameCanvas != null)
        {
            gameCanvas.gameObject.SetActive(true);
        }
        
        successText.SetActive(false);
        isGameActive = true;
        ResetGame();
        Debug.Log("游戏已重新开始");
    }
}
