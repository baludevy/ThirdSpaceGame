using UnityEngine;

public enum ItemType
{
    carrot,
}

namespace Server
{
    public class DroppedItem : Entity, Interactable
    {
        public ItemType itemType;

        public void Interact(Player player)
        {
            Debug.Log("pick up item");
            WorldManager.Instance.entityManager.DestroyEntity(this);
        }
        
        public bool CanInteract() => true;
    }
}