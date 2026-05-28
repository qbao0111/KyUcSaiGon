using UnityEngine;

public class PuzzleInteractable : MonoBehaviour, IInteractable
{
    public string puzzleTitle = "Memory Puzzle";
    [TextArea] public string puzzleDescription = "Enter the correct answer.";
    public string correctAnswer = "123";
    public string inputHint = "Type answer";
    public string[] quickChoices;
    public MemoryZoneController memoryZone;

    public string InteractionPrompt => memoryZone != null && memoryZone.IsRestored ? "Already restored" : "Press E to solve puzzle";

    public void Interact(Interactor interactor)
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Noi nay da co mau sac tro lai roi.");
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
            memoryZone?.RestoreZone();
        }

        return solved;
    }

    private string NormalizeAnswer(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty);
    }
}
