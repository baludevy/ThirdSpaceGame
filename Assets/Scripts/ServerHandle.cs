using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public static class ServerHandle {
    public static void Username(NetPeer peer, NetDataReader reader) {
        string username = reader.GetString();

        if (username.Length == 0) {
            username = $"Player{peer.Id}";
        }

        Player joinedPlayer = new Player {
            id = peer.Id,
            username = username
        };

        foreach (Player player in ServerGameManager.Instance.players.Values) {
            ServerSend.PlayerJoined(player, joinedPlayer.id);

            if (player.gameObject != null) {
                ServerSend.SpawnPlayer(player);
            }
        }
        
        ServerSend.PlayerJoined(joinedPlayer);
        
        ServerGameManager.Instance.AddPlayer(joinedPlayer);
        ServerGameManager.Instance.SpawnPlayer(joinedPlayer);

        Debug.Log($"Peer{peer.RemoteId}'s username is {username}");
    }

    public static void PlayerMove(NetPeer peer, NetDataReader reader) {
        Vector2 position = reader.GetVector2();
        AnimationState animationState = (AnimationState)reader.GetByte();

        if (ServerGameManager.Instance.players.ContainsKey(peer.Id)) {
            Player player = ServerGameManager.Instance.players[peer.Id];
            
            if (player.gameObject != null) {
                GameObject playerObject = player.gameObject;
                
                playerObject.transform.position = position;
                
                ServerSend.PlayerMove(player, animationState);
            }
        }
    }

    public static void OpenChest(NetPeer peer, NetDataReader reader) {
        int chestId = reader.GetInt();
        
        ServerChestManager.Instance.OpenChest(peer.Id, chestId);
    }
}