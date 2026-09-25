using System.Collections.Generic;
using Server;
using UnityEngine;
using AnimationState = Game.AnimationState;

namespace Client
{
    public class Player : Entity
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
                    if(sprite != null)
                        sprite.flipX = false;
                    
                    if(animator != null)
                        animator.SetBool("Side", true);
                    
                    break;
                case AnimationState.right:
                    if(sprite != null)
                        sprite.flipX = true;
                    
                    if(animator != null)
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
