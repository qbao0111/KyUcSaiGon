using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class SkyboxValuePrinter
{
    [MenuItem("KyUcSaiGon/Print Skybox Values")]
    public static void PrintValues()
    {
        string[] scenes = new string[]
        {
            "Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity",
            "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
            "Assets/Scenes/Scene_07_Ending.unity"
        };

        foreach (string scenePath in scenes)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning("Scene file does not exist: " + scenePath);
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                Debug.LogError("Failed to open scene: " + scenePath);
                continue;
            }

            GameObject skybox = GameObject.Find("REPLACE_Skybox");
            if (skybox == null)
            {
                skybox = GameObject.Find("Skybox");
            }

            if (skybox == null)
            {
                Debug.LogWarning($"[SKYBOX INFO] {scenePath}: No skybox object found!");
                continue;
            }

            string parentName = skybox.transform.parent != null ? skybox.transform.parent.name : "ROOT";
            Debug.Log($"[SKYBOX INFO] {scenePath}:");
            Debug.Log($"  - Name: {skybox.name}");
            Debug.Log($"  - Parent: {parentName}");
            Debug.Log($"  - Local Position: {skybox.transform.localPosition}");
            Debug.Log($"  - Local Rotation: {skybox.transform.localRotation.eulerAngles}");
            Debug.Log($"  - Local Scale: {skybox.transform.localScale}");
            Debug.Log($"  - Lossy Scale: {skybox.transform.lossyScale}");

            PrintChildrenScaleRecursive(skybox.transform, "    ");

            // Print renderers and materials
            Renderer[] renderers = skybox.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"  - Renderers Count: {renderers.Length}");
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                string materialName = r.sharedMaterial != null ? r.sharedMaterial.name : "null";
                string shaderName = r.sharedMaterial != null && r.sharedMaterial.shader != null ? r.sharedMaterial.shader.name : "null";
                Debug.Log($"    - Renderer {i}: {r.name}, Active: {r.gameObject.activeInHierarchy}, Material: {materialName}, Shader: {shaderName}");
            }

            // Print Main Camera settings
            Camera cam = Camera.main;
            if (cam != null)
            {
                Debug.Log($"  - Main Camera: FarClip={cam.farClipPlane}, NearClip={cam.nearClipPlane}, Position={cam.transform.position}");
            }
            else
            {
                Debug.Log($"  - Main Camera: null");
            }
        }
    }

    private static void PrintChildrenScaleRecursive(Transform t, string indent)
    {
        foreach (Transform child in t)
        {
            Debug.Log($"{indent}- Child Name: {child.name}, LocalPosition: {child.localPosition}, LocalScale: {child.localScale}, LossyScale: {child.lossyScale}");
            PrintChildrenScaleRecursive(child, indent + "  ");
        }
    }
}
