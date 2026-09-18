using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class Server {
    private readonly EventBasedNetListener _listener;
    private readonly NetManager _server;

    public bool running { get; private set; }

    public Server() {
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);

        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnClientConnected;
        _listener.PeerDisconnectedEvent += OnClientDisconnected;
        _listener.NetworkReceiveEvent += OnNetworkReceive;

        RegisterPacketHandlers();
    }

    private void RegisterPacketHandlers() {
        PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.Username, ServerHandle.Username);
    }

    public void Start(int port = 2067) {
        Debug.Log($"Starting server on port {port}");
        running = _server.Start(port);

        UnityEngine.Object.Instantiate(PrefabManager.Instance.ServerGameManagerPrefab);
    }

    public void Stop() {
        if (!running)
            return;

        _server.Stop();
        running = false;

        UnityEngine.Object.Destroy(ServerGameManager.Instance.gameObject);

        Debug.Log("Server stopped");
    }

    private void OnConnectionRequest(ConnectionRequest request) {
        Debug.Log($"Connection request from {request.RemoteEndPoint}");

        if (_server.ConnectedPeersCount < NetworkSettings.maxPlayers) {
            request.AcceptIfKey(NetworkSettings.connectionKey);
        }
        else {
            Debug.Log("Connection rejected: server full");
            request.Reject();
        }
    }

    private void OnClientConnected(NetPeer peer) {
        Debug.Log($"Client {peer.Id} connected from {peer.Address}");
    }

    private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        Debug.Log(
            $"Client {peer.Id} disconnected: {disconnectInfo.Reason}"
        );

        if (ServerGameManager.Instance.players.ContainsKey(peer.Id)) {
            ServerGameManager.Instance.RemovePlayer(peer.Id);
        }
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) {
        PacketDispatch.HandleServerPacket(peer, reader);
    }

    public void SendPacket(NetPeer peer, ServerPacketId packetId, Action<NetDataWriter> writeData = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) {
        if (peer == null ||
            peer.ConnectionState != ConnectionState.Connected) {
            Debug.LogWarning("Cannot send packet: peer is not connected");
            return;
        }

        NetDataWriter writer = new NetDataWriter();

        writer.Put((ushort)packetId);

        writeData?.Invoke(writer);

        peer.Send(writer, deliveryMethod);
    }

    public void SendPacketToAll(ServerPacketId packetId, Action<NetDataWriter> writeData = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) {
        NetDataWriter writer = new NetDataWriter();

        writer.Put((ushort)packetId);

        writeData?.Invoke(writer);

        _server.SendToAll(writer, deliveryMethod);
    }
    
    public void SendPacketTo(
        ServerPacketId packetId, int peerId, Action<NetDataWriter> writeData = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) {
    
        NetPeer targetPeer = null;

        foreach (NetPeer peer in _server) {
            if (peer.Id == peerId) {
                targetPeer = peer;
                break;
            }
        }

        if (targetPeer == null ||
            targetPeer.ConnectionState != ConnectionState.Connected) {
            Debug.LogWarning($"Cannot send packet: peer {peerId} is not connected");
            return;
        }

        SendPacket(targetPeer, packetId, writeData, deliveryMethod);
    }

    public void SendPacketToAllExcept(
        ServerPacketId packetId, int peerIdExcept, Action<NetDataWriter> writeData = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) {
        NetDataWriter writer = new NetDataWriter();

        writer.Put((ushort)packetId);
        writeData?.Invoke(writer);

        NetPeer excludedPeer = null;

        foreach (NetPeer peer in _server) {
            if (peer.Id == peerIdExcept) {
                excludedPeer = peer;
                break;
            }
        }

        _server.SendToAll(writer, 0, deliveryMethod, excludedPeer);
    }

    public void Update() {
        if (!running)
            return;

        _server.PollEvents();
    }
}