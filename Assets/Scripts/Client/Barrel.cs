namespace Client
{
    public class Barrel : Object, Interactable
    {
        public void Interact()
        {
            
        }

        public InteractionKind GetInteractionKind() => InteractionKind.Object;

        public bool CanInteract() => true;

        public ushort GetId() => id;
    }
}