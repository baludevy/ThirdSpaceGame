using UnityEngine;

namespace Server
{
    public class Chest : Object, Interactable
    {
        public bool opened;

        public void Interact(Player player)
        {
            if (opened)
                return;

            opened = true;
            ServerSend.ChestOpened(id, opened);

            Vector3 pos = (Vector3)position - Vector3.up * 0.75f;

            WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, pos);
        }

        public bool CanInteract() => !opened;
    }
}
