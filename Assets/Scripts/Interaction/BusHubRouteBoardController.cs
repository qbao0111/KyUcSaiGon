using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BusHubRouteBoardController : MonoBehaviour
{
    private const string WorldBoardRootName = "BusHubWorldBoardRoot";
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

        BusHubMapUIController routeMapUI = FindObjectOfType<BusHubMapUIController>();
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
        CreateBlankPanoramaBoard();
        PrototypeLogger.Info("BusHub physical board is now a blank HCM panorama placeholder. Press E opens the paper map UI.");
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

    private void CreateBlankPanoramaBoard()
    {
        GameObject boardRoot = new GameObject(WorldBoardRootName);
        boardRoot.transform.SetParent(transform);
        boardRoot.transform.localPosition = Vector3.zero;
        boardRoot.transform.localRotation = Quaternion.identity;
        boardRoot.transform.localScale = Vector3.one;

        CreateCube(boardRoot.transform, "BlankPanoramaPanel", new Vector3(0.5f, 2.95f, 14.2f), new Vector3(8.25f, 3.85f, 0.16f), new Color(0.035f, 0.04f, 0.045f));

        GameObject borderRoot = new GameObject("BoardGoldBorder");
        borderRoot.transform.SetParent(boardRoot.transform);
        borderRoot.transform.localPosition = Vector3.zero;
        borderRoot.transform.localRotation = Quaternion.identity;
        borderRoot.transform.localScale = Vector3.one;

        Color gold = new Color(1f, 0.58f, 0.12f);
        CreateCube(borderRoot.transform, "Border_Top", new Vector3(0.5f, 4.94f, 14.02f), new Vector3(8.55f, 0.16f, 0.08f), gold, true);
        CreateCube(borderRoot.transform, "Border_Bottom", new Vector3(0.5f, 0.99f, 14.02f), new Vector3(8.55f, 0.16f, 0.08f), gold, true);
        CreateCube(borderRoot.transform, "Border_Left", new Vector3(-3.85f, 2.95f, 14.02f), new Vector3(0.16f, 4f, 0.08f), gold, true);
        CreateCube(borderRoot.transform, "Border_Right", new Vector3(4.85f, 2.95f, 14.02f), new Vector3(0.16f, 4f, 0.08f), gold, true);

        CreateBoardText(boardRoot.transform, "PlaceholderText", "Ảnh toàn cảnh TP.HCM", new Vector3(0.5f, 2.95f, 13.98f), 0.14f, new Color(1f, 0.72f, 0.32f));
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
        UIManager.Instance?.ShowDialogue("6 mảnh ký ức đã hội tụ. Xe buýt sẽ khởi hành chuyến cuối.");
        yield return new WaitForSeconds(3f);
        SceneLoader.Load(SceneLoader.Ending);
    }
}
