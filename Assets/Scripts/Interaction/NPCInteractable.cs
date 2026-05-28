using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string dialogue = "Ky uc o day van dang ngu quen...";
    public string InteractionPrompt => "Press E to talk";

    public void Interact(Interactor interactor)
    {
        PrototypeLogger.Info("NPC interact: " + name);
        UIManager.Instance?.ShowDialogue(dialogue);
    }
}
