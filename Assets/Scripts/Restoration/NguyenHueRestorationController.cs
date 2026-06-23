using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NguyenHueRestorationController : MonoBehaviour
{
    public enum RestorationState
    {
        GrayMemory,
        Restoring,
        Restored
    }

    [Header("References")]
    public MemoryZoneController memoryZone;
    public Transform playerSpawn;
    public GameObject busStopToActivate;
    public Transform[] includeRoots;

    [Header("Timing")]
    public float totalDuration = 6.5f;
    public float objectRestoreDuration = 1.35f;

    [Header("Lost Memory Look")]
    [Range(0f, 1f)] public float graySaturation = 0.04f;
    [Range(0f, 1.5f)] public float grayBrightness = 0.46f;
    [Range(0f, 1f)] public float grayTintStrength = 0.42f;
    public Color grayMemoryTint = new Color(0.32f, 0.36f, 0.40f, 1f);
    [Range(0f, 1f)] public float npcFadedAlpha = 0.28f;

    [Header("Restore Pulse")]
    public float glowIntensity = 0.28f;
    public Color glowColor = new Color(1f, 0.78f, 0.28f);

    public RestorationState State { get; private set; } = RestorationState.GrayMemory;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private readonly List<RendererMemory> cachedRenderers = new List<RendererMemory>();
    private Coroutine restoreRoutine;

    private void Awake()
    {
        ResolveReferences();
        CacheRenderers();
    }

    private void OnEnable()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += StartRestorationWave;
        }
    }

    private void OnDisable()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= StartRestorationWave;
        }
    }

    private void Start()
    {
        if (IsZoneAlreadyRestored())
        {
            ApplyRestoredInstant();
            return;
        }

        ApplyGrayMemoryInstant();
    }

    public void StartRestorationWave()
    {
        if (State == RestorationState.Restoring || State == RestorationState.Restored)
        {
            return;
        }

        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
        }

        restoreRoutine = StartCoroutine(RestoreWaveRoutine());
    }

    private bool IsZoneAlreadyRestored()
    {
        if (memoryZone == null)
        {
            return false;
        }

        if (memoryZone.IsRestored)
        {
            return true;
        }

        return GameProgressManager.Instance != null
            && GameProgressManager.Instance.IsRestored(memoryZone.locationId);
    }

    private IEnumerator RestoreWaveRoutine()
    {
        State = RestorationState.Restoring;
        SetBusStopVisible(false);
        UIManager.Instance?.SetObjective("Ký ức đang trở lại với phố đi bộ...");

        float minDistance = float.MaxValue;
        float maxDistance = 0f;
        Vector3 origin = playerSpawn != null ? playerSpawn.position : transform.position;

        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            if (!rendererMemory.IsValid)
            {
                continue;
            }

            rendererMemory.distanceFromSpawn = Vector3.Distance(origin, rendererMemory.renderer.bounds.center);
            minDistance = Mathf.Min(minDistance, rendererMemory.distanceFromSpawn);
            maxDistance = Mathf.Max(maxDistance, rendererMemory.distanceFromSpawn);
        }

        if (minDistance == float.MaxValue)
        {
            minDistance = 0f;
        }

        float travelDuration = Mathf.Max(0.1f, totalDuration - objectRestoreDuration);

        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            if (!rendererMemory.IsValid)
            {
                continue;
            }

            float distanceT = Mathf.InverseLerp(minDistance, maxDistance, rendererMemory.distanceFromSpawn);
            float delay = distanceT * travelDuration;
            delay += GetCategoryDelay(rendererMemory.path) * 0.55f;
            StartCoroutine(RestoreRendererRoutine(rendererMemory, delay));
        }

        yield return new WaitForSeconds(totalDuration + 0.15f);

        ApplyRestoredInstant();
        State = RestorationState.Restored;
        SetBusStopVisible(true);
        UIManager.Instance?.ShowDialogue("Âm nhạc trở lại. Đài phun nước sáng lên. Phố đi bộ Nguyễn Huệ đã được khôi phục.");
        UIManager.Instance?.SetObjective("Ký ức đã trở lại. Hãy quay về xe buýt ký ức.");
        restoreRoutine = null;
    }

    private IEnumerator RestoreRendererRoutine(RendererMemory rendererMemory, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < objectRestoreDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / objectRestoreDuration));
            float pulse = Mathf.Sin(t * Mathf.PI);
            rendererMemory.LerpToRestored(t, pulse, glowColor, glowIntensity);
            yield return null;
        }

        rendererMemory.ApplyRestored();
    }

    private void ApplyGrayMemoryInstant()
    {
        State = RestorationState.GrayMemory;
        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            rendererMemory.ApplyFaded();
        }

        SetBusStopVisible(false);
    }

    private void ApplyRestoredInstant()
    {
        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            rendererMemory.ApplyRestored();
        }
    }

    private void ResolveReferences()
    {
        if (memoryZone == null)
        {
            memoryZone = FindFirstInScene<MemoryZoneController>();
        }

        if (playerSpawn == null)
        {
            GameObject spawn = GameObject.Find("PlayerSpawn");
            if (spawn != null && spawn.scene == gameObject.scene)
            {
                playerSpawn = spawn.transform;
            }
        }

        if (busStopToActivate == null)
        {
            BusStopInteractable busStop = FindFirstInScene<BusStopInteractable>();
            if (busStop != null)
            {
                busStopToActivate = busStop.gameObject;
            }
        }

        if (includeRoots == null || includeRoots.Length == 0)
        {
            GameObject sceneRoot = GameObject.Find("SceneBlockoutRoot");
            if (sceneRoot != null && sceneRoot.scene == gameObject.scene)
            {
                includeRoots = new[] { sceneRoot.transform };
            }
        }
    }

    private void CacheRenderers()
    {
        cachedRenderers.Clear();

        Renderer[] renderers = GetCandidateRenderers();
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.gameObject.scene != gameObject.scene || ShouldIgnore(renderer.transform))
            {
                continue;
            }

            RendererMemory rendererMemory = new RendererMemory(renderer, IsNpc(renderer.transform), graySaturation, grayBrightness, grayTintStrength, grayMemoryTint, npcFadedAlpha);
            if (rendererMemory.HasAnyMaterial)
            {
                cachedRenderers.Add(rendererMemory);
            }
        }
    }

    private Renderer[] GetCandidateRenderers()
    {
        if (includeRoots == null || includeRoots.Length == 0)
        {
            return FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        List<Renderer> renderers = new List<Renderer>();
        foreach (Transform root in includeRoots)
        {
            if (root == null)
            {
                continue;
            }

            renderers.AddRange(root.GetComponentsInChildren<Renderer>(true));
        }

        return renderers.ToArray();
    }

    private bool ShouldIgnore(Transform target)
    {
        string path = GetPath(target);
        return path.Contains("REPLACE_Player_Character")
            || path.Contains("Visual_Player_AoDai")
            || path.Contains("Player_CameraTarget")
            || path.Contains("UI_Canvas")
            || path.Contains("EventSystem")
            || path.Contains("Direct_InvisibleBoundaryColliders")
            || path.Contains("COLLIDER_")
            || path.Contains("Boundary")
            || path.Contains("Invisible");
    }

    private bool IsNpc(Transform target)
    {
        string path = GetPath(target);
        return path.Contains("REPLACE_NPC")
            || path.Contains("StreetMusician")
            || path.Contains("Musician")
            || path.Contains("NPC_Area");
    }

    private float GetCategoryDelay(string path)
    {
        if (path.Contains("Ground") || path.Contains("Promenade") || path.Contains("Tile"))
        {
            return 0f;
        }

        if (path.Contains("Tree") || path.Contains("StreetFurniture") || path.Contains("Prop"))
        {
            return 0.25f;
        }

        if (path.Contains("Building") || path.Contains("Facade"))
        {
            return 0.55f;
        }

        if (path.Contains("Landmark") || path.Contains("CityHall"))
        {
            return 0.8f;
        }

        return 0.15f;
    }

    private void SetBusStopVisible(bool visible)
    {
        if (busStopToActivate != null)
        {
            busStopToActivate.SetActive(visible);
        }
    }

    private T FindFirstInScene<T>() where T : Component
    {
        T[] components = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (T component in components)
        {
            if (component != null && component.gameObject.scene == gameObject.scene)
            {
                return component;
            }
        }

        return null;
    }

    private static string GetPath(Transform target)
    {
        string path = target.name;
        Transform current = target.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private class RendererMemory
    {
        public readonly Renderer renderer;
        public readonly string path;
        public float distanceFromSpawn;

        private readonly MaterialMemory[] materials;
        private readonly bool isNpc;

        public bool IsValid => renderer != null;
        public bool HasAnyMaterial => materials.Length > 0;

        public RendererMemory(Renderer renderer, bool isNpc, float graySaturation, float grayBrightness, float grayTintStrength, Color grayMemoryTint, float npcFadedAlpha)
        {
            this.renderer = renderer;
            this.isNpc = isNpc;
            path = GetPath(renderer.transform);

            Material[] runtimeMaterials = renderer.materials;
            List<MaterialMemory> materialMemories = new List<MaterialMemory>();
            foreach (Material material in runtimeMaterials)
            {
                if (material == null || !HasColorProperty(material))
                {
                    continue;
                }

                materialMemories.Add(new MaterialMemory(material, isNpc, graySaturation, grayBrightness, grayTintStrength, grayMemoryTint, npcFadedAlpha));
            }

            materials = materialMemories.ToArray();
        }

        public void ApplyFaded()
        {
            foreach (MaterialMemory material in materials)
            {
                material.ApplyFaded();
            }
        }

        public void ApplyRestored()
        {
            foreach (MaterialMemory material in materials)
            {
                material.ApplyRestored();
            }
        }

        public void LerpToRestored(float t, float pulse, Color pulseColor, float pulseIntensity)
        {
            foreach (MaterialMemory material in materials)
            {
                material.LerpToRestored(t, pulse, pulseColor, pulseIntensity, isNpc);
            }
        }
    }

    private class MaterialMemory
    {
        private readonly Material material;
        private readonly Color originalColor;
        private readonly Color fadedColor;
        private readonly Color originalEmission;
        private readonly bool hadEmission;
        private readonly bool shouldBeTransparentAtStart;
        private readonly bool wasOpaque;

        public MaterialMemory(Material material, bool isNpc, float graySaturation, float grayBrightness, float grayTintStrength, Color grayMemoryTint, float npcFadedAlpha)
        {
            this.material = material;
            originalColor = GetMaterialColor(material);
            fadedColor = BuildFadedColor(originalColor, graySaturation, grayBrightness, grayTintStrength, grayMemoryTint, isNpc ? npcFadedAlpha : originalColor.a);
            hadEmission = material.HasProperty(EmissionColorId);
            originalEmission = hadEmission ? material.GetColor(EmissionColorId) : Color.black;
            shouldBeTransparentAtStart = isNpc;
            wasOpaque = originalColor.a >= 0.99f;
        }

        public void ApplyFaded()
        {
            if (shouldBeTransparentAtStart)
            {
                SetTransparent(material);
            }

            SetMaterialColor(material, fadedColor);
            SetEmission(Color.black);
        }

        public void ApplyRestored()
        {
            SetMaterialColor(material, originalColor);
            SetEmission(originalEmission);

            if (shouldBeTransparentAtStart && wasOpaque)
            {
                SetOpaque(material);
            }
        }

        public void LerpToRestored(float t, float pulse, Color pulseColor, float pulseIntensity, bool isNpc)
        {
            Color color = Color.Lerp(fadedColor, originalColor, t);
            SetMaterialColor(material, color);

            if (hadEmission)
            {
                Color pulseEmission = pulseColor * (pulse * pulseIntensity);
                SetEmission(Color.Lerp(Color.black, originalEmission + pulseEmission, t));
            }

            if (isNpc && t >= 0.95f && wasOpaque)
            {
                SetOpaque(material);
            }
        }

        private void SetEmission(Color color)
        {
            if (!hadEmission)
            {
                return;
            }

            material.EnableKeyword("_EMISSION");
            material.SetColor(EmissionColorId, color);
        }

        private static Color BuildFadedColor(Color original, float saturation, float brightness, float tintStrength, Color tint, float alpha)
        {
            float luminance = original.r * 0.2126f + original.g * 0.7152f + original.b * 0.0722f;
            Color gray = new Color(luminance, luminance, luminance, alpha);
            Color muted = Color.Lerp(gray, original, saturation);
            muted.r *= brightness;
            muted.g *= brightness;
            muted.b *= brightness;
            muted = Color.Lerp(muted, tint * brightness, tintStrength);
            muted.a = alpha;
            return muted;
        }
    }

    private static bool HasColorProperty(Material material)
    {
        return material.HasProperty(BaseColorId) || material.HasProperty(ColorId);
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty(BaseColorId))
        {
            return material.GetColor(BaseColorId);
        }

        return material.GetColor(ColorId);
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty(BaseColorId))
        {
            material.SetColor(BaseColorId, color);
        }

        if (material.HasProperty(ColorId))
        {
            material.SetColor(ColorId, color);
        }
    }

    private static void SetTransparent(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;
    }

    private static void SetOpaque(Material material)
    {
        material.SetOverrideTag("RenderType", string.Empty);
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 0f);
        }

        material.SetInt("_SrcBlend", (int)BlendMode.One);
        material.SetInt("_DstBlend", (int)BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = -1;
    }
}
