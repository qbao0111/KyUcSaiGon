using UnityEngine;

public class BusHubMapBoardInteractable : MonoBehaviour, IInteractable
{
    public BusHubMapUIController routeMapUI;

    public string InteractionPrompt => "Nhấn E để mở bản đồ lộ trình";

    public void Interact(Interactor interactor)
    {
        if (routeMapUI == null)
        {
            routeMapUI = FindObjectOfType<BusHubMapUIController>();
        }

        if (routeMapUI != null)
        {
            routeMapUI.OpenMap();
        }
    }
}
