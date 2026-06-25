#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BellSequencePuzzlePrefabBuilder
{
    private const string PrefabPath = "Assets/Resources/UI/PF_BellSequencePuzzleUI.prefab";

    [InitializeOnLoadMethod]
    private static void EnsurePrefabExistsAfterReload()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<BellSequencePuzzleUI>(PrefabPath) != null)
            {
                return;
            }

            EnsurePrefabExists();
        };
    }

    public static void EnsurePrefabExists()
    {
        if (AssetDatabase.LoadAssetAtPath<BellSequencePuzzleUI>(PrefabPath) != null)
        {
            return;
        }

        RebuildPrefab();
    }

    [MenuItem("Ky Uc Sai Gon/Team Setup/Rebuild Bell Sequence Prefab")]
    public static void RebuildPrefab()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");

        // Load reference art assets (if they exist) from the project
        Sprite cathedralPuzzleBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Models/NhaThoDucBa/Puzzle/puzzleBackground.png");
        Sprite cathedralBellBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Models/NhaThoDucBa/Puzzle/BellBackground.png");
        Sprite cathedralBellIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Models/NhaThoDucBa/Puzzle/Bell.png");
        Sprite cathedralBellPull = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Models/NhaThoDucBa/Puzzle/bellPull.png");
        Sprite cathedralBellPullBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Models/NhaThoDucBa/Puzzle/bellPullBackGround.png");

        // Fallback search by name if path is different
        if (cathedralPuzzleBackground == null) cathedralPuzzleBackground = FindSprite("puzzleBackground");
        if (cathedralBellBackground == null) cathedralBellBackground = FindSprite("BellBackground");
        if (cathedralBellIcon == null) cathedralBellIcon = FindSprite("Bell");
        if (cathedralBellPull == null) cathedralBellPull = FindSprite("bellPull");
        if (cathedralBellPullBackground == null) cathedralBellPullBackground = FindSprite("bellPullBackGround");

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject root = new GameObject("PF_BellSequencePuzzleUI", typeof(RectTransform), typeof(BellSequencePuzzleUI));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);
        
        // Add full-screen blocker image
        Image darkOverlay = root.AddComponent<Image>();
        darkOverlay.color = new Color(0.015f, 0.008f, 0.004f, 0.94f);

        RectTransform frame = CreateRect("CathedralBellControlPanel", root.transform, new Vector2(1760f, 880f), Vector2.zero);
        Image frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.sprite = cathedralPuzzleBackground;
        frameImage.color = Color.white;
        frameImage.preserveAspect = true;
        AddOutline(frame.gameObject, new Color(0.37f, 0.18f, 0.06f, 0.95f), new Vector2(5f, -5f));

        CreateText("Title", frame, "HÒA ÂM THÁP CHUÔNG", 40, FontStyle.Bold, new Color(0.23f, 0.13f, 0.07f, 1f), new Vector2(1200f, 75f), new Vector2(0f, 270f), TextAnchor.MiddleCenter, font);

        Button closeButton = CreateButton("CloseButton", frame, "×", new Vector2(45f, 45f), new Vector2(780f, 360f), new Color(0.21f, 0.08f, 0.035f, 0.96f), new Color(1f, 0.90f, 0.68f, 1f), 30, font);

        RectTransform instructionPanel = CreateRect("InstructionPanel", frame, new Vector2(370f, 520f), new Vector2(-510f, -30f));
        Image instructionBackground = instructionPanel.gameObject.AddComponent<Image>();
        instructionBackground.sprite = cathedralBellPullBackground;
        instructionBackground.color = new Color(0.78f, 0.68f, 0.56f, 1f);
        AddOutline(instructionPanel.gameObject, new Color(0.94f, 0.68f, 0.28f, 0.75f), new Vector2(3f, -3f));
        CreateText("InstructionTitle", instructionPanel, "QUY TẮC HÒA ÂM", 22, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(320f, 40f), new Vector2(0f, 185f), TextAnchor.MiddleCenter, font);

        string[] rules =
        {
            "Chuông La phải được rung trước chuông Mi.",
            "Chuông Do phải được rung ngay sau chuông Si [Si - Do].",
            "Chuông Sol phải được rung sau La nhưng trước Re.",
            "Có đúng hai chuông khác được rung xen giữa Sol và Si.",
            "Chuông Mi được rung trước Si, nhưng không ngay trước hoặc ngay sau Sol."
        };
        for (int i = 0; i < rules.Length; i++)
        {
            float ruleY = 100f - i * 72f;
            RectTransform ruleRow = CreateRect("RuleRow_" + (i + 1), instructionPanel, new Vector2(330f, 68f), new Vector2(0f, ruleY));
            Image rowImage = ruleRow.gameObject.AddComponent<Image>();
            rowImage.color = new Color(0.10f, 0.045f, 0.018f, 0.42f);
            AddOutline(ruleRow.gameObject, new Color(0.94f, 0.68f, 0.28f, 0.28f), new Vector2(1f, -1f));

            RectTransform numberPlate = CreateRect("RuleNumber", ruleRow, new Vector2(32f, 30f), new Vector2(-145f, 0f));
            Image plateImage = numberPlate.gameObject.AddComponent<Image>();
            plateImage.color = new Color(0.23f, 0.10f, 0.035f, 0.96f);
            AddOutline(numberPlate.gameObject, new Color(0.94f, 0.68f, 0.28f, 1f), new Vector2(2f, -2f));
            CreateText("Value", numberPlate, (i + 1).ToString(), 14, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(30f, 28f), Vector2.zero, TextAnchor.MiddleCenter, font);

            Text ruleText = CreateText("RuleText", ruleRow, rules[i], 13, FontStyle.Bold, new Color(0.96f, 0.87f, 0.70f, 1f), new Vector2(270f, 60f), new Vector2(20f, 0f), TextAnchor.MiddleLeft, font);
            ruleText.resizeTextForBestFit = true;
            ruleText.resizeTextMinSize = 9;
            ruleText.resizeTextMaxSize = 13;
        }

        RectTransform sequencePanel = CreateRect("PlayerSequencePanel", frame, new Vector2(620f, 520f), new Vector2(0f, -30f));
        Image sequenceBackground = sequencePanel.gameObject.AddComponent<Image>();
        sequenceBackground.sprite = cathedralBellPullBackground;
        sequenceBackground.color = new Color(0.92f, 0.84f, 0.70f, 0.72f);
        AddOutline(sequencePanel.gameObject, new Color(0.94f, 0.68f, 0.28f, 0.60f), new Vector2(3f, -3f));
        CreateText("SequenceTitle", sequencePanel, "CHUỖI CHUÔNG ĐÃ CHỌN", 21, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(550f, 40f), new Vector2(0f, 185f), TextAnchor.MiddleCenter, font);

        RectTransform selectedSlots = CreateRect("SelectedSlots", sequencePanel, new Vector2(550f, 150f), new Vector2(0f, 75f));
        Text[] sequenceSlotLabels = new Text[6];
        Image[] sequenceSlotBells = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            RectTransform slot = CreateRect("SequenceSlot_" + (i + 1), selectedSlots, new Vector2(80f, 118f), new Vector2(-212.5f + i * 85f, 0f));
            Image slotBackground = slot.gameObject.AddComponent<Image>();
            slotBackground.sprite = cathedralBellBackground;
            slotBackground.color = new Color(1f, 1f, 1f, 0.92f);
            slotBackground.preserveAspect = true;
            CreateText("Position", slot, (i + 1).ToString(), 16, FontStyle.Bold, new Color(0.94f, 0.68f, 0.28f, 1f), new Vector2(36f, 30f), new Vector2(0f, 46f), TextAnchor.MiddleCenter, font);
            
            Image selectedBell = CreateImage("SelectedBell", slot, Color.white);
            selectedBell.sprite = cathedralBellIcon;
            selectedBell.rectTransform.sizeDelta = new Vector2(42f, 57f);
            selectedBell.rectTransform.anchoredPosition = new Vector2(0f, 4f);
            selectedBell.preserveAspect = true;
            selectedBell.gameObject.SetActive(false);
            
            sequenceSlotBells[i] = selectedBell;
            sequenceSlotLabels[i] = CreateText("SelectedName", slot, "", 14, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(76f, 26f), new Vector2(0f, -41f), TextAnchor.MiddleCenter, font);
        }

        Text feedbackText = CreateText("Feedback", sequencePanel, string.Empty, 18, FontStyle.Bold, new Color(0.96f, 0.87f, 0.70f, 1f), new Vector2(550f, 40f), new Vector2(0f, -50f), TextAnchor.MiddleCenter, font);

        Button submitButton = CreateButton("ConfirmButton", sequencePanel, "XÁC NHẬN HÒA ÂM", new Vector2(220f, 60f), new Vector2(-125f, -175f), new Color(0.12f, 0.20f, 0.25f, 0.96f), new Color(1f, 0.90f, 0.68f, 1f), 17, font);
        Button resetButton = CreateButton("ResetButton", sequencePanel, "ĐẶT LẠI", new Vector2(220f, 60f), new Vector2(125f, -175f), new Color(0.36f, 0.11f, 0.055f, 0.96f), new Color(1f, 0.90f, 0.68f, 1f), 17, font);

        RectTransform controlsPanel = CreateRect("BellControlsPanel", frame, new Vector2(360f, 520f), new Vector2(510f, -30f));
        Image controlsBackground = controlsPanel.gameObject.AddComponent<Image>();
        controlsBackground.sprite = cathedralBellPullBackground;
        controlsBackground.color = new Color(0.86f, 0.76f, 0.62f, 1f);
        AddOutline(controlsPanel.gameObject, new Color(0.94f, 0.68f, 0.28f, 0.75f), new Vector2(3f, -3f));
        CreateText("ControlsTitle", controlsPanel, "DÂY CHUÔNG", 22, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(320f, 40f), new Vector2(0f, 185f), TextAnchor.MiddleCenter, font);
        CreateText("ControlsSubtitle", controlsPanel, "Âm thấp  →  Âm cao", 14, FontStyle.Normal, new Color(0.96f, 0.87f, 0.70f, 1f), new Vector2(300f, 25f), new Vector2(0f, 155f), TextAnchor.MiddleCenter, font);

        string[] bellNames = { "Sol", "La", "Si", "Do", "Re", "Mi" };
        Button[] ropeButtons = new Button[6];
        Image[] ropeImages = new Image[6];
        Image[] slotHighlights = new Image[6];
        Text[] orderBadges = new Text[6];

        for (int i = 0; i < 6; i++)
        {
            int column = i % 3;
            int row = i / 3;
            Vector2 position = new Vector2(-105f + column * 105f, 40f - row * 195f);

            RectTransform slot = CreateRect("BellControl_" + bellNames[i], controlsPanel, new Vector2(92f, 185f), position);
            Image highlight = slot.gameObject.AddComponent<Image>();
            highlight.color = new Color(0.12f, 0.07f, 0.03f, 0.08f);
            AddOutline(slot.gameObject, new Color(0.94f, 0.68f, 0.28f, 0.35f), new Vector2(2f, -2f));
            slotHighlights[i] = highlight;

            RectTransform badgeRoot = CreateRect("OrderBadge", slot, new Vector2(32f, 30f), new Vector2(30f, 74f));
            Image badgeBackground = badgeRoot.gameObject.AddComponent<Image>();
            badgeBackground.color = new Color(0.20f, 0.095f, 0.035f, 0.96f);
            AddOutline(badgeRoot.gameObject, new Color(0.94f, 0.68f, 0.28f, 1f), new Vector2(2f, -2f));
            orderBadges[i] = CreateText("Value", badgeRoot, "", 16, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(30f, 28f), Vector2.zero, TextAnchor.MiddleCenter, font);
            badgeRoot.gameObject.SetActive(false);

            Image bellFrame = CreateImage("BellBackground", slot, Color.white);
            bellFrame.sprite = cathedralBellBackground;
            bellFrame.rectTransform.sizeDelta = new Vector2(80f, 64f);
            bellFrame.rectTransform.anchoredPosition = new Vector2(0f, 52f);
            bellFrame.preserveAspect = true;

            Image bell = CreateImage("Bell", slot, Color.white);
            bell.sprite = cathedralBellIcon;
            bell.rectTransform.sizeDelta = new Vector2(42f, 55f);
            bell.rectTransform.anchoredPosition = new Vector2(0f, 52f);
            bell.preserveAspect = true;

            CreateText("BellName", slot, bellNames[i], 15, FontStyle.Bold, new Color(1f, 0.90f, 0.68f, 1f), new Vector2(82f, 24f), new Vector2(0f, 10f), TextAnchor.MiddleCenter, font);

            RectTransform ropeFrameRect = CreateRect("RopeButton", slot, new Vector2(80f, 88f), new Vector2(0f, -48f));
            Image ropeFrame = ropeFrameRect.gameObject.AddComponent<Image>();
            ropeFrame.sprite = cathedralBellPullBackground;
            ropeFrame.color = Color.white;
            ropeFrame.preserveAspect = true;

            Button button = ropeFrameRect.gameObject.AddComponent<Button>();
            button.targetGraphic = ropeFrame;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.02f, 0.90f, 1f);
            colors.pressedColor = new Color(0.82f, 0.76f, 0.66f, 1f);
            colors.disabledColor = Color.white;
            colors.fadeDuration = 0.18f;
            button.colors = colors;
            ropeButtons[i] = button;

            Image rope = CreateImage("BellPull", ropeFrameRect, Color.white);
            rope.sprite = cathedralBellPull;
            rope.rectTransform.sizeDelta = new Vector2(18f, 78f);
            rope.rectTransform.anchoredPosition = new Vector2(0f, -1f);
            rope.preserveAspect = true;
            ropeImages[i] = rope;
        }

        // Bind all references on the component
        BellSequencePuzzleUI uiComponent = root.GetComponent<BellSequencePuzzleUI>();
        uiComponent.ropeButtons = ropeButtons;
        uiComponent.ropeImages = ropeImages;
        uiComponent.slotHighlights = slotHighlights;
        uiComponent.orderBadges = orderBadges;
        uiComponent.sequenceSlotLabels = sequenceSlotLabels;
        uiComponent.sequenceSlotBells = sequenceSlotBells;
        uiComponent.feedbackText = feedbackText;
        uiComponent.submitButton = submitButton;
        uiComponent.resetButton = resetButton;
        uiComponent.closeButton = closeButton;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[KyUcSaiGon] Rebuilt scaled Bell Sequence Puzzle UI Prefab: " + PrefabPath);
    }

    private static Sprite FindSprite(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
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

    private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, Color color, Vector2 dimensions, Vector2 position, TextAnchor alignment, Font font)
    {
        RectTransform rect = CreateRect(name, parent, dimensions, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        
        Shadow shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return text;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 position, Color background, Color foreground, int fontSize, Font font)
    {
        RectTransform rect = CreateRect(name, parent, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = background;
        AddOutline(rect.gameObject, new Color(0.94f, 0.68f, 0.28f, 1f), new Vector2(3f, -3f));

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.10f, 1.05f, 0.92f, 1f);
        colors.pressedColor = new Color(0.75f, 0.70f, 0.64f, 1f);
        colors.disabledColor = new Color(0.48f, 0.48f, 0.48f, 0.82f);
        colors.fadeDuration = 0.16f;
        button.colors = colors;

        CreateText("Label", rect, label, fontSize, FontStyle.Bold, foreground, size - new Vector2(18f, 12f), Vector2.zero, TextAnchor.MiddleCenter, font);
        return button;
    }

    private static void AddOutline(GameObject obj, Color color, Vector2 distance)
    {
        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
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
#endif
