using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BusHubMapUIController : MonoBehaviour
{
    private class RouteData
    {
        public string objectName;
        public string displayName;
        public string sceneName;
        public LocationId locationId;
        public string cardPath;
        public string fallbackCardPath;

        public RouteData(string objectName, string displayName, string sceneName, LocationId locationId, string cardPath, string fallbackCardPath)
        {
            this.objectName = objectName;
            this.displayName = displayName;
            this.sceneName = sceneName;
            this.locationId = locationId;
            this.cardPath = cardPath;
            this.fallbackCardPath = fallbackCardPath;
        }
    }

    private const string HeaderPath = "Assets/Art/UI/BusHub/Header/bushub_title_card.png";
    private const string HeaderFallbackPath = "Assets/Art/UI/BusHub/bushub_title_card.png";

    private readonly RouteData[] routes =
    {
        new RouteData("RouteButton_NguyenHue", "Nguyễn Huệ", SceneLoader.NguyenHue, LocationId.NguyenHue, "Assets/Art/UI/BusHub/Routes/route_nguyen_hue.png", "Assets/Art/UI/BusHub/route_nguyen_hue.png"),
        new RouteData("RouteButton_ChoBenThanh", "Chợ Bến Thành", SceneLoader.BenThanh, LocationId.BenThanh, "Assets/Art/UI/BusHub/Routes/route_cho_ben_thanh.png", "Assets/Art/UI/BusHub/route_cho_ben_thanh.png"),
        new RouteData("RouteButton_DinhDocLap", "Dinh Độc Lập", SceneLoader.DinhDocLap, LocationId.DinhDocLap, "Assets/Art/UI/BusHub/Routes/route_dinh_doc_lap.png", "Assets/Art/UI/BusHub/route_dinh_doc_lap.png"),
        new RouteData("RouteButton_NhaThoDucBa", "Nhà thờ Đức Bà", SceneLoader.NhaThoDucBa, LocationId.NhaThoDucBa, "Assets/Art/UI/BusHub/Routes/route_nha_tho_duc_ba.png", "Assets/Art/UI/BusHub/route_nha_tho_duc_ba.png"),
        new RouteData("RouteButton_Bitexco", "Bitexco", SceneLoader.Bitexco, LocationId.Bitexco, "Assets/Art/UI/BusHub/Routes/route_bitexco.png", "Assets/Art/UI/BusHub/route_bitexco.png"),
        new RouteData("RouteButton_BenBachDang", "Bến Bạch Đằng", SceneLoader.BachDang, LocationId.BachDang, "Assets/Art/UI/BusHub/Routes/route_ben_bach_dang.png", "Assets/Art/UI/BusHub/route_ben_bach_dang.png")
    };

    private readonly List<BusHubRouteButtonUI> routeButtons = new List<BusHubRouteButtonUI>();
    private GameObject overlayRoot;
    private RectTransform boardRoot;
    private CanvasGroup overlayGroup;
    private RectTransform devTools;
    private Button devEndingButton;
    private int selectedIndex;
    private int openedFrame = -1;
    private bool confirmingRoute;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;

    public bool IsOpen => overlayRoot != null && overlayRoot.activeSelf;

    public void OpenMap()
    {
        EnsureBuilt();
        RefreshNodes();
        overlayRoot.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimateOpen());
        openedFrame = Time.frameCount;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, routeButtons.Count - 1);
        SelectNode(selectedIndex);
        UIManager.Instance?.SetExternalInputBlocked(true);
        UIManager.Instance?.ShowInteractionPrompt(false, string.Empty);
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PrototypeLogger.Info("Opened BusHub route board UI.");
    }

    public void CloseMap()
    {
        if (confirmingRoute)
        {
            return;
        }

        if (overlayRoot != null)
        {
            overlayRoot.SetActive(false);
        }

        UIManager.Instance?.SetExternalInputBlocked(false);
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
    }

    public void ToggleMap()
    {
        if (IsOpen)
        {
            CloseMap();
        }
        else
        {
            OpenMap();
        }
    }

    private void Update()
    {
        if (!IsOpen || Time.frameCount == openedFrame || confirmingRoute)
        {
            return;
        }

        HandleKeyboardInput();
    }

    public void RefreshNodes()
    {
        GameProgressManager progress = GameProgressManager.Instance;
        foreach (BusHubRouteButtonUI routeButton in routeButtons)
        {
            routeButton.RefreshState(progress);
        }

        bool developerMode = DeveloperMode.IsEnabled;
        if (devTools != null)
        {
            devTools.gameObject.SetActive(developerMode);
        }

        if (devEndingButton != null)
        {
            devEndingButton.gameObject.SetActive(developerMode);
        }
    }

    public void SelectNode(int index)
    {
        if (routeButtons.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, routeButtons.Count - 1);
        for (int i = 0; i < routeButtons.Count; i++)
        {
            routeButtons[i].SetSelected(i == selectedIndex);
        }
    }

    public void ConfirmSelectedRoute()
    {
        if (selectedIndex < 0 || selectedIndex >= routeButtons.Count || confirmingRoute)
        {
            return;
        }

        BusHubRouteButtonUI routeButton = routeButtons[selectedIndex];
        if (routeButton.IsLocked)
        {
            UIManager.Instance?.ShowDialogue("Địa điểm này chưa mở khóa.");
            return;
        }

        StartCoroutine(ConfirmAndLoad(routeButton));
    }

    private IEnumerator ConfirmAndLoad(BusHubRouteButtonUI routeButton)
    {
        confirmingRoute = true;
        PrototypeLogger.Info("BusHub route selected: " + routeButton.displayName + " -> " + routeButton.sceneName);
        yield return routeButton.AnimatePressed();

        overlayRoot.SetActive(false);
        UIManager.Instance?.SetExternalInputBlocked(false);
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;
        SceneLoader.Load(routeButton.sceneName);
    }

    private void HandleKeyboardInput()
    {
        if (GameInput.CancelPressed)
        {
            CloseMap();
            return;
        }

        if (PressedLeft())
        {
            MoveSelectionLeft();
        }
        else if (PressedRight())
        {
            MoveSelectionRight();
        }
        else if (PressedUp())
        {
            MoveSelectionVertical(-1);
        }
        else if (PressedDown())
        {
            MoveSelectionVertical(1);
        }

        if (GameInput.SubmitPressed || GameInput.InteractPressed)
        {
            ConfirmSelectedRoute();
        }
    }

    private void MoveSelectionLeft()
    {
        if (selectedIndex % 2 == 1)
        {
            SelectNode(selectedIndex - 1);
        }
    }

    private void MoveSelectionRight()
    {
        if (selectedIndex % 2 == 0 && selectedIndex + 1 < routeButtons.Count)
        {
            SelectNode(selectedIndex + 1);
        }
    }

    private void MoveSelectionVertical(int rowDelta)
    {
        int next = selectedIndex + rowDelta * 2;
        if (next >= 0 && next < routeButtons.Count)
        {
            SelectNode(next);
        }
    }

    private void EnsureBuilt()
    {
        if (overlayRoot != null)
        {
            return;
        }

        Canvas canvas = CreateOverlayCanvas();
        overlayRoot = new GameObject("RouteMapPanel");
        overlayRoot.transform.SetParent(canvas.transform, false);
        overlayGroup = overlayRoot.AddComponent<CanvasGroup>();
        RectTransform overlayRect = overlayRoot.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        CreateDimBackground(overlayRoot.transform);
        boardRoot = CreateRoot(overlayRoot.transform, "BusHubBoardRoot", new Vector2(1360f, 880f));
        CreatePanel(boardRoot, "BoardShadow", new Vector2(1360f, 880f), new Vector2(22f, -22f), new Color(0.02f, 0.01f, 0f, 0.42f));
        RectTransform boardPanel = CreatePanel(boardRoot, "BoardPanel", new Vector2(1360f, 880f), Vector2.zero, new Color(0.12f, 0.08f, 0.05f, 0.96f));
        Outline boardOutline = boardPanel.GetComponent<Outline>();
        if (boardOutline != null)
        {
            boardOutline.effectColor = new Color(1f, 0.74f, 0.28f, 0.85f);
            boardOutline.effectDistance = new Vector2(4f, -4f);
        }

        CreateHeaderImage();
        CreateRouteGrid();
        CreateDevTools();
        CreateFooterHint();
        overlayRoot.SetActive(false);
    }

    private IEnumerator AnimateOpen()
    {
        overlayGroup.alpha = 0f;
        boardRoot.localScale = Vector3.one * 0.94f;
        const float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            overlayGroup.alpha = t;
            boardRoot.localScale = Vector3.Lerp(Vector3.one * 0.94f, Vector3.one, t);
            yield return null;
        }

        overlayGroup.alpha = 1f;
        boardRoot.localScale = Vector3.one;
    }

    private Canvas CreateOverlayCanvas()
    {
        GameObject canvasObject = new GameObject("RouteMapOverlayCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private void CreateDimBackground(Transform parent)
    {
        RectTransform background = CreatePanel(parent, "DimBackground", Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.68f));
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;
    }

    private void CreateHeaderImage()
    {
        Image header = CreateImage(boardRoot, "HeaderImage", new Vector2(760f, 180f), new Vector2(0f, 325f), Color.white, LoadSprite(HeaderPath, HeaderFallbackPath));
        header.preserveAspect = true;
    }

    private void CreateRouteGrid()
    {
        RectTransform grid = CreateRoot(boardRoot, "RouteGrid", new Vector2(930f, 545f));
        grid.anchoredPosition = new Vector2(0f, -40f);

        Vector2[] positions =
        {
            new Vector2(-235f, 180f),
            new Vector2(235f, 180f),
            new Vector2(-235f, 0f),
            new Vector2(235f, 0f),
            new Vector2(-235f, -180f),
            new Vector2(235f, -180f)
        };

        routeButtons.Clear();
        for (int i = 0; i < routes.Length; i++)
        {
            BusHubRouteButtonUI routeButton = CreateRouteButton(grid, routes[i], positions[i], i);
            routeButtons.Add(routeButton);
        }
    }

    private BusHubRouteButtonUI CreateRouteButton(Transform parent, RouteData route, Vector2 position, int index)
    {
        RectTransform root = CreateRoot(parent, route.objectName, new Vector2(430f, 150f));
        root.anchoredPosition = position;

        GameObject buttonRoot = new GameObject("Button");
        buttonRoot.transform.SetParent(root, false);
        RectTransform buttonRect = buttonRoot.AddComponent<RectTransform>();
        buttonRect.sizeDelta = root.sizeDelta;
        buttonRect.anchoredPosition = Vector2.zero;
        Image hitImage = buttonRoot.AddComponent<Image>();
        hitImage.color = new Color(1f, 1f, 1f, 0.001f);
        Button button = buttonRoot.AddComponent<Button>();
        button.targetGraphic = hitImage;

        Image hoverGlow = CreateImage(root, "HoverGlow", new Vector2(456f, 176f), Vector2.zero, new Color(1f, 0.74f, 0.18f, 0.34f), null);
        hoverGlow.gameObject.SetActive(false);
        Image selectedOutline = CreateImage(root, "SelectedOutline", new Vector2(448f, 168f), Vector2.zero, new Color(1f, 0.96f, 0.72f, 0.95f), null);
        selectedOutline.gameObject.SetActive(false);
        Image cardImage = CreateImage(root, "CardImage", new Vector2(430f, 150f), Vector2.zero, Color.white, LoadSprite(route.cardPath, route.fallbackCardPath));
        cardImage.preserveAspect = true;

        GameObject completedBadge = CreateCompletedBadge(root);
        GameObject lockedOverlay = CreateLockedOverlay(root);

        TMP_Text debugText = CreateText(root, "OptionalDebugText", route.displayName, new Vector2(0f, -73f), new Vector2(390f, 26f), 16, new Color(0.16f, 0.07f, 0.02f), TextAlignmentOptions.Center);
        debugText.gameObject.SetActive(false);

        BusHubRouteButtonUI routeButton = root.gameObject.AddComponent<BusHubRouteButtonUI>();
        routeButton.displayName = route.displayName;
        routeButton.sceneName = route.sceneName;
        routeButton.locationId = route.locationId;
        routeButton.button = button;
        routeButton.cardImage = cardImage;
        routeButton.selectedOutline = selectedOutline;
        routeButton.hoverGlow = hoverGlow;
        routeButton.completedBadge = completedBadge;
        routeButton.lockedOverlay = lockedOverlay;
        routeButton.optionalDebugText = debugText;
        routeButton.Initialize(OnRouteHovered);

        int capturedIndex = index;
        button.onClick.AddListener(() =>
        {
            SelectNode(capturedIndex);
            ConfirmSelectedRoute();
        });

        return routeButton;
    }

    private GameObject CreateCompletedBadge(Transform parent)
    {
        Image badge = CreateImage(parent, "CompletedBadge", new Vector2(46f, 46f), new Vector2(186f, 58f), new Color(0.18f, 0.62f, 0.2f, 1f), CreateCircleSprite());
        CreateText(badge.transform, "CheckText", "✓", Vector2.zero, new Vector2(42f, 42f), 28, Color.white, TextAlignmentOptions.Center);
        badge.gameObject.SetActive(false);
        return badge.gameObject;
    }

    private GameObject CreateLockedOverlay(Transform parent)
    {
        Image overlay = CreateImage(parent, "LockedOverlay", new Vector2(430f, 150f), Vector2.zero, new Color(0f, 0f, 0f, 0.52f), null);
        CreateText(overlay.transform, "LockedText", "Chưa mở khóa", Vector2.zero, new Vector2(330f, 42f), 24, Color.white, TextAlignmentOptions.Center);
        overlay.gameObject.SetActive(false);
        return overlay.gameObject;
    }

    private void CreateDevTools()
    {
        devTools = CreatePanel(boardRoot, "DevTools", new Vector2(420f, 104f), new Vector2(445f, 330f), new Color(0.18f, 0.12f, 0.1f, 0.78f));
        CreateText(devTools, "DevModeText", "DEV MODE: all routes unlocked for testing", new Vector2(0f, 27f), new Vector2(370f, 32f), 16, new Color(1f, 0.75f, 0.35f), TextAlignmentOptions.Center);

        GameObject devObject = new GameObject("DevButton_TestEnding");
        devObject.transform.SetParent(devTools, false);
        Image image = devObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.26f, 0.55f, 0.96f);
        devEndingButton = devObject.AddComponent<Button>();
        devEndingButton.targetGraphic = image;
        RectTransform rect = devObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(310f, 48f);
        rect.anchoredPosition = new Vector2(0f, -22f);
        CreateText(rect, "Label", "DEV: Test Ending", Vector2.zero, new Vector2(290f, 34f), 18, Color.white, TextAlignmentOptions.Center);
        devEndingButton.onClick.AddListener(() =>
        {
            confirmingRoute = true;
            overlayRoot.SetActive(false);
            UIManager.Instance?.SetExternalInputBlocked(false);
            Cursor.lockState = previousCursorLockMode;
            Cursor.visible = previousCursorVisible;
            SceneLoader.Load(SceneLoader.Ending);
        });
    }

    private void CreateFooterHint()
    {
        CreateText(boardRoot, "InputHintText", "WASD / phím mũi tên để chọn, Enter hoặc E để đi, Esc để đóng", new Vector2(0f, -405f), new Vector2(980f, 34f), 22, new Color(1f, 0.84f, 0.52f), TextAlignmentOptions.Center);
    }

    private void OnRouteHovered(BusHubRouteButtonUI routeButton)
    {
        int index = routeButtons.IndexOf(routeButton);
        if (index >= 0)
        {
            SelectNode(index);
        }
    }

    private RectTransform CreateRoot(Transform parent, string name, Vector2 size)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent, false);
        RectTransform rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        return rect;
    }

    private RectTransform CreatePanel(Transform parent, string name, Vector2 size, Vector2 position, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.35f);
        outline.effectDistance = new Vector2(2f, -2f);
        return rect;
    }

    private Image CreateImage(Transform parent, string name, Vector2 size, Vector2 position, Color color, Sprite sprite)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return image;
    }

    private TMP_Text CreateText(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI uiText = textObject.AddComponent<TextMeshProUGUI>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.color = color;
        uiText.alignment = alignment;
        uiText.enableAutoSizing = true;
        uiText.fontSizeMin = Mathf.Max(10, fontSize - 8);
        uiText.fontSizeMax = fontSize;
        uiText.raycastTarget = false;
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return uiText;
    }

    private Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= 30f ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
    }

    private Sprite LoadSprite(string assetPath, string fallbackPath)
    {
#if UNITY_EDITOR
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null && !string.IsNullOrEmpty(fallbackPath))
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fallbackPath);
        }

        return sprite;
#else
        return null;
#endif
    }

    private bool PressedLeft()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
#else
        return false;
#endif
    }

    private bool PressedRight()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
#else
        return false;
#endif
    }

    private bool PressedUp()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#else
        return false;
#endif
    }

    private bool PressedDown()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
#else
        return false;
#endif
    }
}
