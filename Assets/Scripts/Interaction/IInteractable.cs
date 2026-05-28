public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(Interactor interactor);
}
