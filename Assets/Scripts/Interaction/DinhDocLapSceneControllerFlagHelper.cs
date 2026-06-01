using UnityEngine;

public class DinhDocLapSceneControllerFlagHelper : MonoBehaviour
{
    public MemoryZoneController zone;

    private void Start()
    {
        if (zone != null)
        {
            zone.Restored += HandleRestored;
            gameObject.SetActive(zone.IsRestored);
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
        gameObject.SetActive(true);
    }
}
