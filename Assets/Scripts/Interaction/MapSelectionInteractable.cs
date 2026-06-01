using UnityEngine;
using System.Reflection;

public class MapSelectionInteractable : MonoBehaviour, IInteractable
{
    public LocationId targetLocation;
    public string targetScene;
    public string displayName = "Route";
    public bool isEndingRoute;
    public Renderer statusRenderer;
    public Component statusLabel;
    public bool allowRevisit = true;
    public float routeLabelVisibleDistance = 7f;

    public string InteractionPrompt => "Nhấn E để chọn địa điểm";

    private void Start()
    {
        RefreshVisualState();
        RefreshLabelVisibility();
    }

    private void Update()
    {
        RefreshLabelVisibility();
    }

    public void Interact(Interactor interactor)
    {
        GameProgressManager progress = GameProgressManager.Instance;
        bool developerMode = DeveloperMode.IsEnabled;
        PrototypeLogger.Info("Route interact: " + displayName + " -> " + targetScene);

        if (isEndingRoute)
        {
            if (developerMode || (progress != null && progress.endingUnlocked))
            {
                PrototypeLogger.Info("Loading ending route.");
                SceneLoader.Load(SceneLoader.Ending);
            }
            else
            {
                UIManager.Instance?.ShowDialogue("Can du 6 manh ky uc de mo chuyen xe cuoi.");
            }

            return;
        }

        if (!developerMode && targetLocation != LocationId.NguyenHue && progress != null && !progress.IsRestored(LocationId.NguyenHue))
        {
            UIManager.Instance?.ShowDialogue("Hay hoan thanh Nguyen Hue truoc de mo cac tuyen xe khac.");
            return;
        }

        if (!developerMode && progress != null && progress.IsRestored(targetLocation) && !allowRevisit)
        {
            UIManager.Instance?.ShowDialogue(displayName + " đã khôi phục.");
            return;
        }

        SceneLoader.Load(targetScene);
    }

    public void RefreshVisualState()
    {
        if (statusRenderer == null)
        {
            statusRenderer = GetComponentInChildren<Renderer>();
        }

        if (statusRenderer == null || GameProgressManager.Instance == null)
        {
            return;
        }

        bool completed = targetLocation != LocationId.None && GameProgressManager.Instance.IsRestored(targetLocation);
        bool dev = DeveloperMode.IsEnabled;
        bool unlocked = dev || (isEndingRoute ? GameProgressManager.Instance.endingUnlocked : targetLocation == LocationId.NguyenHue || GameProgressManager.Instance.IsRestored(LocationId.NguyenHue));
        statusRenderer.material.color = completed ? Color.green : unlocked ? Color.yellow : Color.gray;

        string status = completed ? "Đã khôi phục" : unlocked ? (dev ? "Mở khóa (Dev)" : "Chưa khôi phục") : "Chưa mở khóa";
        SetStatusLabel(displayName + "\n" + status);
    }

    private void SetStatusLabel(string text)
    {
        if (statusLabel == null)
        {
            return;
        }

        if (statusLabel is TextMesh textMesh)
        {
            textMesh.text = text;
            return;
        }

        PropertyInfo textProperty = statusLabel.GetType().GetProperty("text");
        if (textProperty != null && textProperty.CanWrite)
        {
            textProperty.SetValue(statusLabel, text);
        }
    }

    private void RefreshLabelVisibility()
    {
        if (statusLabel == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        bool isNearby = mainCamera != null && Vector3.Distance(mainCamera.transform.position, transform.position) <= routeLabelVisibleDistance;
        statusLabel.gameObject.SetActive(isNearby);
    }
}
