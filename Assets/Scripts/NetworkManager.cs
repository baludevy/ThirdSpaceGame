using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [NonSerialized]
    public Client.Client Client = new Client.Client();
    [NonSerialized]
    public Server.Server Server = new Server.Server();

    public bool IsHost => Client.connected && Server.running;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Server.running)
            Server.Update();

        if (Client.running)
            Client.Update();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
        Server.Stop();
    }

    public void StartHost()
    {
        if (Server.running)
        {
            Debug.Log("Already hosting, not starting another server");
            return;
        }

        Server.Start();
        Client.Connect("127.0.0.1", 2067);
    }

    public void StopHost()
    {
        Client.Disconnect();
        Server.Stop();
    }

    public void Join(string ip)
    {
        if (IsHost)
        {
            Debug.Log("Already hosting, not joining another server");
        }

        Client.Connect(ip, 2067);
    }
}
