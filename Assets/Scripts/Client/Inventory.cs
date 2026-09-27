using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Client
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance;

        private int slotCount = 27;
        private int hotbarSlotCount = 9;
        private List<InventorySlot> slots = new List<InventorySlot>();

        [SerializeField] private RectTransform inventoryPanel;
        [SerializeField] private RectTransform hotbarPanel;

        [SerializeField] private GameObject slotPrefab;

        [SerializeField]
        public Dictionary<ItemType, Sprite> itemSprites = new Dictionary<ItemType, Sprite>();

        public int activeHotbarSlotIndex;
        public ItemType activeItemType;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        private void Update()
        {
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ToggleInventory();
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame) activeHotbarSlotIndex = 0;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) activeHotbarSlotIndex = 1;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) activeHotbarSlotIndex = 2;
            if (Keyboard.current.digit4Key.wasPressedThisFrame) activeHotbarSlotIndex = 3;
            if (Keyboard.current.digit5Key.wasPressedThisFrame) activeHotbarSlotIndex = 4;
            if (Keyboard.current.digit6Key.wasPressedThisFrame) activeHotbarSlotIndex = 5;
            if (Keyboard.current.digit7Key.wasPressedThisFrame) activeHotbarSlotIndex = 6;
            if (Keyboard.current.digit8Key.wasPressedThisFrame) activeHotbarSlotIndex = 7;
            if (Keyboard.current.digit9Key.wasPressedThisFrame) activeHotbarSlotIndex = 8;
            
            activeItemType = slots[slotCount - hotbarSlotCount + activeHotbarSlotIndex].itemType;
        }

        private void Start()
        {
            for (int i = 0; i < slotCount - hotbarSlotCount; i++)
            {
                InventorySlot inventorySlot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<InventorySlot>();

                inventorySlot.slotIndex = i;

                slots.Add(inventorySlot);
            }

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                InventorySlot hotbarSlot = Instantiate(slotPrefab, hotbarPanel.transform).GetComponent<InventorySlot>();

                hotbarSlot.slotIndex = slotCount - hotbarSlotCount + i;

                slots.Add(hotbarSlot);
            }
        }

        public void UpdateSlot(int slotIndex, ItemType itemType, int itemCount)
        {
            InventorySlot slot = slots[slotIndex];

            slot.UpdateSlot(itemType, itemCount);
            
            activeItemType = slots[slotCount - hotbarSlotCount + activeHotbarSlotIndex].itemType;
        }

        public void ToggleInventory()
        {
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
        }
    }
}
