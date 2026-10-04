using System.Collections.Generic;
using LiteNetLib.Utils;
using Game;
using LiteNetLib;
using UnityEngine;

namespace Server
{
    public static class ServerSend
    {
        public static void Welcome(int id)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.Welcome, id, writer =>
                writer.Put(id));
        }

        public static void InitializeWorld(int id, List<Object> objects, List<Entity> entities)
        {
            NetworkManager.Instance.Server.SendPacketTo(
                ServerPacketId.InitializeWorld,
                id,
                writer =>
                {
                    writer.Put(objects.Count);

                    foreach (Object obj in objects)
                        WriteObject(writer, obj);

                    writer.Put(entities.Count);

                    foreach(Entity entity in entities)
                        WriteEntity(writer, entity);
                }
            );
        }

        public static void SpawnEntity(Entity entity)
        {
            NetworkManager.Instance.Server.SendPacketToAll(
                ServerPacketId.SpawnEntity,
                writer => WriteEntity(writer, entity)
            );
        }

        public static void UpdateTiles(List<Tile> updatedTiles)
        {
            NetworkManager.Instance.Server.SendPacketToAll(
                ServerPacketId.UpdateTiles, writer =>
                {
                    writer.Put(updatedTiles.Count);
                    
                    foreach (Tile tile in updatedTiles)
                    {
                        writer.Put(tile.tileId);
                        writer.Put((byte)tile.tileType);
                    }
                });
        }

        public static void UpdateWorld(int targetId, WorldUpdate update)
        {
            NetworkManager.Instance.Server.SendPacketTo(
                ServerPacketId.UpdateWorld,
                targetId,
                writer =>
                {
                    writer.Put(update.tick);
                    writer.Put(update.entityUpdates.Count);

                    foreach(EntityUpdate entityUpdate in update.entityUpdates)
                        WriteEntityUpdate(writer, entityUpdate);
                },
                LiteNetLib.DeliveryMethod.Unreliable
            );
        }

        public static void UpdateInventory(Player player)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.UpdateInventory, player.id, writer =>
                {
                    foreach(Slot slot in player.inventory.slots)
                    {
                        writer.Put((byte)slot.slotIndex);
                        writer.Put((byte)slot.count);

                        if(slot.count > 0)
                            writer.Put((byte)slot.itemType);
                    }
                }
            );
        }

        public static void DestroyEntity(ushort entityId)
        {
            NetworkManager.Instance.Server.SendPacketToAll(
                ServerPacketId.DestroyEntity,
                writer => writer.Put(entityId)
            );
        }

        public static void ChestOpened(ushort objId, bool opened)
        {
            NetworkManager.Instance.Server.SendPacketToAll(
                ServerPacketId.ChestOpened,
                writer =>
                {
                    writer.Put(objId);
                    writer.Put(opened);
                }
            );
        }

        private static void WriteEntity(NetDataWriter writer, Entity entity)
        {
            writer.Put(entity.entityId);
            writer.Put((byte)entity.entityType);
            writer.Put(entity.position);

            switch (entity)
            {
                case Player player:
                    writer.Put(player.id);
                    writer.Put(player.username);
                    break;

                case Pet pet:
                    writer.Put((byte)pet.petType);
                    break;

                case DroppedItem item:
                    writer.Put((byte)item.itemType);
                    break;
            }
        }

        private static void WriteEntityUpdate(NetDataWriter writer, EntityUpdate update)
        {
            writer.Put(update.entityId);
            writer.Put((byte)update.entityType);
            writer.Put(update.position);

            switch (update)
            {
                case PlayerUpdate player:
                    writer.Put((byte)player.animationState);
                    break;

                case PetUpdate pet:
                    writer.Put(pet.facingRight);
                    writer.Put((byte)pet.animationState);
                    break;
            }
        }

        private static void WriteObject(NetDataWriter writer, Object obj)
        {
            writer.Put(obj.id);
            writer.Put((byte)obj.type);
            writer.Put(obj.position);

            if (obj is Chest chest)
                writer.Put(chest.opened);
        }

        internal static void InitalizeWorld(int id, List<Object> objects, List<Entity> entities)
        {
            throw new System.NotImplementedException();
        }
    }
}