using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace Server
{
    public enum ServerState
    {
        Stopped,
        Starting,
        Running,
        Stopping
    }

    public sealed class Server
    {
        private readonly NetManager _server;
        private readonly Dictionary<int, NetPeer> _peers = new Dictionary<int, NetPeer>();

        private bool _polling;
        private bool _stopRequested;
        private bool _stopping;

        public event Action<ServerState> StateChanged;
        public event Action<int> ClientConnected;
        public event Action<int> ClientDisconnected;

        public ServerState State { get; private set; }
        public string LastError { get; private set; }
        public int Port { get; private set; }

        public int ConnectedClients => _peers.Count;
        public bool running => State == ServerState.Running && _server.IsRunning;

        public Server()
        {
            var listener = new EventBasedNetListener();
            _server = new NetManager(listener) { AutoRecycle = false };

            listener.ConnectionRequestEvent += OnConnectionRequest;
            listener.PeerConnectedEvent += OnClientConnected;
            listener.PeerDisconnectedEvent += OnClientDisconnected;
            listener.NetworkReceiveEvent += OnNetworkReceive;

            RegisterPacketHandlers();
        }

        private static void RegisterPacketHandlers()
        {
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.Username, ServerHandle.Username);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.PlayerMove, ServerHandle.PlayerMove);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.UseHeldItem, ServerHandle.UseHeldItem);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.Interact, ServerHandle.Interact);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.SwitchHotbarSlot, ServerHandle.SwitchHotbarSlot);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.InventoryMove, ServerHandle.InventoryMove);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.InventorySplit, ServerHandle.InventorySplit);
            PacketDispatch.RegisterServerHandler((ushort)ClientPacketId.InventoryDrop, ServerHandle.InventoryDrop);
        }

        public void Start(int port = 2067)
        {
            if (State != ServerState.Stopped || _stopping)
                return;

            if (port < 1 || port > ushort.MaxValue)
            {
                LastError = "Port must be between 1 and 65535.";
                Debug.LogError(LastError);
                return;
            }

            if (WorldManager.Instance == null)
            {
                LastError = "The server world is unavailable.";
                Debug.LogError(LastError);
                return;
            }

            LastError = null;
            Port = port;
            _stopRequested = false;

            SetState(ServerState.Starting);

            if (State != ServerState.Starting)
                return;

            try
            {
                if (!_server.Start(port))
                {
                    LastError = $"Failed to bind the server to port {port}.";

                    Stop();
                    Debug.LogError(LastError);
                    return;
                }

                SetState(ServerState.Running);
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                Debug.LogException(exception);
                Stop();
            }
        }

        public void Stop()
        {
            if (_stopping)
                return;

            if (State == ServerState.Stopped && !_server.IsRunning && _peers.Count == 0)
                return;

            _stopRequested = true;

            SetState(ServerState.Stopping);
            ProcessPendingStop();
        }

        private void ProcessPendingStop()
        {
            if (!_stopRequested || _polling || _stopping)
                return;

            _stopping = true;
            _stopRequested = false;

            var peerIds = new List<int>(_peers.Keys);
            _peers.Clear();

            try
            {
                if (_server.IsRunning)
                    _server.Stop();
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                foreach (int peerId in peerIds)
                {
                    RemovePlayer(peerId);
                    Raise(ClientDisconnected, peerId);
                }

                if (WorldManager.Instance != null)
                    WorldManager.Instance.ClearSession();

                Port = 0;
                SetState(ServerState.Stopped);
                _stopping = false;
            }
        }

        private void OnConnectionRequest(ConnectionRequest request)
        {
            int capacity = Math.Max(0, NetworkSettings.maxPlayers);

            if (!running || _server.ConnectedPeersCount >= capacity)
            {
                request.Reject();
                return;
            }

            request.AcceptIfKey(NetworkSettings.connectionKey);
        }

        private void OnClientConnected(NetPeer peer)
        {
            if (_peers.TryGetValue(peer.Id, out var current))
            {
                if (ReferenceEquals(current, peer))
                    return;

                RemovePlayer(peer.Id);
                _peers.Remove(peer.Id);
            }

            int capacity = Math.Max(0, NetworkSettings.maxPlayers);

            if (!running || _peers.Count >= capacity)
            {
                peer.Disconnect();
                return;
            }

            _peers[peer.Id] = peer;

            try
            {
                ServerSend.Welcome(peer.Id);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                peer.Disconnect();
                return;
            }

            Raise(ClientConnected, peer.Id);
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!_peers.TryGetValue(peer.Id, out var current) || !ReferenceEquals(current, peer))
                return;

            _peers.Remove(peer.Id);
            RemovePlayer(peer.Id);

            Debug.Log($"Client {peer.Id} disconnected: {disconnectInfo.Reason}");

            Raise(ClientDisconnected, peer.Id);
        }

        private static void RemovePlayer(int peerId)
        {
            var world = WorldManager.Instance;

            if (world == null || world.playerManager == null || world.entityManager == null)
                return;

            try
            {
                if (world.playerManager.GetPlayer(peerId) != null)
                    world.playerManager.DestroyPlayer(peerId);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            if (!CanSend(peer))
            {
                reader.Recycle();
                return;
            }

            try
            {
                PacketDispatch.HandleServerPacket(peer, reader);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public bool HasClient(int peerId)
        {
            return _peers.TryGetValue(peerId, out var peer) && CanSend(peer);
        }

        public void DisconnectClient(int peerId)
        {
            if (_peers.TryGetValue(peerId, out var peer))
                peer.Disconnect();
        }

        private bool CanSend(NetPeer peer)
        {
            return running && peer != null &&
                   peer.ConnectionState == ConnectionState.Connected &&
                   _peers.TryGetValue(peer.Id, out var current) &&
                   ReferenceEquals(current, peer);
        }

        private static NetDataWriter CreatePacket(ServerPacketId packetId, Action<NetDataWriter> writeData)
        {
            var writer = new NetDataWriter();

            writer.Put((ushort)packetId);
            writeData?.Invoke(writer);

            return writer;
        }

        public void SendPacket(NetPeer peer, ServerPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            TrySendPacket(peer, packetId, writeData, deliveryMethod);
        }

        public bool TrySendPacket(NetPeer peer, ServerPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (!CanSend(peer))
                return false;

            var writer = CreatePacket(packetId, writeData);

            if (!CanSend(peer))
                return false;

            peer.Send(writer, deliveryMethod);
            return true;
        }

        public void SendPacketTo(ServerPacketId packetId, int peerId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (_peers.TryGetValue(peerId, out var peer))
                SendPacket(peer, packetId, writeData, deliveryMethod);
        }

        public void SendPacketToAll(ServerPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            Broadcast(packetId, null, writeData, deliveryMethod);
        }

        public void SendPacketToAllExcept(ServerPacketId packetId, int peerIdExcept,
            Action<NetDataWriter> writeData = null, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            Broadcast(packetId, peerIdExcept, writeData, deliveryMethod);
        }

        private void Broadcast(ServerPacketId packetId, int? excludedId, Action<NetDataWriter> writeData,
            DeliveryMethod deliveryMethod)
        {
            if (!running || _peers.Count == 0)
                return;

            var writer = CreatePacket(packetId, writeData);
            var peers = new List<NetPeer>(_peers.Values);

            foreach (var peer in peers)
            {
                if (excludedId.HasValue && peer.Id == excludedId.Value)
                    continue;

                if (CanSend(peer))
                    peer.Send(writer, deliveryMethod);
            }
        }

        public void Update()
        {
            ProcessPendingStop();

            if (!running)
            {
                if (State == ServerState.Running)
                {
                    LastError = "The server transport stopped.";
                    Stop();
                }

                return;
            }

            _polling = true;

            try
            {
                _server.PollEvents();
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                Debug.LogException(exception);
                Stop();
            }
            finally
            {
                _polling = false;
                ProcessPendingStop();
            }
        }

        private void SetState(ServerState state)
        {
            if (State == state)
                return;

            State = state;
            Raise(StateChanged, state);
        }

        private static void Raise<T>(Action<T> handlers, T value)
        {
            if (handlers == null)
                return;

            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }
}