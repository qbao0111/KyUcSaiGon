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

    public bool IsBlockingPlayerInput => puzzlePanel != null && puzzlePanel.activeSelf;

    private PuzzleInteractable activePuzzle;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
        puzzleFeedbackText.text = string.Empty;

        ClearQuickChoices();

        if (quickChoiceButtonPrefab != null && quickChoiceRoot != null && puzzle.quickChoices != null)
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
        puzzleInput.Select();
        puzzleInput.ActivateInputField();
    }

    public void SubmitPuzzle()
    {
        if (activePuzzle == null)
        {
            return;
        }

        bool solved = activePuzzle.TrySolve(puzzleInput.text);
        puzzleFeedbackText.text = solved ? "Correct. Memory restored." : "Not yet. Look for the hints.";
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

        activePuzzle = null;
        UnlockCursorIfNoPanel();
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

    private void UnlockCursorIfNoPanel()
    {
        if (!IsBlockingPlayerInput)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
