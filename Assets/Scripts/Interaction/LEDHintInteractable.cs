using UnityEngine;

public class LEDHintInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string hintMessage;
    public NguyenHueTutorialController tutorialController;
    public string InteractionPrompt => "Nhấn E để xem màn hình LED";

    public void Interact(Interactor interactor)
    {
        PrototypeLogger.Info("LED hint inspect: " + name);
        tutorialController?.InspectLedHint(hintMessage);
    }
}
