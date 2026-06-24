using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PigeonRiseEffect : MonoBehaviour
{
    [Header("Restoration")]
    public MemoryZoneController zone;
    public float riseHeight = 4f;
    public float duration = 1.5f;
    public float startDelay;
    public Color restoredColor = new Color(0.92f, 0.93f, 0.96f);

    [Header("Orbit")]
    public Transform orbitCenter;
    public float orbitRadius = 5f;
    public float orbitSpeed = 24f;
    public float verticalBob = 0.3f;
    public float modelYawOffset;

    [Header("Animation clips imported from boCau.glb")]
    public string groundClipName = "Pigeon_Look";
    public string flyClipName = "AA_Pigeon_Fly";
    public AnimationClip groundAnimationClip;
    public AnimationClip flyAnimationClip;

    private Vector3 startPosition;
    private Renderer[] cachedRenderers;
    private Animator cachedAnimator;
    private PlayableGraph animationGraph;
    private float orbitAngle;
    private float orbitHeight;
    private bool isFlying;

    private void Awake()
    {
        startPosition = transform.position;
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedAnimator = GetComponentInChildren<Animator>(true);

        ResolveAnimationClips();
        PlayClip(groundAnimationClip);
    }

    private IEnumerator Start()
    {
        if (zone != null)
        {
            zone.Restored += HandleZoneRestored;
        }

        // MemoryZoneController restores save data in Start, so wait until it has initialized.
        yield return null;

        if (zone != null && zone.IsRestored)
        {
            SetRestoredInstant();
        }
    }

    private void OnDestroy()
    {
        if (zone != null)
        {
            zone.Restored -= HandleZoneRestored;
        }

        if (animationGraph.IsValid())
        {
            animationGraph.Destroy();
        }
    }

    private void Update()
    {
        if (!isFlying || orbitCenter == null)
        {
            return;
        }

        orbitAngle = Mathf.Repeat(orbitAngle + orbitSpeed * Time.deltaTime, 360f);
        SetOrbitPose(orbitAngle, Time.time);
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

        ApplyRestoredColor();
        PlayClip(flyAnimationClip);

        Vector3 center = GetOrbitCenterPosition();
        orbitAngle = GetStartAngle(center);
        orbitHeight = center.y + riseHeight;

        Vector3 from = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration)));
            float angle = orbitAngle + orbitSpeed * elapsed;
            Vector3 target = GetOrbitPosition(angle, 0f);
            transform.position = Vector3.Lerp(from, target, t);
            FaceFlightDirection(angle);
            yield return null;
        }

        orbitAngle = Mathf.Repeat(orbitAngle + orbitSpeed * duration, 360f);
        isFlying = true;
    }

    private void SetRestoredInstant()
    {
        ApplyRestoredColor();
        PlayClip(flyAnimationClip);

        Vector3 center = GetOrbitCenterPosition();
        orbitAngle = GetStartAngle(center);
        orbitHeight = center.y + riseHeight;
        isFlying = true;
        SetOrbitPose(orbitAngle, Time.time);
    }

    private void SetOrbitPose(float angle, float time)
    {
        transform.position = GetOrbitPosition(angle, Mathf.Sin((time + startDelay) * 2.2f) * verticalBob);
        FaceFlightDirection(angle);
    }

    private Vector3 GetOrbitPosition(float angle, float bob)
    {
        Vector3 center = GetOrbitCenterPosition();
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(
            center.x + Mathf.Cos(radians) * orbitRadius,
            orbitHeight + bob,
            center.z + Mathf.Sin(radians) * orbitRadius);
    }

    private void FaceFlightDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector3 tangent = new Vector3(-Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        if (tangent.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up) * Quaternion.Euler(0f, modelYawOffset, 0f);
        }
    }

    private Vector3 GetOrbitCenterPosition()
    {
        if (orbitCenter != null)
        {
            return orbitCenter.position;
        }

        return new Vector3(startPosition.x, 0f, startPosition.z);
    }

    private float GetStartAngle(Vector3 center)
    {
        Vector3 offset = startPosition - center;
        if (offset.sqrMagnitude < 0.01f)
        {
            return startDelay * 90f;
        }

        return Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    private void ResolveAnimationClips()
    {
        if (cachedAnimator == null)
        {
            return;
        }

        if (groundAnimationClip != null && flyAnimationClip != null)
        {
            return;
        }

        AnimationClip[] clips = cachedAnimator.GetComponentsInChildren<Animator>(true).Length > 0
            ? Resources.FindObjectsOfTypeAll<AnimationClip>()
            : new AnimationClip[0];

        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
            {
                continue;
            }

            if (groundAnimationClip == null && clip.name.Contains(groundClipName))
            {
                groundAnimationClip = clip;
            }

            if (flyAnimationClip == null && clip.name.Contains(flyClipName))
            {
                flyAnimationClip = clip;
            }

            if (groundAnimationClip != null && flyAnimationClip != null)
            {
                break;
            }
        }
    }

    private void PlayClip(AnimationClip clip)
    {
        if (cachedAnimator == null || clip == null)
        {
            return;
        }

        if (animationGraph.IsValid())
        {
            animationGraph.Destroy();
        }

        cachedAnimator.applyRootMotion = false;
        animationGraph = PlayableGraph.Create(name + "_PigeonAnimation");
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(animationGraph, "PigeonAnimation", cachedAnimator);
        AnimationClipPlayable playable = AnimationClipPlayable.Create(animationGraph, clip);
        playable.SetApplyFootIK(false);
        playable.SetApplyPlayableIK(false);
        output.SetSourcePlayable(playable);
        animationGraph.Play();
    }

    private void ApplyRestoredColor()
    {
        foreach (Renderer renderer in cachedRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            foreach (Material material in renderer.materials)
            {
                if (material != null && material.HasProperty("_BaseColor"))
                {
                    Color original = material.GetColor("_BaseColor");
                    material.SetColor("_BaseColor", new Color(restoredColor.r, restoredColor.g, restoredColor.b, original.a));
                }
                else if (material != null && material.HasProperty("_Color"))
                {
                    Color original = material.GetColor("_Color");
                    material.SetColor("_Color", new Color(restoredColor.r, restoredColor.g, restoredColor.b, original.a));
                }
            }
        }
    }
}
