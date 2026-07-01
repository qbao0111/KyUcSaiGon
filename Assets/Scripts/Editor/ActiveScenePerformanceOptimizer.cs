#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ActiveScenePerformanceOptimizer
{
    private static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Scene_04_NhaThoDucBa.unity",
        "Assets/Scenes/Scene_07_Ending.unity"
    };

    [MenuItem("Ky Uc Sai Gon/Performance/Optimize Scene 04 And 07")]
    public static void Optimize()
    {
        string originalPath = SceneManager.GetActiveScene().path;
        foreach (string path in ScenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            OptimizeCurrentScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalPath))
        {
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[PerformanceOptimizer] Optimized Scene 04 and Scene 07.");
    }

    [InitializeOnLoadMethod]
    private static void FinishEndingSceneAfterDomainReload()
    {
        EditorApplication.delayCall += () =>
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.name != "Scene_07_Ending" || HasOptimizationFlags())
            {
                return;
            }

            OptimizeCurrentScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[PerformanceOptimizer] Finished Scene 07 after Unity domain reload.");
        };
    }

    private static void OptimizeCurrentScene()
    {
        Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Dictionary<Material, int> materialUseCounts = new Dictionary<Material, int>();
        foreach (Renderer renderer in renderers)
        {
            if (!CanBeStatic(renderer.transform))
            {
                continue;
            }

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                materialUseCounts.TryGetValue(material, out int count);
                materialUseCounts[material] = count + 1;
            }
        }

        foreach (KeyValuePair<Material, int> pair in materialUseCounts)
        {
            if (pair.Value > 1 && pair.Key.shader != null)
            {
                pair.Key.enableInstancing = true;
                EditorUtility.SetDirty(pair.Key);
            }
        }

        foreach (Renderer renderer in renderers)
        {
            if (!CanBeStatic(renderer.transform))
            {
                continue;
            }

            StaticEditorFlags flags = StaticEditorFlags.OccludeeStatic;
            if (renderer.bounds.size.sqrMagnitude > 16f)
            {
                flags |= StaticEditorFlags.OccluderStatic;
            }

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = filter != null ? filter.sharedMesh : null;
            bool repeatedMaterial = false;
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null && materialUseCounts.TryGetValue(material, out int count) && count > 1)
                {
                    repeatedMaterial = true;
                    break;
                }
            }

            // Keep repeated/high-poly objects available for GPU instancing instead of
            // duplicating their geometry into large static batches.
            if (!repeatedMaterial && mesh != null && mesh.vertexCount <= 50000)
            {
                flags |= StaticEditorFlags.BatchingStatic;
            }

            GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, flags);
        }

        Animator[] animators = Object.FindObjectsByType<Animator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Animator animator in animators)
        {
            if (animator.transform.root.name == "REPLACE_Player_Character")
            {
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            }
            else
            {
                animator.cullingMode = AnimatorCullingMode.CullCompletely;
            }
            EditorUtility.SetDirty(animator);
        }

        foreach (Camera camera in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            camera.useOcclusionCulling = true;
            EditorUtility.SetDirty(camera);
        }
    }

    private static bool HasOptimizationFlags()
    {
        foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) != 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanBeStatic(Transform target)
    {
        if (!target.gameObject.activeInHierarchy)
        {
            return false;
        }

        string path = BuildPath(target).ToLowerInvariant();
        if (path.Contains("player") || path.Contains("npc") || path.Contains("pigeon") ||
            path.Contains("bocau") || path.Contains("boat") || path.Contains("bus") ||
            target.GetComponentInParent<Animator>() != null ||
            target.GetComponentInParent<ParticleSystem>() != null ||
            target.GetComponentInParent<BoatMovement>() != null)
        {
            return false;
        }

        return true;
    }

    private static string BuildPath(Transform target)
    {
        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }
}
#endif
