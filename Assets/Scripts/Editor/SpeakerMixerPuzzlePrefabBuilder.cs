using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SpeakerMixerPuzzlePrefabBuilder
{
    private const string PrefabPath = "Assets/Resources/UI/PF_SpeakerMixerPuzzleUI.prefab";

    [InitializeOnLoadMethod]
    private static void EnsurePrefabExistsAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SpeakerMixerPuzzleUI>(PrefabPath) != null)
            {
                return;
            }

            EnsurePrefabExists();
        };
    }

    public static void EnsurePrefabExists()
    {
        if (AssetDatabase.LoadAssetAtPath<SpeakerMixerPuzzleUI>(PrefabPath) != null)
        {
            return;
        }

        RebuildPrefab();
    }

    [MenuItem("Ky Uc Sai Gon/Team Setup/Rebuild Speaker Mixer Puzzle Prefab")]
    public static void RebuildPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");

        GameObject root = new GameObject("PF_SpeakerMixerPuzzleUI", typeof(RectTransform), typeof(SpeakerMixerPuzzleUI), typeof(CanvasGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        Image overlay = CreateImage("Overlay", root.transform, new Color(0.01f, 0.014f, 0.025f, 0.76f));
        Stretch(overlay.rectTransform);

        RectTransform panel = CreateRect("MixerPanel", root.transform, new Vector2(760f, 500f), Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.025f, 0.035f, 0.055f, 0.94f);
        AddOutline(panel.gameObject, new Color(1f, 0.67f, 0.18f, 0.78f), new Vector2(2f, -2f));
        AddShadow(panel.gameObject, new Color(0f, 0f, 0f, 0.55f), new Vector2(0f, -7f));

        CreateEqualizerBars(panel);

        TextMeshProUGUI title = CreateText("Title", panel, "Bộ điều chỉnh âm thanh", new Vector2(620f, 58f), new Vector2(0f, 205f), 36, new Color(1f, 0.86f, 0.56f));
        title.fontStyle = FontStyles.Bold;

        Color bassColor = new Color(0.9f, 0.22f, 0.18f);
        Color midColor = new Color(0.24f, 0.86f, 0.48f);
        Color trebleColor = new Color(1f, 0.68f, 0.18f);

        SpeakerMixerPuzzleUI ui = root.GetComponent<SpeakerMixerPuzzleUI>();
        ui.panelRoot = panel;
        ui.canvasGroup = root.GetComponent<CanvasGroup>();
        ui.panelImage = panelImage;
        ui.titleText = title;
        ui.columns = new SpeakerMixerPuzzleUI.MixerColumn[3];
        ui.columns[0] = CreateColumn(panel, "Bass", bassColor, -220f);
        ui.columns[1] = CreateColumn(panel, "Mid", midColor, 0f);
        ui.columns[2] = CreateColumn(panel, "Treble", trebleColor, 220f);

        ui.warningText = CreateText("WarningText", panel, string.Empty, new Vector2(610f, 30f), new Vector2(0f, -160f), 18, new Color(1f, 0.4f, 0.35f));
        ui.warningText.fontStyle = FontStyles.Bold;

        ui.submitButton = CreateButton("SubmitButton", panel, "♫  XÁC NHẬN", new Vector2(245f, 55f), new Vector2(-95f, -210f), new Color(1f, 0.73f, 0.1f), new Color(0.15f, 0.09f, 0.02f), 23);
        ui.closeButton = CreateButton("CloseButton", panel, "ĐÓNG", new Vector2(155f, 48f), new Vector2(190f, -210f), new Color(0.37f, 0.37f, 0.38f), Color.white, 23);
        ui.topCloseButton = CreateButton("TopCloseButton", panel, "×", new Vector2(48f, 48f), new Vector2(350f, 205f), new Color(0.28f, 0.29f, 0.31f, 0.92f), Color.white, 42);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[KyUcSaiGon] Rebuilt speaker mixer puzzle prefab: " + PrefabPath);
    }

    private static SpeakerMixerPuzzleUI.MixerColumn CreateColumn(RectTransform parent, string label, Color color, float xPosition)
    {
        RectTransform root = CreateRect("MixerColumn_" + label, parent, new Vector2(140f, 250f), new Vector2(xPosition, 20f));
        Image columnImage = root.gameObject.AddComponent<Image>();
        columnImage.color = Color.Lerp(new Color(0.03f, 0.04f, 0.06f, 0.92f), color, 0.12f);
        AddOutline(root.gameObject, new Color(color.r, color.g, color.b, 0.58f), new Vector2(1f, -1f));

        Image glow = CreateImage("ColumnGlow", root, new Color(color.r, color.g, color.b, 0.03f));
        glow.rectTransform.sizeDelta = new Vector2(138f, 248f);

        TextMeshProUGUI labelText = CreateText("Label", root, label, new Vector2(124f, 38f), new Vector2(0f, 103f), 24, color);
        labelText.fontStyle = FontStyles.Bold;

        Button plus = CreateButton("PlusButton", root, "+", new Vector2(46f, 46f), new Vector2(0f, 58f), Color.Lerp(color, Color.white, 0.12f), Color.white, 34);

        RectTransform barRoot = CreateRect("SliderBar", root, new Vector2(22f, 116f), new Vector2(0f, -15f));
        Image barBack = barRoot.gameObject.AddComponent<Image>();
        barBack.color = new Color(0.01f, 0.014f, 0.022f, 0.82f);

        RectTransform fillRect = CreateRect("SliderFill", barRoot, new Vector2(12f, 18f), new Vector2(0f, -52f));
        fillRect.pivot = new Vector2(0.5f, 0f);
        Image fill = fillRect.gameObject.AddComponent<Image>();
        fill.color = new Color(color.r, color.g, color.b, 0.72f);

        RectTransform knob = CreateRect("SliderKnob", barRoot, new Vector2(46f, 24f), new Vector2(0f, -46f));
        Image knobImage = knob.gameObject.AddComponent<Image>();
        knobImage.color = Color.Lerp(color, Color.white, 0.18f);
        AddOutline(knob.gameObject, new Color(1f, 1f, 1f, 0.42f), new Vector2(1f, -1f));

        TextMeshProUGUI value = CreateText("ValueText", root, "0", new Vector2(100f, 54f), new Vector2(0f, -103f), 36, color);
        value.fontStyle = FontStyles.Bold;

        Button minus = CreateButton("MinusButton", root, "-", new Vector2(46f, 46f), new Vector2(0f, -163f), Color.Lerp(color, Color.black, 0.15f), Color.white, 34);

        return new SpeakerMixerPuzzleUI.MixerColumn
        {
            label = label,
            root = root,
            labelText = labelText,
            valueText = value,
            knob = knob,
            fillImage = fill,
            glowImage = glow,
            plusButton = plus,
            minusButton = minus,
            accentColor = color,
            forceCenteredSliderLayout = true,
            sliderCenterX = 0f
        };
    }

    private static void CreateEqualizerBars(RectTransform parent)
    {
        Color barColor = new Color(1f, 0.35f, 0.18f, 0.18f);
        for (int i = 0; i < 22; i++)
        {
            float height = 12f + Mathf.PingPong(i * 9f, 44f);
            float x = -330f + i * 32f;
            RectTransform bar = CreateRect("EqualizerBar_" + i, parent, new Vector2(9f, height), new Vector2(x, -174f + height * 0.5f));
            Image image = bar.gameObject.AddComponent<Image>();
            image.color = barColor;
        }
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 position, Color normalColor, Color textColor, int fontSize)
    {
        RectTransform rect = CreateRect(name, parent, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = normalColor;

        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.14f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TextMeshProUGUI text = CreateText("Label", rect, label, size, Vector2.zero, fontSize, textColor);
        text.fontStyle = FontStyles.Bold;
        AddShadow(rect.gameObject, new Color(0f, 0f, 0f, 0.32f), new Vector2(0f, -3f));
        return button;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, Vector2 size, Vector2 position, int fontSize, Color color)
    {
        RectTransform rect = CreateRect(name, parent, size, position);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        label.outlineWidth = 0.08f;
        label.outlineColor = new Color(0f, 0f, 0f, 0.65f);
        return label;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(name, parent, Vector2.zero, Vector2.zero);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void AddOutline(GameObject obj, Color color, Vector2 distance)
    {
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private static void AddShadow(GameObject obj, Color color, Vector2 distance)
    {
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
