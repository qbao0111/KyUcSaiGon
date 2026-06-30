using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Legacy HUD")]
    public bool showCrosshair;

    [Header("HUD")]
    public Text interactionPromptText;
    public Text memoryProgressText;
    public Text objectiveText;

    [Header("Dialogue")]
    public GameObject dialogueBox;
    public Text dialogueText;

    [Header("Puzzle")]
    public GameObject puzzlePanel;
    public Text puzzleTitleText;
    public Text puzzleDescriptionText;
    public InputField puzzleInput;
    public Transform quickChoiceRoot;
    public Button quickChoiceButtonPrefab;
    public Text puzzleFeedbackText;
    public Button submitPuzzleButton;
    public Button closePuzzleButton;
    public SpeakerMixerPuzzleUI speakerMixerPrefab;
    public BellSequencePuzzleUI bellSequencePrefab;

    [Header("Cathedral Bell Puzzle Art (Scene 04 only)")]
    public Sprite cathedralPuzzleBackground;
    public Sprite cathedralBellBackground;
    public Sprite cathedralBellIcon;
    public Sprite cathedralBellPull;
    public Sprite cathedralBellPullBackground;

    public bool externalInputBlocked;
    public bool IsBlockingPlayerInput => externalInputBlocked || (puzzlePanel != null && puzzlePanel.activeSelf);

    private PuzzleInteractable activePuzzle;
    private readonly int[] stepperValues = new int[3];
    private readonly Button[] stepperValueButtons = new Button[3];
    private readonly List<MixerColumnUi> mixerColumns = new List<MixerColumnUi>();
    private int selectedStepperIndex;
    private float nextStepperKeyboardTime;
    private GameObject mixerRuntimeRoot;
    private RectTransform mixerPanelRect;
    private CanvasGroup mixerCanvasGroup;
    private Image mixerPanelImage;
    private TextMeshProUGUI mixerWarningText;
    private Coroutine mixerOpenRoutine;
    private Coroutine mixerFeedbackRoutine;
    private bool isMixerUiActive;
    private SpeakerMixerPuzzleUI activeSpeakerMixer;
    private BellSequencePuzzleUI activeBellSequence;
    private RectTransform hudStatusPanel;
    private RectTransform hudAccentBar;
    private string currentObjectiveText;
    private RectTransform hudProgressPanel;
    private RectTransform hudObjectivePanel;
    private Image hudProgressBarFill;
    private CanvasGroup hudProgressCanvasGroup;
    private CanvasGroup hudObjectiveCanvasGroup;
    private CanvasGroup hudInteractionCanvasGroup;
    private Coroutine hudProgressAnim;
    private Coroutine hudObjectiveAnim;
    private Coroutine hudInteractionAnim;
    private int lastDisplayedMemoryCount = -1;

    private class MixerColumnUi
    {
        public RectTransform root;
        public RectTransform knob;
        public TextMeshProUGUI valueText;
        public Image glow;
        public Image barFill;
    }

    private void Awake()
    {
        Instance = this;
        SetLegacyCrosshairVisible(showCrosshair);
    }

    private void Start()
    {
        SetLegacyCrosshairVisible(showCrosshair);
        NormalizeHudLayout();
        BindPuzzleButtons();
        ShowInteractionPrompt(false, string.Empty);
        HideDialogue();
        HidePuzzle();
        RefreshProgressText();
    }

    private void Update()
    {
        HandlePuzzleKeyboardInput();

        if (GameInput.CancelPressed)
        {
            HideDialogue();
            HidePuzzle();
        }
    }

    public void ShowInteractionPrompt(bool visible, string prompt)
    {
        if (interactionPromptText == null)
        {
            return;
        }

        interactionPromptText.text = string.IsNullOrEmpty(prompt) ? "Press E to interact" : prompt;

        if (hudInteractionCanvasGroup != null)
        {
            if (visible)
            {
                hudInteractionCanvasGroup.gameObject.SetActive(true);
                if (hudInteractionAnim != null) StopCoroutine(hudInteractionAnim);
                hudInteractionAnim = StartCoroutine(AnimateFade(hudInteractionCanvasGroup, 1f, 0.25f));
            }
            else
            {
                if (hudInteractionAnim != null) StopCoroutine(hudInteractionAnim);
                hudInteractionAnim = StartCoroutine(AnimateFadeAndDeactivate(hudInteractionCanvasGroup, 0.15f));
            }
        }
        else
        {
            interactionPromptText.gameObject.SetActive(visible);
        }
    }

    public void ShowDialogue(string message)
    {
        if (dialogueBox == null || dialogueText == null)
        {
            return;
        }

        dialogueBox.SetActive(true);
        dialogueText.text = message;
        CancelInvoke(nameof(HideDialogue));
        Invoke(nameof(HideDialogue), 4f);
        PrototypeLogger.Info("Dialogue: " + message.Replace("\n", " "));
    }

    public void HideDialogue()
    {
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        UnlockCursorIfNoPanel();
    }

    public void SetObjective(string objective)
    {
        if (objectiveText != null)
        {
            objectiveText.text = objective;
        }

        if (!string.IsNullOrWhiteSpace(objective) && objective != currentObjectiveText)
        {
            currentObjectiveText = objective;
            AudioManager.EnsureInstance().PlaySfx("SFX_ObjectiveUpdated", 0.75f);
            AnimateObjectiveUpdate();
        }
    }

    public void ExtendDialogueDuration(float seconds)
    {
        if (dialogueBox == null || !dialogueBox.activeSelf)
        {
            return;
        }

        CancelInvoke(nameof(HideDialogue));
        Invoke(nameof(HideDialogue), Mathf.Max(0.1f, seconds));
    }

    public void RefreshProgressText()
    {
        if (memoryProgressText == null)
        {
            return;
        }

        int count = GameProgressManager.Instance != null ? GameProgressManager.Instance.memoryFragmentsCollected : 0;
        memoryProgressText.text = "Ký ức: " + count + "/" + GameProgressManager.RequiredMemoryFragments;
        UpdateProgressBar();

        if (count != lastDisplayedMemoryCount && lastDisplayedMemoryCount >= 0)
        {
            AnimateProgressUpdate();
        }
        lastDisplayedMemoryCount = count;
    }

    public void ShowPuzzle(PuzzleInteractable puzzle)
    {
        activePuzzle = puzzle;
        PrototypeLogger.Info("Open puzzle: " + puzzle.puzzleTitle + " | Correct answer: " + puzzle.correctAnswer);

        if (puzzlePanel == null)
        {
            return;
        }

        puzzlePanel.SetActive(true);
        ClearMixerPanel();
        ClearBellSequencePanel();
        bool useBellSequence = IsBellSequencePuzzle(puzzle);
        SetLegacyPuzzleControlsVisible(!puzzle.useThreeValueStepper && !useBellSequence);

        if (puzzleTitleText != null)
        {
            puzzleTitleText.text = puzzle.puzzleTitle;
        }

        if (puzzleDescriptionText != null)
        {
            puzzleDescriptionText.text = puzzle.puzzleDescription;
        }

        if (puzzleInput != null)
        {
            puzzleInput.text = string.Empty;
            puzzleInput.interactable = !puzzle.useThreeValueStepper && !useBellSequence;

            Text placeholderText = puzzleInput.placeholder != null ? puzzleInput.placeholder.GetComponent<Text>() : null;
            if (placeholderText != null)
            {
                placeholderText.text = puzzle.inputHint;
            }
        }

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = string.Empty;
        }

        ClearQuickChoices();

        if (puzzle.useThreeValueStepper)
        {
            EnsureSpeakerMixerAnswer(puzzle);
            BuildThreeValueStepper(puzzle);
        }
        else if (useBellSequence)
        {
            BuildBellSequencePanel(puzzle);
        }
        else if (quickChoiceButtonPrefab != null && quickChoiceRoot != null && puzzle.quickChoices != null)
        {
            foreach (string choice in puzzle.quickChoices)
            {
                Button button = Instantiate(quickChoiceButtonPrefab, quickChoiceRoot);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = choice;
                button.onClick.AddListener(() =>
                {
                    AudioManager.EnsureInstance().PlaySfx("SFX_PuzzleButton", 0.85f);
                    if (string.IsNullOrWhiteSpace(puzzleInput.text))
                    {
                        puzzleInput.text = choice;
                    }
                    else
                    {
                        puzzleInput.text += "-" + choice;
                    }
                });
            }
        }

        CursorLockManager.UnlockForUI();
        if (puzzleInput != null && puzzleInput.interactable && puzzleInput.gameObject.activeInHierarchy)
        {
            puzzleInput.Select();
            puzzleInput.ActivateInputField();
        }
    }

    public void SubmitPuzzle()
    {
        if (activePuzzle == null)
        {
            return;
        }

        string submittedText = puzzleInput != null ? puzzleInput.text : string.Empty;
        bool solved = activePuzzle.TrySolve(submittedText);

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.text = solved ? activePuzzle.correctFeedback : activePuzzle.wrongFeedback;
        }

        if (activeBellSequence != null)
        {
            activeBellSequence.ShowResult(solved, solved ? activePuzzle.correctFeedback : activePuzzle.wrongFeedback);
        }

        PrototypeLogger.Info("Puzzle submit: " + activePuzzle.puzzleTitle + " | Input: " + submittedText + " | Solved: " + solved);

        if (solved)
        {
            if (isMixerUiActive)
            {
                StartMixerFeedback(CorrectMixerAndClose());
            }
            else
            {
                Invoke(nameof(HidePuzzle), 0.8f);
            }
        }
        else if (isMixerUiActive)
        {
            if (mixerWarningText != null)
            {
                mixerWarningText.text = activePuzzle.wrongFeedback;
                mixerWarningText.color = new Color(1f, 0.34f, 0.28f, 1f);
            }

            StartMixerFeedback(ShakeMixerPanel());
        }
    }

    public void HidePuzzle()
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(false);
        }

        if (puzzleInput != null)
        {
            puzzleInput.interactable = true;
        }

        ClearMixerPanel();
        ClearBellSequencePanel();
        SetLegacyPuzzleControlsVisible(true);
        activePuzzle = null;
        UnlockCursorIfNoPanel();
    }

    public void SetExternalInputBlocked(bool blocked)
    {
        externalInputBlocked = blocked;
        if (blocked)
        {
            CursorLockManager.UnlockForUI();
        }
        else
        {
            UnlockCursorIfNoPanel();
        }
    }

    private void ClearQuickChoices()
    {
        if (quickChoiceRoot == null)
        {
            return;
        }

        for (int i = quickChoiceRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(quickChoiceRoot.GetChild(i).gameObject);
        }
    }

    private void BindPuzzleButtons()
    {
        if (puzzlePanel == null)
        {
            return;
        }

        if (submitPuzzleButton == null)
        {
            Transform submitTransform = puzzlePanel.transform.Find("SubmitPuzzleButton");
            if (submitTransform != null)
            {
                submitPuzzleButton = submitTransform.GetComponent<Button>();
            }
        }

        if (closePuzzleButton == null)
        {
            Transform closeTransform = puzzlePanel.transform.Find("ClosePuzzleButton");
            if (closeTransform != null)
            {
                closePuzzleButton = closeTransform.GetComponent<Button>();
            }
        }

        if (submitPuzzleButton != null)
        {
            submitPuzzleButton.onClick.RemoveAllListeners();
            submitPuzzleButton.onClick.AddListener(SubmitPuzzle);
        }

        if (closePuzzleButton != null)
        {
            closePuzzleButton.onClick.RemoveAllListeners();
            closePuzzleButton.onClick.AddListener(HidePuzzle);
        }
    }

    private void BuildThreeValueStepper(PuzzleInteractable puzzle)
    {
        BuildSpeakerMixerPanel(puzzle);

        for (int index = 0; index < stepperValues.Length; index++)
        {
            stepperValues[index] = 0;
            stepperValueButtons[index] = null;
        }

        selectedStepperIndex = 0;
        nextStepperKeyboardTime = Time.unscaledTime + 0.3f;
        RefreshStepperInput(puzzle);
    }

    private void AdjustStepperValue(int index, int amount)
    {
        stepperValues[index] = (stepperValues[index] + amount + 10) % 10;
        AudioManager.EnsureInstance().PlaySfx("SFX_PuzzleButton", 0.85f);
        RefreshStepperInput(activePuzzle);
    }

    private void RefreshStepperInput(PuzzleInteractable puzzle)
    {
        if (puzzle == null)
        {
            return;
        }

        if (puzzleInput != null)
        {
            puzzleInput.text = stepperValues[0] + "-" + stepperValues[1] + "-" + stepperValues[2];
        }

        RefreshMixerColumns(puzzle);

        for (int index = 0; index < stepperValueButtons.Length; index++)
        {
            if (stepperValueButtons[index] == null)
            {
                continue;
            }

            string label = puzzle.stepperLabels != null && index < puzzle.stepperLabels.Length
                ? puzzle.stepperLabels[index]
                : "Value " + (index + 1);
            string selector = index == selectedStepperIndex ? "* " : string.Empty;
            SetButtonText(stepperValueButtons[index], selector + label + "\n" + stepperValues[index] + " >");
        }
    }

    private void HandlePuzzleKeyboardInput()
    {
        if (puzzlePanel == null || !puzzlePanel.activeSelf || puzzleInput == null)
        {
            return;
        }

        if (activeBellSequence != null)
        {
            if (GameInput.SubmitPressed)
            {
                activeBellSequence.RequestSubmit();
                return;
            }

            string bellToken = GameInput.PressedPuzzleToken();
            if (!string.IsNullOrEmpty(bellToken) && int.TryParse(bellToken, out int bellNumber) && bellNumber >= 1 && bellNumber <= 6)
            {
                activeBellSequence.SelectBellByIndex(bellNumber - 1);
                return;
            }

            if (GameInput.BackspacePressed)
            {
                activeBellSequence.UndoLast();
            }

            return;
        }

        if (GameInput.SubmitPressed)
        {
            SubmitPuzzle();
            return;
        }

        if (activePuzzle != null && activePuzzle.useThreeValueStepper)
        {
            if (Time.unscaledTime < nextStepperKeyboardTime)
            {
                return;
            }

            string digit = GameInput.PressedPuzzleToken();
            if (!string.IsNullOrEmpty(digit) && int.TryParse(digit, out int digitValue))
            {
                int changedIndex = selectedStepperIndex;
                stepperValues[selectedStepperIndex] = Mathf.Clamp(digitValue, 0, 9);
                selectedStepperIndex = (selectedStepperIndex + 1) % stepperValues.Length;
                AudioManager.EnsureInstance().PlaySfx("SFX_PuzzleButton", 0.85f);
                RefreshStepperInput(activePuzzle);
                AnimateMixerValueChange(changedIndex);
                return;
            }

            Vector2 move = GameInput.Move;
            if (move.x < -0.5f)
            {
                selectedStepperIndex = (selectedStepperIndex + stepperValues.Length - 1) % stepperValues.Length;
                nextStepperKeyboardTime = Time.unscaledTime + 0.18f;
                RefreshStepperInput(activePuzzle);
                return;
            }

            if (move.x > 0.5f)
            {
                selectedStepperIndex = (selectedStepperIndex + 1) % stepperValues.Length;
                nextStepperKeyboardTime = Time.unscaledTime + 0.18f;
                RefreshStepperInput(activePuzzle);
                return;
            }

            if (move.y > 0.5f)
            {
                nextStepperKeyboardTime = Time.unscaledTime + 0.14f;
                AdjustStepperValue(selectedStepperIndex, 1);
                return;
            }

            if (move.y < -0.5f)
            {
                nextStepperKeyboardTime = Time.unscaledTime + 0.14f;
                AdjustStepperValue(selectedStepperIndex, -1);
                return;
            }
        }

        if (GameInput.BackspacePressed && !string.IsNullOrEmpty(puzzleInput.text))
        {
            int splitIndex = puzzleInput.text.LastIndexOf('-');
            puzzleInput.text = splitIndex >= 0 ? puzzleInput.text.Substring(0, splitIndex) : string.Empty;
            puzzleInput.ActivateInputField();
            return;
        }

        // Number/letter typing is handled by the InputField itself.
        // Quick-choice buttons still append tokens when clicked.
    }

    private void AppendPuzzleToken(string token)
    {
        if (string.IsNullOrWhiteSpace(puzzleInput.text))
        {
            puzzleInput.text = token;
        }
        else
        {
            puzzleInput.text += "-" + token;
        }
    }

    private void SetButtonText(Button button, string text)
    {
        Text label = button != null ? button.GetComponentInChildren<Text>() : null;
        if (label != null)
        {
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = Mathf.Max(label.fontSize, 18);
        }
    }

    private void BuildSpeakerMixerPanel(PuzzleInteractable puzzle)
    {
        if (puzzlePanel == null)
        {
            return;
        }

        isMixerUiActive = true;
        mixerColumns.Clear();

        RectTransform panelRect = puzzlePanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        Image overlayImage = puzzlePanel.GetComponent<Image>();
        if (overlayImage == null)
        {
            overlayImage = puzzlePanel.AddComponent<Image>();
        }

        overlayImage.color = new Color(0.01f, 0.014f, 0.025f, 0.76f);

        CanvasGroup parentCanvasGroup = puzzlePanel.GetComponent<CanvasGroup>();
        if (parentCanvasGroup == null)
        {
            parentCanvasGroup = puzzlePanel.AddComponent<CanvasGroup>();
        }

        parentCanvasGroup.alpha = 0f;
        parentCanvasGroup.blocksRaycasts = true;
        parentCanvasGroup.interactable = true;

        SpeakerMixerPuzzleUI prefab = speakerMixerPrefab != null
            ? speakerMixerPrefab
            : Resources.Load<SpeakerMixerPuzzleUI>("UI/PF_SpeakerMixerPuzzleUI");

        if (prefab != null)
        {
            activeSpeakerMixer = Instantiate(prefab, puzzlePanel.transform);
            parentCanvasGroup.alpha = 1f;
            parentCanvasGroup.blocksRaycasts = true;
            parentCanvasGroup.interactable = true;

            RectTransform activeRect = activeSpeakerMixer.GetComponent<RectTransform>();
            if (activeRect != null)
            {
                activeRect.anchorMin = Vector2.zero;
                activeRect.anchorMax = Vector2.one;
                activeRect.offsetMin = Vector2.zero;
                activeRect.offsetMax = Vector2.zero;
            }

            string[] prefabLabels = puzzle.stepperLabels != null && puzzle.stepperLabels.Length >= 3
                ? puzzle.stepperLabels
                : new[] { "Bass", "Mid", "Treble" };
            activeSpeakerMixer.Bind(prefabLabels, AdjustStepperValue, SubmitPuzzle, HidePuzzle);
            mixerPanelRect = activeSpeakerMixer.panelRoot;
            mixerCanvasGroup = activeSpeakerMixer.canvasGroup;
            mixerPanelImage = activeSpeakerMixer.panelImage;
            mixerWarningText = activeSpeakerMixer.warningText;

            if (mixerCanvasGroup == null || mixerPanelRect == null)
            {
                PrototypeLogger.Info("Speaker mixer prefab is missing required references. Falling back to runtime mixer UI.");
                Destroy(activeSpeakerMixer.gameObject);
                activeSpeakerMixer = null;
                parentCanvasGroup.alpha = 0f;
            }
            else
            {
                if (mixerOpenRoutine != null)
                {
                    StopCoroutine(mixerOpenRoutine);
                }

                mixerOpenRoutine = StartCoroutine(AnimateMixerOpen());
                return;
            }
        }

        mixerCanvasGroup = parentCanvasGroup;

        mixerCanvasGroup.alpha = 0f;
        mixerCanvasGroup.blocksRaycasts = true;
        mixerCanvasGroup.interactable = true;

        mixerRuntimeRoot = new GameObject("RuntimeSpeakerMixerRoot", typeof(RectTransform));
        mixerRuntimeRoot.transform.SetParent(puzzlePanel.transform, false);

        mixerPanelRect = CreateRect("MixerPanel", mixerRuntimeRoot.transform, new Vector2(760f, 500f), Vector2.zero);
        mixerPanelImage = AddImage(mixerPanelRect.gameObject, new Color(0.025f, 0.035f, 0.055f, 0.94f));
        mixerPanelImage.sprite = CreateRoundedSprite(new Color(1f, 1f, 1f, 1f), 38);
        mixerPanelImage.type = Image.Type.Sliced;

        Outline outline = mixerPanelRect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.67f, 0.18f, 0.78f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = mixerPanelRect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -7f);

        CreateEqualizerBackdrop(mixerPanelRect);

        TextMeshProUGUI title = CreateText(
            "Title",
            mixerPanelRect,
            "Bộ điều chỉnh âm thanh",
            new Vector2(620f, 58f),
            new Vector2(0f, 205f),
            36,
            new Color(1f, 0.86f, 0.56f));
        title.fontStyle = FontStyles.Bold;

        string[] labels = puzzle.stepperLabels != null && puzzle.stepperLabels.Length >= 3
            ? puzzle.stepperLabels
            : new[] { "Bass", "Mid", "Treble" };

        Color[] colors =
        {
            new Color(0.9f, 0.22f, 0.18f),
            new Color(0.24f, 0.86f, 0.48f),
            new Color(1f, 0.68f, 0.18f)
        };

        float[] xPositions = { -220f, 0f, 220f };
        for (int i = 0; i < 3; i++)
        {
            MixerColumnUi column = CreateMixerColumn(i, labels[i], colors[i], xPositions[i]);
            mixerColumns.Add(column);
        }

        mixerWarningText = CreateText("WarningText", mixerPanelRect, string.Empty, new Vector2(610f, 30f), new Vector2(0f, -160f), 18, new Color(1f, 0.4f, 0.35f));
        mixerWarningText.fontStyle = FontStyles.Bold;

        Button submitButton = CreateMixerButton(
            "SubmitButton",
            mixerPanelRect,
            "♫  XÁC NHẬN",
            new Vector2(245f, 55f),
            new Vector2(-95f, -210f),
            new Color(1f, 0.73f, 0.1f),
            new Color(0.15f, 0.09f, 0.02f));
        submitButton.onClick.AddListener(SubmitPuzzle);

        Button closeButton = CreateMixerButton(
            "CloseButton",
            mixerPanelRect,
            "ĐÓNG",
            new Vector2(155f, 48f),
            new Vector2(190f, -210f),
            new Color(0.37f, 0.37f, 0.38f),
            Color.white);
        closeButton.onClick.AddListener(HidePuzzle);

        Button topCloseButton = CreateMixerButton(
            "TopCloseButton",
            mixerPanelRect,
            "×",
            new Vector2(48f, 48f),
            new Vector2(350f, 205f),
            new Color(0.28f, 0.29f, 0.31f, 0.92f),
            Color.white);
        topCloseButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 42;
        topCloseButton.onClick.AddListener(HidePuzzle);

        if (mixerOpenRoutine != null)
        {
            StopCoroutine(mixerOpenRoutine);
        }

        mixerOpenRoutine = StartCoroutine(AnimateMixerOpen());
    }

    private MixerColumnUi CreateMixerColumn(int index, string label, Color color, float xPosition)
    {
        MixerColumnUi column = new MixerColumnUi();
        column.root = CreateRect("MixerColumn_" + label, mixerPanelRect, new Vector2(140f, 250f), new Vector2(xPosition, 20f));

        Image columnImage = AddImage(column.root.gameObject, Color.Lerp(new Color(0.03f, 0.04f, 0.06f, 0.92f), color, 0.12f));
        columnImage.sprite = CreateRoundedSprite(Color.white, 24);
        columnImage.type = Image.Type.Sliced;

        Outline outline = column.root.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(color.r, color.g, color.b, 0.58f);
        outline.effectDistance = new Vector2(1f, -1f);

        column.glow = AddImage(CreateRect("ColumnGlow", column.root, new Vector2(138f, 248f), Vector2.zero).gameObject, new Color(color.r, color.g, color.b, 0.03f));
        column.glow.sprite = CreateRoundedSprite(Color.white, 24);
        column.glow.type = Image.Type.Sliced;

        TextMeshProUGUI labelText = CreateText("Label", column.root, label, new Vector2(124f, 38f), new Vector2(0f, 103f), 24, color);
        labelText.fontStyle = FontStyles.Bold;

        Button plusButton = CreateMixerButton("PlusButton", column.root, "+", new Vector2(46f, 46f), new Vector2(0f, 58f), Color.Lerp(color, Color.white, 0.12f), Color.white);
        plusButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 34;
        plusButton.onClick.AddListener(() => AdjustStepperValue(index, 1));

        RectTransform barRoot = CreateRect("SliderBar", column.root, new Vector2(22f, 116f), new Vector2(0f, -15f));
        Image barBack = AddImage(barRoot.gameObject, new Color(0.01f, 0.014f, 0.022f, 0.82f));
        barBack.sprite = CreateRoundedSprite(Color.white, 12);
        barBack.type = Image.Type.Sliced;

        column.barFill = AddImage(CreateRect("SliderFill", barRoot, new Vector2(12f, 18f), new Vector2(0f, -52f)).gameObject, new Color(color.r, color.g, color.b, 0.72f));
        column.barFill.rectTransform.pivot = new Vector2(0.5f, 0f);
        column.barFill.sprite = CreateRoundedSprite(Color.white, 7);
        column.barFill.type = Image.Type.Sliced;

        column.knob = CreateRect("SliderKnob", barRoot, new Vector2(46f, 24f), new Vector2(0f, -46f));
        Image knobImage = AddImage(column.knob.gameObject, Color.Lerp(color, Color.white, 0.18f));
        knobImage.sprite = CreateRoundedSprite(Color.white, 14);
        knobImage.type = Image.Type.Sliced;
        Outline knobOutline = column.knob.gameObject.AddComponent<Outline>();
        knobOutline.effectColor = new Color(1f, 1f, 1f, 0.42f);
        knobOutline.effectDistance = new Vector2(1f, -1f);

        column.valueText = CreateText("ValueText", column.root, "0", new Vector2(100f, 54f), new Vector2(0f, -103f), 36, color);
        column.valueText.fontStyle = FontStyles.Bold;

        Button minusButton = CreateMixerButton("MinusButton", column.root, "-", new Vector2(46f, 46f), new Vector2(0f, -163f), Color.Lerp(color, Color.black, 0.15f), Color.white);
        minusButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 34;
        minusButton.onClick.AddListener(() => AdjustStepperValue(index, -1));

        return column;
    }

    private void CreateEqualizerBackdrop(RectTransform parent)
    {
        Color barColor = new Color(1f, 0.35f, 0.18f, 0.18f);
        for (int i = 0; i < 22; i++)
        {
            float height = 12f + Mathf.PingPong(i * 9f, 44f);
            float x = -330f + i * 32f;
            RectTransform bar = CreateRect("EqualizerBar_" + i, parent, new Vector2(9f, height), new Vector2(x, -174f + height * 0.5f));
            AddImage(bar.gameObject, barColor);
        }
    }

    private void RefreshMixerColumns(PuzzleInteractable puzzle)
    {
        if (!isMixerUiActive || mixerColumns.Count == 0)
        {
            if (activeSpeakerMixer != null)
            {
                activeSpeakerMixer.Refresh(stepperValues, selectedStepperIndex);
            }

            return;
        }

        for (int i = 0; i < mixerColumns.Count && i < stepperValues.Length; i++)
        {
            MixerColumnUi column = mixerColumns[i];
            float normalized = stepperValues[i] / 9f;
            float knobY = Mathf.Lerp(-46f, 46f, normalized);
            column.valueText.text = stepperValues[i].ToString();
            column.knob.anchoredPosition = new Vector2(0f, knobY);

            if (column.barFill != null)
            {
                float fillHeight = Mathf.Lerp(14f, 104f, normalized);
                column.barFill.rectTransform.sizeDelta = new Vector2(12f, fillHeight);
                column.barFill.rectTransform.anchoredPosition = new Vector2(0f, -52f);
            }

            column.root.localScale = i == selectedStepperIndex ? Vector3.one * 1.035f : Vector3.one;
        }
    }

    private void AnimateMixerValueChange(int index)
    {
        if (!isMixerUiActive || index < 0 || index >= mixerColumns.Count)
        {
            if (activeSpeakerMixer != null)
            {
                activeSpeakerMixer.PopColumn(index);
            }

            return;
        }

        StartCoroutine(PopMixerColumn(mixerColumns[index]));
    }

    private IEnumerator AnimateMixerOpen()
    {
        if (activeSpeakerMixer != null)
        {
            yield return activeSpeakerMixer.AnimateOpen();
            yield break;
        }

        if (mixerPanelRect == null || mixerCanvasGroup == null)
        {
            yield break;
        }

        float duration = 0.22f;
        float elapsed = 0f;
        mixerPanelRect.localScale = Vector3.one * 0.9f;
        mixerCanvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            mixerCanvasGroup.alpha = eased;
            mixerPanelRect.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, eased);
            yield return null;
        }

        mixerCanvasGroup.alpha = 1f;
        mixerPanelRect.localScale = Vector3.one;
    }

    private IEnumerator PopMixerColumn(MixerColumnUi column)
    {
        if (column == null || column.valueText == null)
        {
            yield break;
        }

        Color originalGlow = column.glow != null ? column.glow.color : Color.clear;
        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            column.valueText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.22f, t);
            if (column.glow != null)
            {
                column.glow.color = new Color(originalGlow.r, originalGlow.g, originalGlow.b, Mathf.Lerp(originalGlow.a, 0.22f, t));
            }
            yield return null;
        }

        column.valueText.rectTransform.localScale = Vector3.one;
        if (column.glow != null)
        {
            column.glow.color = originalGlow;
        }
    }

    private IEnumerator ShakeMixerPanel()
    {
        if (activeSpeakerMixer != null)
        {
            yield return activeSpeakerMixer.AnimateWrong(activePuzzle != null ? activePuzzle.wrongFeedback : string.Empty);
            yield break;
        }

        if (mixerPanelRect == null)
        {
            yield break;
        }

        Vector2 original = mixerPanelRect.anchoredPosition;
        float duration = 0.28f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float strength = Mathf.Lerp(14f, 0f, elapsed / duration);
            mixerPanelRect.anchoredPosition = original + new Vector2(Mathf.Sin(elapsed * 90f) * strength, 0f);
            yield return null;
        }

        mixerPanelRect.anchoredPosition = original;
    }

    private IEnumerator CorrectMixerAndClose()
    {
        if (activeSpeakerMixer != null)
        {
            yield return activeSpeakerMixer.AnimateCorrect(activePuzzle != null ? activePuzzle.correctFeedback : string.Empty);
            HidePuzzle();
            yield break;
        }

        if (mixerWarningText != null && activePuzzle != null)
        {
            mixerWarningText.text = activePuzzle.correctFeedback;
            mixerWarningText.color = new Color(0.48f, 1f, 0.36f, 1f);
        }

        if (mixerPanelImage != null)
        {
            Color original = mixerPanelImage.color;
            mixerPanelImage.color = new Color(0.14f, 0.17f, 0.08f, 0.96f);
            yield return new WaitForSecondsRealtime(0.75f);
            mixerPanelImage.color = original;
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.75f);
        }

        HidePuzzle();
    }

    private void StartMixerFeedback(IEnumerator routine)
    {
        if (mixerFeedbackRoutine != null)
        {
            StopCoroutine(mixerFeedbackRoutine);
        }

        mixerFeedbackRoutine = StartCoroutine(routine);
    }

    private void ClearMixerPanel()
    {
        if (mixerOpenRoutine != null)
        {
            StopCoroutine(mixerOpenRoutine);
            mixerOpenRoutine = null;
        }

        if (mixerFeedbackRoutine != null)
        {
            StopCoroutine(mixerFeedbackRoutine);
            mixerFeedbackRoutine = null;
        }

        if (mixerRuntimeRoot != null)
        {
            Destroy(mixerRuntimeRoot);
        }

        if (activeSpeakerMixer != null)
        {
            Destroy(activeSpeakerMixer.gameObject);
        }

        mixerRuntimeRoot = null;
        mixerPanelRect = null;
        mixerCanvasGroup = null;
        mixerPanelImage = null;
        mixerWarningText = null;
        activeSpeakerMixer = null;
        mixerColumns.Clear();
        isMixerUiActive = false;
    }

    private void SetLegacyPuzzleControlsVisible(bool visible)
    {
        if (puzzleTitleText != null)
        {
            puzzleTitleText.gameObject.SetActive(visible);
        }

        if (puzzleDescriptionText != null)
        {
            puzzleDescriptionText.gameObject.SetActive(visible);
        }

        if (puzzleInput != null)
        {
            puzzleInput.gameObject.SetActive(visible);
        }

        if (puzzleFeedbackText != null)
        {
            puzzleFeedbackText.gameObject.SetActive(visible);
        }

        if (quickChoiceRoot != null)
        {
            quickChoiceRoot.gameObject.SetActive(visible);
        }

        if (submitPuzzleButton != null)
        {
            submitPuzzleButton.gameObject.SetActive(visible);
        }

        if (closePuzzleButton != null)
        {
            closePuzzleButton.gameObject.SetActive(visible);
        }
    }

    private bool IsBellSequencePuzzle(PuzzleInteractable puzzle)
    {
        return puzzle != null
            && SceneManager.GetActiveScene().name == SceneLoader.NhaThoDucBa
            && string.Equals(puzzle.correctAnswer, "La-Sol-Re-Mi-Si-Do", System.StringComparison.OrdinalIgnoreCase);
    }

    private void BuildBellSequencePanel(PuzzleInteractable puzzle)
    {
        if (puzzlePanel == null)
        {
            return;
        }

        puzzlePanel.transform.SetAsLastSibling();
        CanvasScaler canvasScaler = GetComponentInParent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform puzzlePanelRect = puzzlePanel.GetComponent<RectTransform>();
        if (puzzlePanelRect != null)
        {
            puzzlePanelRect.anchorMin = Vector2.zero;
            puzzlePanelRect.anchorMax = Vector2.one;
            puzzlePanelRect.offsetMin = Vector2.zero;
            puzzlePanelRect.offsetMax = Vector2.zero;
        }

        BellSequencePuzzleUI prefab = bellSequencePrefab != null
            ? bellSequencePrefab
            : Resources.Load<BellSequencePuzzleUI>("UI/PF_BellSequencePuzzleUI");

        if (prefab != null)
        {
            activeBellSequence = Instantiate(prefab, puzzlePanel.transform);
            RectTransform activeRect = activeBellSequence.GetComponent<RectTransform>();
            if (activeRect != null)
            {
                activeRect.anchorMin = Vector2.zero;
                activeRect.anchorMax = Vector2.one;
                activeRect.offsetMin = Vector2.zero;
                activeRect.offsetMax = Vector2.zero;
            }
            activeBellSequence.Bind(
                puzzle,
                puzzleInput,
                SubmitPuzzle,
                HidePuzzle,
                cathedralPuzzleBackground,
                cathedralBellBackground,
                cathedralBellIcon,
                cathedralBellPull,
                cathedralBellPullBackground);
        }
        else
        {
            GameObject root = new GameObject("BellSequenceRuntimeUI", typeof(RectTransform), typeof(BellSequencePuzzleUI));
            root.transform.SetParent(puzzlePanel.transform, false);
            root.transform.SetAsLastSibling();
            activeBellSequence = root.GetComponent<BellSequencePuzzleUI>();
            activeBellSequence.Bind(
                puzzle,
                puzzleInput,
                SubmitPuzzle,
                HidePuzzle,
                cathedralPuzzleBackground,
                cathedralBellBackground,
                cathedralBellIcon,
                cathedralBellPull,
                cathedralBellPullBackground);
        }
    }

    private void ClearBellSequencePanel()
    {
        if (activeBellSequence != null)
        {
            Destroy(activeBellSequence.gameObject);
            activeBellSequence = null;
        }

        if (puzzlePanel == null)
        {
            return;
        }

        Transform staleRoot = puzzlePanel.transform.Find("BellSequenceRuntimeUI");
        if (staleRoot != null)
        {
            Destroy(staleRoot.gameObject);
        }
    }

    private void EnsureSpeakerMixerAnswer(PuzzleInteractable puzzle)
    {
        if (puzzle == null || !puzzle.useThreeValueStepper)
        {
            return;
        }

        bool looksLikeNguyenHueSpeaker =
            (puzzle.puzzleTitle != null && puzzle.puzzleTitle.ToLowerInvariant().Contains("âm thanh")) ||
            (puzzle.inputHint != null && puzzle.inputHint.ToLowerInvariant().Contains("bass"));

        if (looksLikeNguyenHueSpeaker && puzzle.correctAnswer != "1-6-8")
        {
            PrototypeLogger.Info("Speaker mixer answer corrected at runtime from '" + puzzle.correctAnswer + "' to '1-6-8'.");
            puzzle.correctAnswer = "1-6-8";
        }
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        return rect;
    }

    private Image AddImage(GameObject target, Color color)
    {
        Image image = target.GetComponent<Image>();
        if (image == null)
        {
            image = target.AddComponent<Image>();
        }

        image.color = color;
        return image;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, string text, Vector2 size, Vector2 anchoredPosition, int fontSize, Color color)
    {
        RectTransform rect = CreateRect(name, parent, size, anchoredPosition);
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

    private Button CreateMixerButton(string name, Transform parent, string label, Vector2 size, Vector2 anchoredPosition, Color normalColor, Color textColor)
    {
        RectTransform rect = CreateRect(name, parent, size, anchoredPosition);
        Image image = AddImage(rect.gameObject, normalColor);
        image.sprite = CreateRoundedSprite(Color.white, 18);
        image.type = Image.Type.Sliced;

        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.14f);
        colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TextMeshProUGUI text = CreateText("Label", rect, label, size, Vector2.zero, 23, textColor);
        text.fontStyle = FontStyles.Bold;

        Shadow shadow = rect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.32f);
        shadow.effectDistance = new Vector2(0f, -3f);

        return button;
    }

    private Sprite CreateRoundedSprite(Color color, int radius)
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside =
                    IsInsideRoundedCorner(x, y, radius, size) &&
                    IsInsideRoundedCorner(size - 1 - x, y, radius, size) &&
                    IsInsideRoundedCorner(x, size - 1 - y, radius, size) &&
                    IsInsideRoundedCorner(size - 1 - x, size - 1 - y, radius, size);
                texture.SetPixel(x, y, inside ? color : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private bool IsInsideRoundedCorner(int x, int y, int radius, int size)
    {
        if (x >= radius || y >= radius)
        {
            return true;
        }

        float dx = radius - x;
        float dy = radius - y;
        return dx * dx + dy * dy <= radius * radius;
    }

    private void TintStepperButton(Button button, int index, bool isValueButton)
    {
        if (button == null)
        {
            return;
        }

        Color color = Color.white;
        if (index == 0)
        {
            color = new Color(0.9f, 0.18f, 0.14f);
        }
        else if (index == 1)
        {
            color = new Color(0.12f, 0.72f, 0.28f);
        }
        else if (index == 2)
        {
            color = new Color(1f, 0.78f, 0.12f);
        }

        ColorBlock colors = button.colors;
        colors.normalColor = isValueButton ? color : Color.Lerp(color, Color.black, 0.18f);
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.35f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
    }

    private void UnlockCursorIfNoPanel()
    {
        if (!IsBlockingPlayerInput)
        {
            CursorLockManager.LockForGameplay();
        }
    }

    private void SetLegacyCrosshairVisible(bool visible)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child != null && child.name == "Crosshair")
            {
                child.gameObject.SetActive(visible);
            }
        }
    }

    private void NormalizeHudLayout()
    {
        CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        EnsureProgressPanel();
        EnsureObjectivePanel();
        StyleInteractionPrompt();
        StyleDialogueBox();

        StartCoroutine(AnimateHudEntrance());
    }

    private bool ShouldUseCinematicHud()
    {
        return true;
    }

    private void EnsureProgressPanel()
    {
        if (memoryProgressText == null) return;
        Transform canvasRoot = memoryProgressText.transform.parent;

        if (hudProgressPanel == null)
        {
            Transform existing = canvasRoot.Find("HUD_ProgressPanel");
            if (existing != null) hudProgressPanel = existing.GetComponent<RectTransform>();
        }

        if (hudProgressPanel == null)
        {
            GameObject panel = new GameObject("HUD_ProgressPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(canvasRoot, false);
            hudProgressPanel = panel.GetComponent<RectTransform>();
            hudProgressCanvasGroup = panel.GetComponent<CanvasGroup>();

            Image bg = panel.GetComponent<Image>();
            bg.sprite = CreateRoundedSprite(Color.white, 16);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.04f, 0.05f, 0.09f, 0.78f);
            bg.raycastTarget = false;

            Shadow shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(0f, -3f);

            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.75f, 0.22f, 0.12f);
            outline.effectDistance = new Vector2(1f, -1f);

            RectTransform accentLine = CreateRect("AccentLine", hudProgressPanel, new Vector2(260f, 2.5f), new Vector2(0f, 32f));
            Image accentImg = AddImage(accentLine.gameObject, new Color(1f, 0.75f, 0.22f, 0.6f));
            accentImg.sprite = CreateRoundedSprite(Color.white, 2);
            accentImg.type = Image.Type.Sliced;

            // Create an Image for the Vietnamese lantern icon loaded from Resources
            GameObject iconGo = new GameObject("ProgressIcon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(hudProgressPanel, false);
            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(40f, 40f); // 40x40 for a clear lantern icon
            iconRect.anchoredPosition = new Vector2(-112f, 8f);

            Image iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true; // Preserve aspect ratio
            UnityEngine.Texture2D lanternTex = Resources.Load<UnityEngine.Texture2D>("UI/I_VietnameseLantern");
            if (lanternTex != null)
            {
                Sprite lanternSprite = Sprite.Create(lanternTex, new Rect(0f, 0f, lanternTex.width, lanternTex.height), new Vector2(0.5f, 0.5f));
                iconImg.sprite = lanternSprite;
                iconImg.color = Color.white;
            }
            else
            {
                // Fallback to gold diamond if texture load fails
                iconImg.color = new Color(1f, 0.78f, 0.28f);
                iconRect.sizeDelta = new Vector2(9f, 9f);
                iconRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            }
            iconImg.raycastTarget = false;
        }
        else
        {
            hudProgressCanvasGroup = hudProgressPanel.GetComponent<CanvasGroup>();
        }

        hudProgressPanel.anchorMin = new Vector2(0f, 1f);
        hudProgressPanel.anchorMax = new Vector2(0f, 1f);
        hudProgressPanel.pivot = new Vector2(0f, 1f);
        hudProgressPanel.anchoredPosition = new Vector2(28f, -28f);
        hudProgressPanel.sizeDelta = new Vector2(290f, 72f);
        hudProgressPanel.SetAsFirstSibling();

        if (hudProgressCanvasGroup != null) hudProgressCanvasGroup.alpha = 0f;

        memoryProgressText.transform.SetParent(hudProgressPanel, false);
        RectTransform pRect = memoryProgressText.GetComponent<RectTransform>();
        pRect.anchorMin = new Vector2(0.5f, 0.5f);
        pRect.anchorMax = new Vector2(0.5f, 0.5f);
        pRect.pivot = new Vector2(0.5f, 0.5f);
        pRect.anchoredPosition = new Vector2(18f, 8f);
        pRect.sizeDelta = new Vector2(220f, 32f);

        memoryProgressText.font = GameUIFont.Bold;
        memoryProgressText.fontSize = 22;
        memoryProgressText.color = new Color(0.98f, 0.95f, 0.86f);
        memoryProgressText.alignment = TextAnchor.MiddleLeft;
        EnsureShadow(memoryProgressText, new Color(0f, 0f, 0f, 0.6f), new Vector2(1f, -1f));

        EnsureProgressBar();
    }

    private void EnsureProgressBar()
    {
        if (hudProgressPanel == null) return;
        Transform barBg = hudProgressPanel.Find("ProgressBarBg");
        if (barBg == null)
        {
            RectTransform bgRect = CreateRect("ProgressBarBg", hudProgressPanel, new Vector2(240f, 5f), new Vector2(5f, -22f));
            Image bgImg = AddImage(bgRect.gameObject, new Color(1f, 1f, 1f, 0.1f));
            bgImg.sprite = CreateRoundedSprite(Color.white, 3);
            bgImg.type = Image.Type.Sliced;
            bgImg.raycastTarget = false;

            RectTransform fillRect = CreateRect("ProgressBarFill", bgRect, new Vector2(0f, 3.5f), Vector2.zero);
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.anchoredPosition = new Vector2(1f, 0f);
            hudProgressBarFill = AddImage(fillRect.gameObject, new Color(1f, 0.75f, 0.22f, 0.85f));
            hudProgressBarFill.sprite = CreateRoundedSprite(Color.white, 3);
            hudProgressBarFill.type = Image.Type.Sliced;
            hudProgressBarFill.raycastTarget = false;
        }
        else
        {
            Transform fill = barBg.Find("ProgressBarFill");
            if (fill != null) hudProgressBarFill = fill.GetComponent<Image>();
        }
        UpdateProgressBar();
    }

    private void UpdateProgressBar()
    {
        if (hudProgressBarFill == null) return;
        int count = GameProgressManager.Instance != null ? GameProgressManager.Instance.memoryFragmentsCollected : 0;
        float ratio = Mathf.Clamp01((float)count / Mathf.Max(1, GameProgressManager.RequiredMemoryFragments));
        hudProgressBarFill.rectTransform.sizeDelta = new Vector2(238f * ratio, 3.5f);
    }

    private void EnsureObjectivePanel()
    {
        if (objectiveText == null) return;
        Transform canvasRoot = objectiveText.transform.parent;
        if (hudProgressPanel != null && hudProgressPanel.parent != null)
            canvasRoot = hudProgressPanel.parent;

        if (hudObjectivePanel == null)
        {
            Transform existing = canvasRoot.Find("HUD_ObjectivePanel");
            if (existing != null) hudObjectivePanel = existing.GetComponent<RectTransform>();
        }

        if (hudObjectivePanel == null)
        {
            GameObject panel = new GameObject("HUD_ObjectivePanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            panel.transform.SetParent(canvasRoot, false);
            hudObjectivePanel = panel.GetComponent<RectTransform>();
            hudObjectiveCanvasGroup = panel.GetComponent<CanvasGroup>();

            Image bg = panel.GetComponent<Image>();
            bg.sprite = CreateRoundedSprite(Color.white, 12);
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.04f, 0.05f, 0.08f, 0.65f);
            bg.raycastTarget = false;

            Shadow shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -2f);

            RectTransform accent = CreateRect("AccentBar", hudObjectivePanel, new Vector2(4f, 36f), Vector2.zero);
            accent.anchorMin = new Vector2(0f, 0.5f);
            accent.anchorMax = new Vector2(0f, 0.5f);
            accent.pivot = new Vector2(0f, 0.5f);
            accent.anchoredPosition = new Vector2(12f, 0f);
            Image accentImg = AddImage(accent.gameObject, new Color(1f, 0.68f, 0.22f, 0.85f));
            accentImg.sprite = CreateRoundedSprite(Color.white, 2);
            accentImg.type = Image.Type.Sliced;
            accentImg.raycastTarget = false;
        }
        else
        {
            hudObjectiveCanvasGroup = hudObjectivePanel.GetComponent<CanvasGroup>();
        }

        hudObjectivePanel.anchorMin = new Vector2(0f, 1f);
        hudObjectivePanel.anchorMax = new Vector2(0f, 1f);
        hudObjectivePanel.pivot = new Vector2(0f, 1f);
        hudObjectivePanel.anchoredPosition = new Vector2(28f, -110f);
        hudObjectivePanel.sizeDelta = new Vector2(500f, 54f);

        if (hudObjectiveCanvasGroup != null) hudObjectiveCanvasGroup.alpha = 0f;

        objectiveText.transform.SetParent(hudObjectivePanel, false);
        RectTransform oRect = objectiveText.GetComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero;
        oRect.anchorMax = Vector2.one;
        oRect.offsetMin = new Vector2(28f, 4f);
        oRect.offsetMax = new Vector2(-12f, -4f);

        objectiveText.font = GameUIFont.Regular;
        objectiveText.fontSize = 18;
        objectiveText.color = new Color(0.92f, 0.90f, 0.84f, 0.95f);
        objectiveText.alignment = TextAnchor.MiddleLeft;
        objectiveText.horizontalOverflow = HorizontalWrapMode.Wrap;
        objectiveText.verticalOverflow = VerticalWrapMode.Truncate;
        EnsureShadow(objectiveText, new Color(0f, 0f, 0f, 0.65f), new Vector2(1f, -1f));
    }

    private void StyleInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        Transform canvasRoot = interactionPromptText.transform.parent;
        if (canvasRoot != null && canvasRoot.name == "HUD_InteractionWrapper") return;

        GameObject wrapper = new GameObject("HUD_InteractionWrapper", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        wrapper.transform.SetParent(canvasRoot, false);
        RectTransform wrapperRect = wrapper.GetComponent<RectTransform>();

        wrapperRect.anchorMin = new Vector2(0.5f, 0.22f);
        wrapperRect.anchorMax = new Vector2(0.5f, 0.22f);
        wrapperRect.pivot = new Vector2(0.5f, 0.5f);
        wrapperRect.sizeDelta = new Vector2(650f, 50f); // Wider layout for floating text
        wrapperRect.anchoredPosition = Vector2.zero;

        Image bg = wrapper.GetComponent<Image>();
        bg.enabled = false; // Hide background badge

        hudInteractionCanvasGroup = wrapper.GetComponent<CanvasGroup>();

        Outline promptOutline = wrapper.AddComponent<Outline>();
        promptOutline.enabled = false; // Hide border outline

        Shadow promptShadow = wrapper.AddComponent<Shadow>();
        promptShadow.enabled = false; // Hide shadow outline

        interactionPromptText.transform.SetParent(wrapper.transform, false);
        RectTransform textRect = interactionPromptText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 4f);
        textRect.offsetMax = new Vector2(-10f, -4f);

        interactionPromptText.fontSize = 24; // Larger text
        interactionPromptText.font = GameUIFont.Bold; // Bold text
        interactionPromptText.alignment = TextAnchor.MiddleCenter;
        interactionPromptText.color = new Color(1f, 0.85f, 0.45f); // Rich warm gold/amber color

        EnsureShadow(interactionPromptText, new Color(0f, 0f, 0f, 0.95f), new Vector2(2f, -2f));
        EnsureOutline(interactionPromptText, new Color(0f, 0f, 0f, 0.95f), new Vector2(1.5f, -1.5f));
    }

    private void StyleDialogueBox()
    {
        if (dialogueBox == null) return;

        RectTransform dialogueRect = dialogueBox.GetComponent<RectTransform>();
        if (dialogueRect != null)
        {
            dialogueRect.anchorMin = new Vector2(0.5f, 0.10f);
            dialogueRect.anchorMax = new Vector2(0.5f, 0.10f);
            dialogueRect.pivot = new Vector2(0.5f, 0.5f);
            dialogueRect.sizeDelta = new Vector2(950f, 120f); // Wider bounding box for text wrapping
            dialogueRect.anchoredPosition = Vector2.zero;
        }

        Image dialogueBg = dialogueBox.GetComponent<Image>();
        if (dialogueBg == null) dialogueBg = dialogueBox.AddComponent<Image>();
        dialogueBg.enabled = false; // Hide background image

        Outline border = dialogueBox.GetComponent<Outline>();
        if (border == null) border = dialogueBox.AddComponent<Outline>();
        border.enabled = false; // Hide border

        Shadow dShadow = dialogueBox.GetComponent<Shadow>();
        if (dShadow == null) dShadow = dialogueBox.AddComponent<Shadow>();
        dShadow.enabled = false; // Hide shadow

        if (dialogueText != null)
        {
            dialogueText.fontSize = 24;
            dialogueText.font = GameUIFont.Bold; // Bold font is highly recommended for subtitles
            dialogueText.color = new Color(0.98f, 0.98f, 0.96f); // Soft white for subtitles
            dialogueText.alignment = TextAnchor.MiddleCenter;

            RectTransform dtRect = dialogueText.GetComponent<RectTransform>();
            if (dtRect != null)
            {
                dtRect.anchorMin = Vector2.zero;
                dtRect.anchorMax = Vector2.one;
                dtRect.offsetMin = new Vector2(10f, 10f);
                dtRect.offsetMax = new Vector2(-10f, -10f);
            }

            EnsureShadow(dialogueText, new Color(0f, 0f, 0f, 0.95f), new Vector2(2f, -2f));
            EnsureOutline(dialogueText, new Color(0f, 0f, 0f, 0.95f), new Vector2(1.5f, -1.5f));
        }
    }

    private IEnumerator AnimateHudEntrance()
    {
        yield return null;

        if (hudProgressPanel != null && hudProgressCanvasGroup != null)
        {
            Vector2 target = hudProgressPanel.anchoredPosition;
            hudProgressPanel.anchoredPosition = target + new Vector2(-140f, 0f);
            hudProgressCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
                hudProgressCanvasGroup.alpha = t;
                hudProgressPanel.anchoredPosition = Vector2.Lerp(target + new Vector2(-140f, 0f), target, t);
                yield return null;
            }
            hudProgressCanvasGroup.alpha = 1f;
            hudProgressPanel.anchoredPosition = target;
        }

        yield return new WaitForSecondsRealtime(0.08f);

        if (hudObjectivePanel != null && hudObjectiveCanvasGroup != null)
        {
            Vector2 target = hudObjectivePanel.anchoredPosition;
            hudObjectivePanel.anchoredPosition = target + new Vector2(-120f, 0f);
            hudObjectiveCanvasGroup.alpha = 0f;
            float elapsed = 0f;
            float duration = 0.45f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
                hudObjectiveCanvasGroup.alpha = t;
                hudObjectivePanel.anchoredPosition = Vector2.Lerp(target + new Vector2(-120f, 0f), target, t);
                yield return null;
            }
            hudObjectiveCanvasGroup.alpha = 1f;
            hudObjectivePanel.anchoredPosition = target;
        }
    }

    private void AnimateProgressUpdate()
    {
        if (hudProgressAnim != null) StopCoroutine(hudProgressAnim);
        hudProgressAnim = StartCoroutine(PulsePanel(hudProgressPanel));
    }

    private void AnimateObjectiveUpdate()
    {
        if (hudObjectiveAnim != null) StopCoroutine(hudObjectiveAnim);
        hudObjectiveAnim = StartCoroutine(FlashPanel(hudObjectivePanel, hudObjectiveCanvasGroup));
    }

    private IEnumerator PulsePanel(RectTransform panel)
    {
        if (panel == null) yield break;
        float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            panel.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.06f, t);
            yield return null;
        }
        panel.localScale = Vector3.one;
    }

    private IEnumerator FlashPanel(RectTransform panel, CanvasGroup group)
    {
        if (panel == null || group == null) yield break;

        Vector2 original = panel.anchoredPosition;
        float duration = 0.35f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float flashT = Mathf.Sin(t * Mathf.PI);
            group.alpha = Mathf.Lerp(1f, 0.55f, flashT);
            panel.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.03f, flashT);
            yield return null;
        }
        group.alpha = 1f;
        panel.localScale = Vector3.one;
    }

    private IEnumerator AnimateFade(CanvasGroup group, float target, float duration)
    {
        if (group == null) yield break;
        float start = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = target;
    }

    private IEnumerator AnimateFadeAndDeactivate(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        float start = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    private static void EnsureShadow(Text text, Color color, Vector2 distance)
    {
        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = color;
        shadow.effectDistance = distance;
    }

    private static void EnsureOutline(Text text, Color color, Vector2 distance)
    {
        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = color;
        outline.effectDistance = distance;
    }
}
