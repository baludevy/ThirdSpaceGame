using Server.Tools;
using Types;
using UnityEngine;

namespace Server
{
    public class Chest : Object, Interactable
    {
        public bool opened;
        public ItemType itemType;
        public int itemAmount = 1;

        public void Interact(Player player)
        {
            if (opened)
                return;

            Vector3 pos = position + Vector3.back * 1.5f;

            ItemHelper.CreateDroppedItem(itemType, itemAmount, pos);
            
            opened = true;

            ServerSend.ChestOpened(id, opened);
        }

        public bool CanInteract() => !opened;
    }
}
