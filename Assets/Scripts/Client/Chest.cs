using UnityEngine;

namespace Game
{
    public class Chest : Object
    {
        public bool opened;

        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private Sprite chestClosedSprite;
        [SerializeField] private Sprite chestOpenedSprite;

        public void SetOpened(bool open)
        {
            opened = open;

            UpdateSprite();
        }
        
        private void UpdateSprite()
        {
            if(opened)
                renderer.sprite = chestOpenedSprite;
            else
                renderer.sprite = chestClosedSprite;
        }
        public bool CanInteract() => !opened;
    }
}