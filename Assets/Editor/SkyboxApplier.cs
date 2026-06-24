using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public class SkyboxApplier
{
    private const string SkyboxTexturePath = "Assets/Art/Skybox/Scene_-_Root_diffuse.jpg";

    static SkyboxApplier()
    {
        EditorApplication.delayCall += RunOnce;
    }

    private static void RunOnce()
    {
        string sessionKey = "EndingSkyboxApplied_v1";
        if (SessionState.GetBool(sessionKey, false))
        {
            return;
        }
        SessionState.SetBool(sessionKey, true);

        ApplySkyboxToEnding();
    }

    [MenuItem("KyUcSaiGon/Apply Skybox")]
    public static void ApplySkyboxToAll()
    {
        Debug.Log("Starting Skybox Applier...");
        string modelPath = "Assets/Art/Skybox/skybox.glb";
        GameObject skyboxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (skyboxPrefab == null)
        {
            Debug.LogError("skybox.glb not found at " + modelPath);
            return;
        }

        string[] scenes = new string[]
        {
            "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity",
            "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
            "Assets/Scenes/Scene_07_Ending.unity"
        };

        foreach (string scenePath in scenes)
        {
            ApplyToScene(scenePath, skyboxPrefab);
        }

        Debug.Log("Skybox Applier complete!");
    }

    [MenuItem("KyUcSaiGon/Apply Skybox To Ending")]
    public static void ApplySkyboxToEnding()
    {
        Debug.Log("Starting Ending Skybox Applier...");
        string modelPath = "Assets/Art/Skybox/skybox.glb";
        GameObject skyboxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (skyboxPrefab == null)
        {
            Debug.LogError("skybox.glb not found at " + modelPath);
            return;
        }

        ApplyToScene("Assets/Scenes/Scene_07_Ending.unity", skyboxPrefab);
        Debug.Log("Ending Skybox Applier complete!");
    }

    private static void ApplyToScene(string scenePath, GameObject skyboxPrefab)
    {
        if (!File.Exists(scenePath))
        {
            Debug.LogWarning("Scene file does not exist: " + scenePath);
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
        {
            Debug.LogError("Failed to open scene: " + scenePath);
            return;
        }

        // Find existing skybox object and destroy it
        GameObject existing = GameObject.Find("REPLACE_Skybox");
        if (existing == null)
        {
            existing = GameObject.Find("Skybox");
        }
        if (existing != null)
        {
            Debug.Log($"Removing existing skybox object in {scenePath}");
            Object.DestroyImmediate(existing);
        }

        // Instantiate
        GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(skyboxPrefab);
        if (inst == null)
        {
            Debug.LogError("Failed to instantiate skybox prefab in scene " + scenePath);
            return;
        }

        inst.name = "REPLACE_Skybox";
        inst.transform.position = Vector3.zero;
        inst.transform.rotation = Quaternion.Euler(-90f, 180f, 0f);
        inst.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f); // Scale down so lossy scale is 750 (within 1000 far clip plane)

        // Keep at scene root, matching Nguyen Hue and Duc Ba.
        Texture skyboxTexture = ExtractEmbeddedSkyboxTexture();
        ForceUnlitSkyboxMaterials(inst, skyboxTexture);

        if (scenePath.Contains("Scene_07_Ending"))
        {
            RenderSettings.fog = false;
        }

        // CRITICAL: Remove colliders recursively to prevent blocking player movement
        int colliderCount = 0;
        foreach (var col in inst.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(col);
            colliderCount++;
        }
        if (colliderCount > 0)
        {
            Debug.Log($"Removed {colliderCount} colliders from skybox in {scenePath}");
        }

        // Keep the skybox out of EndingSceneController.renderersToWarm.
        // That list is intentionally desaturated during the ending sequence, and it
        // makes the sky texture look like a flat gray/white sphere.
        if (scenePath.Contains("Scene_07_Ending"))
        {
            EndingSceneController controller = Object.FindFirstObjectByType<EndingSceneController>();
            if (controller != null)
            {
                var renderersList = new List<Renderer>();
                if (controller.renderersToWarm != null)
                {
                    foreach (var r in controller.renderersToWarm)
                    {
                        if (r != null && !r.transform.IsChildOf(inst.transform) && !renderersList.Contains(r))
                        {
                            renderersList.Add(r);
                        }
                    }
                }

                controller.renderersToWarm = renderersList.ToArray();
                EditorUtility.SetDirty(controller);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Successfully applied skybox to scene: " + scenePath);
    }

    private static Texture ExtractEmbeddedSkyboxTexture()
    {
        Texture existing = AssetDatabase.LoadAssetAtPath<Texture>(SkyboxTexturePath);
        if (existing != null)
        {
            return existing;
        }

        string glbPath = "Assets/Art/Skybox/skybox.glb";
        if (!File.Exists(glbPath))
        {
            Debug.LogWarning("Cannot extract skybox texture because skybox.glb is missing.");
            return null;
        }

        byte[] bytes = File.ReadAllBytes(glbPath);
        int start = -1;
        for (int i = 0; i < bytes.Length - 1; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0xD8)
            {
                start = i;
                break;
            }
        }

        int end = -1;
        for (int i = bytes.Length - 2; i > start; i--)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0xD9)
            {
                end = i + 2;
                break;
            }
        }

        if (start < 0 || end <= start)
        {
            Debug.LogWarning("Could not find embedded JPEG data in skybox.glb.");
            return null;
        }

        Directory.CreateDirectory("Assets/Art/Skybox");
        byte[] jpg = new byte[end - start];
        System.Array.Copy(bytes, start, jpg, 0, jpg.Length);
        File.WriteAllBytes(SkyboxTexturePath, jpg);
        AssetDatabase.ImportAsset(SkyboxTexturePath, ImportAssetOptions.ForceSynchronousImport);
        return AssetDatabase.LoadAssetAtPath<Texture>(SkyboxTexturePath);
    }

    private static void ForceUnlitSkyboxMaterials(GameObject skybox, Texture skyboxTexture)
    {
        string folder = "Assets/Art/Skybox";
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/Art", "Skybox");
        }

        Renderer[] renderers = skybox.GetComponentsInChildren<Renderer>(true);
        for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
        {
            Renderer renderer = renderers[rendererIndex];
            Material[] materials = renderer.sharedMaterials;
            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
            {
                Material source = materials[materialIndex];
                if (source == null)
                {
                    continue;
                }

                materials[materialIndex] = GetOrCreateUnlitMaterial(source, skyboxTexture, rendererIndex, materialIndex);
            }

            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);
        }

        AssetDatabase.SaveAssets();
    }

    private static Material GetOrCreateUnlitMaterial(Material source, Texture skyboxTexture, int rendererIndex, int materialIndex)
    {
        string materialPath = $"Assets/Art/Skybox/M_Skybox_Unlit_{rendererIndex}_{materialIndex}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Texture");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }

        Texture texture = skyboxTexture != null ? skyboxTexture : FindMainTexture(source);
        if (texture != null)
        {
            SetTexture(material, "_BaseMap", texture);
            SetTexture(material, "_MainTex", texture);
        }

        Color color = Color.white;
        SetColor(material, "_BaseColor", color);
        SetColor(material, "_Color", color);
        SetFloat(material, "_Surface", 0f);
        SetFloat(material, "_AlphaClip", 0f);
        SetFloat(material, "_Cull", 0f);
        SetFloat(material, "_ZWrite", 0f);
        material.doubleSidedGI = true;
        material.renderQueue = -1;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture FindMainTexture(Material material)
    {
        string[] names = { "_BaseMap", "_MainTex", "_EmissionMap", "_EmissionTexture", "baseColorTexture", "_BaseColorMap" };
        foreach (string name in names)
        {
            if (material.HasProperty(name))
            {
                Texture texture = material.GetTexture(name);
                if (texture != null)
                {
                    return texture;
                }
            }
        }

        return null;
    }

    private static Color FindMainColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            return material.GetColor("_BaseColor");
        }
        if (material.HasProperty("_Color"))
        {
            return material.GetColor("_Color");
        }

        return Color.white;
    }

    private static void SetTexture(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static void SetColor(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
