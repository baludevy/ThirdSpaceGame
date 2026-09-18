using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public static class PacketDispatch {
    public delegate void ServerPacketHandler(
        NetPeer peer,
        NetDataReader reader
    );
    
    public delegate void ClientPacketHandler(
        NetDataReader reader
    );

    private static readonly Dictionary<ushort, ServerPacketHandler>
        ServerHandlers = new();

    private static readonly Dictionary<ushort, ClientPacketHandler>
        ClientHandlers = new();

    public static void RegisterServerHandler(
        ushort packetId,
        ServerPacketHandler handler) {
        ServerHandlers[packetId] = handler;
    }

    public static void RegisterClientHandler(
        ushort packetId,
        ClientPacketHandler handler) {
        ClientHandlers[packetId] = handler;
    }

    public static void HandleServerPacket(
        NetPeer peer,
        NetPacketReader reader) {
        if (reader.AvailableBytes < sizeof(ushort)) {
            Debug.LogWarning(
                $"Received invalid packet from {peer.Address}: no packet ID"
            );

            reader.Recycle();
            return;
        }

        ushort packetId = reader.GetUShort();

        if (ServerHandlers.TryGetValue(packetId, out var handler)) {
            try {
                handler(peer, reader);
            }
            catch (Exception exception) {
                Debug.LogError(
                    $"Error handling server packet {packetId}:\n{exception}"
                );
            }
        }
        else {
            Debug.LogWarning(
                $"Server received unknown packet ID: {packetId}"
            );
        }

        reader.Recycle();
    }

    public static void HandleClientPacket(
        NetPacketReader reader) {
        if (reader.AvailableBytes < sizeof(ushort)) {
            Debug.LogWarning(
                "Received invalid packet from server: no packet ID"
            );

            reader.Recycle();
            return;
        }

        ushort packetId = reader.GetUShort();

        if (ClientHandlers.TryGetValue(packetId, out var handler)) {
            try {
                handler(reader);
            }
            catch (Exception exception) {
                Debug.LogError(
                    $"Error handling client packet {packetId}:\n{exception}"
                );
            }
        }
        else {
            Debug.LogWarning(
                $"Client received unknown packet ID: {packetId}"
            );
        }

        reader.Recycle();
    }

    public static void RemoveServerHandler(ushort packetId) {
        ServerHandlers.Remove(packetId);
    }

    public static void RemoveClientHandler(ushort packetId) {
        ClientHandlers.Remove(packetId);
    }

    public static void Clear() {
        ServerHandlers.Clear();
        ClientHandlers.Clear();
    }
}