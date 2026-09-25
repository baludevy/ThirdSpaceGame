using System.Collections.Generic;
using UnityEngine;
using AnimationState = Game.AnimationState;

namespace Server
{
    public class WorldUpdate
    {
        public uint tick;
        public List<PlayerUpdate> playerUpdates = new List<PlayerUpdate>();
    }

    public struct PlayerUpdate
    {
        public ushort entityId;
        public Vector3 position;
        public AnimationState animState;

        public PlayerUpdate(Vector3 position, AnimationState animState, ushort id)
        {
            this.entityId = id;
            this.position = position;
            this.animState = animState;
        }
    }
}
