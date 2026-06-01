using UnityEngine;
using System.Collections;

public class BenThanhSceneController : MonoBehaviour
{
    public MemoryZoneController memoryZone;
    public NPCInteractable marketVendor;
    public GameObject oldConicalHatItem;
    public PuzzleInteractable fruitBasketPuzzle;
    public BusStopInteractable returnBusStop;

    [TextArea]
    public string objectiveEnterMarket = "Đi vào khu chợ và tìm người đang giữ ký ức nơi này.";

    [TextArea]
    public string objectiveFindHat = "Tìm chiếc nón lá cũ quanh khu chợ.";

    [TextArea]
    public string objectiveReturnVendor = "Mang ký ức trở lại cho cô bán hàng.";

    [TextArea]
    public string objectiveSolvePuzzle = "Giải câu đố giỏ trái cây.";

    [TextArea]
    public string objectiveReturnBus = "Đi đến trạm xe buýt ký ức để quay lại hành trình.";

    [TextArea]
    public string firstVendorDialogue = "Ngày xưa khu chợ này lúc nào cũng đầy tiếng rao và bước chân. Nhưng giờ mọi thứ im quá... Có lẽ ký ức nằm ở đâu đó quanh đây.";

    [TextArea]
    public string secondVendorDialogue = "Đúng rồi... chiếc nón này từng đi cùng biết bao buổi chợ sớm. Nhưng âm thanh của khu chợ vẫn chưa trở lại. Hãy sắp xếp lại giỏ trái cây.";

    [TextArea]
    public string restoredDialogue = "Tiếng rao và nhịp sống trở lại. Chợ Bến Thành đã được khôi phục.";

    private bool firstTalkDone;
    private bool hatCollected;
    private bool secondTalkDone;

    private void Awake()
    {
        if (marketVendor != null)
        {
            marketVendor.suppressDefaultDialogue = true;
            marketVendor.interacted.AddListener(OnVendorInteracted);
        }
    }

    private void Start()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored += OnZoneRestored;
        }

        bool restored = memoryZone != null && memoryZone.IsRestored;
        if (oldConicalHatItem != null)
        {
            oldConicalHatItem.SetActive(!restored);
        }

        if (fruitBasketPuzzle != null)
        {
            fruitBasketPuzzle.gameObject.SetActive(restored);
        }

        UIManager.Instance?.SetObjective(restored ? objectiveReturnBus : objectiveEnterMarket);
    }

    private void OnDestroy()
    {
        if (memoryZone != null)
        {
            memoryZone.Restored -= OnZoneRestored;
        }
    }

    public void OnConicalHatCollected()
    {
        hatCollected = true;
        UIManager.Instance?.SetObjective(objectiveReturnVendor);
    }

    private void OnVendorInteracted()
    {
        if (memoryZone != null && memoryZone.IsRestored)
        {
            UIManager.Instance?.ShowDialogue("Khu chợ đã sống lại rồi. Cảm ơn bạn.");
            UIManager.Instance?.SetObjective(objectiveReturnBus);
            return;
        }

        if (!firstTalkDone)
        {
            firstTalkDone = true;
            UIManager.Instance?.ShowDialogue(firstVendorDialogue);
            UIManager.Instance?.SetObjective(objectiveFindHat);
            return;
        }

        if (!hatCollected)
        {
            UIManager.Instance?.ShowDialogue("Ký ức còn nằm đâu đó trong khu chợ. Hãy tìm chiếc nón lá cũ.");
            UIManager.Instance?.SetObjective(objectiveFindHat);
            return;
        }

        if (!secondTalkDone)
        {
            secondTalkDone = true;
            UIManager.Instance?.ShowDialogue(secondVendorDialogue);
            UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
            if (fruitBasketPuzzle != null)
            {
                StartCoroutine(ShowFruitPuzzleSafely());
            }

            return;
        }

        UIManager.Instance?.ShowDialogue("Hãy sắp xếp lại giỏ trái cây theo ký ức của khu chợ.");
        UIManager.Instance?.SetObjective(objectiveSolvePuzzle);
    }

    private void OnZoneRestored()
    {
        UIManager.Instance?.ShowDialogue(restoredDialogue);
        UIManager.Instance?.SetObjective(objectiveReturnBus);
        if (returnBusStop != null)
        {
            returnBusStop.gameObject.SetActive(true);
        }
    }

    private IEnumerator ShowFruitPuzzleSafely()
    {
        fruitBasketPuzzle.gameObject.SetActive(true);

        Collider[] puzzleColliders = fruitBasketPuzzle.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < puzzleColliders.Length; i++)
        {
            puzzleColliders[i].enabled = false;
        }

        MovePlayerOutOfPuzzleIfNeeded();
        PrototypeLogger.Info("BenThanh: Fruit basket puzzle revealed safely.");

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < puzzleColliders.Length; i++)
        {
            if (puzzleColliders[i] != null)
            {
                puzzleColliders[i].enabled = true;
            }
        }
    }

    private void MovePlayerOutOfPuzzleIfNeeded()
    {
        if (fruitBasketPuzzle == null)
        {
            return;
        }

        CharacterController playerController = Object.FindFirstObjectByType<CharacterController>();
        if (playerController == null)
        {
            return;
        }

        Collider[] puzzleColliders = fruitBasketPuzzle.GetComponentsInChildren<Collider>(true);
        Bounds playerBounds = playerController.bounds;

        bool overlap = false;
        Bounds combinedPuzzleBounds = new Bounds(fruitBasketPuzzle.transform.position, Vector3.zero);

        for (int i = 0; i < puzzleColliders.Length; i++)
        {
            Collider col = puzzleColliders[i];
            if (col == null)
            {
                continue;
            }

            if (!overlap && col.bounds.Intersects(playerBounds))
            {
                overlap = true;
            }

            if (combinedPuzzleBounds.size == Vector3.zero)
            {
                combinedPuzzleBounds = col.bounds;
            }
            else
            {
                combinedPuzzleBounds.Encapsulate(col.bounds);
            }
        }

        if (!overlap)
        {
            return;
        }

        Vector3 playerPos = playerController.transform.position;
        Vector3 pushDir = playerPos - combinedPuzzleBounds.center;
        pushDir.y = 0f;
        if (pushDir.sqrMagnitude < 0.001f)
        {
            pushDir = marketVendor != null ? -marketVendor.transform.forward : Vector3.back;
            pushDir.y = 0f;
        }

        pushDir.Normalize();
        Vector3 safePos = playerPos + pushDir * 2.2f;

        playerController.enabled = false;
        playerController.transform.position = safePos;
        playerController.enabled = true;

        PrototypeLogger.Info("BenThanh: Player moved to safe position to avoid puzzle spawn overlap.");
    }
}
