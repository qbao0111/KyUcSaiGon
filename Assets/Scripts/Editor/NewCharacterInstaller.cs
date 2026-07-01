#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NewCharacterInstaller
{
    private const string WalkingModelPath = "Assets/Art/Models/Character_Animation_Walking_withSkin.glb.glb";
    private const string RunningModelPath = "Assets/Art/Models/Character_Animation_Running_withSkin.glb.glb";
    private const string ControllerPath = "Assets/Art/Animations/NewCharacter_Locomotion.controller";
    private const string WalkingClipPath = "Assets/Art/Animations/NewCharacter_Walking.anim";
    private const string RunningClipPath = "Assets/Art/Animations/NewCharacter_Running.anim";

    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity",
        "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
        "Assets/Scenes/Scene_07_Ending.unity"
    };

    [MenuItem("Ky Uc Sai Gon/Player/Apply New GLB Character To Active Scenes")]
    public static void ApplyToActiveScenes()
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(WalkingModelPath);
        AnimationClip sourceWalking = FindClip(WalkingModelPath, "walking");
        AnimationClip sourceRunning = FindClip(RunningModelPath, "running");
        if (model == null || sourceWalking == null || sourceRunning == null)
        {
            Debug.LogError("[NewCharacterInstaller] Missing model, walking clip, or running clip.");
            return;
        }

        Scene originalScene = SceneManager.GetActiveScene();
        string originalPath = originalScene.path;
        if (originalScene.isDirty)
        {
            EditorSceneManager.SaveScene(originalScene);
        }

        AnimationClip walking = ExtractClip(sourceWalking, WalkingClipPath, "NewCharacter_Walking");
        AnimationClip running = ExtractClip(sourceRunning, RunningClipPath, "NewCharacter_Running");
        AnimatorController controller = CreateController(walking, running);
        foreach (string scenePath in ScenePaths)
        {
            ApplyToScene(scenePath, model, controller);
        }

        if (!string.IsNullOrEmpty(originalPath))
        {
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[NewCharacterInstaller] Applied the new GLB character to 3 active gameplay scenes.");
    }

    [MenuItem("Ky Uc Sai Gon/Player/Repair Bus Hub Player")]
    public static void RepairBusHubPlayer()
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(WalkingModelPath);
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (model == null || controller == null)
        {
            Debug.LogError("[NewCharacterInstaller] Bus Hub repair requires the walking model and existing locomotion controller.");
            return;
        }

        string originalPath = SceneManager.GetActiveScene().path;
        ApplyToScene("Assets/Scenes/Scene_00_BusHub.unity", model, controller);
        if (!string.IsNullOrEmpty(originalPath))
        {
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[NewCharacterInstaller] Repaired Bus Hub player and removed missing prefab instances.");
    }

    private static AnimationClip FindClip(string path, string namePart)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            AnimationClip clip = asset as AnimationClip;
            if (clip != null && clip.name.ToLowerInvariant().Contains(namePart))
            {
                return clip;
            }
        }

        return null;
    }

    private static AnimationClip ExtractClip(AnimationClip source, string assetPath, string clipName)
    {
        AnimationClip extracted = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        if (extracted == null)
        {
            extracted = Object.Instantiate(source);
            extracted.name = clipName;
            AssetDatabase.CreateAsset(extracted, assetPath);
        }
        else
        {
            EditorUtility.CopySerialized(source, extracted);
            extracted.name = clipName;
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(source);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(extracted, settings);
        EditorUtility.SetDirty(extracted);
        return extracted;
    }

    private static AnimatorController CreateController(AnimationClip walking, AnimationClip running)
    {
        AnimatorController old = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (old != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

        // Freeze the first authored walking frame as a natural fallback idle pose.
        // This avoids the stiff bind pose until a dedicated idle clip is available.
        AnimatorState idleState = stateMachine.AddState("Idle (Frozen Walk Pose)");
        idleState.motion = walking;
        idleState.speed = 0f;
        idleState.writeDefaultValues = true;

        AnimatorState locomotionState = stateMachine.AddState("Locomotion");
        BlendTree tree = new BlendTree
        {
            name = "Speed Blend",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed",
            useAutomaticThresholds = false
        };
        AssetDatabase.AddObjectToAsset(tree, controller);
        tree.AddChild(walking, 0f);
        tree.AddChild(running, 0.8f);
        locomotionState.motion = tree;
        locomotionState.writeDefaultValues = true;

        AnimatorStateTransition startMoving = idleState.AddTransition(locomotionState);
        startMoving.hasExitTime = false;
        startMoving.duration = 0.12f;
        startMoving.AddCondition(AnimatorConditionMode.Greater, 0.04f, "Speed");

        AnimatorStateTransition stopMoving = locomotionState.AddTransition(idleState);
        stopMoving.hasExitTime = false;
        stopMoving.duration = 0.12f;
        stopMoving.AddCondition(AnimatorConditionMode.Less, 0.025f, "Speed");

        stateMachine.defaultState = idleState;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void ApplyToScene(string scenePath, GameObject model, RuntimeAnimatorController controller)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        RemoveMissingPrefabInstances(scene);
        GameObject player = GameObject.Find("REPLACE_Player_Character");
        if (player == null)
        {
            Debug.LogWarning("[NewCharacterInstaller] Player not found in " + scenePath);
            return;
        }

        Transform aoDai = player.transform.Find("Visual_Player_AoDai");
        if (aoDai != null)
        {
            Object.DestroyImmediate(aoDai.gameObject);
        }

        Transform backup = player.transform.Find("OldVisual_Backup_Disabled");
        if (backup != null)
        {
            Object.DestroyImmediate(backup.gameObject);
        }

        Transform existing = player.transform.Find("Visual_Player_NewCharacter");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        Transform legacy = player.transform.Find("Visual_REPLACE_Player_P09_Humandroid");
        if (legacy != null)
        {
            Object.DestroyImmediate(legacy.gameObject);
        }

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
        visual.name = "Visual_Player_NewCharacter";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = new Vector3(0f, -0.72f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * 1.2f;

        foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        Animator animator = visual.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        PlayerMovementAnimator movementAnimator = player.GetComponent<PlayerMovementAnimator>();
        if (movementAnimator == null)
        {
            movementAnimator = player.AddComponent<PlayerMovementAnimator>();
        }
        movementAnimator.visualRoot = visual.transform;
        movementAnimator.animator = animator;
        movementAnimator.targetVisualScale = 1.2f;
        movementAnimator.bobHeight = 0.015f;
        movementAnimator.leanAngle = 1.5f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RemoveMissingPrefabInstances(Scene scene)
    {
        HashSet<GameObject> missingRoots = new HashSet<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
            {
                if (PrefabUtility.GetPrefabInstanceStatus(item.gameObject) != PrefabInstanceStatus.MissingAsset)
                {
                    continue;
                }

                GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(item.gameObject);
                missingRoots.Add(instanceRoot != null ? instanceRoot : item.gameObject);
            }
        }

        foreach (GameObject missingRoot in missingRoots)
        {
            if (missingRoot != null)
            {
                Object.DestroyImmediate(missingRoot);
            }
        }
    }
}
#endif
