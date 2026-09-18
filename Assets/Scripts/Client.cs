using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public class Client {
    private EventBasedNetListener _listener;
    private NetManager _client;
    private NetPeer _serverPeer;

    public bool connected { get; private set; }
    public bool running { get; private set; }

    public Client() {
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);

        _listener.PeerConnectedEvent += OnClientConnected;
        _listener.PeerDisconnectedEvent += OnClientDisconnected;
        _listener.NetworkReceiveEvent += OnNetworkReceive;

        RegisterPacketHandlers();
    }

    private void RegisterPacketHandlers() {
        PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.Welcome, ClientHandle.Welcome);
        PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.PlayerJoined, ClientHandle.PlayerJoined);
        PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.PlayerLeft, ClientHandle.PlayerLeft);
        PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.SpawnPlayer, ClientHandle.SpawnPlayer);
    }

    public void Connect(string ip, int port) {
        running = _client.Start();

        if (running) {
            _client.Connect(
                ip,
                port,
                NetworkSettings.connectionKey
            );
        }
    }

    public void Disconnect() {
        _client.Stop();

        _serverPeer = null;
        connected = false;
        running = false;
    }

    private void OnClientConnected(NetPeer peer) {
        Debug.Log("Client connected, sending username to server");
        
        _serverPeer = peer;
        connected = true;

        UnityEngine.Object.Instantiate(PrefabManager.Instance.ClientGameManagerPrefab);
        
        NetworkUIManager.Instance.SetConnectionPanel(false);
    }

    private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
        Debug.Log($"Client disconnected: {disconnectInfo.Reason}");
        
        _serverPeer = null;
        connected = false;
        running = false;

        foreach (Player player in ClientGameManager.Instance.players.Values) {
            if (player.gameObject != null) {
                UnityEngine.Object.Destroy(player.gameObject);
            }
        }
        
        ClientGameManager.Instance.players.Clear();
        
        UnityEngine.Object.Destroy(ClientGameManager.Instance.gameObject);
        
        NetworkUIManager.Instance.SetConnectionPanel(true);
    }

    private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) {
        PacketDispatch.HandleClientPacket(reader);
    }

    public void SendPacket(ClientPacketId packetId, Action<NetDataWriter> writeData = null,
        DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) {
        if (_serverPeer == null || _serverPeer.ConnectionState != ConnectionState.Connected) {
            Debug.LogWarning("Cannot send packet: client is not connected");
            return;
        }

        NetDataWriter writer = new NetDataWriter();

        writer.Put((ushort)packetId);

        writeData?.Invoke(writer);

        _serverPeer.Send(writer, deliveryMethod);
    }

    public void Update() {
        _client.PollEvents();
    }
}