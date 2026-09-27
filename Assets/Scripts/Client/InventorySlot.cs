using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Client
{
    public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public int slotIndex;

        public ItemType itemType;
        private int itemCount;

        [SerializeField]
        private TMP_Text itemCountText;
        [SerializeField]
        private Image itemImage;

        [SerializeField]
        private Image slotImage;
        [SerializeField]
        private Sprite activeSprite;
        [SerializeField]
        private Sprite inactiveSprite;

        public void SetActive(bool active)
        {
            slotImage.sprite = active ? activeSprite : inactiveSprite;
        }

        public void UpdateSlot(ItemType itemType, int itemCount)
        {
            if (itemCount > 0)
            {
                this.itemType = itemType;
                this.itemCount = itemCount;

                itemImage.gameObject.SetActive(true);

                itemCountText.text = itemCount.ToString();
                itemImage.sprite = Inventory.Instance.itemSprites[itemType];
            }
            else
            {
                itemImage.gameObject.SetActive(false);

                itemImage.sprite = null;
                itemCountText.text = "";
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemCount <= 0)
                return;

            Inventory.Instance.BeginDrag(this, itemImage.sprite, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Inventory.Instance.MoveDragPreview(eventData.position);
        }

        public void OnDrop(PointerEventData eventData)
        {
            Inventory.Instance.DropOn(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Inventory.Instance.EndDrag();
        }
    }
}
