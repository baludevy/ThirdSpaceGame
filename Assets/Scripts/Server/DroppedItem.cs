using UnityEngine;


namespace Server
{
    public class DroppedItem : Entity, Interactable
    {
        public ItemType itemType;
        public int itemAmount = 1;

        public void Interact(Player player)
        {
            player.inventory.Add(itemType, itemAmount);
            ServerSend.UpdateInventory(player);

            WorldManager.Instance.entityManager.DestroyEntity(this);
        }
        
        public bool CanInteract() => true;
    }
}