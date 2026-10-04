using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Client
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance;

        private const int slotCount = 27;
        private const int hotbarSlotCount = 9;
        private const int hotbarStartIndex = slotCount - hotbarSlotCount;
        private const int maxStackSize = 64;

        [SerializeField] private RectTransform inventoryPanel;
        [SerializeField] private RectTransform hotbarPanel;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private Canvas uiCanvas;

        [SerializeField] private GameObject dragPreviewPrefab;

        private readonly List<InventorySlot> slots = new();
        private readonly List<RaycastResult> pointerHits = new();

        private int activeHotbarSlotIndex;

        private InventorySlot draggedSlot;
        private GameObject dragPreviewObject;
        private bool splitDrag;
        
        public Item currentItem { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < hotbarStartIndex; i++)
            {
                InventorySlot slot = Instantiate(slotPrefab, inventoryPanel).GetComponent<InventorySlot>();
                slot.slotIndex = i;
                slot.UpdateSlot(default, 0);
                slots.Add(slot);
            }

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                InventorySlot slot = Instantiate(slotPrefab, hotbarPanel).GetComponent<InventorySlot>();
                slot.slotIndex = hotbarStartIndex + i;
                slot.UpdateSlot(default, 0);
                slots.Add(slot);
            }

            RefreshHotbar();
        }

        private void Update()
        {
            HandleItemClicks();

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || slots.Count != slotCount)
                return;

            if (keyboard.tabKey.wasPressedThisFrame)
                ToggleInventory();

            for (int i = 0; i < hotbarSlotCount; i++)
            {
                if (keyboard[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                {
                    activeHotbarSlotIndex = i;
                    break;
                }
            }

            RefreshHotbar();
        }

        private void RefreshHotbar()
        {
            if (slots.Count != slotCount)
                return;

            activeHotbarSlotIndex = Mathf.Clamp(activeHotbarSlotIndex, 0, hotbarSlotCount - 1);

            InventorySlot activeSlot = slots[hotbarStartIndex + activeHotbarSlotIndex];
            currentItem = activeSlot.itemCount > 0 ? activeSlot.item : null;

            for (int i = 0; i < hotbarSlotCount; i++)
                slots[hotbarStartIndex + i].SetActive(i == activeHotbarSlotIndex);
        }

        public void UpdateSlot(int slotIndex, ItemType itemType, int itemCount)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return;

            InventorySlot slot = slots[slotIndex];

            // Cancel holding if the server changes the source stack.
            if (slot == draggedSlot && (slot.itemCount != itemCount || !EqualityComparer<ItemType>.Default.Equals(slot.item.type, itemType)))
                ClearHeldItem();

            slot.UpdateSlot(itemType, itemCount);
            RefreshHotbar();
        }

        public void ToggleInventory()
        {
            ClearHeldItem();
            inventoryPanel.gameObject.SetActive(!inventoryPanel.gameObject.activeSelf);
        }

        private void HandleItemClicks()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ClearHeldItem();
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || EventSystem.current == null)
                return;

            Vector2 position = mouse.position.ReadValue();
            MoveDragPreview(position);

            bool leftClick = mouse.leftButton.wasPressedThisFrame;
            bool rightClick = mouse.rightButton.wasPressedThisFrame;

            if (!leftClick && !rightClick)
                return;

            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = position };

            pointerHits.Clear();
            EventSystem.current.RaycastAll(pointer, pointerHits);

            GameObject hit = pointerHits.Count > 0 ? pointerHits[0].gameObject : null;
            InventorySlot clickedSlot = hit != null ? hit.GetComponentInParent<InventorySlot>() : null;

            if (clickedSlot != null && !slots.Contains(clickedSlot))
                return;

            if (draggedSlot == null)
            {
                if (clickedSlot == null || clickedSlot.itemCount <= 0)
                    return;

                bool split = rightClick && !leftClick;
                if (split && clickedSlot.itemCount < 2)
                    return;

                BeginHold(clickedSlot, split, position);
                return;
            }

            if (clickedSlot != null)
            {
                if (clickedSlot == draggedSlot)
                {
                    ClearHeldItem();
                    return;
                }

                InventorySlot source = draggedSlot;
                bool split = splitDrag;

                PredictMove(source, clickedSlot, split);
                ClearHeldItem();
                RefreshHotbar();

                if (split)
                    ClientSend.InventorySplit(-1, source.slotIndex, -1, clickedSlot.slotIndex);
                else
                    ClientSend.InventoryMove(-1, source.slotIndex, -1, clickedSlot.slotIndex);

                return;
            }

            bool overUI = hit != null && hit.GetComponentInParent<Canvas>() != null;
            if (overUI || IsOverInventory(position))
                return;

            InventorySlot dropSource = draggedSlot;
            bool dropHalf = splitDrag;
            int amountToDrop = dropHalf ? GetHalfCount(dropSource.itemCount) : dropSource.itemCount;

            dropSource.UpdateSlot(dropSource.item.type, dropSource.itemCount - amountToDrop);
            ClearHeldItem();
            RefreshHotbar();

            ClientSend.InventoryDrop(-1, dropSource.slotIndex, dropHalf);
        }

        private void BeginHold(InventorySlot source, bool split, Vector2 position)
        {
            draggedSlot = source;
            splitDrag = split;

            int heldCount = split ? GetHalfCount(source.itemCount) : source.itemCount;

            dragPreviewObject = Instantiate(dragPreviewPrefab, uiCanvas.transform);
            dragPreviewObject.transform.SetAsLastSibling();

            Image previewImage = dragPreviewObject.GetComponentInChildren<Image>(true);
            TMP_Text previewText = dragPreviewObject.GetComponentInChildren<TMP_Text>(true);

            previewImage.sprite = source.itemSprite;
            previewText.text = heldCount.ToString();

            foreach (Graphic graphic in dragPreviewObject.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            source.SetPreviewCount(source.itemCount - heldCount);
            source.SetHeld(!split);

            MoveDragPreview(position);
        }

        private void ClearHeldItem()
        {
            if (draggedSlot != null)
            {
                draggedSlot.SetPreviewCount(null);
                draggedSlot.SetHeld(false);
            }

            draggedSlot = null;
            splitDrag = false;

            if (dragPreviewObject != null)
                Destroy(dragPreviewObject);

            dragPreviewObject = null;
        }

        public void MoveDragPreview(Vector2 screenPos)
        {
            if (dragPreviewObject == null)
                return;

            RectTransform canvasRect = (RectTransform)uiCanvas.transform;
            RectTransform previewRect = dragPreviewObject.GetComponent<RectTransform>();
            Camera camera = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, camera, out Vector2 localPoint))
                previewRect.localPosition = new Vector3(localPoint.x, localPoint.y, 0);
        }

        private void PredictMove(InventorySlot from, InventorySlot to, bool split)
        {
            if (from == to || from.itemCount <= 0)
                return;

            if (split && from.itemCount < 2)
                return;

            int amountToMove = split ? GetHalfCount(from.itemCount) : from.itemCount;

            if (to.itemCount == 0 || to.item.type.Equals(from.item.type))
            {
                int availableSpace = maxStackSize - to.itemCount;

                if (amountToMove > availableSpace)
                    amountToMove = availableSpace;

                if (amountToMove <= 0)
                    return;

                to.UpdateSlot(from.item.type, to.itemCount + amountToMove);
                from.UpdateSlot(from.item.type, from.itemCount - amountToMove);
            }
            else if (!split)
            {
                ItemType previousType = to.item.type;
                int previousCount = to.itemCount;

                to.UpdateSlot(from.item.type, from.itemCount);
                from.UpdateSlot(previousType, previousCount);
            }
        }

        private static int GetHalfCount(int count)
        {
            int half = count / 2;

            if (count % 2 != 0)
                half++;

            return half;
        }

        private bool IsOverInventory(Vector2 position)
        {
            Camera camera = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera;
            return IsInsidePanel(inventoryPanel, position, camera) || IsInsidePanel(hotbarPanel, position, camera);
        }

        private static bool IsInsidePanel(RectTransform panel, Vector2 position, Camera camera)
        {
            return panel != null && panel.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(panel, position, camera);
        }

        private void OnDisable()
        {
            ClearHeldItem();
        }

        private void OnDestroy()
        {
            ClearHeldItem();

            if (Instance == this)
                Instance = null;
        }
    }
}
