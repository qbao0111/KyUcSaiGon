using UnityEngine;

public class EndingTriggerInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt => "Nhấn E để tìm lại ký ức đô thị";

    private bool interacted = false;

    public void Interact(Interactor interactor)
    {
        if (interacted) return;
        interacted = true;

        EndingSceneController controller = FindFirstObjectByType<EndingSceneController>();
        if (controller != null)
        {
            controller.TriggerEndingSequence();
        }
        else
        {
            Debug.LogError("EndingSceneController not found!");
        }

        // Disable this interaction point (visual and light)
        gameObject.SetActive(false);
    }
}
