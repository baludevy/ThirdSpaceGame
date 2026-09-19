using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private PointerEventData.InputButton dragButton;

    private Item itemScript;
    private int originalQuantity;
    private int splitAmount;
    private bool isSplitDrag = false;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        itemScript = GetComponent<Item>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left && 
            eventData.button != PointerEventData.InputButton.Right)
        {
            isDragging = false;
            eventData.pointerDrag = null;
            return;
        }

        dragButton = eventData.button;
        isDragging = true;
        originalParent = transform.parent;

        if (itemScript == null) itemScript = GetComponent<Item>();

        if (dragButton == PointerEventData.InputButton.Right && itemScript != null && itemScript.Quantity > 1)
        {
            isSplitDrag = true;
            originalQuantity = itemScript.Quantity;
            
            splitAmount = itemScript.Quantity / 2;
            int remainingAmount = itemScript.Quantity - splitAmount;

            InventoryController controller = InventoryController.Instance ?? GetComponentInParent<InventoryController>();
            Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;

            if (controller != null && originalSlot != null)
            {
                string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
                controller.CreateItemInSlotByName(cleanName, originalSlot, remainingAmount);
            }

            itemScript.Quantity = splitAmount;
            itemScript.UpdateQuantityDisplay();
        }
        else
        {
            isSplitDrag = false;
            Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;
            if (originalSlot != null)
            {
                originalSlot.currentItem = null;
            }
        }

        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;
        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponent<Slot>()
                       ?? eventData.pointerEnter.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;

        if (dropSlot != null && dropSlot != originalSlot)
        {
            if (dropSlot.currentItem != null)
            {
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();
                bool isSameItem = (itemScript != null && targetItem != null) &&
                                  (itemScript.ID == targetItem.ID ||
                                   itemScript.name.Replace("(Clone)", "").Trim() == targetItem.name.Replace("(Clone)", "").Trim());

                if (isSameItem)
                {
                    targetItem.Quantity += itemScript.Quantity;
                    targetItem.UpdateQuantityDisplay();
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    if (isSplitDrag)
                    {
                        CancelSplitDrag(originalSlot);
                        return;
                    }
                    else
                    {
                        dropSlot.currentItem.transform.SetParent(originalParent);
                        dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                        if (originalSlot != null) originalSlot.currentItem = dropSlot.currentItem;

                        transform.SetParent(dropSlot.transform);
                        dropSlot.currentItem = gameObject;
                    }
                }
            }
            else
            {
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
            }
        }
        else
        {
            if (isSplitDrag)
            {
                CancelSplitDrag(originalSlot);
            }
            else
            {
                transform.SetParent(originalParent);
                if (originalSlot != null)
                {
                    originalSlot.currentItem = gameObject;
                }
            }
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    private void CancelSplitDrag(Slot originalSlot)
    {
        if (originalSlot != null && originalSlot.currentItem != null)
        {
            Item originalItemScript = originalSlot.currentItem.GetComponent<Item>();
            if (originalItemScript != null && itemScript != null)
            {
                originalItemScript.Quantity += itemScript.Quantity;
                originalItemScript.UpdateQuantityDisplay();
            }
        }
        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && !eventData.dragging)
        {
            Slot currentSlot = GetComponentInParent<Slot>();
            if (currentSlot != null)
            {
                InventoryController controller = InventoryController.Instance ?? currentSlot.GetComponentInParent<InventoryController>();
                if (controller != null)
                {
                    controller.SplitItemInSlot(currentSlot);
                }
            }
        }
    }
}