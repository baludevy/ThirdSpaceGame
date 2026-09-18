using LiteNetLib.Utils;
using UnityEngine;

public static class ClientHandle {
    public static void PlayerJoined(NetDataReader reader) {
        Player joinedPlayer = new Player {
            id = reader.GetInt(),
            username = reader.GetString(),
        };
        
        ClientGameManager.Instance.AddPlayer(joinedPlayer);
        
        Debug.Log($"{joinedPlayer.username} joined the game");
    }
}