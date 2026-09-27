namespace Client
{
    public interface Interactable
    {
        void Interact();
        ushort GetId();
        bool CanInteract();
    }
}
