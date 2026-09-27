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
            this.slotIndex= slotIndex;
        }
    }

    public class Inventory
    {
        public List<Slot> slots = new List<Slot>();
        private int maxStackSize;

        public Inventory(int slotCount, int maxStackSize = 64)
        {
            if (slotCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount));

            if (maxStackSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStackSize));

            this.maxStackSize = maxStackSize;

            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(new Slot(i));
            }
        }
        public bool Add(ItemType itemType, int amount = 1)
        {
            if(amount <= 0)
                return false;

                int availabeSpace = 0;

                foreach (Slot slot in slots)
            {
                if (slot.isEmpty)
                {
                    availabeSpace += maxStackSize;
                }
                else if (slot.itemType.Equals(itemType))
                {
                    availabeSpace += maxStackSize - slot.count;
                }
            }
            
            if (availabeSpace < amount)
                return false;

            foreach (Slot slot in slots)
            {
                if (slot.isEmpty || !slot.itemType.Equals(itemType))
                    continue;

                int spaceInSlot = maxStackSize - slot.count;
                int amountToAdd = Math.Min(amount, spaceInSlot);

                slot.count += amountToAdd;
                amount -= amountToAdd;

                if (amount == 0)
                    return true;
            }

            foreach (Slot slot in slots)
            {
                if(!slot.isEmpty)
                    continue;

                int amountToAdd = Math.Min (amount, maxStackSize);

                slot.itemType = itemType;
                slot.count = amountToAdd;
                amount -= amountToAdd;

                if(amount == 0)
                    return true;
            }

            return true;
        }
        

        public int GetCount(ItemType itemType)
        {
            int total = 0;

            foreach (Slot slot in slots)
            {
                if (!slot.isEmpty && slot.itemType.Equals(itemType))
                {
                    total += slot.count;
                }
            }
            return total;
        }
        public bool Remove(ItemType itemType, int amount = 1)
        {
            if (amount <= 0)
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
            return true;
        }
    }
}