using System.Collections;
using UnityEngine;

public class MaterialRestoreEffect : RestorableEffect
{
    public Renderer[] renderers;
    public Color grayColor = Color.gray;
    public Color restoredColor = Color.white;
    public float fadeSeconds = 1.2f;
    public bool preserveRendererColors;
    [Range(0f, 1f)] public float grayBlend = 0.75f;

    private Material[] cachedMaterials;
    private Color[] originalColors;

    private void Reset()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    public override void SetRestoredInstant(bool restored)
    {
        CacheOriginalColors();
        ApplyState(restored, 1f);
    }

    public override void PlayRestore()
    {
        CacheOriginalColors();
        StopAllCoroutines();
        StartCoroutine(FadeRestore());
    }

    private IEnumerator FadeRestore()
    {
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            ApplyState(true, elapsed / fadeSeconds);
            yield return null;
        }

        ApplyState(true, 1f);
    }

    private void CacheOriginalColors()
    {
        if (cachedMaterials != null && originalColors != null)
        {
            return;
        }

        cachedMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer itemRenderer = renderers[i];
            if (itemRenderer == null)
            {
                continue;
            }

            Material material = itemRenderer.material;
            cachedMaterials[i] = material;
            originalColors[i] = material != null && material.HasProperty("_Color") ? material.color : Color.white;
        }
    }

    private void ApplyState(bool restored, float amount)
    {
        if (!preserveRendererColors)
        {
            ApplyColor(restored ? restoredColor : grayColor);
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = cachedMaterials != null && i < cachedMaterials.Length ? cachedMaterials[i] : null;
            if (material == null || !material.HasProperty("_Color"))
            {
                continue;
            }

            Color originalColor = originalColors != null && i < originalColors.Length ? originalColors[i] : restoredColor;
            Color grayTone = Color.Lerp(originalColor, grayColor, grayBlend);
            Color targetColor = restored ? originalColor : grayTone;
            material.color = restored ? Color.Lerp(grayTone, targetColor, amount) : targetColor;
        }
    }

    private void ApplyColor(Color color)
    {
        CacheOriginalColors();
        foreach (Renderer itemRenderer in renderers)
        {
            if (itemRenderer != null)
            {
                itemRenderer.material.color = color;
            }
        }
    }
}