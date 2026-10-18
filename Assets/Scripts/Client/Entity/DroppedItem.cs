namespace Client
{
    public class DroppedItem : Entity, Interactable
    {
        public void Interact()
        {
            
        }
        
        public ushort GetId() => entityId;

        public InteractionKind GetInteractionKind() => InteractionKind.Entity;

        public bool CanInteract() => true;
    }
}
