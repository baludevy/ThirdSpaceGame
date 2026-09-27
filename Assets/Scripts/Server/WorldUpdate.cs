using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class WorldUpdate
    {
        public uint tick;
        public List<EntityUpdate> entityUpdates = new List<EntityUpdate>();
    }

    public class EntityUpdate
    {
        public ushort entityId;
        public EntityType entityType;
        public Vector2 position;
    }

    public class PlayerUpdate : EntityUpdate
    {
        public AnimationState animationState;
    }

    public class PetUpdate : EntityUpdate
    {
        public bool facingRight;
        public PetAnimationState animationState;
    }
}