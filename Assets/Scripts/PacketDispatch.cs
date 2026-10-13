using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

public static class PacketDispatch
{
    public delegate void ClientPacketHandler(NetDataReader reader);

    public delegate void ServerPacketHandler(NetPeer peer, NetDataReader reader);

    private static readonly Dictionary<ushort, ServerPacketHandler> ServerHandlers = new Dictionary<ushort, ServerPacketHandler>();

    private static readonly Dictionary<ushort, ClientPacketHandler> ClientHandlers = new Dictionary<ushort, ClientPacketHandler>();

    public static void RegisterServerHandler(ushort packetId, ServerPacketHandler handler)
    {
        ServerHandlers[packetId] = handler;
    }

    public static void RegisterClientHandler(ushort packetId, ClientPacketHandler handler)
    {
        ClientHandlers[packetId] = handler;
    }

    public static void HandleServerPacket(NetPeer peer, NetPacketReader reader)
    {
        try
        {
            ushort packetId = reader.GetUShort();
            ServerHandlers[packetId](peer, reader);
        }
        finally
        {
            reader.Recycle();
        }
    }

    public static void HandleClientPacket(NetPacketReader reader)
    {
        try
        {
            ushort packetId = reader.GetUShort();
            ClientHandlers[packetId](reader);
        }
        finally
        {
            reader.Recycle();
        }
    }

    public static void RemoveServerHandler(ushort packetId)
    {
        ServerHandlers.Remove(packetId);
    }

    public static void RemoveClientHandler(ushort packetId)
    {
        ClientHandlers.Remove(packetId);
    }

    public static void Clear()
    {
        ServerHandlers.Clear();
        ClientHandlers.Clear();
    }
}
