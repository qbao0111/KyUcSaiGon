#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTextureBatchOptimizer
{
    [MenuItem("Ky Uc Sai Gon/Performance/Restore Original GLB Normals")]
    public static void RestoreOriginalGlbNormals()
    {
        string[] roots = { "Assets/Art/Optimized/Scene01Textures", "Assets/Art/Optimized/Scene04Textures", "Assets/Art/Optimized/Scene07Textures" };
        string[] sourceFolders = { "Assets/Art/Models" };
        int materialCount = 0;
        foreach (string guid in AssetDatabase.FindAssets("", sourceFolders))
        {
            string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
            if (!sourcePath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)) continue;
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(sourcePath))
            {
                Material original = asset as Material;
                if (original == null) continue;
                foreach (string root in roots)
                {
                    string key = sourcePath + "|" + original.name + "|" + root;
                    string materialPath = root + "/MAT_" + SafeName(original.name) + "_" + ShortHash(key) + ".mat";
                    Material optimized = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                    if (optimized == null) continue;
                    bool changed = false;
                    foreach (string property in original.GetTexturePropertyNames())
                    {
                        if (property.IndexOf("normal", StringComparison.OrdinalIgnoreCase) < 0 && property.IndexOf("bump", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        Texture texture = original.GetTexture(property);
                        if (texture == null) continue;
                        optimized.SetTexture(property, texture);
                        changed = true;
                    }
                    if (changed) { EditorUtility.SetDirty(optimized); materialCount++; }
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[SceneTextureBatchOptimizer] Restored original GLB normals on {materialCount} optimized materials.");
    }

    [MenuItem("Ky Uc Sai Gon/Performance/Optimize Nguyen Hue and Ending Textures")]
    public static void OptimizeBoth()
    {
        string original = SceneManager.GetActiveScene().path;
        OptimizeScene("Assets/Scenes/Scene_01_NguyenHue_Tutorial.unity", "Assets/Art/Optimized/Scene01Textures");
        OptimizeScene("Assets/Scenes/Scene_07_Ending.unity", "Assets/Art/Optimized/Scene07Textures");
        if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original, OpenSceneMode.Single);
        AssetDatabase.SaveAssets();
    }

    private static void OptimizeScene(string scenePath, string outputRoot)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        Directory.CreateDirectory(outputRoot);
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        int rendererCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] sourceMaterials = renderer.sharedMaterials;
            Material[] result = new Material[sourceMaterials.Length];
            bool changed = false;
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                result[i] = OptimizeMaterial(sourceMaterials[i], outputRoot, textures, materials);
                changed |= result[i] != sourceMaterials[i];
            }
            if (changed) { renderer.sharedMaterials = result; rendererCount++; }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[SceneTextureBatchOptimizer] {scene.name}: {textures.Count} textures, {materials.Count} materials, {rendererCount} renderers.");
    }

    private static Material OptimizeMaterial(Material source, string outputRoot, Dictionary<string, Texture2D> textures, Dictionary<string, Material> materials)
    {
        if (source == null || AssetDatabase.GetAssetPath(source).Contains("/Optimized/")) return source;
        List<string> eligible = new List<string>();
        foreach (string property in source.GetTexturePropertyNames())
        {
            Texture2D texture = source.GetTexture(property) as Texture2D;
            string path = texture == null ? "" : AssetDatabase.GetAssetPath(texture);
            if (path.StartsWith("Assets/Art/") && !path.Contains("/Optimized/")) eligible.Add(property);
        }
        if (eligible.Count == 0) return source;

        string sourcePath = AssetDatabase.GetAssetPath(source);
        string key = sourcePath + "|" + source.name + "|" + outputRoot;
        if (materials.TryGetValue(key, out Material cached)) return cached;
        string materialPath = outputRoot + "/MAT_" + SafeName(source.name) + "_" + ShortHash(key) + ".mat";
        Material optimized = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (optimized == null) { optimized = new Material(source) { name = source.name + "_Optimized" }; AssetDatabase.CreateAsset(optimized, materialPath); }

        foreach (string property in eligible)
        {
            Texture2D sourceTexture = source.GetTexture(property) as Texture2D;
            string texturePath = AssetDatabase.GetAssetPath(sourceTexture);
            bool isEmbeddedNormal = texturePath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) &&
                (property.IndexOf("normal", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("bump", StringComparison.OrdinalIgnoreCase) >= 0);
            if (isEmbeddedNormal)
            {
                optimized.SetTexture(property, sourceTexture);
                continue;
            }
            int targetSize = GetTargetSize(texturePath);
            string textureKey = texturePath + "|" + sourceTexture.name + "|" + property + "|" + targetSize + "|" + outputRoot;
            if (!textures.TryGetValue(textureKey, out Texture2D result))
            {
                result = texturePath.EndsWith(".glb", StringComparison.OrdinalIgnoreCase)
                    ? ExtractEmbeddedTexture(sourceTexture, property, targetSize, textureKey, outputRoot)
                    : CopyExternalTexture(texturePath, targetSize, textureKey, outputRoot);
                textures[textureKey] = result;
            }
            optimized.SetTexture(property, result);
        }
        EditorUtility.SetDirty(optimized);
        materials[key] = optimized;
        return optimized;
    }

    private static Texture2D ExtractEmbeddedTexture(Texture2D source, string property, int targetSize, string key, string outputRoot)
    {
        int width = Mathf.Min(targetSize, source.width), height = Mathf.Min(targetSize, source.height);
        string path = outputRoot + "/TEX_" + SafeName(source.name) + "_" + SafeName(property) + "_" + ShortHash(key) + ".png";
        if (!File.Exists(path))
        {
            RenderTexture rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(source, rt);
            RenderTexture old = RenderTexture.active; RenderTexture.active = rt;
            Texture2D readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, width, height), 0, 0); readable.Apply();
            File.WriteAllBytes(path, readable.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(readable); RenderTexture.active = old; RenderTexture.ReleaseTemporary(rt);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        bool color = property.IndexOf("base", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("main", StringComparison.OrdinalIgnoreCase) >= 0 || property.IndexOf("emiss", StringComparison.OrdinalIgnoreCase) >= 0;
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = color;
        ConfigureImporter(importer, targetSize);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Texture2D CopyExternalTexture(string sourcePath, int targetSize, string key, string outputRoot)
    {
        string extension = Path.GetExtension(sourcePath);
        string path = outputRoot + "/TEX_" + SafeName(Path.GetFileNameWithoutExtension(sourcePath)) + "_" + ShortHash(key) + extension;
        if (!File.Exists(path)) AssetDatabase.CopyAsset(sourcePath, path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        ConfigureImporter(importer, targetSize);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static void ConfigureImporter(TextureImporter importer, int targetSize)
    {
        importer.maxTextureSize = targetSize;
        importer.mipmapEnabled = true;
        importer.streamingMipmaps = true;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }

    private static int GetTargetSize(string path)
    {
        string p = path.ToLowerInvariant();
        if (p.Contains("character") || p.Contains("npc") || p.Contains("facade") || p.Contains("cityhall") ||
            p.Contains("landmark") || p.Contains("causaigon") || p.Contains("skybox")) return 2048;
        return 1024;
    }

    private static string SafeName(string value)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
        return value.Replace(' ', '_').Replace('.', '_');
    }
    private static string ShortHash(string value) => Hash128.Compute(value).ToString().Substring(0, 10);
}
#endif
