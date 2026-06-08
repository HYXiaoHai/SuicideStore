using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorInteraction : MonoBehaviour
{
    [Header("交互设置")]
    public string nextSceneName = "Scene7.2";
    public SpriteRenderer interactableCopywriting;//显示的交互文案
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
        if (interactableCopywriting != null)
        {
            interactableCopywriting.gameObject.SetActive(false);
        }
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

        if (interactableCopywriting != null)
        {
            interactableCopywriting.gameObject.SetActive(true);
                                                               
            Color color = interactableCopywriting.color;
            color.a = 0f;
            interactableCopywriting.color = color;

            yield return interactableCopywriting.DOFade(1f, 0.5f).SetUpdate(true).WaitForCompletion();

            yield return new WaitForSecondsRealtime(1f);  
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

        // 4. 跳转场景
        InteractWithDoor();
    }
}