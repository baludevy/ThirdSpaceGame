using LiteNetLib;
using LiteNetLib.Utils;
using Types;
using Unity.VisualScripting;
using UnityEngine;

namespace Server
{
    public static class ServerHandle
    {
        public static void Username(NetPeer peer, NetDataReader reader)
        {
            string username = reader.GetString();

            ServerSend.InitializeWorld(peer.Id, WorldManager.Instance.tileManager.changedTiles,
                WorldManager.Instance.objectManager.objects, WorldManager.Instance.entityManager.entities);
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

        public static void UseHeldItem(NetPeer peer, NetDataReader reader)
        {
            int playerId = peer.Id;
            int tileId = reader.GetInt();

            Tile targetTile = WorldManager.Instance.tileManager.GetTile(tileId);
            
            if(targetTile.tileType == TileType.Grass)
                WorldManager.Instance.tileManager.ModifyTile(tileId, TileType.Soil);
            else if(targetTile.tileType == TileType.Soil)
                WorldManager.Instance.tileManager.ModifyTile(tileId, TileType.Grass);
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

            short fromContainer = reader.GetShort();
            int fromIndex = reader.GetByte();
            short toContainer = reader.GetShort();
            int toIndex = reader.GetByte();

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Move(fromIndex, toIndex);
            InventoryManager.Move(id, fromContainer, fromIndex, toContainer, toIndex);
            ServerSend.UpdateInventory(WorldManager.Instance.playerManager.GetPlayer(id));
        }

        public static void InventorySplit(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            short fromContainer = reader.GetShort();
            int fromIndex = reader.GetByte();
            short toContainer = reader.GetShort();
            int toIndex = reader.GetByte();

            InventoryManager.Split(id, fromContainer, fromIndex, toContainer, toIndex);

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Split(fromIndex, toIndex);

            ServerSend.UpdateInventory(WorldManager.Instance.playerManager.GetPlayer(id));
        }

        public static void InventoryDrop(NetPeer peer, NetDataReader reader)
        {
            int id = peer.Id;

            int fromContainer = reader.GetShort();
            int fromIndex = reader.GetByte();
            bool split = reader.GetBool();
            
            Debug.Log(fromIndex);

            Player player = WorldManager.Instance.playerManager.GetPlayer(id);
            player.inventory.Drop(fromIndex, split, player.position - Vector2.up);
        }
    }
}
