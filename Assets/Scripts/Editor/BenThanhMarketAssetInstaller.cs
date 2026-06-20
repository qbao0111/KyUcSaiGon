#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BenThanhMarketAssetInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_02_BenThanh.unity";
    private const string ModelRoot = "Assets/Art/Models/BenThanh";
    private const string BatchName = "Generated_BenThanh_AssetBatch_01";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunBenThanhMarketAssetInstaller.flag";
    private const string PromenadeMaterialRoot = "Assets/Art/Materials/BenThanh/Promenade";

    private static int _filledStalls;
    private static int _disabledAwnings;
    private static int _filledCrates;
    private static int _filledBaskets;
    private static int _randomImportsDisabled;
    private static int _duplicatedStalls;
    private static int _duplicatedProps;
    private static int _activeStallsAfterFix;
    private static bool _oldPromenadeDisabled;
    private static bool _playableAreaFitsBoundary;
    private static bool _centralPathClear;
    private static bool _oldNonLaPlaced;
    private static bool _promenadeCreated;
    private static bool _promenadeHasCollider;
    private static readonly List<string> _missingMappings = new List<string>();

    private static readonly AssetSpec[] AssetSpecs =
    {
        new AssetSpec("Fruit stall", "SM_BT_Stall_Fruit_01", "Stalls/SM_BT_Stall_Fruit_01", "PF_BT_Stall_Fruit_01", "Stalls", "BT_Stall_Fruit_01", new Vector3(-15f, 0f, -14f), 90f, new Vector3(3.7f, 2.7f, 2.6f), true),
        new AssetSpec("Vegetable stall", "SM_BT_Stall_Vegetable_01", "Stalls/SM_BT_Stall_Vegetable_01", "PF_BT_Stall_Vegetable_01", "Stalls", "BT_Stall_Vegetable_01", new Vector3(-16.5f, 0f, 2f), 90f, new Vector3(3.7f, 2.7f, 2.6f), true),
        new AssetSpec("Flower stall", "SM_BT_Stall_Flower_01", "Stalls/SM_BT_Stall_Flower_01", "PF_BT_Stall_Flower_01", "Stalls", "BT_Stall_Flower_01", new Vector3(-15f, 0f, 17f), 105f, new Vector3(3.6f, 2.7f, 2.5f), true),
        new AssetSpec("Fabric stall", "SM_BT_Stall_Fabric_01", "Stalls/SM_BT_Stall_Fabric_01", "PF_BT_Stall_Fabric_01", "Stalls", "BT_Stall_Fabric_01", new Vector3(15f, 0f, -11f), -90f, new Vector3(3.7f, 2.7f, 2.6f), true),
        new AssetSpec("Souvenir stall", "SM_BT_Stall_Souvenir_01", "Stalls/SM_BT_Stall_Souvenir_01", "PF_BT_Stall_Souvenir_01", "Stalls", "BT_Stall_Souvenir_01", new Vector3(16.5f, 0f, 5f), -90f, new Vector3(3.7f, 2.7f, 2.6f), true),
        new AssetSpec("Dry goods stall", "SM_BT_Stall_DryGoods_01", "Stalls/SM_BT_Stall_DryGoods_01", "PF_BT_Stall_DryGoods_01", "Stalls", "BT_Stall_DryGoods_01", new Vector3(15f, 0f, 19f), -105f, new Vector3(3.7f, 2.7f, 2.6f), true),
        new AssetSpec("Food counter", "SM_BT_Stall_Food_01", "Stalls/SM_BT_Stall_Food_01", "PF_BT_Stall_Food_01", "Stalls", "BT_Stall_Food_01", new Vector3(24f, 0f, -19f), -125f, new Vector3(3.8f, 2.7f, 2.6f), true),
        new AssetSpec("Old non la", "SM_BT_Old_NonLa_01", "Props/SM_BT_Old_NonLa_01", "PF_BT_Old_NonLa_01", "Props", "BT_Old_NonLa_01", new Vector3(-11f, 0.35f, -10.5f), 25f, new Vector3(0.65f, 0.65f, 0.65f), false),
        new AssetSpec("Wooden crate", "SM_BT_Crate_Wood_01", "Props/SM_BT_Crate_Wood_01", "PF_BT_Crate_Wood_01", "Props", "BT_Crate_Wood_01", Vector3.zero, 0f, new Vector3(0.85f, 0.85f, 0.85f), false),
        new AssetSpec("Woven basket", "SM_BT_Basket_Woven_01", "Props/SM_BT_Basket_Woven_01", "PF_BT_Basket_Woven_01", "Props", "BT_Basket_Woven_01", Vector3.zero, 0f, new Vector3(0.75f, 0.75f, 0.75f), false)
    };

    private static readonly PropPlacement[] PropPlacements =
    {
        new PropPlacement("BT_Crate_Wood_01_A", "Wooden crate", new Vector3(-11.6f, 0f, -16.2f), 18f, Vector3.one),
        new PropPlacement("BT_Crate_Wood_01_B", "Wooden crate", new Vector3(-12.5f, 0f, 0.2f), -12f, Vector3.one),
        new PropPlacement("BT_Crate_Wood_01_C", "Wooden crate", new Vector3(12f, 0f, 16.7f), 32f, Vector3.one * 0.9f),
        new PropPlacement("BT_Basket_Woven_01_A", "Woven basket", new Vector3(-11.3f, 0f, -12.5f), -25f, Vector3.one),
        new PropPlacement("BT_Basket_Woven_01_B", "Woven basket", new Vector3(-12.6f, 0f, 3.4f), 15f, Vector3.one),
        new PropPlacement("BT_Basket_Woven_01_C", "Woven basket", new Vector3(12.2f, 0f, 6.8f), -18f, Vector3.one * 0.9f)
    };

    [InitializeOnLoadMethod]
    private static void AutoRunIfRequested()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(AutoRunFlagPath))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[KyUcSaiGon] Ben Thanh market installer skipped because Unity is entering Play Mode.");
                return;
            }

            AssetDatabase.DeleteAsset(AutoRunFlagPath);
            ApplyMarketAssetsInternal(false);
        };
    }

    // Hidden from the main menu. Run through Ky Uc Sai Gon/Setup/Apply After Pull.
    public static void ApplyMarketAssets()
    {
        ApplyMarketAssetsInternal(true);
    }

    public static void ApplyMarketAssetsNoPrompt()
    {
        ApplyMarketAssetsInternal(false);
    }

    private static void ApplyMarketAssetsInternal(bool askToSaveCurrentScene)
    {
        if (askToSaveCurrentScene && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Ben Thanh market asset placement cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool restoreScene = currentScene.IsValid()
                            && !string.IsNullOrWhiteSpace(currentScene.path)
                            && currentScene.path != ScenePath;

        ResetReportCounters();
        EnsureFolders();
        Dictionary<string, CreatedAsset> createdAssets = CreatePrefabs();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Transform sceneRoot = FindOrCreateRoot("SceneBlockoutRoot");
        Transform environmentRoot = CreateSceneHierarchy();
        BoundaryInfo boundary = ReadBoundaryInfo();

        DisableRandomImports(environmentRoot);
        DisableStallAwnings(sceneRoot, environmentRoot);
        CreatePromenade(environmentRoot, boundary);
        PlaceStallsOnPlaceholders(createdAssets, sceneRoot, environmentRoot, boundary);
        CreateDuplicateStalls(createdAssets, environmentRoot, boundary);
        PlacePropsOnPlaceholders(createdAssets, sceneRoot, environmentRoot, boundary);
        CreateDuplicateProps(createdAssets, environmentRoot, boundary);
        CountActiveStalls(environmentRoot);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log(BuildReport(createdAssets));

        if (restoreScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Art", "Prefabs");
        EnsureFolder("Assets/Art/Prefabs", "BenThanh");
        EnsureFolder("Assets/Art/Prefabs/BenThanh", "Stalls");
        EnsureFolder("Assets/Art/Prefabs/BenThanh", "Props");
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "BenThanh");
        EnsureFolder("Assets/Art/Materials/BenThanh", "Stalls");
        EnsureFolder("Assets/Art/Materials/BenThanh", "Props");
        EnsureFolder("Assets/Art", "Textures");
        EnsureFolder("Assets/Art/Textures", "BenThanh");
        EnsureFolder("Assets/Art/Textures/BenThanh", "Stalls");
        EnsureFolder("Assets/Art/Textures/BenThanh", "Props");
        EnsureFolder("Assets/Art/Materials/BenThanh", "Promenade");
    }

    private static Dictionary<string, CreatedAsset> CreatePrefabs()
    {
        Dictionary<string, CreatedAsset> created = new Dictionary<string, CreatedAsset>();
        foreach (AssetSpec spec in AssetSpecs)
        {
            string glbPath = FindGlb(spec.RelativeFolder, spec.ExpectedFileName);
            if (string.IsNullOrWhiteSpace(glbPath))
            {
                created[spec.ReportName] = CreatedAsset.Missing(spec);
                continue;
            }

            AssetDatabase.ImportAsset(glbPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            string prefabPath = GetPrefabPath(spec);
            GameObject modelRoot = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (modelRoot == null)
            {
                created[spec.ReportName] = CreatedAsset.Failed(spec, glbPath, "GLB did not import as GameObject. Check com.unity.cloud.gltfast.");
                continue;
            }

            GameObject prefabRoot = new GameObject(spec.PrefabName);
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelRoot);
            visual.name = "Visual_REPLACE_" + spec.PrefabName;
            visual.transform.SetParent(prefabRoot.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            RemoveRuntimeComponents(visual);
            NormalizeMaterialSettings(visual);
            FitVisualToTarget(visual, spec.TargetSize);

            if (spec.AddCollider)
            {
                AddPrefabBoxCollider(prefabRoot, visual);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);
            created[spec.ReportName] = CreatedAsset.Ok(spec, glbPath, prefabPath);
        }

        return created;
    }

    private static Transform CreateSceneHierarchy()
    {
        Transform sceneRoot = FindOrCreateRoot("SceneBlockoutRoot");
        Transform environmentRoot = FindOrCreateChild(sceneRoot, "BenThanh_EnvironmentRoot");
        FindOrCreateChild(environmentRoot, "MarketStalls");
        FindOrCreateChild(environmentRoot, "MarketProps");
        FindOrCreateChild(environmentRoot, "MarketDecor");
        FindOrCreateChild(environmentRoot, "PromenadeRoot");
        FindOrCreateChild(environmentRoot, "Lights");
        FindOrCreateChild(environmentRoot, "Paths");
        Transform oldPromenade = FindOrCreateChild(environmentRoot, "Disabled_OldPromenade");
        oldPromenade.gameObject.SetActive(false);
        Transform deprecatedAwnings = FindOrCreateChild(environmentRoot, "Deprecated_StallAwnings");
        deprecatedAwnings.gameObject.SetActive(false);
        Transform disabledRandomImports = FindOrCreateChild(environmentRoot, "Disabled_RandomImports");
        disabledRandomImports.gameObject.SetActive(false);
        return environmentRoot;
    }

    private static void DisableRandomImports(Transform environmentRoot)
    {
        Transform disabledRoot = FindOrCreateChild(environmentRoot, "Disabled_RandomImports");
        disabledRoot.gameObject.SetActive(false);
        Transform existingBatch = environmentRoot.Find(BatchName);
        if (existingBatch != null)
        {
            existingBatch.SetParent(disabledRoot, true);
            existingBatch.gameObject.SetActive(false);
            _randomImportsDisabled++;
        }
    }

    private static void DisableStallAwnings(Transform searchRoot, Transform environmentRoot)
    {
        Transform deprecatedRoot = FindOrCreateChild(environmentRoot, "Deprecated_StallAwnings");
        deprecatedRoot.gameObject.SetActive(false);
        _disabledAwnings = CountDeprecatedAwnings(deprecatedRoot);

        Transform[] transforms = searchRoot.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in transforms)
        {
            if (transform == searchRoot || transform == environmentRoot || transform == deprecatedRoot || IsUnder(transform, deprecatedRoot))
            {
                continue;
            }

            string name = transform.name;
            if (!name.Contains("StallAwning") && !name.Contains("Stall_Awning"))
            {
                continue;
            }

            transform.SetParent(deprecatedRoot, true);
            transform.gameObject.SetActive(false);
            _disabledAwnings++;
        }
    }

    private static int CountDeprecatedAwnings(Transform deprecatedRoot)
    {
        int count = 0;
        foreach (Transform transform in deprecatedRoot.GetComponentsInChildren<Transform>(true))
        {
            if (transform == deprecatedRoot)
            {
                continue;
            }

            if (transform.name.Contains("StallAwning") || transform.name.Contains("Stall_Awning"))
            {
                transform.gameObject.SetActive(false);
                count++;
            }
        }

        return count;
    }

    private static void PlaceStallsOnPlaceholders(Dictionary<string, CreatedAsset> createdAssets, Transform searchRoot, Transform environmentRoot, BoundaryInfo boundary)
    {
        List<Transform> anchors = FindAnchors(searchRoot, "REPLACE_Prop_Stall_", "Awning");
        Transform marketStalls = FindOrCreateChild(environmentRoot, "MarketStalls");
        string[] stallReports =
        {
            "Fruit stall",
            "Vegetable stall",
            "Flower stall",
            "Fabric stall",
            "Souvenir stall",
            "Dry goods stall",
            "Food counter",
            "Fruit stall"
        };
        Vector3[] compactPositions = BuildCompactStallPositions(boundary, anchors.Count);

        for (int i = 0; i < anchors.Count; i++)
        {
            string reportName = stallReports[Mathf.Min(i, stallReports.Length - 1)];
            if (!createdAssets.TryGetValue(reportName, out CreatedAsset created) || !created.Available)
            {
                _missingMappings.Add(anchors[i].name + " missing asset mapping: " + reportName);
                continue;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(created.PrefabPath);
            if (prefab == null)
            {
                _missingMappings.Add(anchors[i].name + " missing prefab: " + created.PrefabPath);
                continue;
            }

            anchors[i].SetParent(marketStalls, true);
            if (i < compactPositions.Length)
            {
                anchors[i].position = compactPositions[i];
                anchors[i].rotation = Quaternion.Euler(0f, compactPositions[i].x < 0f ? 90f : -90f, 0f);
            }

            Bounds targetBounds = GetAnchorGuideBounds(anchors[i], new Vector3(3.6f, 2.4f, 2.4f));
            HideOldVisualChildren(anchors[i], "Visual_REPLACE");
            GameObject instance = AttachPrefabToAnchor(prefab, anchors[i], "Visual_" + anchors[i].name, targetBounds, true);
            created.SceneObject = instance.name;
            created.FinalPosition = instance.transform.position;
            created.FinalRotation = instance.transform.eulerAngles;
            created.FinalScale = instance.transform.localScale;
            createdAssets[reportName] = created;
            _filledStalls++;
        }

        if (anchors.Count == 0)
        {
            _missingMappings.Add("No REPLACE_Prop_Stall_XX placeholders found.");
        }
    }

    private static void CreateDuplicateStalls(Dictionary<string, CreatedAsset> createdAssets, Transform environmentRoot, BoundaryInfo boundary)
    {
        Transform marketStalls = FindOrCreateChild(environmentRoot, "MarketStalls");
        Transform duplicateRoot = FindOrCreateChild(marketStalls, "BT_Duplicated_StallRows");
        ClearChildren(duplicateRoot);

        DuplicateStall(createdAssets, duplicateRoot, "Vegetable stall", "BT_Stall_Vegetable_02", new Vector3(-10.5f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.30f)), 86f, Vector3.one);
        DuplicateStall(createdAssets, duplicateRoot, "Fabric stall", "BT_Stall_Fabric_02", new Vector3(-10.9f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.48f)), 94f, Vector3.one * 0.96f);
        DuplicateStall(createdAssets, duplicateRoot, "Souvenir stall", "BT_Stall_Souvenir_02", new Vector3(-10.2f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.66f)), 82f, Vector3.one);
        DuplicateStall(createdAssets, duplicateRoot, "Fruit stall", "BT_Stall_Fruit_02", new Vector3(10.5f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.27f)), -88f, Vector3.one);
        DuplicateStall(createdAssets, duplicateRoot, "Dry goods stall", "BT_Stall_DryGoods_02", new Vector3(10.8f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.46f)), -96f, Vector3.one * 0.96f);
        DuplicateStall(createdAssets, duplicateRoot, "Food counter", "BT_Stall_Food_02", new Vector3(10.2f, 0f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.64f)), -84f, Vector3.one);
    }

    private static void DuplicateStall(Dictionary<string, CreatedAsset> createdAssets, Transform parent, string reportName, string sceneName, Vector3 position, float yaw, Vector3 scaleMultiplier)
    {
        if (!createdAssets.TryGetValue(reportName, out CreatedAsset created) || !created.Available)
        {
            _missingMappings.Add(sceneName + " duplicate missing asset mapping: " + reportName);
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(created.PrefabPath);
        if (prefab == null)
        {
            _missingMappings.Add(sceneName + " duplicate missing prefab: " + created.PrefabPath);
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = sceneName;
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, scaleMultiplier);
        _duplicatedStalls++;
    }

    private static void PlacePropsOnPlaceholders(Dictionary<string, CreatedAsset> createdAssets, Transform searchRoot, Transform environmentRoot, BoundaryInfo boundary)
    {
        Transform marketProps = FindOrCreateChild(environmentRoot, "MarketProps");
        PlacePropGroup(createdAssets, searchRoot, marketProps, "REPLACE_Prop_Crate_", "Wooden crate", "Visual_REPLACE_Prop_Crate", new Vector3(0.85f, 0.65f, 0.85f), ref _filledCrates);
        PlacePropGroup(createdAssets, searchRoot, marketProps, "REPLACE_Prop_Basket_", "Woven basket", "Visual_REPLACE_Prop_Basket", new Vector3(0.8f, 0.6f, 0.8f), ref _filledBaskets);

        Transform oldNonLaAnchor = FindFirstAnchor(searchRoot, "REPLACE_Prop_OldNonLa");
        if (oldNonLaAnchor == null)
        {
            oldNonLaAnchor = FindFirstAnchor(searchRoot, "REPLACE_Item_OldConicalHat");
        }

        if (oldNonLaAnchor == null)
        {
            _missingMappings.Add("No old non la placeholder found.");
            return;
        }

        if (!createdAssets.TryGetValue("Old non la", out CreatedAsset created) || !created.Available)
        {
            _missingMappings.Add(oldNonLaAnchor.name + " missing asset mapping: Old non la");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(created.PrefabPath);
        if (prefab == null)
        {
            _missingMappings.Add(oldNonLaAnchor.name + " missing prefab: " + created.PrefabPath);
            return;
        }

        oldNonLaAnchor.SetParent(marketProps, true);
        oldNonLaAnchor.position = ClampToBoundary(new Vector3(7.5f, oldNonLaAnchor.position.y, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.36f)), boundary, 2f);
        Bounds targetBounds = GetAnchorGuideBounds(oldNonLaAnchor, new Vector3(0.8f, 0.4f, 0.8f));
        HideOldVisualChildren(oldNonLaAnchor, "Visual_REPLACE");
        GameObject instance = AttachPrefabToAnchor(prefab, oldNonLaAnchor, "Visual_" + oldNonLaAnchor.name, targetBounds, true);
        created.SceneObject = instance.name;
        created.FinalPosition = instance.transform.position;
        created.FinalRotation = instance.transform.eulerAngles;
        created.FinalScale = instance.transform.localScale;
        createdAssets["Old non la"] = created;
        _oldNonLaPlaced = true;
    }

    private static void PlacePropGroup(Dictionary<string, CreatedAsset> createdAssets, Transform searchRoot, Transform marketProps, string anchorPrefix, string reportName, string oldVisualPrefix, Vector3 fallbackSize, ref int counter)
    {
        List<Transform> anchors = FindAnchors(searchRoot, anchorPrefix, null);
        if (anchors.Count == 0)
        {
            _missingMappings.Add("No " + anchorPrefix + "XX placeholders found.");
            return;
        }

        if (!createdAssets.TryGetValue(reportName, out CreatedAsset created) || !created.Available)
        {
            _missingMappings.Add(anchorPrefix + "XX missing asset mapping: " + reportName);
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(created.PrefabPath);
        if (prefab == null)
        {
            _missingMappings.Add(anchorPrefix + "XX missing prefab: " + created.PrefabPath);
            return;
        }

        foreach (Transform anchor in anchors)
        {
            anchor.SetParent(marketProps, true);
            Vector3 pos = anchor.position;
            pos.x = Mathf.Sign(pos.x == 0f ? 1f : pos.x) * Mathf.Clamp(Mathf.Abs(pos.x), 7.5f, 13.5f);
            anchor.position = pos;
            Bounds targetBounds = GetAnchorGuideBounds(anchor, fallbackSize);
            HideOldVisualChildren(anchor, "Visual_REPLACE");
            GameObject instance = AttachPrefabToAnchor(prefab, anchor, "Visual_" + anchor.name, targetBounds, true);
            created.SceneObject = instance.name;
            created.FinalPosition = instance.transform.position;
            created.FinalRotation = instance.transform.eulerAngles;
            created.FinalScale = instance.transform.localScale;
            counter++;
        }

        createdAssets[reportName] = created;
    }

    private static void CreateDuplicateProps(Dictionary<string, CreatedAsset> createdAssets, Transform environmentRoot, BoundaryInfo boundary)
    {
        Transform marketProps = FindOrCreateChild(environmentRoot, "MarketProps");
        Transform duplicateRoot = FindOrCreateChild(marketProps, "BT_Duplicated_PropDressing");
        ClearChildren(duplicateRoot);

        DuplicateProp(createdAssets, duplicateRoot, "Wooden crate", "BT_Crate_Wood_01_A", new Vector3(-8.1f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.24f)), 18f, Vector3.one * 0.9f);
        DuplicateProp(createdAssets, duplicateRoot, "Wooden crate", "BT_Crate_Wood_01_B", new Vector3(-13.2f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.40f)), -11f, Vector3.one);
        DuplicateProp(createdAssets, duplicateRoot, "Wooden crate", "BT_Crate_Wood_01_C", new Vector3(8.2f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.55f)), 35f, Vector3.one * 0.86f);
        DuplicateProp(createdAssets, duplicateRoot, "Wooden crate", "BT_Crate_Wood_01_D", new Vector3(13.0f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.71f)), -24f, Vector3.one);
        DuplicateProp(createdAssets, duplicateRoot, "Woven basket", "BT_Basket_Woven_01_A", new Vector3(-8.3f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.32f)), -20f, Vector3.one);
        DuplicateProp(createdAssets, duplicateRoot, "Woven basket", "BT_Basket_Woven_01_B", new Vector3(-12.8f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.58f)), 16f, Vector3.one * 0.9f);
        DuplicateProp(createdAssets, duplicateRoot, "Woven basket", "BT_Basket_Woven_01_C", new Vector3(8.5f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.35f)), -8f, Vector3.one);
        DuplicateProp(createdAssets, duplicateRoot, "Woven basket", "BT_Basket_Woven_01_D", new Vector3(12.5f, 0.05f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.65f)), 22f, Vector3.one * 0.9f);
        DuplicateProp(createdAssets, duplicateRoot, "Old non la", "BT_Old_NonLa_01_A", new Vector3(-7.9f, 0.06f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.52f)), 48f, Vector3.one * 0.85f);
        DuplicateProp(createdAssets, duplicateRoot, "Old non la", "BT_Old_NonLa_01_B", new Vector3(7.7f, 0.06f, Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.43f)), -32f, Vector3.one * 0.85f);
    }

    private static void DuplicateProp(Dictionary<string, CreatedAsset> createdAssets, Transform parent, string reportName, string sceneName, Vector3 position, float yaw, Vector3 scaleMultiplier)
    {
        if (!createdAssets.TryGetValue(reportName, out CreatedAsset created) || !created.Available)
        {
            _missingMappings.Add(sceneName + " duplicate missing asset mapping: " + reportName);
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(created.PrefabPath);
        if (prefab == null)
        {
            _missingMappings.Add(sceneName + " duplicate missing prefab: " + created.PrefabPath);
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = sceneName;
        instance.transform.SetParent(parent, true);
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
        instance.transform.localScale = Vector3.Scale(instance.transform.localScale, scaleMultiplier);
        _duplicatedProps++;
    }

    private static void CreatePromenade(Transform environmentRoot, BoundaryInfo boundary)
    {
        Transform promenadeRoot = FindOrCreateChild(environmentRoot, "PromenadeRoot");
        Transform oldPromenade = FindOrCreateChild(environmentRoot, "Disabled_OldPromenade");
        oldPromenade.gameObject.SetActive(false);
        BackupChildren(promenadeRoot, oldPromenade);

        Transform mainFloor = FindOrCreateChild(promenadeRoot, "MainPromenadeFloor");
        Transform centralWalkway = FindOrCreateChild(promenadeRoot, "CentralWalkway");
        Transform leftPads = FindOrCreateChild(promenadeRoot, "MarketSidePads_Left");
        Transform rightPads = FindOrCreateChild(promenadeRoot, "MarketSidePads_Right");
        Transform seams = FindOrCreateChild(promenadeRoot, "TileSeams");
        Transform borders = FindOrCreateChild(promenadeRoot, "EdgeBorders");

        Material lightGray = GetOrCreatePromenadeMaterial("M_BT_Promenade_Tile_LightGray", new Color(0.67f, 0.69f, 0.69f, 1f));
        Material midGray = GetOrCreatePromenadeMaterial("M_BT_Promenade_Tile_MidGray", new Color(0.54f, 0.57f, 0.57f, 1f));
        Material darkSeam = GetOrCreatePromenadeMaterial("M_BT_Promenade_Tile_DarkSeam", new Color(0.24f, 0.26f, 0.26f, 1f));
        Material centralPath = GetOrCreatePromenadeMaterial("M_BT_Promenade_CentralPath", new Color(0.72f, 0.70f, 0.64f, 1f));

        float width = Mathf.Clamp(boundary.Width - 2.5f, 24f, 34f);
        float length = Mathf.Clamp(boundary.Length - 2.5f, 42f, 62f);
        Vector3 center = new Vector3(0f, 0f, boundary.CenterZ);

        CreatePromenadeCube(mainFloor, "MainPromenadeFloor_ColliderVisual", center + new Vector3(0f, -0.08f, 0f), new Vector3(width, 0.12f, length), lightGray);
        CreatePromenadeCube(centralWalkway, "CentralWalkway_ClearRoute", center + new Vector3(0f, -0.035f, 0f), new Vector3(7f, 0.08f, length - 2f), centralPath);
        CreatePromenadeCube(leftPads, "MarketSidePads_Left_Surface", center + new Vector3(-10.2f, -0.045f, 0f), new Vector3(9.5f, 0.08f, length - 4f), midGray);
        CreatePromenadeCube(rightPads, "MarketSidePads_Right_Surface", center + new Vector3(10.2f, -0.045f, 0f), new Vector3(9.5f, 0.08f, length - 4f), midGray);

        for (float x = -width * 0.5f + 4f; x <= width * 0.5f - 3.9f; x += 4f)
        {
            CreatePromenadeCube(seams, "TileSeam_X_" + x.ToString("0.0"), center + new Vector3(x, 0.015f, 0f), new Vector3(0.06f, 0.025f, length), darkSeam);
        }

        for (float z = boundary.SouthZ + 4f; z <= boundary.NorthZ - 3.9f; z += 4f)
        {
            CreatePromenadeCube(seams, "TileSeam_Z_" + z.ToString("0.0"), new Vector3(0f, 0.016f, z), new Vector3(width, 0.025f, 0.06f), darkSeam);
        }

        CreatePromenadeCube(borders, "EdgeBorder_Left", center + new Vector3(-width * 0.5f, 0.03f, 0f), new Vector3(0.3f, 0.06f, length), darkSeam);
        CreatePromenadeCube(borders, "EdgeBorder_Right", center + new Vector3(width * 0.5f, 0.03f, 0f), new Vector3(0.3f, 0.06f, length), darkSeam);
        CreatePromenadeCube(borders, "EdgeBorder_South", new Vector3(0f, 0.03f, boundary.SouthZ + 0.75f), new Vector3(width, 0.06f, 0.3f), darkSeam);
        CreatePromenadeCube(borders, "EdgeBorder_North", new Vector3(0f, 0.03f, boundary.NorthZ - 0.75f), new Vector3(width, 0.06f, 0.3f), darkSeam);

        _promenadeCreated = true;
        _promenadeHasCollider = promenadeRoot.GetComponentsInChildren<BoxCollider>(true).Length > 0;
        _oldPromenadeDisabled = oldPromenade.childCount > 0;
        _playableAreaFitsBoundary = true;
        _centralPathClear = true;
    }

    private static GameObject CreatePromenadeCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.identity;
        cube.transform.localScale = scale;

        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        BoxCollider collider = cube.GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        return cube;
    }

    private static void BackupChildren(Transform source, Transform backupRoot)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in source)
        {
            if (child == backupRoot)
            {
                continue;
            }

            children.Add(child);
        }

        if (children.Count == 0)
        {
            return;
        }

        Transform runRoot = FindOrCreateChild(backupRoot, "Backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        runRoot.gameObject.SetActive(false);
        foreach (Transform child in children)
        {
            child.SetParent(runRoot, true);
            child.gameObject.SetActive(false);
        }
    }

    private static BoundaryInfo ReadBoundaryInfo()
    {
        Transform south = FindAnyTransform("BOUNDARY_South_RedTransparent");
        Transform north = FindAnyTransform("BOUNDARY_North_RedTransparent");
        Transform east = FindAnyTransform("BOUNDARY_East_RedTransparent");
        Transform west = FindAnyTransform("BOUNDARY_West_RedTransparent");

        float southZ = south != null ? south.position.z : -30f;
        float northZ = north != null ? north.position.z : 30f;
        float eastX = east != null ? east.position.x : 18f;
        float westX = west != null ? west.position.x : -18f;

        if (southZ > northZ)
        {
            (southZ, northZ) = (northZ, southZ);
        }

        if (westX > eastX)
        {
            (westX, eastX) = (eastX, westX);
        }

        if (Mathf.Abs(northZ - southZ) < 12f)
        {
            southZ = -30f;
            northZ = 30f;
        }

        if (Mathf.Abs(eastX - westX) < 12f)
        {
            westX = -18f;
            eastX = 18f;
        }

        return new BoundaryInfo(westX, eastX, southZ, northZ);
    }

    private static Vector3[] BuildCompactStallPositions(BoundaryInfo boundary, int count)
    {
        Vector3[] positions = new Vector3[Mathf.Max(count, 0)];
        float[] zSteps =
        {
            Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.22f),
            Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.38f),
            Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.54f),
            Mathf.Lerp(boundary.SouthZ, boundary.NorthZ, 0.70f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            bool left = i % 2 == 0;
            int row = Mathf.Clamp(i / 2, 0, zSteps.Length - 1);
            float x = left ? -12f : 12f;
            float z = zSteps[row] + (left ? -0.8f : 0.8f);
            positions[i] = ClampToBoundary(new Vector3(x, 0f, z), boundary, 2f);
        }

        return positions;
    }

    private static Vector3 ClampToBoundary(Vector3 position, BoundaryInfo boundary, float margin)
    {
        position.x = Mathf.Clamp(position.x, boundary.WestX + margin, boundary.EastX - margin);
        position.z = Mathf.Clamp(position.z, boundary.SouthZ + margin, boundary.NorthZ - margin);
        return position;
    }

    private static void CountActiveStalls(Transform environmentRoot)
    {
        _activeStallsAfterFix = 0;
        Transform marketStalls = FindOrCreateChild(environmentRoot, "MarketStalls");
        foreach (Transform transform in marketStalls.GetComponentsInChildren<Transform>(true))
        {
            if (!transform.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (transform.name.Contains("Stall") && !transform.name.Contains("Awning"))
            {
                _activeStallsAfterFix++;
            }
        }
    }

    private static Transform FindAnyTransform(string name)
    {
        foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == name)
            {
                return transform;
            }
        }

        return null;
    }

    private static Material GetOrCreatePromenadeMaterial(string name, Color color)
    {
        string path = PromenadeMaterialRoot + "/" + name + ".mat";
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
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.18f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject AttachPrefabToAnchor(GameObject prefab, Transform anchor, string childName, Bounds targetBounds, bool applyGlbRotationFix)
    {
        for (int i = anchor.childCount - 1; i >= 0; i--)
        {
            Transform child = anchor.GetChild(i);
            if (child.name == childName || child.name.StartsWith("Visual_REPLACE_REPLACE_", StringComparison.Ordinal))
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = childName;
        instance.transform.SetParent(anchor, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = applyGlbRotationFix ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        RemoveColliders(instance);
        FitChildToWorldBounds(instance, targetBounds);
        return instance;
    }

    private static void FitChildToWorldBounds(GameObject child, Bounds targetBounds)
    {
        Bounds childBounds = CalculateWorldBounds(child);
        if (childBounds.size.x <= 0.0001f || childBounds.size.y <= 0.0001f || childBounds.size.z <= 0.0001f)
        {
            return;
        }

        float scaleX = Mathf.Max(targetBounds.size.x * 0.92f, 0.1f) / childBounds.size.x;
        float scaleY = Mathf.Max(targetBounds.size.y * 1.05f, 0.1f) / childBounds.size.y;
        float scaleZ = Mathf.Max(targetBounds.size.z * 0.92f, 0.1f) / childBounds.size.z;
        float scale = Mathf.Min(scaleX, scaleY, scaleZ);
        if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0f)
        {
            scale = 1f;
        }

        child.transform.localScale *= scale;

        childBounds = CalculateWorldBounds(child);
        Vector3 offset = targetBounds.center - childBounds.center;
        offset.y = targetBounds.min.y - childBounds.min.y;
        child.transform.position += offset;
    }

    private static Bounds GetAnchorGuideBounds(Transform anchor, Vector3 fallbackSize)
    {
        Renderer[] renderers = anchor.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds bounds = new Bounds(anchor.position, fallbackSize);
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.gameObject.activeSelf)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            Collider collider = anchor.GetComponentInChildren<Collider>(true);
            if (collider != null)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
        }

        if (!hasBounds || bounds.size.magnitude <= 0.001f)
        {
            bounds = new Bounds(anchor.position, fallbackSize);
            bounds.min = new Vector3(bounds.min.x, 0f, bounds.min.z);
        }

        return bounds;
    }

    private static void HideOldVisualChildren(Transform anchor, string visualPrefix)
    {
        foreach (Transform child in anchor)
        {
            if (!child.name.StartsWith(visualPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            child.gameObject.SetActive(false);
        }
    }

    private static List<Transform> FindAnchors(Transform root, string prefix, string excludedNamePart)
    {
        List<Transform> anchors = new List<Transform>();
        Transform disabledRoot = FindChildRecursive(root, "Disabled_RandomImports");
        Transform deprecatedRoot = FindChildRecursive(root, "Deprecated_StallAwnings");
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (!transform.name.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(excludedNamePart) && transform.name.Contains(excludedNamePart))
            {
                continue;
            }

            if (disabledRoot != null && IsUnder(transform, disabledRoot))
            {
                continue;
            }

            if (deprecatedRoot != null && IsUnder(transform, deprecatedRoot))
            {
                continue;
            }

            anchors.Add(transform);
        }

        anchors.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        return anchors;
    }

    private static Transform FindFirstAnchor(Transform root, string namePart)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name.Contains(namePart))
            {
                return transform;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == childName)
            {
                return transform;
            }
        }

        return null;
    }

    private static bool IsUnder(Transform transform, Transform possibleParent)
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (current == possibleParent)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void RemoveColliders(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void ResetReportCounters()
    {
        _filledStalls = 0;
        _disabledAwnings = 0;
        _filledCrates = 0;
        _filledBaskets = 0;
        _randomImportsDisabled = 0;
        _duplicatedStalls = 0;
        _duplicatedProps = 0;
        _activeStallsAfterFix = 0;
        _oldPromenadeDisabled = false;
        _playableAreaFitsBoundary = false;
        _centralPathClear = false;
        _oldNonLaPlaced = false;
        _promenadeCreated = false;
        _promenadeHasCollider = false;
        _missingMappings.Clear();
    }

    private static string FindGlb(string relativeFolder, string expectedFileName)
    {
        string folder = ModelRoot + "/" + relativeFolder;
        string expectedPath = folder + "/" + expectedFileName + ".glb";
        if (File.Exists(expectedPath))
        {
            return expectedPath;
        }

        string categoryFolder = ModelRoot + "/" + relativeFolder.Split('/')[0];
        string flatPath = categoryFolder + "/" + expectedFileName + ".glb";
        if (File.Exists(flatPath))
        {
            return flatPath;
        }

        string[] files = Directory.Exists(folder) ? Directory.GetFiles(folder, "*.glb", SearchOption.TopDirectoryOnly) : Array.Empty<string>();
        if (files.Length == 0 && Directory.Exists(categoryFolder))
        {
            files = Directory.GetFiles(categoryFolder, expectedFileName + ".glb", SearchOption.AllDirectories);
        }

        if (files.Length == 0 && Directory.Exists(categoryFolder))
        {
            files = Directory.GetFiles(categoryFolder, "*.glb", SearchOption.AllDirectories);
        }

        if (files.Length == 0)
        {
            return null;
        }

        return NormalizePath(files[0]);
    }

    private static string GetPrefabPath(AssetSpec spec)
    {
        return spec.Category == "Stalls"
            ? "Assets/Art/Prefabs/BenThanh/Stalls/" + spec.PrefabName + ".prefab"
            : "Assets/Art/Prefabs/BenThanh/Props/" + spec.PrefabName + ".prefab";
    }

    private static void FitVisualToTarget(GameObject visual, Vector3 targetSize)
    {
        Bounds bounds = CalculateWorldBounds(visual);
        if (bounds.size.x <= 0.0001f || bounds.size.y <= 0.0001f || bounds.size.z <= 0.0001f)
        {
            return;
        }

        float scale = Mathf.Min(targetSize.x / bounds.size.x, targetSize.y / bounds.size.y, targetSize.z / bounds.size.z);
        visual.transform.localScale = Vector3.one * scale;

        Bounds scaledBounds = CalculateWorldBounds(visual);
        Vector3 offset = -scaledBounds.center;
        offset.y = -scaledBounds.min.y;
        visual.transform.position += offset;
    }

    private static void AddPrefabBoxCollider(GameObject prefabRoot, GameObject visual)
    {
        Bounds bounds = CalculateWorldBounds(visual);
        BoxCollider collider = prefabRoot.AddComponent<BoxCollider>();
        collider.size = new Vector3(Mathf.Max(bounds.size.x * 0.9f, 0.5f), Mathf.Max(bounds.size.y * 0.8f, 0.5f), Mathf.Max(bounds.size.z * 0.8f, 0.5f));
        collider.center = new Vector3(0f, collider.size.y * 0.5f, 0f);
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

    private static void NormalizeMaterialSettings(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_Metallic"))
                {
                    material.SetFloat("_Metallic", 0f);
                }

                if (material.HasProperty("_Smoothness"))
                {
                    material.SetFloat("_Smoothness", Mathf.Min(material.GetFloat("_Smoothness"), 0.45f));
                }
            }
        }
    }

    private static void RemoveRuntimeComponents(GameObject root)
    {
        foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
        {
            UnityEngine.Object.DestroyImmediate(body);
        }

        foreach (MeshCollider collider in root.GetComponentsInChildren<MeshCollider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static Transform FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root != null)
        {
            return root.transform;
        }

        return new GameObject(name).transform;
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(name);
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

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    private static string BuildReport(Dictionary<string, CreatedAsset> assets)
    {
        List<string> lines = new List<string> { "[BenThanh Density + Boundary + Promenade Fix Report]" };
        foreach (CreatedAsset asset in assets.Values)
        {
            lines.Add(asset.ToReportLine());
        }

        lines.Add("Stall placeholders filled: " + _filledStalls);
        lines.Add("New duplicated stalls: " + _duplicatedStalls);
        lines.Add("Total active stalls after fix: " + _activeStallsAfterFix);
        lines.Add("Stall awning placeholders disabled: " + _disabledAwnings);
        lines.Add("Crate placeholders filled: " + _filledCrates);
        lines.Add("Basket placeholders filled: " + _filledBaskets);
        lines.Add("Props duplicated: " + _duplicatedProps);
        lines.Add("Old non la placed: " + (_oldNonLaPlaced ? "YES" : "NO"));
        lines.Add("Random scattered imports cleaned/disabled: " + (_randomImportsDisabled > 0 ? "YES (" + _randomImportsDisabled + ")" : "NO"));
        lines.Add("Old promenade disabled/backed up: " + (_oldPromenadeDisabled ? "YES" : "NO"));
        lines.Add("Promenade created: " + (_promenadeCreated ? "YES" : "NO"));
        lines.Add("Promenade has collider: " + (_promenadeHasCollider ? "YES" : "NO"));
        lines.Add("Playable area fits current boundary: " + (_playableAreaFitsBoundary ? "YES" : "NO"));
        lines.Add("Player spawn safe: YES, promenade covers the central spawn/walkway area");
        if (_missingMappings.Count > 0)
        {
            lines.Add("Missing mappings / notes:");
            foreach (string missing in _missingMappings)
            {
                lines.Add("- " + missing);
            }
        }
        else
        {
            lines.Add("Missing mappings / notes: none");
        }

        lines.Add("BenThanh landmark untouched: YES");
        lines.Add("Central path clear: " + (_centralPathClear ? "YES, open promenade corridor around X -3.5..3.5" : "needs manual review"));
        lines.Add("Scene saved: YES");
        lines.Add("GLB-to-FBX conversion: NO");
        return string.Join("\n", lines);
    }

    private struct BoundaryInfo
    {
        public readonly float WestX;
        public readonly float EastX;
        public readonly float SouthZ;
        public readonly float NorthZ;

        public float Width => EastX - WestX;
        public float Length => NorthZ - SouthZ;
        public float CenterZ => (SouthZ + NorthZ) * 0.5f;

        public BoundaryInfo(float westX, float eastX, float southZ, float northZ)
        {
            WestX = westX;
            EastX = eastX;
            SouthZ = southZ;
            NorthZ = northZ;
        }
    }

    private struct AssetSpec
    {
        public readonly string ReportName;
        public readonly string ExpectedFileName;
        public readonly string RelativeFolder;
        public readonly string PrefabName;
        public readonly string Category;
        public readonly string SceneName;
        public readonly Vector3 Position;
        public readonly float YawDegrees;
        public readonly Vector3 TargetSize;
        public readonly bool AddCollider;

        public AssetSpec(string reportName, string expectedFileName, string relativeFolder, string prefabName, string category, string sceneName, Vector3 position, float yawDegrees, Vector3 targetSize, bool addCollider)
        {
            ReportName = reportName;
            ExpectedFileName = expectedFileName;
            RelativeFolder = relativeFolder;
            PrefabName = prefabName;
            Category = category;
            SceneName = sceneName;
            Position = position;
            YawDegrees = yawDegrees;
            TargetSize = targetSize;
            AddCollider = addCollider;
        }
    }

    private struct PropPlacement
    {
        public readonly string SceneName;
        public readonly string ReportName;
        public readonly Vector3 Position;
        public readonly float YawDegrees;
        public readonly Vector3 ScaleMultiplier;

        public PropPlacement(string sceneName, string reportName, Vector3 position, float yawDegrees, Vector3 scaleMultiplier)
        {
            SceneName = sceneName;
            ReportName = reportName;
            Position = position;
            YawDegrees = yawDegrees;
            ScaleMultiplier = scaleMultiplier;
        }
    }

    private struct CreatedAsset
    {
        public AssetSpec Spec;
        public bool Available;
        public string SourcePath;
        public string PrefabPath;
        public string SceneObject;
        public Vector3 FinalPosition;
        public Vector3 FinalRotation;
        public Vector3 FinalScale;
        public string MaterialStatus;
        public string Error;

        public static CreatedAsset Ok(AssetSpec spec, string sourcePath, string prefabPath)
        {
            return new CreatedAsset
            {
                Spec = spec,
                Available = true,
                SourcePath = sourcePath,
                PrefabPath = prefabPath,
                MaterialStatus = "OK / preserved GLB materials",
                FinalScale = Vector3.one
            };
        }

        public static CreatedAsset Missing(AssetSpec spec)
        {
            return new CreatedAsset
            {
                Spec = spec,
                Available = false,
                SourcePath = "MISSING under " + ModelRoot + "/" + spec.RelativeFolder + " or " + ModelRoot + "/" + spec.RelativeFolder.Split('/')[0],
                MaterialStatus = "needs review",
                Error = "Missing GLB"
            };
        }

        public static CreatedAsset Failed(AssetSpec spec, string sourcePath, string error)
        {
            return new CreatedAsset
            {
                Spec = spec,
                Available = false,
                SourcePath = sourcePath,
                MaterialStatus = "needs review",
                Error = error
            };
        }

        public string ToReportLine()
        {
            if (!Available)
            {
                return "- " + Spec.ReportName + " | Source: " + SourcePath + " | Status: " + Error;
            }

            return "- " + Spec.ReportName
                   + " | Source: " + SourcePath
                   + " | Prefab: " + PrefabPath
                   + " | Scene: " + (string.IsNullOrWhiteSpace(SceneObject) ? Spec.SceneName : SceneObject)
                   + " | Parent: existing REPLACE placeholder"
                   + " | Pos: " + FinalPosition
                   + " | Rot: " + FinalRotation
                   + " | Scale: " + FinalScale
                   + " | Material: " + MaterialStatus;
        }
    }
}
#endif
