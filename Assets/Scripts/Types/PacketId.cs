public enum ClientPacketId : ushort
{
    Username = 1,
    
    PlayerMove,
    
    Interact,
    UseHeldItem,
    
    SwitchHotbarSlot,
    InventoryMove,
    InventorySplit,
    InventoryDrop, 
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    
    InitializeWorld,
    
    SpawnEntity,
    SpawnObject,
    
    DestroyEntity,
    DestroyObject,
    
    UpdateWorld,
    UpdateCrop,
    UpdateTiles,
    UpdateInventory,
    
    ChestOpened,
    ChangeTile,
}
