using UnityEngine;

namespace Server
{
    public class Object
    {
        public ushort id;
        public ObjectType type;
    
        // === TRANSFORM ===
        public Vector3 position;

        public virtual void Start()
        {
            
        }
    }
}