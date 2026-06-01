using UnityEngine;

public class NhaThoDucBaSceneController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable pigeonFeederNpc;
    public NhaThoSmallBellInteractable smallBellItem;
    public PuzzleInteractable bellPuzzle;
    public BusStopInteractable returnBusStop;

    [TextArea]
    public string objectiveStart = "Đi vào quảng trường và tìm khoảng lặng bị đánh mất.";

    [TextArea]
    public string objectiveFindBell = "Tìm chiếc chuông nhỏ quanh quảng trường.";

    [TextArea]
    public string objectiveReturnBell = "Mang chiếc chuông về cho cụ già.";

    [TextArea]
    public string objectiveSolvePuzzle = "Giải câu đố thứ tự chuông.";

    [TextArea]
    public string objectiveReturnBus = "Đi đến trạm xe buýt ký ức để quay lại Hub.";

    [TextArea]
    public string firstNpcDialogue = "Tiếng chuông nhà thờ từng dẫn bầy bồ câu bay về mỗi chiều. Nhưng giờ mọi thứ đã im lặng.";

    [TextArea]
    public string secondNpcDialogue = "Đúng rồi... âm thanh này từng thuộc về nơi đây. Hãy đánh thức hòa âm của tháp chuông.";

    [TextArea]
    public string restoredDialogue = "Tiếng chuông vang lên. Bầy bồ câu cất cánh. Nhà thờ Đức Bà đã được khôi phục.";

    private bool firstTalkDone;
    private bool bellCollected;
    private bool secondTalkDone;

    private void Awake()
    {
        if (pigeonFeederNpc != null)
        {
            pigeonFeederNpc.suppressDefaultDialogue = true;
            pigeonFeederNpc.interacted.AddListener(OnNpcInteracted);
        }

        if (smallBellItem != null)
        {
            smallBellItem.sceneController = this;
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += OnRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        if (smallBellItem != null)
        {
            smallBellItem.gameObject.SetActive(!restored);
        }

        if (bellPuzzle != null)
        {
            bellPuzzle.gameObject.SetActive(restored);
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

    public void OnSmallBellCollected()
    {
        bellCollected = true;
        UIManager.Instance?.SetObjective(objectiveReturnBell);
    }

    private void OnNpcInteracted()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Quảng trường đã bình yên trở lại rồi.");
            UIManager.Instance?.SetObjective(objectiveReturnBus);
            return;
        }

        if (!firstTalkDone)
        {
            firstTalkDone = true;
            UIManager.Instance?.ShowDialogue(firstNpcDialogue);
            UIManager.Instance?.SetObjective(objectiveFindBell);
            return;
        }

        if (!bellCollected)
        {
            UIManager.Instance?.ShowDialogue("Hãy tìm chiếc chuông nhỏ quanh quảng trường.");
            UIManager.Instance?.SetObjective(objectiveFindBell);
            return;
        }

        if (!secondTalkDone)
        {
            secondTalkDone = true;
            UIManager.Instance?.ShowDialogue(secondNpcDialogue);
            UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
            if (bellPuzzle != null)
            {
                bellPuzzle.gameObject.SetActive(true);
            }

            return;
        }

        UIManager.Instance?.ShowDialogue("Hãy sắp xếp thứ tự chuông: La, Sol, Re, Mi, Si, Do.");
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
