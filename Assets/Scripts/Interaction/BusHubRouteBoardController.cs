using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BusHubRouteBoardController : MonoBehaviour
{
    private const string WorldBoardRootName = "BusHubWorldBoardRoot";
    private const string PanoramaResourcePath = "BusHub/ho-chi-minh-city";
    private bool endingStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        TryInstallForCurrentScene();
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneLoader.BusHub)
        {
            TryInstallForCurrentScene();
        }
    }

    private static void TryInstallForCurrentScene()
    {
        if (SceneManager.GetActiveScene().name != SceneLoader.BusHub)
        {
            return;
        }

        GameObject boardRoot = GameObject.Find("RouteMapBoardRoot");
        if (boardRoot == null || boardRoot.GetComponent<BusHubRouteBoardController>() != null)
        {
            return;
        }

        boardRoot.AddComponent<BusHubRouteBoardController>();
    }

    private void Start()
    {
        EnsureMapBoardInteractable();
        RebuildBoard();
        CheckNormalEndingUnlock();
    }

    private void EnsureMapBoardInteractable()
    {
        BoxCollider boardCollider = GetComponent<BoxCollider>();
        if (boardCollider == null)
        {
            boardCollider = gameObject.AddComponent<BoxCollider>();
        }

        boardCollider.center = transform.InverseTransformPoint(new Vector3(0.5f, 2.9f, 14.25f));
        boardCollider.size = new Vector3(8.4f, 4.7f, 1.1f);

        BusHubMapUIController routeMapUI = FindFirstObjectByType<BusHubMapUIController>();
        if (routeMapUI == null)
        {
            routeMapUI = gameObject.AddComponent<BusHubMapUIController>();
        }

        BusHubMapBoardInteractable boardInteractable = GetComponent<BusHubMapBoardInteractable>();
        if (boardInteractable == null)
        {
            boardInteractable = gameObject.AddComponent<BusHubMapBoardInteractable>();
        }

        boardInteractable.routeMapUI = routeMapUI;
    }

    private void RebuildBoard()
    {
        RemoveOldWorldRouteVisuals();
        RemoveStaleBoardTexts();
        CreatePanoramaBoard();
        PrototypeLogger.Info("BusHub physical board now uses HCM panorama image. Press E opens the paper map UI.");
    }

    private void RemoveOldWorldRouteVisuals()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("RouteButton_")
                || child.name == "DevToolsRoot"
                || child.name == WorldBoardRootName)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void RemoveStaleBoardTexts()
    {
        TextMesh[] textMeshes = GetComponentsInChildren<TextMesh>(true);
        foreach (TextMesh textMesh in textMeshes)
        {
            if (IsRedundantBoardText(textMesh.text))
            {
                Destroy(textMesh.gameObject);
            }
        }

        TMP_Text[] tmpTexts = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text tmpText in tmpTexts)
        {
            if (IsRedundantBoardText(tmpText.text))
            {
                Destroy(tmpText.gameObject);
            }
        }
    }

    private bool IsRedundantBoardText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.Contains("Bảng lộ trình")
            || text.Contains("Chọn địa điểm")
            || text.Contains("Nhấn E để mở")
            || text.Contains("Nhan E de mo");
    }

    private void CreatePanoramaBoard()
    {
        GameObject boardRoot = new GameObject(WorldBoardRootName);
        boardRoot.transform.SetParent(transform);
        boardRoot.transform.localPosition = Vector3.zero;
        boardRoot.transform.localRotation = Quaternion.identity;
        boardRoot.transform.localScale = Vector3.one;

        Texture2D panoramaTexture = Resources.Load<Texture2D>(PanoramaResourcePath);
        // Keep the physical board close to the 16:9 photo aspect ratio so the image
        // stays readable instead of being stretched across the route board.
        GameObject panoramaPanel = CreateCube(boardRoot.transform, "Visual_REPLACE_BusHub_HCMPanorama", new Vector3(0.5f, 2.95f, 14.2f), new Vector3(7.25f, 4.08f, 0.16f), new Color(0.035f, 0.04f, 0.045f));
        if (panoramaTexture != null)
        {
            Renderer panelRenderer = panoramaPanel.GetComponent<Renderer>();
            panelRenderer.material = CreatePanoramaMaterial(panoramaTexture);
        }

        GameObject borderRoot = new GameObject("BoardGoldBorder");
        borderRoot.transform.SetParent(boardRoot.transform);
        borderRoot.transform.localPosition = Vector3.zero;
        borderRoot.transform.localRotation = Quaternion.identity;
        borderRoot.transform.localScale = Vector3.one;

        Color frameDark = new Color(0.055f, 0.045f, 0.035f);
        Color bronze = new Color(0.72f, 0.42f, 0.16f);

        CreateCube(borderRoot.transform, "Frame_Top", new Vector3(0.5f, 5.08f, 14.02f), new Vector3(7.58f, 0.14f, 0.08f), frameDark);
        CreateCube(borderRoot.transform, "Frame_Bottom", new Vector3(0.5f, 0.82f, 14.02f), new Vector3(7.58f, 0.14f, 0.08f), frameDark);
        CreateCube(borderRoot.transform, "Frame_Left", new Vector3(-3.33f, 2.95f, 14.02f), new Vector3(0.14f, 4.4f, 0.08f), frameDark);
        CreateCube(borderRoot.transform, "Frame_Right", new Vector3(4.33f, 2.95f, 14.02f), new Vector3(0.14f, 4.4f, 0.08f), frameDark);

        CreateCube(borderRoot.transform, "BronzeLine_Top", new Vector3(0.5f, 5f, 13.98f), new Vector3(7.38f, 0.035f, 0.045f), bronze);
        CreateCube(borderRoot.transform, "BronzeLine_Bottom", new Vector3(0.5f, 0.9f, 13.98f), new Vector3(7.38f, 0.035f, 0.045f), bronze);
        CreateCube(borderRoot.transform, "BronzeLine_Left", new Vector3(-3.22f, 2.95f, 13.98f), new Vector3(0.035f, 4.14f, 0.045f), bronze);
        CreateCube(borderRoot.transform, "BronzeLine_Right", new Vector3(4.22f, 2.95f, 13.98f), new Vector3(0.035f, 4.14f, 0.045f), bronze);

        if (panoramaTexture == null)
        {
            CreateBoardText(boardRoot.transform, "PlaceholderText", "Ảnh toàn cảnh TP.HCM", new Vector3(0.5f, 2.95f, 13.98f), 0.14f, new Color(1f, 0.72f, 0.32f));
            PrototypeLogger.Warning("BusHub panorama texture not found at Resources/" + PanoramaResourcePath + ".");
        }
    }

    private GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color, bool emissive = false)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.localPosition = localPosition;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = localScale;
        Destroy(cube.GetComponent<Collider>());

        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.material = CreateMaterial(color, emissive);
        return cube;
    }

    private Material CreateMaterial(Color color, bool emissive)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;

        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.2f);
        }

        return material;
    }

    private Material CreatePanoramaMaterial(Texture2D texture)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = "Runtime_M_BusHub_HCMPanorama";
        material.color = Color.white;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.anisoLevel = 8;
        texture.mipMapBias = -0.75f;
        material.mainTexture = texture;
        material.mainTextureScale = new Vector2(-1f, -1f);
        material.mainTextureOffset = new Vector2(1f, 1f);

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", new Vector2(-1f, -1f));
            material.SetTextureOffset("_BaseMap", new Vector2(1f, 1f));
            material.SetColor("_BaseColor", Color.white);
        }

        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", Color.white * 0.15f);
        return material;
    }

    private TextMesh CreateBoardText(Transform parent, string name, string text, Vector3 localPosition, float characterSize, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 48;
        textMesh.color = color;
        return textMesh;
    }

    private void CheckNormalEndingUnlock()
    {
        GameProgressManager progress = GameProgressManager.Instance;
        if (DeveloperMode.IsEnabled || progress == null || endingStarted || !progress.AreAllMemoriesRestored())
        {
            return;
        }

        progress.endingUnlocked = true;
        endingStarted = true;
        StartCoroutine(LoadEndingAfterDelay());
    }

    private IEnumerator LoadEndingAfterDelay()
    {
        UIManager.Instance?.ShowDialogue("2 ký ức quan trọng đã hội tụ. Xe buýt sẽ khởi hành chuyến cuối.");
        yield return new WaitForSeconds(3f);
        SceneLoader.Load(SceneLoader.Ending);
    }
}
