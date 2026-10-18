using System;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using LiteNetLib;
using LiteNetLib.Utils;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager.Requests;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace Client
{
    public enum ClientState
    {
        Disconnect,
        Connecting,
        Connected,
        Reconnecting,
        Disconnecting
    }

    public sealed class Client
    {
        private readonly NetManager _client;
        private readonly Stopwatch _clock;
        private NetPeer _serverPeer;

        private string _address;
        private int _port;
        private int _reconnectAttempts;
        private double _connectionStartedAt;
        private double _reconnectAt;

        private bool _polling;
        private bool _stopping;
        private bool _stopRequested;
        private bool _retryRequested;

        public event Action<ClientState> StateChanged;
        public event Action Connected;
        public event Action<string> Disconnected;
        public event Action<int> ReconnectScheduled;
        public event Action SessionCleared;

        public int myId = -1;

        public ClientState State { get; private set; }
        public string LastError { get; private set; }

        public string address => _address;
        public int Port => _port;
        public int ReconnectAttempts => _reconnectAttempts;

        public bool AutoReconnect { get; set; } = true;
        public int MaxReconnectAttempts { get; set; } = 3;
        public double ConnectTimeoutSeconds { get; set; } = 10;
        public double ReconnectDelaySeconds { get; set; } = 2;

        public bool running => _client.IsRunning;
        public bool IsBusy => State != ClientState.Disconnect || _stopping;

        public bool connected => State == ClientState.Connected &&
                                 _serverPeer != null &&
                                 _serverPeer.ConnectionState == ConnectionState.Connected;

        private double Now => _clock.Elapsed.TotalSeconds;

        public Client()
        {
            var listener = new EventBasedNetListener();
            _client = new NetManager(listener) { AutoRecycle = false};
            _clock = Stopwatch.StartNew();

            listener.PeerConnectedEvent += OnClientConnected;
            listener.PeerDisconnectedEvent += OnClientDisconnected;
            listener.NetworkReceiveEvent += OnNetworkRecieve;

            RegisterPacketHandlers();
        }

        private static void RegisterPacketHandlers()
        {
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.Welcome, ClientHandle.Welcome);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.InitializeWorld, ClientHandle.InitializeWorld);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.SpawnEntity, ClientHandle.SpawnEntity);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.SpawnObject, ClientHandle.SpawnEntity);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.DestroyObject, ClientHandle.DestroyObject);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.UpdateWorld, ClientHandle.UpdateWorld);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.DestroyEntity, ClientHandle.DestroyEntity);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.UpdateInventory, ClientHandle.UpdateInventory);
            PacketDispatch.RegisterClientHandler((ushort)ServerPacketId.ChestOpened, ClientHandle.ChestOpened);
        }

        public void Connect(string ip, int port)
        {
            if (IsBusy)
                return;

            if (port < 1 || port > ushort.MaxValue)
            {
                LastError = "Port must be between 1 and 65535.";
                Debug.LogError(LastError);
                return;
            }

            _address = string.IsNullOrWhiteSpace(ip) ? "127.0.0.1" : ip.Trim();
            _port = port;
            _reconnectAttempts = 0;
            LastError = null;

            BeginConnection();
        }

        private void BeginConnection()
        {
            _stopRequested = false;
            _retryRequested = false;
            _connectionStartedAt = Now;
            _reconnectAt = 0;
            myId = -1;

            SetState(ClientState.Connecting);

            if (State != ClientState.Connecting || _stopRequested)
                return;

            try
            {
                if (!_client.Start())
                {
                    RequestStop("Failed to start the client transport.", true);
                }
                else
                {
                    _serverPeer = _client.Connect(_address, _port, NetworkSettings.connectionKey);

                    if (_serverPeer == null)
                        RequestStop("Failed to initiate the connection.", true);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                RequestStop(exception.Message, true);
            }

            ProcessPendingStop();
        }

        public void Disconnect()
        {
            _reconnectAttempts = 0;
            LastError = null;

            RequestStop(null, false);
            ProcessPendingStop();
        }

        private void RequestStop(string reason, bool allowRetry)
        {
            if (_stopRequested)
            {
                if (!allowRetry)
                {
                    _retryRequested = false;
                    LastError = reason;
                }

                return;
            }

            LastError = reason;
            _stopRequested = true;
            _retryRequested = allowRetry;
            myId = -1;

            SetState(ClientState.Disconnecting);
        }

        private void ProcessPendingStop()
        {
            if (!_stopRequested || _polling || _stopping)
                return;

            _stopping = true;

            try
            {
                StopTransport();

                bool retry = _retryRequested && AutoReconnect &&
                             _reconnectAttempts < Math.Max(0, MaxReconnectAttempts);

                string reason = LastError;

                _stopRequested = false;
                _retryRequested = false;

                if (retry)
                {
                    _reconnectAttempts++;
                    _reconnectAt = Now + Math.Max(0, ReconnectDelaySeconds);

                    SetState(ClientState.Reconnecting);

                    if (State == ClientState.Reconnecting)
                        Raise(ReconnectScheduled, _reconnectAttempts);
                }
                else
                {
                    _address = null;
                    _port = 0;
                    _reconnectAttempts = 0;

                    SetState(ClientState.Disconnect);
                    Raise(Disconnected, reason);
                }
            }
            finally
            {
                _stopping = false;
            }
        }

        private void StopTransport()
        {
            _serverPeer = null;
            myId = -1;
            _connectionStartedAt = 0;
            _reconnectAt = 0;

            try
            {
                if (_client.IsRunning)
                    _client.Stop();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                Raise(SessionCleared);
            }
        }

        private void OnClientConnected(NetPeer peer)
        {
            if (State != ClientState.Connecting || !ReferenceEquals(peer, _serverPeer))
            {
                peer.Disconnect();
                return;
            }

            LastError = null;
            _reconnectAttempts = 0;

            SetState(ClientState.Connected);

            if (connected)
                Raise(Connected);
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            if (!ReferenceEquals(peer, _serverPeer))
                return;

            RequestStop($"Connection closed: {disconnectInfo}", true);
        }

        private void  OnNetworkRecieve(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            if(!connected || !ReferenceEquals(peer, _serverPeer))
            {
                reader.Recycle();
                return;
            }

            try
            {
                PacketDispatch.HandleClientPacket(reader);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        public void SendPacket(ClientPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            TrySendPacket(packetId, writeData, deliveryMethod);
        }

        public bool TrySendPacket(ClientPacketId packetId, Action<NetDataWriter> writeData = null,
            DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (!connected)
                return false;

            var peer = _serverPeer;
            var writer = new NetDataWriter();

            writer.Put((ushort)packetId);
            writeData?.Invoke(writer);

            if (!connected || !ReferenceEquals(peer, _serverPeer))
                return false;

            peer.Send(writer, deliveryMethod);
            return true;
        }

        public void Update()
        {
            ProcessPendingStop();

            if (State == ClientState.Reconnecting)
            {
                if (Now >= _reconnectAt)
                    BeginConnection();
                
                return;
            }

            if (!_client.IsRunning)
            {
                if (State == ClientState.Connecting || State == ClientState.Connected)
                {
                    RequestStop("The client transport stopped.", true);
                    ProcessPendingStop();
                }

                return;
            }

            _polling = true;

            try
            {
                _client.PollEvents();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                RequestStop("The client failed while processing events.", true);
            }
            finally
            {
                _polling = false;
                ProcessPendingStop();
            }

            if (State == ClientState.Connecting &&
                Now - _connectionStartedAt >= Math.Max(0.1, ConnectTimeoutSeconds))
            {
                RequestStop("The connection attempt timed out,", true);
                ProcessPendingStop();
            }
        }

        private void SetState(ClientState state)
        {
            if (State == state)
                return;

            State = state;
            Raise(StateChanged, state);
        }

        private static void Raise(Action handlers)
        {
            if (handlers == null)
                return;

            foreach (Action handler in handlers.GetInvocationList())
            {
                try
                {
                    handler();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
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