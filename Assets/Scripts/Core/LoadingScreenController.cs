using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressBar;
    public Text progressText;
    public Text tipText;

    [Header("Loading Backgrounds")]
    public Sprite nguyenHueLoadingBackground;
    public Sprite ducBaLoadingBackground;
    public Sprite endingLoadingBackground;
    public Sprite fallbackLoadingBackground;

    private Image runtimeBackgroundImage;

    private void Start()
    {
        // Make sure cursor is visible/unlocked on loading screen
        CursorLockManager.UnlockForUI();

        string target = SceneLoader.TargetScene;
        if (string.IsNullOrEmpty(target))
        {
            target = SceneLoader.BusHub; // Fallback to Bus Hub
        }

        BuildPolishedLoadingUI(target);

        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        StartCoroutine(LoadSceneAsyncRoutine(target));
    }

    private void BuildPolishedLoadingUI(string targetScene)
    {
        Sprite backgroundSprite = GetLoadingBackground(targetScene);
        if (backgroundSprite == null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject oldRoot = GameObject.Find("PolishedLoadingRoot");
        if (oldRoot != null)
        {
            Destroy(oldRoot);
        }

        GameObject root = new GameObject("PolishedLoadingRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        runtimeBackgroundImage = CreateImage(root.transform, "LoadingBackground", backgroundSprite, Color.white);
        Stretch(runtimeBackgroundImage.rectTransform);
        runtimeBackgroundImage.preserveAspect = true;
        AspectRatioFitter fitter = runtimeBackgroundImage.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = backgroundSprite.rect.width / Mathf.Max(1f, backgroundSprite.rect.height);

        if (tipText != null)
        {
            tipText.gameObject.SetActive(false);
            tipText = null;
        }

        GameObject sliderObject = new GameObject("ProgressSlider", typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(root.transform, false);
        progressBar = sliderObject.GetComponent<Slider>();
        progressBar.transition = Selectable.Transition.None;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0f);
        sliderRect.anchorMax = new Vector2(0.5f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, 70f);
        sliderRect.sizeDelta = new Vector2(1160f, 28f);

        Image track = CreateImage(sliderObject.transform, "Track", null, new Color(0.03f, 0.025f, 0.018f, 0.36f));
        Stretch(track.rectTransform);
        Outline trackOutline = track.gameObject.AddComponent<Outline>();
        trackOutline.effectColor = new Color(0.92f, 0.62f, 0.18f, 0.9f);
        trackOutline.effectDistance = new Vector2(2f, -2f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        Image fill = CreateImage(fillArea.transform, "Fill", null, new Color(1f, 0.62f, 0.13f, 0.95f));
        Stretch(fill.rectTransform);
        progressBar.fillRect = fill.rectTransform;
        progressBar.targetGraphic = fill;

        GameObject progressObject = new GameObject("ProgressText", typeof(RectTransform), typeof(Text));
        progressObject.transform.SetParent(root.transform, false);
        progressText = progressObject.GetComponent<Text>();
        progressText.font = GameUIFont.Regular;
        progressText.fontSize = 24;
        progressText.alignment = TextAnchor.MiddleCenter;
        progressText.color = new Color(0.97f, 0.7f, 0.28f, 1f);
        progressText.horizontalOverflow = HorizontalWrapMode.Overflow;
        RectTransform progressRect = progressText.rectTransform;
        progressRect.anchorMin = new Vector2(0.5f, 0f);
        progressRect.anchorMax = new Vector2(0.5f, 0f);
        progressRect.pivot = new Vector2(0.5f, 0.5f);
        progressRect.anchoredPosition = new Vector2(0f, 42f);
        progressRect.sizeDelta = new Vector2(520f, 38f);
    }

    private Sprite GetLoadingBackground(string targetScene)
    {
        if (targetScene == SceneLoader.NguyenHue)
        {
            return nguyenHueLoadingBackground != null ? nguyenHueLoadingBackground : fallbackLoadingBackground;
        }

        if (targetScene == SceneLoader.NhaThoDucBa)
        {
            return ducBaLoadingBackground != null ? ducBaLoadingBackground : fallbackLoadingBackground;
        }

        if (targetScene == SceneLoader.Ending)
        {
            return endingLoadingBackground != null ? endingLoadingBackground : fallbackLoadingBackground;
        }

        return fallbackLoadingBackground != null ? fallbackLoadingBackground : nguyenHueLoadingBackground;
    }

    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        yield return new WaitForSeconds(0.6f); // Give screen time to render

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float progress = 0f;
        while (!op.isDone)
        {
            // op.progress goes from 0 to 0.9. 0.9 means loading is complete.
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);
            
            // Lerp progress bar smoothly
            while (progress < targetProgress)
            {
                progress += Time.deltaTime * 1.5f; // Smooth transition speed
                if (progressBar != null) progressBar.value = progress;
                if (progressText != null) progressText.text = $"ĐANG TẢI...   {Mathf.RoundToInt(progress * 100)}%";
                yield return null;
            }

            if (op.progress >= 0.9f)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (progressText != null) progressText.text = "ĐANG TẢI...   100%";
                yield return new WaitForSeconds(0.5f); // Short delay to show 100%
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
