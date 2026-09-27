using UnityEngine;

namespace Server
{
    public class Entity
    {
        public ushort entityId;
        public EntityType entityType;
        
        // === TRANSFORM ===
        public Vector2 position = Vector2.zero;
    }
}
