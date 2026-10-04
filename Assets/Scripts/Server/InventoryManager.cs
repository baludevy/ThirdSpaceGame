namespace Server
{
    
    public static class InventoryManager
    {
        
        public static bool Move(int playerId, short fromContainer, int fromIndex, short toContainer, int toIndex)
        {
            Inventory from = GetInventory(playerId, fromContainer);
            Inventory to = GetInventory(playerId, toContainer);

            if (from == null || to == null)
                return false;

            return from.TransferTo(to, fromIndex, toIndex);
        }

        public static bool Split(int playerId, short fromContainer, int fromIndex, short toContainer, int toIndex)
        {
            Inventory from = GetInventory(playerId, fromContainer);
            Inventory to = GetInventory(playerId, toContainer);

            if (from == null || to == null)
                return false;
            
            return from.TransferTo(to, fromIndex, toIndex, split: true);
        }

        private static Inventory GetInventory(int playerId, short containerId)
        {
            WorldManager world = WorldManager.Instance;
            Player player = world.playerManager.GetPlayer(playerId);

            if (player == null)
                return null;

            if (containerId == -1)
                return player.inventory;

            if (containerId< 0)
                return null;

            if (world.objectManager.GetObject((ushort)containerId) is Barrel barrel)
            {
                //todo: validation
                return barrel.inventory;
            }

            return null;
        }
    }
}