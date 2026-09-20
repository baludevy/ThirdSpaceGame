using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    [SerializeField] private RectTransform inventoryPanel;

    private Transform originalParent;
    private Slot originalSlot;
    private Transform dragParent;

    private CanvasGroup canvasGroup;
    private Item itemScript;
    
    private bool isDragging;
    private bool isSplitDrag;

    private PointerEventData.InputButton dragButton;

    private int originalQuantity;
    private int splitAmount;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if(canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        itemScript = GetComponent<Item>();

        Canvas canvas = GetComponentInParent<Canvas>();

        if(canvas != null)
            dragParent = canvas.rootCanvas.transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if(itemScript == null)
            itemScript = GetComponent<Item>();

        if(itemScript == null)
            return;

        originalParent = transform.parent;
        originalSlot = originalParent != null
            ? originalParent.GetComponent<Slot>()
            : null;

        dragButton = eventData.button;
        isDragging = true;
        isSplitDrag = false;

        if(dragButton == PointerEventData.InputButton.Right && itemScript.Quantity > 1)
        {
            if(originalSlot == null)
            {
                CancelDragSetup();
                return;
            }

            InventoryController controller = InventoryController.Instance ?? originalSlot.GetComponentInParent<InventoryController>();

            if(controller == null)
            {
                CancelDragSetup();
                return;
            }
            isSplitDrag = true;
            originalQuantity = itemScript.Quantity;
            splitAmount = originalQuantity / 2;

            int remainingAmount = originalQuantity - splitAmount;

            originalSlot.currentItem = null;

            MoveToDragLayer();

            itemScript.Quantity = splitAmount;
            itemScript.UpdateQuantityDisplay();

            string cleanName = GetCleanItemName(gameObject.name);

            controller.CreateItemInSlotByName(
                cleanName,
                originalSlot,
                remainingAmount
            );

            if(originalSlot.currentItem == null || originalSlot.currentItem == gameObject)
            {
                itemScript.Quantity = originalQuantity;
                itemScript.UpdateQuantityDisplay();

                transform.SetParent(originalParent);

                RectTransform rect = GetComponent<RectTransform>();

                if (rect != null)
                    rect.anchoredPosition = Vector2.zero;

                originalSlot.currentItem = gameObject;

                CancelDragSetup();
                return;
            }
        }
        else
        {
            isSplitDrag = false;

            if (originalSlot != null)
                originalSlot.currentItem = null;

            MoveToDragLayer();
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }
    public void OnDrag(PointerEventData eventData)
{
    if (!isDragging)
        return;

    transform.position = eventData.position;
}

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        isDragging = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (IsOutSideInventory(eventData))
        {
            DropItem();

            if(isSplitDrag)
                CancelSplitDrag();
            else
                ReturnToOriginalSlot();
            
            return;
        }

        Slot dropSlot = GetSlotUnderPointer(eventData);

        if(dropSlot == null || dropSlot == originalSlot)
        {
            if(isSplitDrag)
                CancelSplitDrag();
            else
                ReturnToOriginalSlot();

            return;
        }

        if(dropSlot.currentItem == null)
        {
            PlaceInSlot(dropSlot);
            return;
        }

        Item targetItem = dropSlot.currentItem.GetComponent<Item>();

        if(IsSameItem(itemScript, targetItem))
        {
            targetItem.Quantity += itemScript.Quantity;
            targetItem.UpdateQuantityDisplay();

            Destroy(gameObject);
            return;
        }

        if (isSplitDrag)
        {
            CancelSplitDrag();
            return;
        }

        SwapItems(dropSlot);
    }

    private bool IsOutSideInventory(PointerEventData eventData)
    {
        if(inventoryPanel == null)
            return false;

        return !RectTransformUtility.RectangleContainsScreenPoint(
            inventoryPanel,
            eventData.position,
            eventData.pressEventCamera
        );
    }

    private void DropItem()
    {    
    }

    private void SwapItems(Slot dropSlot)
    {
        if(dropSlot == null || dropSlot.currentItem == null)
        {
            ReturnToOriginalSlot();
            return;
        }
    
        GameObject targetObject = dropSlot.currentItem;

        targetObject.transform.SetParent(originalParent);

        RectTransform targetRect = targetObject.GetComponent<RectTransform>();

        if(targetRect != null)
            targetRect.anchoredPosition = Vector2.zero;

        if(originalSlot != null)
            originalSlot.currentItem = targetObject;

        transform.SetParent(dropSlot.transform);

        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        dropSlot.currentItem = gameObject;
    }

    private void PlaceInSlot(Slot slot)
    {
        transform.SetParent(slot.transform);

        RectTransform rect = GetComponent<RectTransform>();

        if(rect != null)
            rect.anchoredPosition = Vector2.zero;

        slot.currentItem = gameObject;
    }

    private void ReturnToOriginalSlot()
    {
        if(originalParent != null)
            transform.SetParent(originalParent);
        
        RectTransform rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        if (originalSlot != null)
            originalSlot.currentItem = gameObject;
    }

    private void CancelSplitDrag()
    {
        if(originalSlot != null && originalSlot.currentItem != null)
        {
            Item remainingItem = originalSlot.currentItem.GetComponent<Item>();

            if(remainingItem != null)
            {
                remainingItem.Quantity += itemScript.Quantity;
                remainingItem.UpdateQuantityDisplay();

                Destroy(gameObject);
                return;
            }
        }
        
        itemScript.Quantity = originalQuantity;
        itemScript.UpdateQuantityDisplay();

        ReturnToOriginalSlot();

        isSplitDrag = false;
    }

    private void MoveToDragLayer()
    {
        if(dragParent != null)
            transform.SetParent(dragParent);
        else
            transform.SetParent(transform.root);

        transform.SetAsLastSibling();
    }

    private Slot GetSlotUnderPointer(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null)
            return null;

        Slot slot = eventData.pointerEnter.GetComponent<Slot>();

        if(slot == null)
            slot = eventData.pointerEnter.GetComponentInParent<Slot>();

        return slot;
    }

    private bool IsSameItem(Item a, Item b)
    {
        if(a == null || b == null)
            return false;

        if(a.ID == b.ID)
            return true;

        string aName = GetCleanItemName(a.gameObject.name);
        string bName = GetCleanItemName(b.gameObject.name);

        return aName == bName;
    }

    private string GetCleanItemName(string itemName)
    {
        return itemName.Replace("(Clone)", "").Trim();
    }

    private void CancelDragSetup()
    {
        isDragging = false;
        isSplitDrag = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Right)
            return;

        if(eventData.dragging)
            return;

        Slot currentSlot = GetComponentInParent<Slot>();
        
        if(currentSlot == null)
            return;

        InventoryController controller = InventoryController.Instance ?? currentSlot.GetComponentInParent<InventoryController>();

        if(controller != null)
            controller.SplitItemInSlot(currentSlot);
    }
}
