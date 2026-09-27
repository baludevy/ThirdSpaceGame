using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

namespace Server
{
    public class Inventory
    {
        private readonly Dictionary<ItemType,int> itemCounts = new();

        public void Add(ItemType itemType, int amount = 1)
        {
            if (amount <= 0) return;

            itemCounts.TryGetValue(itemType, out int currentCount);
            itemCounts[itemType] = currentCount + amount;
        }

        public int GetCount(ItemType itemType)
        {
            return itemCounts.GetValueOrDefault(itemType, 0);
        }

        public bool Remove(ItemType itemType, int amount = 1)
        {
            if (amount <= 0 || GetCount(itemType) < amount) return false;

            int remaining = GetCount(itemType) - amount;
            if (remaining == 0)
                itemCounts.Remove(itemType);
            else
                itemCounts[itemType] = remaining;

            return true;
        }
    }
}

