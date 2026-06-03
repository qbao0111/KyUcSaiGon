using UnityEngine;
using System.Reflection;

public class MapSelectionInteractable : MonoBehaviour, IInteractable
{
    public LocationId targetLocation;
    public string targetScene;
    public string displayName = "Route";
    public string subtitle;
    public bool isEndingRoute;
    public bool isDeveloperOnly;
    public bool allowInDeveloperMode = true;
    public Renderer statusRenderer;
    public Renderer buttonRenderer;
    public Component statusLabel;
    public TextMesh titleText;
    public TextMesh subtitleText;
    public TextMesh statusText;
    public bool allowRevisit = true;
    public float routeLabelVisibleDistance = 7f;

    public string InteractionPrompt => "Nhấn E để chọn địa điểm";

    private void Start()
    {
        ApplyDeveloperVisibility();
        RefreshVisualState();
        RefreshLabelVisibility();
    }

    private void Update()
    {
        ApplyDeveloperVisibility();
        RefreshLabelVisibility();
    }

    public void Interact(Interactor interactor)
    {
        GameProgressManager progress = GameProgressManager.Instance;
        bool developerMode = DeveloperMode.IsEnabled;
        PrototypeLogger.Info("Route interact: " + displayName + " -> " + targetScene);

        if (isDeveloperOnly)
        {
            if (!developerMode || !allowInDeveloperMode)
            {
                UIManager.Instance?.ShowDialogue("Mục này chỉ dùng cho Developer Test Mode.");
                return;
            }

            SceneLoader.Load(targetScene);
            return;
        }

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

        if (!developerMode && progress != null && progress.IsRestored(targetLocation) && !allowRevisit)
        {
            UIManager.Instance?.ShowDialogue(displayName + " đã khôi phục.");
            return;
        }

        SceneLoader.Load(targetScene);
    }

    public void RefreshVisualState()
    {
        ApplyDeveloperVisibility();
        if (!gameObject.activeSelf)
        {
            return;
        }

        if (statusRenderer == null)
        {
            statusRenderer = buttonRenderer != null ? buttonRenderer : GetComponentInChildren<Renderer>();
        }

        if (statusRenderer == null || GameProgressManager.Instance == null)
        {
            return;
        }

        bool completed = targetLocation != LocationId.None && GameProgressManager.Instance.IsRestored(targetLocation);
        bool dev = DeveloperMode.IsEnabled;
        bool unlocked = dev || isDeveloperOnly || (isEndingRoute ? GameProgressManager.Instance.endingUnlocked : true);
        statusRenderer.material.color = completed ? Color.green : unlocked ? Color.yellow : Color.gray;

        string status = isDeveloperOnly
            ? "DEV ONLY"
            : completed ? "Đã khôi phục" : unlocked ? (dev ? "DEV: Mở khóa để test" : "Chưa khôi phục") : "Chưa mở khóa";

        if (titleText != null)
        {
            titleText.text = displayName;
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
        }

        if (statusText != null)
        {
            statusText.text = status;
        }

        SetStatusLabel(displayName + "\n" + (string.IsNullOrWhiteSpace(subtitle) ? string.Empty : subtitle + "\n") + status);
    }

    private void ApplyDeveloperVisibility()
    {
        if (isDeveloperOnly)
        {
            bool visible = DeveloperMode.IsEnabled && allowInDeveloperMode;
            if (gameObject.activeSelf != visible)
            {
                gameObject.SetActive(visible);
            }
        }
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
        Camera mainCamera = Camera.main;
        bool isNearby = mainCamera != null && Vector3.Distance(mainCamera.transform.position, transform.position) <= routeLabelVisibleDistance;

        SetTextVisible(titleText, isNearby);
        SetTextVisible(subtitleText, isNearby);
        SetTextVisible(statusText, isNearby);

        if (statusLabel != null && statusLabel != titleText && statusLabel != subtitleText && statusLabel != statusText)
        {
            statusLabel.gameObject.SetActive(isNearby);
        }
    }

    private void SetTextVisible(TextMesh textMesh, bool visible)
    {
        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(visible);
        }
    }
}
