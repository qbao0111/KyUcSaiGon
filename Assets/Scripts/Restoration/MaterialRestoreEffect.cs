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
    private string[] colorPropertyNames;

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
        if (cachedMaterials != null && originalColors != null && colorPropertyNames != null)
        {
            return;
        }

        cachedMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];
        colorPropertyNames = new string[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer itemRenderer = renderers[i];
            if (itemRenderer == null)
            {
                continue;
            }

            Material material = itemRenderer.material;
            cachedMaterials[i] = material;
            
            if (material != null)
            {
                string propName = null;
                if (material.HasProperty("_BaseColor"))
                {
                    propName = "_BaseColor";
                }
                else if (material.HasProperty("_Color"))
                {
                    propName = "_Color";
                }
                colorPropertyNames[i] = propName;

                if (propName != null)
                {
                    originalColors[i] = material.GetColor(propName);
                }
                else
                {
                    originalColors[i] = Color.white;
                }
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }
    }

    private void ApplyState(bool restored, float amount)
    {
        CacheOriginalColors();

        if (!preserveRendererColors)
        {
            ApplyColor(restored ? restoredColor : grayColor);
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = cachedMaterials != null && i < cachedMaterials.Length ? cachedMaterials[i] : null;
            string propName = colorPropertyNames != null && i < colorPropertyNames.Length ? colorPropertyNames[i] : null;

            if (material == null || propName == null)
            {
                continue;
            }

            Color originalColor = originalColors != null && i < originalColors.Length ? originalColors[i] : restoredColor;
            
            // Calculate a true monochrome desaturated tone
            float luminance = originalColor.r * 0.299f + originalColor.g * 0.587f + originalColor.b * 0.114f;
            Color desaturated = new Color(luminance, luminance, luminance, originalColor.a);
            
            // Blend desaturated color with grayColor (tint) and original color (based on grayBlend)
            Color targetGray = Color.Lerp(desaturated, grayColor, 0.20f); // 20% tint, 80% desaturated monochrome
            Color grayTone = Color.Lerp(originalColor, targetGray, grayBlend);
            
            Color targetColor = restored ? originalColor : grayTone;
            material.SetColor(propName, restored ? Color.Lerp(grayTone, targetColor, amount) : targetColor);
        }
    }

    private void ApplyColor(Color color)
    {
        CacheOriginalColors();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = cachedMaterials != null && i < cachedMaterials.Length ? cachedMaterials[i] : null;
            string propName = colorPropertyNames != null && i < colorPropertyNames.Length ? colorPropertyNames[i] : null;

            if (material != null && propName != null)
            {
                material.SetColor(propName, color);
            }
        }
    }
}