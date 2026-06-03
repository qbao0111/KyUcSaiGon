using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RouteMapNodeUI : MonoBehaviour
{
    public string displayName;
    public string subtitle;
    public string description;
    public string sceneName;
    public LocationId locationId;

    public Button button;
    public Image iconFrameImage;
    public Image landmarkIconImage;
    public Image selectionOutline;
    public Image selectionGlow;
    public Image restoredBadge;
    public Image lockOverlay;
    public TMP_Text restoredBadgeText;
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public TMP_Text statusText;

    public bool IsRestored { get; private set; }

    private Vector3 baseScale = Vector3.one;

    public void RefreshState(GameProgressManager progress)
    {
        IsRestored = progress != null && progress.IsRestored(locationId);
        Color accent = IsRestored
            ? new Color(0.48f, 0.62f, 0.18f, 1f)
            : new Color(1f, 0.63f, 0.08f, 1f);

        if (iconFrameImage != null)
        {
            iconFrameImage.color = accent;
        }

        if (landmarkIconImage != null)
        {
            Color iconColor = landmarkIconImage.color;
            iconColor.a = 1f;
            landmarkIconImage.color = iconColor;
        }

        if (restoredBadge != null)
        {
            restoredBadge.gameObject.SetActive(IsRestored);
        }

        if (restoredBadgeText != null)
        {
            restoredBadgeText.text = "✓";
        }

        RefreshText();
        SetLocked(false);
    }

    public void RefreshState(GameProgressManager progress, bool developerMode)
    {
        RefreshState(progress);
    }

    public void SetRestored(bool restored)
    {
        IsRestored = restored;
        if (restoredBadge != null)
        {
            restoredBadge.gameObject.SetActive(restored);
        }

        if (statusText != null)
        {
            statusText.text = restored ? "Đã khôi phục" : "Chưa khôi phục";
        }
    }

    public void SetLocked(bool locked)
    {
        if (lockOverlay != null)
        {
            lockOverlay.gameObject.SetActive(locked);
        }

        if (landmarkIconImage != null)
        {
            Color iconColor = landmarkIconImage.color;
            iconColor.a = locked ? 0.45f : 1f;
            landmarkIconImage.color = iconColor;
        }

        if (statusText != null && locked)
        {
            statusText.text = "Chưa mở khóa";
        }
    }

    private void RefreshText()
    {
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
            statusText.text = IsRestored ? "Đã khôi phục" : "Chưa khôi phục";
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionOutline != null)
        {
            selectionOutline.enabled = selected;
        }

        if (selectionGlow != null)
        {
            selectionGlow.enabled = selected;
        }

        transform.localScale = selected ? baseScale * 1.12f : baseScale;
    }

    public void Confirm()
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneLoader.Load(sceneName);
        }
    }
}
