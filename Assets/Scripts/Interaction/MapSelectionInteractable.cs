using UnityEngine;

public class MapSelectionInteractable : MonoBehaviour, IInteractable
{
    public LocationId targetLocation;
    public string targetScene;
    public string displayName = "Route";
    public bool isEndingRoute;
    public Renderer statusRenderer;

    public string InteractionPrompt => "Press E to select route";

    private void Start()
    {
        RefreshVisualState();
    }

    public void Interact(Interactor interactor)
    {
        GameProgressManager progress = GameProgressManager.Instance;
        PrototypeLogger.Info("Route interact: " + displayName + " -> " + targetScene);

        if (isEndingRoute)
        {
            if (progress != null && progress.endingUnlocked)
            {
                PrototypeLogger.Info("Loading ending route.");
                SceneLoader.Load(SceneLoader.Ending);
            }
            else
            {
                UIManager.Instance?.ShowDialogue("Can du 6 manh ky uc de mo chuyen xe cuoi.");
            }

            return;
        }

        if (targetLocation != LocationId.NguyenHue && progress != null && !progress.IsRestored(LocationId.NguyenHue))
        {
            UIManager.Instance?.ShowDialogue("Hay hoan thanh Nguyen Hue truoc de mo cac tuyen xe khac.");
            return;
        }

        if (progress != null && progress.IsRestored(targetLocation))
        {
            UIManager.Instance?.ShowDialogue(displayName + " da hoan thanh.");
            return;
        }

        SceneLoader.Load(targetScene);
    }

    public void RefreshVisualState()
    {
        if (statusRenderer == null)
        {
            statusRenderer = GetComponentInChildren<Renderer>();
        }

        if (statusRenderer == null || GameProgressManager.Instance == null)
        {
            return;
        }

        bool completed = targetLocation != LocationId.None && GameProgressManager.Instance.IsRestored(targetLocation);
        bool unlocked = isEndingRoute ? GameProgressManager.Instance.endingUnlocked : targetLocation == LocationId.NguyenHue || GameProgressManager.Instance.IsRestored(LocationId.NguyenHue);
        statusRenderer.material.color = completed ? Color.green : unlocked ? Color.yellow : Color.gray;
    }
}
