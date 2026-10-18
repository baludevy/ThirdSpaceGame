using System.Collections.Generic;
using Client;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerInteraction : MonoBehaviour
    {
        public Interactable interactableInRange;

        private readonly HashSet<Interactable> interacted = new();

        private void FixedUpdate()
        {
            if (interactableInRange != null &&
                !interactableInRange.CanInteract())
            {
                interactableInRange = null;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (interactableInRange != null)
                return;

            if (other.TryGetComponent(out Interactable interactable) &&
                !interacted.Contains(interactable) &&
                interactable.CanInteract())
            {
                interactableInRange = interactable;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out Interactable interactable))
                return;

            interacted.Remove(interactable);

            if (interactable == interactableInRange)
                interactableInRange = null;
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed ||
                interactableInRange == null ||
                !interactableInRange.CanInteract())
                return;

            Interactable target = interactableInRange;

            ClientSend.Interact(
                target.GetInteractionKind(),
                target.GetId()
            );

            interacted.Add(target);
            interactableInRange = null;
        }

        private void OnDisable()
        {
            interacted.Clear();
            interactableInRange = null;
        }
    }
}
