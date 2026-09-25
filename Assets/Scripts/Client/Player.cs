using UnityEngine;
using AnimationState = Game.AnimationState;

namespace Client
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string username;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Animator animator;

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;
        }

        public void UpdateAnimationState(AnimationState animState)
        {
            animator.SetBool("Forward", false);
            animator.SetBool("Side", false);
            animator.SetBool("Back", false);
            
            switch (animState)
            {
                case AnimationState.forward:
                    animator.SetBool("Forward", true);
                    break;
                case AnimationState.left:
                    sprite.flipX = false;
                    animator.SetBool("Side", true);
                    break;
                case AnimationState.right:
                    sprite.flipX = true;
                    animator.SetBool("Side", true);
                    break;
                case AnimationState.back:
                    animator.SetBool("Back", true);
                    break;
                case AnimationState.idle:
                    break;
            }
        }
    }
}
