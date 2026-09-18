using System.Collections.Generic;
using UnityEngine;


public class ClientGameManager : MonoBehaviour {
    public static ClientGameManager Instance;
    
    public List<Player> players = new List<Player>();
    
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
        players.Add(player);
    }
    
    public void RemovePlayer(Player player) {
        players.Remove(player);
    }
    
    public Player GetPlayer(int id) => players.Find(x => x.id == id);
}