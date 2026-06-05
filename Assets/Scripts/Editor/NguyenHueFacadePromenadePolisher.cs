#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHueFacadePromenadePolisher
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunNguyenHueFacadePromenadePolisher.flag";

    private const string BuildingMaterialFolder = "Assets/Art/Materials/NguyenHue/Buildings";
    private const string PromenadeMaterialFolder = "Assets/Art/Materials/NguyenHue/Promenade";

    private const float FloorLength = 96f;
    private const float FloorCenterZ = -4f;

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
            EnsureMaterials();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            PolishOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KyUcSaiGon] Nguyen Hue facades and promenade polished from pending flag.");
        };
    }

    [MenuItem("Ky Uc Sai Gon/Polish Nguyen Hue Facades And Promenade")]
    public static void PolishNguyenHueFacadesAndPromenade()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue facade/promenade polish cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        EnsureMaterials();
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PolishOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (shouldRestoreOpenScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[KyUcSaiGon] Nguyen Hue facades and promenade polished.");
    }

    private static void PolishOpenScene()
    {
        Transform environmentRoot = FindOrCreateScenePath("SceneBlockoutRoot/NguyenHue_EnvironmentRoot");
        Transform groundRoot = FindOrCreateChild(environmentRoot, "Ground_TileModules");
        Transform landmarkRoot = FindOrCreateChild(environmentRoot, "Landmark_Backdrop");

        HideLegacyFloorVisuals(groundRoot);
        HideLegacySideBuildingVisuals();
        EnsureGroundCollider(groundRoot);
        BuildPromenade(ResetChild(groundRoot, "PromenadePolishRoot"));
        BuildSideFacades(ResetChild(landmarkRoot, "SideFacadePolishRoot"));
    }

    private static void BuildPromenade(Transform root)
    {
        Material center = LoadMaterial(PromenadeMaterialFolder + "/M_Promenade_CenterLight.mat");
        Material side = LoadMaterial(PromenadeMaterialFolder + "/M_Promenade_SideGray.mat");
        Material accent = LoadMaterial(PromenadeMaterialFolder + "/M_Promenade_AccentWarm.mat");
        Material edge = LoadMaterial(PromenadeMaterialFolder + "/M_Promenade_EdgeDark.mat");

        CreateCube("Visual_REPLACE_Promenade_CenterBoulevard", root, new Vector3(0f, 0.03f, FloorCenterZ), Vector3.zero, new Vector3(14f, 0.06f, FloorLength), center, false);
        CreateCube("Visual_REPLACE_Promenade_LeftSideWalk", root, new Vector3(-13.8f, 0.025f, FloorCenterZ), Vector3.zero, new Vector3(12.8f, 0.05f, FloorLength), side, false);
        CreateCube("Visual_REPLACE_Promenade_RightSideWalk", root, new Vector3(13.8f, 0.025f, FloorCenterZ), Vector3.zero, new Vector3(12.8f, 0.05f, FloorLength), side, false);

        for (int i = 0; i < 12; i++)
        {
            float z = -48f + i * 8f;
            CreateCube("Visual_REPLACE_Promenade_CenterPanel_" + i.ToString("00"), root, new Vector3(0f, 0.07f, z + 3.8f), Vector3.zero, new Vector3(13.4f, 0.025f, 0.08f), edge, false);
            CreateCube("Visual_REPLACE_Promenade_LeftPanel_" + i.ToString("00"), root, new Vector3(-13.8f, 0.06f, z + 3.8f), Vector3.zero, new Vector3(12f, 0.02f, 0.06f), edge, false);
            CreateCube("Visual_REPLACE_Promenade_RightPanel_" + i.ToString("00"), root, new Vector3(13.8f, 0.06f, z + 3.8f), Vector3.zero, new Vector3(12f, 0.02f, 0.06f), edge, false);
        }

        for (int sideSign = -1; sideSign <= 1; sideSign += 2)
        {
            CreateCube("Visual_REPLACE_Promenade_CenterGuide_" + (sideSign < 0 ? "L" : "R"), root, new Vector3(sideSign * 4.35f, 0.095f, FloorCenterZ), Vector3.zero, new Vector3(0.16f, 0.03f, FloorLength - 8f), accent, false);
            CreateCube("Visual_REPLACE_Promenade_Edge_" + (sideSign < 0 ? "L" : "R"), root, new Vector3(sideSign * 20.8f, 0.12f, FloorCenterZ), Vector3.zero, new Vector3(0.42f, 0.16f, FloorLength), edge, false);
        }

        CreateCube("Visual_REPLACE_Promenade_FountainPlaza_Base", root, new Vector3(0f, 0.085f, 12f), Vector3.zero, new Vector3(24f, 0.035f, 22f), center, false);
        CreateCube("Visual_REPLACE_Promenade_FountainPlaza_NorthLine", root, new Vector3(0f, 0.12f, 23f), Vector3.zero, new Vector3(24f, 0.035f, 0.22f), accent, false);
        CreateCube("Visual_REPLACE_Promenade_FountainPlaza_SouthLine", root, new Vector3(0f, 0.12f, 1f), Vector3.zero, new Vector3(24f, 0.035f, 0.22f), accent, false);
        CreateCube("Visual_REPLACE_Promenade_FountainPlaza_WestLine", root, new Vector3(-12f, 0.12f, 12f), Vector3.zero, new Vector3(0.22f, 0.035f, 22f), accent, false);
        CreateCube("Visual_REPLACE_Promenade_FountainPlaza_EastLine", root, new Vector3(12f, 0.12f, 12f), Vector3.zero, new Vector3(0.22f, 0.035f, 22f), accent, false);
    }

    private static void BuildSideFacades(Transform root)
    {
        Material cream = LoadMaterial(BuildingMaterialFolder + "/M_Facade_Cream.mat");
        Material warmGray = LoadMaterial(BuildingMaterialFolder + "/M_Facade_WarmGray.mat");
        Material blueGray = LoadMaterial(BuildingMaterialFolder + "/M_Facade_BlueGray.mat");
        Material windowDark = LoadMaterial(BuildingMaterialFolder + "/M_Window_Dark.mat");
        Material windowWarm = LoadMaterial(BuildingMaterialFolder + "/M_Window_WarmLit.mat");
        Material awning = LoadMaterial(BuildingMaterialFolder + "/M_Awning_Dark.mat");
        Material shopfront = LoadMaterial(BuildingMaterialFolder + "/M_Shopfront_Muted.mat");
        Material trim = LoadMaterial(BuildingMaterialFolder + "/M_Facade_Trim_Light.mat");

        BuildFacadeRow(root, "Left", -1, 90f, cream, warmGray, blueGray, windowDark, windowWarm, awning, shopfront, trim);
        BuildFacadeRow(root, "Right", 1, -90f, cream, warmGray, blueGray, windowDark, windowWarm, awning, shopfront, trim);
    }

    private static void BuildFacadeRow(Transform root, string sideName, int sideSign, float yaw, Material cream, Material warmGray, Material blueGray, Material windowDark, Material windowWarm, Material awning, Material shopfront, Material trim)
    {
        float[] zPositions = { -36f, -24f, -11f, 2f, 16f, 31f };
        float[] widths = { 10.5f, 12f, 11f, 13f, 10f, 12.5f };
        float[] heights = { 10.5f, 13f, 11.5f, 14.2f, 12f, 15f };
        Material[] bodyMaterials = { cream, warmGray, blueGray, warmGray, cream, blueGray };

        for (int i = 0; i < zPositions.Length; i++)
        {
            Transform building = FindOrCreateChild(root, sideName + "_FacadeBuilding_" + (i + 1).ToString("00"));
            building.localPosition = Vector3.zero;
            building.localRotation = Quaternion.identity;
            building.localScale = Vector3.one;

            float height = heights[i];
            float width = widths[i];
            float z = zPositions[i];
            float rowCenterX = sideSign * 27f;
            float frontX = sideSign * 24.75f;
            float bodyDepth = 4.2f;

            CreateCube("Visual_REPLACE_" + sideName + "_FacadeBody_" + (i + 1).ToString("00"), building, new Vector3(rowCenterX, height * 0.5f, z), new Vector3(0f, yaw, 0f), new Vector3(width, height, bodyDepth), bodyMaterials[i], true);
            CreateCube("Visual_REPLACE_" + sideName + "_Shopfront_" + (i + 1).ToString("00"), building, new Vector3(frontX, 1.7f, z), new Vector3(0f, yaw, 0f), new Vector3(width * 0.82f, 2.6f, 0.16f), shopfront, false);
            CreateCube("Visual_REPLACE_" + sideName + "_Awning_" + (i + 1).ToString("00"), building, new Vector3(frontX - sideSign * 0.42f, 3.25f, z), new Vector3(0f, yaw, 0f), new Vector3(width * 0.9f, 0.26f, 1.05f), awning, false);
            CreateCube("Visual_REPLACE_" + sideName + "_Cornice_" + (i + 1).ToString("00"), building, new Vector3(frontX - sideSign * 0.08f, height + 0.18f, z), new Vector3(0f, yaw, 0f), new Vector3(width * 1.04f, 0.34f, 0.38f), trim, false);
            CreateCube("Visual_REPLACE_" + sideName + "_MidLedge_" + (i + 1).ToString("00"), building, new Vector3(frontX - sideSign * 0.06f, 5.1f, z), new Vector3(0f, yaw, 0f), new Vector3(width * 0.95f, 0.16f, 0.28f), trim, false);

            AddWindowGrid(building, sideName, i, frontX - sideSign * 0.1f, yaw, z, width, height, windowDark, windowWarm);
        }
    }

    private static void AddWindowGrid(Transform parent, string sideName, int buildingIndex, float x, float yaw, float zCenter, float width, float height, Material windowDark, Material windowWarm)
    {
        int columns = Mathf.Max(2, Mathf.RoundToInt(width / 3.6f));
        int rows = Mathf.Clamp(Mathf.FloorToInt((height - 5.2f) / 2.1f), 2, 5);
        float startZ = zCenter - (columns - 1) * 1.65f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                bool warm = (row + col + buildingIndex) % 4 == 0;
                Material material = warm ? windowWarm : windowDark;
                Vector3 position = new Vector3(x, 5.9f + row * 1.75f, startZ + col * 3.3f);
                CreateCube("Visual_REPLACE_" + sideName + "_Window_" + (buildingIndex + 1).ToString("00") + "_" + row + "_" + col, parent, position, new Vector3(0f, yaw, 0f), new Vector3(1.2f, 1.05f, 0.12f), material, false);
            }
        }
    }

    private static void HideLegacyFloorVisuals(Transform groundRoot)
    {
        string[] childNames =
        {
            "MainPromenadeFloor",
            "CentralPathTiles",
            "SidePlazaTiles",
            "AccentGuideTiles",
            "FountainPlazaTiles",
            "DecorativeTileInlays"
        };

        foreach (string childName in childNames)
        {
            Transform child = groundRoot.Find(childName);
            if (child != null)
            {
                SetRenderersEnabled(child.gameObject, false);
            }
        }
    }

    private static void HideLegacySideBuildingVisuals()
    {
        for (int i = 1; i <= 6; i++)
        {
            SetRenderersEnabled("REPLACE_Landmark_NguyenHue_LeftBuilding_" + i, false);
            SetRenderersEnabled("REPLACE_Landmark_NguyenHue_RightBuilding_" + i, false);
        }
    }

    private static void EnsureGroundCollider(Transform groundRoot)
    {
        GameObject colliderObject = GameObject.Find("NguyenHue_GroundCollider");
        if (colliderObject == null)
        {
            colliderObject = new GameObject("NguyenHue_GroundCollider");
            colliderObject.transform.SetParent(groundRoot);
        }

        colliderObject.layer = 0;
        colliderObject.transform.position = new Vector3(0f, -0.16f, FloorCenterZ);
        colliderObject.transform.rotation = Quaternion.identity;
        colliderObject.transform.localScale = Vector3.one;

        BoxCollider collider = colliderObject.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = colliderObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = false;
        collider.center = Vector3.zero;
        collider.size = new Vector3(44f, 0.36f, FloorLength + 4f);
        SetRenderersEnabled(colliderObject, false);
        EditorUtility.SetDirty(colliderObject);
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 euler, Vector3 scale, Material material, bool keepCollider)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.eulerAngles = euler;
        cube.transform.localScale = scale;

        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;

        if (!keepCollider)
        {
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }

        return cube;
    }

    private static void EnsureMaterials()
    {
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "NguyenHue");
        EnsureFolder("Assets/Art/Materials/NguyenHue", "Buildings");
        EnsureFolder("Assets/Art/Materials/NguyenHue", "Promenade");

        CreateMaterial(BuildingMaterialFolder + "/M_Facade_Cream.mat", new Color(0.69f, 0.65f, 0.55f), 0.32f);
        CreateMaterial(BuildingMaterialFolder + "/M_Facade_WarmGray.mat", new Color(0.46f, 0.47f, 0.45f), 0.28f);
        CreateMaterial(BuildingMaterialFolder + "/M_Facade_BlueGray.mat", new Color(0.34f, 0.42f, 0.48f), 0.3f);
        CreateMaterial(BuildingMaterialFolder + "/M_Window_Dark.mat", new Color(0.055f, 0.075f, 0.09f), 0.55f);
        CreateMaterial(BuildingMaterialFolder + "/M_Window_WarmLit.mat", new Color(1f, 0.66f, 0.32f), 0.38f, true);
        CreateMaterial(BuildingMaterialFolder + "/M_Awning_Dark.mat", new Color(0.16f, 0.12f, 0.12f), 0.34f);
        CreateMaterial(BuildingMaterialFolder + "/M_Shopfront_Muted.mat", new Color(0.32f, 0.29f, 0.24f), 0.25f);
        CreateMaterial(BuildingMaterialFolder + "/M_Facade_Trim_Light.mat", new Color(0.74f, 0.7f, 0.6f), 0.24f);

        CreateMaterial(PromenadeMaterialFolder + "/M_Promenade_CenterLight.mat", new Color(0.62f, 0.61f, 0.56f), 0.28f);
        CreateMaterial(PromenadeMaterialFolder + "/M_Promenade_SideGray.mat", new Color(0.48f, 0.49f, 0.48f), 0.25f);
        CreateMaterial(PromenadeMaterialFolder + "/M_Promenade_AccentWarm.mat", new Color(0.82f, 0.57f, 0.24f), 0.22f);
        CreateMaterial(PromenadeMaterialFolder + "/M_Promenade_EdgeDark.mat", new Color(0.22f, 0.22f, 0.2f), 0.22f);
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

        SetMaterialColor(material, "_BaseColor", color);
        SetMaterialColor(material, "_Color", color);
        SetMaterialFloat(material, "_Smoothness", smoothness);
        SetMaterialFloat(material, "_Metallic", 0f);

        if (emission)
        {
            material.EnableKeyword("_EMISSION");
            SetMaterialColor(material, "_EmissionColor", color * 0.75f);
        }

        EditorUtility.SetDirty(material);
    }

    private static Material LoadMaterial(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(path);
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
        childObject.transform.SetParent(parent);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject.transform;
    }

    private static void SetRenderersEnabled(string objectName, bool enabled)
    {
        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            SetRenderersEnabled(target, enabled);
        }
    }

    private static void SetRenderersEnabled(GameObject target, bool enabled)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = enabled;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void SetMaterialColor(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetMaterialFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
#endif
