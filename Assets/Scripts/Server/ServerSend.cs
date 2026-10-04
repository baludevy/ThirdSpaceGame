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

        public static void InitializeWorld(int id, List<Tile> tiles, List<Object> objects, List<Entity> entities)
        {
            NetworkManager.Instance.Server.SendPacketTo(
                ServerPacketId.InitializeWorld,
                id,
                writer =>
                {
                    writer.Put(tiles.Count);

                    foreach (Tile tile in tiles)
                        WriteTile(writer, tile);

                    writer.Put(objects.Count);

                    foreach (Object obj in objects)
                        WriteObject(writer, obj);

                    writer.Put(entities.Count);

                    foreach (Entity entity in entities)
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
                        WriteTile(writer, tile);
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

                    foreach (EntityUpdate entityUpdate in update.entityUpdates)
                        WriteEntityUpdate(writer, entityUpdate);
                },
                LiteNetLib.DeliveryMethod.Unreliable
            );
        }

        public static void UpdateInventory(Player player)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.UpdateInventory, player.id, writer =>
                {
                    List<Slot> filledSlots = new List<Slot>();

                    foreach (Slot slot in player.inventory.slots)
                    {
                        if (slot.item != null)
                            filledSlots.Add(slot);
                    }

                    writer.Put(filledSlots.Count);

                    foreach (Slot slot in filledSlots)
                    {
                        writer.Put((byte)slot.slotIndex);
                        writer.Put((byte)slot.item.count);

                        if (slot.item.count > 0)
                            writer.Put((byte)slot.item.type);
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

        private static void WriteTile(NetDataWriter writer, Tile tile)
        {
            writer.Put(tile.tileId);
            writer.Put((byte)tile.tileType);
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
                    writer.Put((byte)item.item.type);
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
    }
}
