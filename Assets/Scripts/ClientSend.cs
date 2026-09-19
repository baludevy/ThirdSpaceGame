using UnityEngine;

public static class ClientSend {
    public static void Username(string username) {
        NetworkManager.Instance.Client.SendPacket(
            ClientPacketId.Username,
            writer => { writer.Put(username); }
        );
    }

    public static void PlayerMove(Vector2 position, AnimationState animationState) {
        NetworkManager.Instance.Client.SendPacket(
            ClientPacketId.PlayerMove,
            writer => {
                writer.Put(position);
                writer.Put((byte)animationState);
            }
        );
    }
}