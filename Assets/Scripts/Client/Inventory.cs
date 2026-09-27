using System;
using System.Collections.Generic;
using UnityEngine;

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

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        void Start()
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
        }

        public void ToggleInventory()
        {
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
        }
    }
}
