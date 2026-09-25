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

        public static void InitializeWorld(int id, World world)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.InitializeWorld, id, writer =>
            {
                writer.Put(world.entities.Count);

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
            NetworkManager.Instance.Server.SendPacketToAll(
                ServerPacketId.SpawnEntity,
                writer =>
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
                writer.Put(update.playerUpdates.Count);

                foreach (PlayerUpdate playerUpdate in update.playerUpdates)
                {
                    writer.Put(playerUpdate.entityId);
                    writer.Put((Vector2)playerUpdate.position);
                    writer.Put((byte)playerUpdate.animState);
                }
            });
        }
    }
}
