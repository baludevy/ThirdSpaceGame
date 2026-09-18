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
    
    public static void PlayerMove(Player player) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.PlayerMove, player.id, writer => {
            writer.Put(player.id);
            writer.Put(player.gameObject.transform.position);
        });
    }
}