namespace Server
{
    public class DroppedItem : Entity, Interactable
    {
        public Item item;

        public void Interact(Player player)
        {
            if (player.inventory.Add(item))
                WorldManager.Instance.entityManager.DestroyEntity(this);
            
            ServerSend.UpdateInventory(player);
        }

        public bool CanInteract() => true;
    }
}
