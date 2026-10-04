public enum ClientPacketId : ushort
{
    Username = 1,
    PlayerMove,
    Interact,
    TileTest,
    InventoryMove,
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    InitializeWorld,
    SpawnEntity,
    UpdateWorld,
    DestroyEntity,
    UpdateTiles,
    UpdateInventory,
    ChestOpened,
}
