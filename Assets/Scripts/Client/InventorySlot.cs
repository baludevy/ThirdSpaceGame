using System;
using TMPro;
using Types;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class InventorySlot : MonoBehaviour
    {
        [NonSerialized] public int slotIndex;
        [NonSerialized] public Item item;
        [NonSerialized] public int itemCount;

        public Sprite itemSprite => itemImage.sprite;

        [SerializeField] private TMP_Text itemCountText;
        [SerializeField] private Image itemImage;
        [SerializeField] private Image slotImage;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        private bool isHeld;
        private int? previewCount;

        public void SetActive(bool active)
        {
            slotImage.sprite = active ? activeSprite : inactiveSprite;
        }

        public void SetHeld(bool held)
        {
            isHeld = held;
            RefreshItemVisibility();
        }

        public void SetPreviewCount(int? count)
        {
            previewCount = count;
            RefreshItemVisibility();
        }

        public void UpdateSlot(ItemType itemType, int itemCount)
        {
            this.item = CreateItem(itemType);
            this.itemCount = itemCount;

            if (itemCount > 0)
                itemImage.sprite = ItemCatalog.Instance.GetItem(itemType).Sprite;
            else
                itemImage.sprite = null;


            RefreshItemVisibility();
        }

        private void RefreshItemVisibility()
        {
            int displayedCount = itemCount;

            if (previewCount.HasValue)
                displayedCount = previewCount.Value;

            bool visible = displayedCount > 0 && !isHeld;

            itemImage.gameObject.SetActive(visible);
            itemCountText.gameObject.SetActive(visible);
            
            if (item.type != ItemType.empty && ItemCatalog.Instance.GetItem(item.type).Stackable)
            {
                itemCountText.text = visible ? displayedCount.ToString() : "";
            }
            else
            {
                itemCountText.text = "";
            }
        }

        public static Item CreateItem(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.pickaxe or ItemType.shovel or ItemType.hoe => new Tool(itemType),
                _ => new Item(itemType)
            };
        }
    }
}
