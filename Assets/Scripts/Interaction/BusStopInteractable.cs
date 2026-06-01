using UnityEngine;

public class BusStopInteractable : MonoBehaviour, IInteractable
{
    public string targetScene = SceneLoader.BusHub;
    public bool requireCurrentZoneRestored = true;
    public MemoryZoneController currentZone;
    public string interactionPrompt = "Press E to board bus";
    public string InteractionPrompt => interactionPrompt;

    private void Awake()
    {
        if (currentZone != null)
        {
            currentZone.Restored += HandleZoneRestored;
        }
    }

    private void Start()
    {
        if (requireCurrentZoneRestored && currentZone != null)
        {
            gameObject.SetActive(currentZone.IsRestored);
        }
    }

    private void OnDestroy()
    {
        if (currentZone != null)
        {
            currentZone.Restored -= HandleZoneRestored;
        }
    }

    public void Interact(Interactor interactor)
    {
        if (requireCurrentZoneRestored && currentZone != null && !currentZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Hay khoi phuc ky uc o day truoc da.");
            return;
        }

        SceneLoader.Load(targetScene);
    }

    private void HandleZoneRestored()
    {
        gameObject.SetActive(true);
    }
}
