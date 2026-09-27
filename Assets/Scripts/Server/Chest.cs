using UnityEngine;

namespace Server
{
    public class Chest : Object
    {
        public bool opened;

        public override void Interact(Player player)
        {
            if (opened)
                return;

            opened = true;
            ServerSend.ChestOpened(id, opened);
            
            Debug.Log("Chest opened");

            WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, (Vector3)position - Vector3.up * 0.75f);
        }

        public bool CanInteract() => !opened;
    }
}
