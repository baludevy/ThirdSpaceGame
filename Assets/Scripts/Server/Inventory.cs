using System;
using System.Collections.Generic;
using UnityEngine;

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
        public int containerId;
        
        public List<Slot> slots = new List<Slot>();
        private readonly int maxStackSize;

        public Inventory(int containerId,int slotCount, int maxStackSize = 64)
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
            return TransferTo(this, fromIndex, toIndex);
        }

        public bool Split(int fromIndex, int toIndex)
        {
            return TransferTo(this, fromIndex, toIndex, true);
        }

        public bool TransferTo(Inventory destination, int fromIndex, int toIndex, bool split = false)
        {
            if (destination == null ||
                !IsValidIndex(fromIndex) ||
                !destination.IsValidIndex(toIndex))
            {
                return false;
            }

            if (ReferenceEquals(this, destination) && fromIndex == toIndex)
                return false;

            Slot from = slots[fromIndex];
            Slot to = destination.slots[toIndex];

            if (from.count <= 0 || (split && from.count < 2))
                return false;
            
            int requested = split
                ? from.count / 2 + from.count % 2
                : from.count;
            
            if (to.isEmpty || to.itemType.Equals(from.itemType))
            {
                int space = destination.maxStackSize - to.count;
                int moved = Math.Min(requested, space);

                if (moved <= 0)
                    return false;

                to.itemType = from.itemType;
                to.count += moved;
                from.count -= moved;

                if (from.isEmpty)
                    from.itemType = default;

                return true;
            }
            
            if (split)
                return false;
            
            if (from.count > destination.maxStackSize ||
                to.count > maxStackSize)
            {
                return false;
            }

            ItemType previousType = to.itemType;
            int previousCount = to.count;

            to.itemType = from.itemType;
            to.count = from.count;

            from.itemType = previousType;
            from.count = previousCount;

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
    }
}