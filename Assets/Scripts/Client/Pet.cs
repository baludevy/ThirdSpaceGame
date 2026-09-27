using UnityEngine;

namespace Client
{
    public class Pet : Entity
    {
        static readonly int Walking = Animator.StringToHash("Walking");
        static readonly int Sleeping = Animator.StringToHash("Sleeping");

        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private Animator animator;

        public void UpdateAnimationState(bool facingRight, PetAnimationState animState)
        {
            if (renderer != null)
            {
                renderer.flipX = facingRight;
            }
            
            if (animator == null) return;

            animator.SetBool(Walking, false);
            animator.SetBool(Sleeping, false);

            if (animState == PetAnimationState.Walk)
            {
                animator.SetBool(Walking, true);
            }
            else if (animState == PetAnimationState.Sleep)
            {
                animator.SetBool(Sleeping, true);
            }
        }

        public override void ApplySnapshot(EntitySnapshot previous, EntitySnapshot current, float t)
        {
            if (current is PetSnapshot to)
                UpdateAnimationState(to.facingRight, to.animationState);
        }
    }
}
