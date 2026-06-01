using System.Collections;
using UnityEngine;

public class BitexcoTowerLightStrip : MonoBehaviour
{
    public MemoryZoneController zone;
    public float turnOnDelay;
    public Color offColor = new Color(0.22f, 0.24f, 0.28f);
    public Color onColor = new Color(0.52f, 0.34f, 0.92f);
    public float emission = 1.6f;

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        ApplyOff();
    }

    private void Start()
    {
        if (zone != null)
        {
            zone.Restored += HandleRestored;
            if (zone.IsRestored)
            {
                ApplyOn();
            }
        }
    }

    private void OnDestroy()
    {
        if (zone != null)
        {
            zone.Restored -= HandleRestored;
        }
    }

    private void HandleRestored()
    {
        StopAllCoroutines();
        StartCoroutine(TurnOnRoutine());
    }

    private IEnumerator TurnOnRoutine()
    {
        if (turnOnDelay > 0f)
        {
            yield return new WaitForSeconds(turnOnDelay);
        }

        ApplyOn();
    }

    private void ApplyOff()
    {
        if (cachedRenderer == null)
        {
            return;
        }

        cachedRenderer.material.color = offColor;
        cachedRenderer.material.EnableKeyword("_EMISSION");
        cachedRenderer.material.SetColor("_EmissionColor", Color.black);
    }

    private void ApplyOn()
    {
        if (cachedRenderer == null)
        {
            return;
        }

        cachedRenderer.material.color = onColor;
        cachedRenderer.material.EnableKeyword("_EMISSION");
        cachedRenderer.material.SetColor("_EmissionColor", onColor * emission);
    }
}
