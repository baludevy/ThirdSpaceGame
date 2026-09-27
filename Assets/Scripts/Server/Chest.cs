using UnityEngine;

namespace Server
{
    public class Chest : Object, Interactable
    {
        public bool opened;
        public ItemType itemType;

        public void Interact(Player player)
        {
            if (opened)
                return;

            opened = true;
            ServerSend.ChestOpened(id, opened);

            Vector3 pos = (Vector3)position - Vector3.up * 0.75f;

            if(WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, pos, false) is DroppedItem droppedItem)
            {
                droppedItem.itemType = itemType;
                droppedItem.itemAmount = Random.Range(1, 5);

                ServerSend.SpawnEntity(droppedItem);

            }
        }

        public bool CanInteract() => !opened;
    }
}
