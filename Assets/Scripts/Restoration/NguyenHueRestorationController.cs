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
    private static readonly int BaseColorFactorId = Shader.PropertyToID("baseColorFactor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissiveFactorId = Shader.PropertyToID("emissiveFactor");

    private readonly List<RendererMemory> cachedRenderers = new List<RendererMemory>();
    private Coroutine restoreRoutine;

    // Groups for cinematic restoration wave
    private readonly List<RendererMemory> npcGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> cathedralGroup = new List<RendererMemory>(); // Represents City Hall
    private readonly List<RendererMemory> otherGroup = new List<RendererMemory>();

    // Lighting variables for gloomy look
    private Light sunLight;
    private float originalSunIntensity = 1.0f;

    private void Awake()
    {
        // Force the gloomy look parameters to match ending scene perfectly (0% saturation, 22% brightness, 85% tint)
        graySaturation = 0.0f;
        grayBrightness = 0.22f;
        grayTintStrength = 0.85f;
        grayMemoryTint = new Color(0.38f, 0.40f, 0.43f, 1f);

        ResolveReferences();
        DisableConflictingEffects();
        CacheRenderers();
        CategorizeGroups();
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

        // 1. Disable Player movement and Camera tracking
        ThirdPersonPlayerController playerController = FindFirstObjectByType<ThirdPersonPlayerController>();
        ThirdPersonCamera playerCamera = FindFirstObjectByType<ThirdPersonCamera>();

        if (playerController != null)
        {
            playerController.enabled = false;
            AudioManager.EnsureInstance()?.SetFootstepsMoving(false);
        }
        if (playerCamera != null) playerCamera.enabled = false;

        Camera mainCam = Camera.main;
        Vector3 startCamPos = mainCam != null ? mainCam.transform.position : Vector3.zero;
        Quaternion startCamRot = mainCam != null ? mainCam.transform.rotation : Quaternion.identity;

        // Play memory awakening sound immediately upon interaction
        AudioManager.EnsureInstance()?.PlaySfx("SFX_MemoryAwakening", 1.0f);
        AudioManager.EnsureInstance()?.StopAmbience(1.0f);

        SetBusStopVisible(false);
        UIManager.Instance?.SetObjective("Ký ức đang trở lại với phố đi bộ...");

        yield return new WaitForSeconds(1.0f);

        // Play the restoration cinematic wave sound throughout the zoom process
        AudioManager.EnsureInstance()?.PlaySfx("SFX_RestorationCinematicWave", 1.0f);

        // --- STEP A: Zoom to Fountain (Đài phun nước) ---
        // Fountain position is at (0f, 0.55f, 10f)
        Vector3 fountainPos = new Vector3(0f, 0.55f, 10f);
        Vector3 fountainCamPos = new Vector3(0f, 4f, -2f);
        Quaternion fountainCamRot = Quaternion.LookRotation(new Vector3(0f, 1f, 10f) - fountainCamPos);

        UIManager.Instance?.ShowDialogue("Dòng nước mát lành đang quay lại với đài phun nước...");
        AudioManager.EnsureInstance()?.PlayVoice("NguyenHue_Restore_1");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        yield return StartCoroutine(LerpCamera(mainCam, fountainCamPos, fountainCamRot, 1.5f));
        yield return StartCoroutine(RestoreGroupRoutine(npcGroup, 1.8f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP B: Zoom Out to Wide Panorama (Toàn cảnh phố đi bộ) ---
        Vector3 panoramaCamPos = new Vector3(0f, 20f, -40f);
        Quaternion panoramaCamRot = Quaternion.LookRotation(new Vector3(0f, 8f, 5f) - panoramaCamPos);

        UIManager.Instance?.ShowDialogue("Phố đi bộ bừng lên trong ánh đèn lung linh...");
        AudioManager.EnsureInstance()?.PlayVoice("NguyenHue_Restore_2");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        yield return StartCoroutine(LerpCamera(mainCam, panoramaCamPos, panoramaCamRot, 1.8f));
        yield return StartCoroutine(RestoreGroupRoutine(otherGroup, 2.2f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP C: Zoom Out to City Hall Facade (Tương tự landmark zoom, majestic wide front-on shot) ---
        // CityHall center is at (0, 0, 39) with size y = 42.9f.
        // We place the camera at z = -10f (49 units back from City Hall facade), height y = 8f.
        Vector3 cityHallCamPos = new Vector3(0f, 8f, -10f);
        Quaternion cityHallCamRot = Quaternion.LookRotation(new Vector3(0f, 22f, 39f) - cityHallCamPos);

        UIManager.Instance?.ShowDialogue("Tòa nhà Ủy ban rạng rỡ sắc vàng kiêu hãnh...");
        AudioManager.EnsureInstance()?.PlayVoice("NguyenHue_Restore_3");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        yield return StartCoroutine(LerpCamera(mainCam, cityHallCamPos, cityHallCamRot, 2.0f));

        // Play reveal SFX once camera has arrived at City Hall
        AudioManager.EnsureInstance()?.PlaySfx("SFX_LandmarkReveal", 1.0f);

        // Restore lighting
        RestoreLights();

        // Restore City Hall in parallel
        yield return StartCoroutine(RestoreGroupRoutine(cathedralGroup, 2.2f));
        
        // Admire the landmark
        yield return new WaitForSeconds(2.0f);

        // --- STEP D: Zoom out back to Player ---
        UIManager.Instance?.ShowDialogue("Phố đi bộ đã có âm nhạc trở lại rồi.");
        AudioManager.EnsureInstance()?.PlayVoice("NguyenHue_2");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        yield return StartCoroutine(LerpCamera(mainCam, startCamPos, startCamRot, 1.8f));

        // Re-enable Player movement and Camera tracking
        if (playerController != null) playerController.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;

        // Fade in the restored city ambient sound
        AudioManager.EnsureInstance()?.FadeToRestoredAmbienceForCurrentScene();

        ApplyRestoredInstant();
        State = RestorationState.Restored;
        SetBusStopVisible(true);

        UIManager.Instance?.SetObjective("Ký ức đã trở lại. Hãy quay về xe buýt ký ức.");

        restoreRoutine = null;
    }

    private IEnumerator LerpCamera(Camera cam, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        if (cam == null) yield break;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;
    }

    private IEnumerator RestoreGroupRoutine(List<RendererMemory> group, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            foreach (RendererMemory mem in group)
            {
                if (mem != null && mem.IsValid)
                {
                    mem.LerpToRestored(t, 0f, glowColor, 0f);
                }
            }
            yield return null;
        }
        foreach (RendererMemory mem in group)
        {
            if (mem != null && mem.IsValid)
            {
                mem.ApplyRestored();
            }
        }
    }

    private void CategorizeGroups()
    {
        npcGroup.Clear();      // Step A: Fountain & Puzzle/LED group
        cathedralGroup.Clear(); // Step C: City Hall group
        otherGroup.Clear();     // Step B: Boulevard environment group

        foreach (RendererMemory mem in cachedRenderers)
        {
            if (!mem.IsValid) continue;

            string nameLower = mem.renderer.gameObject.name.ToLower();
            string pathLower = mem.path.ToLower();

            if (nameLower.Contains("cityhall") || nameLower.Contains("city_hall") || nameLower.Contains("backdrop") ||
                pathLower.Contains("cityhall") || pathLower.Contains("city_hall") || pathLower.Contains("backdrop"))
            {
                cathedralGroup.Add(mem);
            }
            else if (nameLower.Contains("fountain") || nameLower.Contains("speaker") || nameLower.Contains("puzzle") || nameLower.Contains("led") ||
                     pathLower.Contains("fountain") || pathLower.Contains("speaker") || pathLower.Contains("puzzle") || pathLower.Contains("led"))
            {
                npcGroup.Add(mem);
            }
            else
            {
                otherGroup.Add(mem);
            }
        }
    }

    private void CacheAndDimLights()
    {
        if (sunLight == null)
        {
            sunLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (sunLight == null)
            {
                foreach (Light l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }
        }

        if (sunLight != null)
        {
            originalSunIntensity = sunLight.intensity;
            sunLight.intensity = originalSunIntensity * 0.18f;
        }
    }

    private void RestoreLights()
    {
        if (sunLight != null)
        {
            sunLight.intensity = originalSunIntensity;
        }
    }

    private void DisableConflictingEffects()
    {
        MaterialRestoreEffect[] matEffects = FindObjectsByType<MaterialRestoreEffect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MaterialRestoreEffect effect in matEffects)
        {
            if (effect.gameObject.scene == gameObject.scene)
            {
                effect.enabled = false;
                effect.renderers = new Renderer[0];
            }
        }
    }

    private void ApplyGrayMemoryInstant()
    {
        State = RestorationState.GrayMemory;
        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            rendererMemory.ApplyFaded();
        }

        SetBusStopVisible(false);
        CacheAndDimLights();
    }

    private void ApplyRestoredInstant()
    {
        foreach (RendererMemory rendererMemory in cachedRenderers)
        {
            rendererMemory.ApplyRestored();
        }
        RestoreLights();
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
            || path.Contains("Invisible")
            || path.Contains("REPLACE_NPC")
            || path.Contains("StreetMusician")
            || path.Contains("Musician");
    }

    private bool IsNpc(Transform target)
    {
        return false;
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
            
            bool hasEmissiveFactor = material.HasProperty(EmissiveFactorId);
            bool hasEmissionColor = material.HasProperty(EmissionColorId);
            hadEmission = hasEmissiveFactor || hasEmissionColor;
            
            if (hasEmissiveFactor)
            {
                originalEmission = material.GetColor(EmissiveFactorId);
            }
            else if (hasEmissionColor)
            {
                originalEmission = material.GetColor(EmissionColorId);
            }
            else
            {
                originalEmission = Color.black;
            }

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
            if (material.HasProperty(EmissionColorId))
            {
                material.SetColor(EmissionColorId, color);
            }
            if (material.HasProperty(EmissiveFactorId))
            {
                material.SetColor(EmissiveFactorId, color);
            }
        }

        private static Color BuildFadedColor(Color original, float saturation, float brightness, float tintStrength, Color tint, float alpha)
        {
            float luminance = original.r * 0.299f + original.g * 0.587f + original.b * 0.114f;
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
        return material.HasProperty(BaseColorId) || material.HasProperty(ColorId) || material.HasProperty(BaseColorFactorId);
    }

    private static Color GetMaterialColor(Material material)
    {
        if (material.HasProperty(BaseColorId))
        {
            return material.GetColor(BaseColorId);
        }

        if (material.HasProperty(ColorId))
        {
            return material.GetColor(ColorId);
        }

        return material.GetColor(BaseColorFactorId);
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

        if (material.HasProperty(BaseColorFactorId))
        {
            material.SetColor(BaseColorFactorId, color);
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
