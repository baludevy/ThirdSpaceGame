using UnityEngine;


namespace Server
{
    public class DroppedItem : Entity, Interactable
    {
        public Item item;

        public void Interact(Player player)
        {
            player.inventory.Add(item);
            ServerSend.UpdateInventory(player);

            WorldManager.Instance.entityManager.DestroyEntity(this);
        }

        public bool CanInteract() => true;
    }
}
