using UnityEngine;

public class NguyenHueTutorialController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable streetMusicianNpc;
    public ItemInteractable memoryItem;
    public PuzzleInteractable speakerPuzzle;
    public BusStopInteractable returnBusStop;
    public LEDHintInteractable[] ledHints;
    public NguyenHueRestorationController restorationController;

    [TextArea] public string initialObjective = "Đi dọc phố đi bộ và nói chuyện với nhạc công.";
    [TextArea] public string itemObjective = "Tìm vật gợi nhớ quanh khu vực biểu diễn.";
    [TextArea] public string returnItemObjective = "Mang vật gợi nhớ quay lại cho nhạc công.";
    [TextArea] public string ledObjective = "Tìm 3 màn hình LED lớn quanh phố đi bộ: Bass, Mid, Treble.";
    [TextArea] public string puzzleObjective = "Chỉnh loa theo gợi ý LED: Bass, Mid, Treble.";
    [TextArea] public string busStopObjective = "Đi đến trạm xe buýt ký ức để tiếp tục hành trình.";
    [TextArea] public string restoredDialogue = "Âm nhạc trở lại. Đài phun nước sáng lên. Phố đi bộ Nguyễn Huệ đã được khôi phục.";

    private bool hasTalkedToNpc;
    private bool hasCollectedItem;
    private bool hasReturnedItem;

    private void Awake()
    {
        ResolveReferences();

        if (streetMusicianNpc != null)
        {
            streetMusicianNpc.tutorialController = this;
            streetMusicianNpc.suppressDefaultDialogue = true;
        }

        if (memoryItem != null)
        {
            memoryItem.interacted.AddListener(OnMemoryItemCollected);
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += HandleRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        ApplyFlowVisibility(restored);
        UIManager.Instance?.SetObjective(restored ? busStopObjective : initialObjective);
    }

    private void OnDestroy()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= HandleRestored;
        }

        if (memoryItem != null)
        {
            memoryItem.interacted.RemoveListener(OnMemoryItemCollected);
        }
    }

    public void TalkToMusician()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Phố đi bộ đã có âm nhạc trở lại rồi.");
            AudioManager.EnsureInstance().PlayVoice("NguyenHue_2");
            UIManager.Instance?.SetObjective(busStopObjective);
            return;
        }

        if (!hasTalkedToNpc)
        {
            hasTalkedToNpc = true;
            UIManager.Instance?.ShowDialogue(streetMusicianNpc != null
                ? streetMusicianNpc.dialogue
                : "Nhịp điệu bị nhiễu rồi. Hãy tìm ba màn hình LED quanh phố và chỉnh lại loa.");
            AudioManager.EnsureInstance().PlayVoice("NguyenHue_1");
            SetLedHintsVisible(true);

            if (memoryItem != null)
            {
                memoryItem.gameObject.SetActive(true);
                UIManager.Instance?.SetObjective(itemObjective);
            }
            else
            {
                hasReturnedItem = true;
                SetSpeakerPuzzleVisible(true);
                UIManager.Instance?.SetObjective(ledObjective);
            }

            return;
        }

        if (memoryItem != null && !hasCollectedItem)
        {
            UIManager.Instance?.ShowDialogue("Hãy tìm vật gợi nhớ quanh khu vực biểu diễn trước đã.");
            UIManager.Instance?.SetObjective(itemObjective);
            return;
        }

        if (memoryItem != null && !hasReturnedItem)
        {
            hasReturnedItem = true;
            SetSpeakerPuzzleVisible(true);
            UIManager.Instance?.ShowDialogue("Đúng rồi... ký ức của âm thanh đang quay lại. Giờ hãy chỉnh loa theo ba màn hình LED.");
            UIManager.Instance?.SetObjective(puzzleObjective);
            return;
        }

        UIManager.Instance?.ShowDialogue(streetMusicianNpc != null
            ? streetMusicianNpc.dialogue
            : "Nhịp điệu bị nhiễu rồi. Hãy tìm ba màn hình LED quanh phố và chỉnh lại loa.");
        AudioManager.EnsureInstance().PlayVoice("NguyenHue_1");
        UIManager.Instance?.SetObjective(puzzleObjective);
    }

    public void InspectLedHint(string message)
    {
        if (!hasTalkedToNpc)
        {
            UIManager.Instance?.ShowDialogue("Hãy nói chuyện với nhạc công trước.");
            UIManager.Instance?.SetObjective(initialObjective);
            return;
        }

        UIManager.Instance?.ShowDialogue(message);
        if (hasReturnedItem)
        {
            UIManager.Instance?.SetObjective(puzzleObjective);
        }
        else if (memoryItem == null)
        {
            UIManager.Instance?.SetObjective(ledObjective);
        }
    }

    private void HandleRestored()
    {
        if (restorationController != null && restorationController.State != NguyenHueRestorationController.RestorationState.Restored)
        {
            SetBusStopVisible(false);
            UIManager.Instance?.SetObjective("Ký ức đang trở lại với phố đi bộ...");
            return;
        }

        SetBusStopVisible(true);
        UIManager.Instance?.ShowDialogue(restoredDialogue);
        UIManager.Instance?.SetObjective(busStopObjective);
    }

    private void OnMemoryItemCollected()
    {
        if (!hasTalkedToNpc || hasCollectedItem)
        {
            return;
        }

        hasCollectedItem = true;
        if (memoryItem != null)
        {
            memoryItem.gameObject.SetActive(false);
        }

        UIManager.Instance?.SetObjective(returnItemObjective);
    }

    private void ResolveReferences()
    {
        if (memoryZone == null)
        {
            memoryZone = FindInScene<MemoryZoneController>(null);
        }

        if (streetMusicianNpc == null)
        {
            streetMusicianNpc = FindInScene<NPCInteractable>("REPLACE_NPC_StreetMusician");
        }

        if (speakerPuzzle == null)
        {
            speakerPuzzle = FindInScene<PuzzleInteractable>("REPLACE_Puzzle_SpeakerMixer");
        }

        if (returnBusStop == null)
        {
            returnBusStop = FindInScene<BusStopInteractable>("REPLACE_BusStop_ReturnHub");
        }

        if (restorationController == null)
        {
            restorationController = FindInScene<NguyenHueRestorationController>("NguyenHueRestorationController");
        }

        if (ledHints == null || ledHints.Length == 0)
        {
            ledHints = FindObjectsByType<LEDHintInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        ConfigureNguyenHuePuzzle();
        ConfigureNguyenHueLeds();
    }

    private T FindInScene<T>(string objectName) where T : Component
    {
        T[] components = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (T component in components)
        {
            if (component == null || component.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            if (string.IsNullOrEmpty(objectName) || component.name == objectName)
            {
                return component;
            }
        }

        return null;
    }

    private void ConfigureNguyenHuePuzzle()
    {
        if (speakerPuzzle == null)
        {
            return;
        }

        speakerPuzzle.correctAnswer = "1-6-8";
        speakerPuzzle.puzzleTitle = "Bộ điều chỉnh âm thanh";
        speakerPuzzle.puzzleDescription = "Chỉnh Bass - Mid - Treble theo ba màn hình LED.";
        speakerPuzzle.inputHint = "Bass - Mid - Treble";
        speakerPuzzle.wrongFeedback = "Chưa đúng. Hãy kiểm tra lại ba màn hình LED.";
        speakerPuzzle.correctFeedback = "Đúng rồi. Âm thanh đã trong trẻo trở lại.";
    }

    private void ConfigureNguyenHueLeds()
    {
        if (ledHints == null)
        {
            return;
        }

        foreach (LEDHintInteractable led in ledHints)
        {
            if (led == null || led.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            led.tutorialController = this;
            if (led.name.Contains("Bass"))
            {
                led.hintMessage = "LED đỏ nhấp nháy số 1. Đây là Bass.";
                led.ConfigureDisplay("BASS", "1", new Color(1f, 0.08f, 0.04f));
            }
            else if (led.name.Contains("Mid"))
            {
                led.hintMessage = "LED xanh lá nhấp nháy số 6. Đây là Mid.";
                led.ConfigureDisplay("MID", "6", new Color(0.08f, 1f, 0.28f));
            }
            else if (led.name.Contains("Treble") || led.name.Contains("Gold"))
            {
                led.hintMessage = "LED vàng nhấp nháy số 8. Đây là Treble.";
                led.ConfigureDisplay("TREBLE", "8", new Color(1f, 0.78f, 0.08f));
            }
        }
    }

    private void ApplyFlowVisibility(bool restored)
    {
        SetLedHintsVisible(false);
        SetSpeakerPuzzleVisible(false);
        SetBusStopVisible(restored);

        if (memoryItem != null)
        {
            memoryItem.gameObject.SetActive(false);
        }
    }

    private void SetLedHintsVisible(bool visible)
    {
        if (ledHints == null)
        {
            return;
        }

        foreach (LEDHintInteractable led in ledHints)
        {
            if (led != null && led.gameObject.scene == gameObject.scene)
            {
                led.gameObject.SetActive(visible);
            }
        }
    }

    private void SetSpeakerPuzzleVisible(bool visible)
    {
        if (speakerPuzzle != null)
        {
            speakerPuzzle.gameObject.SetActive(visible);
        }
    }

    private void SetBusStopVisible(bool visible)
    {
        if (returnBusStop != null)
        {
            returnBusStop.gameObject.SetActive(visible);
        }
    }
}
