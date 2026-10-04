using LiteNetLib;
using LiteNetLib.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace Server
{
    public static class ServerHandle
    {
        public static void Username(NetPeer peer, NetDataReader reader)
        {
            string username = reader.GetString();

            ServerSend.InitalizeWorld(peer.Id, WorldManager.Instance.objectManager.objects, WorldManager.Instance.entityManager.entities);
            WorldManager.Instance.playerManager.SpawnPlayer(peer.Id, username, Vector3.zero);
        }

        public static void PlayerMove(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;
            Vector3 position = reader.GetVector2();
            AnimationState animState = (AnimationState)reader.GetByte();

            WorldManager.Instance.playerManager.GetPlayer(id).inputManager.AddMoveInput(new MoveInput
            {
                position = position,
                animState = animState
            });
        }

        public static void Interact(NetPeer peer, NetDataReader reader)
        {
            int playerId = peer.Id;

            InteractionKind interactionKind = (InteractionKind)reader.GetByte();
            ushort id = reader.GetUShort();

            WorldManager worldManager = WorldManager.Instance;

            Player player = worldManager.playerManager.GetPlayer(playerId);

            if (interactionKind == InteractionKind.Entity)
            {
                if (WorldManager.Instance.entityManager.GetEntity(id) is Interactable interactable)
                    if (interactable.CanInteract())
                        interactable.Interact(player);
            }
            else if (interactionKind == InteractionKind.Object)
            {
                if (WorldManager.Instance.objectManager.GetObject(id) is Interactable interactable)
                    if (interactable.CanInteract())
                        interactable.Interact(player);
            }
        }

        public static void InventoryMove(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            int fromIndex = reader.GetByte();
            int toIndex = reader.GetByte();

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Move(fromIndex, toIndex);

            ServerSend.UpdateInventory(player);
        }

        public static void InventorySplit(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            int fromIndex = reader.GetByte();
            int toIndex = reader.GetByte();

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Split(fromIndex, toIndex);

            ServerSend.UpdateInventory(player);
        }

        public static void InventoryDrop(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            int fromIndex = reader.GetByte();
            bool split = reader.GetBool();

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Drop(fromIndex, split, player.position - Vector2.up);
        }
    }
}
