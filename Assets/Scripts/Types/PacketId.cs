public enum ClientPacketId : ushort
{
    Username = 1,
    PlayerMove,
    Interact
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    InitializeWorld,
    SpawnEntity,
    UpdateWorld,
    DestroyEntity,
    ChestOpened,
}
