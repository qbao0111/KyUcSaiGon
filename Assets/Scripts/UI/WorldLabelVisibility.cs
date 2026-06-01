using UnityEngine;

public class WorldLabelVisibility : MonoBehaviour
{
    public bool hideInPlayMode = true;

    private void Start()
    {
        if (!hideInPlayMode)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer labelRenderer in renderers)
        {
            labelRenderer.enabled = false;
        }
    }
}
