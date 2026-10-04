public enum ClientPacketId : ushort
{
    Username = 1,
    PlayerMove,
    Interact,
    InventoryMove,
    InventorySplit,
    InventoryDrop, 
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    InitializeWorld,
    SpawnEntity,
    UpdateWorld,
    DestroyEntity,
    UpdateInventory,
    ChestOpened,
}
