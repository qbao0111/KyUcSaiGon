#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class KyUcSaiGonSceneGenerator
{
    private const string SceneFolder = "Assets/Scenes";
    private static readonly Color InactiveGray = new Color(0.45f, 0.48f, 0.5f);
    private static readonly Color PathGray = new Color(0.58f, 0.62f, 0.64f);
    private static readonly Color DarkBuilding = new Color(0.22f, 0.24f, 0.27f);
    private static readonly Color InteractableBlue = new Color(0.1f, 0.45f, 1f);
    private static readonly Color PuzzleYellow = new Color(1f, 0.78f, 0.12f);
    private static readonly Color RestoredGreen = new Color(0.2f, 0.8f, 0.35f);
    private static readonly Color NpcPurple = new Color(0.55f, 0.25f, 0.9f);
    private static readonly Color BusOrange = new Color(1f, 0.45f, 0.08f);
    private static readonly Color BoundaryRed = new Color(1f, 0f, 0f, 0.18f);
    private static readonly Color PlanterGreen = new Color(0.2f, 0.45f, 0.25f);
    private static readonly Color BusFloorBrown = new Color(0.2f, 0.1f, 0.06f);
    private static readonly Color BusFrameDark = new Color(0.09f, 0.07f, 0.06f);
    private static readonly Color BusSeatRed = new Color(0.35f, 0.08f, 0.07f);
    private static readonly Color BusWindowBlue = new Color(0.2f, 0.55f, 0.75f, 0.42f);
    private static readonly Color BusWarmLight = new Color(1f, 0.62f, 0.2f);

    [MenuItem("Ky Uc Sai Gon/Generate Complete Blockout Prototype")]
    public static void GenerateCompletePrototype()
    {
        Directory.CreateDirectory(SceneFolder);

        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            GenerateBusHub(),
            GenerateNguyenHue(),
            GenerateBenThanh(),
            GenerateDinhDocLap(),
            GenerateNhaThoDucBa(),
            GenerateBitexco(),
            GenerateBachDang(),
            GenerateEnding()
        };

        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Large third-person blockout prototype generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Bus Hub Only %#h")]
    public static void GenerateBusHubOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateBusHub();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Vintage Bus Hub blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Nguyen Hue Tutorial Only")]
    public static void GenerateNguyenHueOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateNguyenHue();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Nguyen Hue tutorial blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Ben Thanh Only")]
    public static void GenerateBenThanhOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateBenThanh();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Ben Thanh blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Dinh Doc Lap Only")]
    public static void GenerateDinhDocLapOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateDinhDocLap();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Dinh Doc Lap blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Nha Tho Duc Ba Only")]
    public static void GenerateNhaThoDucBaOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateNhaThoDucBa();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Nha Tho Duc Ba blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Bitexco Only")]
    public static void GenerateBitexcoOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateBitexco();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Bitexco blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Bach Dang Only")]
    public static void GenerateBachDangOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateBachDang();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Bach Dang blockout generated.");
    }

    [MenuItem("Ky Uc Sai Gon/Generate Ending Only")]
    public static void GenerateEndingOnly()
    {
        Directory.CreateDirectory(SceneFolder);
        GenerateEnding();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] Ending blockout generated.");
    }

    private static EditorBuildSettingsScene GenerateBusHub()
    {
        Scene scene = NewScene(SceneLoader.BusHub);
        GameObject roots = CreateBusHubRoots();
        Transform interior = roots.transform.Find("BusInteriorRoot");
        Transform routeBoard = roots.transform.Find("RouteMapBoardRoot");
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform debugLabels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(0, 1, -9.5f), "Chọn một địa điểm trên bảng lộ trình.", 5.2f, 1.1f, spawnPoints, 14f, 28f);
        CreateVintageBusInterior(interior, debugLabels);
        CreateBusRouteBoard(routeBoard);
        CreateBusGuidanceLights(roots.transform.Find("EffectsRoot"));
        AddLighting();
        return Save(scene, SceneLoader.BusHub);
    }

    private static GameObject CreateBusHubRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[] { "BusInteriorRoot", "RouteMapBoardRoot", "SpawnPoints", "EffectsRoot", "DebugLabels" })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static void CreateVintageBusInterior(Transform parent, Transform debugLabels)
    {
        Transform floor = CreateChildRoot(parent, "Floor");
        Transform walls = CreateChildRoot(parent, "Walls");
        Transform ceiling = CreateChildRoot(parent, "Ceiling");
        Transform windows = CreateChildRoot(parent, "Windows");
        Transform seats = CreateChildRoot(parent, "Seats");
        Transform driverArea = CreateChildRoot(parent, "DriverArea");
        Transform lights = CreateChildRoot(parent, "Lights");

        CreateBusPart(floor, "BusFloor", new Vector3(0, -0.08f, 0), new Vector3(9, 0.16f, 30), BusFloorBrown);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_Aisle", floor, new Vector3(0, 0.02f, 0), new Vector3(2.8f, 0.06f, 28), Quaternion.identity, new Color(0.34f, 0.2f, 0.12f));
        CreateBusPart(ceiling, "Ceiling", new Vector3(0, 5.5f, 0), new Vector3(9, 0.3f, 30), BusFrameDark);

        CreateBusPart(walls, "Wall_Left_Lower", new Vector3(-4.4f, 1.15f, 0), new Vector3(0.25f, 2.3f, 30), BusSeatRed);
        CreateBusPart(walls, "Wall_Right_Lower", new Vector3(4.4f, 1.15f, 0), new Vector3(0.25f, 2.3f, 30), BusSeatRed);
        CreateBusPart(walls, "Wall_Left_Upper", new Vector3(-4.4f, 5f, 0), new Vector3(0.25f, 1, 30), BusFrameDark);
        CreateBusPart(walls, "Wall_Right_Upper", new Vector3(4.4f, 5f, 0), new Vector3(0.25f, 1, 30), BusFrameDark);
        CreateBusPart(walls, "Wall_Front", new Vector3(0, 2.75f, 14.9f), new Vector3(9, 5.5f, 0.3f), BusFrameDark);
        CreateBusPart(walls, "Wall_Rear_Left", new Vector3(-3.25f, 2.75f, -14.9f), new Vector3(2.5f, 5.5f, 0.3f), BusFrameDark);
        CreateBusPart(walls, "Wall_Rear_Right", new Vector3(3.25f, 2.75f, -14.9f), new Vector3(2.5f, 5.5f, 0.3f), BusFrameDark);
        CreateBusPart(walls, "RearEntrance_TopFrame", new Vector3(0, 5.05f, -14.9f), new Vector3(4, 0.9f, 0.35f), BusFrameDark);
        CreateBusWindow(walls, "RearEntrance", new Vector3(0, 2.15f, -14.82f), new Vector3(3.9f, 4.1f, 0.18f));

        for (int i = 0; i < 6; i++)
        {
            float z = -12.3f + i * 4.9f;
            CreateBusWindow(windows, "Window_Left_" + (i + 1).ToString("00"), new Vector3(-4.36f, 3.55f, z), new Vector3(0.12f, 2.35f, 4f));
            CreateBusWindow(windows, "Window_Right_" + (i + 1).ToString("00"), new Vector3(4.36f, 3.55f, z), new Vector3(0.12f, 2.35f, 4f));
        }

        for (int i = 0; i < 5; i++)
        {
            float z = -7.8f + i * 3.8f;
            CreateBusSeat(seats, "BusSeat_Left_" + (i + 1).ToString("00"), new Vector3(-3.05f, 0.7f, z));
            CreateBusSeat(seats, "BusSeat_Right_" + (i + 1).ToString("00"), new Vector3(3.05f, 0.7f, z));
        }

        CreateBusWindow(windows, "FrontWindshield", new Vector3(0, 3.9f, 14.72f), new Vector3(7, 2.4f, 0.12f));
        CreateBusPart(driverArea, "DriverPlatform", new Vector3(-2.7f, 0.22f, 12.8f), new Vector3(3.2f, 0.4f, 3.2f), BusFloorBrown);
        CreateSteeringWheel(driverArea);
        CreateBusPart(driverArea, "Dashboard", new Vector3(-2.85f, 1.35f, 14), new Vector3(3, 1.2f, 0.75f), BusFrameDark);
        CreateBusHandrails(parent);

        for (int i = 0; i < 5; i++)
        {
            float z = -10.5f + i * 5.2f;
            CreateBusVisual(PrimitiveType.Sphere, "Visual_REPLACE_CeilingLight_" + (i + 1).ToString("00"), lights, new Vector3(0, 5.15f, z), new Vector3(0.8f, 0.25f, 0.8f), Quaternion.identity, BusWarmLight);
            AddGlowLight("GLOW_BusHub_CeilingLight_" + (i + 1).ToString("00"), lights, new Vector3(0, 4.95f, z), BusWarmLight, 7f, 1.25f);
        }

        CreateWorldLabel("Xe buýt ký ức", debugLabels, new Vector3(0, 5.15f, 11.8f), 0.52f, false);
    }

    private static void CreateBusRouteBoard(Transform parent)
    {
        CreateBusPart(parent, "RouteMapBoard", new Vector3(0.5f, 3.05f, 14.55f), new Vector3(8, 4.45f, 0.35f), new Color(0.06f, 0.08f, 0.1f));
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_RouteMapBoard_Header", parent, new Vector3(0.5f, 4.85f, 14.34f), new Vector3(7.5f, 0.5f, 0.12f), Quaternion.identity, BusWarmLight);
        CreateWorldLabel("Bảng lộ trình", parent, new Vector3(0.5f, 4.85f, 14.04f), 0.36f, false);
        CreateWorldLabel("Chọn địa điểm tiếp theo", parent, new Vector3(0.5f, 4.4f, 14.04f), 0.24f, false);

        CreateRouteButton(parent, "RouteButton_NguyenHue_Tutorial", "Nguyễn Huệ Tutorial", LocationId.NguyenHue, SceneLoader.NguyenHue, new Vector3(-2.6f, 3.6f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_BenThanh", "Chợ Bến Thành", LocationId.BenThanh, SceneLoader.BenThanh, new Vector3(-0.55f, 3.6f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_DinhDocLap", "Dinh Độc Lập", LocationId.DinhDocLap, SceneLoader.DinhDocLap, new Vector3(1.5f, 3.6f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_NhaThoDucBa", "Nhà thờ Đức Bà", LocationId.NhaThoDucBa, SceneLoader.NhaThoDucBa, new Vector3(3.55f, 3.6f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_Bitexco", "Bitexco", LocationId.Bitexco, SceneLoader.Bitexco, new Vector3(-1.55f, 2.35f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_BachDang", "Bến Bạch Đằng", LocationId.BachDang, SceneLoader.BachDang, new Vector3(0.5f, 2.35f, 14.28f), false);
        CreateRouteButton(parent, "RouteButton_Ending_Landmark81", "Landmark 81", LocationId.None, SceneLoader.Ending, new Vector3(2.55f, 2.35f, 14.28f), true);
    }

    private static void CreateBusGuidanceLights(Transform parent)
    {
        for (int i = 0; i < 8; i++)
        {
            float z = -9 + i * 2.8f;
            GameObject lightMarker = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_GuidanceLight_" + (i + 1).ToString("00"), parent, new Vector3(0, 0.07f, z), new Vector3(0.45f, 0.12f, 0.45f), Quaternion.identity, BusWarmLight);
            Renderer renderer = lightMarker.GetComponent<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", BusWarmLight * 1.4f);
        }
    }

    private static void CreateBusWindow(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_" + name, root.transform, Vector3.zero, scale, Quaternion.identity, BusWindowBlue);
    }

    private static void CreateBusSeat(Transform parent, string name, Vector3 position)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.center = new Vector3(0, 0.55f, 0.25f);
        collider.size = new Vector3(2.1f, 1.8f, 1.7f);
        new GameObject("Collider").transform.SetParent(root.transform);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_" + name + "_Base", root.transform, Vector3.zero, new Vector3(2.1f, 0.7f, 1.7f), Quaternion.identity, BusSeatRed);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_" + name + "_Back", root.transform, new Vector3(0, 1.05f, 0.65f), new Vector3(2.1f, 1.7f, 0.35f), Quaternion.identity, BusSeatRed);
    }

    private static GameObject CreateBusPart(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = scale;
        new GameObject("Collider").transform.SetParent(root.transform);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_" + name, root.transform, Vector3.zero, scale, Quaternion.identity, color);
        return root;
    }

    private static GameObject CreateBusVisual(PrimitiveType primitiveType, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Color color)
    {
        GameObject visual = GameObject.CreatePrimitive(primitiveType);
        visual.name = name;
        visual.transform.SetParent(parent);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = localRotation;
        visual.transform.localScale = localScale;
        visual.GetComponent<Renderer>().material = CreateMaterial(color);
        UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
        return visual;
    }

    private static void CreateSteeringWheel(Transform parent)
    {
        GameObject root = new GameObject("SteeringWheel");
        root.transform.SetParent(parent);
        root.transform.position = new Vector3(-2.85f, 2.15f, 13.7f);
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.1f, 1.1f, 0.25f);
        new GameObject("Collider").transform.SetParent(root.transform);
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_SteeringWheel", root.transform, Vector3.zero, new Vector3(0.62f, 0.12f, 0.62f), Quaternion.Euler(90, 0, 0), BusFrameDark);
    }

    private static void CreateBusHandrails(Transform parent)
    {
        Transform handrails = CreateChildRoot(parent, "Handrails");
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_Handrail_Left", handrails, new Vector3(-1.85f, 4.45f, 0), new Vector3(0.1f, 13.2f, 0.1f), Quaternion.Euler(90, 0, 0), BusWarmLight);
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_Handrail_Right", handrails, new Vector3(1.85f, 4.45f, 0), new Vector3(0.1f, 13.2f, 0.1f), Quaternion.Euler(90, 0, 0), BusWarmLight);

        for (int i = 0; i < 6; i++)
        {
            float z = -11.5f + i * 4.6f;
            CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_HandrailVertical_Left_" + (i + 1).ToString("00"), handrails, new Vector3(-1.85f, 2.75f, z), new Vector3(0.09f, 1.7f, 0.09f), Quaternion.identity, BusWarmLight);
            CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_HandrailVertical_Right_" + (i + 1).ToString("00"), handrails, new Vector3(1.85f, 2.75f, z), new Vector3(0.09f, 1.7f, 0.09f), Quaternion.identity, BusWarmLight);
        }
    }

    private static Transform CreateChildRoot(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        return child.transform;
    }

    private static EditorBuildSettingsScene GenerateNguyenHue()
    {
        Scene scene = NewScene(SceneLoader.NguyenHue);
        GameObject roots = CreateNguyenHueRoots();
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform spawnPoints = roots.transform.Find("SpawnPoints");

        CreateManagersAndPlayer(new Vector3(0, 1, -44), "Tìm 3 màn hình LED lớn quanh phố đi bộ: Bass, Mid, Treble.", 7f, 2.8f, spawnPoints, 18f, 42f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.NguyenHue, "Nhịp sống trẻ");

        GameObject tutorialObject = new GameObject("NguyenHueTutorialController");
        tutorialObject.transform.SetParent(roots.transform);
        NguyenHueTutorialController tutorial = tutorialObject.AddComponent<NguyenHueTutorialController>();
        tutorial.memoryZone = zone;

        CreateNguyenHueEnvironment(environment);
        CreateNguyenHueFountain(landmark);
        CreateNguyenHueMusician(npcRoot, tutorial);
        CreateNguyenHueSpeakerPuzzle(puzzleRoot, zone);
        CreateNguyenHueLedHint(environment, tutorial, "REPLACE_LEDHint_Bass_Red_1", "LED đỏ nhấp nháy số 1. Đây là Bass.", "BASS = 1", new Vector3(-22, 5f, -34), new Vector3(4.5f, 8f, 0.8f), Color.red);
        CreateNguyenHueLedHint(environment, tutorial, "REPLACE_LEDHint_Mid_Green_6", "LED xanh lá nhấp nháy số 6. Đây là Mid.", "MID = 6", new Vector3(22, 5f, -2), new Vector3(4.5f, 8f, 0.8f), new Color(0.1f, 1f, 0.35f));
        CreateNguyenHueLedHint(environment, tutorial, "REPLACE_LEDHint_Treble_Gold_8", "LED vàng nhấp nháy số 8. Đây là Treble.", "TREBLE = 8", new Vector3(9, 5f, 21), new Vector3(4.8f, 8.2f, 0.8f), new Color(1f, 0.82f, 0.15f));
        CreateNguyenHueBusStop(busStopRoot, zone);
        CreateNguyenHueLedGuides(environment);
        AddNguyenHueRestoreEffects(zone, roots.transform, effectsRoot);
        CreateBoundary(environment, 60, 100);
        AddLighting();
        return Save(scene, SceneLoader.NguyenHue);
    }

    private static GameObject CreateNguyenHueRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        CreateChildRoot(root.transform, "EnvironmentRoot");
        CreateChildRoot(root.transform, "LandmarkRoot");
        CreateChildRoot(root.transform, "NPCRoot");
        CreateChildRoot(root.transform, "PuzzleRoot");
        CreateChildRoot(root.transform, "EffectsRoot");
        CreateChildRoot(root.transform, "BusStopRoot");
        CreateChildRoot(root.transform, "SpawnPoints");
        CreateChildRoot(root.transform, "DebugLabels");
        return root;
    }

    private static void CreateNguyenHueEnvironment(Transform parent)
    {
        CreateFloor("REPLACE_Environment_NguyenHue_Boulevard", parent, Vector3.zero, new Vector2(60, 100));
        CreateCube("REPLACE_Prop_NguyenHue_MainWalkingPath", parent, new Vector3(0, 0.03f, 0), new Vector3(14, 0.08f, 96), PathGray);
        CreateCube("REPLACE_Prop_NguyenHue_FountainPlaza", parent, new Vector3(0, 0.05f, 10), new Vector3(30, 0.1f, 22), PathGray);

        for (int i = 0; i < 7; i++)
        {
            float z = -38 + i * 12f;
            CreateCube("REPLACE_Prop_NguyenHue_LeftPlanter_" + (i + 1), parent, new Vector3(-12, 0.45f, z), new Vector3(4.5f, 0.9f, 1.5f), PlanterGreen);
            CreateCube("REPLACE_Prop_NguyenHue_RightPlanter_" + (i + 1), parent, new Vector3(12, 0.45f, z), new Vector3(4.5f, 0.9f, 1.5f), PlanterGreen);
            CreateCube("REPLACE_Prop_NguyenHue_LeftBench_" + (i + 1), parent, new Vector3(-18, 0.4f, z + 4f), new Vector3(3.5f, 0.8f, 1.1f), InactiveGray);
            CreateCube("REPLACE_Prop_NguyenHue_RightBench_" + (i + 1), parent, new Vector3(18, 0.4f, z + 4f), new Vector3(3.5f, 0.8f, 1.1f), InactiveGray);
            CreateStreetLight(parent, "REPLACE_Prop_NguyenHue_LeftStreetLight_" + (i + 1), new Vector3(-22, 2.4f, z));
            CreateStreetLight(parent, "REPLACE_Prop_NguyenHue_RightStreetLight_" + (i + 1), new Vector3(22, 2.4f, z));
        }

        for (int i = 0; i < 6; i++)
        {
            float z = -40 + i * 16f;
            float height = 9 + (i % 3) * 3;
            CreateCube("REPLACE_Landmark_NguyenHue_LeftBuilding_" + (i + 1), parent, new Vector3(-28, height * 0.5f, z), new Vector3(5, height, 11), DarkBuilding);
            CreateCube("REPLACE_Landmark_NguyenHue_RightBuilding_" + (i + 1), parent, new Vector3(28, height * 0.5f, z), new Vector3(5, height, 11), DarkBuilding);
        }

        for (int i = 0; i < 10; i++)
        {
            CreateReplaceablePrimitiveRoot(parent, PrimitiveType.Cube, "GuidanceLight_" + (i + 1).ToString("00"), new Vector3(0, 0.08f, -40 + i * 4f), new Vector3(0.45f, 0.12f, 0.45f), BusWarmLight, false);
        }
    }

    private static void CreateNguyenHueFountain(Transform parent)
    {
        GameObject fountain = new GameObject("REPLACE_Landmark_NguyenHue_Fountain");
        fountain.transform.SetParent(parent);
        fountain.transform.position = new Vector3(0, 0, 10);
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_NguyenHue_Fountain_Base", fountain.transform, new Vector3(0, 0.55f, 0), new Vector3(8f, 0.55f, 8f), Quaternion.identity, InactiveGray);
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_NguyenHue_Fountain_Water", fountain.transform, new Vector3(0, 1.2f, 0), new Vector3(6.5f, 0.18f, 6.5f), Quaternion.identity, InteractableBlue);
        CreateWorldLabel("Đài phun nước Nguyễn Huệ", fountain.transform, new Vector3(0, 4.5f, 10), 0.5f);
    }

    private static void CreateNguyenHueMusician(Transform parent, NguyenHueTutorialController tutorial)
    {
        GameObject npc = new GameObject("REPLACE_NPC_StreetMusician");
        npc.transform.SetParent(parent);
        npc.transform.position = new Vector3(-16, 1, -9);
        npc.AddComponent<CapsuleCollider>();
        new GameObject("Collider").transform.SetParent(npc.transform);
        CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_NPC_StreetMusician", npc.transform, Vector3.zero, Vector3.one, Quaternion.identity, NpcPurple);
        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.dialogue = "Nhịp điệu bị nhiễu rồi. Hãy tìm ba màn hình LED quanh phố và chỉnh lại loa.";
        interactable.interactionPrompt = "Nhấn E để nói chuyện với nhạc công";
        interactable.tutorialController = tutorial;
        AddGlowLight("GLOW_NPC_StreetMusician", npc.transform, npc.transform.position + Vector3.up * 1.2f, NpcPurple, 7f, 2.6f);
        CreateWorldLabel("Nhạc công đường phố", npc.transform, new Vector3(-16, 3.3f, -9), 0.4f);
    }

    private static void CreateNguyenHueSpeakerPuzzle(Transform parent, MemoryZoneController zone)
    {
        GameObject puzzle = CreateReplaceablePrimitiveRoot(parent, PrimitiveType.Cube, "REPLACE_Puzzle_SpeakerMixer", new Vector3(-10, 1, -9), new Vector3(3.5f, 2, 2.6f), PuzzleYellow, true);
        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Loa phố đi bộ";
        interactable.puzzleDescription = "Chỉnh Bass - Mid - Treble theo ba màn hình LED.";
        interactable.correctAnswer = "1-6-8";
        interactable.inputHint = "Bass - Mid - Treble";
        interactable.useThreeValueStepper = true;
        interactable.wrongFeedback = "Chưa đúng. Hãy kiểm tra lại ba màn hình LED.";
        interactable.correctFeedback = "Đúng rồi. Âm nhạc đang trở lại.";
        AddGlowLight("GLOW_Puzzle_SpeakerMixer", puzzle.transform, puzzle.transform.position + Vector3.up * 1.1f, PuzzleYellow, 7f, 2.8f);
        CreateWorldLabel("Loa bị nhiễu", puzzle.transform, new Vector3(-10, 3.4f, -9), 0.4f);
    }

    private static void CreateNguyenHueLedHint(Transform parent, NguyenHueTutorialController tutorial, string name, string message, string ledText, Vector3 position, Vector3 scale, Color color)
    {
        GameObject hint = CreateReplaceablePrimitiveRoot(parent, PrimitiveType.Cube, name, position, scale, color, true);
        LEDHintInteractable interactable = hint.AddComponent<LEDHintInteractable>();
        interactable.hintMessage = message;
        interactable.tutorialController = tutorial;
        Renderer renderer = hint.GetComponentInChildren<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", color * 2.2f);
        AddGlowLight("GLOW_" + name, hint.transform, position + new Vector3(0, 1.4f, 0), color, 12f, 2.8f);
        CreateWorldLabel(ledText, hint.transform, position + new Vector3(0, 4.8f, -0.55f), 0.7f, false);
    }

    private static void CreateNguyenHueLedGuides(Transform parent)
    {
        CreateGuideLightTrail(parent, "GuideToBass", new Vector3(-3, 0.11f, -42), new Vector3(-20, 0.11f, -34), new Color(1f, 0.2f, 0.2f));
        CreateGuideLightTrail(parent, "GuideToMid", new Vector3(3, 0.11f, -26), new Vector3(20, 0.11f, -4), new Color(0.2f, 1f, 0.45f));
        CreateGuideLightTrail(parent, "GuideToTreble", new Vector3(1, 0.11f, 4), new Vector3(8, 0.11f, 20), new Color(1f, 0.84f, 0.25f));
    }

    private static void CreateGuideLightTrail(Transform parent, string name, Vector3 start, Vector3 end, Color color)
    {
        int segments = 8;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 position = Vector3.Lerp(start, end, t);
            GameObject lightMarker = CreateReplaceablePrimitiveRoot(parent, PrimitiveType.Cube, name + "_Step_" + i.ToString("00"), position, new Vector3(0.42f, 0.1f, 0.42f), color, false);
            Renderer markerRenderer = lightMarker.GetComponentInChildren<Renderer>();
            markerRenderer.material.EnableKeyword("_EMISSION");
            markerRenderer.material.SetColor("_EmissionColor", color * 1.8f);
        }
    }

    private static void CreateNguyenHueBusStop(Transform parent, MemoryZoneController zone)
    {
        GameObject stop = CreateReplaceablePrimitiveRoot(parent, PrimitiveType.Cube, "REPLACE_BusStop_ReturnHub", new Vector3(0, 1.4f, 43), new Vector3(6, 2.8f, 1.2f), BusOrange, true);
        BusStopInteractable interactable = stop.AddComponent<BusStopInteractable>();
        interactable.currentZone = zone;
        interactable.targetScene = SceneLoader.BusHub;
        interactable.interactionPrompt = "Nhấn E để lên xe buýt ký ức";
        zone.busStopReturn = stop;
        AddGlowLight("GLOW_BusStop_ReturnHub", stop.transform, stop.transform.position + Vector3.up, BusOrange, 9f, 2.8f);
        CreateWorldLabel("Trạm xe buýt ký ức", stop.transform, new Vector3(0, 4.6f, 43), 0.45f);
    }

    private static GameObject CreateReplaceablePrimitiveRoot(Transform parent, PrimitiveType type, string name, Vector3 position, Vector3 scale, Color color, bool addCollider)
    {
        GameObject root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = position;
        if (addCollider)
        {
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = scale;
            new GameObject("Collider").transform.SetParent(root.transform);
        }

        string visualName = name.StartsWith("REPLACE_", StringComparison.Ordinal) ? "Visual_" + name : "Visual_REPLACE_" + name;
        CreateBusVisual(type, visualName, root.transform, Vector3.zero, scale, Quaternion.identity, color);
        return root;
    }

    private static void AddNguyenHueRestoreEffects(MemoryZoneController zone, Transform root, Transform effectsRoot)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_NguyenHueEnvironment");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        List<Renderer> restoredRenderers = new List<Renderer>();
        restoredRenderers.AddRange(root.Find("EnvironmentRoot").GetComponentsInChildren<Renderer>());
        restoredRenderers.AddRange(root.Find("LandmarkRoot").GetComponentsInChildren<Renderer>());
        materialEffect.renderers = restoredRenderers.ToArray();
        materialEffect.grayColor = InactiveGray;
        materialEffect.restoredColor = RestoredGreen;

        GameObject fountainEffects = new GameObject("FountainEffects");
        fountainEffects.transform.SetParent(effectsRoot);
        fountainEffects.transform.position = new Vector3(0, 1.5f, 10);
        ParticleSystem waterJets = fountainEffects.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = waterJets.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 5f;
        main.startSize = 0.22f;
        main.startColor = new Color(0.25f, 0.7f, 1f);
        ParticleSystem.EmissionModule emission = waterJets.emission;
        emission.rateOverTime = 28f;
        ParticleRestoreEffect particleEffect = fountainEffects.AddComponent<ParticleRestoreEffect>();
        particleEffect.particles = new[] { waterJets };

        GameObject lightObject = new GameObject("LightRestoreEffect_FountainGlow");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(0, 4, 10);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.3f, 0.8f, 1f);
        light.range = 18f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { light };
        lightEffect.dimIntensity = 0f;
        lightEffect.restoredIntensity = 3f;

        GameObject audioObject = new GameObject("AudioRestoreEffect_NguyenHueMusicPlaceholder");
        audioObject.transform.SetParent(zone.transform);
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        AudioRestoreEffect audioEffect = audioObject.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { source };
        zone.restoreEffects = new RestorableEffect[] { materialEffect, particleEffect, lightEffect, audioEffect };
    }

    private static void CreateNguyenHueSideProps(Transform props)
    {
        for (int i = 0; i < 6; i++)
        {
            float z = -22 + i * 8.5f;
            CreateCube("REPLACE_Prop_NguyenHue_LeftPlanter_" + (i + 1), props, new Vector3(-10.5f, 0.45f, z), new Vector3(4, 0.9f, 1.4f), PlanterGreen);
            CreateCube("REPLACE_Prop_NguyenHue_RightPlanter_" + (i + 1), props, new Vector3(10.5f, 0.45f, z), new Vector3(4, 0.9f, 1.4f), PlanterGreen);

            if (i < 5)
            {
                CreateCube("REPLACE_Prop_NguyenHue_LeftBench_" + (i + 1), props, new Vector3(-15.5f, 0.35f, z + 3f), new Vector3(3.5f, 0.7f, 1.1f), InactiveGray);
                CreateCube("REPLACE_Prop_NguyenHue_RightBench_" + (i + 1), props, new Vector3(15.5f, 0.35f, z + 3f), new Vector3(3.5f, 0.7f, 1.1f), InactiveGray);
            }

            CreateStreetLight(props, "REPLACE_Prop_NguyenHue_LeftStreetLight_" + (i + 1), new Vector3(-22, 2.4f, z));
            CreateStreetLight(props, "REPLACE_Prop_NguyenHue_RightStreetLight_" + (i + 1), new Vector3(22, 2.4f, z));
        }
    }

    private static void CreateNguyenHueBuildings(Transform landmark)
    {
        for (int i = 0; i < 5; i++)
        {
            float z = -24 + i * 12f;
            float height = 7f + (i % 3) * 2f;
            CreateCube("REPLACE_Landmark_NguyenHue_LeftBuilding_" + (i + 1), landmark, new Vector3(-42, height * 0.5f, z), new Vector3(10, height, 8), DarkBuilding);
            CreateCube("REPLACE_Landmark_NguyenHue_RightBuilding_" + (i + 1), landmark, new Vector3(42, height * 0.5f, z + 3f), new Vector3(10, height, 8), DarkBuilding);
        }
    }

    private static void CreateStreetLight(Transform parent, string name, Vector3 position)
    {
        CreateCube(name + "_Pole", parent, position, new Vector3(0.35f, 4.8f, 0.35f), InactiveGray);
        CreateCube(name + "_Lamp", parent, position + new Vector3(0, 2.55f, 0), new Vector3(1.2f, 0.35f, 1.2f), PuzzleYellow);
        AddGlowLight(name + "_Glow", parent, position + new Vector3(0, 2.65f, 0), PuzzleYellow, 8f, 0.75f);
    }

    private static EditorBuildSettingsScene GenerateBenThanh()
    {
        Scene scene = NewScene(SceneLoader.BenThanh);
        GameObject roots = CreateBenThanhRoots();
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform itemRoot = roots.transform.Find("ItemRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform spawnPoints = roots.transform.Find("SpawnPoints");

        CreateManagersAndPlayer(new Vector3(0, 1, -32), "Đi vào khu chợ và tìm người đang giữ ký ức nơi này.", 7f, 2.8f, spawnPoints, 18f, 42f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.BenThanh, "Đời sống thường ngày");

        CreateBenThanhEnvironment(environment, landmark);
        NPCInteractable vendor = CreateBenThanhVendor(npcRoot);
        GameObject memoryHat = CreateBenThanhConicalHat(itemRoot);
        PuzzleInteractable puzzle = CreateBenThanhFruitPuzzle(puzzleRoot, zone);
        BusStopInteractable busStop = CreateBenThanhBusStop(busStopRoot, zone);
        CreateBenThanhGuidance(environment);
        AddBenThanhRestoreEffects(zone, roots.transform, effectsRoot);
        CreateBoundary(environment, 86, 80);

        GameObject controllerObject = new GameObject("BenThanhSceneController");
        controllerObject.transform.SetParent(roots.transform);
        BenThanhSceneController controller = controllerObject.AddComponent<BenThanhSceneController>();
        controller.memoryZone = zone;
        controller.marketVendor = vendor;
        controller.oldConicalHatItem = memoryHat;
        controller.fruitBasketPuzzle = puzzle;
        controller.returnBusStop = busStop;

        BenThanhMemoryItemInteractable itemInteractable = memoryHat.AddComponent<BenThanhMemoryItemInteractable>();
        itemInteractable.sceneController = controller;

        AddLighting();
        return Save(scene, SceneLoader.BenThanh);
    }

    private static GameObject CreateBenThanhRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        CreateChildRoot(root.transform, "EnvironmentRoot");
        CreateChildRoot(root.transform, "LandmarkRoot");
        CreateChildRoot(root.transform, "NPCRoot");
        CreateChildRoot(root.transform, "ItemRoot");
        CreateChildRoot(root.transform, "PuzzleRoot");
        CreateChildRoot(root.transform, "EffectsRoot");
        CreateChildRoot(root.transform, "BusStopRoot");
        CreateChildRoot(root.transform, "SpawnPoints");
        CreateChildRoot(root.transform, "DebugLabels");
        return root;
    }

    private static void CreateBenThanhEnvironment(Transform environment, Transform landmark)
    {
        Transform ground = CreateChildRoot(environment, "Ground");
        Transform paths = CreateChildRoot(environment, "Paths");
        Transform stalls = CreateChildRoot(environment, "Stalls");
        Transform props = CreateChildRoot(environment, "Props");
        Transform lights = CreateChildRoot(environment, "Lights");

        CreateFloor("REPLACE_Environment_BenThanh_Plaza", ground, Vector3.zero, new Vector2(86, 80));
        CreateCube("REPLACE_Environment_BenThanh_Path_Main", paths, new Vector3(0, 0.03f, -4), new Vector3(14, 0.08f, 58), PathGray);
        CreateCube("REPLACE_Environment_BenThanh_Path_SideRoad", paths, new Vector3(28, 0.03f, 4), new Vector3(10, 0.08f, 34), PathGray);
        CreateCube("REPLACE_Environment_BenThanh_Path_StallRow", paths, new Vector3(0, 0.03f, 12), new Vector3(50, 0.08f, 10), PathGray);

        GameObject gate = CreateReplaceablePrimitiveRoot(landmark, PrimitiveType.Cube, "REPLACE_Landmark_BenThanh_Gate", new Vector3(0, 5, 30), new Vector3(28, 10, 3), InactiveGray, true);
        CreateReplaceablePrimitiveRoot(landmark, PrimitiveType.Cube, "REPLACE_Landmark_BenThanh_Clock", new Vector3(0, 12.5f, 30), new Vector3(7, 5, 3.1f), InactiveGray, true);
        CreateWorldLabel("Cổng Chợ Bến Thành", gate.transform, new Vector3(0, 17f, 29.2f), 0.45f);

        for (int i = 0; i < 8; i++)
        {
            float x = -30 + i * 8.5f;
            float z = i % 2 == 0 ? 5 : 17;
            GameObject stall = CreateReplaceablePrimitiveRoot(stalls, PrimitiveType.Cube, "REPLACE_Prop_Stall_" + (i + 1).ToString("00"), new Vector3(x, 1.6f, z), new Vector3(6, 3.2f, 4), new Color(0.32f, 0.29f, 0.25f), true);
            CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_Prop_Stall_Awning_" + (i + 1).ToString("00"), stall.transform, new Vector3(0, 1.9f, 0), new Vector3(6.4f, 0.35f, 4.4f), Quaternion.identity, new Color(0.42f, 0.18f, 0.12f));
            CreateReplaceablePrimitiveRoot(props, PrimitiveType.Cube, "REPLACE_Prop_Crate_" + (i + 1).ToString("00"), new Vector3(x + 2f, 0.6f, z + 2.2f), new Vector3(1.2f, 1.2f, 1.2f), new Color(0.35f, 0.26f, 0.18f), true);
            CreateReplaceablePrimitiveRoot(props, PrimitiveType.Cylinder, "REPLACE_Prop_Basket_" + (i + 1).ToString("00"), new Vector3(x - 1.8f, 0.35f, z + 1.8f), new Vector3(0.65f, 0.35f, 0.65f), new Color(0.38f, 0.3f, 0.2f), true);
        }

        for (int i = 0; i < 4; i++)
        {
            float side = i % 2 == 0 ? -35f : 35f;
            float z = -16 + i * 12f;
            CreateReplaceablePrimitiveRoot(props, PrimitiveType.Cube, "REPLACE_Prop_Cart_" + (i + 1).ToString("00"), new Vector3(side, 0.8f, z), new Vector3(3.6f, 1.6f, 2.2f), new Color(0.28f, 0.21f, 0.16f), true);
            CreateReplaceablePrimitiveRoot(props, PrimitiveType.Cube, "REPLACE_Prop_Signboard_" + (i + 1).ToString("00"), new Vector3(side * 0.85f, 2.2f, z + 2.5f), new Vector3(2.8f, 2.2f, 0.4f), new Color(0.25f, 0.25f, 0.25f), true);
            CreateCube("REPLACE_Prop_Pole_" + (i + 1).ToString("00"), props, new Vector3(side * 0.82f, 1.8f, z + 2.5f), new Vector3(0.2f, 3.6f, 0.2f), InactiveGray);
        }

        for (int i = 0; i < 7; i++)
        {
            float z = -26 + i * 8f;
            CreateCube("REPLACE_Prop_PathLight_" + (i + 1).ToString("00"), lights, new Vector3(-6 + i * 2, 0.1f, z), new Vector3(0.45f, 0.1f, 0.45f), new Color(0.95f, 0.64f, 0.2f));
        }
    }

    private static NPCInteractable CreateBenThanhVendor(Transform npcRoot)
    {
        GameObject npc = new GameObject("REPLACE_NPC_MarketVendor");
        npc.transform.SetParent(npcRoot);
        npc.transform.position = new Vector3(-14, 1, 8);
        npc.AddComponent<CapsuleCollider>();
        new GameObject("Collider").transform.SetParent(npc.transform);
        CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_MarketVendor", npc.transform, Vector3.zero, Vector3.one, Quaternion.identity, NpcPurple);
        AddGlowLight("GLOW_REPLACE_NPC_MarketVendor", npc.transform, npc.transform.position + Vector3.up * 1.1f, NpcPurple, 6f, 2.2f);
        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.interactionPrompt = "Nhấn E để nói chuyện với cô bán hàng";
        CreateWorldLabel("Cô bán hàng", npc.transform, new Vector3(-14, 3.2f, 8), 0.34f, false);
        return interactable;
    }

    private static GameObject CreateBenThanhConicalHat(Transform itemRoot)
    {
        GameObject item = new GameObject("REPLACE_Item_OldConicalHat");
        item.transform.SetParent(itemRoot);
        item.transform.position = new Vector3(18, 0.8f, -6);
        BoxCollider collider = item.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.3f, 1f, 1.3f);
        new GameObject("Collider").transform.SetParent(item.transform);
        CreateBusVisual(PrimitiveType.Cylinder, "Visual_REPLACE_OldConicalHat", item.transform, Vector3.zero, new Vector3(0.8f, 0.35f, 0.8f), Quaternion.identity, InteractableBlue);
        AddGlowLight("GLOW_REPLACE_Item_OldConicalHat", item.transform, item.transform.position + Vector3.up * 0.4f, InteractableBlue, 5f, 1.8f);
        CreateWorldLabel("Nón lá cũ", item.transform, new Vector3(18, 2.6f, -6), 0.32f, false);
        return item;
    }

    private static PuzzleInteractable CreateBenThanhFruitPuzzle(Transform puzzleRoot, MemoryZoneController zone)
    {
        GameObject puzzle = new GameObject("REPLACE_Puzzle_FruitBasket");
        puzzle.transform.SetParent(puzzleRoot);
        puzzle.transform.position = new Vector3(-10, 0.9f, 10);
        BoxCollider collider = puzzle.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.8f, 1.8f, 2.8f);
        new GameObject("Collider").transform.SetParent(puzzle.transform);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_FruitBasket", puzzle.transform, Vector3.zero, new Vector3(3.8f, 1.8f, 2.8f), Quaternion.identity, PuzzleYellow);
        AddGlowLight("GLOW_REPLACE_Puzzle_FruitBasket", puzzle.transform, puzzle.transform.position + Vector3.up * 0.9f, PuzzleYellow, 7f, 2.5f);
        CreateWorldLabel("Giỏ trái cây", puzzle.transform, new Vector3(-10, 3.2f, 10), 0.34f, false);

        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Giỏ trái cây";
        interactable.puzzleDescription = "Điều chỉnh số lượng: Xoài - Cam - Bưởi.";
        interactable.correctAnswer = "1-3-2";
        interactable.inputHint = "Xoài - Cam - Bưởi";
        interactable.useThreeValueStepper = true;
        interactable.stepperLabels = new[] { "Xoài", "Cam", "Bưởi" };
        interactable.wrongFeedback = "Số lượng trái cây chưa đúng. Hãy nhớ lại cách sắp xếp trong khu chợ.";
        interactable.correctFeedback = "Đúng rồi. Ký ức khu chợ đang trở lại.";
        interactable.gameObject.SetActive(false);
        return interactable;
    }

    private static BusStopInteractable CreateBenThanhBusStop(Transform busStopRoot, MemoryZoneController zone)
    {
        GameObject busStop = new GameObject("REPLACE_BusStop_ReturnHub");
        busStop.transform.SetParent(busStopRoot);
        busStop.transform.position = new Vector3(32, 1.4f, 4);
        BoxCollider collider = busStop.AddComponent<BoxCollider>();
        collider.size = new Vector3(5.5f, 2.8f, 1.1f);
        new GameObject("Collider").transform.SetParent(busStop.transform);
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BusStop", busStop.transform, Vector3.zero, new Vector3(5.5f, 2.8f, 1.1f), Quaternion.identity, BusOrange);
        AddGlowLight("GLOW_REPLACE_BusStop_ReturnHub", busStop.transform, busStop.transform.position + Vector3.up * 1.1f, BusOrange, 9f, 2.6f);
        CreateWorldLabel("Trạm xe buýt ký ức", busStop.transform, new Vector3(32, 4.5f, 4), 0.36f, false);

        BusStopInteractable interactable = busStop.AddComponent<BusStopInteractable>();
        interactable.currentZone = zone;
        interactable.targetScene = SceneLoader.BusHub;
        interactable.interactionPrompt = "Nhấn E để lên xe buýt ký ức.";
        zone.busStopReturn = busStop;
        return interactable;
    }

    private static void CreateBenThanhGuidance(Transform environment)
    {
        for (int i = 0; i < 12; i++)
        {
            float z = -28 + i * 3.5f;
            GameObject marker = CreateReplaceablePrimitiveRoot(environment, PrimitiveType.Cube, "REPLACE_Prop_GuideLight_" + (i + 1).ToString("00"), new Vector3(-6 + i * 0.75f, 0.1f, z), new Vector3(0.35f, 0.1f, 0.35f), new Color(1f, 0.66f, 0.23f), false);
            Renderer renderer = marker.GetComponentInChildren<Renderer>();
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", new Color(1f, 0.62f, 0.15f) * 1.8f);
        }
    }

    private static void AddBenThanhRestoreEffects(MemoryZoneController zone, Transform root, Transform effectsRoot)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_BenThanh");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = root.GetComponentsInChildren<Renderer>();
        materialEffect.grayColor = InactiveGray;
        materialEffect.restoredColor = new Color(0.88f, 0.63f, 0.36f);

        GameObject lightObject = new GameObject("LightRestoreEffect_MarketWarm");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(0, 7, 10);
        Light warmLight = lightObject.AddComponent<Light>();
        warmLight.type = LightType.Point;
        warmLight.color = new Color(1f, 0.72f, 0.38f);
        warmLight.range = 44f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { warmLight };
        lightEffect.dimIntensity = 0.1f;
        lightEffect.restoredIntensity = 2.5f;

        GameObject ambienceObject = new GameObject("AudioRestoreEffect_MarketAmbience");
        ambienceObject.transform.SetParent(zone.transform);
        ambienceObject.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;
        ambience.volume = 0f;
        AudioRestoreEffect audioEffect = ambienceObject.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { ambience };
        audioEffect.restoredVolume = 0.35f;

        zone.restoreEffects = new RestorableEffect[] { materialEffect, lightEffect, audioEffect };
    }

    private static EditorBuildSettingsScene GenerateDinhDocLap()
    {
        Scene scene = NewScene(SceneLoader.DinhDocLap);
        GameObject roots = CreateDinhDocLapRoots();
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform itemRoot = roots.transform.Find("ItemRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform debugLabels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(0, 1, -35f), "Đi đến cổng Dinh và tìm mảnh bản đồ lịch sử.", 7.4f, 2.9f, spawnPoints, 18f, 46f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.DinhDocLap, "Lịch sử");

        CreateCube("Ground", environment, new Vector3(0, -0.05f, 0), new Vector3(90, 0.1f, 80), InactiveGray);
        CreateCube("Courtyard", environment, new Vector3(0, 0.01f, 0), new Vector3(84, 0.04f, 72), new Color(0.41f, 0.43f, 0.45f));
        CreateCube("Path", environment, new Vector3(0, 0.03f, -2f), new Vector3(14, 0.06f, 62), PathGray);
        CreateCube("GrassArea_Left", environment, new Vector3(-25f, 0.02f, 0), new Vector3(24, 0.05f, 58), new Color(0.27f, 0.29f, 0.31f));
        CreateCube("GrassArea_Right", environment, new Vector3(25f, 0.02f, 0), new Vector3(24, 0.05f, 58), new Color(0.27f, 0.29f, 0.31f));

        CreateCube("REPLACE_Prop_Fog_Main", environment, new Vector3(0, 2f, 4f), new Vector3(90, 4f, 80), new Color(0.55f, 0.57f, 0.6f, 0.16f));
        CreateCube("REPLACE_Prop_Fog_Palace", environment, new Vector3(0, 2.3f, 24f), new Vector3(58, 4.2f, 28), new Color(0.55f, 0.57f, 0.6f, 0.2f));

        for (int i = 0; i < 11; i++)
        {
            float z = -30f + i * 5.5f;
            GameObject marker = CreateCube("REPLACE_PathLight_" + (i + 1).ToString("00"), effectsRoot, new Vector3(0, 0.08f, z), new Vector3(0.9f, 0.12f, 0.5f), new Color(0.9f, 0.76f, 0.28f));
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.EnableKeyword("_EMISSION");
            markerRenderer.material.SetColor("_EmissionColor", new Color(0.95f, 0.8f, 0.3f) * 1.2f);
        }

        CreateCube("REPLACE_Landmark_DinhDocLap_Gate", landmark, new Vector3(0, 3.8f, -31f), new Vector3(30, 7.6f, 2.2f), InactiveGray);
        CreateCube("REPLACE_Prop_GateColumn_Left", landmark, new Vector3(-14f, 4f, -30.5f), new Vector3(3, 8, 3), InactiveGray);
        CreateCube("REPLACE_Prop_GateColumn_Right", landmark, new Vector3(14f, 4f, -30.5f), new Vector3(3, 8, 3), InactiveGray);
        CreateCube("REPLACE_Landmark_DinhDocLap_Palace", landmark, new Vector3(0, 6f, 29.5f), new Vector3(36, 12, 10), InactiveGray);
        CreateCube("REPLACE_Prop_PalaceHall", landmark, new Vector3(0, 2.2f, 22f), new Vector3(22, 4.4f, 6), InactiveGray);

        CreateWorldLabel("Dinh Độc Lập", landmark, new Vector3(0, 13f, 27f), 0.55f);
        CreateWorldLabel("Player Spawn", spawnPoints, new Vector3(0, 2.8f, -35f), 0.32f);
        CreateWorldLabel("Landmark Gate", landmark, new Vector3(0, 8.5f, -31f), 0.32f);
        CreateWorldLabel("Landmark Palace", landmark, new Vector3(0, 14f, 29.5f), 0.36f);

        CreateCube("REPLACE_Prop_FlagPole", environment, new Vector3(0, 6.5f, 15f), new Vector3(0.3f, 13f, 0.3f), new Color(0.4f, 0.4f, 0.42f));
        GameObject flag = CreateCube("REPLACE_Prop_Flag", environment, new Vector3(1.8f, 9.2f, 15f), new Vector3(3.2f, 1.8f, 0.15f), new Color(0.5f, 0.12f, 0.12f));

        for (int i = 0; i < 4; i++)
        {
            float x = -32f + i * 21.5f;
            CreateCube("REPLACE_Prop_InfoBoard_" + (i + 1), environment, new Vector3(x, 1.6f, -18f), new Vector3(3.2f, 3.2f, 0.5f), InactiveGray);
        }

        for (int i = 0; i < 8; i++)
        {
            float x = i < 4 ? -30f + i * 7.5f : 6f + (i - 4) * 7.5f;
            float z = i < 4 ? 16f : 10f;
            CreateCube("REPLACE_Prop_Tree_" + (i + 1).ToString("00"), environment, new Vector3(x, 2f, z), new Vector3(2.6f, 4f, 2.6f), InactiveGray);
        }

        GameObject npc = CreateDinhNpc(npcRoot, debugLabels, new Vector3(-9f, 1f, 22f));
        GameObject item = CreateDinhMapItem(itemRoot, debugLabels, new Vector3(10f, 0.65f, -24f));
        GameObject puzzle = CreateDinhPuzzle(puzzleRoot, debugLabels, zone, new Vector3(8f, 1f, 24.5f));
        GameObject busStop = CreateDinhBusStop(busStopRoot, debugLabels, zone, new Vector3(33f, 1.2f, 30f));

        CreateDinhDocLapEffects(zone, roots.transform, effectsRoot, flag);
        CreateBoundary(landmark, 90, 80);
        AddLighting();

        DinhDocLapSceneController controller = roots.AddComponent<DinhDocLapSceneController>();
        controller.memoryZone = zone;
        controller.tourGuideNpc = npc.GetComponent<NPCInteractable>();
        controller.historicalMapItem = item.GetComponent<DinhDocLapMapItemInteractable>();
        controller.radioPuzzle = puzzle.GetComponent<PuzzleInteractable>();
        controller.returnBusStop = busStop.GetComponent<BusStopInteractable>();

        return Save(scene, SceneLoader.DinhDocLap);
    }

    private static GameObject CreateDinhDocLapRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[]
                 {
                     "EnvironmentRoot", "LandmarkRoot", "NPCRoot", "ItemRoot", "PuzzleRoot", "EffectsRoot", "BusStopRoot", "SpawnPoints", "DebugLabels"
                 })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static GameObject CreateDinhNpc(Transform npcRoot, Transform labelRoot, Vector3 position)
    {
        GameObject npc = new GameObject("REPLACE_NPC_OldTourGuide");
        npc.transform.SetParent(npcRoot);
        npc.transform.position = position;
        CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.45f;
        new GameObject("Collider").transform.SetParent(npc.transform);

        NPCInteractable npcInteractable = npc.AddComponent<NPCInteractable>();
        npcInteractable.interactionPrompt = "Nhấn E để nói chuyện với hướng dẫn viên";

        GameObject visual = CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_OldTourGuide", npc.transform, Vector3.zero, Vector3.one * 1f, Quaternion.identity, NpcPurple);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", NpcPurple * 1.3f);
        AddGlowLight("GLOW_REPLACE_NPC_OldTourGuide", npc.transform, Vector3.up * 1.4f, NpcPurple, 6f, 1.2f);

        CreateWorldLabel("NPC: Hướng dẫn viên", labelRoot, position + Vector3.up * 2.4f, 0.33f);
        return npc;
    }

    private static GameObject CreateDinhMapItem(Transform itemRoot, Transform labelRoot, Vector3 position)
    {
        GameObject item = new GameObject("REPLACE_Item_HistoricalMap");
        item.transform.SetParent(itemRoot);
        item.transform.position = position;
        SphereCollider collider = item.AddComponent<SphereCollider>();
        collider.radius = 0.75f;
        new GameObject("Collider").transform.SetParent(item.transform);

        DinhDocLapMapItemInteractable interactable = item.AddComponent<DinhDocLapMapItemInteractable>();
        interactable.interactionPrompt = "Nhấn E để nhặt mảnh bản đồ";

        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_HistoricalMap", item.transform, Vector3.zero, new Vector3(1.8f, 0.25f, 1.2f), Quaternion.identity, InteractableBlue);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", InteractableBlue * 1.4f);
        AddGlowLight("GLOW_REPLACE_Item_HistoricalMap", item.transform, Vector3.up * 0.8f, InteractableBlue, 5f, 1f);

        CreateWorldLabel("Memory Item: Bản đồ lịch sử", labelRoot, position + Vector3.up * 1.9f, 0.3f);
        return item;
    }

    private static GameObject CreateDinhPuzzle(Transform puzzleRoot, Transform labelRoot, MemoryZoneController zone, Vector3 position)
    {
        GameObject puzzle = new GameObject("REPLACE_Puzzle_Radio1975");
        puzzle.transform.SetParent(puzzleRoot);
        puzzle.transform.position = position;
        BoxCollider collider = puzzle.AddComponent<BoxCollider>();
        collider.size = new Vector3(3.8f, 1.7f, 2.8f);
        new GameObject("Collider").transform.SetParent(puzzle.transform);

        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Mật mã radio lịch sử";
        interactable.puzzleDescription = "Quy luật hình số học dẫn về một cột mốc lịch sử.";
        interactable.correctAnswer = "1975";
        interactable.inputHint = "Nhập 4 chữ số";
        interactable.wrongFeedback = "Mật mã chưa đúng. Đài radio vẫn còn nhiễu sóng.";
        interactable.correctFeedback = "Mật mã chính xác. Tín hiệu lịch sử đã quay lại.";

        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_Radio1975", puzzle.transform, Vector3.zero, new Vector3(3.8f, 1.7f, 2.8f), Quaternion.identity, PuzzleYellow);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.25f);
        AddGlowLight("GLOW_REPLACE_Puzzle_Radio1975", puzzle.transform, Vector3.up * 1.4f, PuzzleYellow, 7f, 1.3f);

        CreateWorldLabel("Puzzle: Radio 1975", labelRoot, position + Vector3.up * 2.35f, 0.32f);
        puzzle.SetActive(false);
        return puzzle;
    }

    private static GameObject CreateDinhBusStop(Transform busStopRoot, Transform labelRoot, MemoryZoneController zone, Vector3 position)
    {
        GameObject busStop = new GameObject("REPLACE_BusStop_ReturnHub");
        busStop.transform.SetParent(busStopRoot);
        busStop.transform.position = position;
        BoxCollider collider = busStop.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 2.4f, 1f);
        new GameObject("Collider").transform.SetParent(busStop.transform);

        BusStopInteractable busStopInteractable = busStop.AddComponent<BusStopInteractable>();
        busStopInteractable.currentZone = zone;
        busStopInteractable.targetScene = SceneLoader.BusHub;
        busStopInteractable.interactionPrompt = "Nhấn E để lên xe buýt ký ức.";

        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.zero, new Vector3(4f, 2.4f, 1f), Quaternion.identity, BusOrange);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", BusOrange * 1.3f);
        AddGlowLight("GLOW_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.up * 1f, BusOrange, 8f, 1.6f);

        zone.busStopReturn = busStop;
        CreateWorldLabel("Bus Stop", labelRoot, position + Vector3.up * 2.6f, 0.33f);
        return busStop;
    }

    private static void CreateDinhDocLapEffects(MemoryZoneController zone, Transform sceneRoot, Transform effectsRoot, GameObject flag)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_DinhDocLap");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = sceneRoot.GetComponentsInChildren<Renderer>();
        materialEffect.grayColor = InactiveGray;
        materialEffect.restoredColor = new Color(0.58f, 0.72f, 0.5f);

        GameObject warmLightObj = new GameObject("LightRestoreEffect_PalaceWarm");
        warmLightObj.transform.SetParent(zone.transform);
        warmLightObj.transform.position = new Vector3(0, 7f, 24f);
        Light warmLight = warmLightObj.AddComponent<Light>();
        warmLight.type = LightType.Point;
        warmLight.color = new Color(1f, 0.73f, 0.42f);
        warmLight.range = 44f;
        LightRestoreEffect lightEffect = warmLightObj.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { warmLight };
        lightEffect.dimIntensity = 0.05f;
        lightEffect.restoredIntensity = 2.3f;

        GameObject ambienceObj = new GameObject("AudioRestoreEffect_HistoryAmbience");
        ambienceObj.transform.SetParent(zone.transform);
        ambienceObj.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObj.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;
        ambience.volume = 0f;
        AudioRestoreEffect audioEffect = ambienceObj.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { ambience };
        audioEffect.restoredVolume = 0.32f;

        zone.restoreEffects = new RestorableEffect[] { materialEffect, lightEffect, audioEffect };

        if (flag != null)
        {
            flag.SetActive(false);
            DinhDocLapSceneControllerFlagHelper helper = flag.AddComponent<DinhDocLapSceneControllerFlagHelper>();
            helper.zone = zone;
        }
    }

    private static EditorBuildSettingsScene GenerateNhaThoDucBa()
    {
        Scene scene = NewScene(SceneLoader.NhaThoDucBa);
        GameObject roots = CreateNhaThoRoots();
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform itemRoot = roots.transform.Find("ItemRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform debugLabels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(0, 1, -33f), "Đi vào quảng trường và tìm khoảng lặng bị đánh mất.", 7.1f, 2.85f, spawnPoints, 18f, 46f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.NhaThoDucBa, "Bình yên");

        CreateCube("Ground", environment, new Vector3(0, -0.05f, 0), new Vector3(80, 0.1f, 80), InactiveGray);
        CreateCube("Plaza", environment, new Vector3(0, 0.01f, 0), new Vector3(72, 0.05f, 68), new Color(0.43f, 0.44f, 0.46f));
        CreateCube("Path_Main", environment, new Vector3(0, 0.03f, -4f), new Vector3(12, 0.06f, 46f), PathGray);
        CreateCube("Path_StatueRing", environment, new Vector3(0, 0.04f, 8f), new Vector3(18, 0.06f, 9f), PathGray);

        GameObject cathedral = CreateCube("REPLACE_Landmark_NhaThoDucBa_Cathedral", landmark, new Vector3(0, 7f, 28f), new Vector3(26, 14, 5), InactiveGray);
        GameObject towerLeft = CreateCube("REPLACE_Landmark_NhaThoDucBa_TowerLeft", landmark, new Vector3(-15f, 12f, 28f), new Vector3(6, 24, 5), InactiveGray);
        GameObject towerRight = CreateCube("REPLACE_Landmark_NhaThoDucBa_TowerRight", landmark, new Vector3(15f, 12f, 28f), new Vector3(6, 24, 5), InactiveGray);
        CreateCube("REPLACE_Prop_CathedralDoor", landmark, new Vector3(0, 2f, 25.5f), new Vector3(6, 4, 1.2f), InactiveGray);

        CreateCube("REPLACE_Prop_Statue", environment, new Vector3(0, 2.2f, 8f), new Vector3(3.2f, 4.4f, 3.2f), InactiveGray);
        CreateCube("REPLACE_Prop_StatueBase", environment, new Vector3(0, 0.7f, 8f), new Vector3(8.5f, 1.4f, 8.5f), InactiveGray);

        for (int i = 0; i < 6; i++)
        {
            float x = -24f + i * 9.6f;
            CreateCube("REPLACE_Prop_Bench_" + (i + 1), environment, new Vector3(x, 0.5f, -8f), new Vector3(4.8f, 1f, 1.6f), InactiveGray);
        }

        for (int i = 0; i < 6; i++)
        {
            float x = -21f + i * 8.4f;
            float z = 2f + (i % 2) * 4.5f;
            CreateCube("REPLACE_Prop_Tree_" + (i + 1), environment, new Vector3(x, 2.4f, z), new Vector3(2.4f, 4.8f, 2.4f), InactiveGray);
        }

        Transform pigeonsRoot = CreateChildRoot(environment, "Pigeons");
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 4.2f, 0.35f, 8f + Mathf.Sin(angle) * 3f);
            GameObject pigeon = CreateBusVisual(PrimitiveType.Sphere, "REPLACE_Prop_Pigeon_" + (i + 1).ToString("00"), pigeonsRoot, pos, Vector3.one * 0.55f, Quaternion.identity, new Color(0.55f, 0.56f, 0.58f));
            PigeonRiseEffect riseEffect = pigeon.AddComponent<PigeonRiseEffect>();
            riseEffect.riseHeight = 4.2f + i * 0.18f;
            riseEffect.duration = 1.6f;
            riseEffect.startDelay = i * 0.07f;
        }

        for (int i = 0; i < 10; i++)
        {
            float z = -29f + i * 5.1f;
            GameObject marker = CreateCube("REPLACE_PathGuide_" + (i + 1).ToString("00"), effectsRoot, new Vector3(0, 0.08f, z), new Vector3(0.85f, 0.1f, 0.45f), new Color(0.92f, 0.76f, 0.26f));
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.EnableKeyword("_EMISSION");
            markerRenderer.material.SetColor("_EmissionColor", new Color(0.96f, 0.78f, 0.28f) * 1.15f);
        }

        GameObject npc = CreateNhaThoNpc(npcRoot, debugLabels, new Vector3(-6f, 1f, 9f));
        GameObject item = CreateNhaThoItem(itemRoot, debugLabels, new Vector3(15f, 0.65f, -7f));
        GameObject puzzle = CreateNhaThoPuzzle(puzzleRoot, debugLabels, zone, new Vector3(10.5f, 1f, 23.5f));
        GameObject busStop = CreateNhaThoBusStop(busStopRoot, debugLabels, zone, new Vector3(30f, 1.2f, -18f));

        CreateWorldLabel("Nhà thờ Đức Bà", landmark, new Vector3(0, 25f, 27f), 0.55f);
        CreateWorldLabel("Player Spawn", spawnPoints, new Vector3(0, 2.8f, -33f), 0.32f);
        CreateWorldLabel("Landmark", landmark, new Vector3(0, 15f, 27f), 0.34f);

        CreateNhaThoEffects(zone, roots.transform, effectsRoot, new[] { cathedral, towerLeft, towerRight }, pigeonsRoot);
        CreateBoundary(landmark, 80, 80);
        AddLighting();

        NhaThoDucBaSceneController controller = roots.AddComponent<NhaThoDucBaSceneController>();
        controller.memoryZone = zone;
        controller.pigeonFeederNpc = npc.GetComponent<NPCInteractable>();
        controller.smallBellItem = item.GetComponent<NhaThoSmallBellInteractable>();
        controller.bellPuzzle = puzzle.GetComponent<PuzzleInteractable>();
        controller.returnBusStop = busStop.GetComponent<BusStopInteractable>();

        return Save(scene, SceneLoader.NhaThoDucBa);
    }

    private static GameObject CreateNhaThoRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[]
                 {
                     "EnvironmentRoot", "LandmarkRoot", "NPCRoot", "ItemRoot", "PuzzleRoot", "EffectsRoot", "BusStopRoot", "SpawnPoints", "DebugLabels"
                 })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static GameObject CreateNhaThoNpc(Transform npcRoot, Transform labelRoot, Vector3 position)
    {
        GameObject npc = new GameObject("REPLACE_NPC_PigeonFeeder");
        npc.transform.SetParent(npcRoot);
        npc.transform.position = position;
        CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.45f;
        new GameObject("Collider").transform.SetParent(npc.transform);

        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.interactionPrompt = "Nhấn E để nói chuyện với cụ già";

        GameObject visual = CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_PigeonFeeder", npc.transform, Vector3.zero, Vector3.one, Quaternion.identity, NpcPurple);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", NpcPurple * 1.35f);
        AddGlowLight("GLOW_REPLACE_NPC_PigeonFeeder", npc.transform, Vector3.up * 1.4f, NpcPurple, 6f, 1.2f);
        CreateWorldLabel("NPC: Cụ cho bồ câu ăn", labelRoot, position + Vector3.up * 2.35f, 0.32f);
        return npc;
    }

    private static GameObject CreateNhaThoItem(Transform itemRoot, Transform labelRoot, Vector3 position)
    {
        GameObject item = new GameObject("REPLACE_Item_SmallBell");
        item.transform.SetParent(itemRoot);
        item.transform.position = position;
        SphereCollider collider = item.AddComponent<SphereCollider>();
        collider.radius = 0.72f;
        new GameObject("Collider").transform.SetParent(item.transform);

        NhaThoSmallBellInteractable interactable = item.AddComponent<NhaThoSmallBellInteractable>();
        interactable.interactionPrompt = "Nhấn E để nhặt chuông nhỏ";

        GameObject visual = CreateBusVisual(PrimitiveType.Sphere, "Visual_REPLACE_SmallBell", item.transform, Vector3.zero, Vector3.one * 1.1f, Quaternion.identity, InteractableBlue);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", InteractableBlue * 1.3f);
        AddGlowLight("GLOW_REPLACE_Item_SmallBell", item.transform, Vector3.up * 0.8f, InteractableBlue, 5f, 1f);
        CreateWorldLabel("Memory Item: Chuông nhỏ", labelRoot, position + Vector3.up * 1.9f, 0.3f);
        return item;
    }

    private static GameObject CreateNhaThoPuzzle(Transform puzzleRoot, Transform labelRoot, MemoryZoneController zone, Vector3 position)
    {
        GameObject puzzle = new GameObject("REPLACE_Puzzle_BellSequence");
        puzzle.transform.SetParent(puzzleRoot);
        puzzle.transform.position = position;
        BoxCollider collider = puzzle.AddComponent<BoxCollider>();
        collider.size = new Vector3(4.2f, 1.8f, 2.9f);
        new GameObject("Collider").transform.SetParent(puzzle.transform);

        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Hòa âm tháp chuông";
        interactable.puzzleDescription = "Sắp xếp 6 tiếng chuông theo thứ tự đúng.";
        interactable.correctAnswer = "La-Sol-Re-Mi-Si-Do";
        interactable.inputHint = "La-Sol-Re-Mi-Si-Do";
        interactable.quickChoices = new[] { "La", "Sol", "Re", "Mi", "Si", "Do" };
        interactable.wrongFeedback = "Thứ tự chuông chưa tạo được hòa âm bình yên.";
        interactable.correctFeedback = "Hòa âm chính xác. Tháp chuông đã thức giấc.";

        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BellSequence", puzzle.transform, Vector3.zero, new Vector3(4.2f, 1.8f, 2.9f), Quaternion.identity, PuzzleYellow);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.25f);
        AddGlowLight("GLOW_REPLACE_Puzzle_BellSequence", puzzle.transform, Vector3.up * 1.35f, PuzzleYellow, 7f, 1.3f);
        CreateWorldLabel("Puzzle: Bell Sequence", labelRoot, position + Vector3.up * 2.35f, 0.32f);
        puzzle.SetActive(false);
        return puzzle;
    }

    private static GameObject CreateNhaThoBusStop(Transform busStopRoot, Transform labelRoot, MemoryZoneController zone, Vector3 position)
    {
        GameObject busStop = new GameObject("REPLACE_BusStop_ReturnHub");
        busStop.transform.SetParent(busStopRoot);
        busStop.transform.position = position;
        BoxCollider collider = busStop.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 2.4f, 1f);
        new GameObject("Collider").transform.SetParent(busStop.transform);

        BusStopInteractable busStopInteractable = busStop.AddComponent<BusStopInteractable>();
        busStopInteractable.currentZone = zone;
        busStopInteractable.targetScene = SceneLoader.BusHub;
        busStopInteractable.interactionPrompt = "Nhấn E để lên xe buýt ký ức.";

        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.zero, new Vector3(4f, 2.4f, 1f), Quaternion.identity, BusOrange);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", BusOrange * 1.3f);
        AddGlowLight("GLOW_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.up * 1f, BusOrange, 8f, 1.6f);

        zone.busStopReturn = busStop;
        CreateWorldLabel("Bus Stop", labelRoot, position + Vector3.up * 2.6f, 0.33f);
        return busStop;
    }

    private static void CreateNhaThoEffects(MemoryZoneController zone, Transform sceneRoot, Transform effectsRoot, GameObject[] cathedralParts, Transform pigeonsRoot)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_NhaTho");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = sceneRoot.GetComponentsInChildren<Renderer>();
        materialEffect.grayColor = InactiveGray;
        materialEffect.restoredColor = new Color(0.68f, 0.42f, 0.36f);

        GameObject lightObject = new GameObject("LightRestoreEffect_WarmSquare");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(0, 8f, 16f);
        Light warmLight = lightObject.AddComponent<Light>();
        warmLight.type = LightType.Point;
        warmLight.color = new Color(1f, 0.76f, 0.45f);
        warmLight.range = 48f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { warmLight };
        lightEffect.dimIntensity = 0.08f;
        lightEffect.restoredIntensity = 2.4f;

        GameObject ambienceObject = new GameObject("AudioRestoreEffect_BellAmbience");
        ambienceObject.transform.SetParent(zone.transform);
        ambienceObject.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;
        ambience.volume = 0f;
        AudioRestoreEffect audioEffect = ambienceObject.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { ambience };
        audioEffect.restoredVolume = 0.33f;

        zone.restoreEffects = new RestorableEffect[] { materialEffect, lightEffect, audioEffect };

        if (cathedralParts != null)
        {
            for (int i = 0; i < cathedralParts.Length; i++)
            {
                if (cathedralParts[i] == null)
                {
                    continue;
                }

                Renderer renderer = cathedralParts[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = InactiveGray;
                }
            }
        }

        if (pigeonsRoot != null)
        {
            PigeonRiseEffect[] riseEffects = pigeonsRoot.GetComponentsInChildren<PigeonRiseEffect>(true);
            for (int i = 0; i < riseEffects.Length; i++)
            {
                riseEffects[i].zone = zone;
            }
        }
    }

    private static EditorBuildSettingsScene GenerateBitexco()
    {
        Scene scene = NewScene(SceneLoader.Bitexco);
        GameObject roots = CreateBitexcoRoots();
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform itemRoot = roots.transform.Find("ItemRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform debugLabels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(0, 1, -33f), "Đi vào quảng trường Bitexco và tìm dấu vết của nhịp sống hiện đại.", 7.2f, 2.9f, spawnPoints, 18f, 46f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.Bitexco, "Chuyển mình");

        CreateCube("Ground", environment, new Vector3(0, -0.05f, 0), new Vector3(80, 0.1f, 80), new Color(0.18f, 0.2f, 0.24f));
        CreateCube("Plaza", environment, new Vector3(0, 0.01f, 0), new Vector3(70, 0.05f, 66), new Color(0.24f, 0.26f, 0.3f));
        CreateCube("Path", environment, new Vector3(0, 0.03f, -4f), new Vector3(13, 0.06f, 48), new Color(0.3f, 0.33f, 0.37f));

        GameObject tower = CreateCube("REPLACE_Landmark_Bitexco_Tower", landmark, new Vector3(0, 22, 20), new Vector3(12, 44, 12), new Color(0.2f, 0.23f, 0.28f));
        GameObject helipad = CreateCube("REPLACE_Landmark_Bitexco_Helipad", landmark, new Vector3(8.5f, 30, 20), new Vector3(9, 0.8f, 7), new Color(0.24f, 0.26f, 0.3f));
        CreateCube("REPLACE_Prop_LobbyEntrance", landmark, new Vector3(0, 2.6f, 9.8f), new Vector3(18, 5.2f, 3.2f), new Color(0.2f, 0.22f, 0.27f));

        Transform towerLights = CreateChildRoot(landmark, "TowerLights");
        for (int i = 0; i < 8; i++)
        {
            float y = 3f + i * 5f;
            CreateCube("REPLACE_Prop_TowerLightStrip_" + (i + 1).ToString("00"), towerLights, new Vector3(-2.8f, y, 13.8f), new Vector3(0.7f, 3.2f, 0.15f), new Color(0.22f, 0.24f, 0.28f));
            CreateCube("REPLACE_Prop_TowerLightStrip_R_" + (i + 1).ToString("00"), towerLights, new Vector3(2.8f, y, 13.8f), new Vector3(0.7f, 3.2f, 0.15f), new Color(0.22f, 0.24f, 0.28f));
        }

        for (int i = 0; i < 4; i++)
        {
            float x = -28 + i * 18.5f;
            CreateCube("REPLACE_Prop_NeonBillboard_" + (i + 1), environment, new Vector3(x, 5f, 0), new Vector3(8f, 6f, 0.7f), new Color(0.18f, 0.2f, 0.24f));
        }

        for (int i = 0; i < 5; i++)
        {
            float z = -27 + i * 4.8f;
            GameObject marker = CreateCube("REPLACE_Prop_NeonStrip_" + (i + 1), environment, new Vector3(0, 0.08f, z), new Vector3(0.85f, 0.12f, 0.45f), new Color(0.38f, 0.38f, 0.42f));
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.EnableKeyword("_EMISSION");
            markerRenderer.material.SetColor("_EmissionColor", new Color(0.18f, 0.18f, 0.2f));
        }

        CreateCube("REPLACE_Prop_SecurityGate", environment, new Vector3(0, 1.4f, 8f), new Vector3(12, 2.8f, 0.6f), InactiveGray);
        CreateCube("REPLACE_Prop_OfficeDesk_01", environment, new Vector3(-6f, 1f, 7f), new Vector3(3, 2, 2), InactiveGray);
        CreateCube("REPLACE_Prop_OfficeDesk_02", environment, new Vector3(6f, 1f, 7f), new Vector3(3, 2, 2), InactiveGray);
        CreateCube("REPLACE_Prop_ServerRack_01", environment, new Vector3(-4f, 2.2f, 15f), new Vector3(2.2f, 4.4f, 2), InactiveGray);
        CreateCube("REPLACE_Prop_ServerRack_02", environment, new Vector3(4f, 2.2f, 15f), new Vector3(2.2f, 4.4f, 2), InactiveGray);

        GameObject npc = CreateBitexcoNpc(npcRoot, debugLabels, new Vector3(-7f, 1f, 6f));
        GameObject item = CreateBitexcoItem(itemRoot, debugLabels, new Vector3(8f, 0.65f, 5.5f));
        GameObject puzzle = CreateBitexcoPuzzle(puzzleRoot, debugLabels, zone, new Vector3(0, 1f, 14f));
        GameObject busStop = CreateBitexcoBusStop(busStopRoot, debugLabels, zone, new Vector3(30f, 1.2f, -16f));

        CreateWorldLabel("Bitexco Tower", landmark, new Vector3(0, 46f, 20f), 0.55f);
        CreateWorldLabel("Player Spawn", spawnPoints, new Vector3(0, 2.8f, -33f), 0.32f);

        CreateBitexcoEffects(zone, roots.transform, effectsRoot, tower, helipad, towerLights);
        CreateBoundary(landmark, 80, 80);
        AddLighting();

        BitexcoSceneController controller = roots.AddComponent<BitexcoSceneController>();
        controller.memoryZone = zone;
        controller.officeWorkerNpc = npc.GetComponent<NPCInteractable>();
        controller.employeeCardItem = item.GetComponent<BitexcoEmployeeCardInteractable>();
        controller.securityPuzzle = puzzle.GetComponent<PuzzleInteractable>();
        controller.returnBusStop = busStop.GetComponent<BusStopInteractable>();

        return Save(scene, SceneLoader.Bitexco);
    }

    private static GameObject CreateBitexcoRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[]
                 {
                     "EnvironmentRoot", "LandmarkRoot", "NPCRoot", "ItemRoot", "PuzzleRoot", "EffectsRoot", "BusStopRoot", "SpawnPoints", "DebugLabels"
                 })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static GameObject CreateBitexcoNpc(Transform root, Transform labels, Vector3 position)
    {
        GameObject npc = new GameObject("REPLACE_NPC_OfficeWorkerCFO");
        npc.transform.SetParent(root);
        npc.transform.position = position;
        CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.45f;
        new GameObject("Collider").transform.SetParent(npc.transform);
        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.interactionPrompt = "Nhấn E để nói chuyện với nhân viên";
        GameObject visual = CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_OfficeWorkerCFO", npc.transform, Vector3.zero, Vector3.one, Quaternion.identity, NpcPurple);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", NpcPurple * 1.35f);
        AddGlowLight("GLOW_REPLACE_NPC_OfficeWorkerCFO", npc.transform, Vector3.up * 1.4f, NpcPurple, 6f, 1.2f);
        CreateWorldLabel("NPC: Office Worker", labels, position + Vector3.up * 2.35f, 0.32f);
        return npc;
    }

    private static GameObject CreateBitexcoItem(Transform root, Transform labels, Vector3 position)
    {
        GameObject item = new GameObject("REPLACE_Item_EmployeeCard");
        item.transform.SetParent(root);
        item.transform.position = position;
        SphereCollider collider = item.AddComponent<SphereCollider>();
        collider.radius = 0.72f;
        new GameObject("Collider").transform.SetParent(item.transform);
        BitexcoEmployeeCardInteractable interactable = item.AddComponent<BitexcoEmployeeCardInteractable>();
        interactable.interactionPrompt = "Nhấn E để nhặt thẻ nhân viên";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_EmployeeCard", item.transform, Vector3.zero, new Vector3(1.5f, 0.2f, 0.9f), Quaternion.identity, InteractableBlue);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", InteractableBlue * 1.3f);
        AddGlowLight("GLOW_REPLACE_Item_EmployeeCard", item.transform, Vector3.up * 0.8f, InteractableBlue, 5f, 1f);
        CreateWorldLabel("Item: Employee Card", labels, position + Vector3.up * 1.9f, 0.3f);
        return item;
    }

    private static GameObject CreateBitexcoPuzzle(Transform root, Transform labels, MemoryZoneController zone, Vector3 position)
    {
        GameObject puzzle = new GameObject("REPLACE_Puzzle_SecurityServer345");
        puzzle.transform.SetParent(root);
        puzzle.transform.position = position;
        BoxCollider collider = puzzle.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 1.8f, 2.9f);
        new GameObject("Collider").transform.SetParent(puzzle.transform);
        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Mã bảo mật tài chính";
        interactable.puzzleDescription = "Ba chữ số lập thành cấp số cộng. Tích bằng 5 lần tổng.";
        interactable.correctAnswer = "345";
        interactable.inputHint = "Nhập mã 3 số";
        interactable.wrongFeedback = "Mã bảo mật chưa đúng. Hệ thống vẫn bị khóa.";
        interactable.correctFeedback = "Mã chính xác. Hệ thống trung tâm đã mở.";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_SecurityServer345", puzzle.transform, Vector3.zero, new Vector3(4f, 1.8f, 2.9f), Quaternion.identity, PuzzleYellow);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.25f);
        AddGlowLight("GLOW_REPLACE_Puzzle_SecurityServer345", puzzle.transform, Vector3.up * 1.35f, PuzzleYellow, 7f, 1.3f);
        CreateWorldLabel("Puzzle: Security Server", labels, position + Vector3.up * 2.35f, 0.32f);
        puzzle.SetActive(false);
        return puzzle;
    }

    private static GameObject CreateBitexcoBusStop(Transform root, Transform labels, MemoryZoneController zone, Vector3 position)
    {
        GameObject busStop = new GameObject("REPLACE_BusStop_ReturnHub");
        busStop.transform.SetParent(root);
        busStop.transform.position = position;
        BoxCollider collider = busStop.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 2.4f, 1f);
        new GameObject("Collider").transform.SetParent(busStop.transform);
        BusStopInteractable busStopInteractable = busStop.AddComponent<BusStopInteractable>();
        busStopInteractable.currentZone = zone;
        busStopInteractable.targetScene = SceneLoader.BusHub;
        busStopInteractable.interactionPrompt = "Nhấn E để lên xe buýt ký ức.";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.zero, new Vector3(4f, 2.4f, 1f), Quaternion.identity, BusOrange);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", BusOrange * 1.3f);
        AddGlowLight("GLOW_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.up * 1f, BusOrange, 8f, 1.6f);
        zone.busStopReturn = busStop;
        CreateWorldLabel("Bus Stop", labels, position + Vector3.up * 2.6f, 0.33f);
        return busStop;
    }

    private static void CreateBitexcoEffects(MemoryZoneController zone, Transform sceneRoot, Transform effectsRoot, GameObject tower, GameObject helipad, Transform towerLights)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_Bitexco");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = sceneRoot.GetComponentsInChildren<Renderer>();
        materialEffect.grayColor = new Color(0.22f, 0.24f, 0.28f);
        materialEffect.restoredColor = new Color(0.35f, 0.55f, 0.82f);

        GameObject lightObject = new GameObject("LightRestoreEffect_BitexcoGlow");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(0, 10f, 16f);
        Light warmLight = lightObject.AddComponent<Light>();
        warmLight.type = LightType.Point;
        warmLight.color = new Color(0.95f, 0.78f, 0.45f);
        warmLight.range = 54f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { warmLight };
        lightEffect.dimIntensity = 0.06f;
        lightEffect.restoredIntensity = 2.6f;

        GameObject ambienceObject = new GameObject("AudioRestoreEffect_BitexcoAmbience");
        ambienceObject.transform.SetParent(zone.transform);
        ambienceObject.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;
        ambience.volume = 0f;
        AudioRestoreEffect audioEffect = ambienceObject.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { ambience };
        audioEffect.restoredVolume = 0.34f;

        zone.restoreEffects = new RestorableEffect[] { materialEffect, lightEffect, audioEffect };

        if (towerLights != null)
        {
            BitexcoTowerLightStrip[] strips = towerLights.GetComponentsInChildren<BitexcoTowerLightStrip>(true);
            if (strips.Length == 0)
            {
                Renderer[] stripRenderers = towerLights.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < stripRenderers.Length; i++)
                {
                    BitexcoTowerLightStrip strip = stripRenderers[i].gameObject.AddComponent<BitexcoTowerLightStrip>();
                    strip.zone = zone;
                    strip.turnOnDelay = i * 0.06f;
                }
            }
        }
    }

    private static EditorBuildSettingsScene GenerateBachDang()
    {
        Scene scene = NewScene(SceneLoader.BachDang);
        GameObject roots = CreateBachDangRoots();
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform itemRoot = roots.transform.Find("ItemRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform busStopRoot = roots.transform.Find("BusStopRoot");
        Transform debugLabels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(-38, 1, -24), "Đi dọc bến sông và tìm ký ức bị mắc kẹt.", 7.2f, 2.9f, spawnPoints, 18f, 46f);
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.BachDang, "Dòng chảy thành phố");

        CreateCube("REPLACE_Landmark_BachDang_Riverside", landmark, new Vector3(-6, -0.05f, -2), new Vector3(100, 0.1f, 44), new Color(0.32f, 0.33f, 0.35f));
        GameObject river = CreateCube("REPLACE_Landmark_SaiGonRiver", landmark, new Vector3(-6, -0.02f, 21), new Vector3(100, 0.06f, 26), new Color(0.12f, 0.18f, 0.26f));
        CreateCube("PromenadePath", environment, new Vector3(-6, 0.03f, -4), new Vector3(16, 0.06f, 38), new Color(0.4f, 0.42f, 0.45f));
        CreateCube("REPLACE_Prop_Dock", environment, new Vector3(4, 0.25f, 10), new Vector3(16, 0.5f, 10), InactiveGray);
        CreateCube("REPLACE_Prop_LifeRing", environment, new Vector3(11, 1.1f, 9), new Vector3(1.1f, 1.1f, 0.35f), InteractableBlue);

        for (int i = 0; i < 10; i++)
        {
            float x = -49 + i * 10.4f;
            CreateCube("REPLACE_Prop_Railing_" + (i + 1).ToString("00"), environment, new Vector3(x, 1.2f, 8.8f), new Vector3(6.2f, 1.8f, 0.25f), InactiveGray);
        }

        Transform lampRoot = CreateChildRoot(environment, "Lamps");
        for (int i = 0; i < 8; i++)
        {
            float x = -43 + i * 11.5f;
            CreateCube("REPLACE_Prop_Lamp_" + (i + 1).ToString("00"), lampRoot, new Vector3(x, 2.5f, -6), new Vector3(0.45f, 5f, 0.45f), InactiveGray);
            CreateCube("REPLACE_Prop_LampHead_" + (i + 1).ToString("00"), lampRoot, new Vector3(x, 5.2f, -6), new Vector3(0.9f, 0.45f, 0.9f), new Color(0.25f, 0.25f, 0.28f));
        }

        Transform boatRoot = CreateChildRoot(environment, "Boats");
        for (int i = 0; i < 4; i++)
        {
            float x = -34 + i * 19.5f;
            GameObject boat = CreateCube("REPLACE_Prop_Boat_" + (i + 1).ToString("00"), boatRoot, new Vector3(x, 0.6f, 22f + (i % 2) * 4.5f), new Vector3(10, 1.1f, 3.2f), new Color(0.24f, 0.25f, 0.28f));
            BoatBobEffect bob = boat.AddComponent<BoatBobEffect>();
            bob.zone = zone;
            bob.bobAmplitude = 0.38f;
            bob.bobSpeed = 1.25f + i * 0.2f;
        }

        GameObject npc = CreateBachDangNpc(npcRoot, debugLabels, new Vector3(-1.5f, 1f, 9f));
        GameObject item = CreateBachDangItem(itemRoot, debugLabels, new Vector3(-20f, 0.65f, 7f));
        GameObject puzzle = CreateBachDangPuzzle(puzzleRoot, debugLabels, zone, new Vector3(6f, 1f, 12f));
        GameObject busStop = CreateBachDangBusStop(busStopRoot, debugLabels, zone, new Vector3(39f, 1.2f, -20f));

        CreateWorldLabel("Bến Bạch Đằng", landmark, new Vector3(-6, 3.1f, 10), 0.55f);
        CreateWorldLabel("Player Spawn", spawnPoints, new Vector3(-38, 2.8f, -24), 0.32f);

        CreateBachDangEffects(zone, roots.transform, effectsRoot, river, lampRoot);
        CreateBoundary(landmark, 100, 70);
        AddLighting();

        BachDangSceneController controller = roots.AddComponent<BachDangSceneController>();
        controller.memoryZone = zone;
        controller.boatmanNpc = npc.GetComponent<NPCInteractable>();
        controller.oldFerryTicketItem = item.GetComponent<BachDangTicketInteractable>();
        controller.riverPuzzle = puzzle.GetComponent<PuzzleInteractable>();
        controller.returnBusStop = busStop.GetComponent<BusStopInteractable>();

        return Save(scene, SceneLoader.BachDang);
    }

    private static GameObject CreateBachDangRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[] { "EnvironmentRoot", "LandmarkRoot", "NPCRoot", "ItemRoot", "PuzzleRoot", "EffectsRoot", "BusStopRoot", "SpawnPoints", "DebugLabels" })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static GameObject CreateBachDangNpc(Transform root, Transform labels, Vector3 position)
    {
        GameObject npc = new GameObject("REPLACE_NPC_Boatman");
        npc.transform.SetParent(root);
        npc.transform.position = position;
        CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.45f;
        new GameObject("Collider").transform.SetParent(npc.transform);
        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.interactionPrompt = "Nhấn E để nói chuyện với bác lái tàu";
        GameObject visual = CreateBusVisual(PrimitiveType.Capsule, "Visual_REPLACE_Boatman", npc.transform, Vector3.zero, Vector3.one, Quaternion.identity, NpcPurple);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", NpcPurple * 1.35f);
        AddGlowLight("GLOW_REPLACE_NPC_Boatman", npc.transform, Vector3.up * 1.4f, NpcPurple, 6f, 1.2f);
        CreateWorldLabel("NPC: Bác lái tàu", labels, position + Vector3.up * 2.35f, 0.32f);
        return npc;
    }

    private static GameObject CreateBachDangItem(Transform root, Transform labels, Vector3 position)
    {
        GameObject item = new GameObject("REPLACE_Item_OldFerryTicket");
        item.transform.SetParent(root);
        item.transform.position = position;
        SphereCollider collider = item.AddComponent<SphereCollider>();
        collider.radius = 0.72f;
        new GameObject("Collider").transform.SetParent(item.transform);
        BachDangTicketInteractable interactable = item.AddComponent<BachDangTicketInteractable>();
        interactable.interactionPrompt = "Nhấn E để nhặt vé tàu cũ";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_OldFerryTicket", item.transform, Vector3.zero, new Vector3(1.4f, 0.2f, 1f), Quaternion.identity, InteractableBlue);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", InteractableBlue * 1.3f);
        AddGlowLight("GLOW_REPLACE_Item_OldFerryTicket", item.transform, Vector3.up * 0.8f, InteractableBlue, 5f, 1f);
        CreateWorldLabel("Item: Vé tàu cũ", labels, position + Vector3.up * 1.9f, 0.3f);
        return item;
    }

    private static GameObject CreateBachDangPuzzle(Transform root, Transform labels, MemoryZoneController zone, Vector3 position)
    {
        GameObject puzzle = new GameObject("REPLACE_Puzzle_RiverCrossing28");
        puzzle.transform.SetParent(root);
        puzzle.transform.position = position;
        BoxCollider collider = puzzle.AddComponent<BoxCollider>();
        collider.size = new Vector3(4.2f, 1.8f, 2.9f);
        new GameObject("Collider").transform.SetParent(puzzle.transform);
        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = "Vượt luồng sương mù";
        interactable.puzzleDescription = "5 con tàu cần qua sông với một chiếc đèn bão. Tổng thời gian tối thiểu là bao nhiêu phút?";
        interactable.correctAnswer = "28";
        interactable.inputHint = "Nhập đáp án";
        interactable.wrongFeedback = "Dòng chảy vẫn bị nghẽn. Hãy tính lại thời gian tối thiểu.";
        interactable.correctFeedback = "Đáp án chính xác. Dòng sông bắt đầu chuyển động.";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_RiverCrossing28", puzzle.transform, Vector3.zero, new Vector3(4.2f, 1.8f, 2.9f), Quaternion.identity, PuzzleYellow);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.25f);
        AddGlowLight("GLOW_REPLACE_Puzzle_RiverCrossing28", puzzle.transform, Vector3.up * 1.35f, PuzzleYellow, 7f, 1.3f);
        CreateWorldLabel("Puzzle: River Crossing", labels, position + Vector3.up * 2.35f, 0.32f);
        puzzle.SetActive(false);
        return puzzle;
    }

    private static GameObject CreateBachDangBusStop(Transform root, Transform labels, MemoryZoneController zone, Vector3 position)
    {
        GameObject busStop = new GameObject("REPLACE_BusStop_ReturnHub");
        busStop.transform.SetParent(root);
        busStop.transform.position = position;
        BoxCollider collider = busStop.AddComponent<BoxCollider>();
        collider.size = new Vector3(4f, 2.4f, 1f);
        new GameObject("Collider").transform.SetParent(busStop.transform);
        BusStopInteractable busStopInteractable = busStop.AddComponent<BusStopInteractable>();
        busStopInteractable.currentZone = zone;
        busStopInteractable.targetScene = SceneLoader.BusHub;
        busStopInteractable.interactionPrompt = "Nhấn E để lên xe buýt ký ức.";
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.zero, new Vector3(4f, 2.4f, 1f), Quaternion.identity, BusOrange);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", BusOrange * 1.3f);
        AddGlowLight("GLOW_REPLACE_BusStop_ReturnHub", busStop.transform, Vector3.up * 1f, BusOrange, 8f, 1.6f);
        zone.busStopReturn = busStop;
        CreateWorldLabel("Bus Stop", labels, position + Vector3.up * 2.6f, 0.33f);
        return busStop;
    }

    private static void CreateBachDangEffects(MemoryZoneController zone, Transform sceneRoot, Transform effectsRoot, GameObject river, Transform lampRoot)
    {
        GameObject materialObject = new GameObject("MaterialRestoreEffect_BachDang");
        materialObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = sceneRoot.GetComponentsInChildren<Renderer>();
        materialEffect.grayColor = new Color(0.28f, 0.3f, 0.33f);
        materialEffect.restoredColor = new Color(0.36f, 0.58f, 0.78f);

        GameObject lightObject = new GameObject("LightRestoreEffect_RiverWarm");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(-4, 7.5f, 6f);
        Light warmLight = lightObject.AddComponent<Light>();
        warmLight.type = LightType.Point;
        warmLight.color = new Color(1f, 0.78f, 0.46f);
        warmLight.range = 52f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { warmLight };
        lightEffect.dimIntensity = 0.06f;
        lightEffect.restoredIntensity = 2.45f;

        GameObject ambienceObject = new GameObject("AudioRestoreEffect_RiverAmbience");
        ambienceObject.transform.SetParent(zone.transform);
        ambienceObject.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.spatialBlend = 0f;
        ambience.volume = 0f;
        AudioRestoreEffect audioEffect = ambienceObject.AddComponent<AudioRestoreEffect>();
        audioEffect.audioSources = new[] { ambience };
        audioEffect.restoredVolume = 0.33f;

        zone.restoreEffects = new RestorableEffect[] { materialEffect, lightEffect, audioEffect };
    }

    private static EditorBuildSettingsScene GenerateEnding()
    {
        Scene scene = NewScene(SceneLoader.Ending);
        GameObject roots = CreateEndingRoots();
        Transform environment = roots.transform.Find("EnvironmentRoot");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform shardRoot = roots.transform.Find("MemoryShardRoot");
        Transform effectsRoot = roots.transform.Find("EffectsRoot");
        Transform returnRoot = roots.transform.Find("ReturnTriggerRoot");
        Transform spawnPoints = roots.transform.Find("SpawnPoints");
        Transform labels = roots.transform.Find("DebugLabels");

        CreateManagersAndPlayer(new Vector3(0, 1, -26f), "Lắng nghe những mảnh ký ức cuối cùng.", 7.4f, 3f, spawnPoints, 18f, 46f);

        CreateCube("Ground", environment, new Vector3(0, -0.05f, 0), new Vector3(84, 0.1f, 84), new Color(0.26f, 0.27f, 0.3f));
        CreateCube("ViewpointPlatform", environment, new Vector3(0, 0.15f, -6f), new Vector3(36, 0.3f, 34), new Color(0.33f, 0.34f, 0.37f));
        CreateCube("REPLACE_Ending_CitySkyline", environment, new Vector3(0, 6f, 28f), new Vector3(78, 12f, 4f), new Color(0.22f, 0.23f, 0.28f));
        CreateCube("RiverOrHorizon", environment, new Vector3(0, -0.02f, 18f), new Vector3(84, 0.06f, 26), new Color(0.13f, 0.19f, 0.28f));

        for (int i = 0; i < 10; i++)
        {
            float z = -20f + i * 4.5f;
            GameObject marker = CreateCube("REPLACE_Ending_PathLight_" + (i + 1).ToString("00"), effectsRoot, new Vector3(0, 0.08f, z), new Vector3(0.95f, 0.12f, 0.55f), new Color(0.92f, 0.74f, 0.24f));
            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.material.EnableKeyword("_EMISSION");
            markerRenderer.material.SetColor("_EmissionColor", new Color(0.92f, 0.74f, 0.24f) * 1.2f);
        }

        GameObject tower = CreateCube("REPLACE_Ending_Landmark81_Tower", landmark, new Vector3(0, 23f, 26f), new Vector3(9, 46, 9), new Color(0.3f, 0.34f, 0.4f));
        CreateCube("REPLACE_Ending_Landmark81_Spire", landmark, new Vector3(0, 49f, 26f), new Vector3(2f, 6f, 2f), new Color(0.45f, 0.5f, 0.58f));

        string[] shardNames =
        {
            "Nhịp sống trẻ",
            "Đời sống thường ngày",
            "Lịch sử",
            "Bình yên",
            "Chuyển mình",
            "Dòng chảy thành phố"
        };

        GameObject[] shards = new GameObject[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI * 2f / 6f;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * 7.5f, 1.8f, -4f + Mathf.Sin(angle) * 5f);
            string shardObjName = "REPLACE_Ending_MemoryShard_" + (i + 1).ToString("00") + "_" +
                                  (i == 0 ? "NguyenHue" : i == 1 ? "BenThanh" : i == 2 ? "DinhDocLap" : i == 3 ? "NhaThoDucBa" : i == 4 ? "Bitexco" : "BachDang");
            GameObject shard = new GameObject(shardObjName);
            shard.transform.SetParent(shardRoot);
            shard.transform.position = pos;
            SphereCollider col = shard.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            new GameObject("Collider").transform.SetParent(shard.transform);
            GameObject visual = CreateBusVisual(PrimitiveType.Sphere, "Visual_REPLACE_" + shardObjName, shard.transform, Vector3.zero, Vector3.one * 1f, Quaternion.identity, new Color(0.35f, 0.38f, 0.42f));
            Renderer r = visual.GetComponent<Renderer>();
            r.material.EnableKeyword("_EMISSION");
            r.material.SetColor("_EmissionColor", Color.black);
            AddGlowLight("GLOW_" + shardObjName, shard.transform, Vector3.up * 0.2f, new Color(0.95f, 0.83f, 0.35f), 4.8f, 0f);
            CreateWorldLabel(shardNames[i], labels, pos + Vector3.up * 1.6f, 0.28f, false);
            shards[i] = shard;
        }

        GameObject finalLight = CreateCube("REPLACE_Ending_FinalLight", effectsRoot, new Vector3(0, 2f, 10f), new Vector3(1.4f, 4f, 1.4f), new Color(0.95f, 0.82f, 0.32f));
        Renderer finalLightRenderer = finalLight.GetComponent<Renderer>();
        finalLightRenderer.material.EnableKeyword("_EMISSION");
        finalLightRenderer.material.SetColor("_EmissionColor", Color.black);
        AddGlowLight("GLOW_REPLACE_Ending_FinalLight", finalLight.transform, Vector3.zero, new Color(1f, 0.84f, 0.34f), 12f, 0f);

        GameObject returnTrigger = new GameObject("REPLACE_Ending_ReturnToHubTrigger");
        returnTrigger.transform.SetParent(returnRoot);
        returnTrigger.transform.position = new Vector3(0, 1.3f, 14f);
        BoxCollider triggerCollider = returnTrigger.AddComponent<BoxCollider>();
        triggerCollider.size = new Vector3(4f, 2.6f, 1.1f);
        new GameObject("Collider").transform.SetParent(returnTrigger.transform);
        BusStopInteractable returnInteract = returnTrigger.AddComponent<BusStopInteractable>();
        returnInteract.currentZone = null;
        returnInteract.requireCurrentZoneRestored = false;
        returnInteract.targetScene = SceneLoader.BusHub;
        returnInteract.interactionPrompt = "Nhấn E để quay về xe buýt ký ức.";
        CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_Ending_ReturnToHubTrigger", returnTrigger.transform, Vector3.zero, new Vector3(4f, 2.6f, 1.1f), Quaternion.identity, BusOrange);
        AddGlowLight("GLOW_REPLACE_Ending_ReturnToHubTrigger", returnTrigger.transform, Vector3.up * 0.9f, BusOrange, 8f, 1.4f);
        returnTrigger.SetActive(false);

        CreateWorldLabel("Landmark 81", labels, new Vector3(0, 50f, 24f), 0.5f, false);
        CreateWorldLabel("Player Spawn", spawnPoints, new Vector3(0, 2.8f, -26f), 0.3f);

        EndingSceneController controller = roots.AddComponent<EndingSceneController>();
        controller.landmarkTower = tower;
        controller.memoryShards = shards;
        controller.memoryNames = shardNames;
        controller.finalLightObject = finalLight;
        controller.returnTrigger = returnTrigger;
        controller.renderersToWarm = roots.GetComponentsInChildren<Renderer>(true);

        GameObject ambienceObject = new GameObject("EndingAmbienceSource");
        ambienceObject.transform.SetParent(effectsRoot);
        AudioSource ambience = ambienceObject.AddComponent<AudioSource>();
        ambience.loop = true;
        ambience.playOnAwake = false;
        ambience.volume = 0f;
        ambience.spatialBlend = 0f;
        controller.endingAmbience = ambience;

        CreateBoundary(landmark, 84, 84);
        AddLighting();
        return Save(scene, SceneLoader.Ending);
    }

    private static GameObject CreateEndingRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[]
                 {
                     "EnvironmentRoot", "LandmarkRoot", "MemoryShardRoot", "EffectsRoot", "ReturnTriggerRoot", "SpawnPoints", "DebugLabels"
                 })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static Scene NewScene(string sceneName)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = sceneName;
        return scene;
    }

    private static GameObject CreateStandardRoots()
    {
        GameObject root = new GameObject("SceneBlockoutRoot");
        foreach (string childName in new[] { "LandmarkRoot", "NPCRoot", "ItemRoot", "PuzzleRoot", "PropRoot", "EffectsRoot", "SpawnPoints" })
        {
            new GameObject(childName).transform.SetParent(root.transform);
        }

        return root;
    }

    private static void CreateManagersAndPlayer(Vector3 spawnPosition, string objective, float cameraDistance = 7f, float cameraHeight = 2.8f, Transform spawnPointParent = null, float minCameraPitch = 18f, float maxCameraPitch = 42f)
    {
        GameObject progress = new GameObject("GameProgressManager");
        progress.AddComponent<GameProgressManager>();

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "REPLACE_Player_Character";
        player.transform.position = spawnPosition;
        player.GetComponent<Renderer>().material = CreateMaterial(InteractableBlue);
        UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.4f;
        controller.center = Vector3.zero;

        GameObject target = new GameObject("Player_CameraTarget");
        target.transform.SetParent(player.transform);
        target.transform.localPosition = new Vector3(0, 1.1f, 0);

        GameObject spawnLabel = new GameObject("PlayerSpawn");
        if (spawnPointParent != null)
        {
            spawnLabel.transform.SetParent(spawnPointParent);
        }

        spawnLabel.transform.position = spawnPosition;
        CreateWorldLabel("Player Spawn", spawnLabel.transform, spawnPosition + Vector3.up * 2.2f, 0.35f);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 60f;
        cameraObject.AddComponent<AudioListener>();
        ThirdPersonCamera thirdPersonCamera = cameraObject.AddComponent<ThirdPersonCamera>();
        thirdPersonCamera.target = target.transform;
        thirdPersonCamera.distance = cameraDistance;
        thirdPersonCamera.height = cameraHeight;
        thirdPersonCamera.mouseSensitivity = 2f;
        thirdPersonCamera.minPitch = minCameraPitch;
        thirdPersonCamera.maxPitch = maxCameraPitch;

        ThirdPersonPlayerController playerController = player.AddComponent<ThirdPersonPlayerController>();
        playerController.cameraTransform = cameraObject.transform;

        Interactor interactor = player.AddComponent<Interactor>();
        interactor.playerCamera = camera;
        interactor.interactionRange = 5f;
        interactor.nearbyInteractionRadius = 3.5f;

        CreateUI(objective);
    }

    private static void CreateUI(string objective)
    {
        GameObject canvasObject = new GameObject("UI_Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        UIManager manager = canvasObject.AddComponent<UIManager>();
        manager.interactionPromptText = CreateUIText("InteractionPrompt", canvasObject.transform, "Press E to interact", new Vector2(0.5f, 0.2f), new Vector2(420, 40), 20);
        manager.memoryProgressText = CreateUIText("MemoryProgress", canvasObject.transform, "Memory progress: 0/6", new Vector2(0.04f, 0.95f), new Vector2(280, 40), 18);
        manager.objectiveText = CreateUIText("ObjectiveText", canvasObject.transform, objective, new Vector2(0.04f, 0.9f), new Vector2(680, 40), 18);
        CreateUIText("Crosshair", canvasObject.transform, "+", new Vector2(0.5f, 0.5f), new Vector2(40, 40), 28);

        GameObject dialogueBox = CreatePanel("DialogueBox", canvasObject.transform, new Vector2(0.5f, 0.12f), new Vector2(760, 120), new Color(0, 0, 0, 0.75f));
        manager.dialogueBox = dialogueBox;
        manager.dialogueText = CreateUIText("DialogueText", dialogueBox.transform, "", new Vector2(0.5f, 0.5f), new Vector2(720, 90), 20);

        GameObject puzzlePanel = CreatePanel("PuzzlePanel", canvasObject.transform, new Vector2(0.5f, 0.5f), new Vector2(620, 430), new Color(0.08f, 0.08f, 0.08f, 0.92f));
        manager.puzzlePanel = puzzlePanel;
        manager.puzzleTitleText = CreateUIText("PuzzleTitle", puzzlePanel.transform, "Puzzle", new Vector2(0.5f, 0.88f), new Vector2(560, 50), 26);
        manager.puzzleDescriptionText = CreateUIText("PuzzleDescription", puzzlePanel.transform, "", new Vector2(0.5f, 0.75f), new Vector2(560, 80), 18);
        manager.puzzleFeedbackText = CreateUIText("PuzzleFeedback", puzzlePanel.transform, "", new Vector2(0.5f, 0.22f), new Vector2(560, 40), 18);
        manager.puzzleInput = CreateInputField("PuzzleInput", puzzlePanel.transform, new Vector2(0.5f, 0.55f), new Vector2(460, 48));

        GameObject quickRoot = new GameObject("QuickChoiceRoot", typeof(RectTransform));
        quickRoot.transform.SetParent(puzzlePanel.transform, false);
        RectTransform quickRect = quickRoot.GetComponent<RectTransform>();
        quickRect.anchorMin = quickRect.anchorMax = new Vector2(0.5f, 0.42f);
        quickRect.sizeDelta = new Vector2(540, 80);
        HorizontalLayoutGroup layout = quickRoot.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 8;
        manager.quickChoiceRoot = quickRoot.transform;
        manager.quickChoiceButtonPrefab = CreateUIButton("QuickChoiceButtonPrefab", puzzlePanel.transform, "Choice", new Vector2(0.5f, -2f), new Vector2(80, 36), null);
        manager.quickChoiceButtonPrefab.gameObject.SetActive(false);

        CreateUIButton("SubmitPuzzleButton", puzzlePanel.transform, "Submit", new Vector2(0.4f, 0.1f), new Vector2(140, 42), manager.SubmitPuzzle);
        CreateUIButton("ClosePuzzleButton", puzzlePanel.transform, "Close", new Vector2(0.62f, 0.1f), new Vector2(140, 42), manager.HidePuzzle);
    }

    private static Text CreateUIText(string name, Transform parent, string text, Vector2 anchor, Vector2 size, int fontSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        Text uiText = obj.AddComponent<Text>();
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.color = Color.white;
        uiText.alignment = TextAnchor.MiddleCenter;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return uiText;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        return panel;
    }

    private static InputField CreateInputField(string name, Transform parent, Vector2 anchor, Vector2 size)
    {
        GameObject inputObj = CreatePanel(name, parent, anchor, size, Color.white);
        InputField input = inputObj.AddComponent<InputField>();
        Text text = CreateUIText("Text", inputObj.transform, "", new Vector2(0.5f, 0.5f), size - new Vector2(20, 10), 20);
        text.color = Color.black;
        Text placeholder = CreateUIText("Placeholder", inputObj.transform, "Answer", new Vector2(0.5f, 0.5f), size - new Vector2(20, 10), 20);
        placeholder.color = new Color(0, 0, 0, 0.35f);
        input.textComponent = text;
        input.placeholder = placeholder;
        return input;
    }

    private static Button CreateUIButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreatePanel(name, parent, anchor, size, PuzzleYellow);
        Button button = buttonObject.AddComponent<Button>();
        Text buttonText = CreateUIText("Label", buttonObject.transform, label, new Vector2(0.5f, 0.5f), size, 18);
        buttonText.color = Color.black;
        if (action != null)
        {
            button.onClick.AddListener(action);
        }

        return button;
    }

    private static MemoryZoneController CreateMemoryZone(Transform root, LocationId locationId, string fragmentName)
    {
        GameObject zoneObject = new GameObject("MemoryZone_" + locationId);
        zoneObject.transform.SetParent(root.Find("EffectsRoot"));
        MemoryZoneController zone = zoneObject.AddComponent<MemoryZoneController>();
        zone.locationId = locationId;
        zone.memoryFragmentName = fragmentName;
        return zone;
    }

    private static void AddZoneEffects(MemoryZoneController zone, Transform root, Color restoredColor, bool addParticles, bool addAudio)
    {
        GameObject materialEffectObject = new GameObject("MaterialRestoreEffect_AllPlaceholders");
        materialEffectObject.transform.SetParent(zone.transform);
        MaterialRestoreEffect materialEffect = materialEffectObject.AddComponent<MaterialRestoreEffect>();
        materialEffect.renderers = root.GetComponentsInChildren<Renderer>();
        materialEffect.restoredColor = restoredColor;

        GameObject lightObject = new GameObject("LightRestoreEffect_KeyLight");
        lightObject.transform.SetParent(zone.transform);
        lightObject.transform.position = new Vector3(0, 10, -5);
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = 60f;
        LightRestoreEffect lightEffect = lightObject.AddComponent<LightRestoreEffect>();
        lightEffect.lights = new[] { light };

        List<RestorableEffect> effects = new List<RestorableEffect> { materialEffect, lightEffect };

        if (addParticles)
        {
            GameObject particleObject = new GameObject("ParticleRestoreEffect_FountainPlaceholder");
            particleObject.transform.SetParent(zone.transform);
            particleObject.transform.position = Vector3.up * 1.5f;
            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            ParticleRestoreEffect particleEffect = particleObject.AddComponent<ParticleRestoreEffect>();
            particleEffect.particles = new[] { particles };
            effects.Add(particleEffect);
        }

        if (addAudio)
        {
            GameObject audioObject = new GameObject("AudioRestoreEffect_AmbiencePlaceholder");
            audioObject.transform.SetParent(zone.transform);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            AudioRestoreEffect audioEffect = audioObject.AddComponent<AudioRestoreEffect>();
            audioEffect.audioSources = new[] { source };
            effects.Add(audioEffect);
        }

        zone.restoreEffects = effects.ToArray();
    }

    private static void CreateFloor(string name, Transform parent, Vector3 position, Vector2 size)
    {
        CreateCube(name, parent, position + Vector3.down * 0.05f, new Vector3(size.x, 0.1f, size.y), InactiveGray);
    }

    private static void CreateNPC(Transform parent, string objectName, string label, Vector3 position, string dialogue)
    {
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = objectName;
        npc.transform.SetParent(parent);
        npc.transform.position = position;
        Renderer npcRenderer = npc.GetComponent<Renderer>();
        npcRenderer.material = CreateMaterial(NpcPurple);
        npcRenderer.material.EnableKeyword("_EMISSION");
        npcRenderer.material.SetColor("_EmissionColor", NpcPurple * 1.2f);
        NPCInteractable interactable = npc.AddComponent<NPCInteractable>();
        interactable.dialogue = dialogue;
        CreateWorldLabel(label, parent, position + Vector3.up * 2.2f, 0.4f);
    }

    private static void CreateItem(Transform parent, string objectName, string label, Vector3 position)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        item.name = objectName;
        item.transform.SetParent(parent);
        item.transform.position = position;
        item.transform.localScale = Vector3.one * 1.2f;
        item.GetComponent<Renderer>().material = CreateMaterial(InteractableBlue);
        ItemInteractable interactable = item.AddComponent<ItemInteractable>();
        interactable.itemName = label;
        CreateWorldLabel(label, parent, position + Vector3.up * 1.8f, 0.35f);
    }

    private static void CreatePuzzle(Transform parent, string objectName, string label, Vector3 position, MemoryZoneController zone, string title, string description, string answer, string hint, string[] choices)
    {
        GameObject puzzle = CreateCube(objectName, parent, position, new Vector3(3.5f, 1.6f, 2.5f), PuzzleYellow);
        Renderer puzzleRenderer = puzzle.GetComponent<Renderer>();
        puzzleRenderer.material.EnableKeyword("_EMISSION");
        puzzleRenderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.1f);
        PuzzleInteractable interactable = puzzle.AddComponent<PuzzleInteractable>();
        interactable.memoryZone = zone;
        interactable.puzzleTitle = title;
        interactable.puzzleDescription = description;
        interactable.correctAnswer = answer;
        interactable.inputHint = hint;
        interactable.quickChoices = choices;
        CreateWorldLabel(label, parent, position + Vector3.up * 2.2f, 0.38f);
    }

    private static void CreateBusStop(Transform parent, MemoryZoneController zone, Vector3 position)
    {
        GameObject stop = CreateCube("REPLACE_BusStop_ReturnHub", parent, position, new Vector3(4, 2.4f, 1), BusOrange);
        BusStopInteractable busStop = stop.AddComponent<BusStopInteractable>();
        busStop.currentZone = zone;
        busStop.targetScene = SceneLoader.BusHub;
        zone.busStopReturn = stop;
        CreateWorldLabel("Bus Stop", parent, position + Vector3.up * 2.5f, 0.42f);
    }

    private static void CreateRouteButton(Transform parent, string objectName, string displayName, LocationId locationId, string sceneName, Vector3 position, bool ending)
    {
        GameObject button = new GameObject(objectName);
        button.transform.SetParent(parent);
        button.transform.position = position;
        BoxCollider collider = button.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.95f, 1f, 0.6f);
        new GameObject("Collider").transform.SetParent(button.transform);
        GameObject visual = CreateBusVisual(PrimitiveType.Cube, "Visual_REPLACE_" + objectName, button.transform, Vector3.zero, new Vector3(1.8f, 0.82f, 0.35f), Quaternion.identity, PuzzleYellow);
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", PuzzleYellow * 1.2f);
        MapSelectionInteractable interactable = button.AddComponent<MapSelectionInteractable>();
        interactable.targetLocation = locationId;
        interactable.targetScene = sceneName;
        interactable.displayName = displayName;
        interactable.isEndingRoute = ending;
        interactable.statusRenderer = renderer;
        interactable.statusLabel = CreateWorldLabel(displayName, button.transform, position + Vector3.up * 0.08f + Vector3.back * 0.22f, 0.18f, false);
    }

    private static void AddGlowLight(string name, Transform parent, Vector3 position, Color color, float range, float intensity)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
    }

    private static void CreateHintCube(Transform parent, string objectName, string number, Vector3 position, Color color)
    {
        GameObject hint = CreateCube(objectName, parent, position, new Vector3(3.2f, 3.2f, 3.2f), color);
        Renderer hintRenderer = hint.GetComponent<Renderer>();
        hintRenderer.material.EnableKeyword("_EMISSION");
        hintRenderer.material.SetColor("_EmissionColor", color * 1.4f);
        AddGlowLight(objectName + "_Glow", parent, position, color, 7f, 1.8f);
        CreateWorldLabel(number, parent, position + Vector3.up * 3.1f, 0.9f, false);
    }

    private static void CreateBoundary(Transform parent, float width, float depth)
    {
        float halfW = width * 0.5f;
        float halfD = depth * 0.5f;
        CreateCube("BOUNDARY_North_RedTransparent", parent, new Vector3(0, 2, halfD), new Vector3(width, 4, 0.5f), BoundaryRed);
        CreateCube("BOUNDARY_South_RedTransparent", parent, new Vector3(0, 2, -halfD), new Vector3(width, 4, 0.5f), BoundaryRed);
        CreateCube("BOUNDARY_East_RedTransparent", parent, new Vector3(halfW, 2, 0), new Vector3(0.5f, 4, depth), BoundaryRed);
        CreateCube("BOUNDARY_West_RedTransparent", parent, new Vector3(-halfW, 2, 0), new Vector3(0.5f, 4, depth), BoundaryRed);
    }

    private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().material = CreateMaterial(color);
        return cube;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;

        if (color.a < 1f)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_AlphaClip", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = 3000;
        }

        return material;
    }

    private static Component CreateWorldLabel(string text, Transform parent, Vector3 position, float size, bool hideInPlayMode = true)
    {
        GameObject label = new GameObject("TMP_Label_" + text.Replace(" ", "_"));
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.transform.rotation = Quaternion.identity;
        WorldLabelVisibility visibility = label.AddComponent<WorldLabelVisibility>();
        visibility.hideInPlayMode = hideInPlayMode;

        Type tmpType = Type.GetType("TMPro.TextMeshPro, Unity.TextMeshPro");
        if (tmpType != null)
        {
            Component tmp = label.AddComponent(tmpType);
            SetProperty(tmp, "text", text);
            SetProperty(tmp, "fontSize", size * 10f);
            Type alignmentType = Type.GetType("TMPro.TextAlignmentOptions, Unity.TextMeshPro");
            if (alignmentType != null)
            {
                SetProperty(tmp, "alignment", Enum.Parse(alignmentType, "Center"));
            }

            return tmp;
        }

        TextMesh mesh = label.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = 32;
        mesh.characterSize = size;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        return mesh;
    }

    private static void SetProperty(Component component, string propertyName, object value)
    {
        PropertyInfo property = component.GetType().GetProperty(propertyName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(component, value);
        }
    }

    private static void AddLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    private static EditorBuildSettingsScene Save(Scene scene, string sceneName)
    {
        string path = SceneFolder + "/" + sceneName + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        return new EditorBuildSettingsScene(path, true);
    }
}
#endif
