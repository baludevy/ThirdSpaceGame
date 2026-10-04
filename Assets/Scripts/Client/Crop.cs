using Types;

namespace Client
{
    public class Crop : Object, Interactable
    {
        public CropType cropType;
        public int cropPhase;

        public void Interact()
        {
            
        }
        
        public InteractionKind GetInteractionKind() => InteractionKind.Object;

        public ushort GetId() => id;
        
        public bool CanInteract() => cropPhase == 2;
    }
}
