using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public class Item
    {
        public bool stackable;
        public int maxStackSize = 64;
        public int count;
        public ItemType type;

        public Item Copy(int amount)
        {
            return new Item
            {
                stackable = stackable,
                maxStackSize = maxStackSize,
                count = amount,
                type = type
            };
        }
    }

    public class Slot
    {
        public int slotIndex;
        public Item item;

        public bool isEmpty => item == null || item.count <= 0;

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

        public Inventory(
            int containerId,
            int slotCount,
            int maxStackSize = 64)
        {
            if (slotCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCount));

            if (maxStackSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxStackSize));

            this.containerId = containerId;
            this.maxStackSize = maxStackSize;

            for (int i = 0; i < slotCount; i++)
                slots.Add(new Slot(i));
        }

        private int GetStackLimit(Item item)
        {
            return item.stackable
                ? Math.Min(maxStackSize, item.maxStackSize)
                : 1;
        }

        private static bool CanStack(Item first, Item second)
        {
            return first != null &&
                   second != null &&
                   first.stackable &&
                   second.stackable &&
                   first.type.Equals(second.type) &&
                   first.maxStackSize == second.maxStackSize;
        }

        private IEnumerable<Slot> HotbarFirst()
        {
            int hotbarStart = Math.Max(0, slots.Count - 9);

            for (int i = hotbarStart; i < slots.Count; i++)
                yield return slots[i];

            for (int i = 0; i < hotbarStart; i++)
                yield return slots[i];
        }

        public bool Add(Item item)
        {
            if (item == null || item.count <= 0)
                return false;

            int stackLimit = GetStackLimit(item);

            if (stackLimit <= 0)
                return false;

            long availableSpace = 0;

            foreach (Slot slot in slots)
            {
                if (slot.isEmpty)
                {
                    availableSpace += stackLimit;
                }
                else if (CanStack(slot.item, item))
                {
                    availableSpace += Math.Max(
                        0,
                        GetStackLimit(slot.item) - slot.item.count);
                }
            }

            if (availableSpace < item.count)
                return false;

            int remaining = item.count;

            foreach (Slot slot in HotbarFirst())
            {
                if (slot.isEmpty || !CanStack(slot.item, item))
                    continue;

                int space = Math.Max(
                    0,
                    GetStackLimit(slot.item) - slot.item.count);

                int amountToAdd = Math.Min(remaining, space);

                slot.item.count += amountToAdd;
                remaining -= amountToAdd;

                if (remaining == 0)
                    return true;
            }

            foreach (Slot slot in HotbarFirst())
            {
                if (!slot.isEmpty)
                    continue;

                int amountToAdd = Math.Min(remaining, stackLimit);

                slot.item = item.Copy(amountToAdd);
                remaining -= amountToAdd;

                if (remaining == 0)
                    return true;
            }

            return false;
        }

        public long GetCount(ItemType itemType)
        {
            long total = 0;

            foreach (Slot slot in slots)
            {
                if (!slot.isEmpty && slot.item.type.Equals(itemType))
                    total += slot.item.count;
            }

            return total;
        }

        public bool Remove(ItemType itemType, int amount = 1)
        {
            if (amount <= 0 || GetCount(itemType) < amount)
                return false;

            foreach (Slot slot in slots)
            {
                if (slot.isEmpty || !slot.item.type.Equals(itemType))
                    continue;

                int amountToRemove = Math.Min(amount, slot.item.count);

                slot.item.count -= amountToRemove;
                amount -= amountToRemove;

                if (slot.isEmpty)
                    slot.item = null;

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
            if (destination == null || !IsValidIndex(fromIndex) || !destination.IsValidIndex(toIndex))
            {
                return false;
            }

            if (ReferenceEquals(this, destination) && fromIndex == toIndex)
                return false;

            Slot from = slots[fromIndex];
            Slot to = destination.slots[toIndex];

            if (from.isEmpty || (split && from.item.count < 2))
                return false;

            int requested = split ? from.item.count / 2 + from.item.count % 2 : from.item.count;

            if (to.isEmpty || CanStack(from.item, to.item))
            {
                int stackLimit = destination.GetStackLimit(to.isEmpty ? from.item : to.item);

                int destinationCount = to.isEmpty ? 0 : to.item.count;
                int space = Math.Max(0, stackLimit - destinationCount);
                int moved = Math.Min(requested, space);

                if (moved <= 0)
                    return false;

                if (to.isEmpty)
                    to.item = from.item.Copy(moved);
                else
                    to.item.count += moved;

                from.item.count -= moved;

                if (from.isEmpty)
                    from.item = null;

                return true;
            }

            if (split)
                return false;

            if (from.item.count > destination.GetStackLimit(from.item) ||
                to.item.count > GetStackLimit(to.item))
            {
                return false;
            }

            (to.item, from.item) = (from.item, to.item);

            return true;
        }

        public bool Drop(int slotIndex, bool split, Vector2 position)
        {
            if (!IsValidIndex(slotIndex))
                return false;

            Slot slot = slots[slotIndex];

            if (slot.isEmpty)
                return false;

            int amountToDrop = split
                ? slot.item.count / 2 + slot.item.count % 2
                : slot.item.count;

            if (WorldManager.Instance.entityManager.SpawnEntity(EntityType.item, position, false) is not DroppedItem droppedItem)
            {
                return false;
            }

            droppedItem.item = slot.item.Copy(amountToDrop);

            ServerSend.SpawnEntity(droppedItem);

            slot.item.count -= amountToDrop;

            if (slot.isEmpty)
                slot.item = null;

            return true;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < slots.Count;
        }
    }
}
