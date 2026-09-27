using System;
using Game;
using UnityEngine;

namespace Server
{
    public class Player : Entity
    {
        public int id;
        public string username;

        [NonSerialized]
        public InputManager inputManager;

        public Game.AnimationState animState;

        public void Initialize(int id, string username)
        {
            this.id = id;
            this.username = username;

            inputManager = new InputManager(this);
        }
        
        public Interactable interactableInRange;
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.name);
            if (other.TryGetComponent(out Interactable interactable) && interactable.CanInteract())
            {
                interactableInRange = interactable;
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out Interactable interactable) && interactable == interactableInRange)
            {
                interactableInRange = null;
            }
        }

        public void Interact()
        {
            Debug.Log("Interact");
            if (interactableInRange == null)
                return;

            interactableInRange.Interact(this);
        }
    }
}
