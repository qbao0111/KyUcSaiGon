#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class MeshyAssetGeneratorWindow : EditorWindow
{
    private const string DefaultApiKey = "msy_3KXMnN2cEr3Ogn7jievooodrE1y8PCIzudD5";
    private const string BaseUrl = "https://api.meshy.ai/openapi/v2/text-to-3d";
    private const string SaveFolder = "Assets/Art/Models/NhaThoDucBa";

    private string apiKey = DefaultApiKey;
    private string prompt = "A classical French colonial style building facade, Saigon architecture, detailed windows, architectural model, low-poly 3D game asset";
    private string mode = "preview"; // "preview" or "refine"
    private string previewTaskId = "";
    private string aiModel = "meshy-4"; // "meshy-4", "latest", "meshy-3"
    private string modelType = "lowpoly"; // "lowpoly" or "standard"
    private string topology = "triangle"; // "triangle" or "quad"

    [Serializable]
    public class TaskInfo
    {
        public string id;
        public string prompt;
        public string status;
        public int progress;
        public string glbUrl;
        public string thumbnailUrl;
    }

    private List<TaskInfo> activeTasks = new List<TaskInfo>();
    private Vector2 scrollPosition;
    private bool isQuerying = false;

    [MenuItem("Ky Uc Sai Gon/Setup/Meshy 3D Asset Generator")]
    public static void ShowWindow()
    {
        GetWindow<MeshyAssetGeneratorWindow>("Meshy 3D Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Meshy AI 3D Asset Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        apiKey = EditorGUILayout.TextField("API Key", apiKey);
        aiModel = EditorGUILayout.TextField("AI Model", aiModel);

        EditorGUILayout.Space();
        string[] modelTypes = { "lowpoly", "standard" };
        int modelTypeIndex = Array.IndexOf(modelTypes, modelType);
        if (modelTypeIndex < 0) modelTypeIndex = 0;
        modelTypeIndex = EditorGUILayout.Popup("Model Type", modelTypeIndex, modelTypes);
        modelType = modelTypes[modelTypeIndex];

        string[] topologies = { "triangle", "quad" };
        int topologyIndex = Array.IndexOf(topologies, topology);
        if (topologyIndex < 0) topologyIndex = 0;
        topologyIndex = EditorGUILayout.Popup("Topology", topologyIndex, topologies);
        topology = topologies[topologyIndex];

        EditorGUILayout.Space();
        GUILayout.Label("Prompt (Describe the 3D asset you want):", EditorStyles.label);
        prompt = EditorGUILayout.TextArea(prompt, GUILayout.Height(60));

        EditorGUILayout.Space();
        string[] modes = { "preview", "refine" };
        int modeIndex = Array.IndexOf(modes, mode);
        if (modeIndex < 0) modeIndex = 0;
        modeIndex = EditorGUILayout.Popup("Mode", modeIndex, modes);
        mode = modes[modeIndex];

        if (mode == "refine")
        {
            previewTaskId = EditorGUILayout.TextField("Preview Task ID", previewTaskId);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate 3D Asset", GUILayout.Height(35)))
        {
            StartGenerationTask();
        }

        EditorGUILayout.Space();
        GUILayout.Label("Active Tasks & Results", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        if (activeTasks.Count == 0)
        {
            GUILayout.Label("No active tasks. Press Generate to start.", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < activeTasks.Count; i++)
            {
                var task = activeTasks[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Prompt: {task.prompt}", EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField($"Task ID: {task.id}");
                EditorGUILayout.LabelField($"Status: {task.status} ({task.progress}%)");

                if (task.status == "SUCCEEDED" && !string.IsNullOrEmpty(task.glbUrl))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Download & Import GLB", GUILayout.Width(200)))
                    {
                        DownloadAndImportGlb(task);
                    }
                    if (GUILayout.Button("Refine Texture", GUILayout.Width(120)))
                    {
                        mode = "refine";
                        previewTaskId = task.id;
                        Repaint();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("Poll Task Statuses", GUILayout.Height(25)))
        {
            PollActiveTasks();
        }
    }

    private async void StartGenerationTask()
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a valid API Key.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(prompt))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a prompt.", "OK");
            return;
        }

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                string jsonPayload = "";
                if (mode == "preview")
                {
                    jsonPayload = $"{{\"mode\":\"preview\",\"prompt\":\"{prompt.Replace("\"", "\\\"")}\",\"ai_model\":\"{aiModel}\",\"model_type\":\"{modelType}\",\"topology\":\"{topology}\"}}";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(previewTaskId))
                    {
                        EditorUtility.DisplayDialog("Error", "Preview Task ID is required for Refine mode.", "OK");
                        return;
                    }
                    jsonPayload = $"{{\"mode\":\"refine\",\"preview_task_id\":\"{previewTaskId}\"}}";
                }

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(BaseUrl, content);
                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var res = JsonUtility.FromJson<MeshyCreateResponse>(responseText);
                    string taskId = !string.IsNullOrEmpty(res.id) ? res.id : res.result;

                    if (string.IsNullOrEmpty(taskId))
                    {
                        Debug.LogError("[Meshy] Failed to parse Task ID from: " + responseText);
                        return;
                    }

                    var newTask = new TaskInfo
                    {
                        id = taskId,
                        prompt = prompt,
                        status = "QUEUED",
                        progress = 0
                    };
                    activeTasks.Add(newTask);
                    Debug.Log($"[Meshy] Started {mode} task: {taskId} for prompt: {prompt}");
                    PollActiveTasks();
                }
                else
                {
                    Debug.LogError("[Meshy] POST Error: " + responseText);
                    EditorUtility.DisplayDialog("API Error", responseText, "OK");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void PollActiveTasks()
    {
        if (isQuerying || activeTasks.Count == 0) return;
        isQuerying = true;

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                for (int i = 0; i < activeTasks.Count; i++)
                {
                    var task = activeTasks[i];
                    if (task.status == "SUCCEEDED" || task.status == "FAILED") continue;

                    string url = $"{BaseUrl}/{task.id}";
                    var response = await client.GetAsync(url);
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var res = JsonUtility.FromJson<MeshyTaskResponse>(responseText);
                        task.status = res.status;
                        task.progress = res.progress;
                        if (res.model_urls != null && !string.IsNullOrEmpty(res.model_urls.glb))
                        {
                            task.glbUrl = res.model_urls.glb;
                        }
                        if (!string.IsNullOrEmpty(res.thumbnail_url))
                        {
                            task.thumbnailUrl = res.thumbnail_url;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[Meshy] GET Error for {task.id}: {responseText}");
                    }
                }
            }
            Repaint();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            isQuerying = false;
        }
    }

    private void DownloadAndImportGlb(TaskInfo task)
    {
        string safePrompt = task.prompt.ToLower();
        // Remove special chars and spaces
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            safePrompt = safePrompt.Replace(c, '_');
        }
        safePrompt = safePrompt.Replace(' ', '_');
        if (safePrompt.Length > 24) safePrompt = safePrompt.Substring(0, 24);
        string filename = $"M_Meshy_{safePrompt}_{task.id.Substring(0, 6)}.glb";

        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        string localPath = Path.Combine(SaveFolder, filename);
        string absolutePath = Path.GetFullPath(localPath);

        Debug.Log($"[Meshy] Downloading {task.glbUrl} to {localPath}...");

        var request = UnityWebRequest.Get(task.glbUrl);
        request.downloadHandler = new DownloadHandlerFile(absolutePath);
        var operation = request.SendWebRequest();

        operation.completed += (op) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[Meshy] Successfully downloaded and imported: {localPath}");
                AssetDatabase.Refresh();

                // Instantiate in scene
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
                if (prefab != null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    instance.name = $"Meshy_{safePrompt}";
                    Selection.activeGameObject = instance;
                    Undo.RegisterCreatedObjectUndo(instance, "Create Meshy Asset");
                    Debug.Log($"[Meshy] Instantiated {instance.name} in current scene.");
                }
            }
            else
            {
                Debug.LogError($"[Meshy] Download failed: {request.error}");
            }
            request.Dispose();
        };
    }

    [Serializable]
    private class MeshyCreateResponse
    {
        public string id;
        public string result;
    }

    [Serializable]
    private class MeshyTaskResponse
    {
        public string id;
        public string status;
        public int progress;
        public MeshyModelUrls model_urls;
        public string thumbnail_url;
    }

    [Serializable]
    private class MeshyModelUrls
    {
        public string glb;
        public string fbx;
        public string obj;
    }
}
#endif