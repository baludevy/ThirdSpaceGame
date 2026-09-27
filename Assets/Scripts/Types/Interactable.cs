namespace Server
{
    public interface Interactable
    {
        void Interact(Player player);
        bool CanInteract();
    }
}
