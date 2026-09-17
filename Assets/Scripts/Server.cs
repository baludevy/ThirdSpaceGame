using LiteNetLib;
using UnityEngine;

public class Server
{
    private readonly EventBasedNetListener _listener;
    private readonly NetManager _server;

    public bool running { get; private set; }

    public Server()
    {
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);

        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnClientConnected;
        _listener.PeerDisconnectedEvent += OnClientDisconnected;
    }

    public void Start(int port = 2067)
    {
        Debug.Log($"Starting server on port {port}");

        running = _server.Start(port);
    }

    public void Stop()
    {
        if (!running)
            return;

        _server.Stop();
        running = false;

        Debug.Log("Server stopped");
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.Log($"Connection request from {request.RemoteEndPoint}");

        if (_server.ConnectedPeersCount < NetworkSettings.maxPlayers)
        {
            request.AcceptIfKey(NetworkSettings.connectionKey);
        }
        else
        {
            Debug.Log("Connection rejected: server full");
            request.Reject();
        }
    }

    private void OnClientConnected(NetPeer peer)
    {
        Debug.Log($"{peer.Address} connected");
    }

    private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log(
            $"{peer.Address} disconnected: {disconnectInfo.Reason}"
        );
    }

    public void Update()
    {
        if (!running)
            return;

        _server.PollEvents();
    }
}