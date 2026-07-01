#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Scene04TextureOptimizer
{
    private const string ScenePath = "Assets/Scenes/Scene_04_NhaThoDucBa.unity";
    private const string OutputRoot = "Assets/Art/Optimized/Scene04Textures";

    [MenuItem("Ky Uc Sai Gon/Performance/Optimize Scene 04 Textures")]
    public static void Optimize()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(OutputRoot);
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        int rendererCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] sourceMaterials = renderer.sharedMaterials;
            Material[] optimizedMaterials = new Material[sourceMaterials.Length];
            bool changed = false;
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                Material source = sourceMaterials[i];
                optimizedMaterials[i] = OptimizeMaterial(source, textures, materials);
                changed |= optimizedMaterials[i] != source;
            }
            if (changed)
            {
                renderer.sharedMaterials = optimizedMaterials;
                rendererCount++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Scene04TextureOptimizer] Optimized {textures.Count} textures across {materials.Count} materials and {rendererCount} renderers.");
    }

    private static Material OptimizeMaterial(Material source, Dictionary<string, Texture2D> textures, Dictionary<string, Material> materials)
    {
        if (source == null) return null;
        string sourcePath = AssetDatabase.GetAssetPath(source);
        if (!sourcePath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)) return source;

        int targetSize = GetTargetSize(sourcePath);
        string materialKey = sourcePath + "|" + source.name + "|" + targetSize;
        if (materials.TryGetValue(materialKey, out Material cached)) return cached;

        string materialPath = OutputRoot + "/MAT_" + SafeName(source.name) + "_" + ShortHash(materialKey) + ".mat";
        Material optimized = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (optimized == null)
        {
            optimized = new Material(source) { name = source.name + "_Scene04Optimized" };
            AssetDatabase.CreateAsset(optimized, materialPath);
        }

        foreach (string property in source.GetTexturePropertyNames())
        {
            Texture2D sourceTexture = source.GetTexture(property) as Texture2D;
            if (sourceTexture == null || !AssetDatabase.GetAssetPath(sourceTexture).EndsWith(".glb", StringComparison.OrdinalIgnoreCase)) continue;
            bool isNormal = property.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("bump", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isNormal)
            {
                optimized.SetTexture(property, sourceTexture);
                continue;
            }
            string textureKey = AssetDatabase.GetAssetPath(sourceTexture) + "|" + sourceTexture.name + "|" + property + "|" + targetSize;
            if (!textures.TryGetValue(textureKey, out Texture2D optimizedTexture))
            {
                optimizedTexture = CreateTexture(sourceTexture, property, targetSize, textureKey);
                textures[textureKey] = optimizedTexture;
            }
            optimized.SetTexture(property, optimizedTexture);
        }
        EditorUtility.SetDirty(optimized);
        materials[materialKey] = optimized;
        return optimized;
    }

    private static Texture2D CreateTexture(Texture2D source, string property, int targetSize, string key)
    {
        int width = Mathf.Min(targetSize, source.width);
        int height = Mathf.Min(targetSize, source.height);
        string path = OutputRoot + "/TEX_" + SafeName(source.name) + "_" + SafeName(property) + "_" + ShortHash(key) + ".png";
        if (!File.Exists(path))
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            readable.Apply();
            File.WriteAllBytes(path, readable.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(readable);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        bool normal = property.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("bump", StringComparison.OrdinalIgnoreCase) >= 0;
        bool color = property.IndexOf("base", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("main", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("emiss", StringComparison.OrdinalIgnoreCase) >= 0;
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = color && !normal;
        importer.maxTextureSize = targetSize;
        importer.mipmapEnabled = true;
        importer.streamingMipmaps = true;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static int GetTargetSize(string path)
    {
        if (path.IndexOf("Vehicals", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("StreetFurniture", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("flower", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("bagia", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("hoa.glb", StringComparison.OrdinalIgnoreCase) >= 0)
            return 1024;
        return 2048;
    }

    private static string SafeName(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        return value.Replace(' ', '_').Replace('.', '_');
    }

    private static string ShortHash(string value) => Hash128.Compute(value).ToString().Substring(0, 10);
}
#endif
