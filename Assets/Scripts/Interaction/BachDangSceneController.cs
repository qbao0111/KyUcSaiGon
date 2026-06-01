using UnityEngine;

public class BachDangSceneController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable boatmanNpc;
    public BachDangTicketInteractable oldFerryTicketItem;
    public PuzzleInteractable riverPuzzle;
    public BusStopInteractable returnBusStop;

    [TextArea] public string objectiveStart = "Đi dọc bến sông và tìm ký ức bị mắc kẹt.";
    [TextArea] public string objectiveFindTicket = "Tìm chiếc vé tàu cũ.";
    [TextArea] public string objectiveReturnTicket = "Mang chiếc vé về cho bác lái tàu.";
    [TextArea] public string objectiveSolvePuzzle = "Giải câu đố vượt luồng sương mù.";
    [TextArea] public string objectiveReturnBus = "Đi đến trạm xe buýt ký ức để quay lại Hub.";

    [TextArea] public string firstNpcDialogue = "Dòng sông này từng chở theo rất nhiều câu chuyện. Nhưng hôm nay nước đứng yên, còn những con tàu thì không thể rời bến.";
    [TextArea] public string secondNpcDialogue = "Đúng rồi... chiếc vé này từng thuộc về một hành trình. Hãy giúp tôi khai thông dòng chảy.";
    [TextArea] public string restoredDialogue = "Dòng sông chuyển động trở lại. Bến Bạch Đằng đã được khôi phục.";

    private bool firstTalkDone;
    private bool ticketCollected;
    private bool secondTalkDone;

    private void Awake()
    {
        if (boatmanNpc != null)
        {
            boatmanNpc.suppressDefaultDialogue = true;
            boatmanNpc.interacted.AddListener(OnBoatmanInteracted);
        }

        if (oldFerryTicketItem != null)
        {
            oldFerryTicketItem.sceneController = this;
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += OnRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        if (oldFerryTicketItem != null)
        {
            oldFerryTicketItem.gameObject.SetActive(!restored);
        }

        if (riverPuzzle != null)
        {
            riverPuzzle.gameObject.SetActive(restored);
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

    public void OnOldFerryTicketCollected()
    {
        ticketCollected = true;
        UIManager.Instance?.SetObjective(objectiveReturnTicket);
    }

    private void OnBoatmanInteracted()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Bến sông đã thông dòng trở lại.");
            UIManager.Instance?.SetObjective(objectiveReturnBus);
            return;
        }

        if (!firstTalkDone)
        {
            firstTalkDone = true;
            UIManager.Instance?.ShowDialogue(firstNpcDialogue);
            UIManager.Instance?.SetObjective(objectiveFindTicket);
            return;
        }

        if (!ticketCollected)
        {
            UIManager.Instance?.ShowDialogue("Hãy tìm chiếc vé tàu cũ quanh bến.");
            UIManager.Instance?.SetObjective(objectiveFindTicket);
            return;
        }

        if (!secondTalkDone)
        {
            secondTalkDone = true;
            UIManager.Instance?.ShowDialogue(secondNpcDialogue);
            UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
            if (riverPuzzle != null)
            {
                riverPuzzle.gameObject.SetActive(true);
            }

            return;
        }

        UIManager.Instance?.ShowDialogue("Hãy giải câu đố vượt luồng sương mù.");
        UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
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
