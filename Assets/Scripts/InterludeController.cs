using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

public class InterludeController : MonoBehaviour
{
    [System.Serializable]
    private struct DialogueEntry
    {
        public string speaker;
        [TextArea(3, 10)] public string text;
    }

    [System.Serializable]
    private struct DialoguePanel
    {
        public GameObject panelRoot;
        public TextMeshProUGUI speakerText;
        public TextMeshProUGUI dialogueText;
    }

    [Header("通用")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private TMP_FontAsset fontOverride;
    [SerializeField] private bool prewarmFontCharacters = true;
    [SerializeField] private List<TMP_FontAsset> fontFallbacks = new List<TMP_FontAsset>();
    [SerializeField] private bool applyFontToAllTextsUnderCanvas = true;
    [SerializeField] private bool autoSwitchToFallbackWhenMissingGlyphs = true;

    [Header("场景2：图片 + 按钮")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private string hint = "点击按钮继续";

    [Header("场景2：对话框序列")]
    [SerializeField] private List<DialoguePanel> dialogPanels = new List<DialoguePanel>();
    [SerializeField] private bool advancePanelsOnClick = true;

    [Header("场景3：对话")]
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    [SerializeField, TextArea(3, 10)] private List<string> dialogueLines = new List<string>();
    [SerializeField] private float typingSpeed = 0.1f;
    [SerializeField] private GameObject endHint;
    [SerializeField] private bool requireButtonToAdvanceWhenAvailable = true;

    private int currentIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private string currentFullText = "";
    private WaitForSeconds typingWait;
    private float typingWaitValue = -1f;
    private TextMeshProUGUI activeSpeakerText;
    private TextMeshProUGUI activeDialogueText;
    private bool sceneLoadTriggered;
    private bool didWarnButtonBinding;
    private bool didLogFirstClick;
    private bool continueButtonAssignedInInspector;

    void Start()
    {
        continueButtonAssignedInInspector = continueButton != null;

        if (endHint != null) endHint.SetActive(false);
        ApplyFontOverride();
        SetupPanels();
        if (continueButtonAssignedInInspector && continueButton != null && HasDialogue() && requireButtonToAdvanceWhenAvailable)
        {
            continueButton.gameObject.SetActive(false);
        }


        if (continueButton == null && !HasDialogue() && !HasPanelSequence())
        {
            continueButton = GetComponentInChildren<Button>(true);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(LoadNextScene);
            continueButton.onClick.AddListener(LoadNextScene);
            continueButton.transform.SetAsLastSibling();
        }
        else
        {
            if (!didWarnButtonBinding)
            {
                didWarnButtonBinding = true;
                Debug.LogError("InterludeController: 未找到 Continue Button。请在 Inspector 里把场景中的 Button 拖到 Continue Button。");
            }
        }

        Debug.Log($"InterludeController: Start in scene '{SceneManager.GetActiveScene().name}', continueButton={(continueButton != null)}, nextSceneName='{nextSceneName}'");

        if (hintText != null) hintText.text = hint;
        PrewarmCharacters();

        if (TryStartPanelSequence()) return;

        if (dialogueText != null && HasDialogue())
        {
            currentIndex = 0;
            activeSpeakerText = speakerText;
            activeDialogueText = dialogueText;
            DisplayNextLine();
        }
    }

    void Update()
    {
        if (!didLogFirstClick && Input.GetMouseButtonDown(0))
        {
            didLogFirstClick = true;
            Debug.Log($"InterludeController: first click detected. continueButton={(continueButton != null)}, interactable={(continueButton != null && continueButton.interactable)}");
        }

        if (TryButtonClickFallback()) return;

        if (TryUpdatePanelSequence()) return;

        if (activeDialogueText != null && HasDialogue())
        {
            if (requireButtonToAdvanceWhenAvailable && continueButtonAssignedInInspector && continueButton != null) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    StopTypingAndShowFull();
                }
                else
                {
                    currentIndex++;
                    DisplayNextLine();
                }
            }
        }
    }

