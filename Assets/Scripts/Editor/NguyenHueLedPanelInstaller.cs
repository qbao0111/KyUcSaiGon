#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class NguyenHueLedPanelInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity";
    private const string ModelPath = "Assets/Art/Models/NguyenHue/LEDPanels/nguyen_hue_led_hint_panel.fbx";
    private const string PrefabFolderPath = "Assets/Art/Prefabs/NguyenHue";
    private const string PrefabPath = "Assets/Art/Prefabs/NguyenHue/PF_NguyenHue_LEDHintPanel.prefab";
    private const string MaterialFolderPath = "Assets/Art/Materials/NguyenHue";

    private const string FrameMaterialPath = "Assets/Art/Materials/NguyenHue/M_LEDPanel_Frame_Dark.mat";
    private const string GreenScreenMaterialPath = "Assets/Art/Materials/NguyenHue/M_LEDPanel_Screen_Green.mat";
    private const string YellowScreenMaterialPath = "Assets/Art/Materials/NguyenHue/M_LEDPanel_Screen_Yellow.mat";
    private const string RedScreenMaterialPath = "Assets/Art/Materials/NguyenHue/M_LEDPanel_Screen_Red.mat";

    private const string ScreenColorPatchName = "ScreenColorPatch";
    private const float ModelCompensationScale = 300f;

    [MenuItem("Ky Uc Sai Gon/Apply Nguyen Hue LED Hint Panel Model")]
    public static void ApplyNguyenHueLedPanels()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Nguyen Hue LED panel install cancelled.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        bool shouldRestoreOpenScene = currentScene.IsValid()
                                      && !string.IsNullOrWhiteSpace(currentScene.path)
                                      && currentScene.path != ScenePath;

        EnsureMaterials();
        CreateOrUpdatePrefab();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        ApplyToOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (shouldRestoreOpenScene)
        {
            EditorSceneManager.OpenScene(currentScene.path, OpenSceneMode.Single);
        }

        Debug.Log("[KyUcSaiGon] Nguyen Hue LED hint panels installed.");
    }

    [MenuItem("Ky Uc Sai Gon/Create Nguyen Hue LED Hint Panel Prefab Only")]
    public static void CreatePrefabOnly()
    {
        EnsureMaterials();
        CreateOrUpdatePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[KyUcSaiGon] PF_NguyenHue_LEDHintPanel prefab created/updated.");
    }

    private static void ApplyToOpenScene()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing prefab at " + PrefabPath);
            return;
        }

        ApplyPanel(
            prefab,
            "REPLACE_LEDHint_Bass_Red_1",
            "Visual_REPLACE_LED_HintPanel_01",
            RedScreenMaterialPath,
            "BASS",
            "1",
            new Color(1f, 0.08f, 0.04f),
            "LED đỏ nhấp nháy số 1. Đây là Bass.");

        ApplyPanel(
            prefab,
            "REPLACE_LEDHint_Mid_Green_6",
            "Visual_REPLACE_LED_HintPanel_02",
            GreenScreenMaterialPath,
            "MID",
            "6",
            new Color(0.08f, 1f, 0.28f),
            "LED xanh lá nhấp nháy số 6. Đây là Mid.");

        ApplyPanel(
            prefab,
            "REPLACE_LEDHint_Treble_Gold_8",
            "Visual_REPLACE_LED_HintPanel_03",
            YellowScreenMaterialPath,
            "TREBLE",
            "8",
            new Color(1f, 0.78f, 0.08f),
            "LED vàng nhấp nháy số 8. Đây là Treble.");

        UpdateNguyenHuePuzzleAnswer("1-6-8");
    }

    private static void ApplyPanel(GameObject prefab, string rootName, string visualName, string screenMaterialPath, string labelText, string clueNumber, Color clueColor, string hintMessage)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing LED hint root: " + rootName);
            return;
        }

        Transform oldVisual = root.transform.Find(visualName);
        Vector3 rootPosition = root.transform.localPosition;
        root.transform.localPosition = new Vector3(rootPosition.x, 0f, rootPosition.z);
        FaceTowardPlayerPath(root.transform);
        ConfigureInteractionCollider(root);

        if (oldVisual != null)
        {
            Object.DestroyImmediate(oldVisual.gameObject);
        }

        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        visual.name = visualName;
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        Material screenMaterial = AssetDatabase.LoadAssetAtPath<Material>(screenMaterialPath);
        DisableScreenColorPatch(visual);
        ApplyLabelColor(root.transform, screenMaterial);
        RemoveCollidersFromVisual(visual);
        UpdateHintMessage(root, hintMessage);
        UpdateTextLabel(root.transform, labelText);
        ConfigureHintDisplay(root, labelText, clueNumber, clueColor);
        EditorUtility.SetDirty(root);
    }

    private static void FaceTowardPlayerPath(Transform root)
    {
        Vector3 target = new Vector3(0f, root.position.y, -8f);
        Vector3 direction = target - root.position;
        if (direction.sqrMagnitude > 0.01f)
        {
            root.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }

    private static void ConfigureInteractionCollider(GameObject root)
    {
        BoxCollider collider = root.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = root.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;
        collider.size = new Vector3(2.2f, 3f, 1.4f);
        collider.center = new Vector3(0f, 1.35f, 0f);
        EditorUtility.SetDirty(collider);
    }

    private static void UpdateNguyenHuePuzzleAnswer(string answer)
    {
        PuzzleInteractable[] puzzles = Object.FindObjectsByType<PuzzleInteractable>(FindObjectsSortMode.None);
        foreach (PuzzleInteractable puzzle in puzzles)
        {
            if (puzzle.puzzleTitle.Contains("Loa") || puzzle.inputHint.Contains("Bass"))
            {
                puzzle.correctAnswer = answer;
                EditorUtility.SetDirty(puzzle);
            }
        }
    }

    private static void UpdateHintMessage(GameObject root, string hintMessage)
    {
        LEDHintInteractable hint = root.GetComponent<LEDHintInteractable>();
        if (hint != null)
        {
            hint.hintMessage = hintMessage;
            EditorUtility.SetDirty(hint);
        }
    }

    private static void UpdateTextLabel(Transform root, string labelText)
    {
        TextMeshPro[] labels = root.GetComponentsInChildren<TextMeshPro>(true);
        foreach (TextMeshPro label in labels)
        {
            if (label.name.StartsWith("TMP_Label_") || label.text.Contains("=") || label.text.Contains(":"))
            {
                label.text = labelText;
                label.fontSize = 4.2f;
                label.alignment = TextAlignmentOptions.Center;
                label.rectTransform.sizeDelta = new Vector2(7f, 3f);
                label.rectTransform.localPosition = new Vector3(0f, 2.35f, -0.25f);
                label.rectTransform.localRotation = Quaternion.identity;
                label.rectTransform.localScale = Vector3.one * 0.16f;
                EditorUtility.SetDirty(label);
                return;
            }
        }
    }

    private static void ConfigureHintDisplay(GameObject root, string labelText, string clueNumber, Color clueColor)
    {
        LEDHintInteractable hint = root.GetComponent<LEDHintInteractable>();
        if (hint == null)
        {
            hint = root.AddComponent<LEDHintInteractable>();
        }

        hint.ConfigureDisplay(labelText, clueNumber, clueColor);
        EditorUtility.SetDirty(hint);
    }

    private static void EnsureMaterials()
    {
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "NguyenHue");
        EnsureFolder("Assets/Art", "Prefabs");
        EnsureFolder("Assets/Art/Prefabs", "NguyenHue");

        CreateOrUpdateLitMaterial(FrameMaterialPath, new Color(0.035f, 0.04f, 0.045f), Color.black, 0f);
        CreateOrUpdateLitMaterial(GreenScreenMaterialPath, new Color(0.05f, 0.9f, 0.25f), new Color(0.05f, 1f, 0.25f), 1.2f);
        CreateOrUpdateLitMaterial(YellowScreenMaterialPath, new Color(1f, 0.78f, 0.08f), new Color(1f, 0.82f, 0.16f), 1.2f);
        CreateOrUpdateLitMaterial(RedScreenMaterialPath, new Color(0.95f, 0.08f, 0.05f), new Color(1f, 0.12f, 0.08f), 1.2f);
    }

    private static void CreateOrUpdatePrefab()
    {
        GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (modelPrefab == null)
        {
            Debug.LogError("[KyUcSaiGon] Missing LED panel FBX at " + ModelPath);
            return;
        }

        GameObject prefabRoot = new GameObject("PF_NguyenHue_LEDHintPanel");
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
        model.name = "Visual_REPLACE_LEDHintPanel_Model";
        model.transform.SetParent(prefabRoot.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one * ModelCompensationScale;

        RemoveCollidersFromVisual(model);
        AssignFrameMaterial(model);
        CreateScreenColorPatch(prefabRoot.transform);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        Object.DestroyImmediate(prefabRoot);
    }

    private static void CreateScreenColorPatch(Transform parent)
    {
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = ScreenColorPatchName;
        screen.transform.SetParent(parent);
        screen.transform.localPosition = new Vector3(0f, 0.55f, -0.04f);
        screen.transform.localRotation = Quaternion.identity;
        screen.transform.localScale = new Vector3(0.62f, 0.72f, 0.02f);
        screen.SetActive(false);

        Collider collider = screen.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        MeshRenderer renderer = screen.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(GreenScreenMaterialPath);
    }

    private static void AssignFrameMaterial(GameObject model)
    {
        Material frameMaterial = AssetDatabase.LoadAssetAtPath<Material>(FrameMaterialPath);
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = frameMaterial;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static void DisableScreenColorPatch(GameObject visualRoot)
    {
        Transform brokenOverlay = visualRoot.transform.Find("Visual_REPLACE_LEDHintPanel_ScreenOverlay");
        if (brokenOverlay != null)
        {
            Object.DestroyImmediate(brokenOverlay.gameObject);
        }

        Transform screen = visualRoot.transform.Find(ScreenColorPatchName);
        if (screen != null)
        {
            screen.gameObject.SetActive(false);
        }
    }

    private static void ApplyLabelColor(Transform root, Material screenMaterial)
    {
        if (screenMaterial == null)
        {
            return;
        }

        Color labelColor = Color.white;
        if (screenMaterial.HasProperty("_BaseColor"))
        {
            labelColor = screenMaterial.GetColor("_BaseColor");
        }

        TextMeshPro[] labels = root.GetComponentsInChildren<TextMeshPro>(true);
        foreach (TextMeshPro label in labels)
        {
            if (label.name.StartsWith("TMP_Label_") || label.text.Contains("=") || label.text.Contains(":"))
            {
                label.color = labelColor;
                EditorUtility.SetDirty(label);
                return;
            }
        }
    }

    private static void RemoveCollidersFromVisual(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }
    }

    private static void CreateOrUpdateLitMaterial(string path, Color baseColor, Color emissionColor, float emissionStrength)
    {
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

        SetColor(material, "_BaseColor", baseColor);
        SetColor(material, "_Color", baseColor);
        SetColor(material, "_EmissionColor", emissionColor * emissionStrength);
        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.55f);

        if (emissionStrength > 0f)
        {
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");
        }

        EditorUtility.SetDirty(material);
    }

    private static void EnsureFolder(string parentPath, string folderName)
    {
        string folderPath = parentPath + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }

    private static void SetColor(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
#endif
