using Client;
using UnityEngine;
using Object = Client.Object;

namespace Game
{
    public class Chest : Object, Interactable
    {
        public bool opened;

        public void SetOpened(bool open)
        {
            opened = open;
        }
        
        public void Interact()
        {
            
        }

        public ushort GetId() => id;
        
        public InteractionKind GetInteractionKind() => InteractionKind.Object;

        public bool CanInteract() => !opened;
    }
}
