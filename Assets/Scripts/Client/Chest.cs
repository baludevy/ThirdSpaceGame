using Client;
using UnityEngine;
using Object = Client.Object;

namespace Game
{
    public class Chest : Object, Interactable
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
            if (opened)
                renderer.sprite = chestOpenedSprite;
            else
                renderer.sprite = chestClosedSprite;
        }

        public void Interact()
        {
            Debug.Log("Interacting with chest");
        }

        public ushort GetId() => id;
        
        public InteractionKind GetInteractionKind() => InteractionKind.Object;

        public bool CanInteract() => !opened;
    }
}
