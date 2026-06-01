using UnityEngine;

public class BenThanhMemoryItemInteractable : MonoBehaviour, IInteractable
{
    public BenThanhSceneController sceneController;
    public string interactionPrompt = "Nhấn E để nhặt nón lá cũ";

    [TextArea]
    public string collectedMessage = "Bạn tìm thấy một chiếc nón lá cũ. Có lẽ nó gắn với ký ức của khu chợ.";

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
        sceneController?.OnConicalHatCollected();
        gameObject.SetActive(false);
    }
}
