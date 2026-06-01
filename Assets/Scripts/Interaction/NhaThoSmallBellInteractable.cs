using UnityEngine;

public class NhaThoSmallBellInteractable : MonoBehaviour, IInteractable
{
    public NhaThoDucBaSceneController sceneController;
    public string interactionPrompt = "Nhấn E để nhặt chuông nhỏ";

    [TextArea]
    public string collectedMessage = "Bạn tìm thấy một chiếc chuông nhỏ.";

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
        sceneController?.OnSmallBellCollected();
        gameObject.SetActive(false);
    }
}
