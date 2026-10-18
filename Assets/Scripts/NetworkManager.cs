using System;
using System.Collections.Generic;
using Game;
using UnityEngine;
using ClientState = global::Client.ClientState;
using ServerState = global::Server.ServerState;

public enum NetworkState
{
    Offline,
    Connecting,
    Connected,
    StartingHost,
    Hosting,
    Reconnecting,
    Stopping
}

public sealed class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [NonSerialized]
    public global::Client.Client Client = new global::Client.Client();

    [NonSerialized]
    public global::Server.Server Server = new global::Server.Server();

    [SerializeField, Range(1, 65535)] private int port = 2067;
    [SerializeField] private bool autoReconnect = true;
    [SerializeField, Min(0)] private int maxReconnectAttempts = 3;
    [SerializeField, Min(0.1f)] private float connectionTimeoutSeconds = 10f;
    [SerializeField, Min(0f)] private float reconnectDelaySeconds = 2f;

    private enum SessionRequest
    {
        None,
        Join,
        Host
    }

    private SessionRequest _pendingSession;
    private string _pendingAddress;
    private int _sessionClearedFrame = -1;

    private bool _hostingRequested;
    private bool _shutdownRequested;
    private bool _shuttingDown;
    private bool _startingSession;
    private bool _ownsInstance;
    private bool _quitting;
    private bool _updating;

    private NetworkUIManager _lastUI;
    private bool? _panelVisible;

    public event Action<NetworkState> StateChanged;
    public event Action<string> ErrorOccurred;

    public NetworkState State { get; private set; }
    public string LastError { get; private set; }

    public int Port => port;
    public bool IsHost => _hostingRequested && Server.running && Client.connected;
    public bool IsConnected => Client.connected;

    public bool IsConnecting => State == NetworkState.Connecting ||
                                State == NetworkState.StartingHost ||
                                State == NetworkState.Reconnecting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _ownsInstance = true;

        Client.SessionCleared += ClearClientSession;

        DontDestroyOnLoad(gameObject);
        QualitySettings.vSyncCount = 1;

        ApplyClientSettings();
    }

    private void OnValidate()
    {
        port = Mathf.Clamp(port, 1, 65535);
        maxReconnectAttempts = Mathf.Max(0, maxReconnectAttempts);
        connectionTimeoutSeconds = Mathf.Max(0.1f, connectionTimeoutSeconds);
        reconnectDelaySeconds = Mathf.Max(0f, reconnectDelaySeconds);
    }

    private void ApplyClientSettings()
    {
        Client.AutoReconnect = autoReconnect;
        Client.MaxReconnectAttempts = Math.Max(0, maxReconnectAttempts);
        Client.ConnectTimeoutSeconds = Math.Max(0.1, connectionTimeoutSeconds);
        Client.ReconnectDelaySeconds = Math.Max(0, reconnectDelaySeconds);
    }

    public void StartHost()
    {
        QueueSession(SessionRequest.Host, null);
    }

    public void Join(string ip)
    {
        QueueSession(SessionRequest.Join, ip);
    }

    public void StopHost()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        _pendingSession = SessionRequest.None;
        _pendingAddress = null;

        RequestShutdown();
    }

    private void QueueSession(SessionRequest request, string address)
    {
        if (!_ownsInstance || _quitting)
            return;

        _pendingSession = request;
        _pendingAddress = address;

        if (State != NetworkState.Offline || Client.IsBusy || Server.State != ServerState.Stopped)
            RequestShutdown();

        ProcessPendingSession();
    }

    private bool CanStart()
    {
        return _ownsInstance && !_quitting && !_updating &&
               !_shuttingDown && !_startingSession && !_shutdownRequested &&
               Time.frameCount > _sessionClearedFrame &&
               State == NetworkState.Offline && !Client.IsBusy &&
               Server.State == ServerState.Stopped;
    }

    private void ProcessPendingSession()
    {
        if (_pendingSession == SessionRequest.None || !CanStart())
            return;

        var request = _pendingSession;
        string address = _pendingAddress;

        _pendingSession = SessionRequest.None;
        _pendingAddress = null;
        _startingSession = true;
        LastError = null;

        try
        {
            ApplyClientSettings();

            if (request == SessionRequest.Host)
                BeginHost();
            else
                BeginJoin(address);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Fail(exception.Message);
        }
        finally
        {
            _startingSession = false;
            ProcessShutdown();
        }
    }

    private void BeginHost()
    {
        _hostingRequested = true;
        SetState(NetworkState.StartingHost);

        if (_shutdownRequested || !_hostingRequested)
            return;

        Server.Start(port);

        if (!Server.running)
        {
            Fail(Server.LastError ?? "The local server could not start.");
            return;
        }

        if (_shutdownRequested)
            return;

        Client.Connect("127.0.0.1", port);

        if (Client.State == ClientState.Disconnected)
            Fail(Client.LastError ?? "The local client could not connect.");

        UpdateConnectionPanel();
    }

    private void BeginJoin(string address)
    {
        _hostingRequested = false;
        SetState(NetworkState.Connecting);

        if (_shutdownRequested || State != NetworkState.Connecting)
            return;

        Client.Connect(address, port);

        if (Client.State == ClientState.Disconnected)
            Fail(Client.LastError ?? "The connection could not start.");

        UpdateConnectionPanel();
    }

    private void RequestShutdown()
    {
        if (!_ownsInstance || _shuttingDown)
            return;

        _shutdownRequested = true;
        SetState(NetworkState.Stopping);

        if (!_updating && !_startingSession)
            ProcessShutdown();
    }

    private void ProcessShutdown()
    {
        if (!_shutdownRequested || _updating || _startingSession || _shuttingDown)
            return;

        _shuttingDown = true;
        _shutdownRequested = false;
        _hostingRequested = false;

        try
        {
            Client.Disconnect();
        }
        finally
        {
            try
            {
                Server.Stop();

                if (global::Server.WorldManager.Instance != null)
                    global::Server.WorldManager.Instance.ClearSession();
            }
            finally
            {
                SetState(NetworkState.Offline);
                _shuttingDown = false;

                if (!_quitting)
                    UpdateConnectionPanel();
            }
        }
    }

    private void ClearClientSession()
    {
        _sessionClearedFrame = Time.frameCount;

        var targets = new HashSet<GameObject>();
        var players = global::Client.PlayerManager.Instance;
        var entities = global::Client.EntityManager.Instance;
        var objects = global::Client.ObjectManager.Instance;

        if (players != null)
        {
            foreach (var player in players.players)
            {
                if (player != null)
                    targets.Add(player.gameObject);
            }

            players.players.Clear();
        }

        if (entities != null)
        {
            foreach (var entity in entities.entities)
            {
                if (entity != null)
                    targets.Add(entity.gameObject);
            }

            entities.entities.Clear();
        }

        if (objects != null)
        {
            foreach (var obj in objects.objects)
            {
                if (obj != null)
                    targets.Add(obj.gameObject);
            }

            objects.objects.Clear();
        }

        if (global::Client.EntityInterpolationManager.Instance != null)
            global::Client.EntityInterpolationManager.Instance.Clear();

        if (global::Client.Inventory.Instance != null)
            global::Client.Inventory.Instance.Clear();

        foreach (var camera in UnityEngine.Object.FindObjectsOfType<MoveCamera>(true))
        {
            if (camera.target == null)
                continue;

            foreach (var target in targets)
            {
                if (target != null && camera.target.IsChildOf(target.transform))
                {
                    camera.target = null;
                    break;
                }
            }
        }

        foreach (var target in targets)
        {
            if (target == null)
                continue;

            target.SetActive(false);
            Destroy(target);
        }

        _lastUI = null;
        _panelVisible = null;
    }

    private void Update()
    {
        if (!_ownsInstance || _quitting)
            return;

        ProcessShutdown();
        _updating = true;

        try
        {
            Server.Update();

            if (!_shutdownRequested)
                Client.Update();

            if (!_shutdownRequested)
                RefreshState();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Fail(exception.Message);
        }
        finally
        {
            _updating = false;
            ProcessShutdown();
            UpdateConnectionPanel();
            ProcessPendingSession();
        }
    }

    private void RefreshState()
    {
        if (_hostingRequested)
        {
            RefreshHostState();
            return;
        }

        if (Server.State != ServerState.Stopped)
        {
            Fail("An unexpected local server is running.");
            return;
        }

        switch (Client.State)
        {
            case ClientState.Connecting:
                SetState(NetworkState.Connecting);
                break;

            case ClientState.Connected:
                SetState(NetworkState.Connected);
                break;

            case ClientState.Reconnecting:
                SetState(NetworkState.Reconnecting);
                break;

            case ClientState.Disconnecting:
                SetState(NetworkState.Stopping);
                break;

            default:
                HandleOfflineClient();
                break;
        }
    }

    private void RefreshHostState()
    {
        if (!Server.running)
        {
            Fail(Server.LastError ?? "The local server stopped.");
            return;
        }

        switch (Client.State)
        {
            case ClientState.Connected:
                SetState(NetworkState.Hosting);
                break;

            case ClientState.Connecting:
                SetState(NetworkState.StartingHost);
                break;

            case ClientState.Reconnecting:
                SetState(NetworkState.Reconnecting);
                break;

            case ClientState.Disconnecting:
                SetState(NetworkState.Stopping);
                break;

            default:
                Fail(Client.LastError ?? "The local client disconnected.");
                break;
        }
    }

    private void HandleOfflineClient()
    {
        if (State == NetworkState.Offline)
            return;

        if (!string.IsNullOrEmpty(Client.LastError))
        {
            Fail(Client.LastError);
            return;
        }

        RequestShutdown();
    }

    private void Fail(string message)
    {
        LastError = message;

        if (!string.IsNullOrEmpty(message))
            Debug.LogWarning(message);

        RequestShutdown();
        Raise(ErrorOccurred, message);
    }

    private void UpdateConnectionPanel()
    {
        if (_quitting)
            return;

        var ui = NetworkUIManager.Instance;

        if (ui == null)
        {
            _lastUI = null;
            _panelVisible = null;
            return;
        }

        if (_lastUI != ui)
        {
            _lastUI = ui;
            _panelVisible = null;
        }

        bool visible = !Client.connected;

        if (_panelVisible == visible)
            return;

        ui.SetConnectionPanel(visible);
        _panelVisible = visible;
    }

    private void SetState(NetworkState state)
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

    private void OnApplicationQuit()
    {
        if (!_ownsInstance)
            return;

        _quitting = true;
        _pendingSession = SessionRequest.None;
        _pendingAddress = null;

        RequestShutdown();
    }

    private void OnDestroy()
    {
        if (!_ownsInstance)
            return;

        _quitting = true;
        _pendingSession = SessionRequest.None;
        _pendingAddress = null;

        try
        {
            RequestShutdown();
        }
        finally
        {
            Client.SessionCleared -= ClearClientSession;
            _ownsInstance = false;

            if (Instance == this)
                Instance = null;
        }
    }
}