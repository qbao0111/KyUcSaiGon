using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionRange = 3f;
    public LayerMask interactableMask = ~0;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsBlockingPlayerInput)
        {
            UIManager.Instance.ShowInteractionPrompt(false, string.Empty);
            return;
        }

        FindInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    private void FindInteractable()
    {
        currentInteractable = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableMask, QueryTriggerInteraction.Collide))
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        }

        bool hasPrompt = currentInteractable != null;
        string prompt = hasPrompt ? currentInteractable.InteractionPrompt : string.Empty;
        UIManager.Instance?.ShowInteractionPrompt(hasPrompt, prompt);
    }
}
