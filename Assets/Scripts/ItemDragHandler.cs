using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    public RectTransform inventoryPanel;

    private CanvasGroup canvasGroup;

    private PointerEventData.InputButton dragButton;
    private Transform dragParent;

    private bool isDragging;
    private bool isSplitDrag;
    private Item itemScript;

    private Transform originalParent;

    private int originalQuantity;
    private Slot originalSlot;
    private int splitAmount;

    private void Awake()
    {
        inventoryPanel = GameObject.Find("Inventory").GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        itemScript = GetComponent<Item>();

        var canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            dragParent = canvas.rootCanvas.transform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left &&
            eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (itemScript == null)
            itemScript = GetComponent<Item>();

        if (itemScript == null)
            return;

        originalParent = transform.parent;
        originalSlot = originalParent != null
            ? originalParent.GetComponent<Slot>()
            : null;

        dragButton = eventData.button;
        isDragging = true;
        isSplitDrag = false;

        if (dragButton == PointerEventData.InputButton.Right && itemScript.Quantity > 1)
        {
            if (originalSlot == null)
            {
                CancelDragSetup();
                return;
            }

            var controller = InventoryController.Instance ??
                             originalSlot.GetComponentInParent<InventoryController>();

            if (controller == null)
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

            if (originalSlot.currentItem == null || originalSlot.currentItem == gameObject)
            {
                itemScript.Quantity = originalQuantity;
                itemScript.UpdateQuantityDisplay();

                transform.SetParent(originalParent);

                var rect = GetComponent<RectTransform>();

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

            if (isSplitDrag)
                CancelSplitDrag();
            else
                ReturnToOriginalSlot();

            return;
        }

        var dropSlot = GetSlotUnderPointer(eventData);

        if (dropSlot == null || dropSlot == originalSlot)
        {
            if (isSplitDrag)
                CancelSplitDrag();
            else
                ReturnToOriginalSlot();

            return;
        }

        if (dropSlot.currentItem == null)
        {
            PlaceInSlot(dropSlot);
            return;
        }

        var targetItem = dropSlot.currentItem.GetComponent<Item>();

        if (IsSameItem(itemScript, targetItem))
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        if (eventData.dragging)
            return;

        var currentSlot = GetComponentInParent<Slot>();

        if (currentSlot == null)
            return;

        var controller =
            InventoryController.Instance ?? currentSlot.GetComponentInParent<InventoryController>();

        if (controller != null)
            controller.SplitItemInSlot(currentSlot);
    }

    private bool IsOutSideInventory(PointerEventData eventData)
    {
        if (inventoryPanel == null)
        {
            return false;
        }

        var canvas = inventoryPanel.GetComponentInParent<Canvas>();

        Camera uiCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        bool inside = RectTransformUtility.RectangleContainsScreenPoint(
            inventoryPanel,
            eventData.position,
            uiCamera
        );


        return !inside;
    }

    private void DropItem()
    {
        var player = ClientGameManager.Instance
            .players[ClientGameManager.Instance.myId]
            .gameObject
            .GetComponent<ClientPlayer>();

        player.DropItem(itemScript.itemType);

        if (originalSlot != null)
            originalSlot.currentItem = null;

        Destroy(gameObject);
    }

    private void SwapItems(Slot dropSlot)
    {
        if (dropSlot == null || dropSlot.currentItem == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        var targetObject = dropSlot.currentItem;

        targetObject.transform.SetParent(originalParent);

        var targetRect = targetObject.GetComponent<RectTransform>();

        if (targetRect != null)
            targetRect.anchoredPosition = Vector2.zero;

        if (originalSlot != null)
            originalSlot.currentItem = targetObject;

        transform.SetParent(dropSlot.transform);

        var rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        dropSlot.currentItem = gameObject;
    }

    private void PlaceInSlot(Slot slot)
    {
        transform.SetParent(slot.transform);

        var rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        slot.currentItem = gameObject;
    }

    private void ReturnToOriginalSlot()
    {
        if (originalParent != null)
            transform.SetParent(originalParent);

        var rect = GetComponent<RectTransform>();

        if (rect != null)
            rect.anchoredPosition = Vector2.zero;

        if (originalSlot != null)
            originalSlot.currentItem = gameObject;
    }

    private void CancelSplitDrag()
    {
        if (originalSlot != null && originalSlot.currentItem != null)
        {
            var remainingItem = originalSlot.currentItem.GetComponent<Item>();

            if (remainingItem != null)
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
        if (dragParent != null)
            transform.SetParent(dragParent);
        else
            transform.SetParent(transform.root);

        transform.SetAsLastSibling();
    }

    private Slot GetSlotUnderPointer(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null)
            return null;

        var slot = eventData.pointerEnter.GetComponent<Slot>();

        if (slot == null)
            slot = eventData.pointerEnter.GetComponentInParent<Slot>();

        return slot;
    }

    private bool IsSameItem(Item a, Item b)
    {
        if (a == null || b == null)
            return false;

        if (a.ID == b.ID)
            return true;

        string aName = GetCleanItemName(a.gameObject.name);
        string bName = GetCleanItemName(b.gameObject.name);

        return aName == bName;
    }

    private string GetCleanItemName(string itemName) => itemName.Replace("(Clone)", "").Trim();

    private void CancelDragSetup()
    {
        isDragging = false;
        isSplitDrag = false;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}
