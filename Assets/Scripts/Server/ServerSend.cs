using UnityEngine;

namespace Server
{
    public static class ServerSend
    {
        public static void Welcome(int id)
        {
            NetworkManager.Instance.Server.SendPacketTo(ServerPacketId.Welcome, id, writer => {
                writer.Put(id);
            });
            
            EntityManager.Instance.SpawnEntity(EntityType.player, Vector3.zero, Quaternion.identity);
        }

        public static void SpawnEntity(Entity entity)
        {
            NetworkManager.Instance.Server.SendPacketToAll(ServerPacketId.SpawnEntity, writer =>
            {
                writer.Put(entity.id);
                writer.Put((byte)entity.GetEntityType);
                writer.Put(entity.transform.position);
                writer.Put(entity.transform.rotation);
            });
        }
    }
}
