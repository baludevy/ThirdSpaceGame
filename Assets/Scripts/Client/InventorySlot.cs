using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client
{
    public class InventorySlot : MonoBehaviour
    {
        public int slotIndex;

        private ItemType itemType;
        private int itemCount;

        [SerializeField]
        private TMP_Text itemCountText;
        [SerializeField]
        private Image image;

        public void UpdateSlot(ItemType itemType, int itemCount)
        {
            if (itemCount > 0)
            {
                this.itemType = itemType;
                this.itemCount = itemCount;

                itemCountText.text = itemCount.ToString();
                image.sprite = Inventory.Instance.itemSprites[itemType];
            }
            else
            {
                image.sprite = null;
                itemCountText.text = "";
            }
        }
    }
}
