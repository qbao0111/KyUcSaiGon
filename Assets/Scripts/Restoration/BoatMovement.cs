using UnityEngine;

public class BoatMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2.0f;
    public float minX = -52f; // Extend slightly past river boundaries
    public float maxX = 52f;
    public bool isMoving = true;

    [Header("Bobbing & Rocking Settings")]
    public bool enableBobbing = true;
    public float bobSpeed = 1.3f;
    public float bobAmplitude = 0.08f;

    public bool enableRocking = true;
    public float rockingSpeed = 0.9f;
    public float rockingAmplitude = 1.8f; // Side-to-side roll amplitude in degrees
    public float pitchAmplitude = 0.6f;   // Front-to-back pitch amplitude in degrees

    [Header("Stern Wake (Wake Trail)")]
    public bool enableSternWake = true;
    public Vector3 sternWakeOffset = new Vector3(-2.2f, -1.6f, 0f);
    public float sternStartSize = 0.5f;
    public float sternEndSize = 2.2f;
    public float sternLifetime = 1.6f;
    public float sternEmissionRate = 22f;
    public float sternStartSpeed = 1.2f;

    [Header("Bow Wave (Front Splash)")]
    public bool enableBowWave = true;
    public Vector3 bowWaveOffset = new Vector3(2.2f, -1.6f, 0f);
    public float bowStartSize = 0.3f;
    public float bowEndSize = 1.2f;
    public float bowLifetime = 0.8f;
    public float bowEmissionRate = 12f;
    public float bowStartSpeed = 0.6f;

    private float startY;
    private float startX;
    private Quaternion startRotation;

    private ParticleSystem sternWakeParticles;
    private ParticleSystem bowWaveParticles;

    private void Start()
    {
        startY = transform.position.y;
        startX = transform.position.x;
        startRotation = transform.rotation;

        // Try to automatically align particle Y offset with water plane height (Y = 0.08)
        GameObject waterPlane = GameObject.Find("WaterPlane");
        float waterY = waterPlane != null ? waterPlane.transform.position.y : 0.08f;
        
        // Calculate the local Y required to place the particle systems exactly at water level
        float localY = waterY - transform.position.y;
        
        // Adjust offsets to fit the water level dynamically
        sternWakeOffset.y = localY + 0.05f; // Slightly above water level to avoid clipping
        bowWaveOffset.y = localY + 0.05f;

        if (enableSternWake)
        {
            CreateSternWake();
        }

        if (enableBowWave)
        {
            CreateBowWave();
        }
    }

    private void Update()
    {
        // 1. Move boat along its local X-axis (forward direction)
        if (isMoving)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);

            // 2. Wrap movement range along the world X axis
            Vector3 currentPos = transform.position;
            if (currentPos.x > maxX)
            {
                currentPos.x = minX;
                transform.position = currentPos;
            }
            else if (currentPos.x < minX)
            {
                currentPos.x = maxX;
                transform.position = currentPos;
            }

            if (sternWakeParticles != null && !sternWakeParticles.isPlaying) sternWakeParticles.Play();
            if (bowWaveParticles != null && !bowWaveParticles.isPlaying) bowWaveParticles.Play();
        }
        else
        {
            if (sternWakeParticles != null && sternWakeParticles.isPlaying) sternWakeParticles.Stop();
            if (bowWaveParticles != null && bowWaveParticles.isPlaying) bowWaveParticles.Stop();
        }

        // 3. Apply gentle bobbing (Y offset)
        if (enableBobbing)
        {
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            Vector3 pos = transform.position;
            pos.y = startY + yOffset;
            transform.position = pos;
        }

        // 4. Apply gentle rocking (pitch and roll)
        if (enableRocking)
        {
            float roll = Mathf.Sin(Time.time * rockingSpeed) * rockingAmplitude;
            float pitch = Mathf.Cos(Time.time * rockingSpeed * 1.3f) * pitchAmplitude;
            transform.rotation = startRotation * Quaternion.Euler(pitch, 0f, roll);
        }
    }

    private void CreateSternWake()
    {
        GameObject wakeObj = new GameObject("WaterSternWake");
        wakeObj.transform.SetParent(transform);
        wakeObj.transform.localPosition = sternWakeOffset;
        // Point the emitter backwards (local -X direction of the boat).
        // Since local Z of the particle system is the emission direction, we rotate Y by -90 to point it along -X.
        wakeObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);

        sternWakeParticles = wakeObj.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(sternWakeParticles, sternLifetime, sternStartSpeed, sternStartSize, sternEndSize, sternEmissionRate, true);
    }

    private void CreateBowWave()
    {
        GameObject bowObj = new GameObject("WaterBowWave");
        bowObj.transform.SetParent(transform);
        bowObj.transform.localPosition = bowWaveOffset;
        // Point the emitter upwards and outwards (local Z points up/out)
        bowObj.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);

        bowWaveParticles = bowObj.AddComponent<ParticleSystem>();
        ConfigureParticleSystem(bowWaveParticles, bowLifetime, bowStartSpeed, bowStartSize, bowEndSize, bowEmissionRate, false);
    }

    private void ConfigureParticleSystem(ParticleSystem ps, float lifetime, float startSpeed, float startSize, float endSize, float rate, bool isWake)
    {
        // Stop particle system before modifying settings to prevent play mode warnings
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 1. Main settings
        var main = ps.main;
        main.duration = 1.0f;
        main.loop = true;
        main.startLifetime = lifetime;
        main.startSpeed = startSpeed;
        main.startSize = startSize;
        // Light-blue/white semi-transparent color for water foam
        main.startColor = new Color(0.92f, 0.96f, 1.0f, 0.28f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        // 2. Emission
        var emission = ps.emission;
        emission.rateOverTime = rate;

        // 3. Shape
        var shape = ps.shape;
        shape.enabled = true;
        if (isWake)
        {
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.4f, 0.1f, 1.2f); // Width covering the boat's stern
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.3f;
        }

        // 4. Size over lifetime (expanding foam)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, endSize / startSize);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, sizeCurve);

        // 5. Color over lifetime (fading out)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.28f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        // Renderer setup (uses the default Unity particle material assigned by AddComponent)
        ParticleSystemRenderer psr = ps.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Billboard;

        // Restart particle system
        ps.Play();
    }
}
