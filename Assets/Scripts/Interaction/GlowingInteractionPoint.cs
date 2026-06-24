using System.Collections.Generic;
using UnityEngine;

public class GlowingInteractionPoint : MonoBehaviour
{
    [Header("Floating Animation")]
    public float floatAmplitude = 0.08f;
    public float floatFrequency = 1.2f;

    [Header("Scale Breathing")]
    public float scaleAmplitude = 0.06f;
    public float scaleFrequency = 1.5f;

    [Header("Light Breathing")]
    public float lightMinIntensity = 0.8f;
    public float lightMaxIntensity = 2.2f;
    public float lightFrequency = 1.4f;

    [Header("Orbiting Sparkles")]
    public int orbiterCount = 3;
    public float orbitRadius = 0.45f;
    public float orbitSpeedMultiplier = 1.2f;

    private Vector3 basePosition;
    private Vector3 baseScale;
    private Light pointLight;
    private Material sharedMaterial;
    private Color baseEmissionColor;

    private readonly List<Transform> orbiters = new List<Transform>();
    private readonly List<Vector3> orbitAxes = new List<Vector3>();
    private readonly List<float> orbitSpeeds = new List<float>();

    private void Start()
    {
        basePosition = transform.position;
        baseScale = transform.localScale;
        pointLight = GetComponent<Light>();

        // Find the visual mesh and cache its material
        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            Renderer r = visual.GetComponent<Renderer>();
            if (r != null)
            {
                sharedMaterial = r.material; // Get runtime instance material
                if (sharedMaterial.HasProperty("_EmissionColor"))
                {
                    baseEmissionColor = sharedMaterial.GetColor("_EmissionColor");
                }
                else if (sharedMaterial.HasProperty("emissiveFactor"))
                {
                    baseEmissionColor = sharedMaterial.GetColor("emissiveFactor");
                }
                else
                {
                    baseEmissionColor = new Color(0.96f, 0.83f, 0.36f) * 2.0f;
                }
            }
        }

        // Configure the Particle System for floating embers
        SetupParticles();

        // Spawn magical orbiting sparkles
        SpawnOrbiters();
    }

    private void SetupParticles()
    {
        ParticleSystem ps = gameObject.GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // Configure main module
        var main = ps.main;
        main.duration = 2.0f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 2.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor = new Color(0.96f, 0.83f, 0.36f, 0.45f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        // Configure emission module
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 16f;

        // Configure shape module (emit from a small disc/circle at the bottom of the beam)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;
        shape.rotation = new Vector3(90f, 0f, 0f); // Face upwards

        // Configure velocity over lifetime (float upwards gently)
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        // Configure color over lifetime (fade in, fade out)
        var colorModule = ps.colorOverLifetime;
        colorModule.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.96f, 0.83f, 0.36f), 0.0f),
                new GradientColorKey(new Color(0.98f, 0.90f, 0.50f), 0.5f),
                new GradientColorKey(new Color(0.96f, 0.83f, 0.36f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.0f, 0.0f), 
                new GradientAlphaKey(0.7f, 0.2f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorModule.color = new ParticleSystem.MinMaxGradient(grad);

        // Configure renderer
        var psRenderer = GetComponent<ParticleSystemRenderer>();
        if (psRenderer != null)
        {
            // Use transparent additive material
            Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            particleMat.name = "M_Ending_TriggerParticles";
            particleMat.SetFloat("_Surface", 1f); // Transparent
            particleMat.SetFloat("_Blend", 1f); // Additive
            particleMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            particleMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            particleMat.SetInt("_ZWrite", 0);
            particleMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            particleMat.renderQueue = 3000;
            particleMat.SetColor("_BaseColor", new Color(0.96f, 0.83f, 0.36f, 0.5f));
            particleMat.EnableKeyword("_EMISSION");
            particleMat.SetColor("_EmissionColor", new Color(0.96f, 0.83f, 0.36f) * 2.5f);
            
            psRenderer.sharedMaterial = particleMat;
        }

        ps.Play();
    }

    private void SpawnOrbiters()
    {
        if (sharedMaterial == null) return;

        // Define distinct tilted axes for each orbiter to orbit on
        Vector3[] axes = {
            new Vector3(0f, 1f, 0.1f).normalized,
            new Vector3(1f, 0.4f, 0f).normalized,
            new Vector3(-0.4f, 1f, 0.4f).normalized
        };

        float[] speeds = { 50f, -70f, 90f };

        for (int i = 0; i < orbiterCount; i++)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = $"MagicSparkle_{i}";
            Destroy(orb.GetComponent<Collider>()); // No collision
            
            orb.transform.SetParent(transform);
            orb.transform.localScale = Vector3.one * 0.15f; // Very tiny sparkles
            
            Renderer r = orb.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = sharedMaterial; // Shares the glowing golden material
            }

            orbiters.Add(orb.transform);
            orbitAxes.Add(axes[i % axes.Length]);
            orbitSpeeds.Add(speeds[i % speeds.Length] * orbitSpeedMultiplier);
        }
    }

    private void Update()
    {
        float time = Time.time;

        // 1. Sinusoidal floating
        float newY = basePosition.y + Mathf.Sin(time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(basePosition.x, newY, basePosition.z);

        // 2. Scale breathing (breath X and Z, keep Y constant or stretch Y)
        float scaleScale = 1.0f + Mathf.Sin(time * scaleFrequency) * scaleAmplitude;
        transform.localScale = new Vector3(baseScale.x * scaleScale, baseScale.y * (1.0f + scaleScale * 0.08f), baseScale.z * scaleScale);

        // 3. Light breathing
        if (pointLight != null)
        {
            float lightT = (Mathf.Sin(time * lightFrequency) + 1.0f) * 0.5f;
            pointLight.intensity = Mathf.Lerp(lightMinIntensity, lightMaxIntensity, lightT);
        }

        // 4. Material emission and transparency pulse
        if (sharedMaterial != null)
        {
            float pulse = Mathf.Sin(time * scaleFrequency);
            float emissionScale = 1.2f + pulse * 0.4f;
            float alphaScale = 0.12f + (pulse + 1.0f) * 0.5f * 0.08f; // Keep it dim and transparent (0.08 - 0.16)

            if (sharedMaterial.HasProperty("_EmissionColor"))
            {
                sharedMaterial.SetColor("_EmissionColor", baseEmissionColor * emissionScale);
            }
            if (sharedMaterial.HasProperty("_BaseColor"))
            {
                Color baseCol = sharedMaterial.GetColor("_BaseColor");
                baseCol.a = alphaScale;
                sharedMaterial.SetColor("_BaseColor", baseCol);
            }
        }

        // 5. Rotate the light beam cylinder slowly
        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            visual.Rotate(Vector3.up * 12f * Time.deltaTime, Space.Self);
        }

        // 6. Update orbiter positions
        for (int i = 0; i < orbiters.Count; i++)
        {
            if (orbiters[i] == null) continue;

            float angle = time * orbitSpeeds[i];
            Quaternion rot = Quaternion.AngleAxis(angle, orbitAxes[i]);
            Vector3 offset = rot * (Vector3.right * orbitRadius);
            
            // Adjust vertical offset for orbiters to orbit around the center of the beam
            orbiters[i].position = transform.position + offset + Vector3.up * Mathf.Sin(time + i) * 0.15f;
            orbiters[i].Rotate(Vector3.one * 30f * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        // Clean up runtime instance material to avoid leaks
        if (sharedMaterial != null)
        {
            Destroy(sharedMaterial);
        }
    }
}
