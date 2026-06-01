using UnityEngine;

public class BitexcoEmployeeCardInteractable : MonoBehaviour, IInteractable
{
    public BitexcoSceneController sceneController;
    public string interactionPrompt = "Nhấn E để nhặt thẻ nhân viên";

    [TextArea]
    public string collectedMessage = "Bạn tìm thấy một chiếc thẻ nhân viên.";

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
        sceneController?.OnEmployeeCardCollected();
        gameObject.SetActive(false);
    }
}
