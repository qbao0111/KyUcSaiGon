#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHueFacadeRowInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunNguyenHueFacadeRowInstaller.flag";

    private const string ModelFolder = "Assets/Art/Models/NguyenHue/Facades";
    private const string PrefabFolder = "Assets/Art/Prefabs/NguyenHue/Facades";
    private const string MaterialFolder = "Assets/Art/Materials/NguyenHue/Facades";
    private const string TextureFolder = "Assets/Art/Textures/NguyenHue/Facades";

    private const string LeftFbxPath = "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_left.fbx";
    private const string RightFbxPath = "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_right.fbx";
    private const string LeftGlbPath = "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_left.glb";
    private const string RightGlbPath = "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_right.glb";

    private const string LeftPrefabPath = "Assets/Art/Prefabs/NguyenHue/Facades/PF_NguyenHue_FacadeRow_Left.prefab";
    private const string RightPrefabPath = "Assets/Art/Prefabs/NguyenHue/Facades/PF_NguyenHue_FacadeRow_Right.prefab";

    private static readonly Vector3 TargetVisualSize = new Vector3(5f, 13.5f, 58f);

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
            EnsureAssetsAndPrefabs();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ApplyToOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KyUcSaiGon] Nguyen Hue facade row models installed from pending flag.");
        };
    }

    [MenuItem("Ky Uc Sai Gon/Install Nguyen Hue Facade Row Models")]
    public static void InstallNguyenHueFacadeRows()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue facade row install cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        EnsureAssetsAndPrefabs();
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

        Debug.Log("[KyUcSaiGon] Nguyen Hue facade row models installed.");
    }

    private static void EnsureAssetsAndPrefabs()
    {
        EnsureFolder("Assets/Art", "Models");
        EnsureFolder("Assets/Art/Models", "NguyenHue");
        EnsureFolder("Assets/Art/Models/NguyenHue", "Facades");
        EnsureFolder("Assets/Art", "Prefabs");
        EnsureFolder("Assets/Art/Prefabs", "NguyenHue");
        EnsureFolder("Assets/Art/Prefabs/NguyenHue", "Facades");
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "NguyenHue");
        EnsureFolder("Assets/Art/Materials/NguyenHue", "Facades");
        EnsureFolder("Assets/Art", "Textures");
        EnsureFolder("Assets/Art/Textures", "NguyenHue");
        EnsureFolder("Assets/Art/Textures/NguyenHue", "Facades");

        CreateFallbackMaterials();
        CreateTexturedFacadeMaterials();

        string leftModel = File.Exists(LeftFbxPath) ? LeftFbxPath : LeftGlbPath;
        string rightModel = File.Exists(RightFbxPath) ? RightFbxPath : RightGlbPath;

        ForceImportModel(leftModel);
        ForceImportModel(rightModel);

        CreateOrUpdatePrefab("PF_NguyenHue_FacadeRow_Left", "Visual_REPLACE_NguyenHue_FacadeRow_Left", leftModel, LeftPrefabPath);
        CreateOrUpdatePrefab("PF_NguyenHue_FacadeRow_Right", "Visual_REPLACE_NguyenHue_FacadeRow_Right", rightModel, RightPrefabPath);
    }

    private static void ApplyToOpenScene()
    {
        Transform environmentRoot = FindOrCreateScenePath("SceneBlockoutRoot/NguyenHue_EnvironmentRoot");
        Transform sideRoot = FindOrCreateChild(environmentRoot, "SideFacadeRoot");
        Transform rejectedRoot = FindOrCreateChild(sideRoot, "RejectedOrOld_FacadePlaceholders");

        MoveOldFacadePlaceholders(environmentRoot, rejectedRoot);
        rejectedRoot.gameObject.SetActive(false);

        Transform leftRoot = ResetChild(sideRoot, "FacadeRow_Left");
        Transform rightRoot = ResetChild(sideRoot, "FacadeRow_Right");

        GameObject leftPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeftPrefabPath);
        GameObject rightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RightPrefabPath);

        GameObject left = SpawnPrefab(leftPrefab, leftRoot, "REPLACE_NguyenHue_FacadeRow_Left");
        if (left != null)
        {
            left.transform.localPosition = new Vector3(-25.8f, 0f, -4f);
            left.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            left.transform.localScale = Vector3.one;
        AddSimpleBoundary(left, new Vector3(4.2f, 14f, 60f), new Vector3(0f, 7f, 0f));
        }

        GameObject right = SpawnPrefab(rightPrefab, rightRoot, "REPLACE_NguyenHue_FacadeRow_Right");
        if (right != null)
        {
            right.transform.localPosition = new Vector3(25.8f, 0f, -4f);
            right.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
            right.transform.localScale = Vector3.one;
            AddSimpleBoundary(right, new Vector3(4.2f, 14f, 60f), new Vector3(0f, 7f, 0f));
        }

        Debug.Log("[KyUcSaiGon] Facade row left final transform: pos (-25.8, 0, -4), rot (0, 0, 0), scale (1, 1, 1).");
        Debug.Log("[KyUcSaiGon] Facade row right final transform: pos (25.8, 0, -4), rot (0, 180, 0), scale (1, 1, 1).");
    }

    private static GameObject SpawnPrefab(GameObject prefab, Transform parent, string name)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing facade row prefab for " + name);
            return null;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        RemoveRigidbodies(instance);
        RemoveMeshColliders(instance);
        EditorUtility.SetDirty(instance);
        return instance;
    }

    private static void CreateOrUpdatePrefab(string prefabName, string visualName, string modelPath, string prefabPath)
    {
        GameObject model = LoadModelRoot(modelPath);
        if (model == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing facade row model root at " + modelPath + ". Subassets: " + DescribeSubassets(modelPath));
            return;
        }

        GameObject prefabRoot = new GameObject(prefabName);
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
        visual.name = visualName;
        visual.transform.SetParent(prefabRoot.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        RemoveRigidbodies(visual);
        RemoveMeshColliders(visual);
        AssignFallbackMaterialsIfMissing(visual);
        AssignTexturedFacadeMaterial(visual, visualName.Contains("Left") ? "Left" : "Right");
        RotateLongAxisToZ(visual);
        FitVisualToTarget(visual, TargetVisualSize);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        Object.DestroyImmediate(prefabRoot);
    }

    private static void RotateLongAxisToZ(GameObject visual)
    {
        Bounds bounds = CalculateWorldBounds(visual);
        if (bounds.size.x > bounds.size.z)
        {
            visual.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    private static void FitVisualToTarget(GameObject visual, Vector3 targetSize)
    {
        Bounds bounds = CalculateWorldBounds(visual);
        if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f || bounds.size.z <= 0.0001f)
        {
            return;
        }

        bool visualRotatedToZ = Mathf.Abs(Mathf.DeltaAngle(visual.transform.localEulerAngles.y, 90f)) < 1f;
        Vector3 fitScale = visualRotatedToZ
            ? new Vector3(targetSize.z / bounds.size.x, targetSize.y / bounds.size.y, targetSize.x / bounds.size.z)
            : new Vector3(targetSize.x / bounds.size.x, targetSize.y / bounds.size.y, targetSize.z / bounds.size.z);
        visual.transform.localScale = Vector3.Scale(visual.transform.localScale, fitScale);

        for (int i = 0; i < 3; i++)
        {
            Bounds currentBounds = CalculateWorldBounds(visual);
            if (visualRotatedToZ)
            {
                visual.transform.localScale = Vector3.Scale(
                    visual.transform.localScale,
                    new Vector3(targetSize.z / currentBounds.size.z, targetSize.y / currentBounds.size.y, targetSize.x / currentBounds.size.x));
            }
            else
            {
                visual.transform.localScale = Vector3.Scale(
                    visual.transform.localScale,
                    new Vector3(targetSize.x / currentBounds.size.x, targetSize.y / currentBounds.size.y, targetSize.z / currentBounds.size.z));
            }
        }

        Bounds scaledBounds = CalculateWorldBounds(visual);
        Vector3 offset = -scaledBounds.center;
        offset.y = -scaledBounds.min.y;
        visual.transform.position += offset;

        Bounds finalBounds = CalculateWorldBounds(visual);
        Debug.Log("[KyUcSaiGon] Facade row prefab " + visual.name + " final bounds: " + finalBounds.size);
    }

    private static void MoveOldFacadePlaceholders(Transform environmentRoot, Transform rejectedRoot)
    {
        MoveChildIfExists(environmentRoot, "SideFacadePolishRoot", rejectedRoot);

        Transform landmark = environmentRoot.Find("Landmark_Backdrop");
        if (landmark != null)
        {
            MoveChildIfExists(landmark, "SideFacadePolishRoot", rejectedRoot);
        }

        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject candidate in allObjects)
        {
            if (candidate == null || candidate.transform == rejectedRoot || candidate.transform.IsChildOf(rejectedRoot))
            {
                continue;
            }

            string name = candidate.name;
            bool isOldFacade = name.StartsWith("REPLACE_Landmark_NguyenHue_LeftBuilding_")
                               || name.StartsWith("REPLACE_Landmark_NguyenHue_RightBuilding_")
                               || name.StartsWith("Visual_REPLACE_Left_FacadeBody_")
                               || name.StartsWith("Visual_REPLACE_Right_FacadeBody_")
                               || name.StartsWith("Left_FacadeBuilding_")
                               || name.StartsWith("Right_FacadeBuilding_")
                               || name.StartsWith("LeftFacadeBuilding_")
                               || name.StartsWith("RightFacadeBuilding_");

            if (isOldFacade)
            {
                candidate.transform.SetParent(rejectedRoot);
                EditorUtility.SetDirty(candidate);
            }
        }
    }

    private static void MoveChildIfExists(Transform parent, string childName, Transform destination)
    {
        Transform child = parent.Find(childName);
        if (child != null && child != destination && !child.IsChildOf(destination))
        {
            child.SetParent(destination);
            EditorUtility.SetDirty(child);
        }
    }

    private static void AddSimpleBoundary(GameObject target, Vector3 size, Vector3 center)
    {
        BoxCollider collider = target.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = target.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.size = size;
        collider.center = center;
        EditorUtility.SetDirty(collider);
    }

    private static void ForceImportModel(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            Debug.LogWarning("[KyUcSaiGon] Missing facade row file at " + modelPath);
            return;
        }

        ModelImporter modelImporter = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (modelImporter != null && Mathf.Abs(modelImporter.globalScale - 1000f) > 0.01f)
        {
            modelImporter.globalScale = 1000f;
            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;
            modelImporter.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
    }

    private static GameObject LoadModelRoot(string modelPath)
    {
        GameObject direct = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (direct != null)
        {
            return direct;
        }

        Object[] subassets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        foreach (Object subasset in subassets)
        {
            if (subasset is GameObject gameObject)
            {
                return gameObject;
            }
        }

        return null;
    }

    private static string DescribeSubassets(string modelPath)
    {
        Object[] subassets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        if (subassets == null || subassets.Length == 0)
        {
            return "none";
        }

        string description = string.Empty;
        foreach (Object subasset in subassets)
        {
            if (subasset == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(description))
            {
                description += ", ";
            }

            description += subasset.GetType().Name + ":" + subasset.name;
        }

        return description;
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

    private static void AssignFallbackMaterialsIfMissing(GameObject visual)
    {
        Material fallback = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_FacadeRow_WarmConcrete.mat");
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool dirty = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null)
                {
                    materials[i] = fallback;
                    dirty = true;
                }
                else
                {
                    NormalizeImportedMaterial(materials[i]);
                }
            }

            if (dirty)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }
    }

    private static void NormalizeImportedMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null && material.shader != urpLit)
        {
            material.shader = urpLit;
        }

        Texture baseTexture = null;
        if (material.HasProperty("_BaseMap"))
        {
            baseTexture = material.GetTexture("_BaseMap");
        }

        if (baseTexture == null && material.HasProperty("_MainTex"))
        {
            baseTexture = material.GetTexture("_MainTex");
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", baseTexture);
            }
        }

        if (baseTexture != null)
        {
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
            ImproveTextureImport(baseTexture);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        SetFloat(material, "_Smoothness", 0.18f);
        EditorUtility.SetDirty(material);
    }

    private static void ImproveTextureImport(Texture texture)
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(texturePath))
        {
            return;
        }

        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool dirty = false;
        if (importer.textureType != TextureImporterType.Default)
        {
            importer.textureType = TextureImporterType.Default;
            dirty = true;
        }

        if (!importer.sRGBTexture)
        {
            importer.sRGBTexture = true;
            dirty = true;
        }

        if (importer.maxTextureSize < 4096)
        {
            importer.maxTextureSize = 4096;
            dirty = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            dirty = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }
    }

    private static void CreateFallbackMaterials()
    {
        CreateMaterial(MaterialFolder + "/M_FacadeRow_WarmConcrete.mat", new Color(0.6f, 0.55f, 0.47f), 0.28f);
        CreateMaterial(MaterialFolder + "/M_FacadeRow_WindowDark.mat", new Color(0.045f, 0.06f, 0.075f), 0.55f);
        CreateMaterial(MaterialFolder + "/M_FacadeRow_WindowWarmLit.mat", new Color(1f, 0.64f, 0.3f), 0.35f, true);
        CreateMaterial(MaterialFolder + "/M_FacadeRow_AwningMuted.mat", new Color(0.23f, 0.18f, 0.16f), 0.32f);
        CreateMaterial(MaterialFolder + "/M_FacadeRow_PlantGreen.mat", new Color(0.25f, 0.42f, 0.18f), 0.25f);
    }

    private static void CreateTexturedFacadeMaterials()
    {
        CreateTexturedFacadeMaterial("Left");
        CreateTexturedFacadeMaterial("Right");
    }

    private static void CreateTexturedFacadeMaterial(string side)
    {
        string materialPath = MaterialFolder + "/M_NguyenHue_FacadeRow_" + side + ".mat";
        string baseMapPath = TextureFolder + "/T_NguyenHue_FacadeRow_" + side + "_BaseColor.png";
        string normalMapPath = TextureFolder + "/T_NguyenHue_FacadeRow_" + side + "_Normal.png";
        string metallicPath = TextureFolder + "/T_NguyenHue_FacadeRow_" + side + "_Metallic.png";
        string roughnessPath = TextureFolder + "/T_NguyenHue_FacadeRow_" + side + "_Roughness.png";

        ConfigureFacadeTexture(baseMapPath, TextureImporterType.Default, true);
        ConfigureFacadeTexture(normalMapPath, TextureImporterType.NormalMap, false);
        ConfigureFacadeTexture(metallicPath, TextureImporterType.Default, false);
        ConfigureFacadeTexture(roughnessPath, TextureImporterType.Default, false);

        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, materialPath);
        }

        Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(baseMapPath);
        Texture2D normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);

        if (baseMap != null)
        {
            material.SetTexture("_BaseMap", baseMap);
            material.SetTexture("_MainTex", baseMap);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
        }

        if (normalMap != null)
        {
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_BumpMap", normalMap);
            SetFloat(material, "_BumpScale", 0.55f);
        }

        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.18f);
        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        EditorUtility.SetDirty(material);
    }

    private static void ConfigureFacadeTexture(string texturePath, TextureImporterType textureType, bool srgb)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
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

        if (importer.sRGBTexture != srgb)
        {
            importer.sRGBTexture = srgb;
            dirty = true;
        }

        if (importer.maxTextureSize < 4096)
        {
            importer.maxTextureSize = 4096;
            dirty = true;
        }

        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            dirty = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            dirty = true;
        }

        if (dirty)
        {
            importer.SaveAndReimport();
        }
    }

    private static void AssignTexturedFacadeMaterial(GameObject visual, string side)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/M_NguyenHue_FacadeRow_" + side + ".mat");
        if (material == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing textured facade material for " + side);
            return;
        }

        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                materials = new Material[] { material };
            }
            else
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
            }

            renderer.sharedMaterials = materials;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void CreateMaterial(string path, Color color, float smoothness, bool emission = false)
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

        SetColor(material, "_BaseColor", color);
        SetColor(material, "_Color", color);
        SetFloat(material, "_Smoothness", smoothness);
        SetFloat(material, "_Metallic", 0f);
        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            SetColor(material, "_EmissionColor", color * 0.75f);
        }

        EditorUtility.SetDirty(material);
    }

    private static void RemoveRigidbodies(GameObject root)
    {
        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            Object.DestroyImmediate(rigidbody);
        }
    }

    private static void RemoveMeshColliders(GameObject root)
    {
        MeshCollider[] colliders = root.GetComponentsInChildren<MeshCollider>(true);
        foreach (MeshCollider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static Transform ResetChild(Transform parent, string childName)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        return FindOrCreateChild(parent, childName);
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
                current = FindOrCreateChild(current, name);
            }
        }

        return current;
    }

    private static Transform FindOrCreateChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        child = childObject.transform;
        child.SetParent(parent);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
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
}
#endif
