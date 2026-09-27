using Client;
using UnityEngine;

namespace Game
{
    public class PlayerInteraction : MonoBehaviour
    {
        public Interactable interactableInRange;
        
        public void OnTriggerEnter2D(Collider2D other)
        {
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
        
        public void OnInteract()
        {
            if(interactableInRange != null)
            {
                ClientSend.Interact(interactableInRange.GetId());
            }
        }
    }
}
