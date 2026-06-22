using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class PhotoTrigger : MonoBehaviour
{
    public int photoIndex;
    public PhotoSystem photoSystem;
    public SpriteRenderer ePrompt;
    public AudioClip clickAudioClip;
    
    public SpriteRenderer interacRenderer;//交互后图片
    public SpriteRenderer[] downRenderers;//交互后图片
    public Light2D[] downlights;

    public SpriteRenderer completeSprite1;//最后一个

    private bool isPlayerInside = false;
    private bool isTriggered = false;
    private SpriteRenderer defualSprite;//默认

    private void Start()
    {
        defualSprite = GetComponent<SpriteRenderer>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (photoSystem.currentPhotoIndex != photoIndex) return;
        if (!other.CompareTag("Player")) return;
        if (isTriggered) return;
        isPlayerInside = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (photoSystem.currentPhotoIndex != photoIndex) return;
        if (!other.CompareTag("Player")) return;
        isPlayerInside = false;
        HidePrompt();
    }
        
    void Update()
    {
        if (!isPlayerInside || isTriggered) return;
        if (photoSystem.currentPhotoIndex!=photoIndex) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            isTriggered = true;
            HidePrompt();
            OnInterac();
            AudioManager.Instance.PlayShortSound(clickAudioClip, 0.8f);
            photoSystem.OnPhotoTrigger(photoIndex);
        }
    }
    void OnInterac()
    {
        defualSprite.DOFade(0f,0.5f);
        interacRenderer.DOFade(1f,0.5f);
        foreach (var d in downRenderers)
        {
            d.DOFade(1f, 0.5f);
        }
        foreach (var l in downlights)
        {
            DOTween.To(() => l.intensity, x => l.intensity = x, 0.4f, 0.5f).SetEase(Ease.OutQuad);
            //l.DOFade(1f, 0.5f);
        }
        //downRenderer.DOFade(1f,0.5f);
        if (completeSprite1!=null)
        completeSprite1.DOFade(1f, 0.5f);
    }

    void ShowPrompt()
    {
        if (ePrompt == null) return;
        ePrompt.gameObject.SetActive(true);
        ePrompt.transform.localScale = Vector3.zero;
        ePrompt.DOFade(1f, 0.2f).SetEase(Ease.OutQuad);
        ePrompt.transform.DOScale(2.3f, 0.25f).SetEase(Ease.OutElastic, 0.8f, 0.5f);
    }

    void HidePrompt()
    {
        if (ePrompt == null) return;
        ePrompt.DOFade(0f, 0.1f).OnComplete(() =>
        {
            ePrompt.gameObject.SetActive(false);
        });
        ePrompt.transform.DOScale(0f, 0.1f);
    }
}