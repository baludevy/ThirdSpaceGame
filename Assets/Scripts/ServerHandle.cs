using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

public static class ServerHandle
{
    public static void Username(NetPeer peer, NetDataReader reader)
    {
        string username = reader.GetString();

        if (username.Length == 0)
        {
            username = $"Player{peer.Id}";
        }

        var joinedPlayer = new Player
        {
            id = peer.Id,
            username = username
        };

        foreach (var player in ServerGameManager.Instance.players.Values)
        {
            ServerSend.PlayerJoined(player, joinedPlayer.id);

            if (player.gameObject != null)
            {
                ServerSend.SpawnPlayer(player);
            }
        }

        ServerSend.PlayerJoined(joinedPlayer);

        ServerGameManager.Instance.AddPlayer(joinedPlayer);
        ServerGameManager.Instance.SpawnPlayer(joinedPlayer);

        Debug.Log($"Peer{peer.RemoteId}'s username is {username}");
    }

    public static void PlayerMove(NetPeer peer, NetDataReader reader)
    {
        var position = reader.GetVector2();
        var animationState = (AnimationState)reader.GetByte();

        if (ServerGameManager.Instance.players.ContainsKey(peer.Id))
        {
            var player = ServerGameManager.Instance.players[peer.Id];

            if (player.gameObject != null)
            {
                var playerObject = player.gameObject;

                playerObject.transform.position = position;

                ServerSend.PlayerMove(player, animationState);
            }
        }
    }

    public static void DroppedItem(NetPeer peer, NetDataReader reader)
    {
        var itemType = (ItemType)reader.GetByte();
        var player = ServerGameManager.Instance.players[peer.Id].gameObject.GetComponent<ServerPlayer>();

        player.DropItem(itemType);
    }

    public static void OpenChest(NetPeer peer, NetDataReader reader)
    {
        int chestId = reader.GetInt();

        ServerChestManager.Instance.OpenChest(peer.Id, chestId);
    }
}
