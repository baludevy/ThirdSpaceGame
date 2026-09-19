public enum ClientPacketId : ushort
{
    Username = 1,
    PlayerMove,
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    PlayerJoined,
    PlayerLeft,
    SpawnPlayer,
    PlayerMove,
}