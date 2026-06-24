using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const float DefaultAmbienceFadeSeconds = 2.5f;

    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private AudioClipLibrary clipLibrary;
    private AudioSource sfxSource;
    private AudioSource footstepSource;
    private AudioSource ambienceA;
    private AudioSource ambienceB;
    private Coroutine ambienceFadeRoutine;
    private bool usingAmbienceA = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeRuntime()
    {
        EnsureInstance();
    }

    public static AudioManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        AudioManager existing = FindFirstObjectByType<AudioManager>();
        if (existing != null)
        {
            Instance = existing;
            DontDestroyOnLoad(existing.gameObject);
            return existing;
        }

        GameObject go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        return go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateSources();
        PreloadKnownClips();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneAmbience(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    public void PlaySfx(string clipName, float volume = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }

    public void SetFootstepsMoving(bool moving)
    {
        if (footstepSource == null)
        {
            return;
        }

        AudioClip clip = GetClip("SFX_PlayerFootstep_Concrete");
        if (clip == null)
        {
            return;
        }

        if (footstepSource.clip != clip)
        {
            footstepSource.clip = clip;
            footstepSource.loop = true;
            footstepSource.volume = 0.42f;
        }

        if (moving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    public void FadeToRestoredAmbienceForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == SceneLoader.NguyenHue)
        {
            FadeToAmbience("AMB_NguyenHue_Restored", DefaultAmbienceFadeSeconds);
        }
        else if (sceneName == SceneLoader.NhaThoDucBa)
        {
            FadeToAmbience("AMB_Cathedral_Restored", DefaultAmbienceFadeSeconds);
        }
    }

    public void FadeToAmbience(string clipName, float fadeSeconds = DefaultAmbienceFadeSeconds)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null)
        {
            return;
        }

        if (ambienceFadeRoutine != null)
        {
            StopCoroutine(ambienceFadeRoutine);
        }

        ambienceFadeRoutine = StartCoroutine(FadeAmbienceRoutine(clip, Mathf.Max(0.05f, fadeSeconds)));
    }

    public void StopAmbience(float fadeSeconds = 0.6f)
    {
        if (ambienceFadeRoutine != null)
        {
            StopCoroutine(ambienceFadeRoutine);
        }

        ambienceFadeRoutine = StartCoroutine(StopAmbienceRoutine(Mathf.Max(0.05f, fadeSeconds)));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetFootstepsMoving(false);
        ApplySceneAmbience(scene.name);

        if (scene.name == SceneLoader.BusHub)
        {
            PlaySfx("SFX_BusArrive", 0.85f);
        }
    }

    private void ApplySceneAmbience(string sceneName)
    {
        if (sceneName == SceneLoader.NguyenHue)
        {
            FadeToAmbience("AMB_NguyenHue_Gloomy", DefaultAmbienceFadeSeconds);
        }
        else if (sceneName == SceneLoader.NhaThoDucBa)
        {
            FadeToAmbience("AMB_Cathedral_Gloomy", DefaultAmbienceFadeSeconds);
        }
        else
        {
            StopAmbience();
        }
    }

    private IEnumerator FadeAmbienceRoutine(AudioClip nextClip, float fadeSeconds)
    {
        AudioSource from = usingAmbienceA ? ambienceA : ambienceB;
        AudioSource to = usingAmbienceA ? ambienceB : ambienceA;
        usingAmbienceA = !usingAmbienceA;

        to.clip = nextClip;
        to.loop = true;
        to.volume = 0f;
        if (!to.isPlaying)
        {
            to.Play();
        }

        float fromStartVolume = from != null ? from.volume : 0f;
        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            if (from != null)
            {
                from.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            }

            to.volume = Mathf.Lerp(0f, 0.5f, t);
            yield return null;
        }

        if (from != null)
        {
            from.Stop();
            from.volume = 0f;
        }

        to.volume = 0.5f;
        ambienceFadeRoutine = null;
    }

    private IEnumerator StopAmbienceRoutine(float fadeSeconds)
    {
        float aStart = ambienceA != null ? ambienceA.volume : 0f;
        float bStart = ambienceB != null ? ambienceB.volume : 0f;
        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            if (ambienceA != null)
            {
                ambienceA.volume = Mathf.Lerp(aStart, 0f, t);
            }

            if (ambienceB != null)
            {
                ambienceB.volume = Mathf.Lerp(bStart, 0f, t);
            }

            yield return null;
        }

        if (ambienceA != null)
        {
            ambienceA.Stop();
        }

        if (ambienceB != null)
        {
            ambienceB.Stop();
        }

        ambienceFadeRoutine = null;
    }

    private void CreateSources()
    {
        sfxSource = CreateSource("SFXSource", false);
        footstepSource = CreateSource("FootstepLoopSource", true);
        ambienceA = CreateSource("AmbienceSourceA", true);
        ambienceB = CreateSource("AmbienceSourceB", true);
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        Transform existing = transform.Find(sourceName);
        GameObject sourceObject = existing != null ? existing.gameObject : new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.GetComponent<AudioSource>();
        if (source == null)
        {
            source = sourceObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = 1f;
        return source;
    }

    private void PreloadKnownClips()
    {
        string[] knownClips =
        {
            "SFX_Interact_E",
            "SFX_MapSelect",
            "SFX_ObjectiveUpdated",
            "SFX_PuzzleButton",
            "SFX_PuzzleSolved",
            "SFX_PlayerFootstep_Concrete",
            "SFX_ItemCollect_Memory",
            "SFX_MemoryRestoreStart",
            "SFX_MemoryRestoreWave",
            "SFX_BusArrive",
            "SFX_BusDepart",
            "AMB_NguyenHue_Gloomy",
            "AMB_NguyenHue_Restored",
            "AMB_Cathedral_Gloomy",
            "AMB_Cathedral_Restored"
        };

        foreach (string clipName in knownClips)
        {
            GetClip(clipName);
        }
    }

    private AudioClip GetClip(string clipName)
    {
        if (clips.TryGetValue(clipName, out AudioClip cached))
        {
            return cached;
        }

        AudioClip clip = GetClipFromLibrary(clipName);
        if (clip == null)
        {
            clip = Resources.Load<AudioClip>(clipName);
        }
#if UNITY_EDITOR
        if (clip == null)
        {
            string[] guids = AssetDatabase.FindAssets(clipName + " t:AudioClip", new[] { "Assets/Audio" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
        }
#endif

        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Missing audio clip: " + clipName);
        }

        clips[clipName] = clip;
        return clip;
    }

    private AudioClip GetClipFromLibrary(string clipName)
    {
        if (clipLibrary == null)
        {
            clipLibrary = Resources.Load<AudioClipLibrary>("AudioClipLibrary");
        }

        return clipLibrary != null ? clipLibrary.GetClip(clipName) : null;
    }
}

[System.Serializable]
public class AudioClipLibraryEntry
{
    public string name;
    public AudioClip clip;
}

public class AudioClipLibrary : ScriptableObject
{
    public AudioClipLibraryEntry[] clips;

    public AudioClip GetClip(string clipName)
    {
        if (clips == null)
        {
            return null;
        }

        foreach (AudioClipLibraryEntry entry in clips)
        {
            if (entry != null && entry.name == clipName)
            {
                return entry.clip;
            }
        }

        return null;
    }
}

#if UNITY_EDITOR
public static class AudioLibraryAssetBuilder
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string LibraryPath = "Assets/Resources/AudioClipLibrary.asset";

    [MenuItem("Ky Uc Sai Gon/Audio/Rebuild Audio Clip Library")]
    public static void EnsureLibraryAsset()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AudioClipLibrary library = AssetDatabase.LoadAssetAtPath<AudioClipLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<AudioClipLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        string[] clipNames =
        {
            "SFX_Interact_E",
            "SFX_MapSelect",
            "SFX_ObjectiveUpdated",
            "SFX_PuzzleButton",
            "SFX_PuzzleSolved",
            "SFX_PlayerFootstep_Concrete",
            "SFX_ItemCollect_Memory",
            "SFX_MemoryRestoreStart",
            "SFX_MemoryRestoreWave",
            "SFX_BusArrive",
            "SFX_BusDepart",
            "AMB_NguyenHue_Gloomy",
            "AMB_NguyenHue_Restored",
            "AMB_Cathedral_Gloomy",
            "AMB_Cathedral_Restored"
        };

        List<AudioClipLibraryEntry> entries = new List<AudioClipLibraryEntry>();
        foreach (string clipName in clipNames)
        {
            AudioClip clip = FindAudioClip(clipName);
            entries.Add(new AudioClipLibraryEntry
            {
                name = clipName,
                clip = clip
            });

            if (clip == null)
            {
                Debug.LogWarning("[AudioLibraryAssetBuilder] Missing audio clip: " + clipName);
            }
        }

        library.clips = entries.ToArray();
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static AudioClip FindAudioClip(string clipName)
    {
        string[] guids = AssetDatabase.FindAssets(clipName + " t:AudioClip", new[] { "Assets/Audio" });
        if (guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
#endif
