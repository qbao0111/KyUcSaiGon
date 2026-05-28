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
    private static readonly Color InteractableBlue = new Color(0.1f, 0.45f, 1f);
    private static readonly Color PuzzleYellow = new Color(1f, 0.78f, 0.12f);
    private static readonly Color RestoredGreen = new Color(0.2f, 0.8f, 0.35f);
    private static readonly Color NpcPurple = new Color(0.55f, 0.25f, 0.9f);
    private static readonly Color BusOrange = new Color(1f, 0.45f, 0.08f);
    private static readonly Color BoundaryRed = new Color(1f, 0f, 0f, 0.18f);

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

    private static EditorBuildSettingsScene GenerateBusHub()
    {
        Scene scene = NewScene(SceneLoader.BusHub);
        CreateManagersAndPlayer(new Vector3(0, 1, -16), "Choose a bus route from the board.");
        GameObject roots = CreateStandardRoots();
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform props = roots.transform.Find("PropRoot");
        Transform puzzle = roots.transform.Find("PuzzleRoot");

        CreateFloor("REPLACE_BusHub_Floor", landmark, new Vector3(0, 0, 0), new Vector2(26, 38));
        CreateCube("REPLACE_BusHub_LeftWall", landmark, new Vector3(-13, 2.5f, 0), new Vector3(0.4f, 5f, 38f), InactiveGray);
        CreateCube("REPLACE_BusHub_RightWall", landmark, new Vector3(13, 2.5f, 0), new Vector3(0.4f, 5f, 38f), InactiveGray);
        CreateCube("REPLACE_BusHub_Ceiling", landmark, new Vector3(0, 5f, 0), new Vector3(26f, 0.35f, 38f), new Color(0.32f, 0.34f, 0.36f));
        CreateCube("REPLACE_BusHub_DriverArea", props, new Vector3(0, 1, 15), new Vector3(8, 2, 3), InactiveGray);
        CreateCube("REPLACE_BusHub_MapBoard", props, new Vector3(0, 3, 8), new Vector3(18, 5, 0.5f), new Color(0.08f, 0.1f, 0.12f));
        CreateWorldLabel("REPLACE_BusHub_MapBoard", props, new Vector3(0, 6.2f, 7.7f), 0.55f);

        CreateRouteButton(puzzle, "Route_NguyenHue_Tutorial", "Nguyễn Huệ Tutorial", LocationId.NguyenHue, SceneLoader.NguyenHue, new Vector3(-7.5f, 3.4f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_BenThanh", "Chợ Bến Thành", LocationId.BenThanh, SceneLoader.BenThanh, new Vector3(-4.5f, 3.4f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_DinhDocLap", "Dinh Độc Lập", LocationId.DinhDocLap, SceneLoader.DinhDocLap, new Vector3(-1.5f, 3.4f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_NhaThoDucBa", "Nhà thờ Đức Bà", LocationId.NhaThoDucBa, SceneLoader.NhaThoDucBa, new Vector3(1.5f, 3.4f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_Bitexco", "Bitexco", LocationId.Bitexco, SceneLoader.Bitexco, new Vector3(4.5f, 3.4f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_BachDang", "Bến Bạch Đằng", LocationId.BachDang, SceneLoader.BachDang, new Vector3(-2.5f, 1.7f, 7.4f), false);
        CreateRouteButton(puzzle, "Route_Ending_Landmark81", "Ending Route", LocationId.None, SceneLoader.Ending, new Vector3(2.5f, 1.7f, 7.4f), true);

        CreateBoundary(landmark, 26, 38);
        AddLighting();
        return Save(scene, SceneLoader.BusHub);
    }

    private static EditorBuildSettingsScene GenerateNguyenHue()
    {
        Scene scene = NewScene(SceneLoader.NguyenHue);
        CreateManagersAndPlayer(new Vector3(0, 1, -26), "Talk to the musician, read the LEDs, then tune the speaker.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.NguyenHue, "Nhịp sống trẻ");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform npcRoot = roots.transform.Find("NPCRoot");
        Transform puzzleRoot = roots.transform.Find("PuzzleRoot");
        Transform props = roots.transform.Find("PropRoot");

        CreateFloor("REPLACE_Landmark_NguyenHue_Boulevard", landmark, Vector3.zero, new Vector2(100, 60));
        CreateCube("REPLACE_Landmark_NguyenHue_Fountain", landmark, new Vector3(0, 0.7f, 0), new Vector3(8, 1.4f, 8), InactiveGray);
        CreateWorldLabel("Landmark: Nguyễn Huệ Fountain", landmark, new Vector3(0, 3.2f, 0), 0.55f);
        CreateCube("REPLACE_Prop_LEDScreen_Left", props, new Vector3(-32, 4, 4), new Vector3(1, 8, 16), InactiveGray);
        CreateCube("REPLACE_Prop_LEDScreen_Right", props, new Vector3(32, 4, 4), new Vector3(1, 8, 16), InactiveGray);
        CreateHintCube(props, "REPLACE_Prop_LED_Red_Number_1", "1", new Vector3(-20, 1.2f, -8), Color.red);
        CreateHintCube(props, "REPLACE_Prop_LED_Green_Number_6", "6", new Vector3(0, 1.2f, 16), Color.green);
        CreateHintCube(props, "REPLACE_Prop_LED_Yellow_Number_8", "8", new Vector3(22, 1.2f, -2), Color.yellow);

        CreateNPC(npcRoot, "REPLACE_NPC_StreetMusician", "Street Musician", new Vector3(-22, 1, -8), "Nếu thành phố có nhịp, nó nằm trong ba màu đèn kia.");
        CreatePuzzle(puzzleRoot, "REPLACE_Puzzle_SpeakerMixer", "Puzzle: Speaker Mixer", new Vector3(-16, 0.8f, -8), zone, "Speaker Mixer", "Bass - Mid - Treble. Hint: Red, Green, Yellow.", "1-6-8", "1-6-8", new[] { "1", "6", "8" });
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(0, 1.2f, 26));

        AddZoneEffects(zone, roots.transform, RestoredGreen, true, true);
        CreateBoundary(landmark, 100, 60);
        AddLighting();
        return Save(scene, SceneLoader.NguyenHue);
    }

    private static EditorBuildSettingsScene GenerateBenThanh()
    {
        Scene scene = NewScene(SceneLoader.BenThanh);
        CreateManagersAndPlayer(new Vector3(0, 1, -32), "Find the vendor and solve the fruit basket puzzle.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.BenThanh, "Đời sống thường ngày");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform props = roots.transform.Find("PropRoot");

        CreateFloor("REPLACE_Landmark_BenThanh_Plaza", landmark, Vector3.zero, new Vector2(80, 80));
        CreateCube("REPLACE_Landmark_BenThanh_Gate", landmark, new Vector3(0, 4, 30), new Vector3(22, 8, 3), InactiveGray);
        CreateCube("REPLACE_Landmark_BenThanh_ClockTower", landmark, new Vector3(0, 11, 30), new Vector3(6, 8, 3), InactiveGray);
        CreateWorldLabel("Landmark: Chợ Bến Thành Gate", landmark, new Vector3(0, 16, 29), 0.6f);

        for (int i = 0; i < 6; i++)
        {
            float x = -24 + i * 9.5f;
            CreateCube("REPLACE_Prop_FruitStall_" + (i + 1), props, new Vector3(x, 0.8f, -2 + (i % 2) * 8), new Vector3(5, 1.6f, 3), InteractableBlue);
        }

        CreateNPC(roots.transform.Find("NPCRoot"), "REPLACE_NPC_MarketVendor", "Market Vendor", new Vector3(-16, 1, -6), "Một rổ trái cây đúng số sẽ mở lại ký ức khu chợ.");
        CreateItem(roots.transform.Find("ItemRoot"), "REPLACE_Item_OldConicalHat", "Item: Old Conical Hat", new Vector3(-5, 0.6f, -14));
        CreatePuzzle(roots.transform.Find("PuzzleRoot"), "REPLACE_Puzzle_FruitBasket", "Puzzle: Fruit Basket", new Vector3(-16, 0.8f, 2), zone, "Fruit Basket Puzzle", "Correct basket: 1 mango, 3 oranges, 2 pomelos.", "1-3-2", "132", new[] { "1", "3", "2" });
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(32, 1.2f, -8));

        AddZoneEffects(zone, roots.transform, RestoredGreen, false, true);
        CreateBoundary(landmark, 80, 80);
        AddLighting();
        return Save(scene, SceneLoader.BenThanh);
    }

    private static EditorBuildSettingsScene GenerateDinhDocLap()
    {
        Scene scene = NewScene(SceneLoader.DinhDocLap);
        CreateManagersAndPlayer(new Vector3(0, 1, -34), "Walk through the courtyard and recover the history code.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.DinhDocLap, "Lịch sử");
        Transform landmark = roots.transform.Find("LandmarkRoot");

        CreateFloor("REPLACE_Landmark_DinhDocLap_Courtyard", landmark, Vector3.zero, new Vector2(90, 80));
        CreateCube("REPLACE_Landmark_DinhDocLap_EntranceGate", landmark, new Vector3(0, 2.5f, -30), new Vector3(26, 5, 2), InactiveGray);
        CreateCube("REPLACE_Prop_DinhDocLap_LongPath", roots.transform.Find("PropRoot"), new Vector3(0, 0.08f, 0), new Vector3(8, 0.12f, 62), new Color(0.35f, 0.35f, 0.35f));
        CreateCube("REPLACE_Prop_DinhDocLap_LeftGrass", roots.transform.Find("PropRoot"), new Vector3(-22, 0.07f, 0), new Vector3(26, 0.1f, 58), new Color(0.25f, 0.45f, 0.25f));
        CreateCube("REPLACE_Prop_DinhDocLap_RightGrass", roots.transform.Find("PropRoot"), new Vector3(22, 0.07f, 0), new Vector3(26, 0.1f, 58), new Color(0.25f, 0.45f, 0.25f));
        CreateCube("REPLACE_Landmark_DinhDocLap_Palace", landmark, new Vector3(0, 6, 30), new Vector3(34, 12, 7), InactiveGray);
        CreateWorldLabel("Landmark: Dinh Độc Lập Palace", landmark, new Vector3(0, 14, 28), 0.6f);

        CreateNPC(roots.transform.Find("NPCRoot"), "REPLACE_NPC_OldTourGuide", "Old Tour Guide", new Vector3(-12, 1, 24), "Có một năm nằm trong ký ức của sân này.");
        CreateItem(roots.transform.Find("ItemRoot"), "REPLACE_Item_HistoricalMap", "Item: Historical Map", new Vector3(12, 0.6f, -24));
        CreatePuzzle(roots.transform.Find("PuzzleRoot"), "REPLACE_Puzzle_RadioCode", "Puzzle: Radio Code", new Vector3(10, 0.8f, 24), zone, "Historical Code", "Enter the 4 digit year hidden in the memory.", "1975", "1975", new string[0]);
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(-36, 1.2f, 0));

        AddZoneEffects(zone, roots.transform, RestoredGreen, false, true);
        CreateBoundary(landmark, 90, 80);
        AddLighting();
        return Save(scene, SceneLoader.DinhDocLap);
    }

    private static EditorBuildSettingsScene GenerateNhaThoDucBa()
    {
        Scene scene = NewScene(SceneLoader.NhaThoDucBa);
        CreateManagersAndPlayer(new Vector3(0, 1, -32), "Listen to the square and solve the bell sequence.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.NhaThoDucBa, "Bình yên");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform props = roots.transform.Find("PropRoot");

        CreateFloor("REPLACE_Landmark_NhaThoDucBa_Square", landmark, Vector3.zero, new Vector2(80, 80));
        CreateCube("REPLACE_Landmark_NhaThoDucBa_CathedralFront", landmark, new Vector3(0, 7, 28), new Vector3(24, 14, 4), InactiveGray);
        CreateCube("REPLACE_Landmark_NhaThoDucBa_LeftTower", landmark, new Vector3(-15, 12, 28), new Vector3(6, 24, 4), InactiveGray);
        CreateCube("REPLACE_Landmark_NhaThoDucBa_RightTower", landmark, new Vector3(15, 12, 28), new Vector3(6, 24, 4), InactiveGray);
        CreateWorldLabel("Landmark: Nhà thờ Đức Bà", landmark, new Vector3(0, 24, 27), 0.6f);
        CreateCube("REPLACE_Prop_Statue", props, new Vector3(0, 2, 6), new Vector3(4, 4, 4), InactiveGray);

        for (int i = 0; i < 5; i++)
        {
            CreateCube("REPLACE_Prop_Bench_" + (i + 1), props, new Vector3(-24 + i * 12, 0.4f, -4), new Vector3(5, 0.8f, 1.5f), InactiveGray);
            CreateCube("REPLACE_Prop_Pigeon_" + (i + 1), props, new Vector3(-18 + i * 8, 0.25f, 5 + (i % 2) * 4), Vector3.one * 0.5f, InteractableBlue);
        }

        CreateNPC(roots.transform.Find("NPCRoot"), "REPLACE_NPC_OldMan", "Old Man", new Vector3(-7, 1, 5), "Tiếng chuông đúng thứ tự sẽ làm quảng trường yên lại.");
        CreateItem(roots.transform.Find("ItemRoot"), "REPLACE_Item_SmallBell", "Item: Small Bell", new Vector3(16, 0.6f, -6));
        CreatePuzzle(roots.transform.Find("PuzzleRoot"), "REPLACE_Puzzle_BellConsole", "Puzzle: Bell Sequence", new Vector3(8, 0.8f, 21), zone, "Bell Sequence", "Correct order: La, Sol, Re, Mi, Si, Do.", "La-Sol-Re-Mi-Si-Do", "La-Sol-Re-Mi-Si-Do", new[] { "La", "Sol", "Re", "Mi", "Si", "Do" });
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(32, 1.2f, -12));

        AddZoneEffects(zone, roots.transform, RestoredGreen, false, true);
        CreateBoundary(landmark, 80, 80);
        AddLighting();
        return Save(scene, SceneLoader.NhaThoDucBa);
    }

    private static EditorBuildSettingsScene GenerateBitexco()
    {
        Scene scene = NewScene(SceneLoader.Bitexco);
        CreateManagersAndPlayer(new Vector3(0, 1, -32), "Enter the lobby and restore the city changing memory.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.Bitexco, "Chuyển mình");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform props = roots.transform.Find("PropRoot");

        CreateFloor("REPLACE_Landmark_Bitexco_ModernPlaza", landmark, Vector3.zero, new Vector2(80, 80));
        CreateCube("REPLACE_Landmark_Bitexco_Tower", landmark, new Vector3(0, 22, 22), new Vector3(10, 44, 10), InactiveGray);
        CreateCube("REPLACE_Landmark_Bitexco_Helipad", landmark, new Vector3(8, 28, 22), new Vector3(10, 0.8f, 7), InactiveGray);
        CreateCube("REPLACE_Landmark_Bitexco_LobbyEntrance", landmark, new Vector3(0, 2.5f, 10), new Vector3(16, 5, 3), InactiveGray);
        CreateWorldLabel("Landmark: Bitexco Tower", landmark, new Vector3(0, 46, 21), 0.7f);

        for (int i = 0; i < 4; i++)
        {
            CreateCube("REPLACE_Prop_NeonStrip_" + (i + 1), props, new Vector3(-24 + i * 16, 1.2f, -6), new Vector3(10, 0.35f, 0.35f), InteractableBlue);
            CreateCube("REPLACE_Prop_Billboard_" + (i + 1), props, new Vector3(-30 + i * 20, 4, 2), new Vector3(8, 5, 0.6f), InactiveGray);
        }

        CreateNPC(roots.transform.Find("NPCRoot"), "REPLACE_NPC_OfficeWorker_CFO", "Office Worker / CFO", new Vector3(-10, 1, 6), "Mã bảo vệ nằm trong nhịp chuyển mình của tòa nhà.");
        CreateItem(roots.transform.Find("ItemRoot"), "REPLACE_Item_EmployeeCard", "Item: Employee Card", new Vector3(12, 0.6f, 0));
        CreatePuzzle(roots.transform.Find("PuzzleRoot"), "REPLACE_Puzzle_ServerSecurity", "Puzzle: Security Server", new Vector3(0, 0.8f, 4), zone, "Security Code", "Enter the 3 digit office access code.", "345", "345", new string[0]);
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(-32, 1.2f, -12));

        AddZoneEffects(zone, roots.transform, RestoredGreen, false, true);
        CreateBoundary(landmark, 80, 80);
        AddLighting();
        return Save(scene, SceneLoader.Bitexco);
    }

    private static EditorBuildSettingsScene GenerateBachDang()
    {
        Scene scene = NewScene(SceneLoader.BachDang);
        CreateManagersAndPlayer(new Vector3(0, 1, -28), "Walk to the dock and solve the river crossing memory.");
        GameObject roots = CreateStandardRoots();
        MemoryZoneController zone = CreateMemoryZone(roots.transform, LocationId.BachDang, "Dòng chảy thành phố");
        Transform landmark = roots.transform.Find("LandmarkRoot");
        Transform props = roots.transform.Find("PropRoot");

        CreateFloor("REPLACE_Landmark_BachDang_Promenade", landmark, new Vector3(0, 0, -8), new Vector2(100, 44));
        CreateCube("REPLACE_Landmark_BachDang_RiverPlane", landmark, new Vector3(0, -0.03f, 24), new Vector3(100, 0.05f, 26), new Color(0.1f, 0.3f, 0.55f));
        CreateWorldLabel("Landmark: Bến Bạch Đằng Riverside", landmark, new Vector3(0, 3, 14), 0.6f);

        for (int i = 0; i < 5; i++)
        {
            CreateCube("REPLACE_Prop_LampPost_" + (i + 1), props, new Vector3(-38 + i * 19, 2.5f, -3), new Vector3(0.5f, 5, 0.5f), InactiveGray);
        }

        for (int i = 0; i < 3; i++)
        {
            CreateCube("REPLACE_Prop_Boat_" + (i + 1), props, new Vector3(-24 + i * 22, 0.5f, 24), new Vector3(10, 1, 3), InactiveGray);
        }

        CreateCube("REPLACE_Prop_Pier", props, new Vector3(0, 0.25f, 10), new Vector3(14, 0.5f, 14), InactiveGray);
        CreateNPC(roots.transform.Find("NPCRoot"), "REPLACE_NPC_Boatman", "Boatman", new Vector3(-8, 1, 8), "Con sông luôn nhớ số bước để sang bờ.");
        CreateItem(roots.transform.Find("ItemRoot"), "REPLACE_Item_OldFerryTicket", "Item: Old Ferry Ticket", new Vector3(14, 0.6f, 2));
        CreatePuzzle(roots.transform.Find("PuzzleRoot"), "REPLACE_Puzzle_RiverCrossing", "Puzzle: River Crossing", new Vector3(0, 0.8f, 8), zone, "River Crossing", "How many steps to cross the memory river?", "28", "28", new string[0]);
        CreateBusStop(roots.transform.Find("SpawnPoints"), zone, new Vector3(42, 1.2f, -22));

        AddZoneEffects(zone, roots.transform, RestoredGreen, false, true);
        CreateBoundary(landmark, 100, 70);
        AddLighting();
        return Save(scene, SceneLoader.BachDang);
    }

    private static EditorBuildSettingsScene GenerateEnding()
    {
        Scene scene = NewScene(SceneLoader.Ending);
        CreateManagersAndPlayer(new Vector3(0, 1, -28), "Walk toward Landmark 81 and remember the city.");
        GameObject roots = CreateStandardRoots();
        Transform landmark = roots.transform.Find("LandmarkRoot");

        CreateFloor("REPLACE_Ending_RiverWalk", landmark, Vector3.zero, new Vector2(80, 80));
        CreateCube("REPLACE_Landmark81_Tower", landmark, new Vector3(0, 24, 22), new Vector3(10, 48, 10), new Color(0.55f, 0.75f, 1f));
        CreateCube("REPLACE_Landmark81_Spire", landmark, new Vector3(0, 51, 22), new Vector3(2, 6, 2), Color.white);
        CreateWorldLabel("Landmark 81 Ending", landmark, new Vector3(0, 56, 20), 0.7f);
        CreateBoundary(landmark, 80, 80);
        AddLighting();
        return Save(scene, SceneLoader.Ending);
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

    private static void CreateManagersAndPlayer(Vector3 spawnPosition, string objective)
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
        spawnLabel.transform.position = spawnPosition;
        CreateWorldLabel("Player Spawn", spawnLabel.transform, spawnPosition + Vector3.up * 2.2f, 0.35f);

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        cameraObject.AddComponent<AudioListener>();
        ThirdPersonCamera thirdPersonCamera = cameraObject.AddComponent<ThirdPersonCamera>();
        thirdPersonCamera.target = target.transform;
        thirdPersonCamera.distance = 6f;
        thirdPersonCamera.height = 1.4f;
        thirdPersonCamera.mouseSensitivity = 2f;
        thirdPersonCamera.minPitch = 8f;
        thirdPersonCamera.maxPitch = 38f;

        ThirdPersonPlayerController playerController = player.AddComponent<ThirdPersonPlayerController>();
        playerController.cameraTransform = cameraObject.transform;

        Interactor interactor = player.AddComponent<Interactor>();
        interactor.playerCamera = camera;
        interactor.interactionRange = 4f;
        interactor.nearbyInteractionRadius = 3f;

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
        npc.GetComponent<Renderer>().material = CreateMaterial(NpcPurple);
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
        GameObject button = CreateCube(objectName, parent, position, new Vector3(2.2f, 1.1f, 0.35f), PuzzleYellow);
        MapSelectionInteractable interactable = button.AddComponent<MapSelectionInteractable>();
        interactable.targetLocation = locationId;
        interactable.targetScene = sceneName;
        interactable.displayName = displayName;
        interactable.isEndingRoute = ending;
        interactable.statusRenderer = button.GetComponent<Renderer>();
        CreateWorldLabel(displayName, parent, position + Vector3.up * 1.1f, 0.25f);
    }

    private static void CreateHintCube(Transform parent, string objectName, string number, Vector3 position, Color color)
    {
        CreateCube(objectName, parent, position, new Vector3(2.5f, 2.5f, 2.5f), color);
        CreateWorldLabel(number, parent, position + Vector3.up * 2.4f, 0.65f);
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

    private static void CreateWorldLabel(string text, Transform parent, Vector3 position, float size)
    {
        GameObject label = new GameObject("TMP_Label_" + text.Replace(" ", "_"));
        label.transform.SetParent(parent);
        label.transform.position = position;
        label.transform.rotation = Quaternion.identity;

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
        }
        else
        {
            TextMesh mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 32;
            mesh.characterSize = size;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
        }
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
