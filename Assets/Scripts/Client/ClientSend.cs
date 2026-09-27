using LiteNetLib;
using UnityEngine;

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
                    writer.Put((Vector2)position);
                    writer.Put((byte)animState);
                }, DeliveryMethod.Unreliable);
        }
        
        public static void Interact(ushort objectId)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.Interact, writer =>
            {
                writer.Put(objectId);
            });
        }
    }
}
