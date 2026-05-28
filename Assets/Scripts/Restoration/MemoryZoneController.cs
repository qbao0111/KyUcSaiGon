using System;
using UnityEngine;

public class MemoryZoneController : MonoBehaviour
{
    public LocationId locationId;
    public string memoryFragmentName;
    public RestorableEffect[] restoreEffects;
    public GameObject busStopReturn;

    public bool IsRestored { get; private set; }
    public event Action Restored;

    private void Awake()
    {
        if (restoreEffects == null || restoreEffects.Length == 0)
        {
            restoreEffects = GetComponentsInChildren<RestorableEffect>(true);
        }
    }

    private void Start()
    {
        IsRestored = GameProgressManager.Instance != null && GameProgressManager.Instance.IsRestored(locationId);
        ApplyRestoredState(IsRestored, true);
    }

    public void RestoreZone()
    {
        if (IsRestored)
        {
            return;
        }

        IsRestored = true;
        GameProgressManager.Instance?.MarkLocationRestored(locationId, memoryFragmentName);
        ApplyRestoredState(true, false);
        UIManager.Instance?.ShowDialogue("Da nhan manh ky uc: " + memoryFragmentName);
        UIManager.Instance?.SetObjective("Tro ve tram xe buyt co.");
        Restored?.Invoke();
    }

    private void ApplyRestoredState(bool restored, bool instant)
    {
        foreach (RestorableEffect effect in restoreEffects)
        {
            if (effect == null)
            {
                continue;
            }

            if (instant)
            {
                effect.SetRestoredInstant(restored);
            }
            else
            {
                effect.PlayRestore();
            }
        }

        if (busStopReturn != null)
        {
            busStopReturn.SetActive(restored);
        }
    }
}
