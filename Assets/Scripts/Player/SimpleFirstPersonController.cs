using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFirstPersonController : MonoBehaviour
{
    public Transform cameraRoot;
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;
    public float gravity = -18f;

    private CharacterController controller;
    private float cameraPitch;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
        {
            cameraRoot = Camera.main.transform;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -80f, 80f);

        if (cameraRoot != null)
        {
            cameraRoot.localEulerAngles = Vector3.right * cameraPitch;
        }
    }

    private void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = (transform.right * horizontal + transform.forward * vertical).normalized * moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }
}
