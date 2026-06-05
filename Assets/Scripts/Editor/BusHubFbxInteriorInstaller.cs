using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BusHubFbxInteriorInstaller
{
    private const string AutoRunFlagPath = "ProjectSettings/KyUcSaiGonApplyBusHubFbx.flag";
    private const string ScenePath = "Assets/Scenes/Scene_00_BusHub.unity";

    private const string OldSeatModelPath = "Assets/Art/UI/BusHub/Seats/bus_seat_double.fbx";
    private const string OldWindowModelPath = "Assets/Art/UI/BusHub/Windows/bus_window_module.fbx";
    private const string OldHandrailModelPath = "Assets/Art/UI/BusHub/Handrails/bus_handrail_module.fbx";

    private const string SeatModelPath = "Assets/Art/Models/BusHub/Seats/bus_seat_double.fbx";
    private const string WindowModelPath = "Assets/Art/Models/BusHub/Windows/bus_window_module.fbx";
    private const string HandrailModelPath = "Assets/Art/Models/BusHub/Handrails/bus_handrail_module.fbx";

    private const string PrefabFolder = "Assets/Art/Prefabs/BusHub";
    private const string MaterialFolder = "Assets/Art/Materials/BusHub";
    private const string SeatPrefabPath = PrefabFolder + "/PF_BusSeat_Double.prefab";
    private const string WindowPrefabPath = PrefabFolder + "/PF_BusWindow_Module.prefab";
    private const string HandrailPrefabPath = PrefabFolder + "/PF_BusHandrail_Module.prefab";

    private static Material seatCushionMaterial;
    private static Material seatFrameMaterial;
    private static Material windowFrameMaterial;
    private static Material windowGlassMaterial;
    private static Material handrailMaterial;
    private static Material metalMaterial;

    private static Vector3 lastSeatBounds;
    private static Vector3 lastWindowBounds;
    private static Vector3 lastHandrailBounds;
    private static Vector3 lastSeatVisualScale;
    private static Vector3 lastWindowVisualScale;
    private static Vector3 lastHandrailVisualScale;

    [InitializeOnLoadMethod]
    private static void AutoRunIfRequested()
    {
        if (!File.Exists(AutoRunFlagPath))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[KyUcSaiGon] BusHub FBX installer skipped because Unity is entering Play Mode.");
                return;
            }

            File.Delete(AutoRunFlagPath);
            ApplyFbxInteriorProps();
        };
    }

    [MenuItem("Ky Uc Sai Gon/BusHub/Apply FBX Interior Props")]
    public static void ApplyFbxInteriorProps()
    {
        EnsureFolders();
        MoveModelIfNeeded(OldSeatModelPath, SeatModelPath);
        MoveModelIfNeeded(OldWindowModelPath, WindowModelPath);
        MoveModelIfNeeded(OldHandrailModelPath, HandrailModelPath);
        ConfigureModelImporter(SeatModelPath, 1f);
        ConfigureModelImporter(WindowModelPath, 1f);
        ConfigureModelImporter(HandrailModelPath, 1f);
        AssetDatabase.Refresh();

        if (!ValidateModel(SeatModelPath) || !ValidateModel(WindowModelPath) || !ValidateModel(HandrailModelPath))
        {
            return;
        }

        CreateMaterials();
        CreateWrapperPrefab(SeatModelPath, SeatPrefabPath, "PF_BusSeat_Double", "Visual_REPLACE_BusSeat_Double", ModelKind.Seat, new Vector3(1.45f, 1.5f, 1.18f));
        CreateWrapperPrefab(WindowModelPath, WindowPrefabPath, "PF_BusWindow_Module", "Visual_REPLACE_BusWindow_Module", ModelKind.Window, new Vector3(4.45f, 2.25f, 0.14f));
        CreateWrapperPrefab(HandrailModelPath, HandrailPrefabPath, "PF_BusHandrail_Module", "Visual_REPLACE_BusHandrail_Module", ModelKind.Handrail, new Vector3(2.6f, 2.25f, 0.22f));

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject busInteriorRoot = FindOrCreate("BusInteriorRoot");
        GameObject rejectedRoot = FindOrCreateChild(busInteriorRoot.transform, "BusInterior_FBX_RejectedOrOld");
        rejectedRoot.SetActive(false);

        MoveOldFbxRootsToRejected(busInteriorRoot.transform, rejectedRoot.transform);
        GameObject seatsRoot = RecreateChild(busInteriorRoot.transform, "BusInterior_Seats_FBX");
        GameObject windowsRoot = RecreateChild(busInteriorRoot.transform, "BusInterior_Windows_FBX");
        GameObject handrailsRoot = RecreateChild(busInteriorRoot.transform, "BusInterior_Handrails_FBX");

        FindOrCreateChild(busInteriorRoot.transform, "BusInterior_Shell_Primitives");
        FindOrCreateChild(busInteriorRoot.transform, "BusInterior_Lights");
        FindOrCreateChild(busInteriorRoot.transform, "BusInterior_Board");

        HideOldPrimitiveVisuals("Visual_REPLACE_BusSeat");
        HideOldPrimitiveVisuals("Visual_REPLACE_Window");
        RestoreOldPrimitiveVisuals("Visual_REPLACE_Handrail");

        PlaceCoachSeats(seatsRoot.transform, AssetDatabase.LoadAssetAtPath<GameObject>(SeatPrefabPath));
        PlaceWallWindows(windowsRoot.transform, AssetDatabase.LoadAssetAtPath<GameObject>(WindowPrefabPath));
        handrailsRoot.SetActive(false);

        WriteSpecsReport();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[KyUcSaiGon] BusHub FBX cleanup complete. Scene saved: " + ScenePath);
    }

    private enum ModelKind
    {
        Seat,
        Window,
        Handrail
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Art", "Models");
        EnsureFolder("Assets/Art/Models", "BusHub");
        EnsureFolder("Assets/Art/Models/BusHub", "Seats");
        EnsureFolder("Assets/Art/Models/BusHub", "Windows");
        EnsureFolder("Assets/Art/Models/BusHub", "Handrails");
        EnsureFolder("Assets/Art", "Prefabs");
        EnsureFolder("Assets/Art/Prefabs", "BusHub");
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "BusHub");
        EnsureFolder("Assets/Docs", "BusHub");
    }

    private static void MoveModelIfNeeded(string oldPath, string newPath)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(newPath) != null)
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(oldPath) == null)
        {
            return;
        }

        string error = AssetDatabase.MoveAsset(oldPath, newPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError("[KyUcSaiGon] Could not move FBX from " + oldPath + " to " + newPath + ": " + error);
        }
    }

    private static void ConfigureModelImporter(string path, float scaleFactor)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            return;
        }

        importer.globalScale = scaleFactor;
        importer.useFileScale = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.addCollider = false;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.SaveAndReimport();
    }

    private static bool ValidateModel(string path)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            return true;
        }

        Debug.LogError("[KyUcSaiGon] Missing FBX model: " + path);
        return false;
    }

    private static void CreateMaterials()
    {
        seatCushionMaterial = CreateOrUpdateMaterial("M_BusSeat_Cushion_Burgundy", new Color(0.34f, 0.07f, 0.08f, 1f), false);
        seatFrameMaterial = CreateOrUpdateMaterial("M_BusSeat_Frame_DarkCharcoal", new Color(0.035f, 0.035f, 0.04f, 1f), false);
        windowFrameMaterial = CreateOrUpdateMaterial("M_BusWindow_Frame_DarkCharcoal", new Color(0.025f, 0.027f, 0.03f, 1f), false);
        windowGlassMaterial = CreateOrUpdateMaterial("M_BusWindow_Glass_LightBlueTransparent", new Color(0.62f, 0.82f, 0.95f, 0.32f), true);
        handrailMaterial = CreateOrUpdateMaterial("M_BusHandrail_Orange", new Color(1f, 0.49f, 0.08f, 1f), false);
        metalMaterial = CreateOrUpdateMaterial("M_BusMetal_ConnectorGray", new Color(0.42f, 0.42f, 0.42f, 1f), false);
    }

    private static void PlaceCoachSeats(Transform parent, GameObject prefab)
    {
        float[] zPositions = { -7.8f, -5.1f, -2.4f, 0.3f, 3f };
        for (int i = 0; i < zPositions.Length; i++)
        {
            string suffix = (i + 1).ToString("00");
            PlacePrefab(parent, prefab, "BusSeat_FBX_Left_" + suffix, new Vector3(-2.15f, 0.24f, zPositions[i]), Quaternion.identity);
            PlacePrefab(parent, prefab, "BusSeat_FBX_Right_" + suffix, new Vector3(2.15f, 0.24f, zPositions[i]), Quaternion.identity);
        }
    }

    private static void PlaceWallWindows(Transform parent, GameObject prefab)
    {
        float[] zPositions = { -12.2f, -7.35f, -2.5f, 2.35f, 7.2f, 12.05f };
        for (int i = 0; i < zPositions.Length; i++)
        {
            string suffix = (i + 1).ToString("00");
            PlacePrefab(parent, prefab, "BusWindow_FBX_Left_" + suffix, new Vector3(-4.31f, 3.45f, zPositions[i]), Quaternion.Euler(0f, 90f, 0f));
            PlacePrefab(parent, prefab, "BusWindow_FBX_Right_" + suffix, new Vector3(4.31f, 3.45f, zPositions[i]), Quaternion.Euler(0f, -90f, 0f));
        }
    }

    private static void PlaceCeilingHandrails(Transform parent, GameObject prefab)
    {
        float[] zPositions = { -8.2f, -2.2f, 3.8f, 9.8f };
        for (int i = 0; i < zPositions.Length; i++)
        {
            string suffix = (i + 1).ToString("00");
            PlacePrefab(parent, prefab, "BusHandrail_FBX_Center_" + suffix, new Vector3(0f, 4.48f, zPositions[i]), Quaternion.identity);
        }
    }

    private static void PlacePrefab(Transform parent, GameObject prefab, string name, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = Vector3.one;
        RemoveImportedColliders(instance);
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void RemoveImportedColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void HideOldPrimitiveVisuals(string prefix)
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform.gameObject.scene.IsValid() && transform.name.StartsWith(prefix))
            {
                transform.gameObject.SetActive(false);
            }
        }
    }

    private static void RestoreOldPrimitiveVisuals(string prefix)
    {
        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform.gameObject.scene.IsValid() && transform.name.StartsWith(prefix))
            {
                transform.gameObject.SetActive(true);
            }
        }
    }

    private static void MoveOldFbxRootsToRejected(Transform busInteriorRoot, Transform rejectedRoot)
    {
        string[] oldNames =
        {
            "BusInterior_Seats",
            "BusInterior_Windows",
            "BusInterior_Handrails",
            "BusInterior_Seats_FBX",
            "BusInterior_Windows_FBX",
            "BusInterior_Handrails_FBX"
        };

        foreach (string oldName in oldNames)
        {
            Transform existing = busInteriorRoot.Find(oldName);
            if (existing != null)
            {
                existing.SetParent(rejectedRoot);
            }
        }
    }

    private static void CreateWrapperPrefab(string modelPath, string prefabPath, string rootName, string visualName, ModelKind kind, Vector3 targetSize)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        GameObject root = new GameObject(rootName);
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
        visual.name = visualName;
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;
        RemoveUnwantedComponents(visual);
        AssignMaterialsByRendererName(visual, kind);
        NormalizeVisualChildToTarget(visual, targetSize, kind);
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static void NormalizeVisualChildToTarget(GameObject visual, Vector3 targetSize, ModelKind kind)
    {
        Bounds rawBounds = CalculateBounds(visual);
        if (rawBounds.size.x <= 0.0001f || rawBounds.size.y <= 0.0001f || rawBounds.size.z <= 0.0001f)
        {
            Debug.LogWarning("[KyUcSaiGon] No renderer bounds found for " + visual.name);
            return;
        }

        Vector3 scale = new Vector3(
            targetSize.x / rawBounds.size.x,
            targetSize.y / rawBounds.size.y,
            targetSize.z / rawBounds.size.z);
        scale.x = Mathf.Clamp(scale.x, 0.001f, 10000f);
        scale.y = Mathf.Clamp(scale.y, 0.001f, 10000f);
        scale.z = Mathf.Clamp(scale.z, 0.001f, 10000f);
        visual.transform.localScale = scale;

        Bounds finalBounds = CalculateBounds(visual);
        if (kind == ModelKind.Seat)
        {
            lastSeatBounds = finalBounds.size;
            lastSeatVisualScale = scale;
        }
        else if (kind == ModelKind.Window)
        {
            lastWindowBounds = finalBounds.size;
            lastWindowVisualScale = scale;
        }
        else
        {
            lastHandrailBounds = finalBounds.size;
            lastHandrailVisualScale = scale;
        }

        Debug.Log("[KyUcSaiGon] " + visual.name + " raw bounds " + rawBounds.size + " normalized to " + finalBounds.size + " with visual scale " + scale);
    }

    private static void RemoveUnwantedComponents(GameObject root)
    {
        foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
        {
            Object.DestroyImmediate(camera.gameObject);
        }

        foreach (Light light in root.GetComponentsInChildren<Light>(true))
        {
            Object.DestroyImmediate(light.gameObject);
        }

        RemoveImportedColliders(root);
    }

    private static void AssignMaterialsByRendererName(GameObject root, ModelKind kind)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            string lowerName = renderer.name.ToLowerInvariant();
            Material material = GetMaterialForRenderer(kind, lowerName);
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static Material GetMaterialForRenderer(ModelKind kind, string lowerName)
    {
        if (kind == ModelKind.Seat)
        {
            return lowerName.Contains("frame") || lowerName.Contains("leg") || lowerName.Contains("metal") || lowerName.Contains("base")
                ? seatFrameMaterial
                : seatCushionMaterial;
        }

        if (kind == ModelKind.Window)
        {
            return lowerName.Contains("glass") || lowerName.Contains("pane") || lowerName.Contains("window")
                ? windowGlassMaterial
                : windowFrameMaterial;
        }

        return lowerName.Contains("connector") || lowerName.Contains("joint") || lowerName.Contains("metal")
            ? metalMaterial
            : handrailMaterial;
    }

    private static Material CreateOrUpdateMaterial(string materialName, Color color, bool transparent)
    {
        string path = MaterialFolder + "/" + materialName + ".mat";
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

        material.color = color;
        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_AlphaClip", 0f);
            material.renderQueue = 3000;
        }
        else
        {
            material.SetFloat("_Surface", 0f);
            material.renderQueue = -1;
        }

        return material;
    }

    private static GameObject FindOrCreate(string name)
    {
        GameObject existing = GameObject.Find(name);
        return existing != null ? existing : new GameObject(name);
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static GameObject RecreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        return FindOrCreateChild(parent, name);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void WriteSpecsReport()
    {
        string path = "Assets/Docs/BusHub/BusHub_FBX_Interior_Specs.md";
        string report =
@"# BusHub FBX Interior Specs

Generated by `Ky Uc Sai Gon/BusHub/Apply FBX Interior Props`.

## Moved FBX Assets

- Seat: `Assets/Art/Models/BusHub/Seats/bus_seat_double.fbx`
- Window: `Assets/Art/Models/BusHub/Windows/bus_window_module.fbx`
- Handrail: `Assets/Art/Models/BusHub/Handrails/bus_handrail_module.fbx`

## Prefabs

- `Assets/Art/Prefabs/BusHub/PF_BusSeat_Double.prefab`
- `Assets/Art/Prefabs/BusHub/PF_BusWindow_Module.prefab`
- `Assets/Art/Prefabs/BusHub/PF_BusHandrail_Module.prefab`

## Current Interior Dimensions

- Length: about 30 units
- Width: about 9 units
- Height: about 5.5 units
- Comfortable central aisle target: about 2.8 to 3.2 units

## Final FBX Layout

## Scale Diagnostics

- Seat FBX import Scale Factor: 1
- Window FBX import Scale Factor: 1
- Handrail FBX import Scale Factor: 1
- Seat prefab root scale: 1,1,1
- Window prefab root scale: 1,1,1
- Handrail prefab root scale: 1,1,1
- Seat visual child scale: " + FormatVector(lastSeatVisualScale) + @"
- Window visual child scale: " + FormatVector(lastWindowVisualScale) + @"
- Handrail visual child scale: " + FormatVector(lastHandrailVisualScale) + @"
- Seat MeshRenderer bounds after prefab normalization: " + FormatVector(lastSeatBounds) + @" units
- Window MeshRenderer bounds after prefab normalization: " + FormatVector(lastWindowBounds) + @" units
- Handrail MeshRenderer bounds after prefab normalization: " + FormatVector(lastHandrailBounds) + @" units
- Scene instance scale for all new FBX props: 1,1,1

### Seats

- Parent: `BusInterior_Seats_FBX`
- Layout: coach bus, two forward-facing columns
- Rows: 5
- Columns: 2
- Visible seat modules: 10
- Left x: -2.15
- Right x: 2.15
- Y: 0.24
- Z positions: -7.8, -5.1, -2.4, 0.3, 3.0
- Rotation: Y 0 for both columns
- Scene instance scale: 1,1,1
- Estimated aisle width: about 2.8 units between the two seat columns

### Windows

- Parent: `BusInterior_Windows_FBX`
- Modules per side: 6
- Left x: -4.31
- Right x: 4.31
- Y: 3.45
- Z positions: -12.2, -7.35, -2.5, 2.35, 7.2, 12.05
- Rotation: left Y 90, right Y -90
- Scale: uniform fitted at scene placement, kept as close to 1 as possible
- Placement: large continuous row along the upper side wall, bottom slightly above the red lower panel and top below the ceiling/handrail area
- Glass material alpha: about 0.32

### Handrails

- Parent: `BusInterior_Handrails_FBX`
- FBX handrail modules rejected for this layout and parent is disabled
- Existing primitive orange ceiling handrails are restored and kept
- Existing primitive ceiling lights remain unchanged

## Kept Gameplay Objects

- Floor, walls, ceiling, ceiling lights
- Blank HCM panorama board
- Route map trigger and paper map UI
- Player, camera, UI, GameProgressManager

## Collision And Camera Notes

- Imported FBX colliders are removed.
- Old primitive seat/window visuals are hidden after FBX replacement.
- Old primitive handrail visuals are kept because the FBX handrail is not suitable for the coach-bus layout yet.
- Old primitive roots/colliders remain where useful for broad collision.
- Failed previous FBX instance roots are moved under disabled `BusInterior_FBX_RejectedOrOld`.

## Recommendation

Keep the current primitive shell and use the fixed FBX seat/window modules for now. Regenerate or re-export the handrail module later if qb wants a coach-bus-specific overhead rail model.
";

        File.WriteAllText(path, report);
        AssetDatabase.ImportAsset(path);
    }

    private static string FormatVector(Vector3 value)
    {
        return value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ", " + value.z.ToString("0.###");
    }
}
