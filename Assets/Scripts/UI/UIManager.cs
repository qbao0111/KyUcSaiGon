using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

    public bool externalInputBlocked;
    public bool IsBlockingPlayerInput => externalInputBlocked || (puzzlePanel != null && puzzlePanel.activeSelf);

    private PuzzleInteractable activePuzzle;
    private readonly int[] stepperValues = new int[3];
    private readonly Button[] stepperValueButtons = new Button[3];
    private int selectedStepperIndex;
    private float nextStepperKeyboardTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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

        interactionPromptText.gameObject.SetActive(visible);
        interactionPromptText.text = string.IsNullOrEmpty(prompt) ? "Press E to interact" : prompt;
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
    }

    public void RefreshProgressText()
    {
        if (memoryProgressText == null)
        {
            return;
        }

        int count = GameProgressManager.Instance != null ? GameProgressManager.Instance.memoryFragmentsCollected : 0;
        memoryProgressText.text = "Memory progress: " + count + "/6";
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
        puzzleTitleText.text = puzzle.puzzleTitle;
        puzzleDescriptionText.text = puzzle.puzzleDescription;
        puzzleInput.text = string.Empty;
        puzzleInput.placeholder.GetComponent<Text>().text = puzzle.inputHint;
        puzzleInput.interactable = !puzzle.useThreeValueStepper;
        puzzleFeedbackText.text = string.Empty;

        ClearQuickChoices();

        if (puzzle.useThreeValueStepper)
        {
            BuildThreeValueStepper(puzzle);
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (puzzleInput.interactable)
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

        bool solved = activePuzzle.TrySolve(puzzleInput.text);
        puzzleFeedbackText.text = solved ? activePuzzle.correctFeedback : activePuzzle.wrongFeedback;
        PrototypeLogger.Info("Puzzle submit: " + activePuzzle.puzzleTitle + " | Input: " + puzzleInput.text + " | Solved: " + solved);

        if (solved)
        {
            Invoke(nameof(HidePuzzle), 0.8f);
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

        activePuzzle = null;
        UnlockCursorIfNoPanel();
    }

    public void SetExternalInputBlocked(bool blocked)
    {
        externalInputBlocked = blocked;
        if (blocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
        if (quickChoiceButtonPrefab == null || quickChoiceRoot == null)
        {
            return;
        }

        for (int index = 0; index < stepperValues.Length; index++)
        {
            int capturedIndex = index;
            stepperValues[index] = 0;

            Button decrease = Instantiate(quickChoiceButtonPrefab, quickChoiceRoot);
            decrease.gameObject.SetActive(true);
            SetButtonText(decrease, "<");
            TintStepperButton(decrease, index, false);
            decrease.onClick.AddListener(() => AdjustStepperValue(capturedIndex, -1));

            Button increase = Instantiate(quickChoiceButtonPrefab, quickChoiceRoot);
            increase.gameObject.SetActive(true);
            stepperValueButtons[index] = increase;
            TintStepperButton(increase, index, true);
            increase.onClick.AddListener(() => AdjustStepperValue(capturedIndex, 1));
        }

        selectedStepperIndex = 0;
        RefreshStepperInput(puzzle);
    }

    private void AdjustStepperValue(int index, int amount)
    {
        stepperValues[index] = (stepperValues[index] + amount + 10) % 10;
        RefreshStepperInput(activePuzzle);
    }

    private void RefreshStepperInput(PuzzleInteractable puzzle)
    {
        if (puzzle == null)
        {
            return;
        }

        puzzleInput.text = stepperValues[0] + "-" + stepperValues[1] + "-" + stepperValues[2];
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

        if (GameInput.SubmitPressed)
        {
            SubmitPuzzle();
            return;
        }

        if (activePuzzle != null && activePuzzle.useThreeValueStepper)
        {
            string digit = GameInput.PressedPuzzleToken();
            if (!string.IsNullOrEmpty(digit) && int.TryParse(digit, out int digitValue))
            {
                stepperValues[selectedStepperIndex] = Mathf.Clamp(digitValue, 0, 9);
                selectedStepperIndex = (selectedStepperIndex + 1) % stepperValues.Length;
                RefreshStepperInput(activePuzzle);
                return;
            }

            Vector2 move = GameInput.Move;
            if (Time.unscaledTime < nextStepperKeyboardTime)
            {
                return;
            }

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
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
