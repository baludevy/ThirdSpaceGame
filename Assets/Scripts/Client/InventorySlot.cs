using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class InventorySlot : MonoBehaviour
    {
        public int slotIndex;
        public ItemType itemType;
        public int itemCount;

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
            this.itemType = itemType;
            this.itemCount = itemCount;

            if (itemCount > 0)
                itemImage.sprite = Inventory.Instance.itemSprites[itemType];
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
            itemCountText.text = visible ? displayedCount.ToString() : "";
        }
    }
}