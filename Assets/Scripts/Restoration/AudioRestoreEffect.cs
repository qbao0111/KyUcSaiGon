using System.Collections;
using UnityEngine;

public class AudioRestoreEffect : RestorableEffect
{
    public AudioSource[] audioSources;
    public float restoredVolume = 0.5f;
    public float fadeSeconds = 1.5f;

    private void Reset()
    {
        audioSources = GetComponentsInChildren<AudioSource>(true);
    }

    public override void SetRestoredInstant(bool restored)
    {
        foreach (AudioSource source in audioSources)
        {
            if (source == null)
            {
                continue;
            }

            source.volume = restored ? restoredVolume : 0f;
            if (restored && !source.isPlaying)
            {
                source.Play();
            }
        }
    }

    public override void PlayRestore()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInAudio());
    }

    private IEnumerator FadeInAudio()
    {
        float elapsed = 0f;
        foreach (AudioSource source in audioSources)
        {
            if (source != null && !source.isPlaying)
            {
                source.volume = 0f;
                source.Play();
            }
        }

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.deltaTime;
            float volume = Mathf.Lerp(0f, restoredVolume, elapsed / fadeSeconds);
            foreach (AudioSource source in audioSources)
            {
                if (source != null)
                {
                    source.volume = volume;
                }
            }

            yield return null;
        }
    }
}
