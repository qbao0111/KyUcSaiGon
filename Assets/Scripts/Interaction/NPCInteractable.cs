using UnityEngine;
using UnityEngine.Events;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string dialogue = "Ky uc o day van dang ngu quen...";
    public string interactionPrompt = "Press E to talk";
    public bool suppressDefaultDialogue;
    public UnityEvent interacted = new UnityEvent();
    public NguyenHueTutorialController tutorialController;
    public string InteractionPrompt => interactionPrompt;

    public void Interact(Interactor interactor)
    {
        PrototypeLogger.Info("NPC interact: " + name);
        if (!suppressDefaultDialogue)
        {
            UIManager.Instance?.ShowDialogue(dialogue);
        }

        tutorialController?.TalkToMusician();
        interacted?.Invoke();
    }
}
