using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 1.4f;
    public float mouseSensitivity = 2f;
    public float minPitch = 8f;
    public float maxPitch = 38f;
    public float cameraCollisionRadius = 0.25f;
    public LayerMask collisionMask = ~0;

    private float yaw;
    private float pitch = 18f;

    private void Start()
    {
        ApplyStableViewDefaults();

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

        transform.position = desiredPosition;
        transform.rotation = Quaternion.LookRotation(focusPoint - desiredPosition, Vector3.up);
    }

    private void ApplyStableViewDefaults()
    {
        // Older generated scenes may have allowed negative pitch, which lets the
        // camera slide below the character. Clamp those serialized values here.
        if (height > 1.7f)
        {
            height = 1.4f;
        }

        if (minPitch < 0f)
        {
            minPitch = 8f;
        }

        if (maxPitch > 42f)
        {
            maxPitch = 38f;
        }

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }
}
