using System.Collections;
using UnityEngine;

public class LightRestoreEffect : RestorableEffect
{
    public Light[] lights;
    public float dimIntensity = 0.2f;
    public float restoredIntensity = 1.5f;
    public float fadeSeconds = 1f;

    private void Reset()
    {
        lights = GetComponentsInChildren<Light>();
    }

    public override void SetRestoredInstant(bool restored)
    {
        foreach (Light lightItem in lights)
        {
            if (lightItem != null)
            {
                lightItem.intensity = restored ? restoredIntensity : dimIntensity;
                lightItem.enabled = true;
            }
        }
    }

    public override void PlayRestore()
    {
        StopAllCoroutines();
        StartCoroutine(FadeLight());
    }

    private IEnumerator FadeLight()
    {
        float elapsed = 0f;
        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float amount = Mathf.Lerp(dimIntensity, restoredIntensity, elapsed / fadeSeconds);
            foreach (Light lightItem in lights)
            {
                if (lightItem != null)
                {
                    lightItem.enabled = true;
                    lightItem.intensity = amount;
                }
            }

            yield return null;
        }
    }
}
