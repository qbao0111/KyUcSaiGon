using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BusHubRouteBoardController : MonoBehaviour
{
    private struct RouteData
    {
        public string objectName;
        public string displayName;
        public string subtitle;
        public LocationId locationId;
        public string sceneName;
        public Vector3 localPosition;

        public RouteData(string objectName, string displayName, string subtitle, LocationId locationId, string sceneName, Vector3 localPosition)
        {
            this.objectName = objectName;
            this.displayName = displayName;
            this.subtitle = subtitle;
            this.locationId = locationId;
            this.sceneName = sceneName;
            this.localPosition = localPosition;
        }
    }

    private static readonly RouteData[] Routes =
    {
        new RouteData("RouteButton_NguyenHue", "Nguyễn Huệ", "Nhịp sống trẻ", LocationId.NguyenHue, SceneLoader.NguyenHue, new Vector3(-1.95f, 3.6f, 14.28f)),
        new RouteData("RouteButton_BenThanh", "Chợ Bến Thành", "Đời sống thường ngày", LocationId.BenThanh, SceneLoader.BenThanh, new Vector3(0.5f, 3.6f, 14.28f)),
        new RouteData("RouteButton_DinhDocLap", "Dinh Độc Lập", "Lịch sử", LocationId.DinhDocLap, SceneLoader.DinhDocLap, new Vector3(2.95f, 3.6f, 14.28f)),
        new RouteData("RouteButton_NhaThoDucBa", "Nhà thờ\nĐức Bà", "Bình yên", LocationId.NhaThoDucBa, SceneLoader.NhaThoDucBa, new Vector3(-1.95f, 2.35f, 14.28f)),
        new RouteData("RouteButton_Bitexco", "Bitexco", "Chuyển mình", LocationId.Bitexco, SceneLoader.Bitexco, new Vector3(0.5f, 2.35f, 14.28f)),
        new RouteData("RouteButton_BachDang", "Bến Bạch\nĐằng", "Dòng chảy thành phố", LocationId.BachDang, SceneLoader.BachDang, new Vector3(2.95f, 2.35f, 14.28f))
    };

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

        boardCollider.center = new Vector3(0.5f, 2.9f, 14.25f) - transform.position;
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
        RemoveGeneratedButtons();

        foreach (RouteData route in Routes)
        {
            CreateRouteButton(transform, route);
        }

        CreateDeveloperTools();
        PrototypeLogger.Info("BusHub route board rebuilt: 6 memory routes + separated dev tools.");
    }

    private void RemoveGeneratedButtons()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("RouteButton_") || child.name == "DevToolsRoot")
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void CreateDeveloperTools()
    {
        GameObject devRoot = new GameObject("DevToolsRoot");
        devRoot.transform.SetParent(transform);
        devRoot.transform.localPosition = Vector3.zero;
        devRoot.transform.localRotation = Quaternion.identity;
        devRoot.transform.localScale = Vector3.one;

        GameObject devButton = CreateButtonRoot(devRoot.transform, "DevButton_Ending", new Vector3(0.5f, 1.25f, 14.28f), new Vector3(2.8f, 0.75f, 0.35f), new Color(0.35f, 0.45f, 0.95f));
        devButton.SetActive(DeveloperMode.IsEnabled);
        CreateButtonText(devButton.transform, "DEV: Ending");
    }

    private void CreateRouteButton(Transform parent, RouteData route)
    {
        GameObject button = CreateButtonRoot(parent, route.objectName, route.localPosition, new Vector3(2.25f, 0.85f, 0.35f), new Color(1f, 0.87f, 0.08f));
        CreateButtonText(button.transform, route.displayName);
    }

    private GameObject CreateButtonRoot(Transform parent, string objectName, Vector3 localPosition, Vector3 size, Color color)
    {
        GameObject button = new GameObject(objectName);
        button.transform.SetParent(parent);
        button.transform.localPosition = localPosition;
        button.transform.localRotation = Quaternion.identity;
        button.transform.localScale = Vector3.one;

        BoxCollider collider = button.AddComponent<BoxCollider>();
        collider.enabled = false;
        collider.size = size + new Vector3(0.25f, 0.25f, 0.35f);
        new GameObject("Collider").transform.SetParent(button.transform);

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual_REPLACE_" + objectName;
        visual.transform.SetParent(button.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = size;
        Destroy(visual.GetComponent<Collider>());

        Renderer renderer = visual.GetComponent<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        renderer.material = new Material(shader);
        renderer.material.color = color;
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", color * 1.15f);

        return button;
    }

    private void CreateButtonText(Transform parent, string title)
    {
        CreateText(parent, "TitleText", title, new Vector3(0f, 0f, -0.23f), 0.055f);
    }

    private TextMesh CreateText(Transform parent, string name, string text, Vector3 localPosition, float characterSize)
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
        textMesh.fontSize = 36;
        textMesh.color = Color.white;
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
