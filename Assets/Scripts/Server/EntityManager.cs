using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class EntityManager
    {
        public ushort nextEntityID;
        public List<Entity> entities = new List<Entity>();

        public Entity SpawnEntity(EntityType type, Vector3 position, bool broadcast = true)
        {
            Entity entity = type == EntityType.player ? new Player() : new Entity();

            entity.entityId = nextEntityID++;
            entity.position = position;

            entities.Add(entity);
            
            return entity;
        }
        
        public void DestroyEntity(Entity entity)
        {
            ServerSend.DestroyEntity(entity.entityId);
            entities.Remove(entity);
        }
        
        public Entity GetEntity(ushort id) => entities.Find(e => e.entityId == id);
    }
}
