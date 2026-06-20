#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GLTFast;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class DinhDocLapImportedLandmarkInstaller
{
    private const string ScenePath = "Assets/Scenes/Scene_03_DinhDocLap.unity";
    private const string PalacePlazaPath = "Assets/Art/Models/DinhDocLap/DinhDocLap_PalacePlaza.glb";
    private const string FountainPath = "Assets/Art/Models/DinhDocLap/DinhDocLap_Fountain.glb";
    private const string TreePrefabPath = "Assets/Art/Prefabs/NguyenHue/StreetFurniture/PF_NguyenHue_StreetTree.prefab";
    private const string MenuPath = "Ky Uc Sai Gon/Setup/Apply Dinh Doc Lap Imported Models";
    private const string AutoRunFlagPath = "Assets/EditorBuildFlags/RunDinhDocLapImportedLandmarkInstaller.flag";


    [InitializeOnLoadMethod]
    private static void AutoRunIfRequested()
    {
        EditorApplication.update -= TryRunPendingAutoApply;
        EditorApplication.update += TryRunPendingAutoApply;
    }

    private static void TryRunPendingAutoApply()
    {
        if (!File.Exists(AutoRunFlagPath))
        {
            EditorApplication.update -= TryRunPendingAutoApply;
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= TryRunPendingAutoApply;
        AssetDatabase.DeleteAsset(AutoRunFlagPath);
        ApplyNoPrompt();
    }
    [MenuItem(MenuPath)]
    public static void ApplyFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.Log("[KyUcSaiGon] Dinh Doc Lap imported model setup cancelled.");
            return;
        }

        ApplyNoPrompt();
    }

    public static void ApplyNoPrompt()
    {
        string startingScene = SceneManager.GetActiveScene().path;

        try
        {
            if (!File.Exists(PalacePlazaPath) || !File.Exists(FountainPath))
            {
                Debug.LogError("[KyUcSaiGon] Missing Dinh Doc Lap GLB model assets.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject sceneRoot = FindOrCreateRoot("SceneBlockoutRoot");
            Transform importedRoot = FindOrCreateChild(sceneRoot.transform, "DinhDocLap_ImportedLandmarkRoot");
            ClearChildren(importedRoot);

            DisableDuplicatePrimitiveVisuals(sceneRoot.transform);
            EnsureWalkableColliders(sceneRoot.transform, importedRoot);
            if (!PlaceImportedModels(importedRoot))
            {
                Debug.LogError("[KyUcSaiGon] Dinh Doc Lap imported model setup failed. Scene was left with gameplay objects and colliders intact.");
                return;
            }

            PolishImportedVisualLayout(importedRoot);
            PlaceImportedTrees(importedRoot);
            RepositionGameplayObjects(sceneRoot.transform);
            RepairGameplayReferences(sceneRoot.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[KyUcSaiGon] Scene_03_DinhDocLap imported PalacePlaza/Fountain models placed. Gameplay objects preserved.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(startingScene) && File.Exists(startingScene))
            {
                EditorSceneManager.OpenScene(startingScene, OpenSceneMode.Single);
            }
        }
    }

    private static bool PlaceImportedModels(Transform importedRoot)
    {
        Transform palaceRoot = CreateChild(importedRoot, "Visual_REPLACE_DinhDocLap_PalacePlaza");
        palaceRoot.position = Vector3.zero;
        palaceRoot.rotation = Quaternion.identity;
        palaceRoot.localScale = Vector3.one;
        if (!InstantiateGlbIntoScene(PalacePlazaPath, palaceRoot, "DinhDocLap_PalacePlaza_Model"))
        {
            return false;
        }

        FitBoundsToScene(palaceRoot, new Vector3(0f, 0f, 2.5f), 76f);
        DisableModelColliders(palaceRoot);

        Transform fountainRoot = CreateChild(importedRoot, "Visual_REPLACE_DinhDocLap_Fountain");
        fountainRoot.position = new Vector3(0f, 0.08f, -0.5f);
        fountainRoot.rotation = Quaternion.identity;
        fountainRoot.localScale = Vector3.one;
        if (!InstantiateGlbIntoScene(FountainPath, fountainRoot, "DinhDocLap_Fountain_Model"))
        {
            return false;
        }

        FitBoundsToSize(fountainRoot, 5.2f);
        DisableModelColliders(fountainRoot);
        return true;
    }

    private static bool InstantiateGlbIntoScene(string assetPath, Transform parent, string modelName)
    {
        string fullPath = Path.GetFullPath(assetPath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError("[KyUcSaiGon] Missing GLB file: " + fullPath);
            return false;
        }

        try
        {
            using GltfImport gltf = new GltfImport(deferAgent: new UninterruptedDeferAgent());
            string glbUri = new Uri(fullPath).AbsoluteUri;
            Debug.Log($"[KyUcSaiGon] Loading GLB model from {glbUri}");
            bool loaded = RunTask(() => gltf.Load(glbUri));
            if (!loaded)
            {
                Debug.LogError("[KyUcSaiGon] glTFast could not load GLB file: " + assetPath);
                return false;
            }

            bool instantiated = RunTask(() => gltf.InstantiateMainSceneAsync(parent));
            if (!instantiated || parent.childCount == 0)
            {
                Debug.LogError("[KyUcSaiGon] glTFast loaded but did not instantiate: " + assetPath);
                return false;
            }

            if (parent.childCount == 1)
            {
                parent.GetChild(0).name = modelName;
            }
            else
            {
                GameObject wrapper = new GameObject(modelName);
                wrapper.transform.SetParent(parent, false);
                for (int i = parent.childCount - 2; i >= 0; i--)
                {
                    parent.GetChild(i).SetParent(wrapper.transform, true);
                }
            }

            EnsureExtractedGlbMeshAssigned(assetPath, parent);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("[KyUcSaiGon] Failed to instantiate GLB through glTFast API: " + assetPath);
            Debug.LogException(exception);
            return false;
        }
    }

    private static void EnsureExtractedGlbMeshAssigned(string assetPath, Transform parent)
    {
        MeshFilter[] meshFilters = parent.GetComponentsInChildren<MeshFilter>(true);
        bool hasMissingMesh = meshFilters.Length == 0;
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null || mesh.vertexCount == 0 || mesh.bounds.size.sqrMagnitude < 0.0001f || !AssetDatabase.Contains(mesh))
            {
                hasMissingMesh = true;
                break;
            }
        }

        Debug.Log($"[KyUcSaiGon] GLB fallback check for {assetPath}: filters={meshFilters.Length}, hasMissingMesh={hasMissingMesh}");
        if (!hasMissingMesh)
        {
            return;
        }

        GlbExtractedModel extracted = ExtractGlbModel(assetPath);
        if (extracted.Mesh == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Could not extract fallback mesh from " + assetPath);
            return;
        }

        GameObject target = meshFilters.Length > 0 ? meshFilters[0].gameObject : parent.gameObject;
        MeshFilter targetFilter = target.GetComponent<MeshFilter>();
        if (targetFilter == null)
        {
            targetFilter = target.AddComponent<MeshFilter>();
        }

        MeshRenderer targetRenderer = target.GetComponent<MeshRenderer>();
        if (targetRenderer == null)
        {
            targetRenderer = target.AddComponent<MeshRenderer>();
        }

        targetFilter.sharedMesh = extracted.Mesh;
        targetRenderer.sharedMaterials = extracted.Materials;
        EditorUtility.SetDirty(targetFilter);
        EditorUtility.SetDirty(targetRenderer);
        Debug.Log($"[KyUcSaiGon] Assigned extracted GLB mesh fallback for {assetPath}. vertices={extracted.Mesh.vertexCount}");
    }

    private sealed class GlbExtractedModel
    {
        public Mesh Mesh;
        public Material[] Materials;
    }

    private static GlbExtractedModel ExtractGlbModel(string assetPath)
    {
        Debug.Log("[KyUcSaiGon] Extracting GLB fallback mesh from " + assetPath);
        string assetName = Path.GetFileNameWithoutExtension(assetPath);
        string outputRoot = "Assets/Art/Generated/DinhDocLap/ExtractedFromGlb";
        string meshPath = $"{outputRoot}/{assetName}_Mesh.asset";
        string materialRoot = $"{outputRoot}/Materials";
        string textureRoot = $"{outputRoot}/Textures";
        Directory.CreateDirectory(outputRoot);
        Directory.CreateDirectory(materialRoot);
        Directory.CreateDirectory(textureRoot);

        byte[] fileBytes = File.ReadAllBytes(assetPath);
        JObject json = ReadGlbJson(fileBytes, out byte[] binaryChunk);
        if (json == null || binaryChunk == null)
        {
            return new GlbExtractedModel();
        }

        JArray meshes = (JArray)json["meshes"];
        if (meshes == null || meshes.Count == 0)
        {
            return new GlbExtractedModel();
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int[]> submeshTriangles = new List<int[]>();
        List<int> materialIndices = new List<int>();

        foreach (JToken primitive in meshes[0]["primitives"])
        {
            JObject attributes = (JObject)primitive["attributes"];
            if (attributes == null || attributes["POSITION"] == null)
            {
                continue;
            }

            int baseVertex = vertices.Count;
            Vector3[] primitivePositions = ReadVec3Accessor(json, binaryChunk, (int)attributes["POSITION"]);
            Vector3[] primitiveNormals = attributes["NORMAL"] != null
                ? ReadVec3Accessor(json, binaryChunk, (int)attributes["NORMAL"])
                : new Vector3[primitivePositions.Length];
            Vector2[] primitiveUvs = attributes["TEXCOORD_0"] != null
                ? ReadVec2Accessor(json, binaryChunk, (int)attributes["TEXCOORD_0"])
                : new Vector2[primitivePositions.Length];

            vertices.AddRange(primitivePositions);
            normals.AddRange(primitiveNormals);
            uvs.AddRange(primitiveUvs);

            int[] indices = primitive["indices"] != null
                ? ReadIndexAccessor(json, binaryChunk, (int)primitive["indices"])
                : CreateSequentialIndices(primitivePositions.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] += baseVertex;
            }

            submeshTriangles.Add(indices);
            materialIndices.Add(primitive["material"] != null ? (int)primitive["material"] : -1);
        }

        Mesh mesh = new Mesh
        {
            name = assetName + "_ExtractedMesh",
            indexFormat = vertices.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
        };
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = submeshTriangles.Count;
        for (int i = 0; i < submeshTriangles.Count; i++)
        {
            mesh.SetTriangles(submeshTriangles[i], i, true);
        }

        mesh.RecalculateBounds();
        if (normals.Count == 0 || normals.TrueForAll(n => n == Vector3.zero))
        {
            mesh.RecalculateNormals();
        }

        Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (existingMesh == null)
        {
            AssetDatabase.CreateAsset(mesh, meshPath);
            existingMesh = mesh;
        }
        else
        {
            EditorUtility.CopySerialized(mesh, existingMesh);
            EditorUtility.SetDirty(existingMesh);
        }

        Material[] materials = CreateExtractedMaterials(json, binaryChunk, materialIndices, materialRoot, textureRoot, assetName);
        AssetDatabase.SaveAssets();

        return new GlbExtractedModel
        {
            Mesh = existingMesh,
            Materials = materials
        };
    }

    private static JObject ReadGlbJson(byte[] fileBytes, out byte[] binaryChunk)
    {
        binaryChunk = null;
        if (fileBytes.Length < 20 || BitConverter.ToUInt32(fileBytes, 0) != 0x46546C67)
        {
            return null;
        }

        int offset = 12;
        JObject json = null;
        while (offset + 8 <= fileBytes.Length)
        {
            int chunkLength = BitConverter.ToInt32(fileBytes, offset);
            uint chunkType = BitConverter.ToUInt32(fileBytes, offset + 4);
            offset += 8;
            if (offset + chunkLength > fileBytes.Length)
            {
                break;
            }

            byte[] chunk = new byte[chunkLength];
            Buffer.BlockCopy(fileBytes, offset, chunk, 0, chunkLength);
            offset += chunkLength;

            if (chunkType == 0x4E4F534A)
            {
                json = JObject.Parse(System.Text.Encoding.UTF8.GetString(chunk));
            }
            else if (chunkType == 0x004E4942)
            {
                binaryChunk = chunk;
            }
        }

        return json;
    }

    private static Vector3[] ReadVec3Accessor(JObject json, byte[] binaryChunk, int accessorIndex)
    {
        AccessorView view = GetAccessorView(json, accessorIndex);
        Vector3[] values = new Vector3[view.Count];
        for (int i = 0; i < view.Count; i++)
        {
            int offset = view.StartOffset + i * view.Stride;
            values[i] = new Vector3(
                BitConverter.ToSingle(binaryChunk, offset),
                BitConverter.ToSingle(binaryChunk, offset + 4),
                BitConverter.ToSingle(binaryChunk, offset + 8));
        }

        return values;
    }

    private static Vector2[] ReadVec2Accessor(JObject json, byte[] binaryChunk, int accessorIndex)
    {
        AccessorView view = GetAccessorView(json, accessorIndex);
        Vector2[] values = new Vector2[view.Count];
        for (int i = 0; i < view.Count; i++)
        {
            int offset = view.StartOffset + i * view.Stride;
            values[i] = new Vector2(
                BitConverter.ToSingle(binaryChunk, offset),
                1f - BitConverter.ToSingle(binaryChunk, offset + 4));
        }

        return values;
    }

    private static int[] ReadIndexAccessor(JObject json, byte[] binaryChunk, int accessorIndex)
    {
        JArray accessors = (JArray)json["accessors"];
        JObject accessor = (JObject)accessors[accessorIndex];
        AccessorView view = GetAccessorView(json, accessorIndex);
        int componentType = (int)accessor["componentType"];
        int[] indices = new int[view.Count];
        for (int i = 0; i < view.Count; i++)
        {
            int offset = view.StartOffset + i * view.Stride;
            indices[i] = componentType switch
            {
                5121 => binaryChunk[offset],
                5123 => BitConverter.ToUInt16(binaryChunk, offset),
                5125 => unchecked((int)BitConverter.ToUInt32(binaryChunk, offset)),
                _ => i
            };
        }

        return indices;
    }

    private static int[] CreateSequentialIndices(int count)
    {
        int[] indices = new int[count];
        for (int i = 0; i < count; i++)
        {
            indices[i] = i;
        }

        return indices;
    }

    private sealed class AccessorView
    {
        public int StartOffset;
        public int Stride;
        public int Count;
    }

    private static AccessorView GetAccessorView(JObject json, int accessorIndex)
    {
        JArray accessors = (JArray)json["accessors"];
        JArray bufferViews = (JArray)json["bufferViews"];
        JObject accessor = (JObject)accessors[accessorIndex];
        JObject bufferView = (JObject)bufferViews[(int)accessor["bufferView"]];
        int componentSize = GetComponentSize((int)accessor["componentType"]);
        int componentCount = GetTypeComponentCount((string)accessor["type"]);
        int accessorOffset = accessor["byteOffset"] != null ? (int)accessor["byteOffset"] : 0;
        int bufferViewOffset = bufferView["byteOffset"] != null ? (int)bufferView["byteOffset"] : 0;
        int stride = bufferView["byteStride"] != null ? (int)bufferView["byteStride"] : componentSize * componentCount;

        return new AccessorView
        {
            StartOffset = bufferViewOffset + accessorOffset,
            Stride = stride,
            Count = (int)accessor["count"]
        };
    }

    private static int GetComponentSize(int componentType)
    {
        return componentType switch
        {
            5120 or 5121 => 1,
            5122 or 5123 => 2,
            5125 or 5126 => 4,
            _ => 4
        };
    }

    private static int GetTypeComponentCount(string type)
    {
        return type switch
        {
            "SCALAR" => 1,
            "VEC2" => 2,
            "VEC3" => 3,
            "VEC4" => 4,
            "MAT4" => 16,
            _ => 1
        };
    }

    private static Material[] CreateExtractedMaterials(JObject json, byte[] binaryChunk, List<int> materialIndices, string materialRoot, string textureRoot, string assetName)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material fallback = CreateOrUpdateMaterial($"{materialRoot}/{assetName}_Fallback.mat", shader, new Color(0.72f, 0.72f, 0.68f, 1f), null);
        Material[] rendererMaterials = new Material[materialIndices.Count];

        for (int i = 0; i < materialIndices.Count; i++)
        {
            int materialIndex = materialIndices[i];
            if (materialIndex < 0)
            {
                rendererMaterials[i] = fallback;
                continue;
            }

            rendererMaterials[i] = CreateMaterialFromGlb(json, binaryChunk, materialIndex, materialRoot, textureRoot, assetName, shader) ?? fallback;
        }

        return rendererMaterials;
    }

    private static Material CreateMaterialFromGlb(JObject json, byte[] binaryChunk, int materialIndex, string materialRoot, string textureRoot, string assetName, Shader shader)
    {
        JArray materials = (JArray)json["materials"];
        if (materials == null || materialIndex >= materials.Count)
        {
            return null;
        }

        JObject source = (JObject)materials[materialIndex];
        string materialName = SanitizeAssetName((string)source["name"] ?? $"Material_{materialIndex:00}");
        JObject pbr = (JObject)source["pbrMetallicRoughness"];
        Color baseColor = Color.white;
        if (pbr?["baseColorFactor"] is JArray factor && factor.Count >= 3)
        {
            baseColor = new Color((float)factor[0], (float)factor[1], (float)factor[2], factor.Count > 3 ? (float)factor[3] : 1f);
        }

        Texture2D baseTexture = null;
        if (pbr?["baseColorTexture"]?["index"] != null)
        {
            baseTexture = ExtractTexture(json, binaryChunk, (int)pbr["baseColorTexture"]["index"], textureRoot, $"{assetName}_{materialName}");
        }

        return CreateOrUpdateMaterial($"{materialRoot}/{assetName}_{materialName}.mat", shader, baseColor, baseTexture);
    }

    private static Texture2D ExtractTexture(JObject json, byte[] binaryChunk, int textureIndex, string textureRoot, string namePrefix)
    {
        JArray textures = (JArray)json["textures"];
        JArray images = (JArray)json["images"];
        if (textures == null || images == null || textureIndex >= textures.Count)
        {
            return null;
        }

        JObject texture = (JObject)textures[textureIndex];
        if (texture["source"] == null)
        {
            return null;
        }

        JObject image = (JObject)images[(int)texture["source"]];
        if (image["bufferView"] == null)
        {
            return null;
        }

        JArray bufferViews = (JArray)json["bufferViews"];
        JObject bufferView = (JObject)bufferViews[(int)image["bufferView"]];
        int byteOffset = bufferView["byteOffset"] != null ? (int)bufferView["byteOffset"] : 0;
        int byteLength = (int)bufferView["byteLength"];
        byte[] imageBytes = new byte[byteLength];
        Buffer.BlockCopy(binaryChunk, byteOffset, imageBytes, 0, byteLength);

        string extension = ((string)image["mimeType"]) == "image/jpeg" ? ".jpg" : ".png";
        string textureName = SanitizeAssetName((string)image["name"] ?? $"{namePrefix}_Texture");
        string texturePath = $"{textureRoot}/{textureName}{extension}";
        File.WriteAllBytes(texturePath, imageBytes);
        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
    }

    private static Material CreateOrUpdateMaterial(string path, Shader shader, Color baseColor, Texture2D baseTexture)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseColor);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", baseColor);
        }

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", baseTexture);
        }
        else if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", baseTexture);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.35f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static string SanitizeAssetName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Replace(' ', '_');
    }

    private static T RunTask<T>(Func<Task<T>> taskFactory)
    {
        SynchronizationContext oldContext = SynchronizationContext.Current;
        EditorSynchronizationContext syncContext = new EditorSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(syncContext);
        T result = default;

        try
        {
            syncContext.Post(async _ =>
            {
                try
                {
                    result = await taskFactory();
                }
                catch (Exception exception)
                {
                    syncContext.InnerException = exception;
                }
                finally
                {
                    syncContext.EndMessageLoop();
                }
            }, null);

            syncContext.BeginMessageLoop();
            if (syncContext.InnerException != null)
            {
                throw syncContext.InnerException;
            }
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        return result;
    }

    private sealed class EditorSynchronizationContext : SynchronizationContext
    {
        private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
        private readonly Queue<Tuple<SendOrPostCallback, object>> workItems = new Queue<Tuple<SendOrPostCallback, object>>();
        private bool done;

        public Exception InnerException { get; set; }

        public override void Post(SendOrPostCallback callback, object state)
        {
            lock (workItems)
            {
                workItems.Enqueue(Tuple.Create(callback, state));
            }

            workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object> workItem = null;
                lock (workItems)
                {
                    if (workItems.Count > 0)
                    {
                        workItem = workItems.Dequeue();
                    }
                }

                if (workItem != null)
                {
                    workItem.Item1(workItem.Item2);
                    if (InnerException != null)
                    {
                        throw InnerException;
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }
    }

    private static void FitBoundsToScene(Transform root, Vector3 targetCenter, float targetXZSize)
    {
        Bounds bounds = CalculateBounds(root);
        float largestXZ = Mathf.Max(bounds.size.x, bounds.size.z);
        if (largestXZ > 0.001f)
        {
            float scale = targetXZSize / largestXZ;
            root.localScale = Vector3.one * scale;
        }

        bounds = CalculateBounds(root);
        root.position += targetCenter - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        Debug.Log($"[KyUcSaiGon] PalacePlaza final bounds: {CalculateBounds(root)} scale={root.localScale}");
    }

    private static void FitBoundsToSize(Transform root, float targetXZSize)
    {
        Bounds bounds = CalculateBounds(root);
        float largestXZ = Mathf.Max(bounds.size.x, bounds.size.z);
        if (largestXZ > 0.001f)
        {
            root.localScale = Vector3.one * (targetXZSize / largestXZ);
        }

        Bounds scaledBounds = CalculateBounds(root);
        root.position += new Vector3(-scaledBounds.center.x, -scaledBounds.min.y + 0.08f, -scaledBounds.center.z - 0.5f) - root.position;
        Debug.Log($"[KyUcSaiGon] Fountain final bounds: {CalculateBounds(root)} scale={root.localScale}");
    }

    private static void PolishImportedVisualLayout(Transform importedRoot)
    {
        Transform polishRoot = CreateChild(importedRoot, "VisualLayoutPolish_DoNotReplaceGameplay");
        Material tileLight = GetOrCreateMaterial("Assets/Art/Materials/DinhDocLap/M_DinhDocLap_PlazaTile_LightGray.mat", new Color(0.64f, 0.68f, 0.70f, 1f), false, 0f);
        Material tileMid = GetOrCreateMaterial("Assets/Art/Materials/DinhDocLap/M_DinhDocLap_PlazaTile_MidGray.mat", new Color(0.52f, 0.56f, 0.58f, 1f), false, 0f);
        Material seamDark = GetOrCreateMaterial("Assets/Art/Materials/DinhDocLap/M_DinhDocLap_PlazaTile_SeamDark.mat", new Color(0.22f, 0.24f, 0.25f, 1f), false, 0f);
        Material grassBright = GetOrCreateMaterial("Assets/Art/Materials/DinhDocLap/M_DinhDocLap_Grass_BrightNatural.mat", new Color(70f / 255f, 150f / 255f, 55f / 255f, 1f), false, 0f);

        CreatePlazaTiles(polishRoot, tileLight, tileMid, seamDark);
        BrightenExtractedGrassMaterials(grassBright.color);
    }

    private static void CreatePlazaTiles(Transform parent, Material tileLight, Material tileMid, Material seamDark)
    {
        Transform tileRoot = CreateChild(parent, "RestoredPlazaTiles");
        const float tileSize = 5.8f;
        const float halfWidth = 38f;
        const float minZ = -39f;
        const float maxZ = 35f;
        Vector3 lawnCenter = new Vector3(0f, 0f, 2.5f);
        const float keepClearRadius = 20.5f;

        int index = 0;
        for (float x = -34.8f; x <= 34.8f; x += tileSize)
        {
            for (float z = -36.5f; z <= 32.5f; z += tileSize)
            {
                float distanceToLawn = Vector2.Distance(new Vector2(x, z), new Vector2(lawnCenter.x, lawnCenter.z));
                bool isMainApproach = Mathf.Abs(x) < 5.2f && (z < -17.5f || z > 22.5f);
                if (distanceToLawn < keepClearRadius && !isMainApproach)
                {
                    continue;
                }

                Material material = (index % 2 == 0) ? tileLight : tileMid;
                CreateBox(tileRoot, "Visual_REPLACE_DinhDocLap_PlazaTile_" + index.ToString("000"), new Vector3(x, 0.025f, z), new Vector3(tileSize - 0.16f, 0.035f, tileSize - 0.16f), material, false, false);
                index++;
            }
        }

        CreateBox(tileRoot, "Visual_REPLACE_DinhDocLap_PlazaEdge_Left", new Vector3(-halfWidth, 0.052f, -2f), new Vector3(0.12f, 0.02f, maxZ - minZ), seamDark, false, false);
        CreateBox(tileRoot, "Visual_REPLACE_DinhDocLap_PlazaEdge_Right", new Vector3(halfWidth, 0.052f, -2f), new Vector3(0.12f, 0.02f, maxZ - minZ), seamDark, false, false);
        CreateBox(tileRoot, "Visual_REPLACE_DinhDocLap_PlazaEdge_Front", new Vector3(0f, 0.052f, minZ), new Vector3(halfWidth * 2f, 0.02f, 0.12f), seamDark, false, false);
        CreateBox(tileRoot, "Visual_REPLACE_DinhDocLap_PlazaEdge_Back", new Vector3(0f, 0.052f, maxZ), new Vector3(halfWidth * 2f, 0.02f, 0.12f), seamDark, false, false);
    }

    private static void BrightenExtractedGrassMaterials(Color grassColor)
    {
        string materialRoot = "Assets/Art/Generated/DinhDocLap/ExtractedFromGlb/Materials";
        if (!Directory.Exists(materialRoot))
        {
            return;
        }

        foreach (string materialPath in Directory.GetFiles(materialRoot, "*.mat"))
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath.Replace('\\', '/'));
            if (material == null)
            {
                continue;
            }

            string lowerName = material.name.ToLowerInvariant();
            Color current = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            bool looksLikeGrass = lowerName.Contains("grass")
                || lowerName.Contains("lawn")
                || lowerName.Contains("material.003")
                || (current.g > current.r * 1.8f && current.g > current.b * 1.4f && current.g < 0.35f);

            if (!looksLikeGrass)
            {
                continue;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", grassColor);
            }
            else
            {
                material.color = grassColor;
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.25f);
            }

            EditorUtility.SetDirty(material);
        }
    }

    private static void DisableDuplicatePrimitiveVisuals(Transform sceneRoot)
    {
        string[] duplicateRoots =
        {
            "DinhDocLap_RecognizableLayout",
            "DinhDocLap_LandmarkLayout",
            "DinhDocLap_TreeRows"
        };

        foreach (string rootName in duplicateRoots)
        {
            Transform root = FindSceneTransform(rootName);
            if (root == null)
            {
                continue;
            }

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
                EditorUtility.SetDirty(renderer);
            }
        }

        // Keep old ground collider disabled from rendering but usable as backup floor if it exists.
        Transform oldGroundCollider = FindSceneTransform("DinhDocLap_GroundCollider");
        if (oldGroundCollider != null)
        {
            Collider collider = oldGroundCollider.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = true;
            }
        }
    }

    private static void EnsureWalkableColliders(Transform sceneRoot, Transform importedRoot)
    {
        Transform colliderRoot = FindOrCreateChild(importedRoot, "Gameplay_Colliders_DoNotReplace");
        ClearChildren(colliderRoot);

        GameObject floor = new GameObject("DinhDocLap_ImportedWalkableGroundCollider");
        floor.transform.SetParent(colliderRoot, false);
        floor.transform.position = new Vector3(0f, -0.06f, 2f);
        floor.transform.localScale = Vector3.one;
        BoxCollider floorCollider = floor.AddComponent<BoxCollider>();
        floorCollider.size = new Vector3(86f, 0.12f, 84f);
        floorCollider.center = Vector3.zero;
        floorCollider.isTrigger = false;

        GameObject palaceBlocker = new GameObject("DinhDocLap_PalaceSimpleBlocker");
        palaceBlocker.transform.SetParent(colliderRoot, false);
        palaceBlocker.transform.position = new Vector3(0f, 2.2f, 27.5f);
        BoxCollider palaceCollider = palaceBlocker.AddComponent<BoxCollider>();
        palaceCollider.size = new Vector3(58f, 4.4f, 5.5f);
        palaceCollider.isTrigger = false;
    }

    private static void PlaceImportedTrees(Transform importedRoot)
    {
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreePrefabPath);
        if (treePrefab == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Nguyen Hue tree prefab not found, skipped Dinh Doc Lap imported tree rows.");
            return;
        }

        Transform treeRoot = CreateChild(importedRoot, "ImportedTreeRows_UseNguyenHueTreePrefab");
        float[] zPositions = { -30f, -24f, -18f, -12f, -6f, 0f, 6f, 12f, 18f, 24f };
        int index = 1;
        foreach (float z in zPositions)
        {
            CreateTree(treeRoot, treePrefab, $"Tree_Left_{index:00}", new Vector3(-24f, 0f, z), 10f);
            CreateTree(treeRoot, treePrefab, $"Tree_Right_{index:00}", new Vector3(24f, 0f, z), -10f);
            index++;
        }
    }

    private static void CreateTree(Transform parent, GameObject prefab, string name, Vector3 position, float yRotation)
    {
        GameObject tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        tree.name = name;
        tree.transform.position = position;
        tree.transform.rotation = Quaternion.Euler(90f, yRotation, 0f);
        tree.transform.localScale = new Vector3(5.4f, 6.2f, 6.2f);
        DisableModelColliders(tree.transform);
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collider, bool trigger)
    {
        GameObject item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.position = position;
        item.transform.localScale = scale;
        Renderer renderer = item.GetComponent<Renderer>();
        renderer.sharedMaterial = material;

        Collider boxCollider = item.GetComponent<Collider>();
        if (collider)
        {
            boxCollider.isTrigger = trigger;
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(boxCollider);
        }

        return item;
    }

    private static Material GetOrCreateMaterial(string path, Color color, bool transparent, float emission)
    {
        EnsureFolder("Assets/Art", "Materials");
        EnsureFolder("Assets/Art/Materials", "DinhDocLap");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else
        {
            material.color = color;
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.28f);
        }

        if (emission > 0f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
        }

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.renderQueue = 3000;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string full = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(full))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void RepositionGameplayObjects(Transform sceneRoot)
    {
        SetTransform("PlayerSpawn", new Vector3(0f, 0.05f, -35f), Quaternion.Euler(0f, 0f, 0f), null);
        SetTransform("REPLACE_Player_Character", new Vector3(0f, 1f, -35f), Quaternion.Euler(0f, 0f, 0f), null);
        SetTransform("REPLACE_Item_HistoricalMap", new Vector3(-7f, 0.65f, -24.5f), Quaternion.Euler(0f, 20f, 0f), null);
        SetTransform("REPLACE_NPC_OldTourGuide", new Vector3(-6f, 1f, 19.5f), Quaternion.Euler(0f, 180f, 0f), null);
        SetTransform("REPLACE_Puzzle_Radio1975", new Vector3(6f, 1f, 19.3f), Quaternion.Euler(0f, 180f, 0f), null);
        SetTransform("REPLACE_BusStop_ReturnHub", new Vector3(20f, 1.2f, 13.5f), Quaternion.Euler(0f, -70f, 0f), null);

        RefitGameplayCollider("REPLACE_Item_HistoricalMap", 1.6f, 1.2f, 1.6f);
        RefitGameplayCollider("REPLACE_Puzzle_Radio1975", 2.2f, 2f, 1.8f);
        RefitGameplayCollider("REPLACE_BusStop_ReturnHub", 2.8f, 3f, 2.2f);
    }

    private static void RepairGameplayReferences(Transform sceneRoot)
    {
        MemoryZoneController zone = UnityEngine.Object.FindFirstObjectByType<MemoryZoneController>();
        if (zone != null)
        {
            zone.busStopReturn = FindSceneObject("REPLACE_BusStop_ReturnHub");
            MaterialRestoreEffect materialEffect = UnityEngine.Object.FindFirstObjectByType<MaterialRestoreEffect>();
            if (materialEffect != null)
            {
                List<Renderer> renderers = new List<Renderer>();
                Transform importedRoot = FindSceneTransform("DinhDocLap_ImportedLandmarkRoot");
                if (importedRoot != null)
                {
                    renderers.AddRange(importedRoot.GetComponentsInChildren<Renderer>(true));
                }

                foreach (string gameplayName in new[] { "REPLACE_NPC_OldTourGuide", "REPLACE_Item_HistoricalMap", "REPLACE_Puzzle_Radio1975" })
                {
                    GameObject gameplayObject = FindSceneObject(gameplayName);
                    if (gameplayObject != null)
                    {
                        renderers.AddRange(gameplayObject.GetComponentsInChildren<Renderer>(true));
                    }
                }

                materialEffect.renderers = renderers.ToArray();
                materialEffect.grayColor = new Color(0.46f, 0.48f, 0.49f);
                materialEffect.preserveRendererColors = true;
                materialEffect.grayBlend = 0.25f;
                EditorUtility.SetDirty(materialEffect);
            }
            EditorUtility.SetDirty(zone);
        }

        DinhDocLapSceneController controller = UnityEngine.Object.FindFirstObjectByType<DinhDocLapSceneController>();
        if (controller != null)
        {
            controller.historicalMapItem = FindComponent<DinhDocLapMapItemInteractable>("REPLACE_Item_HistoricalMap");
            controller.tourGuideNpc = FindComponent<NPCInteractable>("REPLACE_NPC_OldTourGuide");
            controller.radioPuzzle = FindComponent<PuzzleInteractable>("REPLACE_Puzzle_Radio1975");
            controller.returnBusStop = FindComponent<BusStopInteractable>("REPLACE_BusStop_ReturnHub");
            EditorUtility.SetDirty(controller);
        }

        PuzzleInteractable puzzle = FindComponent<PuzzleInteractable>("REPLACE_Puzzle_Radio1975");
        if (puzzle != null)
        {
            puzzle.correctAnswer = "1975";
            puzzle.memoryZone = zone;
            EditorUtility.SetDirty(puzzle);
        }

        BusStopInteractable busStop = FindComponent<BusStopInteractable>("REPLACE_BusStop_ReturnHub");
        if (busStop != null)
        {
            busStop.currentZone = zone;
            busStop.targetScene = "Scene_00_BusHub";
            EditorUtility.SetDirty(busStop);
        }
    }

    private static Bounds CalculateBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void DisableModelColliders(Transform root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
            EditorUtility.SetDirty(collider);
        }
    }

    private static void RefitGameplayCollider(string objectName, float sizeX, float sizeY, float sizeZ)
    {
        GameObject item = FindSceneObject(objectName);
        if (item == null)
        {
            return;
        }

        BoxCollider box = item.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = item.AddComponent<BoxCollider>();
        }

        box.size = new Vector3(sizeX, sizeY, sizeZ);
        box.center = new Vector3(0f, sizeY * 0.5f - 0.1f, 0f);
        box.isTrigger = false;
        EditorUtility.SetDirty(box);
    }

    private static void SetTransform(string objectName, Vector3 position, Quaternion rotation, Vector3? scale)
    {
        GameObject item = FindSceneObject(objectName);
        if (item == null)
        {
            Debug.LogWarning("[KyUcSaiGon] Missing Dinh Doc Lap object: " + objectName);
            return;
        }

        item.transform.position = position;
        item.transform.rotation = rotation;
        if (scale.HasValue)
        {
            item.transform.localScale = scale.Value;
        }

        EditorUtility.SetDirty(item.transform);
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject item = FindSceneObject(objectName);
        return item != null ? item.GetComponent<T>() : null;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject activeObject = GameObject.Find(objectName);
        if (activeObject != null)
        {
            return activeObject;
        }

        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate.name == objectName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    private static Transform FindSceneTransform(string objectName)
    {
        GameObject item = FindSceneObject(objectName);
        return item != null ? item.transform : null;
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child : CreateChild(parent, name);
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child.transform;
    }

    private static GameObject FindOrCreateRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        return root != null ? root : new GameObject(name);
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(root.GetChild(i).gameObject);
        }
    }
}
#endif
