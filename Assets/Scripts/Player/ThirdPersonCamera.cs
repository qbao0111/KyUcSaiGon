using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 7f;
    public float height = 2.8f;
    public float mouseSensitivity = 2f;
    public float minPitch = -30f;
    public float maxPitch = 65f;
    public float followSmoothTime = 0.08f;
    public float rotationSmoothTime = 0.04f;
    public float cameraCollisionRadius = 0.25f;
    public LayerMask collisionMask = ~0;

    private float yaw;
    private float pitch = 18f;
    private Vector3 positionVelocity;
    private Vector3 currentLookDirection;
    private Vector3 lookDirectionVelocity;
    private bool initialized;

    private void Start()
    {
        ApplyStableViewDefaults();
        Camera attachedCamera = GetComponent<Camera>();
        if (attachedCamera != null)
        {
            attachedCamera.fieldOfView = 60f;
        }

        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (UIManager.Instance == null || !UIManager.Instance.IsBlockingPlayerInput)
        {
            Vector2 look = GameInput.Look * mouseSensitivity * 0.08f;
            yaw += look.x;
            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
        }

        Vector3 focusPoint = target.position + Vector3.up * height;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 backward = yawRotation * Vector3.back;

        float pitchRadians = pitch * Mathf.Deg2Rad;
        float horizontalDistance = Mathf.Cos(pitchRadians) * distance;
        float verticalOffset = Mathf.Sin(pitchRadians) * distance;
        Vector3 desiredPosition = focusPoint + backward * horizontalDistance + Vector3.up * verticalOffset;
        Vector3 toCamera = desiredPosition - focusPoint;

        if (toCamera.sqrMagnitude > 0.01f &&
            Physics.SphereCast(focusPoint, cameraCollisionRadius, toCamera.normalized, out RaycastHit hit, distance, collisionMask, QueryTriggerInteraction.Ignore) &&
            !hit.collider.transform.IsChildOf(target))
        {
            desiredPosition = focusPoint + toCamera.normalized * Mathf.Max(0.6f, hit.distance - 0.1f);
        }

        if (!initialized)
        {
            transform.position = desiredPosition;
            initialized = true;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, followSmoothTime);
        }

        Vector3 desiredLookDirection = (focusPoint - transform.position).normalized;
        if (currentLookDirection == Vector3.zero)
        {
            currentLookDirection = desiredLookDirection;
        }

        currentLookDirection = Vector3.SmoothDamp(currentLookDirection, desiredLookDirection, ref lookDirectionVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.LookRotation(currentLookDirection, Vector3.up);
    }

    private void ApplyStableViewDefaults()
    {
        // Keep orbit stable but allow looking up to inspect tall landmarks.
        if (distance < 4f || distance > 8f)
        {
            distance = 7f;
        }

        if (height < 0.8f || height > 3.2f)
        {
            height = 2.8f;
        }

        if (minPitch < -45f || minPitch > 10f)
        {
            minPitch = -30f;
        }

        if (maxPitch < 25f || maxPitch > 80f || maxPitch <= minPitch)
        {
            maxPitch = 65f;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }
}
