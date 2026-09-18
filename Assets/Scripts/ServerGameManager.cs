using System.Collections.Generic;
using UnityEngine;

public struct Player {
    public int id;
    public string username;
    public GameObject gameObject;
}

public class ServerGameManager : MonoBehaviour {
    public static ServerGameManager Instance;
    
    public Dictionary<int, Player> players = new Dictionary<int, Player>();
    
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

    public void AddPlayer(Player player) {
        players.Add(player.id, player);
    }

    public void RemovePlayer(Player player) {
        players.Remove(player.id);
    }
    
    public void RemovePlayer(int playerId) {
        players.Remove(playerId);
    }
}