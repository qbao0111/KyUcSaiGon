using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BellSequencePuzzleUI : MonoBehaviour
{
    private static readonly string[] BellNames = { "Sol", "La", "Si", "Do", "Re", "Mi" };

    private readonly List<string> sequence = new List<string>(6);
    private readonly Button[] ropeButtons = new Button[6];
    private readonly Image[] ropeImages = new Image[6];
    private readonly Image[] slotHighlights = new Image[6];
    private readonly Text[] orderBadges = new Text[6];
    private readonly Text[] sequenceSlotLabels = new Text[6];
    private readonly Image[] sequenceSlotBells = new Image[6];

    private PuzzleInteractable puzzle;
    private InputField answerInput;
    private Action submitAction;
    private Action closeAction;
    private Text feedbackText;
    private Button submitButton;
    private Font displayFont;
    private bool ownsDisplayFont;
    private Coroutine feedbackRoutine;

    private Sprite puzzleBackground;
    private Sprite bellBackground;
    private Sprite bellIcon;
    private Sprite bellPull;
    private Sprite bellPullBackground;

    private readonly Color gold = new Color(0.94f, 0.68f, 0.28f, 1f);
    private readonly Color paleGold = new Color(1f, 0.90f, 0.68f, 1f);
    private readonly Color parchmentText = new Color(0.23f, 0.13f, 0.07f, 1f);
    private readonly Color warmText = new Color(0.96f, 0.87f, 0.70f, 1f);
    private readonly Color selectedBlue = new Color(0.12f, 0.63f, 1f, 1f);
    private readonly Color successGreen = new Color(0.35f, 0.92f, 0.58f, 1f);
    private readonly Color errorRed = new Color(0.95f, 0.31f, 0.24f, 1f);

    public void Bind(
        PuzzleInteractable targetPuzzle,
        InputField hiddenAnswerInput,
        Action onSubmit,
        Action onClose,
        Sprite panelBackground,
        Sprite slotBackground,
        Sprite bellSprite,
        Sprite pullSprite,
        Sprite pullBackgroundSprite)
    {
        puzzle = targetPuzzle;
        answerInput = hiddenAnswerInput;
        submitAction = onSubmit;
        closeAction = onClose;
        puzzleBackground = panelBackground;
        bellBackground = slotBackground;
        bellIcon = bellSprite;
        bellPull = pullSprite;
        bellPullBackground = pullBackgroundSprite;
        displayFont = ResolveFont();

        BuildLayout();
        ResetSequence();
    }

    public void SelectBellByIndex(int index)
    {
        if (index < 0 || index >= BellNames.Length || sequence.Count >= 6 || ropeButtons[index] == null || !ropeButtons[index].interactable)
        {
            return;
        }

        sequence.Add(BellNames[index]);
        ropeButtons[index].interactable = false;
        SetSlotSelected(index, true, sequence.Count);
        RefreshAnswer();
        AudioManager.EnsureInstance().PlaySfx("SFX_PuzzleButton", 0.85f);
    }

    public void UndoLast()
    {
        if (sequence.Count == 0)
        {
            return;
        }

        string removed = sequence[sequence.Count - 1];
        sequence.RemoveAt(sequence.Count - 1);
        int bellIndex = Array.IndexOf(BellNames, removed);
        if (bellIndex >= 0)
        {
            ropeButtons[bellIndex].interactable = true;
            SetSlotSelected(bellIndex, false, 0);
        }

        RefreshAnswer();
    }

    public void ResetSequence()
    {
        sequence.Clear();
        for (int i = 0; i < ropeButtons.Length; i++)
        {
            if (ropeButtons[i] != null)
            {
                ropeButtons[i].interactable = true;
                SetSlotSelected(i, false, 0);
            }
        }

        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
            feedbackText.color = warmText;
        }

        SetButtonColor(submitButton, new Color(0.12f, 0.20f, 0.25f, 0.96f));
        RefreshAnswer();
    }

    public void RequestSubmit()
    {
        if (sequence.Count < 6)
        {
            ShowResult(false, "Hãy chọn đủ 6 chuông trước khi xác nhận.");
            return;
        }

        submitAction?.Invoke();
    }

    public void ShowResult(bool solved, string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = solved ? successGreen : errorRed;
        }

        SetButtonColor(submitButton, solved ? new Color(0.12f, 0.43f, 0.25f, 1f) : new Color(0.42f, 0.12f, 0.08f, 1f));
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(FeedbackPulse(solved));
    }

    private void BuildLayout()
    {
        RectTransform root = GetComponent<RectTransform>();
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        AddImage(gameObject, null, new Color(0.015f, 0.008f, 0.004f, 0.94f));

        RectTransform frame = CreateRect("CathedralBellControlPanel", transform, new Vector2(1760f, 880f), Vector2.zero);
        Image frameImage = AddImage(frame.gameObject, puzzleBackground, Color.white);
        frameImage.preserveAspect = true;
        AddOutline(frame.gameObject, new Color(0.37f, 0.18f, 0.06f, 0.95f), new Vector2(5f, -5f));

        CreateText("Title", frame, "HÒA ÂM THÁP CHUÔNG", 44, FontStyle.Bold, parchmentText, new Vector2(1040f, 62f), new Vector2(0f, 350f), TextAnchor.MiddleCenter, true);

        Button closeButton = CreateButton("CloseButton", frame, "×", new Vector2(58f, 58f), new Vector2(720f, 325f), new Color(0.21f, 0.08f, 0.035f, 0.96f), paleGold, 38);
        closeButton.onClick.AddListener(() => closeAction?.Invoke());

        RectTransform instructionPanel = CreateRect("InstructionPanel", frame, new Vector2(430f, 600f), new Vector2(-605f, -35f));
        Image instructionBackground = AddImage(instructionPanel.gameObject, bellPullBackground, new Color(0.78f, 0.68f, 0.56f, 1f));
        instructionBackground.preserveAspect = false;
        AddOutline(instructionPanel.gameObject, new Color(gold.r, gold.g, gold.b, 0.75f), new Vector2(3f, -3f));
        CreateText("InstructionTitle", instructionPanel, "QUY TẮC HÒA ÂM", 27, FontStyle.Bold, paleGold, new Vector2(350f, 48f), new Vector2(0f, 250f), TextAnchor.MiddleCenter, true);
        string[] displayedRules =
        {
            "Chuông La phải được rung trước chuông Mi.",
            "Chuông Do phải được rung ngay sau chuông Si [Si - Do].",
            "Chuông Sol phải được rung sau La nhưng trước Re.",
            "Có đúng hai chuông khác được rung xen giữa Sol và Si.",
            "Chuông Mi được rung trước Si, nhưng không ngay trước hoặc ngay sau Sol."
        };
        for (int i = 0; i < displayedRules.Length; i++)
        {
            float ruleY = 160f - i * 90f;
            RectTransform ruleRow = CreateRect("RuleRow_" + (i + 1), instructionPanel, new Vector2(382f, 80f), new Vector2(0f, ruleY));
            AddImage(ruleRow.gameObject, null, new Color(0.10f, 0.045f, 0.018f, 0.42f));
            AddOutline(ruleRow.gameObject, new Color(gold.r, gold.g, gold.b, 0.28f), new Vector2(1f, -1f));
            RectTransform numberPlate = CreateRect("RuleNumber", ruleRow, new Vector2(38f, 36f), new Vector2(-163f, 0f));
            AddImage(numberPlate.gameObject, null, new Color(0.23f, 0.10f, 0.035f, 0.96f));
            AddOutline(numberPlate.gameObject, gold, new Vector2(2f, -2f));
            CreateText("Value", numberPlate, (i + 1).ToString(), 20, FontStyle.Bold, paleGold, new Vector2(36f, 34f), Vector2.zero, TextAnchor.MiddleCenter, true);
            Text ruleText = CreateText("RuleText", ruleRow, displayedRules[i], 16, FontStyle.Bold, warmText, new Vector2(316f, 70f), new Vector2(24f, 0f), TextAnchor.MiddleLeft, true);
            ruleText.resizeTextForBestFit = true;
            ruleText.resizeTextMinSize = 13;
            ruleText.resizeTextMaxSize = 16;
        }

        RectTransform sequencePanel = CreateRect("PlayerSequencePanel", frame, new Vector2(730f, 600f), new Vector2(5f, -35f));
        Image sequenceBackground = AddImage(sequencePanel.gameObject, bellPullBackground, new Color(0.92f, 0.84f, 0.70f, 0.72f));
        sequenceBackground.preserveAspect = false;
        AddOutline(sequencePanel.gameObject, new Color(gold.r, gold.g, gold.b, 0.60f), new Vector2(3f, -3f));
        CreateText("SequenceTitle", sequencePanel, "CHUỖI CHUÔNG ĐÃ CHỌN", 26, FontStyle.Bold, paleGold, new Vector2(620f, 46f), new Vector2(0f, 242f), TextAnchor.MiddleCenter, true);
        RectTransform selectedSlots = CreateRect("SelectedSlots", sequencePanel, new Vector2(640f, 176f), new Vector2(0f, 112f));
        for (int i = 0; i < BellNames.Length; i++)
        {
            BuildSequenceSlot(selectedSlots, i, new Vector2(-275f + i * 110f, 0f));
        }

        feedbackText = CreateText("Feedback", sequencePanel, string.Empty, 21, FontStyle.Bold, warmText, new Vector2(620f, 56f), new Vector2(0f, -55f), TextAnchor.MiddleCenter, true);

        submitButton = CreateButton("ConfirmButton", sequencePanel, "XÁC NHẬN HÒA ÂM", new Vector2(260f, 70f), new Vector2(-145f, -225f), new Color(0.12f, 0.20f, 0.25f, 0.96f), paleGold, 21);
        submitButton.onClick.AddListener(RequestSubmit);
        Button resetButton = CreateButton("ResetButton", sequencePanel, "ĐẶT LẠI", new Vector2(260f, 70f), new Vector2(145f, -225f), new Color(0.36f, 0.11f, 0.055f, 0.96f), paleGold, 21);
        resetButton.onClick.AddListener(ResetSequence);

        RectTransform controlsPanel = CreateRect("BellControlsPanel", frame, new Vector2(420f, 600f), new Vector2(610f, -35f));
        Image controlsBackground = AddImage(controlsPanel.gameObject, bellPullBackground, new Color(0.86f, 0.76f, 0.62f, 1f));
        controlsBackground.preserveAspect = false;
        AddOutline(controlsPanel.gameObject, new Color(gold.r, gold.g, gold.b, 0.75f), new Vector2(3f, -3f));
        CreateText("ControlsTitle", controlsPanel, "DÂY CHUÔNG", 27, FontStyle.Bold, paleGold, new Vector2(340f, 46f), new Vector2(0f, 248f), TextAnchor.MiddleCenter, true);
        CreateText("ControlsSubtitle", controlsPanel, "Âm thấp  →  Âm cao", 16, FontStyle.Normal, warmText, new Vector2(280f, 30f), new Vector2(0f, 215f), TextAnchor.MiddleCenter, true);
        for (int i = 0; i < BellNames.Length; i++)
        {
            int column = i % 3;
            int row = i / 3;
            BuildBellControl(controlsPanel, i, new Vector2(-125f + column * 125f, 92f - row * 240f));
        }
    }

    private void BuildSequenceSlot(Transform parent, int index, Vector2 position)
    {
        RectTransform slot = CreateRect("SequenceSlot_" + (index + 1), parent, new Vector2(100f, 148f), position);
        Image slotBackground = AddImage(slot.gameObject, bellBackground, new Color(1f, 1f, 1f, 0.92f));
        slotBackground.preserveAspect = true;
        slotBackground.raycastTarget = false;
        CreateText("Position", slot, (index + 1).ToString(), 20, FontStyle.Bold, gold, new Vector2(36f, 30f), new Vector2(0f, 58f), TextAnchor.MiddleCenter, true);
        Image selectedBell = AddImage(CreateRect("SelectedBell", slot, new Vector2(52f, 70f), new Vector2(0f, 5f)).gameObject, bellIcon, Color.white);
        selectedBell.preserveAspect = true;
        selectedBell.raycastTarget = false;
        selectedBell.gameObject.SetActive(false);
        sequenceSlotBells[index] = selectedBell;
        sequenceSlotLabels[index] = CreateText("SelectedName", slot, "", 19, FontStyle.Bold, paleGold, new Vector2(88f, 30f), new Vector2(0f, -52f), TextAnchor.MiddleCenter, true);
    }

    private void BuildBellControl(Transform parent, int index, Vector2 position)
    {
        RectTransform slot = CreateRect("BellControl_" + BellNames[index], parent, new Vector2(110f, 215f), position);
        Image highlight = AddImage(slot.gameObject, null, new Color(0.12f, 0.07f, 0.03f, 0.08f));
        AddOutline(slot.gameObject, new Color(gold.r, gold.g, gold.b, 0.35f), new Vector2(2f, -2f));
        slotHighlights[index] = highlight;

        RectTransform badgeRoot = CreateRect("OrderBadge", slot, new Vector2(38f, 34f), new Vector2(36f, 87f));
        Image badgeBackground = AddImage(badgeRoot.gameObject, null, new Color(0.20f, 0.095f, 0.035f, 0.96f));
        badgeBackground.raycastTarget = false;
        AddOutline(badgeRoot.gameObject, gold, new Vector2(2f, -2f));
        Text badge = CreateText("Value", badgeRoot, "", 21, FontStyle.Bold, paleGold, new Vector2(36f, 32f), Vector2.zero, TextAnchor.MiddleCenter, true);
        orderBadges[index] = badge;
        badgeRoot.gameObject.SetActive(false);

        Image bellFrame = AddImage(CreateRect("BellBackground", slot, new Vector2(96f, 75f), new Vector2(0f, 64f)).gameObject, bellBackground, Color.white);
        bellFrame.preserveAspect = true;
        bellFrame.raycastTarget = false;
        Image bell = AddImage(CreateRect("Bell", slot, new Vector2(50f, 65f), new Vector2(0f, 64f)).gameObject, bellIcon, Color.white);
        bell.preserveAspect = true;
        bell.raycastTarget = false;

        Text nameLabel = CreateText("BellName", slot, BellNames[index], 20, FontStyle.Bold, paleGold, new Vector2(94f, 30f), new Vector2(0f, 14f), TextAnchor.MiddleCenter, true);

        RectTransform ropeFrameRect = CreateRect("RopeButton", slot, new Vector2(98f, 105f), new Vector2(0f, -58f));
        Image ropeFrame = AddImage(ropeFrameRect.gameObject, bellPullBackground, Color.white);
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
        int capturedIndex = index;
        button.onClick.AddListener(() => SelectBellByIndex(capturedIndex));
        ropeButtons[index] = button;

        Image rope = AddImage(CreateRect("BellPull", ropeFrameRect, new Vector2(22f, 95f), new Vector2(0f, -2f)).gameObject, bellPull, Color.white);
        rope.preserveAspect = true;
        rope.raycastTarget = false;
        ropeImages[index] = rope;

        nameLabel.transform.SetAsLastSibling();
    }

    private void SetSlotSelected(int index, bool selected, int order)
    {
        if (slotHighlights[index] != null)
        {
            slotHighlights[index].color = selected ? new Color(0.03f, 0.20f, 0.34f, 0.14f) : new Color(0.12f, 0.07f, 0.03f, 0.08f);
            Outline outline = slotHighlights[index].GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = selected ? selectedBlue : new Color(gold.r, gold.g, gold.b, 0.35f);
                outline.effectDistance = selected ? new Vector2(4f, -4f) : new Vector2(2f, -2f);
            }
        }

        if (orderBadges[index] != null)
        {
            orderBadges[index].transform.parent.gameObject.SetActive(selected);
            orderBadges[index].text = selected ? order.ToString() : string.Empty;
            orderBadges[index].color = selected ? Color.white : paleGold;
        }

        if (ropeImages[index] != null)
        {
            ropeImages[index].color = selected ? new Color(0.62f, 0.82f, 1f, 1f) : Color.white;
        }
    }

    private void RefreshAnswer()
    {
        if (answerInput != null)
        {
            answerInput.text = string.Join("-", sequence);
        }

        if (feedbackText != null && sequence.Count > 0)
        {
            feedbackText.text = string.Empty;
            feedbackText.color = warmText;
        }

        for (int i = 0; i < sequenceSlotLabels.Length; i++)
        {
            bool filled = i < sequence.Count;
            sequenceSlotLabels[i].text = filled ? sequence[i] : string.Empty;
            sequenceSlotBells[i].gameObject.SetActive(filled);
        }

        if (submitButton != null)
        {
            SetButtonColor(submitButton, sequence.Count == 6 ? new Color(0.08f, 0.32f, 0.48f, 1f) : new Color(0.12f, 0.20f, 0.25f, 0.96f));
        }
    }

    private IEnumerator FeedbackPulse(bool solved)
    {
        Vector3 original = feedbackText.rectTransform.localScale;
        for (float t = 0f; t < 0.24f; t += Time.unscaledDeltaTime)
        {
            float strength = solved ? 0.10f : 0.045f;
            feedbackText.rectTransform.localScale = original * (1f + Mathf.Sin(t / 0.24f * Mathf.PI) * strength);
            yield return null;
        }

        feedbackText.rectTransform.localScale = original;
        feedbackRoutine = null;
    }

    private RectTransform CreateRect(string objectName, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private Text CreateText(string objectName, Transform parent, string value, int size, FontStyle style, Color color, Vector2 dimensions, Vector2 position, TextAnchor alignment, bool shadowed)
    {
        RectTransform rect = CreateRect(objectName, parent, dimensions, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = displayFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(12, size - 4);
        text.resizeTextMaxSize = size;
        text.raycastTarget = false;
        if (shadowed)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        return text;
    }

    private Button CreateButton(string objectName, Transform parent, string label, Vector2 size, Vector2 position, Color background, Color foreground, int fontSize)
    {
        RectTransform rect = CreateRect(objectName, parent, size, position);
        Image image = AddImage(rect.gameObject, null, background);
        AddOutline(rect.gameObject, gold, new Vector2(3f, -3f));
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.10f, 1.05f, 0.92f, 1f);
        colors.pressedColor = new Color(0.75f, 0.70f, 0.64f, 1f);
        colors.disabledColor = new Color(0.48f, 0.48f, 0.48f, 0.82f);
        colors.fadeDuration = 0.16f;
        button.colors = colors;
        CreateText("Label", rect, label, fontSize, FontStyle.Bold, foreground, size - new Vector2(18f, 12f), Vector2.zero, TextAnchor.MiddleCenter, true);
        return button;
    }

    private Image AddImage(GameObject target, Sprite sprite, Color color)
    {
        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            image = target.AddComponent<Image>();
        }

        image.sprite = sprite;
        image.color = color;
        return image;
    }

    private void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private void SetButtonColor(Button button, Color color)
    {
        Image image = button != null ? button.targetGraphic as Image : null;
        if (image != null)
        {
            image.color = color;
        }
    }

    private Font ResolveFont()
    {
        string[] installed = Font.GetOSInstalledFontNames();
        string[] candidates = { "Times New Roman", "Georgia", "Arial" };
        foreach (string candidate in candidates)
        {
            if (Array.Exists(installed, name => string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                ownsDisplayFont = true;
                return Font.CreateDynamicFontFromOSFont(candidate, 24);
            }
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void OnDestroy()
    {
        if (displayFont != null && ownsDisplayFont)
        {
            Destroy(displayFont);
        }
    }
}
