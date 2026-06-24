using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EndingSceneVisualPolisher
{
    private const string ScenePath = "Assets/Scenes/Scene_07_Ending.unity";

    [MenuItem("Ky Uc Sai Gon/Polish/Polish Ending Riverside View")]
    public static void PolishEndingRiversideView()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject root = FindOrCreate("SceneBlockoutRoot");
        Transform props = FindOrCreateChild(root.transform, "RiversidePropRoot");
        Transform returnRoot = FindOrCreateChild(root.transform, "ReturnTriggerRoot");

        // We DO NOT clear environment, props, landmark, or effects.
        // We preserve all models and only build the interaction point.

        Material gold = Mat("M_Ending_SunsetGold", new Color(0.85f, 0.85f, 0.85f), true, 1.0f);
        
        BuildInteractionPoint(props, gold);
        SetupReturnTrigger(returnRoot);
        FixPlayerSpawn();
        HideMemoryShardPlaceholders(root);
        UpdateEndingController(root);

        // Automatically restore and configure the WaterPlane and Landmark 81 model
        WaterEndingFixer.FixWaterEnding(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[KyUcSaiGon] Scene_07_Ending polished: preserved all models and set up interaction point.");
    }

    private static void BuildRiverside(Transform parent, Material tile, Material river, Material waterGlow, Material waterDark)
    {
        Cube("REPLACE_Ending_ViewpointPlatform", parent, new Vector3(0, -0.05f, -12f), new Vector3(38f, 0.25f, 40f), tile);

        // Grid container for modular pavement tiles
        Transform gridParent = FindOrCreateChild(parent, "REPLACE_Ending_PromenadeTiles_Grid");
        ClearChildren(gridParent);

        // Load SM_Ending_PavementTile_2x2.glb
        string tilePath = "Assets/Art/Models/Ending/Ground/SM_Ending_PavementTile_2x2.glb";
        GameObject tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(tilePath);

        if (tilePrefab != null)
        {
            Debug.Log("[KyUcSaiGon] Building pavement tile grid using SM_Ending_PavementTile_2x2.glb...");
            // Promenade area: 36m wide (x: -17 to 17), 38m deep (z: -30 to 6)
            for (float x = -17f; x <= 17f; x += 2f)
            {
                for (float z = -30f; z <= 6f; z += 2f)
                {
                    GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, gridParent);
                    inst.name = $"Tile_{x}_{z}";
                    inst.transform.localPosition = new Vector3(x, 0.02f, z);
                    inst.transform.localRotation = Quaternion.identity;
                    inst.transform.localScale = Vector3.one;
                }
            }
        }
        else
        {
            Debug.LogWarning("[KyUcSaiGon] SM_Ending_PavementTile_2x2.glb not found, falling back to primitive tiles.");
            Cube("REPLACE_Ending_PromenadeTiles_Main", parent, new Vector3(0, 0.02f, -12f), new Vector3(36f, 0.05f, 38f), tile);
            for (int x = -14; x <= 14; x += 4)
            {
                Cube("REPLACE_Ending_TileSeam_X_" + x, parent, new Vector3(x, 0.06f, -12f), new Vector3(0.035f, 0.04f, 37f), Mat("M_Ending_TileSeam", new Color(0.22f, 0.2f, 0.18f)));
            }
            for (int z = -30; z <= 6; z += 4)
            {
                Cube("REPLACE_Ending_TileSeam_Z_" + z, parent, new Vector3(0, 0.06f, z), new Vector3(35f, 0.04f, 0.035f), Mat("M_Ending_TileSeam", new Color(0.22f, 0.2f, 0.18f)));
            }
        }

        Cube("REPLACE_Ending_River_Saigon", parent, new Vector3(0, 0.04f, 18f), new Vector3(96f, 0.06f, 38f), river);
        // Removed the REPLACE_Ending_River_GoldenReflection object as requested to avoid the yellow/orange reflection strip on the water
        for (int i = 0; i < 9; i++)
        {
            float z = 4.5f + i * 3.6f;
            float x = (i % 2 == 0) ? -12f : 12f;
            Cube("Visual_REPLACE_Ending_River_Wave_" + i.ToString("00"), parent, new Vector3(x, 0.12f, z), new Vector3(46f, 0.025f, 0.08f), waterDark);
        }
        Cube("REPLACE_Ending_OppositeBank", parent, new Vector3(0, 0.02f, 35f), new Vector3(96f, 0.12f, 8f), Mat("M_Ending_FarBank", new Color(0.24f, 0.22f, 0.18f)));
    }

    private static void BuildSkyline(Transform environment, Transform landmark, Material skylineMat, Material towerMat, Material gold)
    {
        Transform skylineRoot = FindOrCreateChild(environment, "REPLACE_Ending_CitySkyline");
        float[] xs = { -33, -27, -22, -17, -12, -7, 7, 13, 18, 23, 29, 35 };
        float[] hs = { 8, 12, 10, 15, 11, 13, 12, 16, 10, 14, 9, 11 };
        for (int i = 0; i < xs.Length; i++)
        {
            Cube("Visual_REPLACE_Ending_SkylineBlock_" + (i + 1).ToString("00"), skylineRoot, new Vector3(xs[i], hs[i] * 0.5f, 35f), new Vector3(4.2f, hs[i], 2.2f), skylineMat);
        }

        if (landmark.childCount > 0)
        {
            Debug.Log("[KyUcSaiGon] Preserving user's Landmark 81 model under LandmarkRoot.");
            return;
        }

        Transform towerRoot = FindOrCreateChild(landmark, "REPLACE_Ending_Landmark81_Tower");
        Cube("Visual_REPLACE_Ending_Landmark81_Core", towerRoot, new Vector3(0, 18f, 34f), new Vector3(5.2f, 36f, 4.2f), towerMat);
        Cube("Visual_REPLACE_Ending_Landmark81_Mid", towerRoot, new Vector3(0, 38f, 34f), new Vector3(3.4f, 10f, 3f), towerMat);
        Cube("Visual_REPLACE_Ending_Landmark81_Top", towerRoot, new Vector3(0, 46f, 34f), new Vector3(2.1f, 7f, 2f), towerMat);
        Cylinder("Visual_REPLACE_Ending_Landmark81_Spire", towerRoot, new Vector3(0, 54f, 34f), new Vector3(0.18f, 5f, 0.18f), gold);
    }

    private static void BuildRailing(Transform parent, Material dark)
    {
        Transform root = FindOrCreateChild(parent, "REPLACE_Ending_Railing");
        for (int i = 0; i < 13; i++)
        {
            float x = -15f + i * 2.5f;
            Cube("Visual_REPLACE_Ending_Railing_Post_" + i.ToString("00"), root, new Vector3(x, 0.85f, 3.1f), new Vector3(0.28f, 1.7f, 0.28f), dark);
        }
        Cube("Visual_REPLACE_Ending_Railing_TopRail", root, new Vector3(0, 1.55f, 3.1f), new Vector3(31f, 0.14f, 0.14f), dark);
        Cube("Visual_REPLACE_Ending_Railing_MidRail", root, new Vector3(0, 0.9f, 3.1f), new Vector3(31f, 0.1f, 0.1f), dark);
        BoxCollider railCollider = root.gameObject.AddComponent<BoxCollider>();
        railCollider.center = new Vector3(0, 0.9f, 3.1f);
        railCollider.size = new Vector3(32f, 1.8f, 0.6f);
    }

    private static void BuildStreetDetails(Transform parent, Material dark, Material gold, Material bannerMat, Material tile)
    {
        Bench(parent, new Vector3(-11.5f, 0.4f, -14.5f), dark, tile);
        Banner(parent, new Vector3(-14.5f, 2.6f, -7.5f), bannerMat, gold, dark);
        Lamp(parent, new Vector3(-14f, 2.7f, -3.2f), dark, gold);
        Lamp(parent, new Vector3(13.5f, 2.7f, -1.2f), dark, gold);
        Cube("REPLACE_Ending_Planter_Left", parent, new Vector3(-15f, 0.35f, -12f), new Vector3(2.8f, 0.7f, 2.8f), tile);
        Sphere("Visual_REPLACE_Ending_PlanterLeaves_Left", parent, new Vector3(-15f, 1.25f, -12f), new Vector3(2.2f, 1.3f, 2.2f), Mat("M_Ending_PlantGreen", new Color(0.18f, 0.42f, 0.15f)));
    }

    private static void BuildBoat(Transform parent, Material dark, Material gold)
    {
        Transform boat = FindOrCreateChild(parent, "REPLACE_Ending_Boat_OnRiver");
        Cube("Visual_REPLACE_Ending_Boat_Hull", boat, new Vector3(20f, 0.45f, 18f), new Vector3(8f, 0.8f, 2.2f), dark);
        Cube("Visual_REPLACE_Ending_Boat_Cabin", boat, new Vector3(20f, 1.3f, 18f), new Vector3(5.5f, 1.2f, 1.6f), Mat("M_Ending_BoatCabin", new Color(0.72f, 0.62f, 0.46f)));
        Cube("Visual_REPLACE_Ending_Boat_Light", boat, new Vector3(17.4f, 2.05f, 18f), new Vector3(0.35f, 0.35f, 0.35f), gold);

        // Attach BoatMovement if not already present
        BoatMovement movement = boat.gameObject.GetComponent<BoatMovement>();
        if (movement == null)
        {
            movement = boat.gameObject.AddComponent<BoatMovement>();
        }
        movement.speed = 2.0f;
        movement.minX = -52f;
        movement.maxX = 52f;
    }

    private static void BuildInteractionPoint(Transform props, Material gold)
    {
        Transform point = FindOrCreateChild(props, "REPLACE_Ending_CutsceneTriggerPoint");
        point.position = new Vector3(0.11f, 1.6f, -1.6f); // Positioned exactly at Railing (5)
        point.localScale = new Vector3(1.0f, 1.0f, 1.0f); // Use standard scale

        // Remove primitive collider if any, and add a sphere collider with isTrigger = true
        Collider col = point.GetComponent<Collider>();
        if (col != null)
        {
            Object.DestroyImmediate(col);
        }

        SphereCollider sphereCol = point.gameObject.GetComponent<SphereCollider>();
        if (sphereCol == null)
        {
            sphereCol = point.gameObject.AddComponent<SphereCollider>();
        }
        sphereCol.isTrigger = true;
        sphereCol.radius = 2.0f; // Large trigger area for easy interaction

        // Visual mesh: A soft vertical cylinder (light shaft)
        Transform visualT = point.Find("Visual");
        GameObject visual = visualT != null ? visualT.gameObject : null;
        if (visual != null)
        {
            Object.DestroyImmediate(visual);
        }

        visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(point);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = new Vector3(0.28f, 1.2f, 0.28f); // Tall and thin light shaft

        // Destroy visual collider to prevent blocking raycasts
        Collider visCol = visual.GetComponent<Collider>();
        if (visCol != null)
        {
            Object.DestroyImmediate(visCol);
        }

        // Configure transparent additive material
        Material beamMat = Mat("M_Ending_GoldBeam", new Color(0.96f, 0.83f, 0.36f, 0.14f), true, 2.0f);
        beamMat.SetFloat("_Surface", 1f); // Transparent
        beamMat.SetFloat("_Blend", 1f); // Additive
        beamMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        beamMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        beamMat.SetInt("_ZWrite", 0);
        beamMat.DisableKeyword("_ALPHATEST_ON");
        beamMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        beamMat.renderQueue = 3000;
        EditorUtility.SetDirty(beamMat);

        Renderer r = visual.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = beamMat;
        }

        // Attach EndingTriggerInteractable
        if (point.gameObject.GetComponent<EndingTriggerInteractable>() == null)
        {
            point.gameObject.AddComponent<EndingTriggerInteractable>();
        }

        // Attach GlowingInteractionPoint
        if (point.gameObject.GetComponent<GlowingInteractionPoint>() == null)
        {
            point.gameObject.AddComponent<GlowingInteractionPoint>();
        }

        // Add a small point light to make it glow in the dark scene
        Light light = point.gameObject.GetComponent<Light>();
        if (light == null)
        {
            light = point.gameObject.AddComponent<Light>();
        }
        light.type = LightType.Point;
        light.color = new Color(0.96f, 0.83f, 0.36f);
        light.intensity = 1.5f;
        light.range = 5f;
    }

    private static void SetupReturnTrigger(Transform returnRoot)
    {
        Transform trigger = returnRoot.Find("REPLACE_Ending_ReturnToHubTrigger");
        if (trigger == null)
        {
            GameObject go = new GameObject("REPLACE_Ending_ReturnToHubTrigger");
            go.transform.SetParent(returnRoot);
            trigger = go.transform;
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.isTrigger = false;
        }

        trigger.gameObject.SetActive(false);
        trigger.position = new Vector3(0f, 1.25f, -18f);
        BoxCollider box = trigger.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = trigger.gameObject.AddComponent<BoxCollider>();
        }
        box.size = new Vector3(4f, 2.5f, 2f);
        box.center = Vector3.zero;

        BusStopInteractable interact = trigger.GetComponent<BusStopInteractable>();
        if (interact == null)
        {
            interact = trigger.gameObject.AddComponent<BusStopInteractable>();
        }
        interact.requireCurrentZoneRestored = false;
        interact.targetScene = SceneLoader.BusHub;
        interact.interactionPrompt = "Nhấn E để quay về xe buýt ký ức.";
    }

    private static void PolishLighting(Transform effects, Material gold)
    {
        List<GameObject> lightsToDestroy = new List<GameObject>();
        foreach (Light light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.gameObject.name.Contains("CutsceneTriggerPoint") || 
                light.gameObject.name.Contains("Player") ||
                light.gameObject.name.Contains("Character"))
            {
                continue;
            }
            lightsToDestroy.Add(light.gameObject);
        }
        foreach (var go in lightsToDestroy)
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        GameObject sun = new GameObject("REPLACE_Ending_SunsetDirectionalLight");
        sun.transform.SetParent(effects);
        sun.transform.rotation = Quaternion.Euler(45f, -42f, 0f); // Higher angle for natural daylight look
        Light directional = sun.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(0.98f, 0.98f, 1.0f); // Neutral white sunlight
        directional.intensity = 1.25f;

        GameObject glow = new GameObject("REPLACE_Ending_RiverWarmFillLight");
        glow.transform.SetParent(effects);
        glow.transform.position = new Vector3(8f, 5f, 4f);
        Light point = glow.AddComponent<Light>();
        point.type = LightType.Point;
        point.color = new Color(0.85f, 0.92f, 1.0f); // Soft cool blue fill light
        point.range = 42f;
        point.intensity = 0.5f;

        // Sunset disc is removed for a natural day/sky view
        RenderSettings.ambientLight = new Color(0.24f, 0.26f, 0.30f); // Neutral ambient light
        RenderSettings.fog = false;
        RenderSettings.fogColor = new Color(0.78f, 0.82f, 0.86f); // Neutral light gray-blue fog
        RenderSettings.fogDensity = 0.003f; // Lower fog density for a clearer look
    }

    private static void FixPlayerSpawn()
    {
        Vector3 safeSpawn = new Vector3(0f, 1.05f, -18f);
        GameObject player = GameObject.Find("REPLACE_Player_Character");
        if (player != null)
        {
            player.transform.position = safeSpawn;
            player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }

        GameObject spawn = GameObject.Find("PlayerSpawn");
        if (spawn != null)
        {
            spawn.transform.position = safeSpawn;
            spawn.transform.rotation = Quaternion.identity;
        }
    }

    private static void HideMemoryShardPlaceholders(GameObject root)
    {
        Transform shardRoot = root.transform.Find("MemoryShardRoot");
        if (shardRoot != null)
        {
            shardRoot.gameObject.SetActive(false);
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains("MemoryShard"))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static void UpdateEndingController(GameObject root)
    {
        EndingSceneController controller = root.GetComponent<EndingSceneController>();
        if (controller == null)
        {
            controller = root.AddComponent<EndingSceneController>();
        }
        controller.landmarkTower = GameObject.Find("REPLACE_Ending_Landmark81_Tower");
        controller.memoryShards = new GameObject[0];
        controller.memoryNames = new string[0];
        controller.finalLightObject = GameObject.Find("REPLACE_Ending_SunsetDisc");
        controller.returnTrigger = GameObject.Find("REPLACE_Ending_ReturnToHubTrigger");
        controller.renderersToWarm = root.GetComponentsInChildren<Renderer>(true);
    }

    private static void Bench(Transform parent, Vector3 pos, Material dark, Material wood)
    {
        Transform root = FindOrCreateChild(parent, "REPLACE_Ending_Bench");
        Cube("Visual_REPLACE_Ending_Bench_Seat", root, pos, new Vector3(4f, 0.25f, 1f), wood);
        Cube("Visual_REPLACE_Ending_Bench_Back", root, pos + new Vector3(0, 0.75f, 0.45f), new Vector3(4f, 1f, 0.18f), wood);
        Cube("Visual_REPLACE_Ending_Bench_Legs", root, pos + new Vector3(0, -0.35f, 0), new Vector3(3.5f, 0.55f, 0.15f), dark);
    }

    private static void Banner(Transform parent, Vector3 pos, Material banner, Material gold, Material dark)
    {
        Transform root = FindOrCreateChild(parent, "REPLACE_Ending_BannerWelcome");
        Cube("Visual_REPLACE_Ending_Banner_Cloth", root, pos, new Vector3(2.2f, 4f, 0.12f), banner);
        Cube("Visual_REPLACE_Ending_Banner_Top", root, pos + new Vector3(0, 2.15f, 0), new Vector3(2.6f, 0.12f, 0.12f), gold);
        Cube("Visual_REPLACE_Ending_Banner_Pole", root, pos + new Vector3(-1.35f, -1.1f, 0), new Vector3(0.12f, 5.2f, 0.12f), dark);
    }

    private static void Lamp(Transform parent, Vector3 pos, Material dark, Material gold)
    {
        Transform root = FindOrCreateChild(parent, "REPLACE_Ending_StreetLamp_" + Mathf.Abs(pos.x).ToString("00"));
        Cylinder("Visual_REPLACE_Ending_Lamp_Post", root, pos + Vector3.down * 0.9f, new Vector3(0.12f, 1.9f, 0.12f), dark);
        Sphere("Visual_REPLACE_Ending_Lamp_Glow", root, pos + Vector3.up * 1.1f, new Vector3(0.8f, 0.8f, 0.8f), gold);
    }

    private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
    {
        return Primitive(PrimitiveType.Cube, name, parent, position, scale, Quaternion.identity, mat);
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
    {
        return Primitive(PrimitiveType.Sphere, name, parent, position, scale, Quaternion.identity, mat);
    }

    private static GameObject Cylinder(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
    {
        return Primitive(PrimitiveType.Cylinder, name, parent, position, scale, Quaternion.identity, mat);
    }

    private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material mat)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
        if (mat != null)
        {
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }
        return go;
    }

    private static Material Mat(string name, Color color, bool emission = false, float emissionStrength = 1f)
    {
        string folder = "Assets/Art/Materials/Ending";
        if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder("Assets/Art/Materials")) AssetDatabase.CreateFolder("Assets/Art", "Materials");
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Art/Materials", "Ending");

        string path = folder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Smoothness", 0.38f);
        if (emission)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emissionStrength);
        }
        else
        {
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject FindOrCreate(string name)
    {
        GameObject go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        return go.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parent) children.Add(child.gameObject);
        foreach (GameObject child in children) Object.DestroyImmediate(child);
    }
}
