using System.Collections.Generic;
using UnityEngine;
using AnimationState = Game.AnimationState;

namespace Server
{
    public class WorldUpdate
    {
        public List<PlayerUpdate> playerUpdates = new List<PlayerUpdate>();
    }

    public struct PlayerUpdate
    {
        public int id;
        public Vector3 position;
        public AnimationState animState;

        public PlayerUpdate(Vector3 position, AnimationState animState, int id)
        {
            this.id = id;
            this.position = position;
            this.animState = animState;
        }
    }
}
