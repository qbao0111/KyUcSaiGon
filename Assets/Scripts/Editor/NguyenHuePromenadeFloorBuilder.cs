#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHuePromenadeFloorBuilder
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string MaterialFolderPath = "Assets/Art/Materials/NguyenHue";
    private const string LightPavementPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_Pavement_LightGray.mat";
    private const string DarkPavementPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_Pavement_DarkGray.mat";
    private const string SeamPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_Pavement_Seam.mat";
    private const string GoldAccentPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_GoldAccent.mat";
    private const string StoneEdgePath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_StoneEdge.mat";
    private const string CityHallPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_CityHall_WarmWhite.mat";
    private const string SideBuildingPath = "Assets/Art/Materials/NguyenHue/M_NguyenHue_SideBuilding_Dark.mat";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunNguyenHuePromenadeFloorBuilder.flag";

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
            BuildInOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KyUcSaiGon] Nguyen Hue promenade tile floor auto-built from pending flag.");
        };
    }

    [MenuItem("Ky Uc Sai Gon/Build Nguyen Hue Promenade Floor")]
    public static void BuildNguyenHuePromenadeFloor()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue promenade floor build cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        EnsureMaterials();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        BuildInOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (shouldRestoreOpenScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[KyUcSaiGon] Nguyen Hue promenade tile floor built.");
    }

    private static void BuildInOpenScene()
    {
        Transform environmentRoot = FindOrCreateRoot("NguyenHue_EnvironmentRoot");
        Transform groundRoot = FindOrCreateChild(environmentRoot, "Ground_TileModules");
        ClearChildren(groundRoot);

        HideOldGroundVisuals();

        Transform mainFloor = FindOrCreateChild(groundRoot, "MainPromenadeFloor");
        Transform centralPath = FindOrCreateChild(groundRoot, "CentralPathTiles");
        Transform sidePlaza = FindOrCreateChild(groundRoot, "SidePlazaTiles");
        Transform accents = FindOrCreateChild(groundRoot, "AccentGuideTiles");
        Transform fountainPlaza = FindOrCreateChild(groundRoot, "FountainPlazaTiles");
        Transform inlays = FindOrCreateChild(groundRoot, "DecorativeTileInlays");

        Material light = LoadMaterial(LightPavementPath);
        Material dark = LoadMaterial(DarkPavementPath);
        Material seam = LoadMaterial(SeamPath);
        Material gold = LoadMaterial(GoldAccentPath);
        Material edge = LoadMaterial(StoneEdgePath);

        CreateWalkableCollider(groundRoot);
        CreatePromenadeTiles(mainFloor, light, dark);
        CreateSidePlazaTiles(sidePlaza, light, dark);
        CreateCentralPathTiles(centralPath, dark);
        CreateGuideAccents(accents, gold);
        CreateFountainPlaza(fountainPlaza, light, dark, seam, gold);
        CreateSeams(inlays, seam);
        CreateStoneEdges(inlays, edge);
        LayoutNguyenHueAxis();
    }

    private static void HideOldGroundVisuals()
    {
        SetVisualActive("REPLACE_Environment_NguyenHue_Boulevard", false);
        SetVisualActive("REPLACE_Prop_NguyenHue_MainWalkingPath", false);
        SetVisualActive("REPLACE_Prop_NguyenHue_FountainPlaza", false);
    }

    private static void SetVisualActive(string objectName, bool isActive)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isActive;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void CreateWalkableCollider(Transform parent)
    {
        GameObject colliderObject = new GameObject("NguyenHue_GroundCollider");
        colliderObject.isStatic = true;
        colliderObject.transform.SetParent(parent);
        colliderObject.transform.localPosition = new Vector3(0f, 0f, -4f);
        BoxCollider collider = colliderObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(42f, 0.4f, 92f);
        collider.center = Vector3.zero;
    }

    private static void CreatePromenadeTiles(Transform parent, Material light, Material dark)
    {
        const float tileWidth = 4f;
        const float tileLength = 4f;
        int xCount = 11;
        int zCount = 23;
        float startX = -20f;
        float startZ = -48f;

        for (int x = 0; x < xCount; x++)
        {
            for (int z = 0; z < zCount; z++)
            {
                Vector3 position = new Vector3(startX + x * tileWidth, 0.035f, startZ + z * tileLength);
                Material material = ((x + z) % 3 == 0) ? dark : light;
                GameObject tile = CreateTile("Tile_Main_" + x.ToString("00") + "_" + z.ToString("00"), parent, position, new Vector3(3.88f, 0.06f, 3.88f), material);
                RemoveCollider(tile);
            }
        }
    }

    private static void CreateSidePlazaTiles(Transform parent, Material light, Material dark)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            for (int z = 0; z < 17; z++)
            {
                Vector3 position = new Vector3(side * 26f, 0.045f, -42f + z * 5f);
                Vector3 scale = new Vector3(10f, 0.045f, 4.8f);
                Material material = z % 2 == 0 ? light : dark;
                GameObject tile = CreateTile("Tile_SidePlaza_" + (side < 0 ? "L_" : "R_") + z.ToString("00"), parent, position, scale, material);
                RemoveCollider(tile);
            }
        }
    }

    private static void CreateCentralPathTiles(Transform parent, Material material)
    {
        for (int i = 0; i < 23; i++)
        {
            Vector3 position = new Vector3(0f, 0.065f, -48f + i * 4f);
            GameObject tile = CreateTile("Tile_CentralPath_" + i.ToString("00"), parent, position, new Vector3(4.6f, 0.07f, 3.6f), material);
            RemoveCollider(tile);
        }
    }

    private static void CreateGuideAccents(Transform parent, Material gold)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector3 position = new Vector3(0f, 0.105f, -44f + i * 5.2f);
            GameObject accent = CreateTile("GoldGuide_Inlay_" + i.ToString("00"), parent, position, new Vector3(0.6f, 0.045f, 1.65f), gold);
            RemoveCollider(accent);
        }

        CreateTile("GoldGuide_Fountain_Left", parent, new Vector3(-4.5f, 0.085f, 10f), new Vector3(7.5f, 0.04f, 0.32f), gold);
        CreateTile("GoldGuide_Fountain_Right", parent, new Vector3(4.5f, 0.085f, 10f), new Vector3(7.5f, 0.04f, 0.32f), gold);
        CreateTile("GoldGuide_Fountain_Front", parent, new Vector3(0f, 0.085f, 5.2f), new Vector3(0.32f, 0.04f, 7.2f), gold);
        CreateTile("GoldGuide_Fountain_Back", parent, new Vector3(0f, 0.085f, 14.8f), new Vector3(0.32f, 0.04f, 7.2f), gold);
    }

    private static void CreateFountainPlaza(Transform parent, Material light, Material dark, Material seam, Material gold)
    {
        for (int x = -3; x <= 3; x++)
        {
            for (int z = -3; z <= 3; z++)
            {
                if (Mathf.Abs(x) <= 1 && Mathf.Abs(z) <= 1)
                {
                    continue;
                }

                Vector3 position = new Vector3(x * 3.2f, 0.04f, 10f + z * 3.2f);
                Material material = (Mathf.Abs(x) + Mathf.Abs(z)) % 2 == 0 ? light : dark;
                GameObject tile = CreateTile("Tile_FountainPlaza_" + x + "_" + z, parent, position, new Vector3(3.05f, 0.055f, 3.05f), material);
                RemoveCollider(tile);
            }
        }

        CreateTile("FountainPlaza_ThinSeam_North", parent, new Vector3(0f, 0.09f, 20f), new Vector3(23f, 0.035f, 0.18f), seam);
        CreateTile("FountainPlaza_ThinSeam_South", parent, new Vector3(0f, 0.09f, 0f), new Vector3(23f, 0.035f, 0.18f), seam);
        CreateTile("FountainPlaza_ThinSeam_West", parent, new Vector3(-11.5f, 0.09f, 10f), new Vector3(0.18f, 0.035f, 20f), seam);
        CreateTile("FountainPlaza_ThinSeam_East", parent, new Vector3(11.5f, 0.09f, 10f), new Vector3(0.18f, 0.035f, 20f), seam);

        CreateTile("FountainPlaza_GoldCorner_NW", parent, new Vector3(-8.7f, 0.1f, 18.1f), new Vector3(2.2f, 0.045f, 0.32f), gold);
        CreateTile("FountainPlaza_GoldCorner_NE", parent, new Vector3(8.7f, 0.1f, 18.1f), new Vector3(2.2f, 0.045f, 0.32f), gold);
        CreateTile("FountainPlaza_GoldCorner_SW", parent, new Vector3(-8.7f, 0.1f, 1.9f), new Vector3(2.2f, 0.045f, 0.32f), gold);
        CreateTile("FountainPlaza_GoldCorner_SE", parent, new Vector3(8.7f, 0.1f, 1.9f), new Vector3(2.2f, 0.045f, 0.32f), gold);
    }

    private static void CreateSeams(Transform parent, Material seam)
    {
        for (int x = -20; x <= 20; x += 4)
        {
            GameObject line = CreateTile("Seam_Longitudinal_" + x, parent, new Vector3(x, 0.095f, -4f), new Vector3(0.08f, 0.03f, 88f), seam);
            RemoveCollider(line);
        }

        for (int z = -48; z <= 40; z += 4)
        {
            GameObject line = CreateTile("Seam_Cross_" + z, parent, new Vector3(0f, 0.096f, z), new Vector3(41f, 0.03f, 0.08f), seam);
            RemoveCollider(line);
        }
    }

    private static void CreateStoneEdges(Transform parent, Material edge)
    {
        CreateTile("StoneEdge_Left", parent, new Vector3(-21.2f, 0.13f, -4f), new Vector3(0.5f, 0.18f, 92f), edge);
        CreateTile("StoneEdge_Right", parent, new Vector3(21.2f, 0.13f, -4f), new Vector3(0.5f, 0.18f, 92f), edge);
        CreateTile("StoneEdge_Start", parent, new Vector3(0f, 0.13f, -50.2f), new Vector3(42f, 0.18f, 0.5f), edge);
        CreateTile("StoneEdge_End", parent, new Vector3(0f, 0.13f, 42.2f), new Vector3(42f, 0.18f, 0.5f), edge);
    }

    private static void LayoutNguyenHueAxis()
    {
        SetTransform("REPLACE_Player_Character", new Vector3(0f, 1f, -44f), Quaternion.identity);
        SetTransform("PlayerSpawn", new Vector3(0f, 1f, -44f), Quaternion.identity);

        SetTransform("REPLACE_Landmark_NguyenHue_Fountain", new Vector3(0f, 0f, 12f), Quaternion.identity);
        SetTransform("REPLACE_Puzzle_SpeakerMixer", new Vector3(-8.5f, 1f, 5.5f), FacePoint(new Vector3(-8.5f, 1f, 5.5f), new Vector3(0f, 1f, -22f)));
        SetTransform("REPLACE_NPC_StreetMusician", new Vector3(-12.5f, 1f, 5.8f), FacePoint(new Vector3(-12.5f, 1f, 5.8f), new Vector3(0f, 1f, -22f)));

        SetTransform("REPLACE_LEDHint_Bass_Red_1", new Vector3(-10.5f, 0f, 8f), FacePoint(new Vector3(-10.5f, 0f, 8f), new Vector3(0f, 0f, 0f)));
        SetTransform("REPLACE_LEDHint_Mid_Green_6", new Vector3(10.5f, 0f, 13f), FacePoint(new Vector3(10.5f, 0f, 13f), new Vector3(0f, 0f, 3f)));
        SetTransform("REPLACE_LEDHint_Treble_Gold_8", new Vector3(0f, 0f, 23f), FacePoint(new Vector3(0f, 0f, 23f), new Vector3(0f, 0f, 8f)));

        RepositionSideBuildings();
        BuildCityHallBackdrop();
    }

    private static void RepositionSideBuildings()
    {
        for (int i = 1; i <= 6; i++)
        {
            float z = -34f + (i - 1) * 13f;
            float height = 10f + (i % 3) * 2.5f;
            SetTransform("REPLACE_Landmark_NguyenHue_LeftBuilding_" + i, new Vector3(-27f, height * 0.5f, z), Quaternion.identity, new Vector3(5.5f, height, 11f));
            SetTransform("REPLACE_Landmark_NguyenHue_RightBuilding_" + i, new Vector3(27f, height * 0.5f, z), Quaternion.identity, new Vector3(5.5f, height, 11f));
        }
    }

    private static void BuildCityHallBackdrop()
    {
        Transform environmentRoot = FindOrCreateRoot("NguyenHue_EnvironmentRoot");
        Transform backdropRoot = FindOrCreateChild(environmentRoot, "Landmark_Backdrop");
        Transform cityHall = FindOrCreateChild(backdropRoot, "Visual_REPLACE_CityHallBackdrop");
        ClearChildren(cityHall);

        cityHall.localPosition = new Vector3(0f, 0f, 39f);
        cityHall.localRotation = Quaternion.identity;
        cityHall.localScale = Vector3.one;

        Material cityHallMaterial = LoadMaterial(CityHallPath);
        Material buildingMaterial = LoadMaterial(SideBuildingPath);
        Material gold = LoadMaterial(GoldAccentPath);

        CreateTile("Visual_REPLACE_CityHall_MainBlock", cityHall, new Vector3(0f, 4.2f, 0f), new Vector3(16f, 8.4f, 2.2f), cityHallMaterial);
        CreateTile("Visual_REPLACE_CityHall_LeftWing", cityHall, new Vector3(-11f, 3f, 0.4f), new Vector3(6f, 6f, 1.8f), cityHallMaterial);
        CreateTile("Visual_REPLACE_CityHall_RightWing", cityHall, new Vector3(11f, 3f, 0.4f), new Vector3(6f, 6f, 1.8f), cityHallMaterial);
        CreateTile("Visual_REPLACE_CityHall_Tower", cityHall, new Vector3(0f, 10.5f, 0f), new Vector3(4.4f, 8f, 2.4f), cityHallMaterial);
        CreateTile("Visual_REPLACE_CityHall_RoofLine", cityHall, new Vector3(0f, 8.7f, -0.2f), new Vector3(19f, 0.45f, 2.6f), gold);
        CreateTile("Visual_REPLACE_CityHall_Clock", cityHall, new Vector3(0f, 10.6f, -1.35f), new Vector3(2.2f, 2.2f, 0.18f), gold);

        CreateTile("Visual_REPLACE_Backdrop_LeftMass", cityHall, new Vector3(-20f, 6f, 1.4f), new Vector3(6f, 12f, 3f), buildingMaterial);
        CreateTile("Visual_REPLACE_Backdrop_RightMass", cityHall, new Vector3(20f, 6f, 1.4f), new Vector3(6f, 12f, 3f), buildingMaterial);

        RemoveCollidersRecursive(cityHall.gameObject);
    }

    private static void SetTransform(string objectName, Vector3 position, Quaternion rotation)
    {
        SetTransform(objectName, position, rotation, null);
    }

    private static void SetTransform(string objectName, Vector3 position, Quaternion rotation, Vector3? scale)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        target.transform.position = position;
        target.transform.rotation = rotation;
        if (scale.HasValue)
        {
            target.transform.localScale = scale.Value;
        }

        EditorUtility.SetDirty(target.transform);
    }

    private static Quaternion FacePoint(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f)
        {
            return Quaternion.identity;
        }

        return Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private static void RemoveCollidersRecursive(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static GameObject CreateTile(string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = name;
        tile.transform.SetParent(parent);
        tile.transform.localPosition = localPosition;
        tile.transform.localRotation = Quaternion.identity;
        tile.transform.localScale = localScale;
        MeshRenderer renderer = tile.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return tile;
    }

    private static void RemoveCollider(GameObject target)
    {
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void EnsureMaterials()
    {
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "NguyenHue");

        CreateOrUpdateLitMaterial(LightPavementPath, new Color(0.62f, 0.61f, 0.57f), 0.35f);
        CreateOrUpdateLitMaterial(DarkPavementPath, new Color(0.44f, 0.44f, 0.42f), 0.3f);
        CreateOrUpdateLitMaterial(SeamPath, new Color(0.16f, 0.16f, 0.15f), 0.2f);
        CreateOrUpdateLitMaterial(GoldAccentPath, new Color(1f, 0.66f, 0.16f), 0.25f);
        CreateOrUpdateLitMaterial(StoneEdgePath, new Color(0.52f, 0.49f, 0.43f), 0.3f);
        CreateOrUpdateLitMaterial(CityHallPath, new Color(0.72f, 0.68f, 0.58f), 0.35f);
        CreateOrUpdateLitMaterial(SideBuildingPath, new Color(0.17f, 0.2f, 0.23f), 0.22f);
    }

    private static void CreateOrUpdateLitMaterial(string path, Color baseColor, float smoothness)
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

        SetMaterialColor(material, "_BaseColor", baseColor);
        SetMaterialColor(material, "_Color", baseColor);
        SetMaterialFloat(material, "_Smoothness", smoothness);
        SetMaterialFloat(material, "_Metallic", 0f);
        EditorUtility.SetDirty(material);
    }

    private static Material LoadMaterial(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Material>(path);
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

    private static Transform FindOrCreateRoot(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing.transform;
        }

        GameObject root = new GameObject(name);
        return root.transform;
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

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
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
}
#endif
