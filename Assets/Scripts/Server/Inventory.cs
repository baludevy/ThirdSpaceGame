using System;
using System.Collections.Generic;

namespace Server
{
    public class Slot
    {
        public int slotIndex;
        public ItemType itemType;
        public int count;

        public bool isEmpty => count == 0;

        public Slot(int slotIndex)
        {
            this.slotIndex = slotIndex;
        }
    }

    public class Inventory
    {
        public List<Slot> slots = new List<Slot>();
        private readonly int maxStackSize;

        public Inventory(int slotCount, int maxStackSize = 64)
        {
            if (slotCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount));

            if (maxStackSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStackSize));

            this.maxStackSize = maxStackSize;

            for (int i = 0; i < slotCount; i++)
                slots.Add(new Slot(i));
        }
        
        private IEnumerable<Slot> HotbarFirst()
        {
            int hotbarStart = Math.Max(0, slots.Count - 9);

            for (int i = hotbarStart; i < slots.Count; i++)
                yield return slots[i];

            for (int i = 0; i < hotbarStart; i++)
                yield return slots[i];
        }

        public bool Add(ItemType itemType, int amount = 1)
        {
            if (amount <= 0)
                return false;
            
            long availableSpace = 0;

            foreach (Slot slot in slots)
            {
                if (slot.isEmpty)
                    availableSpace += maxStackSize;
                else if (slot.itemType.Equals(itemType))
                    availableSpace += maxStackSize - slot.count;
            }

            if (availableSpace < amount)
                return false;
            
            foreach (Slot slot in HotbarFirst())
            {
                if (slot.isEmpty || !slot.itemType.Equals(itemType))
                    continue;

                int amountToAdd = Math.Min(amount, maxStackSize - slot.count);
                slot.count += amountToAdd;
                amount -= amountToAdd;

                if (amount == 0)
                    return true;
            }
            
            foreach (Slot slot in HotbarFirst())
            {
                if (!slot.isEmpty)
                    continue;

                int amountToAdd = Math.Min(amount, maxStackSize);
                slot.itemType = itemType;
                slot.count = amountToAdd;
                amount -= amountToAdd;

                if (amount == 0)
                    return true;
            }

            return false;
        }

        public int GetCount(ItemType itemType)
        {
            int total = 0;

            foreach (Slot slot in slots)
            {
                if (!slot.isEmpty && slot.itemType.Equals(itemType))
                    total += slot.count;
            }

            return total;
        }

        public bool Remove(ItemType itemType, int amount = 1)
        {
            if (amount <= 0 || GetCount(itemType) < amount)
                return false;

            foreach (Slot slot in slots)
            {
                if (slot.isEmpty || !slot.itemType.Equals(itemType))
                    continue;

                int amountToRemove = Math.Min(amount, slot.count);
                slot.count -= amountToRemove;
                amount -= amountToRemove;

                if (amount == 0)
                    return true;
            }

            return false;
        }
    }
}