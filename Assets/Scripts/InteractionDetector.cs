using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour {
    private Interactable interactableInRange;

    [SerializeField] private GameObject interactionIcon;

    private void Start() {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context) {
        if (context.performed && interactableInRange != null) {
            interactableInRange.Interact();

            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent(out Interactable interactable)
            && interactable.CanInteract()) {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.TryGetComponent(out Interactable interactable)
            && interactable == interactableInRange) {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}