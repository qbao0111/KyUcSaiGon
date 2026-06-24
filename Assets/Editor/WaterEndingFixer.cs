using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Linq;

public class WaterEndingFixer
{
    [MenuItem("KyUcSaiGon/Restore and Fix Ending Scene")]
    public static void FixWaterEnding()
    {
        FixWaterEnding(true);
    }

    public static void FixWaterEnding(bool openScene)
    {
        Debug.Log("Starting Ending Scene Restoration and Fix...");
        
        UnityEngine.SceneManagement.Scene scene = default;
        if (openScene)
        {
            string scenePath = "Assets/Scenes/Scene_07_Ending.unity";
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("Failed to open scene: " + scenePath);
                return;
            }
        }
        else
        {
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        // 1. Recreate or find WaterPlane
        GameObject waterPlane = GameObject.Find("WaterPlane");
        if (waterPlane == null)
        {
            Debug.Log("WaterPlane not found, creating a new one.");
            waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "WaterPlane";
            
            // Parent to SceneBlockoutRoot
            GameObject root = GameObject.Find("SceneBlockoutRoot");
            if (root != null)
            {
                waterPlane.transform.SetParent(root.transform);
            }
        }

        // Configure WaterPlane transform
        waterPlane.transform.localPosition = new Vector3(0f, 0.08f, 18f);
        waterPlane.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        waterPlane.transform.localScale = new Vector3(9.6f, 1f, 3.8f); // Cover the 96x38 river area

        // Set Material
        string matPath = "Assets/E_Water/Models/Materials/Water.mat";
        Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (waterMat != null)
        {
            waterPlane.GetComponent<Renderer>().sharedMaterial = waterMat;
        }
        else
        {
            Debug.LogError("Water Material not found at " + matPath);
        }

        // Find or add WaterAnim2 component
        WaterAnim2 waterAnim = waterPlane.GetComponent<WaterAnim2>();
        if (waterAnim == null)
        {
            Debug.Log("WaterAnim2 component not found, adding it.");
            waterAnim = waterPlane.AddComponent<WaterAnim2>();
        }

        // Remove/disable Animation and Animator components on WaterPlane
        var anim = waterPlane.GetComponent<Animation>();
        if (anim != null)
        {
            Debug.Log("Removing Animation component from WaterPlane.");
            Object.DestroyImmediate(anim);
        }

        var animator = waterPlane.GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("Removing Animator component from WaterPlane.");
            Object.DestroyImmediate(animator);
        }

        // Load normal textures sorted alphabetically
        string normalFolder = "Assets/E_Water/Textures/waterA_nor";
        string[] normalGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { normalFolder });
        var normalTextures = normalGuids
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Distinct()
            .Select(path => AssetDatabase.LoadAssetAtPath<Texture2D>(path))
            .Where(t => t != null && t.name.StartsWith("frame_"))
            .OrderBy(t => t.name)
            .ToArray();

        Debug.Log($"Loaded {normalTextures.Length} normal textures from {normalFolder}");

        // Assign textures
        waterAnim.Textures = new Texture2D[0];
        waterAnim.NormalTextures = normalTextures;
        waterAnim.NormalMapOn = true;
        waterAnim.fps = 15;

        // 2. Restore Landmark 81 model
        GameObject landmarkRoot = GameObject.Find("LandmarkRoot");
        if (landmarkRoot != null)
        {
            // Clear any existing children (like old cube placeholders)
            var children = landmarkRoot.transform.Cast<Transform>().Select(t => t.gameObject).ToList();
            foreach (var child in children)
            {
                Object.DestroyImmediate(child);
            }

            // Load and instantiate landmark.glb
            string modelPath = "Assets/Art/Models/Ending/landmark.glb";
            GameObject landmarkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (landmarkPrefab != null)
            {
                GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(landmarkPrefab, landmarkRoot.transform);
                inst.name = "REPLACE_Ending_Landmark81_Tower";
                inst.transform.localPosition = new Vector3(0f, 0f, 34f);
                inst.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                inst.transform.localScale = Vector3.one; // User can adjust if scale needs tuning
                Debug.Log("Successfully instantiated landmark.glb under LandmarkRoot.");
            }
            else
            {
                Debug.LogError("landmark.glb not found at " + modelPath);
            }
        }
        else
        {
            Debug.LogError("LandmarkRoot not found in scene.");
        }

        // Update EndingSceneController references
        GameObject sceneRoot = GameObject.Find("SceneBlockoutRoot");
        if (sceneRoot != null)
        {
            EndingSceneController controller = sceneRoot.GetComponent<EndingSceneController>();
            if (controller != null)
            {
                controller.landmarkTower = GameObject.Find("REPLACE_Ending_Landmark81_Tower");
                controller.returnTrigger = GameObject.Find("REPLACE_Ending_ReturnToHubTrigger");
                controller.finalLightObject = GameObject.Find("REPLACE_Ending_SunsetDisc");
                controller.renderersToWarm = sceneRoot.GetComponentsInChildren<Renderer>(true);
                EditorUtility.SetDirty(controller);
            }
        }

        // Mark dirty and save
        EditorUtility.SetDirty(waterAnim);
        EditorUtility.SetDirty(waterPlane);
        EditorSceneManager.MarkSceneDirty(scene);
        
        bool saveSuccess = EditorSceneManager.SaveScene(scene);
        Debug.Log("Save Scene Success: " + saveSuccess);

        AssetDatabase.SaveAssets();
        Debug.Log("Ending Scene Restoration and Fix Complete!");
    }
}
