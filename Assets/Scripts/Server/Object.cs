using UnityEngine;

namespace Server
{
    public class Object
    {
        public ushort id;
        public ObjectType type;
    
        // === TRANSFORM ===
        public Vector2 position;

        public virtual void Interact(Player player)
        {
            
        }
    }
}