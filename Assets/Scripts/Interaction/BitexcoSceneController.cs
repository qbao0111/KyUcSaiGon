using UnityEngine;

public class BitexcoSceneController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable officeWorkerNpc;
    public BitexcoEmployeeCardInteractable employeeCardItem;
    public PuzzleInteractable securityPuzzle;
    public BusStopInteractable returnBusStop;

    [TextArea]
    public string objectiveStart = "Đi vào quảng trường Bitexco và tìm dấu vết của nhịp sống hiện đại.";

    [TextArea]
    public string objectiveUseCard = "Quét thẻ tại khu vực an ninh.";

    [TextArea]
    public string objectiveSolvePuzzle = "Giải mã hệ thống máy chủ trung tâm.";

    [TextArea]
    public string objectiveReturnBus = "Đi đến trạm xe buýt ký ức để quay lại Hub.";

    [TextArea]
    public string npcDialogue = "Tôi cứ nghĩ chỉ cần chạy thật nhanh là đủ. Nhưng khi hệ thống điện vụt tắt, tôi quên mất lý do mình bắt đầu.";

    [TextArea]
    public string restoredDialogue = "Hệ thống điện khởi động lại. Bitexco sáng lên giữa bầu trời Sài Gòn.";

    private bool cardCollected;
    private bool npcSpoken;

    private void Awake()
    {
        if (officeWorkerNpc != null)
        {
            officeWorkerNpc.suppressDefaultDialogue = true;
            officeWorkerNpc.interacted.AddListener(OnNpcInteracted);
        }

        if (employeeCardItem != null)
        {
            employeeCardItem.sceneController = this;
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += OnRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        if (employeeCardItem != null)
        {
            employeeCardItem.gameObject.SetActive(!restored);
        }

        if (securityPuzzle != null)
        {
            securityPuzzle.gameObject.SetActive(restored);
        }

        if (returnBusStop != null)
        {
            returnBusStop.gameObject.SetActive(restored);
        }

        UIManager.Instance?.SetObjective(restored ? objectiveReturnBus : objectiveStart);
    }

    private void OnDestroy()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= OnRestored;
        }
    }

    public void OnEmployeeCardCollected()
    {
        cardCollected = true;
        UIManager.Instance?.SetObjective(objectiveUseCard);
    }

    private void OnNpcInteracted()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Bitexco đã sáng trở lại rồi.");
            UIManager.Instance?.SetObjective(objectiveReturnBus);
            return;
        }

        if (!cardCollected)
        {
            UIManager.Instance?.ShowDialogue("Bạn cần tìm thẻ nhân viên trước.");
            UIManager.Instance?.SetObjective(objectiveStart);
            return;
        }

        if (!npcSpoken)
        {
            npcSpoken = true;
            UIManager.Instance?.ShowDialogue(npcDialogue);
        }

        UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
        if (securityPuzzle != null)
        {
            securityPuzzle.gameObject.SetActive(true);
        }
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
