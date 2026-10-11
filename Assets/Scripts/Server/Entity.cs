using UnityEngine;

namespace Server
{
    public class Entity
    {
        public ushort entityId;
        public EntityType entityType;

        // === TRANSFORM === 
        public Vector3 position = Vector3.zero;
        public virtual void Tick(float deltaTime)
        {
            
        }

        public virtual EntityUpdate GetUpdate() => new EntityUpdate
        {
            entityId = entityId,
            entityType = entityType,
            position = position
        };
    }
}
