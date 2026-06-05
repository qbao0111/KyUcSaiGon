using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LEDHintInteractable : MonoBehaviour, IInteractable
{
    [TextArea] public string hintMessage;
    public NguyenHueTutorialController tutorialController;
    [Header("LED Display")]
    public string clueLabel;
    public string clueNumber;
    public Color clueColor = Color.white;
    public float blinkSpeed = 0.85f;
    public TextMeshPro labelText;
    public TextMeshPro numberText;
    public Renderer screenRenderer;
    [Header("Editable Layout")]
    public bool autoLayout = true;
    public Vector3 screenLocalPosition = new Vector3(0f, 2.023f, -0.18f);
    public Vector3 screenLocalScale = new Vector3(1.18f, 1.48f, 0.025f);
    public Vector3 labelLocalPosition = new Vector3(0f, 1.98f, -0.215f);
    public Vector3 labelLocalScale = Vector3.one * 0.16f;
    public Vector2 labelSize = new Vector2(5.2f, 1.1f);
    public float labelFontSize = 6.5f;
    public Vector3 numberLocalPosition = new Vector3(0f, 1.27f, -0.225f);
    public Vector3 numberLocalScale = Vector3.one * 0.34f;
    public Vector2 numberSize = new Vector2(3.2f, 2.9f);
    public float numberFontSize = 22f;
    public float screenEmissionIdle = 1.35f;
    public float screenEmissionPulse = 2.6f;

    private const string ScreenRootName = "ScreenRoot";
    private const string LabelName = "TMP_LED_Label";
    private const string NumberName = "TMP_LED_Number";
    private const string ScreenName = "ScreenQuad";

    private Material runtimeScreenMaterial;
    private Transform screenRoot;
    private Color resolvedColor;
    private string resolvedLabel;
    private string resolvedNumber;

    public string InteractionPrompt => "Nhấn E để xem màn hình LED";

    private void OnEnable()
    {
        RefreshDisplay();
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void OnValidate()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        if (!Application.isPlaying || numberText == null)
        {
            return;
        }

        float pulse = 0.45f + Mathf.PingPong(Time.time / Mathf.Max(0.1f, blinkSpeed), 1f) * 0.55f;
        Color numberColor = Color.Lerp(Color.white, resolvedColor, pulse);
        numberColor.a = Mathf.Lerp(0.55f, 1f, pulse);
        numberText.color = numberColor;

        if (screenRenderer != null)
        {
            Material material = screenRenderer.material;
            ApplyEmission(material, resolvedColor, Mathf.Lerp(screenEmissionIdle, screenEmissionPulse, pulse));
        }
    }

    public void Interact(Interactor interactor)
    {
        PrototypeLogger.Info("LED hint inspect: " + name);
        tutorialController?.InspectLedHint(hintMessage);
    }

    public void ConfigureDisplay(string label, string number, Color color)
    {
        clueLabel = label;
        clueNumber = number;
        clueColor = color;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        ResolveClue();
        EnsureScreenPatch();
        EnsureTextObjects();
        ApplyStaticDisplay();
    }

    private void ResolveClue()
    {
        resolvedLabel = clueLabel;
        resolvedNumber = clueNumber;
        resolvedColor = clueColor;

        if (string.IsNullOrWhiteSpace(resolvedLabel) || string.IsNullOrWhiteSpace(resolvedNumber))
        {
            if (name.Contains("Bass"))
            {
                resolvedLabel = "BASS";
                resolvedNumber = "1";
                resolvedColor = new Color(1f, 0.08f, 0.04f);
            }
            else if (name.Contains("Mid"))
            {
                resolvedLabel = "MID";
                resolvedNumber = "6";
                resolvedColor = new Color(0.08f, 1f, 0.28f);
            }
            else if (name.Contains("Treble") || name.Contains("Gold"))
            {
                resolvedLabel = "TREBLE";
                resolvedNumber = "8";
                resolvedColor = new Color(1f, 0.78f, 0.08f);
            }
        }
    }

    private void EnsureScreenPatch()
    {
        screenRoot = EnsureScreenRoot();

        if (screenRenderer == null)
        {
            Transform existing = screenRoot.Find(ScreenName);
            if (existing == null)
            {
                existing = transform.Find("Visual_REPLACE_LED_ScreenGlow");
            }

            if (existing != null)
            {
                existing.name = ScreenName;
                existing.SetParent(screenRoot, false);
                screenRenderer = existing.GetComponent<Renderer>();
            }
        }

        if (screenRenderer == null)
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = ScreenName;
            screen.transform.SetParent(screenRoot);
            screen.transform.localPosition = screenLocalPosition;
            screen.transform.localRotation = Quaternion.identity;
            screen.transform.localScale = screenLocalScale;

            Collider collider = screen.GetComponent<Collider>();
            if (collider != null)
            {
                DestroySafe(collider);
            }

            screenRenderer = screen.GetComponent<Renderer>();
        }
        else
        {
            Transform screenTransform = screenRenderer.transform;
            if (screenTransform.parent != screenRoot)
            {
                screenTransform.SetParent(screenRoot, false);
            }

            screenTransform.name = ScreenName;
            if (autoLayout)
            {
                screenTransform.localPosition = screenLocalPosition;
                screenTransform.localRotation = Quaternion.identity;
                screenTransform.localScale = screenLocalScale;
            }
        }
    }

    private void EnsureTextObjects()
    {
        screenRoot = EnsureScreenRoot();

        if (labelText == null)
        {
            Transform existing = screenRoot.Find(LabelName);
            labelText = existing != null ? existing.GetComponent<TextMeshPro>() : FindLegacyLabel();
        }

        if (labelText == null)
        {
            labelText = CreateText(LabelName).GetComponent<TextMeshPro>();
        }

        if (numberText == null)
        {
            Transform existing = screenRoot.Find(NumberName);
            numberText = existing != null ? existing.GetComponent<TextMeshPro>() : null;
        }

        if (numberText == null)
        {
            numberText = CreateText(NumberName).GetComponent<TextMeshPro>();
        }
    }

    private TextMeshPro FindLegacyLabel()
    {
        TextMeshPro[] labels = GetComponentsInChildren<TextMeshPro>(true);
        foreach (TextMeshPro label in labels)
        {
            if (label.name.StartsWith("TMP_Label_"))
            {
                label.name = LabelName;
                label.transform.SetParent(EnsureScreenRoot(), false);
                return label;
            }
        }

        return null;
    }

    private GameObject CreateText(string objectName)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(EnsureScreenRoot());
        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return textObject;
    }

    private void ApplyStaticDisplay()
    {
        if (labelText != null)
        {
            labelText.text = resolvedLabel;
            labelText.fontSize = labelFontSize;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            if (autoLayout)
            {
                labelText.rectTransform.sizeDelta = labelSize;
                labelText.rectTransform.localPosition = labelLocalPosition;
                labelText.rectTransform.localRotation = Quaternion.identity;
                labelText.rectTransform.localScale = labelLocalScale;
            }
        }

        if (numberText != null)
        {
            numberText.text = resolvedNumber;
            numberText.fontSize = numberFontSize;
            numberText.fontStyle = FontStyles.Bold;
            numberText.alignment = TextAlignmentOptions.Center;
            numberText.color = resolvedColor;
            if (autoLayout)
            {
                numberText.rectTransform.sizeDelta = numberSize;
                numberText.rectTransform.localPosition = numberLocalPosition;
                numberText.rectTransform.localRotation = Quaternion.identity;
                numberText.rectTransform.localScale = numberLocalScale;
            }
        }

        if (screenRenderer != null)
        {
            Material material = Application.isPlaying
                ? (runtimeScreenMaterial ??= screenRenderer.material)
                : screenRenderer.sharedMaterial;

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                if (Application.isPlaying)
                {
                    runtimeScreenMaterial = material;
                    screenRenderer.material = material;
                }
                else
                {
                    screenRenderer.sharedMaterial = material;
                }
            }

            Color screenColor = Color.Lerp(Color.black, resolvedColor, 0.38f);
            SetBaseColor(material, screenColor);
            ApplyEmission(material, resolvedColor, screenEmissionIdle);
        }
    }

    private Transform EnsureScreenRoot()
    {
        if (screenRoot != null)
        {
            return screenRoot;
        }

        Transform existing = transform.Find(ScreenRootName);
        bool created = false;
        if (existing == null)
        {
            GameObject root = new GameObject(ScreenRootName);
            existing = root.transform;
            existing.SetParent(transform);
            created = true;
        }

        if (autoLayout || created)
        {
            existing.localPosition = Vector3.zero;
            existing.localRotation = Quaternion.identity;
            existing.localScale = Vector3.one;
        }

        screenRoot = existing;
        return screenRoot;
    }

    private static void SetBaseColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private static void ApplyEmission(Material material, Color color, float intensity)
    {
        if (material == null)
        {
            return;
        }

        material.EnableKeyword("_EMISSION");
        Color emission = color * intensity;
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
        }
    }

    private static void DestroySafe(Object target)
    {
        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
