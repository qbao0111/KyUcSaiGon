using System.Collections;
using UnityEngine;

public class MaterialRestoreEffect : RestorableEffect
{
    public Renderer[] renderers;
    public Color grayColor = Color.gray;
    public Color restoredColor = Color.white;
    public float fadeSeconds = 1.2f;

    private void Reset()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public override void SetRestoredInstant(bool restored)
    {
        ApplyColor(restored ? restoredColor : grayColor);
    }

    public override void PlayRestore()
    {
        StopAllCoroutines();
        StartCoroutine(FadeToColor(restoredColor));
    }

    private IEnumerator FadeToColor(Color targetColor)
    {
        Color startColor = renderers.Length > 0 ? renderers[0].material.color : grayColor;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            ApplyColor(Color.Lerp(startColor, targetColor, elapsed / fadeSeconds));
            yield return null;
        }

        ApplyColor(targetColor);
    }

    private void ApplyColor(Color color)
    {
        foreach (Renderer itemRenderer in renderers)
        {
            if (itemRenderer != null)
            {
                itemRenderer.material.color = color;
            }
        }
    }
}
