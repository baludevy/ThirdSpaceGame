using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    [NonSerialized] public Server Server = new Server();
    [NonSerialized] public Client Client = new Client();
    
    public static NetworkManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void StartServer() {
        Server.Start();
    }

    public void StopServer() {
        Server.Stop();
    }

    public void ConnectClient() {
        Client.Connect("127.0.0.1", 2067);
    }

    public void DisconnectClient() {
        Client.Disconnect();
    }

    private void Update()
    {
        if (Server.running)
            Server.Update();

        Client.Update();
    }
}