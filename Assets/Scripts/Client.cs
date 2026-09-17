using LiteNetLib;
using UnityEngine;

public class Client
{
    private EventBasedNetListener _listener;
    private NetManager _client;

    public bool connected { get; private set; }
    public bool running { get; private set; }

    public Client()
    {
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);

        _listener.PeerConnectedEvent += OnClientConnected;
        _listener.PeerDisconnectedEvent += OnClientDisconnected;
    }

    public void Connect(string ip, int port)
    {
        running = _client.Start();

        if (running)
            _client.Connect(ip, port, NetworkSettings.connectionKey);
    }

    public void Disconnect()
    {
        _client.Stop();
        
        connected = false;
        running = false;
    }

    private void OnClientConnected(NetPeer peer)
    {
        Debug.Log("Client connected");
        connected = true;
    }

    private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log($"Client disconnected: {disconnectInfo.Reason}");
        connected = false;
        running = false;
    }

    public void Update()
    {
        _client.PollEvents();
    }
}