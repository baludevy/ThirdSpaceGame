using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    void Start()
    {
        gameObject.AddComponent<CanvasGroup>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null)
        {
            GameObject item = eventData.pointerEnter;
            if (item != null && eventData.pointerEnter != null)
            {
                dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            Item draggedItem = GetComponent<Item>();
            if (dropSlot.currentItem != null)
            {
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();
                if (draggedItem != null && targetItem != null && draggedItem.ID == targetItem.ID)
                {
                    if (
                        draggedItem != null
                        && targetItem != null
                        && draggedItem.ID == targetItem.ID
                    )
                    {
                        targetItem.Quantity += draggedItem.Quantity;
                        targetItem.UpdateQuantityDisplay();
                    }
                    if (originalSlot != null)
                    {
                        originalSlot.currentItem = null;
                    }

                    Destroy(gameObject);
                    return;
                }
                else
                {
                    dropSlot.currentItem.transform.SetParent(originalParent);
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition =
                        Vector2.zero;

                    if (originalSlot != null)
                    {
                        originalSlot.currentItem = dropSlot.currentItem;
                    }

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                }
            }
            else
            {
                if (originalSlot != null)
                {
                    originalSlot.currentItem = null;
                }
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
            }
        }
        else
        {
            transform.SetParent(originalParent);
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}
