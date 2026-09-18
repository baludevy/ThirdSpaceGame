public enum ClientPacketId : ushort
{
    Username = 1,
}

public enum ServerPacketId : ushort
{
    Welcome = 1,
    PlayerJoined,
    PlayerLeft,
    SpawnPlayer,
}