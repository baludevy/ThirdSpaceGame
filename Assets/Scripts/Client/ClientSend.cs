using UnityEngine;
using AnimationState = Game.AnimationState;

namespace Client
{
    public static class ClientSend
    {
        public static void Username(string username)
        {
            NetworkManager.Instance.Client.SendPacket(
                ClientPacketId.Username,
                writer => { writer.Put(username); }
            );
        }

        public static void PlayerMove(Vector3 position, AnimationState animState)
        {
            NetworkManager.Instance.Client.SendPacket(
                ClientPacketId.PlayerMove, writer =>
                {
                    writer.Put(position);
                    writer.Put((byte)animState);
                });
        }
    }
}
