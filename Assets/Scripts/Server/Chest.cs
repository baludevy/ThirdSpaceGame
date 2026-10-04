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

            ItemDefinition definition = ItemCatalog.Instance.GetItem(itemType);

            if (definition == null || itemAmount <= 0)
                return;

            Item item = CreateItem(definition, itemAmount);
            Vector3 pos = (Vector3)position - Vector3.up * 0.75f;

            if (WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, pos, false) is not DroppedItem droppedItem)
                return;

            droppedItem.item = item;
            opened = true;

            ServerSend.ChestOpened(id, opened);
            ServerSend.SpawnEntity(droppedItem);
        }

        public static Item CreateItem(ItemDefinition item, int count)
        {
            return item.Type switch
            {
                ItemType.PotatoSeed => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Potato },
                ItemType.Carrot => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Carrot },
                
                
                ItemType.Hoe => new Hoe(item.Type, count, item.Stackable, item.MaxStackSize),
                _ => new Item(item.Type, count, item.Stackable, item.MaxStackSize)
            };
        }

        public bool CanInteract() => !opened;
    }
}
