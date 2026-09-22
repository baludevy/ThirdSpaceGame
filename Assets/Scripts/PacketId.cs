public enum ClientPacketId : ushort
{
    Username = 1,
    PlayerMove,
    DropItem,
    OpenChest
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    PlayerJoined,
    PlayerLeft,
    SpawnPlayer,
    PlayerMove,
    ChestOpened,
    ItemDropped,
    InitializeWorld
}
