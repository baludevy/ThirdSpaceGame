using Types;
using UnityEngine;

namespace Server
{
    public static class ItemHelper
    {
        public static DroppedItem CreateDroppedItem(ItemType itemType, int itemAmount, Vector3 position = default)
        {
            ItemDefinition definition = Catalog.Instance.GetItem(itemType);

            if (definition == null || itemAmount <= 0)
                return null;

            Item item = CreateItem(definition, itemAmount);
            
            if (WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, position, false) is not DroppedItem droppedItem)
                return null;

            droppedItem.item = item;
            
            ServerSend.SpawnEntity(droppedItem);

            return droppedItem;
        }
        
        public static Item CreateItem(ItemDefinition item, int count)
        {
            return item.Type switch
            {
                ItemType.PotatoSeed => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Potato },
                ItemType.CarrotSeed => new Seed(item.Type, count, item.Stackable, item.MaxStackSize) { cropType = CropType.Carrot },
                _ => new Item(item.Type, count, item.Stackable, item.MaxStackSize)
            };
        }
    }
}
