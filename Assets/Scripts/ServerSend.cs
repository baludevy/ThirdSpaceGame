using LiteNetLib;
using UnityEngine;

public static class ServerSend {
    public static void Welcome(int id) {
        NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.Welcome, id, writer => {
            writer.Put(id);
        });
    }
    
    public static void PlayerJoined(Player player, int toPlayer) {
        NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.PlayerJoined, toPlayer, writer => {
            writer.Put(player.id);
            writer.Put(player.username);
        });
    }
    
    public static void PlayerJoined(Player player) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.PlayerJoined, player.id, writer => {
            writer.Put(player.id);
            writer.Put(player.username);
        });
    }
    
    public static void PlayerLeft(int player) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.PlayerLeft, player, writer => {
            writer.Put(player);
        });
    }
    
    public static void SpawnPlayer(Player player) {
        NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.SpawnPlayer, writer => {
            writer.Put(player.id);
        });
    }
    
    public static void PlayerMove(Player player, AnimationState animationState) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.PlayerMove, player.id, writer => {
            writer.Put(player.id);
            writer.Put((Vector2)player.gameObject.transform.position);
            writer.Put((byte)animationState);
        }, DeliveryMethod.ReliableUnordered);
    }

    public static void ChestOpened(int fromId, int chestId) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.ChestOpened, fromId, writer => {
            writer.Put(chestId);
        });
    }
}