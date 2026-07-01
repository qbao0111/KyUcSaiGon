#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class GeneratedLodInstaller
{
    private sealed class LodMapping
    {
        public string source;
        public string lod1;
        public string lod2;

        public LodMapping(string source, string lod1, string lod2 = null)
        {
            this.source = source;
            this.lod1 = lod1;
            this.lod2 = lod2;
        }
    }

    private static readonly LodMapping[] EndingMappings =
    {
        new LodMapping(
            "Assets/Art/Models/Ending/villa.glb",
            "Assets/Art/Optimized/Ending/villa_LOD1.glb",
            "Assets/Art/Optimized/Ending/villa_LOD2.glb")
    };

    private static readonly LodMapping[] CathedralMappings =
    {
        new LodMapping("Assets/Art/Models/NguyenHue/StreetFurniture/nguyen_hue_street_tree.glb", "Assets/Art/Optimized/NhaThoDucBa/tree_LOD1.glb", "Assets/Art/Optimized/NhaThoDucBa/tree_LOD2.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/NhaThoDucBa_StatueDucMe.glb", "Assets/Art/Optimized/NhaThoDucBa/statue_LOD1.glb", "Assets/Art/Optimized/NhaThoDucBa/statue_LOD2.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_01.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_01_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_02.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_02_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_03.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_03_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_04.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_04_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_05.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_05_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Car_06.glb", "Assets/Art/Optimized/NhaThoDucBa/Car_06_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Motorbike_01.glb", "Assets/Art/Optimized/NhaThoDucBa/Motorbike_01_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Motorbike_02.glb", "Assets/Art/Optimized/NhaThoDucBa/Motorbike_02_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Motorbike_03.glb", "Assets/Art/Optimized/NhaThoDucBa/Motorbike_03_LOD1.glb"),
        new LodMapping("Assets/Art/Models/NhaThoDucBa/Vehicals/NhaThoDucBa_Motorbike_04.glb", "Assets/Art/Optimized/NhaThoDucBa/Motorbike_04_LOD1.glb")
    };

    [MenuItem("Ky Uc Sai Gon/Performance/Install Generated LODs")]
    public static void InstallAll()
    {
        string originalPath = SceneManager.GetActiveScene().path;
        InstallScene("Assets/Scenes/Scene_04_NhaThoDucBa.unity", CathedralMappings);
        InstallScene("Assets/Scenes/Scene_07_Ending.unity", EndingMappings);
        if (!string.IsNullOrEmpty(originalPath))
        {
            EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[GeneratedLodInstaller] Installed generated LODs in Scene 04 and Scene 07.");
    }

    [InitializeOnLoadMethod]
    private static void InstallAfterImport()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Optimized/Ending/villa_LOD1.glb") == null)
            {
                return;
            }

            if (ScenesAlreadyInstalled())
            {
                return;
            }

            InstallAll();
        };
    }

    private static bool ScenesAlreadyInstalled()
    {
        string endingText = System.IO.File.ReadAllText("Assets/Scenes/Scene_07_Ending.unity");
        string cathedralText = System.IO.File.ReadAllText("Assets/Scenes/Scene_04_NhaThoDucBa.unity");
        return endingText.Contains("LODOPT_villa") && cathedralText.Contains("LODOPT_");
    }

    private static void InstallScene(string scenePath, LodMapping[] mappings)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        CleanupInvalidGeneratedLods(scene);
        foreach (LodMapping mapping in mappings)
        {
            InstallMapping(scene, mapping);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void CleanupInvalidGeneratedLods(Scene scene)
    {
        List<GameObject> invalidWrappers = new List<GameObject>();
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (LODGroup group in rootObject.GetComponentsInChildren<LODGroup>(true))
            {
                if (!group.name.StartsWith("LODOPT_", StringComparison.Ordinal))
                {
                    continue;
                }

                LOD[] lods = group.GetLODs();
                bool ownsLod0 = lods.Length > 0 && lods[0].renderers.Length > 0 &&
                    lods[0].renderers[0] != null && lods[0].renderers[0].transform.IsChildOf(group.transform);
                if (!ownsLod0)
                {
                    invalidWrappers.Add(group.gameObject);
                }
            }
        }

        foreach (GameObject wrapper in invalidWrappers)
        {
            UnityEngine.Object.DestroyImmediate(wrapper);
        }
    }

    private static void InstallMapping(Scene scene, LodMapping mapping)
    {
        GameObject lod1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(mapping.lod1);
        GameObject lod2Prefab = string.IsNullOrEmpty(mapping.lod2) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(mapping.lod2);
        if (lod1Prefab == null)
        {
            Debug.LogWarning("[GeneratedLodInstaller] Missing LOD asset: " + mapping.lod1);
            return;
        }

        List<GameObject> roots = FindPrefabRoots(scene, mapping.source);
        foreach (GameObject originalRoot in roots)
        {
            if (originalRoot == null || originalRoot.GetComponentInParent<LODGroup>() != null)
            {
                continue;
            }

            CreateLodWrapper(scene, originalRoot, lod1Prefab, lod2Prefab);
        }
    }

    private static List<GameObject> FindPrefabRoots(Scene scene, string sourcePath)
    {
        HashSet<GameObject> unique = new HashSet<GameObject>();
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Transform transform in rootObject.GetComponentsInChildren<Transform>(true))
            {
                GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(transform.gameObject);
                if (instanceRoot == null || instanceRoot.scene != scene)
                {
                    continue;
                }

                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
                if (string.Equals(path, sourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    unique.Add(instanceRoot);
                }
            }
        }
        return new List<GameObject>(unique);
    }

    private static void CreateLodWrapper(Scene scene, GameObject originalRoot, GameObject lod1Prefab, GameObject lod2Prefab)
    {
        GameObject outermostPrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(originalRoot);
        if (outermostPrefabRoot != null && outermostPrefabRoot != originalRoot)
        {
            CreateLodInPlace(scene, originalRoot, lod1Prefab, lod2Prefab);
            return;
        }

        Transform originalTransform = originalRoot.transform;
        Transform originalParent = originalTransform.parent;
        GameObject wrapper = new GameObject("LODOPT_" + originalRoot.name);
        SceneManager.MoveGameObjectToScene(wrapper, scene);
        wrapper.transform.SetParent(originalParent, false);
        wrapper.transform.localPosition = originalTransform.localPosition;
        wrapper.transform.localRotation = originalTransform.localRotation;
        wrapper.transform.localScale = originalTransform.localScale;

        originalTransform.SetParent(wrapper.transform, true);
        originalTransform.localPosition = Vector3.zero;
        originalTransform.localRotation = Quaternion.identity;
        originalTransform.localScale = Vector3.one;

        Renderer[] lod0Renderers = originalRoot.GetComponentsInChildren<Renderer>(true);
        GameObject lod1 = InstantiateLod(scene, wrapper.transform, lod1Prefab, "LOD1", lod0Renderers, false);
        Renderer[] lod1Renderers = lod1.GetComponentsInChildren<Renderer>(true);

        LODGroup group = wrapper.AddComponent<LODGroup>();
        group.fadeMode = LODFadeMode.CrossFade;
        group.animateCrossFading = false;
        if (lod2Prefab != null)
        {
            GameObject lod2 = InstantiateLod(scene, wrapper.transform, lod2Prefab, "LOD2", lod0Renderers, true);
            Renderer[] lod2Renderers = lod2.GetComponentsInChildren<Renderer>(true);
            group.SetLODs(new[]
            {
                MakeLod(0.42f, lod0Renderers, 0.08f),
                MakeLod(0.16f, lod1Renderers, 0.08f),
                MakeLod(0.045f, lod2Renderers, 0.04f)
            });
        }
        else
        {
            group.SetLODs(new[]
            {
                MakeLod(0.34f, lod0Renderers, 0.08f),
                MakeLod(0.075f, lod1Renderers, 0.05f)
            });
        }
        group.RecalculateBounds();
    }

    private static void CreateLodInPlace(Scene scene, GameObject originalRoot, GameObject lod1Prefab, GameObject lod2Prefab)
    {
        Renderer[] lod0Renderers = originalRoot.GetComponentsInChildren<Renderer>(true);
        GameObject lod1 = InstantiateLod(scene, originalRoot.transform, lod1Prefab, "LODOPT_LOD1", lod0Renderers, false);
        Renderer[] lod1Renderers = lod1.GetComponentsInChildren<Renderer>(true);

        LODGroup group = originalRoot.AddComponent<LODGroup>();
        group.fadeMode = LODFadeMode.CrossFade;
        group.animateCrossFading = false;
        if (lod2Prefab != null)
        {
            GameObject lod2 = InstantiateLod(scene, originalRoot.transform, lod2Prefab, "LODOPT_LOD2", lod0Renderers, true);
            Renderer[] lod2Renderers = lod2.GetComponentsInChildren<Renderer>(true);
            group.SetLODs(new[]
            {
                MakeLod(0.42f, lod0Renderers, 0.08f),
                MakeLod(0.16f, lod1Renderers, 0.08f),
                MakeLod(0.045f, lod2Renderers, 0.04f)
            });
        }
        else
        {
            group.SetLODs(new[]
            {
                MakeLod(0.34f, lod0Renderers, 0.08f),
                MakeLod(0.075f, lod1Renderers, 0.05f)
            });
        }
        group.RecalculateBounds();
    }

    private static LOD MakeLod(float screenHeight, Renderer[] renderers, float fadeWidth)
    {
        LOD lod = new LOD(screenHeight, renderers);
        lod.fadeTransitionWidth = fadeWidth;
        return lod;
    }

    private static GameObject InstantiateLod(Scene scene, Transform parent, GameObject prefab, string suffix, Renderer[] sourceRenderers, bool cheapShadows)
    {
        GameObject lod = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        lod.name = parent.name + "_" + suffix;
        lod.transform.SetParent(parent, false);
        lod.transform.localPosition = Vector3.zero;
        lod.transform.localRotation = Quaternion.identity;
        lod.transform.localScale = Vector3.one;

        foreach (Collider collider in lod.GetComponentsInChildren<Collider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }

        Renderer[] targetRenderers = lod.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer source = sourceRenderers[Mathf.Min(i, sourceRenderers.Length - 1)];
            targetRenderers[i].sharedMaterials = source.sharedMaterials;
            targetRenderers[i].receiveShadows = !cheapShadows;
            targetRenderers[i].shadowCastingMode = cheapShadows ? ShadowCastingMode.Off : ShadowCastingMode.On;
        }
        return lod;
    }
}
#endif
