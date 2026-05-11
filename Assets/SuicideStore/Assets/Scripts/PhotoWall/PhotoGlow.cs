using UnityEngine;

public class PhotoGlow : MonoBehaviour
{
    [Header("发光设置")]
    public Color glowColor = Color.yellow;
    public float glowIntensity = 2f;
    public bool isGlowing = true;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();

        if (isGlowing)
        {
            StartGlow();
        }
    }

    public void StartGlow()
    {
        isGlowing = true;
        UpdateGlowEffect();
    }

    public void StopGlow()
    {
        isGlowing = false;
        UpdateGlowEffect();
    }

    void UpdateGlowEffect()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.GetPropertyBlock(materialPropertyBlock);

        if (isGlowing)
        {
            materialPropertyBlock.SetColor("_GlowColor", glowColor * glowIntensity);
            materialPropertyBlock.SetFloat("_GlowIntensity", glowIntensity);
            spriteRenderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
        else
        {
            materialPropertyBlock.SetColor("_GlowColor", Color.black);
            materialPropertyBlock.SetFloat("_GlowIntensity", 0f);
            spriteRenderer.material.SetColor("_EmissionColor", Color.black);
        }

        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}
