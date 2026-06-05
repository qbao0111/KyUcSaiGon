#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHueCityHallBackdropInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string ModelFolderPath = "Assets/Art/Models/NguyenHue/Backdrops";
    private const string ModelPath = "Assets/Art/Models/NguyenHue/Backdrops/nguyen_hue_city_hall_backdrop.fbx";
    private const string SourceTextureFolderPath = "Assets/Art/Models/NguyenHue/Backdrops/nguyen_hue_city_hall_backdrop.fbm";
    private const string MaterialFolderPath = "Assets/Art/Materials/NguyenHue/CityHall";
    private const string TextureFolderPath = "Assets/Art/Textures/NguyenHue/CityHall";
    private const string MaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_NguyenHue_CityHallBackdrop.mat";
    private const string FacadeMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Facade_Cream.mat";
    private const string TrimMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Trim_Ivory.mat";
    private const string RoofMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Roof_Terracotta.mat";
    private const string DoorsMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_DoorsWindows_DarkGreen.mat";
    private const string StoneMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Stone_Base.mat";
    private const string ShrubsMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Shrubs_Green.mat";
    private const string FlagRedMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Flag_Red.mat";
    private const string FlagStarMaterialPath = "Assets/Art/Materials/NguyenHue/CityHall/M_CityHall_Flag_Star_Yellow.mat";
    private const string PrefabFolderPath = "Assets/Art/Prefabs/NguyenHue";
    private const string PrefabPath = "Assets/Art/Prefabs/NguyenHue/PF_NguyenHue_CityHallBackdrop.prefab";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunNguyenHueCityHallBackdropInstaller.flag";

    private const string LandmarkRootName = "Landmark_Backdrop";
    private const string SceneInstanceName = "REPLACE_NguyenHue_CityHallBackdrop";
    private const string VisualChildName = "Visual_REPLACE_CityHallBackdrop_Model";
    private static readonly Vector3 ScenePosition = new Vector3(0f, 0f, 39f);
    private static readonly Quaternion SceneRotation = Quaternion.Euler(0f, 180f, 0f);
    // Match the manually approved scene scale of 3.3x, but bake it into the prefab visual.
    // The scene instance root remains scale 1,1,1 for cleaner replacement later.
    private static readonly Vector3 TargetVisualSize = new Vector3(79.2f, 42.9f, 13.2f);

    [InitializeOnLoadMethod]
    private static void AutoRunIfRequested()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(AutoRunFlagPath))
            {
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            EnsureAssets();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyToOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KyUcSaiGon] Nguyen Hue City Hall backdrop installed from pending flag.");
        };
    }

    [MenuItem("Ky Uc Sai Gon/Apply Nguyen Hue City Hall Backdrop Model")]
    public static void ApplyCityHallBackdrop()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue City Hall backdrop install cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        EnsureAssets();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ApplyToOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (shouldRestoreOpenScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[KyUcSaiGon] Nguyen Hue City Hall backdrop installed.");
    }

    private static void EnsureAssets()
    {
        EnsureFolder("Assets/Art", "Models");
        EnsureFolder("Assets/Art/Models", "NguyenHue");
        EnsureFolder("Assets/Art/Models/NguyenHue", "Backdrops");
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "NguyenHue");
        EnsureFolder("Assets/Art/Materials/NguyenHue", "CityHall");
        EnsureFolder("Assets/Art", "Textures");
        EnsureFolder("Assets/Art/Textures", "NguyenHue");
        EnsureFolder("Assets/Art/Textures/NguyenHue", "CityHall");
        EnsureFolder("Assets/Art", "Prefabs");
        EnsureFolder("Assets/Art/Prefabs", "NguyenHue");

        ConfigureModelImporter();
        CopyExtractedTexturesToProjectFolder();
        ConfigureTextureImporters();
        Material material = CreateOrUpdateMaterial();
        CreateFallbackMaterials();
        CreateOrUpdatePrefab(material);
    }

    private static void ConfigureModelImporter()
    {
        ModelImporter importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing City Hall model at " + ModelPath);
            return;
        }

        bool dirty = false;
        if (importer.materialImportMode != ModelImporterMaterialImportMode.ImportStandard)
        {
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            dirty = true;
        }

        if (importer.materialLocation != ModelImporterMaterialLocation.External)
        {
            importer.materialLocation = ModelImporterMaterialLocation.External;
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }

        try
        {
            importer.ExtractTextures(TextureFolderPath);
        }
        catch (System.Exception exception)
        {
            Debug.Log("[KyUcSaiGon] City Hall embedded texture extraction skipped: " + exception.Message);
        }
    }

    private static Material CreateOrUpdateMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        Texture baseMap = FindTexture("texture_pbr_20250901.png");
        Texture normalMap = FindTexture("texture_pbr_20250901_normal.png");

        if (baseMap != null)
        {
            SetTexture(material, "_BaseMap", baseMap);
            SetTexture(material, "_MainTex", baseMap);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
        }
        else
        {
            SetColor(material, "_BaseColor", new Color(0.86f, 0.77f, 0.52f));
            SetColor(material, "_Color", new Color(0.86f, 0.77f, 0.52f));
        }

        if (normalMap != null)
        {
            SetTexture(material, "_BumpMap", normalMap);
            material.EnableKeyword("_NORMALMAP");
        }
        else
        {
            SetTexture(material, "_BumpMap", null);
            material.DisableKeyword("_NORMALMAP");
        }

        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.32f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateFallbackMaterials()
    {
        CreateOrUpdateColorMaterial(FacadeMaterialPath, HtmlColor("E3C985"), 0.32f);
        CreateOrUpdateColorMaterial(TrimMaterialPath, HtmlColor("F0E7D8"), 0.32f);
        CreateOrUpdateColorMaterial(RoofMaterialPath, HtmlColor("C76528"), 0.38f);
        CreateOrUpdateColorMaterial(DoorsMaterialPath, HtmlColor("1F3028"), 0.28f);
        CreateOrUpdateColorMaterial(StoneMaterialPath, HtmlColor("B6B0A4"), 0.3f);
        CreateOrUpdateColorMaterial(ShrubsMaterialPath, HtmlColor("4F6F2F"), 0.28f);
        CreateOrUpdateColorMaterial(FlagRedMaterialPath, HtmlColor("C8171E"), 0.35f);
        CreateOrUpdateColorMaterial(FlagStarMaterialPath, HtmlColor("FFD34D"), 0.35f);
    }

    private static Material CreateOrUpdateColorMaterial(string path, Color color, float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        SetTexture(material, "_BaseMap", null);
        SetTexture(material, "_MainTex", null);
        SetTexture(material, "_BumpMap", null);
        material.DisableKeyword("_NORMALMAP");
        SetColor(material, "_BaseColor", color);
        SetColor(material, "_Color", color);
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateOrUpdatePrefab(Material material)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (model == null)
        {
            return;
        }

        GameObject prefabRoot = new GameObject("PF_NguyenHue_CityHallBackdrop");
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
        visual.name = VisualChildName;
        visual.transform.SetParent(prefabRoot.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        AssignSmartMaterials(visual, material);
        RemoveColliders(visual);
        FitVisualToTarget(visual, TargetVisualSize);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);
    }

    private static void ApplyToOpenScene()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing prefab at " + PrefabPath);
            return;
        }

        Transform backdropRoot = FindOrCreateScenePath("SceneBlockoutRoot/NguyenHue_EnvironmentRoot/" + LandmarkRootName);
        HideOldPrimitiveBackdrop(backdropRoot);

        Transform existing = backdropRoot.Find(SceneInstanceName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = SceneInstanceName;
        instance.transform.SetParent(backdropRoot);
        instance.transform.position = ScenePosition;
        instance.transform.rotation = SceneRotation;
        instance.transform.localScale = Vector3.one;

        RemoveColliders(instance);
        EditorUtility.SetDirty(instance);
    }

    private static void HideOldPrimitiveBackdrop(Transform backdropRoot)
    {
        Transform old = backdropRoot.Find("Visual_REPLACE_CityHallBackdrop");
        if (old == null)
        {
            return;
        }

        Renderer[] renderers = old.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void FitVisualToTarget(GameObject visual, Vector3 targetSize)
    {
        Bounds bounds = CalculateWorldBounds(visual);
        if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f)
        {
            visual.transform.localScale = Vector3.one;
            return;
        }

        float scale = Mathf.Min(targetSize.x / bounds.size.x, targetSize.y / bounds.size.y);
        visual.transform.localScale = Vector3.one * scale;

        Bounds scaledBounds = CalculateWorldBounds(visual);
        Vector3 offset = -scaledBounds.center;
        offset.y = -scaledBounds.min.y;
        visual.transform.position += offset;
    }

    private static Bounds CalculateWorldBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void AssignSmartMaterials(GameObject root, Material textureMaterial)
    {
        Material facade = AssetDatabase.LoadAssetAtPath<Material>(FacadeMaterialPath);
        Material trim = AssetDatabase.LoadAssetAtPath<Material>(TrimMaterialPath);
        Material roof = AssetDatabase.LoadAssetAtPath<Material>(RoofMaterialPath);
        Material doors = AssetDatabase.LoadAssetAtPath<Material>(DoorsMaterialPath);
        Material stone = AssetDatabase.LoadAssetAtPath<Material>(StoneMaterialPath);
        Material shrubs = AssetDatabase.LoadAssetAtPath<Material>(ShrubsMaterialPath);
        Material flagRed = AssetDatabase.LoadAssetAtPath<Material>(FlagRedMaterialPath);
        Material flagStar = AssetDatabase.LoadAssetAtPath<Material>(FlagStarMaterialPath);

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                string hint = ((materials[i] != null ? materials[i].name : string.Empty) + " " + renderer.name).ToLowerInvariant();
                materials[i] = PickMaterialByNameHint(hint, textureMaterial, facade, trim, roof, doors, stone, shrubs, flagRed, flagStar);
            }

            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static Material PickMaterialByNameHint(string hint, Material textureMaterial, Material facade, Material trim, Material roof, Material doors, Material stone, Material shrubs, Material flagRed, Material flagStar)
    {
        if (hint.Contains("roof") || hint.Contains("mai") || hint.Contains("terracotta"))
        {
            return roof;
        }

        if (hint.Contains("window") || hint.Contains("door") || hint.Contains("cua") || hint.Contains("glass"))
        {
            return doors;
        }

        if (hint.Contains("tree") || hint.Contains("shrub") || hint.Contains("plant") || hint.Contains("green"))
        {
            return shrubs;
        }

        if (hint.Contains("stone") || hint.Contains("base") || hint.Contains("stair") || hint.Contains("plaza"))
        {
            return stone;
        }

        if (hint.Contains("flag") && (hint.Contains("star") || hint.Contains("yellow")))
        {
            return flagStar;
        }

        if (hint.Contains("flag"))
        {
            return flagRed;
        }

        if (hint.Contains("trim") || hint.Contains("ornament") || hint.Contains("ivory") || hint.Contains("white") || hint.Contains("column"))
        {
            return trim;
        }

        if (textureMaterial != null && HasBaseMap(textureMaterial))
        {
            return textureMaterial;
        }

        return facade;
    }

    private static bool HasBaseMap(Material material)
    {
        return (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
               || (material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null);
    }

    private static void CopyExtractedTexturesToProjectFolder()
    {
        if (!AssetDatabase.IsValidFolder(SourceTextureFolderPath))
        {
            return;
        }

        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { SourceTextureFolderPath });
        foreach (string guid in textureGuids)
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = TextureFolderPath + "/" + fileName;
            if (!File.Exists(destinationPath))
            {
                AssetDatabase.CopyAsset(sourcePath, destinationPath);
            }
        }

        AssetDatabase.Refresh();
    }

    private static void ConfigureTextureImporters()
    {
        ConfigureTextureImporter("texture_pbr_20250901.png", TextureImporterType.Default, true);
        ConfigureTextureImporter("texture_pbr_20250901_metallic.png", TextureImporterType.Default, false);
        ConfigureTextureImporter("texture_pbr_20250901_roughness.png", TextureImporterType.Default, false);
        ConfigureTextureImporter("texture_pbr_20250901_normal.png", TextureImporterType.NormalMap, false);
    }

    private static void ConfigureTextureImporter(string fileName, TextureImporterType textureType, bool useSrgb)
    {
        string path = FindTexturePath(fileName);
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

    private static Texture FindTexture(string fileName)
    {
        string path = FindTexturePath(fileName);
        return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<Texture>(path);
    }

    private static string FindTexturePath(string fileName)
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture", new[] { TextureFolderPath, SourceTextureFolderPath });
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path).Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }
        }

        return null;
    }

    private static Color HtmlColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    private static void RemoveColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static Transform FindOrCreateScenePath(string path)
    {
        string[] names = path.Split('/');
        Transform current = null;
        foreach (string name in names)
        {
            if (current == null)
            {
                GameObject root = GameObject.Find(name);
                if (root == null)
                {
                    root = new GameObject(name);
                }

                current = root.transform;
            }
            else
            {
                Transform child = current.Find(name);
                if (child == null)
                {
                    GameObject childObject = new GameObject(name);
                    child = childObject.transform;
                    child.SetParent(current);
                    child.localPosition = Vector3.zero;
                    child.localRotation = Quaternion.identity;
                    child.localScale = Vector3.one;
                }

                current = child;
            }
        }

        return current;
    }

    private static void EnsureFolder(string parentPath, string folderName)
    {
        string folderPath = parentPath + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }

    private static void SetColor(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static void SetTexture(Material material, string propertyName, Texture value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, value);
        }
    }
}
#endif
