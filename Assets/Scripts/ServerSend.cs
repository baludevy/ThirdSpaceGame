using UnityEngine;

public static class ServerSend {
    public static void PlayerJoined(Player player, int toPlayer) {
        NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.PlayerJoined, toPlayer, writer => {
            writer.Put(player.id);
            writer.Put(player.username);
            writer.Put(Vector3.zero);
        });
    }
    
    public static void PlayerJoined(Player player) {
        NetworkManager.Instance.Server.SendPacketToAllExcept(ServerPacketId.PlayerJoined, player.id, writer => {
            writer.Put(player.id);
            writer.Put(player.username);
            writer.Put(Vector3.zero);
        });
    }
}