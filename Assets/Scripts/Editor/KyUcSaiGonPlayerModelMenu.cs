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
}
#endif
