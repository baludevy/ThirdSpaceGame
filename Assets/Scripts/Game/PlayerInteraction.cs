using Client;
using UnityEngine;

namespace Game
{
    public class PlayerInteraction : MonoBehaviour
    {
    
    public void OnInteract()
        {
            ClientSend.Interact();
        }
    }
}
