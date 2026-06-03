using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class BusHubRouteMapUI : MonoBehaviour
{
    private struct RouteOption
    {
        public string displayName;
        public string sceneName;
        public LocationId locationId;
        public bool developerOnly;

        public RouteOption(string displayName, string sceneName, LocationId locationId, bool developerOnly)
        {
            this.displayName = displayName;
            this.sceneName = sceneName;
            this.locationId = locationId;
            this.developerOnly = developerOnly;
        }
    }

    private static readonly RouteOption[] AllRoutes =
    {
        new RouteOption("Nguyễn Huệ", SceneLoader.NguyenHue, LocationId.NguyenHue, false),
        new RouteOption("Chợ Bến Thành", SceneLoader.BenThanh, LocationId.BenThanh, false),
        new RouteOption("Dinh Độc Lập", SceneLoader.DinhDocLap, LocationId.DinhDocLap, false),
        new RouteOption("Nhà thờ Đức Bà", SceneLoader.NhaThoDucBa, LocationId.NhaThoDucBa, false),
        new RouteOption("Bitexco", SceneLoader.Bitexco, LocationId.Bitexco, false),
        new RouteOption("Bến Bạch Đằng", SceneLoader.BachDang, LocationId.BachDang, false),
        new RouteOption("DEV: Test Ending", SceneLoader.Ending, LocationId.None, true)
    };

    private readonly List<RouteOption> visibleRoutes = new List<RouteOption>();
    private readonly List<Button> buttons = new List<Button>();
    private GameObject panel;
    private int selectedIndex;
    private int openedFrame = -1;

    public bool IsOpen => panel != null && panel.activeSelf;

    public void Open()
    {
        EnsureBuilt();
        RefreshRoutes();
        panel.SetActive(true);
        openedFrame = Time.frameCount;
        selectedIndex = Mathf.Clamp(selectedIndex, 0, buttons.Count - 1);
        UIManager.Instance?.SetExternalInputBlocked(true);
        UIManager.Instance?.ShowInteractionPrompt(false, string.Empty);
        RefreshSelection();
        PrototypeLogger.Info("Opened BusHub route map UI.");
    }

    public void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        UIManager.Instance?.SetExternalInputBlocked(false);
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Time.frameCount == openedFrame)
        {
            return;
        }

        if (GameInput.CancelPressed)
        {
            Close();
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
            MoveSelection(-3);
        }
        else if (PressedDown())
        {
            MoveSelection(3);
        }

        if (GameInput.SubmitPressed || GameInput.InteractPressed)
        {
            SelectCurrent();
        }
    }

    private void EnsureBuilt()
    {
        if (panel != null)
        {
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("UI_Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        panel = new GameObject("BusHubRouteMapPanel");
        panel.transform.SetParent(canvas.transform, false);

        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.04f, 0.05f, 0.06f, 0.92f);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 470f);
        rect.anchoredPosition = Vector2.zero;

        CreateText(panel.transform, "Title", "Bản đồ lộ trình ký ức", new Vector2(0f, 190f), 34, TextAnchor.MiddleCenter);
        CreateText(panel.transform, "Hint", "WASD / phím mũi tên để chọn, Enter hoặc E để đi, Esc để đóng", new Vector2(0f, -200f), 18, TextAnchor.MiddleCenter);
        panel.SetActive(false);
    }

    private void RefreshRoutes()
    {
        foreach (Button button in buttons)
        {
            Destroy(button.gameObject);
        }

        buttons.Clear();
        visibleRoutes.Clear();

        bool developerMode = DeveloperMode.IsEnabled;
        foreach (RouteOption route in AllRoutes)
        {
            if (route.developerOnly && !developerMode)
            {
                continue;
            }

            visibleRoutes.Add(route);
        }

        for (int i = 0; i < visibleRoutes.Count; i++)
        {
            int capturedIndex = i;
            RouteOption route = visibleRoutes[i];
            Button button = CreateRouteButton(route, i);
            button.onClick.AddListener(() =>
            {
                selectedIndex = capturedIndex;
                SelectCurrent();
            });

            buttons.Add(button);
        }
    }

    private Button CreateRouteButton(RouteOption route, int index)
    {
        GameObject buttonObject = new GameObject("MapButton_" + route.displayName.Replace(" ", "_"));
        buttonObject.transform.SetParent(panel.transform, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = route.developerOnly ? new Color(0.28f, 0.38f, 0.95f) : GetRouteColor(route.locationId);

        Button button = buttonObject.AddComponent<Button>();
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(210f, 86f);
        rect.anchoredPosition = GetButtonPosition(index);

        Text label = CreateText(buttonObject.transform, "Label", route.displayName, Vector2.zero, 22, TextAnchor.MiddleCenter);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(190f, 70f);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 14;
        label.resizeTextMaxSize = 22;
        label.color = Color.white;
        return button;
    }

    private Vector2 GetButtonPosition(int index)
    {
        if (index == 6)
        {
            return new Vector2(0f, -120f);
        }

        int row = index / 3;
        int column = index % 3;
        return new Vector2(-240f + column * 240f, 75f - row * 110f);
    }

    private Color GetRouteColor(LocationId locationId)
    {
        GameProgressManager progress = GameProgressManager.Instance;
        if (progress != null && progress.IsRestored(locationId))
        {
            return new Color(0.18f, 0.62f, 0.25f);
        }

        return new Color(0.95f, 0.76f, 0.08f);
    }

    private Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, int fontSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text textComponent = textObject.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = fontSize;
        textComponent.color = Color.white;
        textComponent.alignment = anchor;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(680f, 56f);
        rect.anchoredPosition = anchoredPosition;
        return textComponent;
    }

    private void MoveSelection(int amount)
    {
        if (buttons.Count == 0)
        {
            return;
        }

        selectedIndex = (selectedIndex + amount + buttons.Count) % buttons.Count;
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Image image = buttons[i].GetComponent<Image>();
            RouteOption route = visibleRoutes[i];
            Color color = route.developerOnly ? new Color(0.28f, 0.38f, 0.95f) : GetRouteColor(route.locationId);
            image.color = i == selectedIndex ? Color.white : color;

            Text text = buttons[i].GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = i == selectedIndex ? Color.black : Color.white;
            }
        }
    }

    private void SelectCurrent()
    {
        if (selectedIndex < 0 || selectedIndex >= visibleRoutes.Count)
        {
            return;
        }

        RouteOption route = visibleRoutes[selectedIndex];
        PrototypeLogger.Info("Route map selected: " + route.displayName + " -> " + route.sceneName);
        Close();
        SceneLoader.Load(route.sceneName);
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
