#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class ScenePublishHelper
{
    private const string MainMenuPath = "Assets/Scenes/Scene_MainMenu.unity";
    private const string LoadingPath = "Assets/Scenes/Scene_Loading.unity";

    [MenuItem("Ky Uc Sai Gon/Publish/Generate All Publish Scenes")]
    public static void GenerateAll()
    {
        CreateMainMenuScene();
        CreateLoadingScene();
        AddScenesToBuildSettings();
    }

    [MenuItem("Ky Uc Sai Gon/Publish/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        // 1. Create a new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.015f, 0.02f, 0.028f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
        }

        // 2. Create Canvas
        GameObject canvasObj = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 3. Create Main Panel
        GameObject mainPanel = new GameObject("MainPanel", typeof(RectTransform), typeof(Image));
        mainPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        mainPanel.GetComponent<Image>().color = new Color(0.012f, 0.015f, 0.022f, 0.98f);

        // 4. Create Logo/Title
        GameObject titleObj = new GameObject("GameTitle", typeof(RectTransform), typeof(Text));
        titleObj.transform.SetParent(mainPanel.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0f, 150f);
        titleRect.sizeDelta = new Vector2(600f, 80f);
        Text titleText = titleObj.GetComponent<Text>();
        titleText.text = "KÝ ỨC\nĐÔ THỊ";
        titleText.font = GameUIFont.Bold;
        titleText.fontSize = 46;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.96f, 0.83f, 0.36f); // Gold

        // Subtitle
        GameObject subObj = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
        subObj.transform.SetParent(mainPanel.transform, false);
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchoredPosition = new Vector2(0f, 80f);
        subRect.sizeDelta = new Vector2(600f, 35f);
        Text subText = subObj.GetComponent<Text>();
        subText.text = "Hành trình tìm lại thành phố";
        subText.font = GameUIFont.Regular;
        subText.fontSize = 16;
        subText.fontStyle = FontStyle.Italic;
        subText.alignment = TextAnchor.MiddleCenter;
        subText.color = new Color(0.72f, 0.75f, 0.8f);

        // Buttons Container
        GameObject btnGroup = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        btnGroup.transform.SetParent(mainPanel.transform, false);
        RectTransform groupRect = btnGroup.GetComponent<RectTransform>();
        groupRect.anchoredPosition = new Vector2(0f, -80f);
        groupRect.sizeDelta = new Vector2(300f, 240f);
        
        VerticalLayoutGroup layout = btnGroup.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = false;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;

        // Settings Panel
        GameObject settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        settingsPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform setRect = settingsPanel.GetComponent<RectTransform>();
        setRect.sizeDelta = new Vector2(500f, 350f);
        settingsPanel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.98f);
        Outline setOutline = settingsPanel.AddComponent<Outline>();
        setOutline.effectColor = new Color(0.96f, 0.83f, 0.36f, 0.6f);
        setOutline.effectDistance = new Vector2(2f, -2f);

        // Settings Title
        GameObject setTitleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
        setTitleObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform setTitleRect = setTitleObj.GetComponent<RectTransform>();
        setTitleRect.anchoredPosition = new Vector2(0f, 130f);
        setTitleRect.sizeDelta = new Vector2(400f, 40f);
        Text setTitleText = setTitleObj.GetComponent<Text>();
        setTitleText.text = "CÀI ĐẶT ÂM THANH";
        setTitleText.font = GameUIFont.Bold;
        setTitleText.fontSize = 22;
        setTitleText.fontStyle = FontStyle.Bold;
        setTitleText.alignment = TextAnchor.MiddleCenter;
        setTitleText.color = new Color(0.96f, 0.83f, 0.36f);

        // BGM Slider Label
        GameObject bgmLabelObj = new GameObject("BGMLabel", typeof(RectTransform), typeof(Text));
        bgmLabelObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform bgmLabelRect = bgmLabelObj.GetComponent<RectTransform>();
        bgmLabelRect.anchoredPosition = new Vector2(-120f, 50f);
        bgmLabelRect.sizeDelta = new Vector2(120f, 30f);
        Text bgmLabel = bgmLabelObj.GetComponent<Text>();
        bgmLabel.text = "Nhạc nền:";
        bgmLabel.font = GameUIFont.Regular;
        bgmLabel.fontSize = 16;
        bgmLabel.alignment = TextAnchor.MiddleLeft;
        bgmLabel.color = Color.white;

        // BGM Slider
        GameObject bgmSliderObj = new GameObject("BGMSlider", typeof(RectTransform), typeof(Slider));
        bgmSliderObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform bgmSliderRect = bgmSliderObj.GetComponent<RectTransform>();
        bgmSliderRect.anchoredPosition = new Vector2(70f, 50f);
        bgmSliderRect.sizeDelta = new Vector2(220f, 25f);
        Slider bgmSlider = bgmSliderObj.GetComponent<Slider>();

        GameObject sliderBg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        sliderBg.transform.SetParent(bgmSliderObj.transform, false);
        sliderBg.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 8f);
        sliderBg.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f);

        GameObject sliderHandleSlide = new GameObject("Handle Slide Area", typeof(RectTransform));
        sliderHandleSlide.transform.SetParent(bgmSliderObj.transform, false);
        sliderHandleSlide.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        sliderHandleSlide.GetComponent<RectTransform>().anchorMax = Vector2.one;
        sliderHandleSlide.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        sliderHandleSlide.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        GameObject sliderHandle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        sliderHandle.transform.SetParent(sliderHandleSlide.transform, false);
        sliderHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(18f, 18f);
        sliderHandle.GetComponent<Image>().color = new Color(0.96f, 0.83f, 0.36f);

        bgmSlider.fillRect = sliderBg.GetComponent<RectTransform>();
        bgmSlider.handleRect = sliderHandle.GetComponent<RectTransform>();
        bgmSlider.targetGraphic = sliderHandle.GetComponent<Image>();
        bgmSlider.minValue = 0f;
        bgmSlider.maxValue = 1f;

        // SFX Slider Label
        GameObject sfxLabelObj = new GameObject("SFXLabel", typeof(RectTransform), typeof(Text));
        sfxLabelObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform sfxLabelRect = sfxLabelObj.GetComponent<RectTransform>();
        sfxLabelRect.anchoredPosition = new Vector2(-120f, -10f);
        sfxLabelRect.sizeDelta = new Vector2(120f, 30f);
        Text sfxLabel = sfxLabelObj.GetComponent<Text>();
        sfxLabel.text = "Hiệu ứng:";
        sfxLabel.font = GameUIFont.Regular;
        sfxLabel.fontSize = 16;
        sfxLabel.alignment = TextAnchor.MiddleLeft;
        sfxLabel.color = Color.white;

        // SFX Slider
        GameObject sfxSliderObj = new GameObject("SFXSlider", typeof(RectTransform), typeof(Slider));
        sfxSliderObj.transform.SetParent(settingsPanel.transform, false);
        RectTransform sfxSliderRect = sfxSliderObj.GetComponent<RectTransform>();
        sfxSliderRect.anchoredPosition = new Vector2(70f, -10f);
        sfxSliderRect.sizeDelta = new Vector2(220f, 25f);
        Slider sfxSlider = sfxSliderObj.GetComponent<Slider>();

        GameObject sfxSliderBg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        sfxSliderBg.transform.SetParent(sfxSliderObj.transform, false);
        sfxSliderBg.GetComponent<RectTransform>().sizeDelta = new Vector2(220f, 8f);
        sfxSliderBg.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f);

        GameObject sfxSliderHandleSlide = new GameObject("Handle Slide Area", typeof(RectTransform));
        sfxSliderHandleSlide.transform.SetParent(sfxSliderObj.transform, false);
        sfxSliderHandleSlide.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        sfxSliderHandleSlide.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        GameObject sfxSliderHandle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        sfxSliderHandle.transform.SetParent(sfxSliderHandleSlide.transform, false);
        sfxSliderHandle.GetComponent<RectTransform>().sizeDelta = new Vector2(18f, 18f);
        sfxSliderHandle.GetComponent<Image>().color = new Color(0.96f, 0.83f, 0.36f);

        sfxSlider.fillRect = sfxSliderBg.GetComponent<RectTransform>();
        sfxSlider.handleRect = sfxSliderHandle.GetComponent<RectTransform>();
        sfxSlider.targetGraphic = sfxSliderHandle.GetComponent<Image>();
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // Close Settings Button
        GameObject closeSetBtnObj = CreateButton("CloseSettingsButton", settingsPanel.transform, "LƯU & QUAY LẠI", new Vector2(200f, 40f), new Vector2(0f, -95f));
        Button closeSetBtn = closeSetBtnObj.GetComponent<Button>();

        // Credits Panel
        GameObject creditsPanel = new GameObject("CreditsPanel", typeof(RectTransform), typeof(Image));
        creditsPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform credRect = creditsPanel.GetComponent<RectTransform>();
        credRect.sizeDelta = new Vector2(500f, 350f);
        creditsPanel.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.98f);
        Outline credOutline = creditsPanel.AddComponent<Outline>();
        credOutline.effectColor = new Color(0.96f, 0.83f, 0.36f, 0.6f);
        credOutline.effectDistance = new Vector2(2f, -2f);

        // Credits Title
        GameObject credTitleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
        credTitleObj.transform.SetParent(creditsPanel.transform, false);
        RectTransform credTitleRect = credTitleObj.GetComponent<RectTransform>();
        credTitleRect.anchoredPosition = new Vector2(0f, 130f);
        credTitleRect.sizeDelta = new Vector2(400f, 40f);
        Text credTitleText = credTitleObj.GetComponent<Text>();
        credTitleText.text = "HƯỚNG DẪN / CREDITS";
        credTitleText.font = GameUIFont.Bold;
        credTitleText.fontSize = 20;
        credTitleText.fontStyle = FontStyle.Bold;
        credTitleText.alignment = TextAnchor.MiddleCenter;
        credTitleText.color = new Color(0.96f, 0.83f, 0.36f);

        // Credits Content
        GameObject credTextObj = new GameObject("Content", typeof(RectTransform), typeof(Text));
        credTextObj.transform.SetParent(creditsPanel.transform, false);
        RectTransform credTextRect = credTextObj.GetComponent<RectTransform>();
        credTextRect.anchoredPosition = new Vector2(0f, 10f);
        credTextRect.sizeDelta = new Vector2(420f, 160f);
        Text credContent = credTextObj.GetComponent<Text>();
        credContent.text = "Ký ức Sài Gòn là trò chơi phiêu lưu giải đố nhẹ nhàng đưa bạn tìm lại vẻ đẹp lịch sử đô thị.\n\n" +
                            "Cách chơi:\n" +
                            "- Di chuyển bằng phím WASD hoặc phím mũi tên.\n" +
                            "- Nhấn E gần điểm sáng để tương tác / giải đố.\n" +
                            "- Khôi phục màu sắc và ký ức cho toàn thành phố!";
        credContent.font = GameUIFont.Regular;
        credContent.fontSize = 14;
        credContent.alignment = TextAnchor.MiddleCenter;
        credContent.color = new Color(0.85f, 0.87f, 0.9f);
        credContent.horizontalOverflow = HorizontalWrapMode.Wrap;

        // Close Credits Button
        GameObject closeCredBtnObj = CreateButton("CloseCreditsButton", creditsPanel.transform, "QUAY LẠI", new Vector2(160f, 40f), new Vector2(0f, -95f));
        Button closeCredBtn = closeCredBtnObj.GetComponent<Button>();

        // Create Main Buttons
        GameObject startBtnObj = CreateButton("StartButton", btnGroup.transform, "BẮT ĐẦU HÀNH TRÌNH", new Vector2(260f, 42f), Vector2.zero);
        GameObject setBtnObj = CreateButton("SettingsButton", btnGroup.transform, "CÀI ĐẶT", new Vector2(260f, 42f), Vector2.zero);
        GameObject credBtnObj = CreateButton("CreditsButton", btnGroup.transform, "HƯỚNG DẪN / CREDITS", new Vector2(260f, 42f), Vector2.zero);
        GameObject quitBtnObj = CreateButton("QuitButton", btnGroup.transform, "THOÁT", new Vector2(260f, 42f), Vector2.zero);

        // 5. Attach Controller Script
        GameObject controllerObj = new GameObject("MainMenuController", typeof(MainMenuController));
        MainMenuController ctrl = controllerObj.GetComponent<MainMenuController>();
        ctrl.mainPanel = mainPanel;
        ctrl.settingsPanel = settingsPanel;
        ctrl.creditsPanel = creditsPanel;
        ctrl.bgmVolumeSlider = bgmSlider;
        ctrl.sfxVolumeSlider = sfxSlider;

        // Wire buttons
        startBtnObj.GetComponent<Button>().onClick.AddListener(ctrl.StartGame);
        setBtnObj.GetComponent<Button>().onClick.AddListener(ctrl.OpenSettings);
        credBtnObj.GetComponent<Button>().onClick.AddListener(ctrl.OpenCredits);
        quitBtnObj.GetComponent<Button>().onClick.AddListener(ctrl.QuitGame);
        closeSetBtn.onClick.AddListener(ctrl.CloseSettings);
        closeCredBtn.onClick.AddListener(ctrl.CloseCredits);

        // Save Scene
        EditorSceneManager.SaveScene(newScene, MainMenuPath);
        Debug.Log("Created Main Menu Scene at: " + MainMenuPath);
    }

    [MenuItem("Ky Uc Sai Gon/Publish/Create Loading Scene")]
    public static void CreateLoadingScene()
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.012f, 0.015f, 0.022f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
        }

        GameObject canvasObj = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Background Panel
        GameObject mainPanel = new GameObject("MainPanel", typeof(RectTransform), typeof(Image));
        mainPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        mainPanel.GetComponent<Image>().color = new Color(0.012f, 0.015f, 0.022f);

        // Center Container
        GameObject container = new GameObject("Container", typeof(RectTransform));
        container.transform.SetParent(mainPanel.transform, false);
        container.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -50f);

        // Title/Logo
        GameObject logoObj = new GameObject("LogoText", typeof(RectTransform), typeof(Text));
        logoObj.transform.SetParent(mainPanel.transform, false);
        RectTransform logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchoredPosition = new Vector2(0f, 130f);
        logoRect.sizeDelta = new Vector2(500f, 50f);
        Text logoText = logoObj.GetComponent<Text>();
        logoText.text = "ĐANG TẢI KÝ ỨC...";
        logoText.font = GameUIFont.Bold;
        logoText.fontSize = 30;
        logoText.fontStyle = FontStyle.Bold;
        logoText.alignment = TextAnchor.MiddleCenter;
        logoText.color = new Color(0.96f, 0.83f, 0.36f, 0.85f);

        // Progress Slider
        GameObject sliderObj = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(container.transform, false);
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(0f, 20f);
        sliderRect.sizeDelta = new Vector2(400f, 15f);
        Slider slider = sliderObj.GetComponent<Slider>();

        GameObject sliderBg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        sliderBg.transform.SetParent(sliderObj.transform, false);
        sliderBg.GetComponent<RectTransform>().sizeDelta = new Vector2(400f, 12f);
        sliderBg.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.20f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        fillArea.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        fillArea.GetComponent<RectTransform>().anchorMax = Vector2.one;
        fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(2f, 2f);
        fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-2f, -2f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        fill.GetComponent<Image>().color = new Color(0.96f, 0.83f, 0.36f);

        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Progress Text
        GameObject progressTextObj = new GameObject("ProgressText", typeof(RectTransform), typeof(Text));
        progressTextObj.transform.SetParent(container.transform, false);
        RectTransform prgRect = progressTextObj.GetComponent<RectTransform>();
        prgRect.anchoredPosition = new Vector2(0f, -20f);
        prgRect.sizeDelta = new Vector2(400f, 30f);
        Text progressText = progressTextObj.GetComponent<Text>();
        progressText.text = "0%";
        progressText.font = GameUIFont.Regular;
        progressText.fontSize = 18;
        progressText.fontStyle = FontStyle.Bold;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.color = new Color(0.85f, 0.87f, 0.9f);

        // Tip text box (matches screenshot)
        GameObject tipContainer = new GameObject("TipContainer", typeof(RectTransform), typeof(Image));
        tipContainer.transform.SetParent(container.transform, false);
        RectTransform tipContRect = tipContainer.GetComponent<RectTransform>();
        tipContRect.anchoredPosition = new Vector2(0f, -90f);
        tipContRect.sizeDelta = new Vector2(440f, 75f);
        tipContainer.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.07f, 0.90f);
        Outline tipOutline = tipContainer.AddComponent<Outline>();
        tipOutline.effectColor = new Color(0.96f, 0.83f, 0.36f, 0.35f);
        tipOutline.effectDistance = new Vector2(1f, -1f);

        GameObject tipObj = new GameObject("TipText", typeof(RectTransform), typeof(Text));
        tipObj.transform.SetParent(tipContainer.transform, false);
        RectTransform tipRect = tipObj.GetComponent<RectTransform>();
        tipRect.anchorMin = Vector2.zero;
        tipRect.anchorMax = Vector2.one;
        tipRect.offsetMin = new Vector2(15f, 5f);
        tipRect.offsetMax = new Vector2(-15f, -5f);
        Text tipText = tipObj.GetComponent<Text>();
        tipText.text = "Mẹo: Hãy trò chuyện với NPC để mở khóa manh mối.";
        tipText.font = GameUIFont.Regular;
        tipText.fontSize = 13;
        tipText.alignment = TextAnchor.MiddleCenter;
        tipText.color = new Color(0.8f, 0.82f, 0.85f);
        tipText.horizontalOverflow = HorizontalWrapMode.Wrap;

        // 5. Attach Controller Script
        GameObject controllerObj = new GameObject("LoadingScreenController", typeof(LoadingScreenController));
        LoadingScreenController ctrl = controllerObj.GetComponent<LoadingScreenController>();
        ctrl.progressBar = slider;
        ctrl.progressText = progressText;
        ctrl.tipText = tipText;

        EditorSceneManager.SaveScene(newScene, LoadingPath);
        Debug.Log("Created Loading Scene at: " + LoadingPath);
    }

    [MenuItem("Ky Uc Sai Gon/Publish/Add Scenes to Build Settings")]
    public static void AddScenesToBuildSettings()
    {
        // Gather all existing scene paths in correct order
        string[] standardScenes = {
            MainMenuPath,
            LoadingPath,
            "Assets/Scenes/Scene_00_BusHub.unity",
            "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity",
            "Assets/Scenes/Scene_02_BenThanh.unity",
            "Assets/Scenes/Scene_03_DinhDocLap.unity",
            "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
            "Assets/Scenes/Scene_05_Bitexco.unity",
            "Assets/Scenes/Scene_06_BachDang.unity",
            "Assets/Scenes/Scene_07_Ending.unity"
        };

        var buildScenes = new EditorBuildSettingsScene[standardScenes.Length];
        for (int i = 0; i < standardScenes.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(standardScenes[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("Added all scenes to Editor Build Settings.");
    }

    private static GameObject CreateButton(string name, Transform parent, string text, Vector2 size, Vector2 position)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        btnObj.GetComponent<Image>().color = new Color(0.045f, 0.055f, 0.075f, 0.95f);
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.96f, 0.83f, 0.36f, 0.6f);
        outline.effectDistance = new Vector2(1f, -1f);

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = textObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        Text btnText = textObj.GetComponent<Text>();
        btnText.text = text;
        btnText.font = GameUIFont.Bold;
        btnText.fontSize = 13;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = new Color(0.96f, 0.83f, 0.36f);

        return btnObj;
    }
}
#endif
