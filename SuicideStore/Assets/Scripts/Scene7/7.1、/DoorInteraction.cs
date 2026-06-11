using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    public ElephantController controller;

    [Header("交互设置")]
    public CanvasGroup endBG;
    public Image image2;

    public string nextSceneName = "Scene7.2";
    public bool canInteractable = false;

    [Header("交互方式")]
    public KeyCode interactKey = KeyCode.E;


    [Header("视觉提示")]
    public SpriteRenderer doorSprite;

    private bool isPlayerNear = false;
    public bool isCompleted = false; //是否已经触发
    public SpriteRenderer interactPrompt;//对应的交互提示
    void Start()
    {

    }
    public void SetInteractable(bool enable)
    {
        if (enable)
        {
            interactPrompt.gameObject.SetActive(true);
            interactPrompt.DOFade(1f, 0.5f).OnComplete(() => {
                canInteractable = true;
                interactPrompt.GetComponent<EFloating>().StartFloating(true);
            });
        }
        else
        {
            interactPrompt.gameObject.SetActive(false);
            canInteractable = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    void Update()
    {
        if (GameManage.Instance.isSetting) return;
        if (isPlayerNear)
        {
            if (canInteractable&&Input.GetKeyDown(interactKey))
            {
                StartCoroutine(Interact());
            }
        }
    }

    void InteractWithDoor()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            // 并行执行转场淡出和 BGM 淡出
            TransitionManage.Instance.FadeOut(0.5f, Color.white, () =>
            {
                // 转场完成后加载新场景
                SceneManager.LoadScene(nextSceneName);
            });
        }
        else
        {
            Debug.LogError("场景名称为空！请检查Next Scene Name设置");
        }
    }

    // 交互逻辑（具体功能在此实现，目前为空）
    public IEnumerator Interact()
    {
        if (isCompleted) yield break;
        isCompleted = true;

        // 1. 淡出提示（不受 Time.timeScale 影响）
        if (interactPrompt != null)
        {
            interactPrompt.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                interactPrompt.GetComponent<EFloating>()?.StartFloating(false);
                interactPrompt.gameObject.SetActive(false);
            });
        }
        yield return new WaitForSecondsRealtime(0.5f);
        controller.canMove = false;
        yield return endBG.DOFade(1f, 1f).WaitForCompletion();

        //等待玩家按下任意键（或指定按键）
        // 等待玩家按下任意键（排除 ESC）
        yield return new WaitUntil(() =>
        {
            if (!Input.anyKeyDown) return false;
            if (Input.GetKeyDown(KeyCode.Escape)) return false;
            return true;
        });
        //yield return new WaitForSeconds(2f);

        //可选：避免同一帧内触发多次，稍微延迟一帧
        yield return null;

        //继续后续动画
        yield return image2.DOFade(1f, 1f).WaitForCompletion();
        // 等待玩家按下任意键（排除 ESC）
        yield return new WaitUntil(() =>
        {
            if (!Input.anyKeyDown) return false;
            if (Input.GetKeyDown(KeyCode.Escape)) return false;
            return true;
        });
        //yield return new WaitForSeconds(2f);
        yield return null;

        // 最后跳转
        InteractWithDoor();
    }
}