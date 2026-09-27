using TMPro;
using UnityEngine;
namespace Client
{
    public class Player : Entity
    {
        public int id;
        public string username;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Animator animator;
        [SerializeField] private TMP_Text usernameText;
        
        static readonly int Forward = Animator.StringToHash("Forward");
        static readonly int Side = Animator.StringToHash("Side");
        static readonly int Back = Animator.StringToHash("Back");

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;

            if (usernameText)
                usernameText.text = username;
        }

        public void UpdateAnimationState(AnimationState animState)
        {
            if (animator == null) return;

            animator.SetBool(Forward, false);
            animator.SetBool(Side, false);
            animator.SetBool(Back, false);

            if (animState == AnimationState.forward)
            {
                animator.SetBool(Forward, true);
            }
            else if (animState == AnimationState.back)
            {
                animator.SetBool(Back, true);
            }
            else if (animState == AnimationState.left)
            {
                if (sprite != null)
                    sprite.flipX = false;

                if (animator != null)
                    animator.SetBool(Side, true);
            }
            else if (animState == AnimationState.right)
            {
                if (sprite != null)
                    sprite.flipX = true;

                if (animator != null)
                    animator.SetBool(Side, true);
            }
        }

        public override void ApplySnapshot(EntitySnapshot previous, EntitySnapshot current, float t)
        {
            if (current is PlayerSnapshot to)
                UpdateAnimationState(to.animationState);
        }
    }
}
