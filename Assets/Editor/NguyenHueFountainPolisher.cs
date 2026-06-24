using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class NguyenHueFountainPolisher
{
    [MenuItem("KyUcSaiGon/Polish Nguyen Hue Fountain")]
    public static void PolishFountain()
    {
        Debug.Log("Starting Nguyen Hue Fountain Polish...");

        string scenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + scenePath);
            return;
        }

        // 1. Configure FountainEffects Particle Systems
        GameObject fountainEffects = GameObject.Find("FountainEffects");
        if (fountainEffects == null)
        {
            fountainEffects = new GameObject("FountainEffects");
            GameObject effectsRoot = GameObject.Find("EffectsRoot");
            if (effectsRoot != null)
            {
                fountainEffects.transform.SetParent(effectsRoot.transform);
            }
        }

        // Align with REPLACE_Landmark_NguyenHue_Fountain position z=12
        fountainEffects.transform.position = new Vector3(0f, 1.0f, 12f);
        fountainEffects.transform.localRotation = Quaternion.identity;

        // Configure Outer Jets ParticleSystem
        ParticleSystem outerPs = fountainEffects.GetComponent<ParticleSystem>();
        if (outerPs == null)
        {
            outerPs = fountainEffects.AddComponent<ParticleSystem>();
        }

        var main = outerPs.main;
        main.duration = 1.0f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 1.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 6.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.15f);
        main.gravityModifier = 1.25f;
        main.startColor = new Color(0.85f, 0.93f, 1.0f, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;

        var emission = outerPs.emission;
        emission.rateOverTime = 160f;

        var shape = outerPs.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f; // Spray outwards and slightly upwards
        shape.radius = 1.1f; // Match the lotus petals radius
        shape.radiusThickness = 0f;

        // Orient the emitter straight up (Euler -90, 0, 0 relative to parent)
        fountainEffects.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystemRenderer psr = fountainEffects.GetComponent<ParticleSystemRenderer>();
        psr.renderMode = ParticleSystemRenderMode.Stretch;
        psr.velocityScale = 0.03f;
        psr.lengthScale = 1.4f;
        psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");

        // Configure Central Jet ParticleSystem (Child GameObject)
        Transform centralJetT = fountainEffects.transform.Find("CentralJet");
        GameObject centralJetGo;
        if (centralJetT == null)
        {
            centralJetGo = new GameObject("CentralJet");
            centralJetGo.transform.SetParent(fountainEffects.transform);
        }
        else
        {
            centralJetGo = centralJetT.gameObject;
        }

        centralJetGo.transform.localPosition = new Vector3(0f, 0f, 0.1f); // Offset along Z (straight up in world space)
        centralJetGo.transform.localRotation = Quaternion.identity;
        centralJetGo.transform.localScale = Vector3.one;

        ParticleSystem centralPs = centralJetGo.GetComponent<ParticleSystem>();
        if (centralPs == null)
        {
            centralPs = centralJetGo.AddComponent<ParticleSystem>();
        }

        var cMain = centralPs.main;
        cMain.duration = 1.0f;
        cMain.loop = true;
        cMain.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.3f);
        cMain.startSpeed = new ParticleSystem.MinMaxCurve(7.5f, 9.5f);
        cMain.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
        cMain.gravityModifier = 1.4f;
        cMain.startColor = new Color(0.9f, 0.95f, 1.0f, 0.55f);
        cMain.simulationSpace = ParticleSystemSimulationSpace.World;
        cMain.playOnAwake = true;

        var cEmission = centralPs.emission;
        cEmission.rateOverTime = 80f;

        var cShape = centralPs.shape;
        cShape.enabled = true;
        cShape.shapeType = ParticleSystemShapeType.Cone;
        cShape.angle = 2.5f; // Narrow vertical stream
        cShape.radius = 0.15f;

        ParticleSystemRenderer cPsr = centralPs.GetComponent<ParticleSystemRenderer>();
        cPsr.renderMode = ParticleSystemRenderMode.Stretch;
        cPsr.velocityScale = 0.03f;
        cPsr.lengthScale = 1.4f;
        cPsr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");

        // 2. Hide static water jet meshes in the FBX model
        GameObject fbxModel = GameObject.Find("Visual_REPLACE_NguyenHue_Fountain_FBX");
        if (fbxModel != null)
        {
            Renderer[] renderers = fbxModel.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                string name = r.gameObject.name.ToLower();
                // Check if it represents the water meshes
                if (name.Contains("jet") || name.Contains("spray") || name.Contains("stream") || name.Contains("spurt") || name.Contains("flow") || name.Contains("water"))
                {
                    // Exclude base, bowl, or lotus components
                    if (!name.Contains("base") && !name.Contains("lotus") && !name.Contains("bowl") && !name.Contains("flower"))
                    {
                        r.enabled = false;
                        Debug.Log("Deactivated static water mesh renderer: " + r.gameObject.name);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Visual_REPLACE_NguyenHue_Fountain_FBX not found in scene.");
        }

        // 3. Connect to ParticleRestoreEffect
        ParticleRestoreEffect restoreEffect = fountainEffects.GetComponent<ParticleRestoreEffect>();
        if (restoreEffect != null)
        {
            restoreEffect.particles = fountainEffects.GetComponentsInChildren<ParticleSystem>(true);
            EditorUtility.SetDirty(restoreEffect);
        }

        // Mark dirty and save
        EditorUtility.SetDirty(fountainEffects);
        if (outerPs != null) EditorUtility.SetDirty(outerPs);
        if (centralPs != null) EditorUtility.SetDirty(centralPs);
        if (fbxModel != null) EditorUtility.SetDirty(fbxModel);

        EditorSceneManager.MarkSceneDirty(scene);
        bool saveSuccess = EditorSceneManager.SaveScene(scene);
        Debug.Log("Nguyen Hue Fountain Polish Complete! Save Scene: " + saveSuccess);

        AssetDatabase.SaveAssets();
    }
}
