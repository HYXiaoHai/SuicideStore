using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM 设置")]
    public AudioSource bgmSource;
    public float defaultFadeDuration = 1f;
    public List<SceneBGMEntry> sceneBGMList;

    [Header("音效设置")]
    public GameObject audioSourcePrefab;
    public Transform audioSourcePoolParent;

    // 音量级别（0-10）
    private int masterVolume = 10;
    private int bgmVolume = 10;
    private int sfxVolume = 10;

    // 私有变量
    private Dictionary<string, AudioClip> sceneBGMDict;
    private AudioClip currentBGMClip;
    private Coroutine bgmFadeRoutine;
    private List<AudioSource> activeLoopingSources;

    [System.Serializable]
    public class SceneBGMEntry
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        sceneBGMDict = new Dictionary<string, AudioClip>();
        foreach (var entry in sceneBGMList)
        {
            if (!sceneBGMDict.ContainsKey(entry.sceneName))
                sceneBGMDict.Add(entry.sceneName, entry.bgmClip);
            else
                Debug.LogWarning($"场景 {entry.sceneName} 的 BGM 映射重复");
        }

        activeLoopingSources = new List<AudioSource>();

        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // 加载音量设置
        LoadVolumes();
        ApplyVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // ===================== 音量控制接口 =====================
    public int GetMasterVolume() => masterVolume;
    public int GetBGMVolume() => bgmVolume;
    public int GetSFXVolume() => sfxVolume;

    public void SetMasterVolume(int value)
    {
        masterVolume = Mathf.Clamp(value, 0, 10);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetBGMVolume(int value)
    {
        bgmVolume = Mathf.Clamp(value, 0, 10);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSFXVolume(int value)
    {
        sfxVolume = Mathf.Clamp(value, 0, 10);
        ApplyVolumes();
        UpdateAllLoopingSoundsVolume();  // 更新正在播放的音效音量
        SaveVolumes();
    }

    private void ApplyVolumes()
    {
        // BGM 最终音量 = (全局音量/10) * (背景音量/10)
        float bgmFinalVolume = (masterVolume / 10f) * (bgmVolume / 10f);
        bgmSource.volume = bgmFinalVolume;
    }

    private void UpdateAllLoopingSoundsVolume()
    {
        float sfxFinalVolume = (masterVolume / 10f) * (sfxVolume / 10f);
        foreach (var src in activeLoopingSources)
        {
            if (src != null)
                src.volume = sfxFinalVolume;
        }
    }

    private void SaveVolumes()
    {
        PlayerPrefs.SetInt("MasterVolume", masterVolume);
        PlayerPrefs.SetInt("BGMVolume", bgmVolume);
        PlayerPrefs.SetInt("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetInt("MasterVolume", 10);
        bgmVolume = PlayerPrefs.GetInt("BGMVolume", 10);
        sfxVolume = PlayerPrefs.GetInt("SFXVolume", 10);
    }

    // 为音效计算最终音量（短音效调用时使用）
    private float GetFinalSFXVolume(float originalScale = 1f)
    {
        return (masterVolume / 10f) * (sfxVolume / 10f) * originalScale;
    }

    // ===================== BGM 原有方法 =====================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name, defaultFadeDuration);
    }

    public void PlayBGMForScene(string sceneName, float fadeDuration = -1)
    {
        if (sceneBGMDict.TryGetValue(sceneName, out AudioClip targetClip))
            PlayBGM(targetClip, fadeDuration);
        else
        {
            StopBGM();
            Debug.LogWarning($"场景 {sceneName} 没有配置 BGM，已停止音乐");
        }
    }

    public void PlayBGM(AudioClip newClip, float fadeDuration = -1)
    {
        if (newClip == null) return;
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        if (currentBGMClip == newClip && bgmSource.isPlaying) return;

        if (bgmFadeRoutine != null)
            StopCoroutine(bgmFadeRoutine);

        bgmFadeRoutine = StartCoroutine(CrossFadeBGM(newClip, fadeDuration));
    }

    public void SwitchBGM(AudioClip newClip, float fadeDuration = -1) => PlayBGM(newClip, fadeDuration);

    public void PauseBGM() => bgmSource?.Pause();
    public void ResumeBGM()
    {
        if (bgmSource != null && currentBGMClip != null)
            bgmSource.UnPause();
    }

    public void StopBGM()
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmSource.Stop();
        currentBGMClip = null;
    }

    private IEnumerator CrossFadeBGM(AudioClip newClip, float duration)
    {
        if (currentBGMClip == null || bgmSource.clip == null)
        {
            bgmSource.clip = newClip;
            bgmSource.volume = 0f;
            bgmSource.Play();
            float timer = 0f;
            while (timer < duration)
            {
                bgmSource.volume = Mathf.Lerp(0f, GetFinalBGMVolumeForFadeIn(), timer / duration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            bgmSource.volume = GetFinalBGMVolumeForFadeIn();
            currentBGMClip = newClip;
            bgmFadeRoutine = null;
            yield break;
        }

        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = newClip;
        newSource.loop = true;
        newSource.volume = 0f;
        newSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            bgmSource.volume = Mathf.Lerp(GetFinalBGMVolumeForFadeIn(), 0f, t);
            newSource.volume = Mathf.Lerp(0f, GetFinalBGMVolumeForFadeIn(), t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.volume = GetFinalBGMVolumeForFadeIn();
        bgmSource.Play();
        newSource.Stop();
        Destroy(newSource);

        currentBGMClip = newClip;
        bgmFadeRoutine = null;
    }

    private float GetFinalBGMVolumeForFadeIn()
    {
        return (masterVolume / 10f) * (bgmVolume / 10f);
    }

    // ===================== 音效接口（支持音量） =====================
    public void PlayShortSound(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        float finalVolume = GetFinalSFXVolume(volumeScale);
        AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, finalVolume);
    }

    public AudioSource PlayLoopingSound(AudioClip clip, bool loop = true, float volumeScale = 1f)
    {
        if (clip == null) return null;
        float finalVolume = GetFinalSFXVolume(volumeScale);

        GameObject go = audioSourcePrefab != null ? Instantiate(audioSourcePrefab, audioSourcePoolParent) : new GameObject("LoopingSound_" + clip.name);
        AudioSource src = go.GetComponent<AudioSource>();
        if (src == null) src = go.AddComponent<AudioSource>();

        src.clip = clip;
        src.loop = loop;
        src.volume = finalVolume;
        src.Play();

        activeLoopingSources.Add(src);
        if (!loop)
            StartCoroutine(AutoRemoveOnFinish(src));
        return src;
    }

    public void StopLoopingSound(AudioSource source)
    {
        if (source == null) return;
        if (activeLoopingSources.Contains(source))
            activeLoopingSources.Remove(source);
        source.Stop();
        Destroy(source.gameObject);
    }

    public void StopAllLoopingSounds()
    {
        foreach (var src in activeLoopingSources)
        {
            if (src != null)
            {
                src.Stop();
                Destroy(src.gameObject);
            }
        }
        activeLoopingSources.Clear();
    }

    private IEnumerator AutoRemoveOnFinish(AudioSource src)
    {
        yield return new WaitForSeconds(src.clip.length);
        if (activeLoopingSources.Contains(src))
            activeLoopingSources.Remove(src);
        if (src != null) Destroy(src.gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}