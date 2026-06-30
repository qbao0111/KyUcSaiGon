#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ConfigureLanternSprite
{
    static ConfigureLanternSprite()
    {
        string path = "Assets/Resources/UI/I_VietnameseLantern.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite || !importer.alphaIsTransparency)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
                Debug.Log("[ConfigureLanternSprite] Successfully configured lantern texture importer as Sprite.");
            }
        }
    }
}
#endif