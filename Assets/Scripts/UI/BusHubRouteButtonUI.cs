using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BusHubRouteButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string displayName;
    public string sceneName;
    public LocationId locationId;

    public Button button;
    public Image cardImage;
    public Image selectedOutline;
    public Image hoverGlow;
    public GameObject completedBadge;
    public GameObject lockedOverlay;
    public TMP_Text optionalDebugText;

    public float normalScale = 1f;
    public float hoverScale = 1.04f;
    public float selectedScale = 1.08f;
    public float pressedScale = 0.96f;

    public bool IsSelected { get; private set; }
    public bool IsHovered { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsCompleted { get; private set; }

    private Coroutine scaleRoutine;
    private System.Action<BusHubRouteButtonUI> hoverCallback;

    public void Initialize(System.Action<BusHubRouteButtonUI> onHover)
    {
        hoverCallback = onHover;
        SetSelected(false);
        SetHovered(false);
        SetLocked(false);
    }

    public void RefreshState(GameProgressManager progress)
    {
        bool completed = progress != null && progress.IsRestored(locationId);
        bool unlocked = progress == null
            ? locationId == LocationId.NguyenHue
            : progress.IsRouteUnlocked(locationId);

        SetCompleted(completed);
        SetLocked(!unlocked);
        RefreshVisualTint();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selectedOutline != null)
        {
            selectedOutline.gameObject.SetActive(selected);
        }

        if (hoverGlow != null)
        {
            hoverGlow.gameObject.SetActive(selected || IsHovered);
        }

        AnimateScale(selected ? selectedScale : IsHovered ? hoverScale : normalScale);
    }

    public void SetHovered(bool hovered)
    {
        IsHovered = hovered;

        if (hoverGlow != null)
        {
            hoverGlow.gameObject.SetActive(hovered || IsSelected);
        }

        AnimateScale(IsSelected ? selectedScale : hovered ? hoverScale : normalScale);
    }

    public void SetCompleted(bool completed)
    {
        IsCompleted = completed;

        if (completedBadge != null)
        {
            completedBadge.SetActive(completed);
        }

        RefreshVisualTint();
    }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(locked);
        }

        if (button != null)
        {
            // Keep the button clickable so locked routes can show a clear feedback message.
            button.interactable = true;
        }

        RefreshVisualTint();
    }

    public IEnumerator AnimatePressed()
    {
        yield return ScaleTo(pressedScale, 0.06f);
        yield return ScaleTo(IsSelected ? selectedScale : normalScale, 0.08f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHovered(true);
        hoverCallback?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHovered(false);
    }

    private void AnimateScale(float targetScale)
    {
        if (!gameObject.activeInHierarchy)
        {
            transform.localScale = Vector3.one * targetScale;
            return;
        }

        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(ScaleTo(targetScale, 0.11f));
    }

    private void RefreshVisualTint()
    {
        if (cardImage == null)
        {
            return;
        }

        if (IsLocked)
        {
            cardImage.color = new Color(0.42f, 0.42f, 0.42f, 0.78f);
        }
        else if (IsCompleted)
        {
            cardImage.color = new Color(0.92f, 1f, 0.76f, 1f);
        }
        else
        {
            cardImage.color = Color.white;
        }
    }

    private IEnumerator ScaleTo(float targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }
}
