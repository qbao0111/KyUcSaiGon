using UnityEngine;

public class PlayerMovementAnimator : MonoBehaviour
{
    public Transform visualRoot;
    public Animator animator;
    public float targetVisualScale = 1.45f;
    public float bobHeight = 0.04f;
    public float bobSpeed = 8f;
    public float leanAngle = 3f;

    private ThirdPersonPlayerController playerController;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private bool hasSpeedParameter;

    private void Awake()
    {
        playerController = GetComponent<ThirdPersonPlayerController>();

        if (visualRoot == null)
        {
            Transform found = transform.Find("Visual_REPLACE_Player_P09_Humandroid");
            visualRoot = found != null ? found : transform;
        }

        if (animator == null && visualRoot != null)
        {
            animator = visualRoot.GetComponentInChildren<Animator>();
        }

        ApplyVisualScale();

        initialLocalPosition = visualRoot != null ? visualRoot.localPosition : Vector3.zero;
        initialLocalRotation = visualRoot != null ? visualRoot.localRotation : Quaternion.identity;
        hasSpeedParameter = HasAnimatorFloat("Speed");
    }

    private void Update()
    {
        float moveAmount = playerController != null ? playerController.NormalizedMoveSpeed : 0f;

        if (animator != null && hasSpeedParameter)
        {
            animator.SetFloat("Speed", moveAmount, 0.12f, Time.deltaTime);
        }

        AnimateVisualRoot(moveAmount);
    }

    private void AnimateVisualRoot(float moveAmount)
    {
        if (visualRoot == null)
        {
            return;
        }

        if (moveAmount < 0.02f)
        {
            visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, initialLocalPosition, Time.deltaTime * 8f);
            visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, initialLocalRotation, Time.deltaTime * 8f);
            return;
        }

        float wave = Mathf.Sin(Time.time * bobSpeed);
        Vector3 bob = Vector3.up * (Mathf.Abs(wave) * bobHeight * moveAmount);
        Quaternion lean = Quaternion.Euler(wave * leanAngle * moveAmount, 0f, -wave * leanAngle * 0.5f * moveAmount);
        visualRoot.localPosition = initialLocalPosition + bob;
        visualRoot.localRotation = initialLocalRotation * lean;
    }

    private void ApplyVisualScale()
    {
        if (visualRoot == null || targetVisualScale <= 0f)
        {
            return;
        }

        visualRoot.localScale = Vector3.one * targetVisualScale;
    }

    private bool HasAnimatorFloat(string parameterName)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName && parameter.type == AnimatorControllerParameterType.Float)
            {
                return true;
            }
        }

        return false;
    }
}
