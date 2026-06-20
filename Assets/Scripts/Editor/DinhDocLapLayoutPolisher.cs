#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DinhDocLapLayoutPolisher
{
    private const string ScenePath = "Assets/Scenes/Scene_03_DinhDocLap.unity";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunDinhDocLapLayoutPolisher.flag";
    private const string MenuPath = "Ky Uc Sai Gon/Setup/Apply Dinh Doc Lap Layout Fix";

    private static Material palaceWhite;
    private static Material palaceShadow;
    private static Material roofRed;
    private static Material grassGreen;
    private static Material roadGray;
    private static Material stoneGray;
    private static Material waterBlue;
    private static Material treeTrunk;
    private static Material treeLeaf;
    private static Material gateDark;
    private static Material boundaryRed;
    private static Material gold;

    [InitializeOnLoadMethod]
    private static void AutoRunIfRequested()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(AutoRunFlagPath) || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("[KyUcSaiGon] Dinh Doc Lap layout polish cancelled.");
                return;
            }

            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            ApplyNoPrompt();
        };
    }

    [MenuItem(MenuPath)]
    public static void ApplyFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Dinh Doc Lap layout polish cancelled.");
            return;
        }

        ApplyNoPrompt();
    }

    public static void ApplyNoPrompt()
    {
        string startingScene = SceneManager.GetActiveScene().path;

        try
        {
            EnsureMaterials();
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject sceneRoot = FindOrCreateRoot("SceneBlockoutRoot");

            Transform environmentRoot = FindOrCreateChild(sceneRoot.transform, "EnvironmentRoot");
            Transform landmarkRoot = FindOrCreateChild(sceneRoot.transform, "LandmarkRoot");
            Transform propRoot = FindOrCreateChild(sceneRoot.transform, "PropRoot");
            Transform effectsRoot = FindOrCreateChild(sceneRoot.transform, "EffectsRoot");
            Transform spawnRoot = FindOrCreateChild(sceneRoot.transform, "SpawnPoints");
            ClearChildren(environmentRoot);
            ClearChildren(landmarkRoot);
            ClearChildren(propRoot);
            ClearGenerated(effectsRoot, "DinhDocLap_BoundaryAndGuides");

            DisableOldBoundaries(sceneRoot.transform);
            BuildEnvironment(environmentRoot);
            BuildPalaceAndGate(landmarkRoot);
            BuildTrees(propRoot);
            BuildGuides(effectsRoot);
            RepositionGameplay(sceneRoot.transform, spawnRoot);
            RepairRestoreEffects(sceneRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[KyUcSaiGon] Scene_03_DinhDocLap layout polished: circular lawn, fountain, curved road, palace, trees, safe boundary.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(startingScene) && File.Exists(startingScene))
            {
                EditorSceneManager.OpenScene(startingScene, OpenSceneMode.Single);
            }
        }
    }

    private static void BuildEnvironment(Transform environmentRoot)
    {
        Transform layout = CreateChild(environmentRoot, "DinhDocLap_RecognizableLayout");

        CreateBox(layout, "DinhDocLap_GroundCollider", new Vector3(0f, -0.05f, 0f), new Vector3(90f, 0.1f, 86f), stoneGray, true, false);
        CreateBox(layout, "REPLACE_Prop_PalaceCourtyard_Tiles", new Vector3(0f, 0.01f, 3f), new Vector3(78f, 0.04f, 76f), stoneGray, false, false);

        CreateBox(layout, "REPLACE_Prop_MainEntrancePath", new Vector3(0f, 0.06f, -19f), new Vector3(8f, 0.08f, 27f), roadGray, false, false);
        CreateBox(layout, "REPLACE_Prop_PalaceApproachPath", new Vector3(0f, 0.065f, 17.5f), new Vector3(10f, 0.08f, 20f), roadGray, false, false);
        CreateBox(layout, "REPLACE_Prop_LeftSideWalk", new Vector3(-26f, 0.055f, 2f), new Vector3(6f, 0.08f, 62f), roadGray, false, false);
        CreateBox(layout, "REPLACE_Prop_RightSideWalk", new Vector3(26f, 0.055f, 2f), new Vector3(6f, 0.08f, 62f), roadGray, false, false);

        GameObject lawn = CreatePrimitive(layout, "REPLACE_Prop_CircularLawn", PrimitiveType.Cylinder, new Vector3(0f, 0.09f, -1f), Quaternion.identity, new Vector3(16.8f, 0.05f, 16.8f), grassGreen, false, false);
        lawn.transform.localRotation = Quaternion.identity;

        CreateRoadRing(layout);
        CreateFountain(layout);
        CreateBoundary(layout);
    }

    private static void CreateRoadRing(Transform parent)
    {
        const int segmentCount = 32;
        const float radius = 19.2f;
        for (int i = 0; i < segmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / segmentCount;
            Vector3 position = new Vector3(Mathf.Sin(angle) * radius, 0.075f, -1f + Mathf.Cos(angle) * radius);
            Quaternion rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
            CreateBox(parent, "REPLACE_Prop_CurvedRoad_" + (i + 1).ToString("00"), position, new Vector3(5.3f, 0.07f, 2.35f), roadGray, false, false, rotation);
        }

        CreateBox(parent, "REPLACE_Prop_FrontRoadConnector", new Vector3(0f, 0.08f, -21.5f), new Vector3(18f, 0.07f, 3f), roadGray, false, false);
        CreateBox(parent, "REPLACE_Prop_PalaceRoadConnector", new Vector3(0f, 0.08f, 18.5f), new Vector3(22f, 0.07f, 3f), roadGray, false, false);
    }

    private static void CreateFountain(Transform parent)
    {
        Transform fountainRoot = CreateChild(parent, "REPLACE_Prop_CenterFountain");
        CreatePrimitive(fountainRoot, "Visual_REPLACE_CenterFountain_Basin", PrimitiveType.Cylinder, new Vector3(0f, 0.26f, -1f), Quaternion.identity, new Vector3(2.8f, 0.22f, 2.8f), stoneGray, false, false);
        CreatePrimitive(fountainRoot, "Visual_REPLACE_CenterFountain_Water", PrimitiveType.Cylinder, new Vector3(0f, 0.5f, -1f), Quaternion.identity, new Vector3(2.25f, 0.04f, 2.25f), waterBlue, false, true);
        CreatePrimitive(fountainRoot, "Visual_REPLACE_CenterFountain_Jet", PrimitiveType.Cylinder, new Vector3(0f, 1.2f, -1f), Quaternion.identity, new Vector3(0.18f, 0.8f, 0.18f), waterBlue, false, true);
        CreatePrimitive(fountainRoot, "Visual_REPLACE_CenterFountain_Drop", PrimitiveType.Sphere, new Vector3(0f, 2.05f, -1f), Quaternion.identity, Vector3.one * 0.35f, waterBlue, false, true);
    }

    private static void BuildPalaceAndGate(Transform landmarkRoot)
    {
        Transform layout = CreateChild(landmarkRoot, "DinhDocLap_LandmarkLayout");
        Transform gate = CreateChild(layout, "REPLACE_Landmark_DinhDocLap_Gate");
        CreateBox(gate, "Visual_REPLACE_Gate_LeftPost", new Vector3(-8.5f, 2f, -36f), new Vector3(1.3f, 4f, 1.2f), gateDark, false, false);
        CreateBox(gate, "Visual_REPLACE_Gate_RightPost", new Vector3(8.5f, 2f, -36f), new Vector3(1.3f, 4f, 1.2f), gateDark, false, false);
        CreateBox(gate, "Visual_REPLACE_Gate_TopBeam", new Vector3(0f, 4.1f, -36f), new Vector3(20f, 0.75f, 1f), gateDark, false, false);
        CreateBox(gate, "Visual_REPLACE_Gate_LeftFence", new Vector3(-20f, 1.3f, -36f), new Vector3(13f, 2.6f, 0.55f), gateDark, false, false);
        CreateBox(gate, "Visual_REPLACE_Gate_RightFence", new Vector3(20f, 1.3f, -36f), new Vector3(13f, 2.6f, 0.55f), gateDark, false, false);

        Transform palace = CreateChild(layout, "REPLACE_Landmark_DinhDocLap_Palace");
        CreateBox(palace, "Visual_REPLACE_Palace_MainWideBody", new Vector3(0f, 5f, 32f), new Vector3(42f, 8f, 6f), palaceWhite, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_LeftWing", new Vector3(-23f, 4f, 31.5f), new Vector3(8f, 6f, 5.5f), palaceWhite, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_RightWing", new Vector3(23f, 4f, 31.5f), new Vector3(8f, 6f, 5.5f), palaceWhite, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_CentralEntrance", new Vector3(0f, 2.2f, 28.55f), new Vector3(6f, 4.1f, 0.7f), palaceShadow, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_CentralBalcony", new Vector3(0f, 5.5f, 28.2f), new Vector3(10f, 0.55f, 1.2f), palaceShadow, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_LongRoof", new Vector3(0f, 9.25f, 32f), new Vector3(45f, 1.1f, 7.2f), roofRed, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_CentralRoofBlock", new Vector3(0f, 10.6f, 32f), new Vector3(12f, 1.15f, 6.5f), roofRed, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_LeftRoofBlock", new Vector3(-18f, 10.15f, 32f), new Vector3(8f, 0.9f, 6.2f), roofRed, false, false);
        CreateBox(palace, "Visual_REPLACE_Palace_RightRoofBlock", new Vector3(18f, 10.15f, 32f), new Vector3(8f, 0.9f, 6.2f), roofRed, false, false);

        for (int i = 0; i < 11; i++)
        {
            float x = -18f + i * 3.6f;
            CreatePrimitive(palace, "Visual_REPLACE_Palace_Column_" + (i + 1).ToString("00"), PrimitiveType.Cylinder, new Vector3(x, 3.55f, 28.15f), Quaternion.identity, new Vector3(0.32f, 2.9f, 0.32f), palaceWhite, false, false);
        }

        for (int row = 0; row < 2; row++)
        {
            for (int i = 0; i < 10; i++)
            {
                float x = -17f + i * 3.8f;
                float y = 4.2f + row * 2.3f;
                CreateBox(palace, "Visual_REPLACE_Palace_Window_" + row + "_" + i, new Vector3(x, y, 28.05f), new Vector3(1.45f, 0.95f, 0.2f), palaceShadow, false, false);
            }
        }
    }

    private static void BuildTrees(Transform propRoot)
    {
        Transform treesRoot = CreateChild(propRoot, "DinhDocLap_TreeRows");
        for (int i = 0; i < 11; i++)
        {
            float z = -28f + i * 5.8f;
            CreateTree(treesRoot, "REPLACE_Prop_DinhDocLap_Tree_Left_" + (i + 1).ToString("00"), new Vector3(-34f, 0f, z), 1.05f);
            CreateTree(treesRoot, "REPLACE_Prop_DinhDocLap_Tree_Right_" + (i + 1).ToString("00"), new Vector3(34f, 0f, z), 1.05f);
        }

        for (int i = 0; i < 8; i++)
        {
            float x = -24.5f + i * 7f;
            CreateTree(treesRoot, "REPLACE_Prop_DinhDocLap_Tree_Back_" + (i + 1).ToString("00"), new Vector3(x, 0f, 38f), 0.95f);
        }
    }

    private static void CreateTree(Transform parent, string name, Vector3 basePosition, float scale)
    {
        Transform tree = CreateChild(parent, name);
        CreatePrimitive(tree, "Visual_REPLACE_Tree_Trunk", PrimitiveType.Cylinder, basePosition + new Vector3(0f, 1.5f * scale, 0f), Quaternion.identity, new Vector3(0.45f * scale, 1.5f * scale, 0.45f * scale), treeTrunk, false, false);
        CreatePrimitive(tree, "Visual_REPLACE_Tree_Canopy_Lower", PrimitiveType.Sphere, basePosition + new Vector3(0f, 3.1f * scale, 0f), Quaternion.identity, new Vector3(2.2f * scale, 1.25f * scale, 2.2f * scale), treeLeaf, false, false);
        CreatePrimitive(tree, "Visual_REPLACE_Tree_Canopy_Upper", PrimitiveType.Sphere, basePosition + new Vector3(0.15f * scale, 4.1f * scale, 0.1f * scale), Quaternion.identity, new Vector3(1.65f * scale, 1.05f * scale, 1.65f * scale), treeLeaf, false, false);
    }

    private static void BuildGuides(Transform effectsRoot)
    {
        Transform guides = CreateChild(effectsRoot, "DinhDocLap_BoundaryAndGuides");
        Vector3[] markers =
        {
            new Vector3(0f, 0.14f, -31f),
            new Vector3(-5f, 0.14f, -23f),
            new Vector3(0f, 0.14f, -13f),
            new Vector3(0f, 0.14f, 10f),
            new Vector3(-4f, 0.14f, 20f),
            new Vector3(5f, 0.14f, 20f),
            new Vector3(15f, 0.14f, 15f)
        };

        for (int i = 0; i < markers.Length; i++)
        {
            CreateBox(guides, "Walkable_TestMarker_" + (i + 1).ToString("00"), markers[i], new Vector3(0.75f, 0.08f, 0.75f), gold, false, true);
        }
    }

    private static void RepositionGameplay(Transform sceneRoot, Transform spawnRoot)
    {
        Transform playerSpawn = FindOrCreateChild(spawnRoot, "PlayerSpawn");
        playerSpawn.position = new Vector3(0f, 0.05f, -34f);
        playerSpawn.rotation = Quaternion.Euler(0f, 0f, 0f);

        SetTransform("REPLACE_Player_Character", new Vector3(0f, 1f, -34f), Quaternion.Euler(0f, 0f, 0f), null);
        SetTransform("REPLACE_Item_HistoricalMap", new Vector3(-5.4f, 0.65f, -23.5f), Quaternion.Euler(0f, 20f, 0f), null);
        SetTransform("REPLACE_NPC_OldTourGuide", new Vector3(-5.2f, 1f, 20.5f), Quaternion.Euler(0f, 180f, 0f), null);
        SetTransform("REPLACE_Puzzle_Radio1975", new Vector3(5.4f, 1f, 20.2f), Quaternion.Euler(0f, 180f, 0f), null);
        SetTransform("REPLACE_BusStop_ReturnHub", new Vector3(15f, 1.2f, 15.5f), Quaternion.Euler(0f, -70f, 0f), null);

        RefitGameplayCollider("REPLACE_Item_HistoricalMap", 1.4f, 1.4f, 1.4f);
        RefitGameplayCollider("REPLACE_NPC_OldTourGuide", 0.55f, 2.1f, 0.55f);
        RefitGameplayCollider("REPLACE_Puzzle_Radio1975", 3.2f, 1.8f, 2.2f);
        RefitGameplayCollider("REPLACE_BusStop_ReturnHub", 4f, 2.4f, 1.2f);

        DinhDocLapSceneController controller = sceneRoot.GetComponent<DinhDocLapSceneController>();
        if (controller != null)
        {
            controller.memoryZone = UnityEngine.Object.FindFirstObjectByType<MemoryZoneController>();
            controller.tourGuideNpc = FindComponent<NPCInteractable>("REPLACE_NPC_OldTourGuide");
            controller.historicalMapItem = FindComponent<DinhDocLapMapItemInteractable>("REPLACE_Item_HistoricalMap");
            controller.radioPuzzle = FindComponent<PuzzleInteractable>("REPLACE_Puzzle_Radio1975");
            controller.returnBusStop = FindComponent<BusStopInteractable>("REPLACE_BusStop_ReturnHub");
        }
    }

    private static void RepairRestoreEffects(Transform sceneRoot)
    {
        MemoryZoneController zone = UnityEngine.Object.FindFirstObjectByType<MemoryZoneController>();
        if (zone == null)
        {
            return;
        }

        MaterialRestoreEffect materialEffect = UnityEngine.Object.FindFirstObjectByType<MaterialRestoreEffect>();
        if (materialEffect != null)
        {
            List<Renderer> renderers = new List<Renderer>();
            foreach (Renderer renderer in sceneRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponentInParent<Canvas>() != null)
                {
                    continue;
                }

                renderers.Add(renderer);
            }

            materialEffect.renderers = renderers.ToArray();
            materialEffect.grayColor = new Color(0.46f, 0.48f, 0.49f);
            materialEffect.restoredColor = new Color(0.72f, 0.82f, 0.64f);
            materialEffect.preserveRendererColors = true;
            materialEffect.grayBlend = 0.45f;
            EditorUtility.SetDirty(materialEffect);
        }

        BusStopInteractable busStop = FindComponent<BusStopInteractable>("REPLACE_BusStop_ReturnHub");
        if (busStop != null)
        {
            busStop.currentZone = zone;
            busStop.targetScene = SceneLoader.BusHub;
            zone.busStopReturn = busStop.gameObject;
        }

        PuzzleInteractable puzzle = FindComponent<PuzzleInteractable>("REPLACE_Puzzle_Radio1975");
        if (puzzle != null)
        {
            puzzle.correctAnswer = "1975";
            puzzle.memoryZone = zone;
        }
    }

    private static void CreateBoundary(Transform parent)
    {
        Transform boundary = CreateChild(parent, "DinhDocLap_OuterBoundary");
        CreateBox(boundary, "BOUNDARY_North_RedTransparent", new Vector3(0f, 2f, 42.5f), new Vector3(90f, 4f, 1f), boundaryRed, true, false);
        CreateBox(boundary, "BOUNDARY_South_RedTransparent", new Vector3(0f, 2f, -42.5f), new Vector3(90f, 4f, 1f), boundaryRed, true, false);
        CreateBox(boundary, "BOUNDARY_West_RedTransparent", new Vector3(-44.5f, 2f, 0f), new Vector3(1f, 4f, 86f), boundaryRed, true, false);
        CreateBox(boundary, "BOUNDARY_East_RedTransparent", new Vector3(44.5f, 2f, 0f), new Vector3(1f, 4f, 86f), boundaryRed, true, false);
    }

    private static void DisableOldBoundaries(Transform sceneRoot)
    {
        foreach (Transform transform in sceneRoot.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name.StartsWith("BOUNDARY_", StringComparison.Ordinal) && transform.GetComponentInParent<Transform>().name != "DinhDocLap_OuterBoundary")
            {
                Collider collider = transform.GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider, bool emissive, Quaternion? rotation = null)
    {
        return CreatePrimitive(parent, name, PrimitiveType.Cube, position, rotation ?? Quaternion.identity, scale, material, keepCollider, emissive);
    }

    private static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type, Vector3 position, Quaternion rotation, Vector3 scale, Material material, bool keepCollider, bool emissive)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = name;
        item.transform.SetParent(parent, true);
        item.transform.position = position;
        item.transform.rotation = rotation;
        item.transform.localScale = scale;

        if (!keepCollider)
        {
            Collider collider = item.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        Renderer renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        if (emissive && renderer.sharedMaterial != null)
        {
            renderer.sharedMaterial.EnableKeyword("_EMISSION");
        }

        return item;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject activeObject = GameObject.Find(objectName);
        if (activeObject != null)
        {
            return activeObject;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject candidate in allObjects)
        {
            if (candidate.name == objectName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }
    private static void RefitGameplayCollider(string objectName, float sizeX, float sizeY, float sizeZ)
    {
        GameObject item = FindSceneObject(objectName);
        if (item == null)
        {
            return;
        }

        BoxCollider box = item.GetComponent<BoxCollider>();
        if (box != null)
        {
            box.size = new Vector3(sizeX, sizeY, sizeZ);
            box.center = new Vector3(0f, sizeY * 0.5f - 0.1f, 0f);
            box.isTrigger = false;
            return;
        }

        SphereCollider sphere = item.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            sphere.radius = Mathf.Max(sizeX, sizeZ) * 0.5f;
            sphere.center = Vector3.up * 0.25f;
            sphere.isTrigger = false;
            return;
        }

        CapsuleCollider capsule = item.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = sizeX;
            capsule.height = sizeY;
            capsule.center = Vector3.up * sizeY * 0.5f;
            capsule.isTrigger = false;
        }
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject item = FindSceneObject(objectName);
        return item != null ? item.GetComponent<T>() : null;
    }

    private static void SetTransform(string objectName, Vector3 position, Quaternion rotation, Vector3? scale)
    {
        GameObject item = FindSceneObject(objectName);
        if (item == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing Dinh Doc Lap object: " + objectName);
            return;
        }

        item.transform.position = position;
        item.transform.rotation = rotation;
        if (scale.HasValue)
        {
            item.transform.localScale = scale.Value;
        }
    }

    private static void ClearGenerated(Transform root, string specificChild = null)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            if (specificChild != null)
            {
                if (child.name == specificChild)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }

                continue;
            }

            if (child.name.StartsWith("DinhDocLap_", StringComparison.Ordinal)
                || child.name.StartsWith("REPLACE_Landmark_DinhDocLap_", StringComparison.Ordinal)
                || child.name.StartsWith("REPLACE_Prop_CircularLawn", StringComparison.Ordinal)
                || child.name.StartsWith("REPLACE_Prop_CenterFountain", StringComparison.Ordinal)
                || child.name.StartsWith("REPLACE_Prop_DinhDocLap_Tree", StringComparison.Ordinal))
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child : CreateChild(parent, name);
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child.transform;
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        return root != null ? root : new GameObject(name);
    }

    private static void EnsureMaterials()
    {
        const string root = "Assets/Art/Materials/DinhDocLap";
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "DinhDocLap");

        palaceWhite = CreateMaterial(root + "/M_DinhDocLap_PalaceWarmWhite.mat", new Color(0.9f, 0.88f, 0.78f), false, 0f);
        palaceShadow = CreateMaterial(root + "/M_DinhDocLap_PalaceShadow.mat", new Color(0.18f, 0.2f, 0.21f), false, 0f);
        roofRed = CreateMaterial(root + "/M_DinhDocLap_RoofRedOrange.mat", new Color(0.72f, 0.22f, 0.12f), false, 0f);
        grassGreen = CreateMaterial(root + "/M_DinhDocLap_GrassCircle.mat", new Color(0.2f, 0.58f, 0.2f), false, 0f);
        roadGray = CreateMaterial(root + "/M_DinhDocLap_CurvedRoad.mat", new Color(0.38f, 0.4f, 0.41f), false, 0f);
        stoneGray = CreateMaterial(root + "/M_DinhDocLap_StoneTiles.mat", new Color(0.56f, 0.6f, 0.62f), false, 0f);
        waterBlue = CreateMaterial(root + "/M_DinhDocLap_FountainWater.mat", new Color(0.2f, 0.65f, 0.95f, 0.78f), true, 1.2f);
        treeTrunk = CreateMaterial(root + "/M_DinhDocLap_TreeTrunk.mat", new Color(0.27f, 0.14f, 0.07f), false, 0f);
        treeLeaf = CreateMaterial(root + "/M_DinhDocLap_TreeLeaves.mat", new Color(0.12f, 0.42f, 0.14f), false, 0f);
        gateDark = CreateMaterial(root + "/M_DinhDocLap_GateDark.mat", new Color(0.08f, 0.09f, 0.1f), false, 0f);
        boundaryRed = CreateMaterial(root + "/M_DinhDocLap_BoundaryTransparent.mat", new Color(1f, 0f, 0f, 0.16f), true, 0f);
        gold = CreateMaterial(root + "/M_DinhDocLap_GuidanceGold.mat", new Color(1f, 0.72f, 0.18f), false, 1.4f);
    }

    private static Material CreateMaterial(string path, Color color, bool transparent, float emission)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader != null ? shader : Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (transparent && material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.renderQueue = 3000;
        }
        else if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 0f);
            material.renderQueue = -1;
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.28f);
        }

        if (emission > 0f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(material);
        return material;
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
