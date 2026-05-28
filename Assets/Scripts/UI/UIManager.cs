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

    public bool IsBlockingPlayerInput => (dialogueBox != null && dialogueBox.activeSelf) || (puzzlePanel != null && puzzlePanel.activeSelf);

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
        if (Input.GetKeyDown(KeyCode.Escape))
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
    }

    public void SubmitPuzzle()
    {
        if (activePuzzle == null)
        {
            return;
        }

        bool solved = activePuzzle.TrySolve(puzzleInput.text);
        puzzleFeedbackText.text = solved ? "Correct. Memory restored." : "Not yet. Look for the hints.";

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

    private void UnlockCursorIfNoPanel()
    {
        if (!IsBlockingPlayerInput)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
