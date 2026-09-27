using UnityEngine;

namespace Server
{
    public class ChestObject : Object
    {
        public bool opened;
    }
    
    public class Chest : Object, Interactable
    {
        public bool opened;

        public void Interact(Player player)
        {
            if(opened)
                return;

            opened = true;
            ServerSend.ChestOpened(id, opened);

            WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, position);
        }
        
        public bool CanInteract() => !opened;
    }
}