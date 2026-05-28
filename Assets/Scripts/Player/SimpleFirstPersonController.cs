using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    public Transform cameraRoot;
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;
    public float gravity = -18f;
    public float cameraDistance = 2.15f;
    public float cameraHeight = 0.82f;
    public float cameraPitchMin = -18f;
    public float cameraPitchMax = 38f;
    public float cameraCollisionRadius = 0.18f;
    public LayerMask cameraCollisionMask = ~0;
    public Vector3 cameraShoulderOffset = Vector3.zero;

    private CharacterController controller;
    private float cameraYaw;
    private float cameraPitch;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
        {
            cameraRoot = Camera.main.transform;
        }

        ApplyComfortCameraDefaults();
        EnsureThirdPersonVisual();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraYaw = transform.eulerAngles.y;
        cameraPitch = 0f;
        PrototypeLogger.Info("Third-person player ready. WASD move, mouse orbit, E interact.");
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsBlockingPlayerInput)
        {
            return;
        }

        Look();
        Move();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    private void Look()
    {
        Vector2 look = GameInput.Look * mouseSensitivity * 0.08f;
        cameraYaw += look.x;
        cameraPitch = Mathf.Clamp(cameraPitch - look.y, cameraPitchMin, cameraPitchMax);
    }

    private void Move()
    {
        Vector2 moveInput = GameInput.Move;
        float horizontal = moveInput.x;
        float vertical = moveInput.y;

        Quaternion yawRotation = Quaternion.Euler(0f, cameraYaw, 0f);
        Vector3 movement = (yawRotation * new Vector3(horizontal, 0f, vertical)).normalized;

        if (movement.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * 12f);
        }

        movement *= moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    private void UpdateCamera()
    {
        if (cameraRoot == null)
        {
            return;
        }

        Quaternion cameraRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        Vector3 focusPoint = transform.position + Vector3.up * cameraHeight;
        Vector3 desiredPosition = focusPoint + cameraRotation * (cameraShoulderOffset + Vector3.back * cameraDistance);
        Vector3 cameraDirection = desiredPosition - focusPoint;
        float desiredDistance = cameraDirection.magnitude;

        if (desiredDistance > 0.01f && Physics.SphereCast(focusPoint, cameraCollisionRadius, cameraDirection.normalized, out RaycastHit hit, desiredDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.transform.IsChildOf(transform))
            {
                desiredPosition = focusPoint + cameraDirection.normalized * Mathf.Max(0.35f, hit.distance - 0.08f);
            }
        }

        cameraRoot.position = desiredPosition;
        cameraRoot.rotation = Quaternion.LookRotation(focusPoint - desiredPosition, Vector3.up);
    }

    private void ApplyComfortCameraDefaults()
    {
        // Older generated scenes may have the previous high camera values serialized.
        // Clamp them at runtime so existing scenes immediately get the improved view.
        if (cameraDistance > 2.3f)
        {
            cameraDistance = 2.15f;
        }

        if (cameraHeight > 0.95f)
        {
            cameraHeight = 0.82f;
        }
    }

    private void EnsureThirdPersonVisual()
    {
        if (transform.Find("REPLACE_Player_ThirdPersonVisual") != null)
        {
            return;
        }

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "REPLACE_Player_ThirdPersonVisual";
        visual.transform.SetParent(transform);
        visual.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(0.75f, 0.9f, 0.75f);

        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            Destroy(visualCollider);
        }

        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.45f, 0.95f);
        }

    }
}
