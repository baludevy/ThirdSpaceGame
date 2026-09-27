namespace Client
{
    public interface Interactable
    {
        void Interact();
        ushort GetId();
        InteractionKind GetInteractionKind();
        bool CanInteract();
    }
}
