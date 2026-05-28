using UnityEngine;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    public string itemName = "Memory Item";
    [TextArea] public string inspectText = "Mot mon do cu giu lai mot manh ky uc.";
    public string InteractionPrompt => "Press E to inspect";

    public void Interact(Interactor interactor)
    {
        UIManager.Instance?.ShowDialogue(itemName + "\n" + inspectText);
    }
}
