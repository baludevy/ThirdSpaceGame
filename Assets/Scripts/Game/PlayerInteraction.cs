using Client;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if(!context.performed)
                return;
            
            if(interactableInRange != null)
            {
                Debug.Log(interactableInRange.GetId());
                ClientSend.Interact(interactableInRange.GetInteractionKind(), interactableInRange.GetId());
            }
        }
    }
}
