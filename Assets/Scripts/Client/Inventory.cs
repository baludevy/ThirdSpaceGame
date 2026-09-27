using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Client
{
    public class Inventory : MonoBehaviour
    {

        public static Inventory Instance;

        private const int slotCount = 27;
        private const int hotbarSlotCount = 9;
        private const int hotbarStartIndex = slotCount - hotbarSlotCount;

        [SerializeField]
        private RectTransform inventoryPanel;
        [SerializeField]
        private RectTransform hotbarPanel;
        [SerializeField]
        private GameObject slotPrefab;
        [SerializeField]
        public Dictionary<ItemType, Sprite> itemSprites = new();

        private List<InventorySlot> slots = new();

        public int activeHotbarSlotIndex;
        public ItemType activeItemType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < hotbarStartIndex; i++)
            {
                InventorySlot slot =
                    Instantiate(slotPrefab, inventoryPanel).GetComponent<InventorySlot>();

                slot.slotIndex = i;
                slots.Add(slot);
            }

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                InventorySlot slot =
                    Instantiate(slotPrefab, hotbarPanel).GetComponent<InventorySlot>();

                slot.slotIndex = hotbarStartIndex + i;
                slots.Add(slot);
            }

            RefreshHotbar();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || slots.Count != slotCount) return;

            if (keyboard.tabKey.wasPressedThisFrame)
                ToggleInventory();

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                {
                    activeHotbarSlotIndex = i;
                    break;
                }
            }

            RefreshHotbar();
        }

        private void RefreshHotbar()
        {
            if (slots.Count != slotCount) return;

            activeHotbarSlotIndex = Mathf.Clamp(activeHotbarSlotIndex, 0, hotbarSlotCount - 1);

            activeItemType = slots[hotbarStartIndex + activeHotbarSlotIndex].itemType;

            for (int i = 0; i < hotbarSlotCount; i++)
                slots[hotbarStartIndex + i].SetActive(i == activeHotbarSlotIndex);
        }

        public void UpdateSlot(int slotIndex, ItemType itemType, int itemCount)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;

            slots[slotIndex].UpdateSlot(itemType, itemCount);
            RefreshHotbar();
        }

        public void ToggleInventory()
        {
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
