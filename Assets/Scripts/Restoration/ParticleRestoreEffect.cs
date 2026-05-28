using UnityEngine;

public class ParticleRestoreEffect : RestorableEffect
{
    public ParticleSystem[] particles;

    private void Reset()
    {
        particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public override void SetRestoredInstant(bool restored)
    {
        foreach (ParticleSystem particle in particles)
        {
            if (particle == null)
            {
                continue;
            }

            if (restored)
            {
                particle.gameObject.SetActive(true);
                particle.Play();
            }
            else
            {
                particle.Stop();
                particle.gameObject.SetActive(false);
            }
        }
    }

    public override void PlayRestore()
    {
        SetRestoredInstant(true);
    }
}
