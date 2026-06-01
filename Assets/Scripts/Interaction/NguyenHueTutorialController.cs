using UnityEngine;

public class NguyenHueTutorialController : MonoBehaviour
{
    public MemoryZoneController memoryZone;

    [TextArea] public string initialObjective = "Đi dọc phố đi bộ và tìm nguồn âm thanh bị nhiễu.";
    [TextArea] public string ledObjective = "Tìm 3 màn hình LED lớn quanh phố đi bộ: Bass, Mid, Treble.";
    [TextArea] public string busStopObjective = "Đi đến trạm xe buýt ký ức để tiếp tục hành trình.";
    [TextArea] public string restoredDialogue = "Âm nhạc trở lại. Đài phun nước sáng lên. Phố đi bộ Nguyễn Huệ đã được khôi phục.";

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += HandleRestored;
        }

        UIManager.Instance?.SetObjective(memoryZone != null && memoryZone.IsRestored ? busStopObjective : initialObjective);
    }

    private void OnDestroy()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= HandleRestored;
        }
    }

    public void TalkToMusician()
    {
        UIManager.Instance?.SetObjective(ledObjective);
    }

    public void InspectLedHint(string message)
    {
        UIManager.Instance?.ShowDialogue(message);
    }

    private void HandleRestored()
    {
        UIManager.Instance?.ShowDialogue(restoredDialogue);
        UIManager.Instance?.SetObjective(busStopObjective);
    }
}
