using UnityEngine;

public class DinhDocLapMapItemInteractable : MonoBehaviour, IInteractable
{
    public DinhDocLapSceneController sceneController;
    public string interactionPrompt = "Nhấn E để nhặt mảnh bản đồ";

    [TextArea]
    public string collectedMessage = "Bạn tìm thấy một mảnh bản đồ lịch sử cũ.";

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
        sceneController?.OnHistoricalMapCollected();
        gameObject.SetActive(false);
    }
}
