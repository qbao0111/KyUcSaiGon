using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAnim2 : MonoBehaviour {
    public Texture2D[] Textures;
    public Texture2D[] NormalTextures;

    public bool NormalMapOn;
    public int fps = 12;
    int counter = 0;
    Renderer cachedRenderer;
    Material runtimeMaterial;

    void Start () {
        cachedRenderer = GetComponent<Renderer>();

        if (cachedRenderer == null)
        {
            Debug.LogWarning("[KyUcSaiGon] WaterAnim2 disabled: no Renderer found.", this);
            enabled = false;
            return;
        }

        bool hasColor = Textures != null && Textures.Length > 0;
        bool hasNormal = NormalMapOn && NormalTextures != null && NormalTextures.Length > 0;

        if (!hasColor && !hasNormal)
        {
            Debug.LogWarning("[KyUcSaiGon] WaterAnim2 disabled: no color or normal animation frames assigned.", this);
            enabled = false;
            return;
        }

        runtimeMaterial = cachedRenderer.material;
        fps = Mathf.Max(1, fps);
        InvokeRepeating(nameof(Increment), 0, 1.0f / fps);
    }

    void Increment()
    {
        int maxFrames = 1;
        if (Textures != null && Textures.Length > 0) maxFrames = Mathf.Max(maxFrames, Textures.Length);
        if (NormalTextures != null && NormalTextures.Length > 0) maxFrames = Mathf.Max(maxFrames, NormalTextures.Length);
        
        counter = (counter + 1) % maxFrames;
    }

    void Update () {
        if (runtimeMaterial == null)
            return;

        if (Textures != null && Textures.Length > 0)
        {
            int colorIndex = counter % Textures.Length;
            runtimeMaterial.mainTexture = Textures[colorIndex];
            runtimeMaterial.SetTexture("_BaseMap", Textures[colorIndex]);
        }

        if (NormalMapOn && NormalTextures != null && NormalTextures.Length > 0)
        {
            int normalIndex = counter % NormalTextures.Length;
            runtimeMaterial.SetTexture("_BumpMap", NormalTextures[normalIndex]);
        }
    }
}
