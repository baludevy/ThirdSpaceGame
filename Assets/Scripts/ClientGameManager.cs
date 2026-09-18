using System.Collections.Generic;
using UnityEngine;


public class ClientGameManager : MonoBehaviour {
    public static ClientGameManager Instance;
    
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