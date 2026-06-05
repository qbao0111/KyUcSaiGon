#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHueFountainModelInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string FountainModelPath = "Assets/Art/Models/NguyenHue/Fountain/Fountain.fbx";
    private const string FountainFolderPath = "Assets/Art/Models/NguyenHue/Fountain";
    private const string ExtractedTextureFolderPath = "Assets/Art/Models/NguyenHue/Fountain/output.fbm";
    private const string ExtractedMaterialFolderPath = "Assets/Art/Models/NguyenHue/Fountain/Materials";
    private const string RuntimeMaterialPath = "Assets/Art/Models/NguyenHue/Fountain/Materials/MAT_NguyenHue_Fountain_URP.mat";
    private const string FountainRootName = "REPLACE_Landmark_NguyenHue_Fountain";
    private const string FountainVisualRootName = "Visual_REPLACE_NguyenHue_Fountain";
    private const string InstalledModelName = "Visual_REPLACE_NguyenHue_Fountain_FBX";

    [MenuItem("Ky Uc Sai Gon/Apply Nguyen Hue Fountain Model")]
    public static void ApplyToNguyenHueScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue fountain install cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PrepareFountainMaterials();
        ApplyToOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (shouldRestoreOpenScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[KyUcSaiGon] Nguyen Hue fountain FBX installed.");
    }

    public static void ApplyToOpenScene()
    {
        PrepareFountainMaterials();

        GameObject fountainRoot = GameObject.Find(FountainRootName);
        if (fountainRoot == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing " + FountainRootName + " in Nguyen Hue scene.");
            return;
        }

        Transform visualRoot = fountainRoot.transform.Find(FountainVisualRootName);
        if (visualRoot == null)
        {
            GameObject visualRootObject = new GameObject(FountainVisualRootName);
            visualRoot = visualRootObject.transform;
            visualRoot.SetParent(fountainRoot.transform);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }

        RemoveOldInstalledModel(visualRoot);
        HidePrimitivePlaceholderChildren(visualRoot);

        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FountainModelPath);
        if (modelPrefab == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing fountain FBX at " + FountainModelPath);
            return;
        }

        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        modelInstance.name = InstalledModelName;
        modelInstance.transform.SetParent(visualRoot);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;

        RemoveGameplayCollidersFromVisual(modelInstance);
        ApplyFountainMaterial(modelInstance);
        FitModelToPlaceholder(modelInstance, visualRoot);
        EditorUtility.SetDirty(fountainRoot);
    }

    [MenuItem("Ky Uc Sai Gon/Extract Nguyen Hue Fountain Textures")]
    public static void PrepareFountainMaterialsMenu()
    {
        PrepareFountainMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Nguyen Hue fountain material setup complete.");
    }

    private static void PrepareFountainMaterials()
    {
        EnsureFolder(FountainFolderPath, "Materials");
        EnsureFolder(FountainFolderPath, "output.fbm");

        ModelImporter importer = AssetImporter.GetAtPath(FountainModelPath) as ModelImporter;
        if (importer != null)
        {
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.External;
            importer.materialSearch = ModelImporterMaterialSearch.Local;
            importer.SaveAndReimport();

            TryExtractModelTextures(importer, ExtractedTextureFolderPath);
        }

        Material fountainMaterial = CreateOrUpdateRuntimeMaterial();
        if (fountainMaterial != null)
        {
            EditorUtility.SetDirty(fountainMaterial);
        }
    }

    private static void EnsureFolder(string parentPath, string folderName)
    {
        string folderPath = parentPath + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }

    private static void TryExtractModelTextures(ModelImporter importer, string folderPath)
    {
        try
        {
            importer.ExtractTextures(folderPath);
        }
        catch (System.Exception exception)
        {
            // Some FBX files only reference external textures instead of embedding them.
            Debug.Log("[KyUcSaiGon] Could not extract fountain textures: " + exception.Message);
        }
    }

    private static Material CreateOrUpdateRuntimeMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
        }

        ConfigureTextureImporters();

        Texture baseMap = FindExactTexture("texture_pbr_20250901.png");
        Texture normalMap = FindExactTexture("texture_pbr_20250901_normal.png");
        Texture metallicMap = FindExactTexture("texture_pbr_20250901_metallic.png");

        if (baseMap != null)
        {
            SetTexture(material, "_BaseMap", baseMap);
            SetTexture(material, "_MainTex", baseMap);
        }

        if (normalMap != null)
        {
            SetTexture(material, "_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
        }

        if (metallicMap != null)
        {
            SetTexture(material, "_MetallicGlossMap", metallicMap);
        }

        SetColor(material, "_BaseColor", Color.white);
        SetColor(material, "_Color", Color.white);
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.35f);
        return material;
    }

    private static void ConfigureTextureImporters()
    {
        ConfigureTextureImporter("texture_pbr_20250901.png", TextureImporterType.Default, true);
        ConfigureTextureImporter("texture_pbr_20250901_normal.png", TextureImporterType.NormalMap, false);
        ConfigureTextureImporter("texture_pbr_20250901_metallic.png", TextureImporterType.Default, false);
        ConfigureTextureImporter("texture_pbr_20250901_roughness.png", TextureImporterType.Default, false);
    }

    private static void ConfigureTextureImporter(string fileName, TextureImporterType textureType, bool useSrgb)
    {
        string path = FindExactTexturePath(fileName);
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool dirty = false;
        if (importer.textureType != textureType)
        {
            importer.textureType = textureType;
            dirty = true;
        }

        if (importer.sRGBTexture != useSrgb)
        {
            importer.sRGBTexture = useSrgb;
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }
    }

    private static Texture FindExactTexture(string fileName)
    {
        string path = FindExactTexturePath(fileName);
        return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<Texture>(path);
    }

    private static string FindExactTexturePath(string fileName)
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { FountainFolderPath });
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileName(path).Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }

        return null;
    }

    private static void ApplyFountainMaterial(GameObject modelInstance)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath);
        if (material == null)
        {
            return;
        }

        Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
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

    private static void RemoveOldInstalledModel(Transform visualRoot)
    {
        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);
            if (child.name.StartsWith(InstalledModelName))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void HidePrimitivePlaceholderChildren(Transform visualRoot)
    {
        for (int i = 0; i < visualRoot.childCount; i++)
        {
            Transform child = visualRoot.GetChild(i);
            if (child.name.StartsWith(InstalledModelName))
            {
                continue;
            }

            MeshRenderer[] renderers = child.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }
        }
    }

    private static void RemoveGameplayCollidersFromVisual(GameObject modelInstance)
    {
        Collider[] colliders = modelInstance.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void FitModelToPlaceholder(GameObject modelInstance, Transform visualRoot)
    {
        Renderer[] renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.z);
        if (maxSize > 0.01f)
        {
            float targetDiameter = 10f;
            float scale = targetDiameter / maxSize;
            modelInstance.transform.localScale = Vector3.one * scale;
        }

        Renderer[] scaledRenderers = modelInstance.GetComponentsInChildren<Renderer>(true);
        if (scaledRenderers.Length == 0)
        {
            return;
        }

        Bounds scaledBounds = scaledRenderers[0].bounds;
        for (int i = 1; i < scaledRenderers.Length; i++)
        {
            scaledBounds.Encapsulate(scaledRenderers[i].bounds);
        }

        Vector3 visualRootPosition = visualRoot.position;
        Vector3 offset = visualRootPosition - new Vector3(scaledBounds.center.x, scaledBounds.min.y, scaledBounds.center.z);
        modelInstance.transform.position += offset;
    }
}
#endif
