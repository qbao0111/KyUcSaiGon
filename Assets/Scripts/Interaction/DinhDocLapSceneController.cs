using UnityEngine;

public class DinhDocLapSceneController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable tourGuideNpc;
    public DinhDocLapMapItemInteractable historicalMapItem;
    public PuzzleInteractable radioPuzzle;
    public BusStopInteractable returnBusStop;

    [TextArea]
    public string objectiveFindMap = "Đi đến cổng Dinh và tìm mảnh bản đồ lịch sử.";

    [TextArea]
    public string objectiveReturnNpc = "Đưa mảnh bản đồ cho người hướng dẫn viên.";

    [TextArea]
    public string objectiveSolveRadio = "Giải mã chiếc radio lịch sử.";

    [TextArea]
    public string objectiveReturnBus = "Đi đến trạm xe buýt ký ức để quay lại Hub.";

    [TextArea]
    public string npcMainDialogue = "Mỗi căn phòng, mỗi hiện vật nơi đây đều giữ một phần lịch sử. Nhưng các dòng sự kiện đã bị đảo lộn, khiến đài radio bị nhiễu sóng.";

    [TextArea]
    public string restoredDialogue = "Đài radio hoạt động trở lại. Sương mù tan dần. Dinh Độc Lập đã được khôi phục.";

    private bool mapCollected;
    private bool npcGuided;

    private void Awake()
    {
        if (tourGuideNpc != null)
        {
            tourGuideNpc.suppressDefaultDialogue = true;
            tourGuideNpc.interacted.AddListener(OnTourGuideInteracted);
        }

        if (historicalMapItem != null)
        {
            historicalMapItem.sceneController = this;
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += OnRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        if (historicalMapItem != null)
        {
            historicalMapItem.gameObject.SetActive(!restored);
        }

        if (radioPuzzle != null)
        {
            radioPuzzle.gameObject.SetActive(restored);
        }

        if (returnBusStop != null)
        {
            returnBusStop.gameObject.SetActive(restored);
        }

        UIManager.Instance?.SetObjective(restored ? objectiveReturnBus : objectiveFindMap);
    }

    private void OnDestroy()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= OnRestored;
        }
    }

    public void OnHistoricalMapCollected()
    {
        mapCollected = true;
        UIManager.Instance?.SetObjective(objectiveReturnNpc);
    }

    private void OnTourGuideInteracted()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Lịch sử đã sáng rõ trở lại rồi.");
            UIManager.Instance?.SetObjective(objectiveReturnBus);
            return;
        }

        if (!mapCollected)
        {
            UIManager.Instance?.ShowDialogue("Hãy tìm mảnh bản đồ lịch sử trước đã.");
            UIManager.Instance?.SetObjective(objectiveFindMap);
            return;
        }

        if (!npcGuided)
        {
            npcGuided = true;
            UIManager.Instance?.ShowDialogue(npcMainDialogue);
            UIManager.Instance?.SetObjective(objectiveSolveRadio);
            if (radioPuzzle != null)
            {
                radioPuzzle.gameObject.SetActive(true);
            }

            return;
        }

        UIManager.Instance?.ShowDialogue("Hãy giải mã chiếc radio bằng mốc năm lịch sử.");
        UIManager.Instance?.SetObjective(objectiveSolveRadio);
    }

    private void OnRestored()
    {
        UIManager.Instance?.ShowDialogue(restoredDialogue);
        UIManager.Instance?.SetObjective(objectiveReturnBus);

        if (returnBusStop != null)
        {
            returnBusStop.gameObject.SetActive(true);
        }
    }
}
