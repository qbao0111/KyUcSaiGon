using UnityEngine;

public class PuzzleInteractable : MonoBehaviour, IInteractable
{
    public string puzzleTitle = "Memory Puzzle";
    [TextArea] public string puzzleDescription = "Enter the correct answer.";
    public string correctAnswer = "123";
    public string inputHint = "Type answer";
    [TextArea] public string wrongFeedback = "Not yet. Look for the hints.";
    [TextArea] public string correctFeedback = "Correct. Memory restored.";
    public string[] quickChoices;
    public bool useThreeValueStepper;
    public string[] stepperLabels = { "Bass", "Mid", "Treble" };
    public MemoryZoneController memoryZone;

    public string InteractionPrompt => memoryZone != null && memoryZone.IsRestored ? "Khu vực đã được khôi phục" : "Nhấn E để giải câu đố";

    public void Interact(Interactor interactor)
    {
        PrototypeLogger.Info("Puzzle interact: " + puzzleTitle);
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Nơi này đã có màu sắc trở lại rồi.");
            return;
        }

        UIManager.Instance?.ShowPuzzle(this);
    }

    public bool TrySolve(string submittedAnswer)
    {
        string left = NormalizeAnswer(submittedAnswer);
        string right = NormalizeAnswer(correctAnswer);
        bool solved = left == right;

        if (solved)
        {
            AudioManager.EnsureInstance().PlaySfx("SFX_PuzzleSolved", 0.95f);
            memoryZone?.RestoreZone();
        }

        return solved;
    }

    private string NormalizeAnswer(string value)
    {
        return (value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace(",", string.Empty);
    }
}
