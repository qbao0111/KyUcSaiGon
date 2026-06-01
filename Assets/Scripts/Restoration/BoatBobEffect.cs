using UnityEngine;

public class BoatBobEffect : MonoBehaviour
{
    public MemoryZoneController zone;
    public float bobAmplitude = 0.35f;
    public float bobSpeed = 1.4f;

    private Vector3 startPosition;
    private bool active;

    private void Awake()
    {
        startPosition = transform.localPosition;
    }

    private void Start()
    {
        if (zone != null)
        {
            zone.Restored += HandleRestored;
            active = zone.IsRestored;
        }
    }

    private void OnDestroy()
    {
        if (zone != null)
        {
            zone.Restored -= HandleRestored;
        }
    }

    private void Update()
    {
        if (!active)
        {
            return;
        }

        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.localPosition = startPosition + Vector3.up * yOffset;
    }

    private void HandleRestored()
    {
        active = true;
    }
}
