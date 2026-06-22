using UnityEngine;
using UnityEngine.Events;

public class ItemInteractable : MonoBehaviour, IInteractable
{
    public string itemName = "Memory Item";
    [TextArea] public string inspectText = "Mot mon do cu giu lai mot manh ky uc.";
    public UnityEvent interacted = new UnityEvent();
    public string InteractionPrompt => "Nhấn E để xem vật phẩm";

    public void Interact(Interactor interactor)
    {
        UIManager.Instance?.ShowDialogue(itemName + "\n" + inspectText);
        interacted.Invoke();
    }
}
