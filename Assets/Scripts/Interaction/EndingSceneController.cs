using System.Collections;
using UnityEngine;

public class EndingSceneController : MonoBehaviour
{
    public GameObject landmarkTower;
    public GameObject[] memoryShards;
    public string[] memoryNames;
    public GameObject finalLightObject;
    public GameObject returnTrigger;
    public Renderer[] renderersToWarm;
    public AudioSource endingAmbience;

    public float shardStepDelay = 0.9f;
    public float toneShiftDuration = 2.5f;

    private readonly Color grayTone = new Color(0.38f, 0.4f, 0.43f);
    private readonly Color shardWarm = new Color(0.96f, 0.83f, 0.36f);

    private Color[] originalColors;
    private string[] colorPropNames;

    private void Start()
    {
        CacheOriginalColors();

        if (returnTrigger != null)
        {
            returnTrigger.SetActive(false);
        }

        if (!DeveloperMode.IsEnabled && (GameProgressManager.Instance == null || !GameProgressManager.Instance.AreAllMemoriesRestored()))
        {
            StartCoroutine(ReturnToHubIfNotEnoughMemory());
            return;
        }

        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator ReturnToHubIfNotEnoughMemory()
    {
        UIManager.Instance?.ShowDialogue("Bạn chưa thu thập đủ ký ức.");
        UIManager.Instance?.SetObjective("Quay lại xe buýt ký ức...");
        yield return new WaitForSeconds(2.6f);
        SceneLoader.Load(SceneLoader.BusHub);
    }

    private IEnumerator PlayEndingSequence()
    {
        ApplyToneLerp(0f);
        if (endingAmbience != null)
        {
            endingAmbience.Play();
            endingAmbience.volume = 0f;
        }

        UIManager.Instance?.SetObjective("Lắng nghe những mảnh ký ức của thành phố.");

        if (memoryShards != null)
        {
            for (int i = 0; i < memoryShards.Length; i++)
            {
                LightUpShard(i);
                if (memoryNames != null && i < memoryNames.Length)
                {
                    UIManager.Instance?.ShowDialogue(memoryNames[i]);
                }

                if (endingAmbience != null)
                {
                    endingAmbience.volume = Mathf.Clamp01((i + 1) / 6f) * 0.42f;
                }

                yield return new WaitForSeconds(shardStepDelay);
            }
        }

        LightUpLandmark();
        yield return StartCoroutine(ShiftToneToOriginal());

        UIManager.Instance?.ShowDialogue("Khi ký ức được lắng nghe, thành phố lại tìm thấy màu sắc của mình.");
        yield return new WaitForSeconds(3.2f);
        UIManager.Instance?.ShowDialogue("Tương lai không bắt đầu bằng việc quên đi quá khứ.\nTương lai bắt đầu khi ta biết mang ký ức đi cùng.");
        yield return new WaitForSeconds(3.4f);
        UIManager.Instance?.ShowDialogue("BẠN ĐÃ TÌM LẠI KÝ ỨC ĐÔ THỊ\nTHÀNH PHỐ ĐÃ SỐNG LẠI");
        UIManager.Instance?.SetObjective("Tiến đến cổng sáng để quay lại xe buýt ký ức.");

        if (returnTrigger != null)
        {
            returnTrigger.SetActive(true);
        }
    }

    private void LightUpShard(int index)
    {
        if (memoryShards == null || index < 0 || index >= memoryShards.Length || memoryShards[index] == null)
        {
            return;
        }

        Renderer renderer = memoryShards[index].GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = shardWarm;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", shardWarm * 1.8f);
        }

        Light[] lights = memoryShards[index].GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = 1.25f;
        }
    }

    private void LightUpLandmark()
    {
        if (landmarkTower == null)
        {
            return;
        }

        Renderer renderer = landmarkTower.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color towerColor = new Color(0.54f, 0.74f, 1f);
            renderer.material.color = towerColor;
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", towerColor * 1.6f);
        }

        if (finalLightObject != null)
        {
            Renderer finalRenderer = finalLightObject.GetComponent<Renderer>();
            if (finalRenderer != null)
            {
                finalRenderer.material.EnableKeyword("_EMISSION");
                finalRenderer.material.SetColor("_EmissionColor", shardWarm * 2f);
            }

            Light[] lights = finalLightObject.GetComponentsInChildren<Light>(true);
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = 2.2f;
            }
        }
    }

    private IEnumerator ShiftToneToOriginal()
    {
        float elapsed = 0f;
        while (elapsed < toneShiftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / toneShiftDuration);
            ApplyToneLerp(t);
            yield return null;
        }

        ApplyToneLerp(1f);
    }

    private void CacheOriginalColors()
    {
        if (originalColors != null && colorPropNames != null)
        {
            return;
        }

        if (renderersToWarm == null)
        {
            return;
        }

        originalColors = new Color[renderersToWarm.Length];
        colorPropNames = new string[renderersToWarm.Length];

        for (int i = 0; i < renderersToWarm.Length; i++)
        {
            Renderer r = renderersToWarm[i];
            if (r == null)
            {
                continue;
            }

            Material material = r.material;
            if (material != null)
            {
                string propName = null;
                if (material.HasProperty("_BaseColor"))
                {
                    propName = "_BaseColor";
                }
                else if (material.HasProperty("_Color"))
                {
                    propName = "_Color";
                }
                colorPropNames[i] = propName;

                if (propName != null)
                {
                    originalColors[i] = material.GetColor(propName);
                }
                else
                {
                    originalColors[i] = Color.white;
                }
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }
    }

    private void ApplyToneLerp(float t)
    {
        CacheOriginalColors();

        if (renderersToWarm == null)
        {
            return;
        }

        for (int i = 0; i < renderersToWarm.Length; i++)
        {
            Renderer r = renderersToWarm[i];
            string propName = colorPropNames != null && i < colorPropNames.Length ? colorPropNames[i] : null;

            if (r == null || propName == null)
            {
                continue;
            }

            Color originalColor = originalColors != null && i < originalColors.Length ? originalColors[i] : Color.white;

            // Calculate monochrome desaturated tone of the original color
            float luminance = originalColor.r * 0.299f + originalColor.g * 0.587f + originalColor.b * 0.114f;
            Color desaturated = new Color(luminance, luminance, luminance, originalColor.a);

            // Blend desaturated color with grayTone (tint) for initial state (t=0)
            Color grayVersion = Color.Lerp(desaturated, grayTone, 0.2f);
            
            // Lerp from the gray version to the original color based on t
            Color targetColor = Color.Lerp(grayVersion, originalColor, t);
            
            r.material.SetColor(propName, targetColor);
        }
    }
}
