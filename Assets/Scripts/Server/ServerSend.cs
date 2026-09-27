using Game;
using LiteNetLib;
using UnityEngine;

namespace Server
{
    public static class ServerSend
    {
        public static void Welcome (int id)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.Welcome, id, writer =>
            {
                writer.Put(id);
            });
        }
        public static void InitalizeWorld(int id, World world)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.InitializeWorld, id, writer =>
            {
                writer.Put(world.objects.Count);
                foreach (Object obj in world.objects)
                {
                 writer.Put(obj.id);
                 writer.Put((byte)obj.type);

                 writer.Put((Vector2)obj.go.transform.position);
                 if (obj.type == ObjectType.chest)
                    {
                        ChestObject chestObj = obj as ChestObject;
                        writer.Put(chestObj != null && chestObj.opened);
                    }   
                }
                writer.Put(world.entities.Count);

                Debug.Log($"sending{world.entities.Count} entities");
                foreach (Entity entity in world.entities)
                {
                writer.Put(entity.entityId);
                writer.Put((byte)entity.GetEntityType);
                writer.Put(entity.transform.position);
                writer.Put(entity.transform.rotation);
                if (entity.GetEntityType == EntityType.player)
                    {
                        Player player = entity.GetComponent<Player>();
                        writer.Put(player.id);
                        writer.Put(player.username);
                    }    
                }
            });
        }
        public static void SpawnEntity(Entity entity)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.SpawnEntity,writer =>
            {
                writer.Put(entity.entityId);
                writer.Put((byte)entity.GetEntityType);
                writer.Put(entity.transform.position);
                writer.Put(entity.transform.rotation);
                if (entity.GetEntityType == EntityType.player)
                {
                    Player player = entity.GetComponent<Player>();
                    writer.Put(player.id);
                    writer.Put(player.username);
                }
            });
        }

        public static void UpdateWorld(int targetId, WorldUpdate update)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.UpdateWorld, targetId, writer =>
            {
                writer.Put(update.tick);

                writer.Put(update.playerUpdates.Count);

                foreach (PlayerUpdate playerUpdate in update.playerUpdates)
                {
                    writer.Put(playerUpdate.entityId);
                    writer.Put((Vector2)playerUpdate.position);
                    writer.Put((byte)playerUpdate.animState);
                    }
                
                
            }, DeliveryMethod.Unreliable);
        }
        public static void ChestOpened(ushort objId, bool opened)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.ChestOpened,writer =>
            {
                NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.ChestOpened,writer =>
                {
                    writer.Put(objId);
                    writer.Put(opened);
                });
            });
        }
    }
}