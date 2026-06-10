#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KyUcSaiGonPlayerModelMenu
{
    private const string PlayerPrefabPath = "Assets/P09_Modular_Humanoid/Model_DATA/Prefab/P09_Human.prefab";
    private const string PlayerControllerPath = "Assets/P09_Modular_Humanoid/KyUcSaiGon_P09_Player.controller";
    private const string IdleClipPath = "Assets/P09_Modular_Humanoid/Scenes/DemoScene_Data/Animation/Demo_Pose/P09_Male_idle.anim";
    private const string RunClipPath = "Assets/P09_Modular_Humanoid/Scenes/DemoScene_Data/Animation/Other/Run_A_v01.anim";
    private const string P09MaterialSearchRoot = "Assets/P09_Modular_Humanoid/Model_DATA/Materials";
    private const string AoDaiModelPath = "Assets/Art/Models/Common/Characters/AoDai/AoDai.fbx";
    private const string AoDaiIdlePath = "Assets/Art/Animations/Characters/AoDai/AoDai_Idle.fbx";
    private const string AoDaiWalkPath = "Assets/Art/Animations/Characters/AoDai/AoDai_Walk.fbx";
    private const string AoDaiJogPath = "Assets/Art/Animations/Characters/AoDai/AoDai_Jog.fbx";
    private const string AoDaiControllerPath = "Assets/Art/Animations/Characters/AoDai/AC_Player_AoDai.controller";

    [MenuItem("Ky Uc Sai Gon/Apply P09 Player To Open Scene")]
    public static void ApplyToOpenScene()
    {
        ResetP09MaterialTints();
        ApplyToScene(SceneManager.GetActiveScene().path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] P09 player applied to open scene.");
    }

    [MenuItem("Ky Uc Sai Gon/Apply P09 Player To All Scenes")]
    public static void ApplyToAllScenes()
    {
        ResetP09MaterialTints();
        string[] scenePaths =
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

        foreach (string scenePath in scenePaths)
        {
            ApplyToScene(scenePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] P09 player applied to all gameplay scenes.");
    }

    [MenuItem("Ky Uc Sai Gon/Apply AoDai Player To Open Scene")]
    public static void ApplyAoDaiToOpenScene()
    {
        ConfigureAoDaiImports();
        AnimatorController controller = CreateOrLoadAoDaiController();
        ApplyAoDaiToScene(SceneManager.GetActiveScene().path, controller);
        AssetDatabase.SaveAssets();
        Debug.Log("[KyUcSaiGon] AoDai player applied to open scene.");
    }

    [MenuItem("Ky Uc Sai Gon/Apply AoDai Player To All Scenes")]
    public static void ApplyAoDaiToAllScenes()
    {
        ConfigureAoDaiImports();
        AnimatorController controller = CreateOrLoadAoDaiController();
        string[] scenePaths =
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

        foreach (string scenePath in scenePaths)
        {
            ApplyAoDaiToScene(scenePath, controller);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[KyUcSaiGon] AoDai player applied to all gameplay scenes.");
    }

    [MenuItem("Ky Uc Sai Gon/Reset P09 Original Materials")]
    public static void ResetP09MaterialsOnly()
    {
        ResetP09MaterialTints();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] P09 materials converted to URP/Lit with original texture maps.");
    }

    private static void ResetP09MaterialTints()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { P09MaterialSearchRoot });
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                continue;
            }

            ConvertMaterialToUrpLit(material);
            EditorUtility.SetDirty(material);
        }
    }

    private static void ConvertMaterialToUrpLit(Material material)
    {
        Texture baseTexture = GetFirstTexture(material, "_BaseMap", "_BaseColorMap", "_MainTex");
        Texture normalTexture = GetFirstTexture(material, "_BumpMap", "_NormalMap");
        Texture metallicTexture = GetFirstTexture(material, "_MetallicGlossMap", "_MaskMap");

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit != null && material.shader != urpLit)
        {
            material.shader = urpLit;
        }
        else if (urpLit == null)
        {
            Shader standard = Shader.Find("Standard");
            if (standard != null && material.shader != standard)
            {
                material.shader = standard;
            }
        }

        SetTextureIfPossible(material, "_BaseMap", baseTexture);
        SetTextureIfPossible(material, "_BaseColorMap", baseTexture);
        SetTextureIfPossible(material, "_MainTex", baseTexture);
        SetTextureIfPossible(material, "_BumpMap", normalTexture);
        SetTextureIfPossible(material, "_NormalMap", normalTexture);
        SetTextureIfPossible(material, "_MetallicGlossMap", metallicTexture);
        SetTextureIfPossible(material, "_MaskMap", metallicTexture);

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", Color.black);
        }

        if (material.HasProperty("_EmissionStrength"))
        {
            material.SetFloat("_EmissionStrength", 0f);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.35f);
        }

        if (normalTexture != null)
        {
            material.EnableKeyword("_NORMALMAP");
        }
        else
        {
            material.DisableKeyword("_NORMALMAP");
        }

        material.DisableKeyword("_EMISSION");
    }

    private static Texture GetFirstTexture(Material material, params string[] propertyNames)
    {
        foreach (string propertyName in propertyNames)
        {
            if (material.HasProperty(propertyName))
            {
                Texture texture = material.GetTexture(propertyName);
                if (texture != null)
                {
                    return texture;
                }
            }
        }

        return null;
    }

    private static void SetTextureIfPossible(Material material, string propertyName, Texture texture)
    {
        if (texture != null && material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

    private static void ApplyToScene(string scenePath)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject player = GameObject.Find("REPLACE_Player_Character");
        if (player == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Player not found in " + scenePath);
            return;
        }

        RemoveRootPlaceholderMesh(player);
        ConfigurePlayerCollider(player);
        GameObject visual = EnsureP09Visual(player.transform);
        PlayerMovementAnimator movementAnimator = player.GetComponent<PlayerMovementAnimator>();
        if (movementAnimator == null)
        {
            movementAnimator = player.AddComponent<PlayerMovementAnimator>();
        }

        movementAnimator.visualRoot = visual.transform;
        movementAnimator.animator = visual.GetComponentInChildren<Animator>();
        movementAnimator.targetVisualScale = 1.45f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ApplyAoDaiToScene(string scenePath, RuntimeAnimatorController controller)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return;
        }

        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AoDaiModelPath);
        if (modelPrefab == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing AoDai model: " + AoDaiModelPath);
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject player = GameObject.Find("REPLACE_Player_Character");
        if (player == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Player not found in " + scenePath);
            return;
        }

        RemoveRootPlaceholderMesh(player);
        ConfigurePlayerCollider(player);
        Transform oldVisual = player.transform.Find("Visual_REPLACE_Player_P09_Humandroid");
        Vector3 localPosition = oldVisual != null ? oldVisual.localPosition : Vector3.zero;
        Quaternion localRotation = oldVisual != null ? oldVisual.localRotation : Quaternion.identity;
        Vector3 localScale = oldVisual != null ? oldVisual.localScale : Vector3.one;

        BackupP09Visual(player.transform, oldVisual);
        RemoveExistingAoDaiVisual(player.transform);

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        visual.name = "Visual_Player_AoDai";
        visual.transform.SetParent(player.transform, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = localRotation;
        visual.transform.localScale = localScale;
        RemoveChildColliders(visual);

        Animator animator = visual.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.avatar = LoadAoDaiAvatar();
        animator.applyRootMotion = false;

        PlayerMovementAnimator movementAnimator = player.GetComponent<PlayerMovementAnimator>();
        if (movementAnimator == null)
        {
            movementAnimator = player.AddComponent<PlayerMovementAnimator>();
        }

        movementAnimator.visualRoot = visual.transform;
        movementAnimator.animator = animator;
        movementAnimator.targetVisualScale = Mathf.Max(localScale.x, 1.45f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RemoveRootPlaceholderMesh(GameObject player)
    {
        MeshRenderer renderer = player.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Object.DestroyImmediate(renderer);
        }

        MeshFilter filter = player.GetComponent<MeshFilter>();
        if (filter != null)
        {
            Object.DestroyImmediate(filter);
        }
    }

    private static GameObject EnsureP09Visual(Transform playerRoot)
    {
        // Recreate the visual from the prefab each time so earlier test material
        // overrides never stick to the player.
        RemoveOldVisuals(playerRoot);

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        GameObject visual;
        if (prefab != null)
        {
            visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            visual.name = "Visual_REPLACE_Player_P09_Humandroid";
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual_REPLACE_Player_CapsuleFallback";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            Debug.LogWarning("[KyUcSaiGon] Missing P09 prefab. Fallback capsule was used.");
        }

        visual.transform.SetParent(playerRoot);
        visual.transform.localPosition = new Vector3(0f, -0.9f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * 1.45f;
        ConfigureVisual(visual);
        return visual;
    }

    private static void ConfigurePlayerCollider(GameObject player)
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller == null)
        {
            return;
        }

        controller.height = 2.2f;
        controller.radius = 0.45f;
        controller.center = new Vector3(0f, 0.2f, 0f);
    }

    private static void RemoveOldVisuals(Transform playerRoot)
    {
        for (int i = playerRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = playerRoot.GetChild(i);
            if (child.name.StartsWith("Visual_REPLACE_Player"))
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void BackupP09Visual(Transform playerRoot, Transform oldVisual)
    {
        Transform backup = playerRoot.Find("OldVisual_Backup_Disabled");
        if (backup == null)
        {
            GameObject backupObject = new GameObject("OldVisual_Backup_Disabled");
            backupObject.transform.SetParent(playerRoot, false);
            backup = backupObject.transform;
        }

        if (oldVisual != null && oldVisual.parent != backup)
        {
            oldVisual.SetParent(backup, true);
        }

        backup.gameObject.SetActive(false);
    }

    private static void RemoveExistingAoDaiVisual(Transform playerRoot)
    {
        Transform existing = playerRoot.Find("Visual_Player_AoDai");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
    }

    private static void RemoveChildColliders(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void ConfigureVisual(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }

        Animator animator = visual.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = visual.AddComponent<Animator>();
        }

        RuntimeAnimatorController controller = CreateOrLoadController();
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
        }
    }

    private static RuntimeAnimatorController CreateOrLoadController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerControllerPath);
        if (controller != null)
        {
            return controller;
        }

        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClipPath);
        AnimationClip runClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(RunClipPath);
        if (idleClip == null || runClip == null)
        {
            Debug.LogWarning("[KyUcSaiGon] P09 idle/run clips not found. Procedural movement animation still works.");
            return null;
        }

        controller = AnimatorController.CreateAnimatorControllerAtPath(PlayerControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idle = stateMachine.AddState("Idle");
        idle.motion = idleClip;
        AnimatorState run = stateMachine.AddState("Run");
        run.motion = runClip;
        stateMachine.defaultState = idle;

        AnimatorStateTransition toRun = idle.AddTransition(run);
        toRun.hasExitTime = false;
        toRun.duration = 0.18f;
        toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        AnimatorStateTransition toIdle = run.AddTransition(idle);
        toIdle.hasExitTime = false;
        toIdle.duration = 0.18f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.08f, "Speed");

        return controller;
    }

    private static void ConfigureAoDaiImports()
    {
        ConfigureAoDaiModelImport();
        Avatar avatar = LoadAoDaiAvatar();
        ConfigureAoDaiAnimationImport(AoDaiIdlePath, "ANIM_AoDai_Idle", avatar);
        ConfigureAoDaiAnimationImport(AoDaiWalkPath, "ANIM_AoDai_Walk", avatar);
        ConfigureAoDaiAnimationImport(AoDaiJogPath, "ANIM_AoDai_Jog", avatar);
    }

    private static void ConfigureAoDaiModelImport()
    {
        ModelImporter importer = AssetImporter.GetAtPath(AoDaiModelPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[KyUcSaiGon] AoDai model importer not found: " + AoDaiModelPath);
            return;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        importer.SaveAndReimport();
    }

    private static void ConfigureAoDaiAnimationImport(string path, string clipName, Avatar sourceAvatar)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[KyUcSaiGon] AoDai animation importer not found: " + path);
            return;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
        importer.sourceAvatar = sourceAvatar;
        importer.importAnimation = true;

        ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
        if (clips.Length == 0)
        {
            clips = importer.clipAnimations;
        }

        if (clips.Length > 0)
        {
            clips[0].name = clipName;
            clips[0].loopTime = true;
            clips[0].wrapMode = WrapMode.Loop;
            importer.clipAnimations = new[] { clips[0] };
        }

        importer.SaveAndReimport();
    }

    private static AnimatorController CreateOrLoadAoDaiController()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AoDaiControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(AoDaiControllerPath);
        }

        EnsureAnimatorFloat(controller, "Speed");

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idle = EnsureState(stateMachine, "Idle", LoadClip(AoDaiIdlePath, "ANIM_AoDai_Idle"));
        AnimatorState walk = EnsureState(stateMachine, "Walk", LoadClip(AoDaiWalkPath, "ANIM_AoDai_Walk"));
        AnimatorState jog = EnsureState(stateMachine, "Jog", LoadClip(AoDaiJogPath, "ANIM_AoDai_Jog"));
        stateMachine.defaultState = idle;

        ClearTransitions(idle);
        ClearTransitions(walk);
        ClearTransitions(jog);
        AddTransition(idle, walk, AnimatorConditionMode.Greater, 0.1f, 0.1f);
        AddTransition(walk, idle, AnimatorConditionMode.Less, 0.1f, 0.1f);
        AddTransition(walk, jog, AnimatorConditionMode.Greater, 1.8f, 0.15f);
        AddTransition(jog, walk, AnimatorConditionMode.Less, 1.8f, 0.15f);
        AddTransition(jog, idle, AnimatorConditionMode.Less, 0.1f, 0.15f);

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static Avatar LoadAoDaiAvatar()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(AoDaiModelPath);
        foreach (Object asset in assets)
        {
            if (asset is Avatar avatar)
            {
                return avatar;
            }
        }

        return null;
    }

    private static AnimationClip LoadClip(string path, string preferredName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && clip.name == preferredName)
            {
                return clip;
            }
        }

        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview", System.StringComparison.OrdinalIgnoreCase))
            {
                return clip;
            }
        }

        Debug.LogWarning("[KyUcSaiGon] Animation clip not found: " + path);
        return null;
    }

    private static void EnsureAnimatorFloat(AnimatorController controller, string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name == parameterName)
            {
                return;
            }
        }

        controller.AddParameter(parameterName, AnimatorControllerParameterType.Float);
    }

    private static AnimatorState EnsureState(AnimatorStateMachine stateMachine, string stateName, Motion motion)
    {
        foreach (ChildAnimatorState child in stateMachine.states)
        {
            if (child.state.name == stateName)
            {
                child.state.motion = motion;
                return child.state;
            }
        }

        AnimatorState state = stateMachine.AddState(stateName);
        state.motion = motion;
        return state;
    }

    private static void ClearTransitions(AnimatorState state)
    {
        foreach (AnimatorStateTransition transition in state.transitions)
        {
            state.RemoveTransition(transition);
        }
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, float threshold, float duration)
    {
        AnimatorStateTransition transition = from.AddTransition(to);
        transition.hasExitTime = false;
        transition.duration = duration;
        transition.AddCondition(mode, threshold, "Speed");
    }
}
#endif
