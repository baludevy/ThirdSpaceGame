using System.Collections.Generic;
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
            {
                writer.Put(id);
            });
        }
        public static void InitalizeWorld(int id, List<Object> objects, List<Entity> entities)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.InitializeWorld, id, writer =>
            {
                // === OBJECTS === ///
                writer.Put(objects.Count);

                foreach (Object obj in objects)
                {
                    writer.Put(obj.id);
                    writer.Put((byte)obj.type);

                    writer.Put(obj.position);

                    if (obj.type == ObjectType.chest)
                    {
                        Chest chest = obj as Chest;
                        writer.Put(chest != null && chest.opened);
                    }
                }

                int wrotePlayer = 0;
                
                // === ENTITIES === ///
                writer.Put(entities.Count);

                foreach (Entity entity in entities)
                {
                    writer.Put(entity.entityId);
                    writer.Put((byte)entity.entityType);
                    writer.Put(entity.position);

                    if (entity.entityType == EntityType.player)
                    {
                        if (entity is Player player)
                        {
                            writer.Put(player.id);
                            writer.Put(player.username);
                        }
                    }
                    if (entity.entityType == EntityType.pet)
                    {
                        if (entity is Pet pet)
                        {
                            writer.Put((byte)pet.petType);   
                        }
                    }
                    if (entity.entityType == EntityType.item)
                    {
                        if (entity is DroppedItem item)
                        {
                            writer.Put((byte)item.itemType);
                        }
                    }
                    wrotePlayer++;
                }
            });
        }
        public static void SpawnEntity(Entity entity)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.SpawnEntity, writer =>
            {
                writer.Put(entity.entityId);
                writer.Put((byte)entity.entityType);
                writer.Put(entity.position);

                if (entity.entityType == EntityType.player)
                {
                    if (entity is Player player)
                    {
                        writer.Put(player.id);
                        writer.Put(player.username);
                    }
                }
                if (entity.entityType == EntityType.pet)
                {
                    if (entity is Pet pet)
                    {
                        writer.Put((byte)pet.petType);   
                    }
                }
                if (entity.entityType == EntityType.item)
                {
                    if (entity is DroppedItem item)
                    {
                        writer.Put((byte)item.itemType);
                    }
                }
            });
        }
        public static void UpdateInventory(Player player)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.UpdateInventory, player.id, writer =>
            {
                foreach (Slot slot in player.inventory.slots)
                {
                    writer.Put((byte)slot.slotIndex);
                    writer.Put((byte)slot.count);

                    if(slot.count > 0)
                        writer.Put((byte)slot.itemType);
                }
            });
        }

        public static void UpdateWorld(int targetId, WorldUpdate update)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.UpdateWorld, targetId, writer =>
            {
                writer.Put(update.tick);

                writer.Put(update.entityUpdates.Count);

                foreach (EntityUpdate entityUpdate in update.entityUpdates)
                {
                    writer.Put(entityUpdate.entityId);
                    writer.Put((byte)entityUpdate.entityType);
                    writer.Put(entityUpdate.position);
                    
                    if (entityUpdate is PlayerUpdate playerUpdate)
                    {
                        writer.Put((byte)playerUpdate.animationState);
                    }

                    if (entityUpdate is PetUpdate petUpdate)
                    {
                        writer.Put(petUpdate.facingRight);
                        writer.Put((byte)petUpdate.animationState);
                    }
                }

            }, DeliveryMethod.Unreliable);
        }

        public static void DestroyEntity(ushort entityId)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.DestroyEntity, writer =>
            {
                writer.Put(entityId);
            });
        }

        public static void ChestOpened(ushort objId, bool opened)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.ChestOpened, writer =>
            {
                writer.Put(objId);
                writer.Put(opened);
            });
        }
    }
}
