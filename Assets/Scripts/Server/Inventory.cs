using System;
using System.Collections.Generic;
using System.Numerics;

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
        public bool Move(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || toIndex >= slots.Count || toIndex < 0 || toIndex >= slots.Count || fromIndex == toIndex)
                return false;

            Slot from = slots[fromIndex];
            Slot to = slots[toIndex];

            if (from.isEmpty)
                return false;

            if (to.isEmpty)
            {
                to.itemType = from.itemType;
                to.count = from.count;
                from.count = 0;
            }
            else if
            (to.itemType.Equals(from.itemType))
            {
                int space = maxStackSize - to.count;
                if (space <= 0)
                    return false;

                int moved = Math.Min(space, from.count);
                to.count += moved;
                from.count -= moved;
            }
            else
            {
                ItemType previousType = to.itemType;
                int previousCount = to.count;

                to.itemType = from.itemType;
                to.count = from.count;

                from.itemType = from.itemType;
                to.count = from.count;

                from.itemType = previousType;
                from.count = previousCount;
            }
            return true;
        }

        public bool Split(int fromIndex, int toIndex)
        {
            if(!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                return false;
            
            if(fromIndex == toIndex)
                return false;

            Slot from = slots[fromIndex];
            Slot to = slots[toIndex];

            if(from.count < 2)
                return false;

            if(!to.isEmpty && !to.itemType.Equals(from.itemType))
                return false;

            int half = from.count / 2;

            if(from.count % 2 != 0)
                half++;

            int avaibleSpace = maxStackSize - to.count;
            int amountToMove = half;

            if(amountToMove > avaibleSpace)
                amountToMove = avaibleSpace;

            if(amountToMove <= 0)
                return false;

            to.itemType = from.itemType;
            to.count += amountToMove;
            from.count -= amountToMove;

            return true;
        }

        public bool Drop(int slotIndex, bool split, Vector2 position)
        {
            if(!IsValidIndex(slotIndex))
                return false;

            
            Slot slot = slots[slotIndex];

            if(slot.isEmpty)
                return false;

            int amountToDrop = slot.count;

            if (split)
            {
                amountToDrop = slot.count / 2;

                if(slot.count % 2 != 0) 
                    amountToDrop++;
            }

            if(WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, position, false) is DroppedItem droppedItem)
            {
                droppedItem.itemType = slots[slotIndex].itemType;
                droppedItem.itemAmount = amountToDrop;

                ServerSend.SpawnEntity(droppedItem);           
            }

            slot.count -= amountToDrop;

            return true;
        }

        private bool IsValidIndex(int index)
        {
            if(index < 0)
                return false;

            if(index >= slots.Count)
                return false;

            return true;
        }

        internal void Drop(int fromIndex, bool split, UnityEngine.Vector2 vector2)
        {
            throw new NotImplementedException();
        }
    }
}