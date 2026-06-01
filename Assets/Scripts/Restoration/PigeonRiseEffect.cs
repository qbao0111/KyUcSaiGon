using System.Collections;
using UnityEngine;

public class PigeonRiseEffect : MonoBehaviour
{
    public MemoryZoneController zone;
    public float riseHeight = 4f;
    public float duration = 1.5f;
    public float startDelay;
    public Color restoredColor = new Color(0.92f, 0.93f, 0.96f);

    private Vector3 startPosition;
    private Renderer cachedRenderer;

    private void Awake()
    {
        startPosition = transform.localPosition;
        cachedRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        if (zone != null)
        {
            zone.Restored += HandleZoneRestored;
            if (zone.IsRestored)
            {
                SetRestoredInstant();
            }
        }
    }

    private void OnDestroy()
    {
        if (zone != null)
        {
            zone.Restored -= HandleZoneRestored;
        }
    }

    private void HandleZoneRestored()
    {
        StopAllCoroutines();
        StartCoroutine(RiseRoutine());
    }

    private IEnumerator RiseRoutine()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = restoredColor;
        }

        float elapsed = 0f;
        Vector3 from = startPosition;
        Vector3 to = startPosition + Vector3.up * riseHeight;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localPosition = to;
    }

    private void SetRestoredInstant()
    {
        transform.localPosition = startPosition + Vector3.up * riseHeight;
        if (cachedRenderer != null)
        {
            cachedRenderer.material.color = restoredColor;
        }
    }
}
