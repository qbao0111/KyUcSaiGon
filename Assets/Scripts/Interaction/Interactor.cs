using UnityEngine;

public class Interactor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionRange = 4f;
    public float nearbyInteractionRadius = 3f;
    public float facingDotThreshold = 0.25f;
    public LayerMask interactableMask = ~0;

    private IInteractable currentInteractable;
    private IInteractable lastInteractable;

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

        if (currentInteractable != null && GameInput.InteractPressed)
        {
            PrototypeLogger.Info("Interact pressed on: " + currentInteractable.GetType().Name);
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

        currentInteractable = FindByCameraAim();

        if (currentInteractable == null)
        {
            currentInteractable = FindByPlayerForward();
        }

        if (currentInteractable == null)
        {
            currentInteractable = FindFacingNearbyInteractable();
        }

        bool hasPrompt = currentInteractable != null;
        string prompt = hasPrompt ? currentInteractable.InteractionPrompt : string.Empty;
        UIManager.Instance?.ShowInteractionPrompt(hasPrompt, prompt);

        if (currentInteractable != lastInteractable)
        {
            if (currentInteractable != null)
            {
                PrototypeLogger.Info("Interactable nearby: " + currentInteractable.GetType().Name);
            }

            lastInteractable = currentInteractable;
        }
    }

    private IInteractable FindByCameraAim()
    {
        if (playerCamera == null)
        {
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        return FindFromRay(ray);
    }

    private IInteractable FindByPlayerForward()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1f, transform.forward);
        return FindFromRay(ray);
    }

    private IInteractable FindFromRay(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionRange, interactableMask, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                return interactable;
            }
        }

        return null;
    }

    private IInteractable FindFacingNearbyInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, nearbyInteractionRadius, interactableMask, QueryTriggerInteraction.Collide);
        IInteractable bestInteractable = null;
        float bestScore = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.transform.IsChildOf(transform))
            {
                continue;
            }

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            Vector3 toTarget = hit.bounds.center - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            float facingPenalty = 0f;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                float dot = Vector3.Dot(transform.forward, toTarget.normalized);
                if (dot < facingDotThreshold)
                {
                    continue;
                }

                facingPenalty = Mathf.Clamp01(1f - dot);
            }

            float score = distance + facingPenalty;
            if (score < bestScore)
            {
                bestScore = score;
                bestInteractable = interactable;
            }
        }

        return bestInteractable;
    }
}
