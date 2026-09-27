using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class EntityManager
    {
        public ushort nextEntityID;
        public List<Entity> entities = new List<Entity>();

        public Entity SpawnEntity(EntityType type, Vector2 position, bool broadcast = true)
        {
            Entity entity = type switch
            {
                EntityType.player => new Player(),
                EntityType.pet => new Pet(),
                EntityType.item => new DroppedItem(),
                _ => new Entity()
            };

            entity.entityId = nextEntityID++;
            entity.entityType = type;
            entity.position = position;

            entities.Add(entity);
            
            if(broadcast)
                ServerSend.SpawnEntity(entity);
            
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
