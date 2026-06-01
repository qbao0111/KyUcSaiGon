using UnityEngine;

public class BachDangTicketInteractable : MonoBehaviour, IInteractable
{
    public BachDangSceneController sceneController;
    public string interactionPrompt = "Nhấn E để nhặt vé tàu cũ";

    [TextArea]
    public string collectedMessage = "Bạn tìm thấy một chiếc vé tàu cũ.";

    public string InteractionPrompt => interactionPrompt;

    private bool collected;

    public void Interact(Interactor interactor)
    {
        if (collected)
        {
            return;
        }

        collected = true;
        UIManager.Instance?.ShowDialogue(collectedMessage);
        sceneController?.OnOldFerryTicketCollected();
        gameObject.SetActive(false);
    }
}
