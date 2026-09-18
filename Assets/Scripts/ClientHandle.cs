using LiteNetLib.Utils;
using UnityEngine;

public static class ClientHandle {
    public static void PlayerJoined(NetDataReader reader) {
        int id = reader.GetInt();
        string username =  reader.GetString();
        Vector3 position = reader.GetVector2();
        
        Debug.Log($"{username} joined the game");
    }
}