using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpeakerMixerPuzzleUI : MonoBehaviour
{
    [System.Serializable]
    public class MixerColumn
    {
        public string label;
        public RectTransform root;
        public TextMeshProUGUI labelText;
        public TextMeshProUGUI valueText;
        public RectTransform knob;
        public Image fillImage;
        public Image glowImage;
        public Button plusButton;
        public Button minusButton;
        public Color accentColor = Color.white;
        public float knobMinY = -46f;
        public float knobMaxY = 46f;
        public float fillMinHeight = 14f;
        public float fillMaxHeight = 104f;
        public float fillBaseY = -52f;
        [Header("Editable Slider Alignment")]
        public bool forceCenteredSliderLayout = true;
        public float sliderCenterX = 0f;
    }

    [Header("Root")]
    public RectTransform panelRoot;
    public CanvasGroup canvasGroup;
    public Image panelImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI warningText;

    [Header("Controls")]
    public MixerColumn[] columns = new MixerColumn[3];
    public Button submitButton;
    public Button closeButton;
    public Button topCloseButton;

    [Header("Animation")]
    public float openDuration = 0.22f;
    public float wrongShakeDuration = 0.28f;
    public float correctGlowDuration = 0.75f;

    private Coroutine[] popRoutines;

    public void Bind(string[] labels, UnityAction<int, int> onAdjust, UnityAction onSubmit, UnityAction onClose)
    {
        if (popRoutines == null || popRoutines.Length != columns.Length)
        {
            popRoutines = new Coroutine[columns.Length];
        }

        for (int i = 0; i < columns.Length; i++)
        {
            int index = i;
            MixerColumn column = columns[i];
            if (column == null)
            {
                continue;
            }

            string label = labels != null && i < labels.Length ? labels[i] : "Value " + (i + 1);
            column.label = label;
            if (column.labelText != null)
            {
                column.labelText.text = label;
            }

            if (column.plusButton != null)
            {
                column.plusButton.onClick.RemoveAllListeners();
                column.plusButton.onClick.AddListener(() => onAdjust?.Invoke(index, 1));
            }

            if (column.minusButton != null)
            {
                column.minusButton.onClick.RemoveAllListeners();
                column.minusButton.onClick.AddListener(() => onAdjust?.Invoke(index, -1));
            }
        }

        BindButton(submitButton, onSubmit);
        BindButton(closeButton, onClose);
        BindButton(topCloseButton, onClose);
        ShowWarning(string.Empty, Color.white);
    }

    public void Refresh(int[] values, int selectedIndex)
    {
        if (values == null)
        {
            return;
        }

        for (int i = 0; i < columns.Length && i < values.Length; i++)
        {
            MixerColumn column = columns[i];
            if (column == null)
            {
                continue;
            }

            float normalized = values[i] / 9f;
            if (column.valueText != null)
            {
                column.valueText.text = values[i].ToString();
                column.valueText.color = column.accentColor;
            }

            if (column.knob != null)
            {
                if (column.forceCenteredSliderLayout)
                {
                    NormalizeSliderRect(column.knob, new Vector2(0.5f, 0.5f));
                }

                float knobX = column.forceCenteredSliderLayout ? column.sliderCenterX : column.knob.anchoredPosition.x;
                column.knob.anchoredPosition = new Vector2(knobX, Mathf.Lerp(column.knobMinY, column.knobMaxY, normalized));
            }

            if (column.fillImage != null)
            {
                RectTransform fillRect = column.fillImage.rectTransform;
                if (column.forceCenteredSliderLayout)
                {
                    NormalizeSliderRect(fillRect, new Vector2(0.5f, 0f));
                }

                float fillHeight = Mathf.Lerp(column.fillMinHeight, column.fillMaxHeight, normalized);
                fillRect.sizeDelta = new Vector2(fillRect.sizeDelta.x, fillHeight);
                float fillX = column.forceCenteredSliderLayout ? column.sliderCenterX : fillRect.anchoredPosition.x;
                fillRect.anchoredPosition = new Vector2(fillX, column.fillBaseY);
                column.fillImage.color = new Color(column.accentColor.r, column.accentColor.g, column.accentColor.b, 0.72f);
            }

            if (column.root != null)
            {
                column.root.localScale = i == selectedIndex ? Vector3.one * 1.035f : Vector3.one;
            }
        }
    }

    public IEnumerator AnimateOpen()
    {
        if (panelRoot == null || canvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        panelRoot.localScale = Vector3.one * 0.9f;
        canvasGroup.alpha = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            canvasGroup.alpha = eased;
            panelRoot.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, eased);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        panelRoot.localScale = Vector3.one;
    }

    public void PopColumn(int index)
    {
        if (index < 0 || index >= columns.Length || columns[index] == null)
        {
            return;
        }

        if (popRoutines == null || popRoutines.Length != columns.Length)
        {
            popRoutines = new Coroutine[columns.Length];
        }

        if (popRoutines[index] != null)
        {
            StopCoroutine(popRoutines[index]);
        }

        popRoutines[index] = StartCoroutine(PopColumnRoutine(index));
    }

    public IEnumerator AnimateWrong(string message)
    {
        ShowWarning(message, new Color(1f, 0.34f, 0.28f, 1f));

        if (panelRoot == null)
        {
            yield break;
        }

        Vector2 original = panelRoot.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < wrongShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float strength = Mathf.Lerp(14f, 0f, elapsed / wrongShakeDuration);
            panelRoot.anchoredPosition = original + new Vector2(Mathf.Sin(elapsed * 90f) * strength, 0f);
            yield return null;
        }

        panelRoot.anchoredPosition = original;
    }

    public IEnumerator AnimateCorrect(string message)
    {
        ShowWarning(message, new Color(0.48f, 1f, 0.36f, 1f));

        if (panelImage == null)
        {
            yield return new WaitForSecondsRealtime(correctGlowDuration);
            yield break;
        }

        Color original = panelImage.color;
        panelImage.color = new Color(0.14f, 0.17f, 0.08f, 0.96f);
        yield return new WaitForSecondsRealtime(correctGlowDuration);
        panelImage.color = original;
    }

    public void ShowWarning(string message, Color color)
    {
        if (warningText == null)
        {
            return;
        }

        warningText.text = message;
        warningText.color = color;
    }

    private IEnumerator PopColumnRoutine(int index)
    {
        MixerColumn column = columns[index];
        if (column == null || column.valueText == null)
        {
            yield break;
        }

        Color originalGlow = column.glowImage != null ? column.glowImage.color : Color.clear;
        float duration = 0.18f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            column.valueText.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.22f, t);
            if (column.glowImage != null)
            {
                column.glowImage.color = new Color(originalGlow.r, originalGlow.g, originalGlow.b, Mathf.Lerp(originalGlow.a, 0.22f, t));
            }

            yield return null;
        }

        column.valueText.rectTransform.localScale = Vector3.one;
        if (column.glowImage != null)
        {
            column.glowImage.color = originalGlow;
        }
    }

    private void BindButton(Button button, UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static void NormalizeSliderRect(RectTransform rect, Vector2 pivot)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = pivot;
    }
}