    void OnDisable()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(LoadNextScene);
    }

    private void ApplyFontOverride()
    {
        if (applyFontToAllTextsUnderCanvas)
        {
            var canvas = GetComponentInParent<Canvas>();
            Transform root = canvas != null ? canvas.transform : transform.root;
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                ApplyFontToText(texts[i]);
            }
        }
        else
        {
            ApplyFontToText(hintText);
            ApplyFontToText(speakerText);
            ApplyFontToText(dialogueText);
        }
    }

    private void ApplyFontToText(TMP_Text tmp)
    {
        if (tmp == null) return;

        if (fontOverride != null)
        {
            tmp.font = fontOverride;
        }
        if (tmp.font != null) tmp.fontSharedMaterial = tmp.font.material;

        ApplyFallbacks(tmp.font);

        var subMeshes = tmp.GetComponentsInChildren<TMP_SubMeshUI>(true);
        for (int i = 0; i < subMeshes.Length; i++)
        {
            if (subMeshes[i] != null) subMeshes[i].sharedMaterial = tmp.fontSharedMaterial;
        }

        if (tmp.color.a <= 0.001f)
        {
            Color c = tmp.color;
            c.a = 1f;
            tmp.color = c;
        }

        tmp.ForceMeshUpdate(true, true);
    }

    private void ApplyFallbacks(TMP_FontAsset baseFont)
    {
        if (baseFont == null) return;
        if (fontFallbacks == null || fontFallbacks.Count == 0) return;

        var table = baseFont.fallbackFontAssetTable;
        bool changed = false;
        if (table == null)
        {
            table = new List<TMP_FontAsset>();
            changed = true;
        }
        foreach (var fb in fontFallbacks)
        {
            if (fb == null) continue;
            if (!table.Contains(fb))
            {
                table.Add(fb);
                changed = true;
            }
        }
        if (changed)
        {
            baseFont.fallbackFontAssetTable = table;
        }
    }

    private void PrewarmCharacters()
    {
        if (!prewarmFontCharacters) return;

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(hint)) sb.Append(hint);

        if (dialogueEntries != null && dialogueEntries.Count > 0)
        {
            for (int i = 0; i < dialogueEntries.Count; i++)
            {
                if (!string.IsNullOrEmpty(dialogueEntries[i].speaker)) sb.Append(dialogueEntries[i].speaker);
                if (!string.IsNullOrEmpty(dialogueEntries[i].text)) sb.Append(dialogueEntries[i].text);
            }
        }
        else if (dialogueLines != null && dialogueLines.Count > 0)
        {
            for (int i = 0; i < dialogueLines.Count; i++)
            {
                if (!string.IsNullOrEmpty(dialogueLines[i])) sb.Append(dialogueLines[i]);
            }
        }

        string allChars = sb.ToString();
        if (allChars.Length == 0) return;

        TryPrewarmFont(hintText, allChars);
        TryPrewarmFont(speakerText, allChars);
        TryPrewarmFont(dialogueText, allChars);

        for (int i = 0; i < dialogPanels.Count; i++)
        {
            TryPrewarmFont(dialogPanels[i].speakerText, allChars);
            TryPrewarmFont(dialogPanels[i].dialogueText, allChars);
        }
    }

    private static void TryPrewarmFont(TextMeshProUGUI tmp, string allChars)
    {
        if (tmp == null) return;
        if (tmp.font == null) return;
        tmp.font.TryAddCharacters(allChars, out _);
    }

    private void ApplyBestFontForText(TextMeshProUGUI tmp, string text)
    {
        if (tmp == null) return;
        if (string.IsNullOrEmpty(text)) return;

        TMP_FontAsset selected = SelectFontThatCanRender(text, tmp.font);
        if (selected == null) return;

        if (tmp.font != selected)
        {
            tmp.font = selected;
        }
        tmp.fontSharedMaterial = selected.material;

        var subMeshes = tmp.GetComponentsInChildren<TMP_SubMeshUI>(true);
        for (int i = 0; i < subMeshes.Length; i++)
        {
            if (subMeshes[i] != null) subMeshes[i].sharedMaterial = tmp.fontSharedMaterial;
        }

        tmp.ForceMeshUpdate(true, true);
    }

    private TMP_FontAsset SelectFontThatCanRender(string text, TMP_FontAsset primary)
    {
        if (primary != null && CanRenderAllCharacters(primary, text)) return primary;

        if (fontOverride != null && CanRenderAllCharacters(fontOverride, text)) return fontOverride;

        if (fontFallbacks != null)
        {
            for (int i = 0; i < fontFallbacks.Count; i++)
            {
                TMP_FontAsset fb = fontFallbacks[i];
                if (fb == null) continue;
                if (CanRenderAllCharacters(fb, text)) return fb;
            }
        }

        return primary != null ? primary : fontOverride;
    }

    private static bool CanRenderAllCharacters(TMP_FontAsset font, string text)
    {
        if (font == null) return false;
        if (string.IsNullOrEmpty(text)) return true;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c) || char.IsControl(c)) continue;
            if (!font.HasCharacter(c, false, true)) return false;
        }
        return true;
    }

    private void DisplayNextLine()
    {
        if (currentIndex < 0) currentIndex = 0;

        int count = GetDialogueCount();
        if (currentIndex < count)
        {
            GetDialogueAt(currentIndex, out string speaker, out string body);
            if (activeSpeakerText != null) activeSpeakerText.text = speaker;

            currentFullText = activeSpeakerText != null ? body : ComposeLine(speaker, body);

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypewriterRoutine(activeDialogueText, currentFullText));

            EnsureTextOnTop(activeSpeakerText);
            EnsureTextOnTop(activeDialogueText);
        }
        else
        {
            if (endHint != null) endHint.SetActive(true);

            if (continueButton != null && requireButtonToAdvanceWhenAvailable)
            {
                continueButton.gameObject.SetActive(true);
                return;
            }

            if (!string.IsNullOrWhiteSpace(nextSceneName)) LoadNextScene();
        }
    }

    private bool HasDialogue()
    {
        return (dialogueEntries != null && dialogueEntries.Count > 0) || (dialogueLines != null && dialogueLines.Count > 0);
    }

    private int GetDialogueCount()
    {
        if (dialogueEntries != null && dialogueEntries.Count > 0) return dialogueEntries.Count;
        return dialogueLines != null ? dialogueLines.Count : 0;
    }

    private void GetDialogueAt(int index, out string speaker, out string body)
    {
        speaker = "";
        body = "";

        if (dialogueEntries != null && dialogueEntries.Count > 0)
        {
            if (index < 0 || index >= dialogueEntries.Count) return;
            speaker = dialogueEntries[index].speaker ?? "";
            body = dialogueEntries[index].text ?? "";
            return;
        }

        if (dialogueLines == null || index < 0 || index >= dialogueLines.Count) return;
        string line = dialogueLines[index] ?? "";

        int split = line.IndexOf('：');
        if (split < 0) split = line.IndexOf(':');

        if (split > 0)
        {
            speaker = line.Substring(0, split).Trim();
            body = line.Substring(split + 1).TrimStart();
        }
        else
        {
            body = line;
        }
    }

    private string ComposeLine(string speaker, string body)
    {
        if (string.IsNullOrEmpty(speaker)) return body ?? "";
        if (string.IsNullOrEmpty(body)) return speaker + "：";
        return speaker + "：" + body;
    }

    private void SetupPanels()
    {
        if (dialogPanels == null || dialogPanels.Count == 0) return;
        for (int i = 0; i < dialogPanels.Count; i++)
        {
            var root = dialogPanels[i].panelRoot;
            if (root != null) root.SetActive(i == 0);
        }
    }

    private bool TryStartPanelSequence()
    {
        if (!HasPanelSequence()) return false;
        currentIndex = 0;
        DisplayPanel(currentIndex);
        return true;
    }

    private bool TryUpdatePanelSequence()
    {
        if (!HasPanelSequence()) return false;
        if (!advancePanelsOnClick) return false;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopTypingAndShowFull();
                return true;
            }

            currentIndex++;
            DisplayPanel(currentIndex);
        }

        return true;
    }

    private bool HasPanelSequence()
    {
        if (dialogPanels == null || dialogPanels.Count == 0) return false;
        for (int i = 0; i < dialogPanels.Count; i++)
        {
            if (dialogPanels[i].panelRoot != null) return true;
        }
        return false;
    }

    private void DisplayPanel(int index)
    {
        if (dialogPanels == null || dialogPanels.Count == 0) return;

        if (index >= dialogPanels.Count)
        {
            if (!string.IsNullOrWhiteSpace(nextSceneName)) LoadNextScene();
            return;
        }

        for (int i = 0; i < dialogPanels.Count; i++)
        {
            var root = dialogPanels[i].panelRoot;
            if (root != null) root.SetActive(i == index);
        }

        var panel = dialogPanels[index];
        var panelDialogue = panel.dialogueText;
        var panelSpeaker = panel.speakerText;
        if (panel.panelRoot != null && (panelDialogue == null || panelSpeaker == null))
        {
            var allTexts = panel.panelRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (panelSpeaker == null && allTexts.Length > 0) panelSpeaker = allTexts[0];
            if (panelDialogue == null)
            {
                for (int i = 0; i < allTexts.Length; i++)
                {
                    if (allTexts[i] == null) continue;
                    if (allTexts[i] != panelSpeaker)
                    {
                        panelDialogue = allTexts[i];
                        break;
                    }
                }
                if (panelDialogue == null && allTexts.Length > 0) panelDialogue = allTexts[0];
            }
        }

        activeDialogueText = panelDialogue;
        activeSpeakerText = panelSpeaker;
        if (activeDialogueText == null) return;

        GetDialogueAt(index, out string speaker, out string body);
        if (activeSpeakerText != null) activeSpeakerText.text = speaker;

        currentFullText = activeSpeakerText != null ? body : ComposeLine(speaker, body);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypewriterRoutine(activeDialogueText, currentFullText));

        if (activeSpeakerText != null) activeSpeakerText.gameObject.SetActive(true);
        activeDialogueText.gameObject.SetActive(true);
        EnsureTextOnTop(activeSpeakerText);
        EnsureTextOnTop(activeDialogueText);
    }

    private static void EnsureTextOnTop(TextMeshProUGUI tmp)
    {
        if (tmp == null) return;
        tmp.transform.SetAsLastSibling();
    }

    private IEnumerator TypewriterRoutine(TextMeshProUGUI target, string text)
    {
        isTyping = true;
        if (target == null)
        {
            isTyping = false;
            yield break;
        }

        if (autoSwitchToFallbackWhenMissingGlyphs)
        {
            ApplyBestFontForText(activeSpeakerText, activeSpeakerText != null ? activeSpeakerText.text : null);
            ApplyBestFontForText(target, text);
        }

        target.text = text;
        target.maxVisibleCharacters = 0;
        target.ForceMeshUpdate(true, true);

        int totalCharacters = target.textInfo.characterCount;
        if (totalCharacters <= 1 && text.Length > 1) totalCharacters = text.Length;

        if (typingWait == null || !Mathf.Approximately(typingWaitValue, typingSpeed))
        {
            typingWaitValue = typingSpeed;
            typingWait = new WaitForSeconds(typingSpeed);
        }

        for (int i = 0; i < totalCharacters; i++)
        {
            if (target == null) break;
            target.maxVisibleCharacters = i + 1;
            yield return typingWait;
        }

        isTyping = false;
    }

    private void StopTypingAndShowFull()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (activeDialogueText != null)
        {
            activeDialogueText.text = currentFullText;
            activeDialogueText.maxVisibleCharacters = int.MaxValue;
        }
        isTyping = false;
    }

    public void LoadNextScene()
    {
        if (sceneLoadTriggered) return;

        var active = SceneManager.GetActiveScene();
        Debug.Log($"InterludeController: LoadNextScene called. active='{active.name}'({active.buildIndex}), nextSceneName='{nextSceneName}', scenesInBuild={SceneManager.sceneCountInBuildSettings}");

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            int next = current + 1;
            if (current >= 0 && next >= 0 && next < SceneManager.sceneCountInBuildSettings)
            {
                sceneLoadTriggered = true;
                Debug.Log($"InterludeController: loading next buildIndex={next} ({Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(next))})");
                SceneManager.LoadScene(next, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("无法加载下一场景：nextSceneName 为空，且 Build Settings 没有下一条场景。");
            }
            return;
        }

        if (TryResolveSceneBuildIndex(nextSceneName, out int index))
        {
            if (index == active.buildIndex)
            {
                Debug.LogError($"InterludeController: 目标场景与当前场景相同（{active.name}）。请检查 Next Scene Name 是否填错。");
            }
            sceneLoadTriggered = true;
            Debug.Log($"InterludeController: loading buildIndex={index} ({Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index))})");
            SceneManager.LoadScene(index, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"无法加载场景 '{nextSceneName}'：请先把它加入 Build Settings 的 Scenes In Build。当前 Scenes In Build：{GetScenesInBuildString()}");
        }
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
            sb.Append(Path.GetFileNameWithoutExtension(p));
        }
        return sb.ToString();
    }

    private bool TryButtonClickFallback()
    {
        if (sceneLoadTriggered) return false;
        if (continueButton == null) return false;
        if (!continueButtonAssignedInInspector) return false;
        if (!continueButton.gameObject.activeInHierarchy) return false;
        if (!continueButton.interactable) return false;
        if (!Input.GetMouseButtonDown(0)) return false;

        var rt = continueButton.transform as RectTransform;
        if (rt == null) return false;

        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (!RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam)) return false;

        LoadNextScene();
        return true;
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

    private static string NormalizeSceneKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        string s = value.Trim();
        while (s.Length > 0 && s[s.Length - 1] == '.') s = s.Substring(0, s.Length - 1);
        return s;
    }
}
