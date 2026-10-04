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

        public static void UseHeldItem(int tileId)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.UseHeldItem, writer =>
            {
                writer.Put(tileId);
            });
        }

        public static void Interact(InteractionKind interactionKind, ushort objectId)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.Interact, writer =>
            {
                writer.Put((byte)interactionKind);
                writer.Put(objectId);
            });
        }

        public static void SwitchHotbarSlot(int toIndex)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.SwitchHotbarSlot, writer =>
            {
                writer.Put((byte)toIndex);
            });
        }

        public static void InventoryMove(int fromContainer, int fromIndex, int toContainer, int toIndex)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.InventoryMove, writer =>
            {
                writer.Put((short)fromContainer);
                writer.Put((byte)fromIndex);
                writer.Put((short)toContainer);
                writer.Put((byte)toIndex);
            });
        }

        public static void InventorySplit(int fromContainer, int fromIndex, int toContainer, int toIndex)
        {
            NetworkManager.Instance.Client  .SendPacket(ClientPacketId.InventorySplit, writer =>
            {
                writer.Put((short)fromContainer);
                writer.Put((byte)fromIndex);
                writer.Put((short)toContainer);
                writer.Put((byte)toIndex);
            });
        }

        public static void InventoryDrop(int fromContainer, int fromIndex, bool drop)
        {
            NetworkManager.Instance.Client.SendPacket(ClientPacketId.InventoryDrop, writer =>
            {
                writer.Put((short)fromContainer);
                writer.Put((byte)fromIndex);
                writer.Put(drop);
            });
        }
    }
}
