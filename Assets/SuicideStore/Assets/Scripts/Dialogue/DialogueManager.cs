using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
public class DialogueManager : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject endHint; // 结束提示

    [Header("对话")]
    [SerializeField, TextArea(3, 10)]
    private List<string> dialogueLines = new List<string>()
    {
        "害怕离去吗",
        "请不要担心",
        "顿丘会让你的灵魂得以安栖",
        "无需心忧，无需哭泣",
        "花开花落是自然的规律",
        "生命女神在顿丘拥抱你",
        "...",
        "但请不要忘却顿丘外的爱与经历",
        "家人与朋友在你熟悉之处等你"
    };

    [SerializeField] private float typingSpeed = 0.1f;

    [Header("揭示遮罩")]
    [SerializeField] private bool enableRevealMask = true;
    [SerializeField] private int revealStartAfterLineIndex = 4;
    [SerializeField] private RawImage revealMask;
    [SerializeField] private int revealMaskTextureSize = 256;
    [SerializeField] private float revealInnerRadius = 0.22f;
    [SerializeField] private float revealOuterRadius = 0.95f;
    [SerializeField] private float revealEase = 1.6f;
    [SerializeField] private Image blackOverlayImage;
    [SerializeField] private bool fadeBlackOverlayWithReveal = true;

    [Header("对话结束转场")]
    [SerializeField] private bool loadSceneOnDialogueEnd = true;
    [SerializeField] private string nextSceneName = "Scene02_Interlude";

    [Header("文字粒子效果")]
    [SerializeField] private bool enableTextParticles = true;
    [SerializeField] private RectTransform particleRoot;
    [SerializeField] private int particlesPerCharacter = 8;
    [SerializeField] private float particleLifetime = 0.9f;
    [SerializeField] private float particleSpeed = 70f;
    [SerializeField] private float particleRise = 28f;
    [SerializeField] private float particleTurbulence = 55f;
    [SerializeField] private float particleTurbulenceFrequency = 0.85f;
    [SerializeField] private float particleSpawnRadius = 10f;
    [SerializeField] private Vector2 particleSizeRange = new Vector2(10f, 22f);
    [SerializeField] private float particleDamping = 2.2f;
    [SerializeField] private float particleScaleEnd = 2.2f;
    [SerializeField] private Color particleTint = new Color(1f, 1f, 1f, 0.38f);
    [SerializeField] private int prewarmDotPool = 128;
    [SerializeField] private bool prewarmFontCharacters = true;

    private int currentIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    private struct ParticleDot
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Velocity;
        public float Age;
        public float Lifetime;
        public float StartAlpha;
        public float StartScale;
        public float NoiseSeed;
    }

    private readonly List<ParticleDot> activeDots = new List<ParticleDot>();
    private readonly List<Image> dotPool = new List<Image>();
    private Sprite dotSprite;
    private Canvas rootCanvas;
    private Texture2D dotTexture;
    private float particleTime;
    private WaitForSeconds typingWait;
    private float typingWaitValue = -1f;

    private Texture2D revealTexture;
    private int revealStartLineIndex = -1;
    private int revealTotalChars;
    private int revealShownChars;
    private bool revealFinished;

    void Start()
    {
        // 初始检查
        if (dialogueText == null)
        {
            Debug.LogError("DialogueManager: 请在 Inspector 中分配 DialogueText (TextMeshProUGUI)!");
            return;
        }

        if (endHint != null) endHint.SetActive(false);
        EnsureParticleRoot();
        PrewarmTextMeshProCharacters();
        EnsureRevealMask();
        EnsureBlackOverlay();
        
        // 开始第一句
        StartDialogue();
    }

    void Update()
    {
        // 检测玩家输入：鼠标左键点击 或 空格键
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayerInput();
        }

        particleTime += Time.deltaTime;
        UpdateParticles(Time.deltaTime);
    }

    private void StartDialogue()
    {
        currentIndex = 0;
        DisplayNextLine();
    }

    private void OnPlayerInput()
    {
        if (isTyping)
        {
            // 如果正在打字，点击则直接显示全文
            StopTypingAndShowFull();
        }
        else
        {
            // 如果已经显示完当前行，点击进入下一行
            currentIndex++;
            DisplayNextLine();
        }
    }

    private void DisplayNextLine()
    {
        if (currentIndex < dialogueLines.Count)
        {
            currentFullText = dialogueLines[currentIndex];

            EnsureParticleRoot();
            EnsureRevealMask();
            EnsureBlackOverlay();

            if (enableRevealMask && revealStartLineIndex < 0)
            {
                int startLine = Mathf.Clamp(revealStartAfterLineIndex + 1, 0, dialogueLines.Count);
                if (currentIndex == startLine)
                {
                    BeginRevealFrom(startLine);
                }
            }
            
            // 停止之前的协程（如果有）
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            
            // 开始打字机效果
            typingCoroutine = StartCoroutine(TypewriterRoutine(currentFullText));
        }
        else
        {
            // 对话结束
            HandleDialogueEnd();
        }
    }

    private static bool TryResolveSceneBuildIndex(string sceneNameOrPath, out int buildIndex)
    {
        buildIndex = -1;
        if (string.IsNullOrWhiteSpace(sceneNameOrPath)) return false;

        string key = NormalizeSceneKey(sceneNameOrPath);
        bool looksLikePath = key.IndexOf('/') >= 0 || key.IndexOf('\\') >= 0 || key.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase);
        if (looksLikePath)
        {
            string normalizedKey = key.Replace('\\', '/');
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string p = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(p)) continue;
                if (string.Equals(p.Replace('\\', '/'), normalizedKey, System.StringComparison.OrdinalIgnoreCase))
                {
                    buildIndex = i;
                    return true;
                }
            }
            return false;
        }

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string p = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(p)) continue;
            string name = NormalizeSceneKey(Path.GetFileNameWithoutExtension(p));
            if (string.Equals(name, key, System.StringComparison.OrdinalIgnoreCase))
            {
                buildIndex = i;
                return true;
            }
        }

        return false;
    }

    private static string GetScenesInBuildString()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        if (count <= 0) return "(空)";

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            string p = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(p)) continue;
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(NormalizeSceneKey(Path.GetFileNameWithoutExtension(p)));
        }
        return sb.ToString();
    }

    private static string NormalizeSceneKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string s = value.Trim();
        while (s.Length > 0 && s[s.Length - 1] == '.') s = s.Substring(0, s.Length - 1);
        return s;
    }

    private IEnumerator TypewriterRoutine(string text)
    {
        isTyping = true;
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate(true, true);

        int totalCharacters = dialogueText.textInfo.characterCount;
        if (totalCharacters <= 1 && text.Length > 1) totalCharacters = text.Length;

        if (typingWait == null || !Mathf.Approximately(typingWaitValue, typingSpeed))
        {
            typingWaitValue = typingSpeed;
            typingWait = new WaitForSeconds(typingSpeed);
        }
        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            EmitParticlesAtCharacter(i);
            StepReveal(1);
            yield return typingWait;
        }

        isTyping = false;
    }

    private void StopTypingAndShowFull()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (dialogueText != null)
        {
            int remaining = Mathf.Max(0, currentFullText.Length - dialogueText.maxVisibleCharacters);
            if (remaining > 0) StepReveal(remaining);
        }
        dialogueText.text = currentFullText;
        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
    }

    private void HandleDialogueEnd()
    {
        Debug.Log("对话已结束");
        if (loadSceneOnDialogueEnd && !string.IsNullOrWhiteSpace(nextSceneName))
        {
            if (TryResolveSceneBuildIndex(nextSceneName, out int index))
            {
                SceneManager.LoadScene(index, LoadSceneMode.Single);
                return;
            }

            Debug.LogError($"无法加载场景 '{nextSceneName}'：请先把它加入 Build Settings 的 Scenes In Build。当前 Scenes In Build：{GetScenesInBuildString()}");
        }

        if (endHint != null) endHint.SetActive(true);
    }

    void OnDestroy()
    {
        if (revealTexture != null)
        {
            Destroy(revealTexture);
            revealTexture = null;
        }
    }

    private void EnsureRevealMask()
    {
        if (!enableRevealMask) return;

        if (rootCanvas == null) rootCanvas = dialogueText.canvas != null ? dialogueText.canvas : dialogueText.GetComponentInParent<Canvas>();
        if (revealMask == null)
        {
            Transform parent = rootCanvas != null ? rootCanvas.transform : dialogueText.transform.parent;
            var go = new GameObject("RevealMask", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            revealMask = go.GetComponent<RawImage>();
            revealMask.raycastTarget = false;
            revealMask.uvRect = new Rect(0f, 0f, 1f, 1f);
            int sibling = dialogueText.transform.GetSiblingIndex();
            rt.SetSiblingIndex(Mathf.Clamp(sibling, 0, parent.childCount - 1));
        }

        if (revealTexture == null)
        {
            int size = Mathf.Clamp(revealMaskTextureSize, 64, 1024);
            revealTexture = CreateVignetteTexture(size, Mathf.Clamp01(revealInnerRadius), Mathf.Clamp01(revealOuterRadius));
            revealMask.texture = revealTexture;
            revealMask.color = new Color(0f, 0f, 0f, 0f);
        }
    }

    private void EnsureBlackOverlay()
    {
        if (!enableRevealMask) return;
        if (blackOverlayImage != null) return;
        if (rootCanvas == null) rootCanvas = dialogueText.canvas != null ? dialogueText.canvas : dialogueText.GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        Transform t = rootCanvas.transform.Find("BlackBG");
        if (t == null) return;
        blackOverlayImage = t.GetComponent<Image>();
    }

    private static Texture2D CreateVignetteTexture(int size, float innerRadius, float outerRadius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[size * size];
        float half = (size - 1) * 0.5f;
        float invHalf = 1f / half;
        float inner = Mathf.Max(0f, innerRadius);
        float outer = Mathf.Max(inner + 0.0001f, outerRadius);

        for (int y = 0; y < size; y++)
        {
            float ny = (y - half) * invHalf;
            for (int x = 0; x < size; x++)
            {
                float nx = (x - half) * invHalf;
                float r = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny));
                float t = Mathf.InverseLerp(inner, outer, r);
                t = t * t * (3f - 2f * t);
                byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(t * 255f), 0, 255);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }

    private void BeginRevealFrom(int startLineIndex)
    {
        revealStartLineIndex = startLineIndex;
        revealTotalChars = 0;
        revealShownChars = 0;
        revealFinished = false;

        for (int i = startLineIndex; i < dialogueLines.Count; i++)
        {
            if (!string.IsNullOrEmpty(dialogueLines[i])) revealTotalChars += dialogueLines[i].Length;
        }

        if (revealMask != null)
        {
            Color c = revealMask.color;
            c.a = 1f;
            revealMask.color = c;
        }

        if (blackOverlayImage != null && fadeBlackOverlayWithReveal)
        {
            Color c = blackOverlayImage.color;
            c.a = 1f;
            blackOverlayImage.color = c;
        }
    }

    private void StepReveal(int chars)
    {
        if (!enableRevealMask) return;
        if (revealStartLineIndex < 0) return;
        if (revealMask == null) return;
        if (revealFinished) return;
        if (currentIndex < revealStartLineIndex) return;
        if (revealTotalChars <= 0) return;

        revealShownChars = Mathf.Clamp(revealShownChars + Mathf.Max(0, chars), 0, revealTotalChars);
        float p = Mathf.Clamp01((float)revealShownChars / revealTotalChars);
        float eased = Mathf.Pow(p, Mathf.Max(0.01f, revealEase));
        float a = 1f - eased;
        Color c = revealMask.color;
        c.a = a;
        revealMask.color = c;

        if (blackOverlayImage != null && fadeBlackOverlayWithReveal)
        {
            Color bc = blackOverlayImage.color;
            bc.a = a;
            blackOverlayImage.color = bc;
        }

        if (revealShownChars >= revealTotalChars)
        {
            revealFinished = true;
            if (revealMask != null) revealMask.gameObject.SetActive(false);
            if (blackOverlayImage != null && fadeBlackOverlayWithReveal) blackOverlayImage.gameObject.SetActive(false);
        }
    }

    private void EnsureParticleRoot()
    {
        if (!enableTextParticles) return;

        rootCanvas = dialogueText.canvas != null ? dialogueText.canvas : dialogueText.GetComponentInParent<Canvas>();
        if (dotSprite == null)
        {
            const int size = 32;
            dotTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            dotTexture.wrapMode = TextureWrapMode.Clamp;
            dotTexture.filterMode = FilterMode.Bilinear;

            float half = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half;
                    float dy = (y - half) / half;
                    float r = Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy));
                    float a = Mathf.Exp(-r * r * 6.5f);
                    dotTexture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            dotTexture.Apply();
            dotSprite = Sprite.Create(dotTexture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        if (particleRoot == null)
        {
            Transform parent = rootCanvas != null ? rootCanvas.transform : dialogueText.transform.parent;
            var go = new GameObject("DialogueParticles", typeof(RectTransform));
            particleRoot = go.GetComponent<RectTransform>();
            particleRoot.SetParent(parent, false);
            particleRoot.anchorMin = Vector2.zero;
            particleRoot.anchorMax = Vector2.one;
            particleRoot.offsetMin = Vector2.zero;
            particleRoot.offsetMax = Vector2.zero;
            particleRoot.localScale = Vector3.one;
            int siblingIndex = dialogueText.transform.GetSiblingIndex();
            particleRoot.SetSiblingIndex(siblingIndex);
        }
        else
        {
            int dialogueIndex = dialogueText.transform.GetSiblingIndex();
            if (particleRoot.GetSiblingIndex() > dialogueIndex) particleRoot.SetSiblingIndex(dialogueIndex);
        }

        PrewarmDotPool();
        PrewarmActiveDotsCapacity();
    }

    private int ComputeRequiredDotPool()
    {
        if (!enableTextParticles) return 0;

        float safeTyping = Mathf.Max(0.001f, typingSpeed);
        int charsInLifetime = Mathf.CeilToInt(particleLifetime / safeTyping) + 2;
        int required = Mathf.Max(0, particlesPerCharacter) * Mathf.Max(0, charsInLifetime);
        required = Mathf.CeilToInt(required * 1.25f) + 16;
        return Mathf.Clamp(required, 0, 3000);
    }

    private void PrewarmDotPool()
    {
        int target = Mathf.Max(0, prewarmDotPool, ComputeRequiredDotPool());
        int need = target - dotPool.Count;
        if (need <= 0) return;

        if (dotPool.Capacity < target) dotPool.Capacity = target;
        for (int i = 0; i < need; i++)
        {
            var go = new GameObject("Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var img = go.GetComponent<Image>();
            img.sprite = dotSprite;
            img.raycastTarget = false;
            img.type = Image.Type.Simple;
            img.gameObject.SetActive(false);
            img.transform.SetParent(particleRoot, false);
            dotPool.Add(img);
        }
    }

    private void PrewarmActiveDotsCapacity()
    {
        if (!enableTextParticles) return;

        int maxLineLength = 0;
        for (int i = 0; i < dialogueLines.Count; i++)
        {
            if (dialogueLines[i] != null && dialogueLines[i].Length > maxLineLength)
            {
                maxLineLength = dialogueLines[i].Length;
            }
        }

        int target = Mathf.Max(ComputeRequiredDotPool(), Mathf.Max(0, particlesPerCharacter) * Mathf.Max(16, maxLineLength));
        if (activeDots.Capacity < target) activeDots.Capacity = target;
    }

    private void PrewarmTextMeshProCharacters()
    {
        if (!prewarmFontCharacters) return;
        if (dialogueText == null || dialogueText.font == null) return;
        if (dialogueLines == null || dialogueLines.Count == 0) return;

        var sb = new StringBuilder();
        for (int i = 0; i < dialogueLines.Count; i++)
        {
            if (!string.IsNullOrEmpty(dialogueLines[i])) sb.Append(dialogueLines[i]);
        }

        string allChars = sb.ToString();
        if (allChars.Length == 0) return;

        dialogueText.font.TryAddCharacters(allChars, out _);
    }

    private void EmitParticlesAtCharacter(int charIndex)
    {
        if (!enableTextParticles) return;
        if (particleRoot == null || dialogueText == null) return;
        if (charIndex < 0 || charIndex >= dialogueText.textInfo.characterCount) return;

        var charInfo = dialogueText.textInfo.characterInfo[charIndex];
        if (!charInfo.isVisible) return;

        Vector3 localCenter = (charInfo.bottomLeft + charInfo.topRight) * 0.5f;
        Vector3 world = dialogueText.rectTransform.TransformPoint(localCenter);
        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(particleRoot, screen, cam, out Vector2 localPoint)) return;

        Color baseColor = dialogueText.color;
        float baseAlpha = baseColor.a;
        Color dotColor = new Color(
            baseColor.r * particleTint.r,
            baseColor.g * particleTint.g,
            baseColor.b * particleTint.b,
            particleTint.a * baseAlpha
        );

        int count = Mathf.Max(0, particlesPerCharacter);
        for (int i = 0; i < count; i++)
        {
            Image img = GetDotImage();
            var rt = (RectTransform)img.transform;
            rt.anchoredPosition = localPoint + Random.insideUnitCircle * particleSpawnRadius;
            float size = Random.Range(particleSizeRange.x, particleSizeRange.y);
            rt.sizeDelta = new Vector2(size, size);
            rt.localScale = Vector3.one * Random.Range(0.65f, 1.05f);
            img.color = dotColor;

            Vector2 dir = Random.insideUnitCircle;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
            dir.Normalize();
            Vector2 vel = dir * Random.Range(particleSpeed * 0.15f, particleSpeed);
            vel += new Vector2(0f, particleRise * Random.Range(0.6f, 1.25f));

            activeDots.Add(new ParticleDot
            {
                Rect = rt,
                Image = img,
                Velocity = vel,
                Age = 0f,
                Lifetime = Mathf.Max(0.05f, particleLifetime * Random.Range(0.85f, 1.25f)),
                StartAlpha = dotColor.a,
                StartScale = rt.localScale.x,
                NoiseSeed = Random.Range(0.1f, 1000f)
            });
        }
    }

    private Image GetDotImage()
    {
        Image img;
        if (dotPool.Count > 0)
        {
            int last = dotPool.Count - 1;
            img = dotPool[last];
            dotPool.RemoveAt(last);
        }
        else
        {
            var go = new GameObject("Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            img = go.GetComponent<Image>();
            img.sprite = dotSprite;
            img.raycastTarget = false;
            img.type = Image.Type.Simple;
        }

        img.gameObject.SetActive(true);
        img.transform.SetParent(particleRoot, false);
        return img;
    }

    private void ReleaseDot(Image img)
    {
        if (img == null) return;
        img.gameObject.SetActive(false);
        dotPool.Add(img);
    }

    private void UpdateParticles(float dt)
    {
        if (!enableTextParticles) return;
        if (activeDots.Count == 0) return;

        float damping = Mathf.Max(0f, particleDamping);
        for (int i = activeDots.Count - 1; i >= 0; i--)
        {
            ParticleDot dot = activeDots[i];
            dot.Age += dt;

            float t = dot.Lifetime <= 0f ? 1f : Mathf.Clamp01(dot.Age / dot.Lifetime);
            float turb = Mathf.Max(0f, particleTurbulence);
            float freq = Mathf.Max(0.01f, particleTurbulenceFrequency);
            float nx = Mathf.PerlinNoise(dot.NoiseSeed, particleTime * freq) - 0.5f;
            float ny = Mathf.PerlinNoise(dot.NoiseSeed + 31.7f, particleTime * freq) - 0.5f;
            Vector2 noise = new Vector2(nx, ny) * (turb * 2f);

            dot.Velocity += (Vector2.up * Mathf.Max(0f, particleRise) + noise) * dt;
            dot.Velocity = Vector2.Lerp(dot.Velocity, Vector2.zero, 1f - Mathf.Exp(-damping * dt));
            dot.Rect.anchoredPosition += dot.Velocity * dt;

            float ease = t * t * (3f - 2f * t);
            float alpha = Mathf.Lerp(dot.StartAlpha, 0f, ease);
            Color c = dot.Image.color;
            c.a = alpha;
            dot.Image.color = c;

            float endScale = Mathf.Max(0.01f, particleScaleEnd);
            float scale = Mathf.Lerp(dot.StartScale, endScale, ease);
            dot.Rect.localScale = new Vector3(scale, scale, 1f);

            if (t >= 1f)
            {
                ReleaseDot(dot.Image);
                activeDots.RemoveAt(i);
            }
            else
            {
                activeDots[i] = dot;
            }
        }
    }
}
