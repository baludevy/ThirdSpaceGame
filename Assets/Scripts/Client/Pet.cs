using UnityEngine;

namespace Client
{
    public class Pet : Entity
    {
        public override void ApplySnapshot(EntitySnapshot previous, EntitySnapshot current, float t)
        {
            if (current is PetSnapshot to)
                Debug.Log($"pet animation: {to.animationState}");
        }
    }
}
