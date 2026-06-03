using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BusHubMapUIController : MonoBehaviour
{
    private static readonly Vector2 PaperSize = new Vector2(1680f, 885f);

    private class RouteData
    {
        public string displayName;
        public string subtitle;
        public string description;
        public string sceneName;
        public LocationId locationId;
        public Vector2 position;
        public string nodeName;
        public string iconPath;

        public RouteData(string displayName, string subtitle, string description, string sceneName, LocationId locationId, Vector2 position, string nodeName, string iconPath)
        {
            this.displayName = displayName;
            this.subtitle = subtitle;
            this.description = description;
            this.sceneName = sceneName;
            this.locationId = locationId;
            this.position = position;
            this.nodeName = nodeName;
            this.iconPath = iconPath;
        }
    }

    private readonly RouteData[] routes =
    {
        new RouteData("Nguyễn Huệ", "Nhịp sống trẻ", "Phố đi bộ, âm nhạc đường phố và nhịp sống trẻ.", SceneLoader.NguyenHue, LocationId.NguyenHue, new Vector2(-300f, -145f), "Node_NguyenHue", "Assets/Art/UI/RouteIcons/icon_route_nguyen_hue.png"),
        new RouteData("Chợ Bến Thành", "Đời sống thường ngày", "Tiếng rao, đời sống thường ngày và ký ức khu chợ.", SceneLoader.BenThanh, LocationId.BenThanh, new Vector2(-555f, -20f), "Node_ChoBenThanh", "Assets/Art/UI/RouteIcons/icon_route_cho_ben_thanh.png"),
        new RouteData("Dinh Độc Lập", "Lịch sử", "Lịch sử, sương mù và chiếc radio năm 1975.", SceneLoader.DinhDocLap, LocationId.DinhDocLap, new Vector2(-360f, 200f), "Node_DinhDocLap", "Assets/Art/UI/RouteIcons/icon_route_dinh_doc_lap.png"),
        new RouteData("Nhà thờ Đức Bà", "Bình yên", "Tiếng chuông, bồ câu và khoảng lặng giữa đô thị.", SceneLoader.NhaThoDucBa, LocationId.NhaThoDucBa, new Vector2(-40f, 205f), "Node_NhaThoDucBa", "Assets/Art/UI/RouteIcons/icon_route_nha_tho_duc_ba.png"),
        new RouteData("Bitexco", "Chuyển mình", "Ánh đèn hiện đại, áp lực công sở và sự chuyển mình.", SceneLoader.Bitexco, LocationId.Bitexco, new Vector2(285f, 80f), "Node_Bitexco", "Assets/Art/UI/RouteIcons/icon_route_bitexco.png"),
        new RouteData("Bến Bạch Đằng", "Dòng chảy thành phố", "Dòng sông, bến cảng và hành trình của thành phố.", SceneLoader.BachDang, LocationId.BachDang, new Vector2(410f, -190f), "Node_BenBachDang", "Assets/Art/UI/RouteIcons/icon_route_ben_bach_dang.png")
    };

    private readonly List<RouteMapNodeUI> nodes = new List<RouteMapNodeUI>();
    private GameObject overlayRoot;
    private RectTransform paperPanel;
    private CanvasGroup overlayGroup;
    private RectTransform routeNodesRoot;
    private TMP_Text detailTitleText;
    private TMP_Text detailSubtitleText;
    private TMP_Text detailStatusText;
    private TMP_Text detailDescriptionText;
    private TMP_Text devModeText;
    private Button devEndingButton;
    private RectTransform devPanel;
    private Sprite circleSprite;
    private int selectedIndex;
    private int openedFrame = -1;
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
        selectedIndex = Mathf.Clamp(selectedIndex, 0, nodes.Count - 1);
        SelectNode(selectedIndex);
        UIManager.Instance?.SetExternalInputBlocked(true);
        UIManager.Instance?.ShowInteractionPrompt(false, string.Empty);
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PrototypeLogger.Info("Opened paper route map.");
    }

    public void CloseMap()
    {
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
        if (!IsOpen || Time.frameCount == openedFrame)
        {
            return;
        }

        HandleKeyboardInput();
    }

    public void RefreshNodes()
    {
        GameProgressManager progress = GameProgressManager.Instance;
        foreach (RouteMapNodeUI node in nodes)
        {
            node.RefreshState(progress, DeveloperMode.IsEnabled);
        }

        bool developerMode = DeveloperMode.IsEnabled;
        if (devPanel != null)
        {
            devPanel.gameObject.SetActive(developerMode);
        }

        if (devEndingButton != null)
        {
            devEndingButton.gameObject.SetActive(developerMode);
        }

        if (devModeText != null)
        {
            devModeText.gameObject.SetActive(developerMode);
        }
    }

    public void SelectNode(int index)
    {
        if (nodes.Count == 0)
        {
            return;
        }

        selectedIndex = Mathf.Clamp(index, 0, nodes.Count - 1);
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].SetSelected(i == selectedIndex);
        }

        UpdateDetailPanel();
    }

    public void ConfirmSelectedRoute()
    {
        if (selectedIndex < 0 || selectedIndex >= nodes.Count)
        {
            return;
        }

        RouteMapNodeUI node = nodes[selectedIndex];
        PrototypeLogger.Info("Paper map selected: " + node.displayName + " -> " + node.sceneName);
        CloseMap();
        node.Confirm();
    }

    public void UpdateDetailPanel()
    {
        if (selectedIndex < 0 || selectedIndex >= nodes.Count)
        {
            return;
        }

        RouteMapNodeUI node = nodes[selectedIndex];
        if (detailTitleText != null) detailTitleText.text = node.displayName;
        if (detailSubtitleText != null) detailSubtitleText.text = node.subtitle;
        if (detailStatusText != null) detailStatusText.text = node.IsRestored ? "Đã khôi phục" : "Chưa khôi phục";
        if (detailDescriptionText != null)
        {
            detailDescriptionText.text = node.description + "\n\nNhấn Enter / E để khởi hành";
        }
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
            MoveSelection(-1);
        }
        else if (PressedRight())
        {
            MoveSelection(1);
        }
        else if (PressedUp())
        {
            MoveSelection(-2);
        }
        else if (PressedDown())
        {
            MoveSelection(2);
        }

        if (GameInput.SubmitPressed || GameInput.InteractPressed)
        {
            ConfirmSelectedRoute();
        }
    }

    private void MoveSelection(int amount)
    {
        if (nodes.Count == 0)
        {
            return;
        }

        selectedIndex = (selectedIndex + amount + nodes.Count) % nodes.Count;
        SelectNode(selectedIndex);
    }

    private void EnsureBuilt()
    {
        if (overlayRoot != null)
        {
            return;
        }

        circleSprite = CreateCircleSprite();
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
        paperPanel = CreateRoot(overlayRoot.transform, "PaperMapRoot", PaperSize);
        paperPanel.localRotation = Quaternion.Euler(0f, 0f, -1.25f);
        CreatePanel(paperPanel, "PaperShadow", PaperSize, new Vector2(22f, -22f), new Color(0.02f, 0.015f, 0.01f, 0.38f));
        RectTransform paperBackground = CreatePanel(paperPanel, "PaperBackground", PaperSize, Vector2.zero, new Color(0.84f, 0.74f, 0.53f, 1f));
        Outline paperBorder = paperBackground.GetComponent<Outline>();
        if (paperBorder != null)
        {
            paperBorder.effectColor = new Color(0.16f, 0.08f, 0.025f, 0.9f);
            paperBorder.effectDistance = new Vector2(4f, -4f);
        }

        CreatePaperDetails();
        CreateRouteLinesAndNodes();
        CreateLegendAndDetailPanel();
        CreateFooterAndDevButton();
        overlayRoot.SetActive(false);
    }

    private System.Collections.IEnumerator AnimateOpen()
    {
        overlayGroup.alpha = 0f;
        paperPanel.localScale = Vector3.one * 0.92f;
        const float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            overlayGroup.alpha = t;
            paperPanel.localScale = Vector3.Lerp(Vector3.one * 0.92f, Vector3.one, t);
            yield return null;
        }

        overlayGroup.alpha = 1f;
        paperPanel.localScale = Vector3.one;
    }

    private Canvas CreateOverlayCanvas()
    {
        // Editor note: check UI sharpness with Game View Scale at 1x,
        // or use Maximize On Play. Scaled editor previews can look softer.
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
        RectTransform background = CreatePanel(parent, "DimBackground", Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.66f));
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;
    }

    private void CreatePaperDetails()
    {
        TMP_Text title = CreateText(paperPanel, "MapTitleText", "KÝ ỨC SÀI GÒN", new Vector2(-420f, 363f), new Vector2(700f, 82f), 54, new Color(0.12f, 0.07f, 0.03f), TextAlignmentOptions.MidlineLeft);
        title.fontStyle = FontStyles.Bold;
        title.outlineWidth = 0.08f;
        title.outlineColor = new Color(0.98f, 0.88f, 0.62f, 0.55f);
        CreateText(paperPanel, "MapSubtitleText", "Bản đồ lộ trình ký ức", new Vector2(285f, 342f), new Vector2(430f, 44f), 28, new Color(0.23f, 0.12f, 0.04f), TextAlignmentOptions.Center);

        RectTransform folds = CreateRoot(paperPanel, "PaperFoldLines", PaperSize);
        CreatePanel(folds, "VerticalCenterFold", new Vector2(3f, 785f), new Vector2(-80f, 0f), new Color(1f, 0.94f, 0.72f, 0.18f));
        CreatePanel(folds, "HorizontalFold", new Vector2(1530f, 3f), new Vector2(0f, -20f), new Color(0.34f, 0.21f, 0.1f, 0.16f));
        CreatePanel(paperPanel, "PaperCornerShadow", new Vector2(300f, 118f), new Vector2(580f, -350f), new Color(0.26f, 0.15f, 0.06f, 0.16f));
        CreateCornerMark("CornerMark_TopLeft", new Vector2(-790f, 395f), 1f);
        CreateCornerMark("CornerMark_TopRight", new Vector2(790f, 395f), -1f);
        CreateCornerMark("CornerMark_BottomLeft", new Vector2(-790f, -395f), 1f);
        CreateCornerMark("CornerMark_BottomRight", new Vector2(790f, -395f), -1f);
    }

    private void CreateCornerMark(string name, Vector2 position, float horizontalSign)
    {
        RectTransform mark = CreateRoot(paperPanel, name, new Vector2(70f, 70f));
        mark.anchoredPosition = position;
        CreatePanel(mark, "CornerMark_Horizontal", new Vector2(54f, 5f), new Vector2(-horizontalSign * 8f, 0f), new Color(0.2f, 0.1f, 0.03f, 0.62f));
        CreatePanel(mark, "CornerMark_Vertical", new Vector2(5f, 54f), new Vector2(0f, -Mathf.Sign(position.y) * 8f), new Color(0.2f, 0.1f, 0.03f, 0.62f));
    }

    private void CreateRouteLinesAndNodes()
    {
        RectTransform lineRoot = CreateRoot(paperPanel, "RouteLineContainer", new Vector2(1500f, 780f));
        routeNodesRoot = CreateRoot(paperPanel, "RouteNodeContainer", new Vector2(1500f, 780f));
        Vector2 routeMapOffset = new Vector2(-165f, -28f);
        lineRoot.anchoredPosition = routeMapOffset;
        routeNodesRoot.anchoredPosition = routeMapOffset;

        for (int i = 0; i < routes.Length - 1; i++)
        {
            CreateLine(lineRoot, routes[i].position, routes[i + 1].position);
        }

        nodes.Clear();
        for (int i = 0; i < routes.Length; i++)
        {
            RouteMapNodeUI node = CreateNode(routeNodesRoot, routes[i], i);
            nodes.Add(node);
        }
    }

    private void CreateLegendAndDetailPanel()
    {
        RectTransform legend = CreatePanel(paperPanel, "LegendPanel", new Vector2(335f, 108f), new Vector2(-590f, -335f), new Color(0.54f, 0.36f, 0.18f, 0.4f));
        CreateText(legend, "LegendTitle", "Chú giải", new Vector2(0f, 27f), new Vector2(230f, 26f), 19, new Color(0.14f, 0.07f, 0.03f), TextAlignmentOptions.Center);
        CreateSmallLegendDot(legend, new Vector2(-104f, -18f), new Color(1f, 0.63f, 0.08f), "Chưa khôi phục");
        CreateSmallLegendDot(legend, new Vector2(42f, -18f), new Color(0.48f, 0.62f, 0.18f), "Đã khôi phục");

        RectTransform detail = CreatePanel(paperPanel, "InfoPanel", new Vector2(420f, 440f), new Vector2(610f, 6f), new Color(0.64f, 0.49f, 0.28f, 0.92f));
        detailTitleText = CreateText(detail, "DetailTitle", string.Empty, new Vector2(0f, 168f), new Vector2(382f, 54f), 34, new Color(0.1f, 0.05f, 0.02f), TextAlignmentOptions.Center);
        detailTitleText.fontStyle = FontStyles.Bold;
        detailSubtitleText = CreateText(detail, "DetailSubtitle", string.Empty, new Vector2(0f, 114f), new Vector2(382f, 36f), 22, new Color(0.2f, 0.09f, 0.03f), TextAlignmentOptions.Center);
        detailStatusText = CreateText(detail, "DetailStatus", string.Empty, new Vector2(0f, 70f), new Vector2(382f, 32f), 20, new Color(0.12f, 0.06f, 0.02f), TextAlignmentOptions.Center);
        detailDescriptionText = CreateText(detail, "DetailDescription", string.Empty, new Vector2(0f, -72f), new Vector2(360f, 220f), 20, new Color(0.1f, 0.05f, 0.02f), TextAlignmentOptions.TopLeft);
    }

    private void CreateFooterAndDevButton()
    {
        CreateText(paperPanel, "InputHintText", "WASD / phím mũi tên để chọn, Enter hoặc E để đi, Esc để đóng", new Vector2(0f, -405f), new Vector2(1040f, 38f), 24, new Color(0.17f, 0.08f, 0.03f), TextAlignmentOptions.Center);
        devPanel = CreatePanel(paperPanel, "DevPanel", new Vector2(285f, 96f), new Vector2(650f, 338f), new Color(0.32f, 0.22f, 0.16f, 0.42f));
        devModeText = CreateText(devPanel, "DevModeText", "DEV MODE: mở khóa để test", new Vector2(0f, 24f), new Vector2(230f, 28f), 18, new Color(0.48f, 0.05f, 0.03f), TextAlignmentOptions.Center);

        GameObject devObject = new GameObject("DevButton_Ending");
        devObject.transform.SetParent(devPanel, false);
        Image image = devObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.26f, 0.55f, 0.95f);
        devEndingButton = devObject.AddComponent<Button>();
        devEndingButton.targetGraphic = image;
        RectTransform rect = devObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(240f, 50f);
        rect.anchoredPosition = new Vector2(0f, -18f);
        CreateText(rect, "Label", "DEV: Test Ending", Vector2.zero, new Vector2(200f, 34f), 18, Color.white, TextAlignmentOptions.Center);
        devEndingButton.onClick.AddListener(() =>
        {
            CloseMap();
            SceneLoader.Load(SceneLoader.Ending);
        });
    }

    private RouteMapNodeUI CreateNode(Transform parent, RouteData route, int index)
    {
        GameObject nodeObject = new GameObject(route.nodeName);
        nodeObject.transform.SetParent(parent, false);
        RectTransform rect = nodeObject.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(220f, 248f);
        rect.anchoredPosition = route.position;

        GameObject buttonObject = new GameObject("ButtonRoot");
        buttonObject.transform.SetParent(nodeObject.transform, false);
        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.sizeDelta = rect.sizeDelta;
        buttonRect.anchoredPosition = Vector2.zero;
        Image hitImage = buttonObject.AddComponent<Image>();
        hitImage.color = new Color(1f, 1f, 1f, 0.001f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = hitImage;

        Image glow = CreateImage(nodeObject.transform, "SelectedGlow", new Vector2(176f, 176f), new Vector2(0f, 72f), new Color(1f, 0.82f, 0.18f, 0.32f), circleSprite);
        glow.raycastTarget = false;
        glow.enabled = false;
        Image outline = CreateImage(nodeObject.transform, "SelectedOutline", new Vector2(162f, 162f), new Vector2(0f, 72f), new Color(1f, 0.95f, 0.55f, 1f), circleSprite);
        outline.raycastTarget = false;
        outline.enabled = false;

        Image iconFrame = CreateImage(nodeObject.transform, "IconFrame", new Vector2(150f, 150f), new Vector2(0f, 72f), new Color(1f, 0.63f, 0.08f, 1f), circleSprite);
        Sprite routeSprite = LoadRouteIcon(route.iconPath);
        Image landmarkIcon = CreateImage(nodeObject.transform, "LandmarkIcon", new Vector2(136f, 136f), new Vector2(0f, 72f), Color.white, routeSprite != null ? routeSprite : circleSprite);
        landmarkIcon.preserveAspect = true;

        Image restoredBadge = CreateImage(nodeObject.transform, "RestoredBadge", new Vector2(38f, 38f), new Vector2(58f, 132f), new Color(0.28f, 0.58f, 0.2f, 1f), circleSprite);
        TMP_Text badgeText = CreateText(restoredBadge.transform, "BadgeText", "✓", Vector2.zero, new Vector2(30f, 30f), 21, Color.white, TextAlignmentOptions.Center);

        Image lockOverlay = CreateImage(nodeObject.transform, "LockOverlay", new Vector2(150f, 150f), new Vector2(0f, 72f), new Color(0.05f, 0.04f, 0.03f, 0.48f), circleSprite);
        lockOverlay.gameObject.SetActive(false);

        CreatePanel(rect, "LabelBox", new Vector2(210f, 94f), new Vector2(0f, -67f), new Color(0.94f, 0.84f, 0.59f, 0.98f));
        TMP_Text title = CreateText(rect, "TitleText", route.displayName, new Vector2(0f, -41f), new Vector2(194f, 32f), 24, new Color(0.08f, 0.04f, 0.015f), TextAlignmentOptions.Center);
        TMP_Text subtitle = CreateText(rect, "SubtitleText", route.subtitle, new Vector2(0f, -69f), new Vector2(194f, 25f), 16, new Color(0.18f, 0.08f, 0.025f), TextAlignmentOptions.Center);
        TMP_Text status = CreateText(rect, "StatusText", string.Empty, new Vector2(0f, -95f), new Vector2(194f, 24f), 15, new Color(0.12f, 0.06f, 0.02f), TextAlignmentOptions.Center);

        RouteMapNodeUI node = nodeObject.AddComponent<RouteMapNodeUI>();
        node.displayName = route.displayName;
        node.subtitle = route.subtitle;
        node.description = route.description;
        node.sceneName = route.sceneName;
        node.locationId = route.locationId;
        node.button = button;
        node.iconFrameImage = iconFrame;
        node.landmarkIconImage = landmarkIcon;
        node.selectionOutline = outline;
        node.selectionGlow = glow;
        node.restoredBadge = restoredBadge;
        node.restoredBadgeText = badgeText;
        node.lockOverlay = lockOverlay;
        node.titleText = title;
        node.subtitleText = subtitle;
        node.statusText = status;

        int capturedIndex = index;
        button.onClick.AddListener(() =>
        {
            SelectNode(capturedIndex);
            ConfirmSelectedRoute();
        });

        return node;
    }

    private void CreateLine(RectTransform parent, Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        RectTransform line = CreatePanel(parent, "RouteLine", new Vector2(direction.magnitude, 8f), (start + end) * 0.5f, new Color(0.12f, 0.38f, 0.36f, 0.95f));
        line.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void CreateSmallLegendDot(RectTransform parent, Vector2 position, Color color, string label)
    {
        CreateImage(parent, "LegendDot", new Vector2(20f, 20f), position, color, circleSprite);
        CreateText(parent, "LegendText", label, position + new Vector2(68f, 0f), new Vector2(120f, 24f), 14, new Color(0.12f, 0.06f, 0.02f), TextAlignmentOptions.MidlineLeft);
    }

    private RectTransform CreateRoot(Transform parent, string name)
    {
        return CreateRoot(parent, name, new Vector2(1180f, 680f));
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
        outline.effectColor = new Color(0.2f, 0.1f, 0.03f, 0.45f);
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

    private Sprite LoadRouteIcon(string assetPath)
    {
#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
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
