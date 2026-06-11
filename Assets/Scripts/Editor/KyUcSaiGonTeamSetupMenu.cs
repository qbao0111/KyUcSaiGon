#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KyUcSaiGonTeamSetupMenu
{
    private const string MenuRoot = "Ky Uc Sai Gon/Team Setup/";

    private static readonly string[] GameplayScenes =
    {
        "Assets/Scenes/Scene_00_BusHub.unity",
        "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity",
        "Assets/Scenes/Scene_02_BenThanh.unity",
        "Assets/Scenes/Scene_03_DinhDocLap.unity",
        "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
        "Assets/Scenes/Scene_05_Bitexco.unity",
        "Assets/Scenes/Scene_06_BachDang.unity",
        "Assets/Scenes/Scene_07_Ending.unity"
    };

    private static readonly string[] RequiredLargeAssets =
    {
        "Assets/Art/Models/Common/Characters/AoDai/AoDai.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Idle.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Walk.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Jog.fbx",
        "Assets/Art/Models/BusHub/Seats/bus_seat_double.fbx",
        "Assets/Art/Models/BusHub/Windows/bus_window_module.fbx",
        "Assets/Art/Models/BusHub/Handrails/bus_handrail_module.fbx",
        "Assets/Art/Models/BenThanh/BenThanh.glb",
        "Assets/Art/Models/NguyenHue/Backdrops/nguyen_hue_city_hall_backdrop.fbx",
        "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_left.fbx",
        "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_right.fbx",
        "Assets/Art/Models/NguyenHue/Fountain/Fountain.fbx",
        "Assets/Art/Models/NguyenHue/LEDPanels/nguyen_hue_led_hint_panel.fbx",
        "Assets/Art/Models/NguyenHue/PromenadeTiles/nguyen_hue_promenade_tile_module.fbx",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_bench.glb",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_lamp.glb",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_tree.glb"
    };

    private static readonly string[] FocusedReimportAssets =
    {
        "Assets/Art/Models/Common/Characters/AoDai/AoDai.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Idle.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Walk.fbx",
        "Assets/Art/Animations/Characters/AoDai/AoDai_Jog.fbx",
        "Assets/Art/Models/BusHub/Seats/bus_seat_double.fbx",
        "Assets/Art/Models/BusHub/Windows/bus_window_module.fbx",
        "Assets/Art/Models/BusHub/Handrails/bus_handrail_module.fbx",
        "Assets/Art/Models/BenThanh/BenThanh.glb",
        "Assets/Art/Models/NguyenHue/Backdrops/nguyen_hue_city_hall_backdrop.fbx",
        "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_left.fbx",
        "Assets/Art/Models/NguyenHue/Facades/nguyen_hue_facade_row_right.fbx",
        "Assets/Art/Models/NguyenHue/Fountain/Fountain.fbx",
        "Assets/Art/Models/NguyenHue/LEDPanels/nguyen_hue_led_hint_panel.fbx",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_bench.glb",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_lamp.glb",
        "Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_tree.glb"
    };

    [MenuItem(MenuRoot + "Apply After Pull")]
    public static void ApplyAfterPull()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[KyUcSaiGon] Stop Play Mode before running Team Setup.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Team setup cancelled.");
            return;
        }

        Scene startingScene = SceneManager.GetActiveScene();
        string startingScenePath = startingScene.IsValid() ? startingScene.path : string.Empty;
        List<string> report = new List<string>();
        List<string> blockingIssues = ValidateCloneAssets();

        if (blockingIssues.Count > 0)
        {
            string message = "Team setup stopped. Missing or unhydrated assets:\n\n"
                             + string.Join("\n", blockingIssues)
                             + "\n\nRun:\n git lfs install\n git lfs pull\n git lfs checkout";
            Debug.LogError("[KyUcSaiGon] " + message);
            EditorUtility.DisplayDialog("Ky Uc Sai Gon Team Setup", message, "OK");
            return;
        }

        try
        {
            EnsureBuildSettings();
            report.Add("Build Settings scenes verified.");

            ReimportFocusedAssets();
            report.Add("Focused art/model assets reimported.");

            ApplyBusHub();
            report.Add("BusHub interior props applied.");

            ApplyNguyenHueArtSetup();
            report.Add("Nguyen Hue art references applied.");

            KyUcSaiGonPlayerModelMenu.ApplyAoDaiToAllScenes();
            report.Add("AoDai player visual applied to all gameplay scenes.");

            RestoreScene(startingScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("[KyUcSaiGon] Team setup complete:\n- " + string.Join("\n- ", report));
            EditorUtility.DisplayDialog("Ky Uc Sai Gon Team Setup", "Apply complete.\n\n- " + string.Join("\n- ", report), "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            RestoreScene(startingScenePath);
            EditorUtility.DisplayDialog("Ky Uc Sai Gon Team Setup", "Apply failed. Check Console for details.", "OK");
        }
    }

    [MenuItem(MenuRoot + "Validate Clone Assets")]
    public static void ValidateCloneAssetsMenu()
    {
        List<string> issues = ValidateCloneAssets();
        if (issues.Count == 0)
        {
            Debug.Log("[KyUcSaiGon] Clone asset validation passed.");
            EditorUtility.DisplayDialog("Ky Uc Sai Gon Team Setup", "Clone asset validation passed.", "OK");
            return;
        }

        string message = string.Join("\n", issues);
        Debug.LogError("[KyUcSaiGon] Clone asset validation failed:\n" + message);
        EditorUtility.DisplayDialog("Ky Uc Sai Gon Team Setup", message, "OK");
    }

    private static void EnsureBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (string scenePath in GameplayScenes)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning("[KyUcSaiGon] Build scene missing on disk: " + scenePath);
                continue;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void ReimportFocusedAssets()
    {
        foreach (string assetPath in FocusedReimportAssets)
        {
            if (!File.Exists(assetPath))
            {
                continue;
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }
    }

    private static void ApplyBusHub()
    {
        BusHubFbxInteriorInstaller.ApplyFbxInteriorProps();
    }

    private static void ApplyNguyenHueArtSetup()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity", OpenSceneMode.Single);

        InvokePrivateStatic(typeof(NguyenHueFacadeRowInstaller), "EnsureAssetsAndPrefabs");
        InvokePrivateStatic(typeof(NguyenHueFacadeRowInstaller), "ApplyToOpenScene");

        InvokePrivateStatic(typeof(NguyenHueCityHallBackdropInstaller), "EnsureAssets");
        InvokePrivateStatic(typeof(NguyenHueCityHallBackdropInstaller), "ApplyToOpenScene");

        NguyenHueFountainModelInstaller.ApplyToOpenScene();

        InvokePrivateStatic(typeof(NguyenHueLedPanelInstaller), "EnsureMaterials");
        InvokePrivateStatic(typeof(NguyenHueLedPanelInstaller), "CreateOrUpdatePrefab");
        InvokePrivateStatic(typeof(NguyenHueLedPanelInstaller), "ApplyToOpenScene");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RestoreScene(string scenePath)
    {
        if (!string.IsNullOrWhiteSpace(scenePath) && File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
    }

    private static List<string> ValidateCloneAssets()
    {
        List<string> issues = new List<string>();
        foreach (string assetPath in RequiredLargeAssets)
        {
            if (!File.Exists(assetPath))
            {
                issues.Add("Missing: " + assetPath);
                continue;
            }

            FileInfo file = new FileInfo(assetPath);
            if (file.Length < 1024)
            {
                issues.Add("Too small, likely not pulled correctly: " + assetPath);
                continue;
            }

            if (LooksLikeGitLfsPointer(assetPath))
            {
                issues.Add("Git LFS pointer instead of real asset: " + assetPath);
            }
        }

        if (!ManifestContains("com.unity.cloud.gltfast"))
        {
            issues.Add("Missing package in Packages/manifest.json: com.unity.cloud.gltfast");
        }

        return issues;
    }

    private static bool LooksLikeGitLfsPointer(string assetPath)
    {
        byte[] bytes = File.ReadAllBytes(assetPath);
        int count = Mathf.Min(bytes.Length, 128);
        string header = System.Text.Encoding.UTF8.GetString(bytes, 0, count);
        return header.StartsWith("version https://git-lfs.github.com/spec/v1", StringComparison.Ordinal);
    }

    private static bool ManifestContains(string packageName)
    {
        const string manifestPath = "Packages/manifest.json";
        if (!File.Exists(manifestPath))
        {
            return false;
        }

        return File.ReadAllText(manifestPath).Contains("\"" + packageName + "\"");
    }

    private static void InvokePrivateStatic(Type type, string methodName)
    {
        MethodInfo method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null)
        {
            throw new MissingMethodException(type.Name, methodName);
        }

        method.Invoke(null, null);
    }
}
#endif
