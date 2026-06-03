using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    public Transform cameraTransform;
    public float moveSpeed = 5f;
    public float rotationSpeed = 12f;
    public float gravity = -18f;

    private CharacterController controller;
    private float verticalVelocity;
    private Vector3 lastPlanarVelocity;

    public float PlanarSpeed => lastPlanarVelocity.magnitude;
    public float NormalizedMoveSpeed => moveSpeed > 0.01f ? Mathf.Clamp01(PlanarSpeed / moveSpeed) : 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PrototypeLogger.Info("Third-person controller ready. WASD move, mouse orbit, E interact.");
    }

    private void Update()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsBlockingPlayerInput)
        {
            return;
        }

        MoveRelativeToCamera();
    }

    private void MoveRelativeToCamera()
    {
        Vector2 input = GameInput.Move;
        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;

        if (cameraTransform != null)
        {
            cameraForward = cameraTransform.forward;
            cameraRight = cameraTransform.right;
        }

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * input.y + cameraRight * input.x;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = moveDirection * moveSpeed;
        lastPlanarVelocity = velocity;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}
