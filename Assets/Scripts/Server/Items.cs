using System.Collections.Generic;

namespace Server
{
    public static class Items
    {
        public static List<Item> items = new List<Item>
        {
            new Item
            {
                stackable = false,
                type = ItemType.hoe
            },
            new Item
            {
                stackable = true,
                maxStackSize = 64,
                type = ItemType.potato
            },
            new Item
            {
                stackable = true,
                maxStackSize = 64,
                type = ItemType.potatoSeed
            },
            new Item
            {
                stackable = true,
                maxStackSize = 64,
                type = ItemType.carrot
            },
            new Item
            {
                stackable = true,
                maxStackSize = 64,
                type = ItemType.carrotSeed
            },
        };
    }
}
