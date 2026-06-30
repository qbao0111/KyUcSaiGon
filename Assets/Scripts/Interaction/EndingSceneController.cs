using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EndingSceneController : MonoBehaviour
{
    [Header("References")]
    public GameObject landmarkTower;
    public GameObject[] memoryShards;
    public string[] memoryNames;
    public GameObject finalLightObject;
    public GameObject returnTrigger;
    public Renderer[] renderersToWarm;
    public AudioSource endingAmbience;
    public Sprite completedEndingBackground;

    [Header("Settings")]
    public float shardStepDelay = 0.9f;
    public float toneShiftDuration = 2.5f;

    private readonly Color grayTone = new Color(0.38f, 0.4f, 0.43f);
    private readonly Color shardWarm = new Color(0.96f, 0.83f, 0.36f);

    // Group cached renderers for cinematic wave restoration
    private readonly List<RendererMemory> cachedRenderers = new List<RendererMemory>();
    private readonly List<RendererMemory> boatGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> redboatGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> causaigonGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> villaGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> landmarkGroup = new List<RendererMemory>();
    private readonly List<RendererMemory> otherGroup = new List<RendererMemory>();

    // Cached boat gameobjects for dynamic zoom positioning
    private GameObject boatObj;
    private GameObject redboatObj;

    // Light states for u ám desaturated look
    private float originalSunIntensity = 1f;
    private Light sunLight;
    private readonly List<Light> sunsetDiscLights = new List<Light>();
    private readonly List<float> originalSunsetDiscIntensities = new List<float>();
    private GameObject completedPanel;
    private Button[] completedButtons;
    private int selectedCompletedButtonIndex;
    private bool completedScreenActive;
    private readonly Color completedButtonNormal = new Color(0.08f, 0.085f, 0.09f, 0.72f);
    private readonly Color completedButtonSelected = new Color(0.82f, 0.84f, 0.84f, 0.55f);

    private void Update()
    {
        if (!completedScreenActive || completedButtons == null || completedButtons.Length == 0)
        {
            return;
        }

        if (CompletedUpPressed())
        {
            MoveCompletedSelection(-1);
        }
        else if (CompletedDownPressed())
        {
            MoveCompletedSelection(1);
        }

        if (CompletedSubmitPressed())
        {
            Button selectedButton = GetSelectedCompletedButton();
            if (selectedButton != null && selectedButton.interactable)
            {
                selectedButton.onClick.Invoke();
            }
        }
    }

    private void Awake()
    {
        // Cache boat and redboat references
        boatObj = GameObject.Find("SceneBlockoutRoot/RiversidePropRoot/boat") ?? GameObject.Find("boat");
        redboatObj = GameObject.Find("redboat");

        // Gather all renderers dynamically at runtime to ensure we don't miss redboat or new elements
        List<Renderer> allRenderers = new List<Renderer>();
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r == null || r.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                continue;
            }

            string path = GetPath(r.transform);
            if (path.Contains("REPLACE_Player_Character") || 
                path.Contains("Visual_Player_AoDai") || 
                path.Contains("Player_CameraTarget") || 
                path.Contains("Invisible") ||
                path.Contains("Trigger"))
            {
                continue;
            }

            allRenderers.Add(r);
        }
        renderersToWarm = allRenderers.ToArray();

        // Initialize RendererMemory cache
        cachedRenderers.Clear();
        foreach (Renderer r in renderersToWarm)
        {
            if (r == null) continue;
            RendererMemory mem = new RendererMemory(r);
            if (mem.HasAnyMaterial)
            {
                cachedRenderers.Add(mem);
            }
        }

        // Categorize into restoration wave groups
        CategorizeGroups();
    }

    private void Start()
    {
        if (returnTrigger != null)
        {
            returnTrigger.SetActive(false);
        }

        // Apply desaturated lost-memory look and dim lights immediately when scene starts
        ApplyGrayMemoryInstant();
        CacheAndDimLights();
        UIManager.Instance?.SetObjective("Tìm kiếm điểm ký ức lấp lánh ở lan can bờ sông.");

        if (!DeveloperMode.IsEnabled && (GameProgressManager.Instance == null || !GameProgressManager.Instance.AreAllMemoriesRestored()))
        {
            StartCoroutine(ReturnToHubIfNotEnoughMemory());
            return;
        }
    }

    public void TriggerEndingSequence()
    {
        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator ReturnToHubIfNotEnoughMemory()
    {
        UIManager.Instance?.ShowDialogue("Bạn chưa thu thập đủ ký ức.");
        UIManager.Instance?.SetObjective("Quay lại xe buýt ký ức...");
        yield return new WaitForSeconds(2.6f);
        SceneLoader.Load(SceneLoader.BusHub);
    }

    private IEnumerator PlayEndingSequence()
    {
        // 1. Disable Player movement and Camera tracking immediately when entering cinema
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

        UIManager.Instance?.SetObjective("Lắng nghe những mảnh ký ức của thành phố.");

        // Lit up shards if present
        if (memoryShards != null && memoryShards.Length > 0)
        {
            for (int i = 0; i < memoryShards.Length; i++)
            {
                LightUpShard(i);
                if (memoryNames != null && i < memoryNames.Length)
                {
                    UIManager.Instance?.ShowDialogue(memoryNames[i]);
                }

                yield return new WaitForSeconds(shardStepDelay);
            }
        }

        yield return new WaitForSeconds(0.8f);

        // 2. Play main dialogue
        UIManager.Instance?.ShowDialogue("BẠN ĐÃ TÌM LẠI KÝ ỨC ĐÔ THỊ\nTHÀNH PHỐ ĐÃ SỐNG TRỞ LẠI");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_1");
        yield return new WaitForSeconds(4.2f);

        // Stop boat movement during the cinematic cutscene
        BoatMovement[] boatMovements = FindObjectsByType<BoatMovement>(FindObjectsSortMode.None);
        foreach (var bm in boatMovements)
        {
            bm.isMoving = false;
        }

        // Play the restoration cinematic wave sound throughout the zoom process
        AudioManager.EnsureInstance()?.PlaySfx("SFX_RestorationCinematicWave", 1.0f);

        // --- STEP A: Zoom to boat ---
        UIManager.Instance?.ShowDialogue("Dòng chảy ký ức đánh thức những chuyến tàu...");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_2");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        // Calculate camera position dynamically relative to the boat's current position
        Vector3 currentBoatPos = boatObj != null ? boatObj.transform.position : new Vector3(15.57f, 0.8f, 21.8f);
        Vector3 boatCamPos = currentBoatPos + new Vector3(0f, 2.0f, -9.8f);
        Quaternion boatCamRot = Quaternion.LookRotation(currentBoatPos - boatCamPos);
        yield return StartCoroutine(LerpCamera(mainCam, boatCamPos, boatCamRot, 1.5f));
        yield return StartCoroutine(RestoreGroupRoutine(boatGroup, 2.0f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP B: Zoom to redboat ---
        UIManager.Instance?.ShowDialogue("Sắc đỏ kiêu hãnh xuôi dòng nước lớn...");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_3");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        // Calculate camera position dynamically relative to redboat's current position
        Vector3 currentRedboatPos = redboatObj != null ? redboatObj.transform.position : new Vector3(-31.03f, 1.74f, 11.65f);
        Vector3 redboatCamPos = currentRedboatPos + new Vector3(0f, 1.8f, -8.65f);
        Quaternion redboatCamRot = Quaternion.LookRotation(currentRedboatPos - redboatCamPos);
        yield return StartCoroutine(LerpCamera(mainCam, redboatCamPos, redboatCamRot, 1.5f));
        yield return StartCoroutine(RestoreGroupRoutine(redboatGroup, 2.0f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP C: Zoom to causaigon (Saigon Bridge) ---
        UIManager.Instance?.ShowDialogue("Cầu Sài Gòn nối nhịp những dòng sông...");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_4");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        Vector3 causaigonCamPos = new Vector3(10f, 6f, 5f);
        Quaternion causaigonCamRot = Quaternion.LookRotation(new Vector3(27.5f, 9.6f, 21.3f) - causaigonCamPos);
        yield return StartCoroutine(LerpCamera(mainCam, causaigonCamPos, causaigonCamRot, 1.5f));
        yield return StartCoroutine(RestoreGroupRoutine(causaigonGroup, 2.0f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP D: Zoom to villa ---
        UIManager.Instance?.ShowDialogue("Những khu biệt thự cổ rạng rỡ bên sông...");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_5");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        Vector3 villaCamPos = new Vector3(-10f, 12f, 15f);
        Quaternion villaCamRot = Quaternion.LookRotation(new Vector3(-10f, 1.8f, 48f) - villaCamPos);
        yield return StartCoroutine(LerpCamera(mainCam, villaCamPos, villaCamRot, 1.5f));
        yield return StartCoroutine(RestoreGroupRoutine(villaGroup, 2.0f));
        yield return new WaitForSeconds(0.4f);

        // --- STEP E: Zoom to landmark (and restore the rest of the map!) ---
        UIManager.Instance?.ShowDialogue("Và Landmark 81 vút cao đón ánh nắng vàng...");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_6");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        Vector3 landmarkCamPos = new Vector3(0f, 6f, 5f);
        Quaternion landmarkCamRot = Quaternion.LookRotation(new Vector3(0f, 25f, 34f) - landmarkCamPos);
        yield return StartCoroutine(LerpCamera(mainCam, landmarkCamPos, landmarkCamRot, 2.0f));

        // Play reveal SFX once camera has arrived at Landmark
        AudioManager.EnsureInstance()?.PlaySfx("SFX_LandmarkReveal", 1.0f);

        // Restore lighting and activate landmark emission
        RestoreLights();
        LightUpLandmark(); // Triggers tower emission and lighting intensities

        // Restore landmark and everything else in parallel
        Coroutine landmarkRestore = StartCoroutine(RestoreGroupRoutine(landmarkGroup, 3.0f));
        Coroutine otherRestore = StartCoroutine(RestoreGroupRoutine(otherGroup, 3.0f));

        yield return landmarkRestore;
        yield return otherRestore;
        yield return new WaitForSeconds(2.0f); // Delay for 2 seconds to admire the landmark before zooming out

        // --- STEP F: Zoom out back to Player ---
        UIManager.Instance?.ShowDialogue("Cả thành phố bừng sáng rực rỡ sắc màu.");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_7");
        AudioManager.EnsureInstance()?.PlaySfx("SFX_CameraFocus", 1.0f);
        yield return StartCoroutine(LerpCamera(mainCam, startCamPos, startCamRot, 2.0f));

        // Re-enable Player movement and Camera tracking
        if (playerController != null) playerController.enabled = true;
        if (playerCamera != null) playerCamera.enabled = true;

        // Fade in the restored city ambient sound
        AudioManager.EnsureInstance()?.FadeToAmbience("AMB_Ending_Restored", 2.0f);

        // Resume boat movement
        foreach (var bm in boatMovements)
        {
            bm.isMoving = true;
        }

        yield return new WaitForSeconds(1.0f);
        UIManager.Instance?.ShowDialogue("Khi ký ức được lắng nghe, thành phố lại tìm thấy màu sắc của mình.");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_8");
        yield return new WaitForSeconds(5.0f);
        UIManager.Instance?.ShowDialogue("Tương lai không bắt đầu bằng việc quên đi quá khứ.\nTương lai bắt đầu khi ta biết mang ký ức đi cùng.");
        AudioManager.EnsureInstance()?.PlayVoice("Ending_9");
        yield return new WaitForSeconds(6.8f);
        yield return StartCoroutine(ShowGameCompletedPanelRoutine());
    }

    private IEnumerator ShowGameCompletedPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowGameCompletedPanel();
    }

    private void ShowGameCompletedPanel()
    {
        StartCoroutine(ShowGameCompletedPanelRoutine());
    }

    private IEnumerator ShowGameCompletedPanelRoutine()
    {
        // Freeze player character
        ThirdPersonPlayerController playerController = FindFirstObjectByType<ThirdPersonPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            AudioManager.EnsureInstance()?.SetFootstepsMoving(false);
        }

        // Block input and release cursor via UIManager
        UIManager.Instance?.SetExternalInputBlocked(true);

        // Find existing canvas or create one
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject go = new GameObject("EndingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        canvas.sortingOrder = 500;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (completedPanel != null)
        {
            Destroy(completedPanel);
        }

        // Create fullscreen completed panel.
        GameObject panelObj = new GameObject("GameCompletedPanel", typeof(RectTransform), typeof(CanvasGroup));
        completedPanel = panelObj;
        panelObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        CanvasGroup group = panelObj.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;

        Image bgImage = CreateCompletedImage(panelObj.transform, "CompletedBackground", completedEndingBackground, Color.white);
        Stretch(bgImage.rectTransform);
        bgImage.preserveAspect = true;
        if (completedEndingBackground != null)
        {
            AspectRatioFitter fitter = bgImage.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = completedEndingBackground.rect.width / Mathf.Max(1f, completedEndingBackground.rect.height);
        }

        // Only add actions. The completed artwork already contains the ending copy.
        GameObject container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(panelObj.transform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0f, 0f);
        containerRect.anchorMax = new Vector2(0f, 0f);
        containerRect.pivot = new Vector2(0f, 0f);
        containerRect.anchoredPosition = new Vector2(96f, 230f);
        containerRect.sizeDelta = new Vector2(560f, 140f);

        Button mainMenuButton = CreateCompletedButton(container.transform, "MainMenuButton", "VỀ MÀN HÌNH CHÍNH", new Vector2(0f, 0f), () =>
        {
            AudioManager.EnsureInstance()?.PlaySfx("SFX_MapSelect", 1.0f);
            UIManager.Instance?.SetExternalInputBlocked(false);
            SceneLoader.Load(SceneLoader.MainMenu);
        });
        Button quitButton = CreateCompletedButton(container.transform, "QuitButton", "THOÁT GAME", new Vector2(0f, -76f), () =>
        {
            AudioManager.EnsureInstance()?.PlaySfx("SFX_BusDepart", 1.0f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });

        completedButtons = new[] { mainMenuButton, quitButton };
        selectedCompletedButtonIndex = 0;
        completedScreenActive = true;
        SelectCompletedButton(0, false);

        // Explicitly unlock cursor just to be doubly sure
        CursorLockManager.UnlockForUI();

        AudioManager.EnsureInstance()?.StopAmbience(1.2f);

        float elapsed = 0f;
        const float fadeDuration = 1.25f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
    }

    private Button CreateCompletedButton(Transform parent, string name, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 58f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = completedButtonNormal;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(onClick);

        Image tick = CreateCompletedImage(buttonObject.transform, "GoldTick", null, new Color(0.86f, 0.6f, 0.22f, 0.95f));
        RectTransform tickRect = tick.rectTransform;
        tickRect.anchorMin = new Vector2(0f, 0f);
        tickRect.anchorMax = new Vector2(0f, 1f);
        tickRect.offsetMin = new Vector2(0f, 8f);
        tickRect.offsetMax = new Vector2(4f, -8f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(buttonObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 0f);
        textRect.offsetMax = new Vector2(-16f, 0f);

        Text text = textObject.GetComponent<Text>();
        text.text = label;
        text.font = GameUIFont.Bold;
        text.fontSize = 28;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleLeft;
        text.color = new Color(0.92f, 0.93f, 0.9f, 1f);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;

        return button;
    }

    private void MoveCompletedSelection(int direction)
    {
        if (completedButtons == null || completedButtons.Length == 0)
        {
            return;
        }

        selectedCompletedButtonIndex = (selectedCompletedButtonIndex + direction + completedButtons.Length) % completedButtons.Length;
        SelectCompletedButton(selectedCompletedButtonIndex, true);
    }

    private void SelectCompletedButton(int index, bool playSound)
    {
        selectedCompletedButtonIndex = Mathf.Clamp(index, 0, completedButtons.Length - 1);
        UpdateCompletedButtonVisuals();
        EventSystem.current?.SetSelectedGameObject(null);

        if (playSound)
        {
            AudioManager.EnsureInstance()?.PlaySfx("SFX_ItemCollect_Memory", 0.55f);
        }
    }

    private Button GetSelectedCompletedButton()
    {
        if (completedButtons == null || selectedCompletedButtonIndex < 0 || selectedCompletedButtonIndex >= completedButtons.Length)
        {
            return null;
        }

        return completedButtons[selectedCompletedButtonIndex];
    }

    private void UpdateCompletedButtonVisuals()
    {
        if (completedButtons == null)
        {
            return;
        }

        for (int i = 0; i < completedButtons.Length; i++)
        {
            if (completedButtons[i] == null)
            {
                continue;
            }

            if (completedButtons[i].targetGraphic is Image image)
            {
                image.color = i == selectedCompletedButtonIndex ? completedButtonSelected : completedButtonNormal;
            }
        }
    }

    private static bool CompletedUpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame))
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#else
        return false;
#endif
    }

    private static bool CompletedDownPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame))
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
#else
        return false;
#endif
    }

    private static bool CompletedSubmitPressed()
    {
        if (GameInput.SubmitPressed || GameInput.InteractPressed)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return true;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }

    private static Image CreateCompletedImage(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private void CacheAndDimLights()
    {
        // Find sunset directional light
        sunLight = GameObject.Find("REPLACE_Ending_SunsetDirectionalLight")?.GetComponent<Light>();
        if (sunLight == null)
        {
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional)
                {
                    sunLight = l;
                    break;
                }
            }
        }

        if (sunLight != null)
        {
            originalSunIntensity = sunLight.intensity;
            sunLight.intensity = originalSunIntensity * 0.18f; // Dim the sun to 18% (u ám look)
        }

        // Cache and turn off sunset disc glow lights
        sunsetDiscLights.Clear();
        originalSunsetDiscIntensities.Clear();
        if (finalLightObject != null)
        {
            foreach (var l in finalLightObject.GetComponentsInChildren<Light>(true))
            {
                sunsetDiscLights.Add(l);
                originalSunsetDiscIntensities.Add(l.intensity);
                l.intensity = 0f; // Turn off sunset glow lights initially
            }
        }
    }

    private void RestoreLights()
    {
        if (sunLight != null)
        {
            sunLight.intensity = originalSunIntensity;
        }

        // Final light intensities will be handled or restored
        if (finalLightObject != null)
        {
            for (int i = 0; i < sunsetDiscLights.Count; i++)
            {
                if (sunsetDiscLights[i] != null)
                {
                    sunsetDiscLights[i].intensity = 2.2f; // Matches LightUpLandmark specification
                }
            }
        }
    }

    private IEnumerator LerpCamera(Camera cam, Vector3 targetPos, Quaternion targetRot, float duration)
    {
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
            foreach (var mem in group)
            {
                mem.LerpToRestored(t, grayTone);
            }
            yield return null;
        }
        foreach (var mem in group)
        {
            mem.ApplyRestored();
        }
    }

    private void ApplyGrayMemoryInstant()
    {
        foreach (var mem in cachedRenderers)
        {
            mem.ApplyFaded(grayTone);
        }
    }

    private void CategorizeGroups()
    {
        boatGroup.Clear();
        redboatGroup.Clear();
        causaigonGroup.Clear();
        villaGroup.Clear();
        landmarkGroup.Clear();
        otherGroup.Clear();

        foreach (var mem in cachedRenderers)
        {
            if (!mem.IsValid) continue;

            string nameLower = mem.renderer.gameObject.name.ToLower();
            string pathLower = mem.path.ToLower();

            if (nameLower.Contains("redboat") || pathLower.Contains("redboat"))
            {
                redboatGroup.Add(mem);
            }
            else if (nameLower.Contains("boat") || pathLower.Contains("boat"))
            {
                boatGroup.Add(mem);
            }
            else if (nameLower.Contains("causaigon") || nameLower.Contains("bridge") || pathLower.Contains("causaigon"))
            {
                causaigonGroup.Add(mem);
            }
            else if (nameLower.Contains("villa") || pathLower.Contains("villa"))
            {
                villaGroup.Add(mem);
            }
            else if (nameLower.Contains("landmark") || pathLower.Contains("landmark") || nameLower.Contains("tower") || nameLower.Contains("spire"))
            {
                landmarkGroup.Add(mem);
            }
            else
            {
                otherGroup.Add(mem);
            }
        }
    }

    private void LightUpShard(int index)
    {
        if (memoryShards == null || index < 0 || index >= memoryShards.Length || memoryShards[index] == null)
        {
            return;
        }

        Renderer renderer = memoryShards[index].GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = shardWarm;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", shardWarm * 1.8f);
        }

        Light[] lights = memoryShards[index].GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = 1.25f;
        }
    }

    private void LightUpLandmark()
    {
        if (landmarkTower == null)
        {
            return;
        }

        Renderer renderer = landmarkTower.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color towerColor = new Color(0.54f, 0.74f, 1f);
            renderer.material.color = towerColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", towerColor * 1.6f);
        }

        if (finalLightObject != null)
        {
            Renderer finalRenderer = finalLightObject.GetComponent<Renderer>();
            if (finalRenderer != null)
            {
                finalRenderer.material.EnableKeyword("_EMISSION");
                finalRenderer.material.SetColor("_EmissionColor", shardWarm * 2f);
            }

            Light[] lights = finalLightObject.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = 2.2f;
            }
        }
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

    // Helper classes for desaturation and group restoration
    private class RendererMemory
    {
        public readonly Renderer renderer;
        public readonly string path;
        private readonly MaterialMemory[] materials;

        public bool IsValid => renderer != null;
        public bool HasAnyMaterial => materials.Length > 0;

        public RendererMemory(Renderer renderer)
        {
            this.renderer = renderer;
            path = GetPath(renderer.transform);

            Material[] runtimeMaterials = renderer.materials;
            List<MaterialMemory> materialMemories = new List<MaterialMemory>();
            foreach (Material material in runtimeMaterials)
            {
                if (material == null || !MaterialMemory.HasColorProperty(material))
                {
                    continue;
                }

                materialMemories.Add(new MaterialMemory(material));
            }
            materials = materialMemories.ToArray();
        }

        public void ApplyFaded(Color grayTone)
        {
            foreach (MaterialMemory mat in materials)
            {
                mat.ApplyFaded(grayTone);
            }
        }

        public void ApplyRestored()
        {
            foreach (MaterialMemory mat in materials)
            {
                mat.ApplyRestored();
            }
        }

        public void LerpToRestored(float t, Color grayTone)
        {
            foreach (MaterialMemory mat in materials)
            {
                mat.LerpToRestored(t, grayTone);
            }
        }
    }

    private class MaterialMemory
    {
        private readonly Material material;
        private readonly Color originalColor;
        private readonly Color originalEmission;
        private readonly bool hadEmission;
        private readonly int colorPropId;
        private readonly int emissionPropId;

        public MaterialMemory(Material material)
        {
            this.material = material;
            
            if (material.HasProperty("_BaseColor"))
            {
                colorPropId = Shader.PropertyToID("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                colorPropId = Shader.PropertyToID("_Color");
            }
            else if (material.HasProperty("baseColorFactor"))
            {
                colorPropId = Shader.PropertyToID("baseColorFactor");
            }
            else
            {
                colorPropId = -1;
            }

            originalColor = colorPropId != -1 ? material.GetColor(colorPropId) : Color.white;

            if (material.HasProperty("emissiveFactor"))
            {
                emissionPropId = Shader.PropertyToID("emissiveFactor");
                hadEmission = true;
            }
            else if (material.HasProperty("_EmissionColor"))
            {
                emissionPropId = Shader.PropertyToID("_EmissionColor");
                hadEmission = true;
            }
            else
            {
                emissionPropId = -1;
                hadEmission = false;
            }

            originalEmission = hadEmission ? material.GetColor(emissionPropId) : Color.black;
        }

        public void ApplyFaded(Color grayTone)
        {
            if (colorPropId != -1)
            {
                Color desaturated = GetDesaturatedColor(originalColor, grayTone);
                material.SetColor(colorPropId, desaturated);
            }
            if (hadEmission)
            {
                material.SetColor(emissionPropId, Color.black);
            }
        }

        public void ApplyRestored()
        {
            if (colorPropId != -1)
            {
                material.SetColor(colorPropId, originalColor);
            }
            if (hadEmission)
            {
                material.SetColor(emissionPropId, originalEmission);
            }
        }

        public void LerpToRestored(float t, Color grayTone)
        {
            if (colorPropId != -1)
            {
                Color fadedColor = GetDesaturatedColor(originalColor, grayTone);
                Color targetColor = Color.Lerp(fadedColor, originalColor, t);
                material.SetColor(colorPropId, targetColor);
            }
            
            if (hadEmission)
            {
                material.SetColor(emissionPropId, Color.Lerp(Color.black, originalEmission, t));
            }
        }

        private Color GetDesaturatedColor(Color original, Color grayTone)
        {
            float luminance = original.r * 0.299f + original.g * 0.587f + original.b * 0.114f;
            Color gray = new Color(luminance, luminance, luminance, original.a);
            
            // Make the color very dark (u ám) to correctly darken textured materials
            float brightness = 0.22f;
            Color faded = gray * brightness;
            
            // Blend strongly with the dark grey-blue tint (85%)
            faded = Color.Lerp(faded, grayTone * brightness, 0.85f);
            faded.a = original.a; // Keep original transparency
            
            return faded;
        }

        public static bool HasColorProperty(Material material)
        {
            return material.HasProperty("_BaseColor") || material.HasProperty("_Color") || material.HasProperty("baseColorFactor");
        }
    }
}
