using UnityEngine;

namespace Server
{
    
    public class Chest : ServerObject, Interactable
    {
        public bool opened;

        public override ObjectType Type => ObjectType.chest;

        public void Interact(Player player)
        {
            if(opened)
                return;

            opened = true;
            ServerSend.ChestOpened(objectInstance.id, opened);

            EntityManager.Instance.SpawnEntity(EntityType.item, transform.position, Quaternion.identity);
        }
        public bool CanInteract() => !opened;
    }
}