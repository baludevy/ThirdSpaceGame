using System;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Client
{
    public class Client
    {
        private readonly NetManager _client;
        private readonly EventBasedNetListener _listener;
        private NetPeer _serverPeer;

        public Client()
        {
            _listener = new EventBasedNetListener();
            _client = new NetManager(_listener);

            _listener.PeerConnectedEvent += OnClientConnected;
            _listener.PeerDisconnectedEvent += OnClientDisconnected;
            _listener.NetworkReceiveEvent += OnNetworkReceive;

            RegisterPacketHandlers();
        }

        public bool connected { get; private set; }
        public bool running { get; private set; }

        private void RegisterPacketHandlers()
        {
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.Welcome, ClientHandle.Welcome);
        }

        public void Connect(string ip, int port)
        {
            running = _client.Start();

            if (running)
            {
                _client.Connect(
                    ip,
                    port,
                    NetworkSettings.connectionKey
                );
            }
        }

        public void Disconnect()
        {
            _client.Stop();

            _serverPeer = null;
            connected = false;
            running = false;
        }

        private void OnClientConnected(NetPeer peer)
        {
            Debug.Log("Client connected, sending username to server");

            _serverPeer = peer;
            connected = true;

            NetworkUIManager.Instance.SetConnectionPanel(false);
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log($"Client disconnected: {disconnectInfo.Reason}");

            _serverPeer = null;
            connected = false;
            running = false;

            NetworkUIManager.Instance.SetConnectionPanel(true);
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            PacketDispatch.HandleClientPacket(reader);
        }

        public void SendPacket(ClientPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (_serverPeer == null || _serverPeer.ConnectionState != ConnectionState.Connected)
            {
                Debug.LogWarning("Cannot send packet: client is not connected");
                return;
            }

            var writer = new NetDataWriter();

            writer.Put((ushort)packetId);

            writeData?.Invoke(writer);

            _serverPeer.Send(writer, deliveryMethod);
        }

        public void Update()
        {
            _client.PollEvents();
        }
    }
}
