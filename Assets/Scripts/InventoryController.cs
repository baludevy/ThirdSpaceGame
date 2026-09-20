using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class InventorySaveData
{
    public string itemName;
    public int Quantity;

    public InventorySaveData(string name, int quantity)
    {
        itemName = name;
        Quantity = quantity;
    }
}
public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [Header("Inventory")]
    public GameObject invPanel;
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 20;
    public int columns = 5;
    public GameObject[] itemPrefabs;
    private readonly List<Slot> slots = new List<Slot>(); 
    private List<InventorySaveData> savedInventoryData = new List<InventorySaveData>();
    private int selectedHotbarIndex = 0;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        CreateSlots();
        invPanel.SetActive(false);
        hotbarPanel.SetActive(true);
        SelectHotbarSlot(0);        
    }
    private void CreateSlots()
    {
        slots.Clear();
        int hotbarStartIndex = slotCount - columns;
        for (int i = 0; i < slotCount; i++)
        {
           Transform parent = i >= hotbarStartIndex ? hotbarPanel.transform : invPanel.transform;
           Slot slot = Instantiate(slotPrefab, parent).GetComponent<Slot>();
           slots.Add(slot);
           if (i < itemPrefabs.Length && itemPrefabs[i] != null)
           CreateItemInSlot(itemPrefabs[i], slot); 
        }
    }
    private void Update()
    {
    if (Keyboard.current == null) return;

    if (Keyboard.current.iKey.wasPressedThisFrame) ToggleInventory();
    if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectHotbarSlot(0);
    if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectHotbarSlot(1);
    if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectHotbarSlot(3);
    if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectHotbarSlot(4);
    if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectHotbarSlot(5);
    if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectHotbarSlot(6);
    if (Keyboard.current.digit7Key.wasPressedThisFrame) SelectHotbarSlot(7);
    if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectHotbarSlot(8);
     }
     public void ToggleInventory()
    {
        invPanel.SetActive(!invPanel.activeSelf);
    }
    public void ToggleInventory(bool active)
    {
        invPanel.SetActive(active);
    }
    public Slot GetHotbarSlot(int hotbarIndex)
    {
        if (hotbarIndex < 0 || hotbarIndex >= columns) return null;
        int slotIndex = slots.Count - columns + hotbarIndex;
        if (slotIndex < 0 || slotIndex >= slots.Count) return null;
        return slots[slotIndex];
    }
    public void SelectHotbarSlot(int index)
    {
        if (index < 0 || index >= columns) return;
        selectedHotbarIndex = index;
        Slot slot = GetHotbarSlot(index);
        if (slot == null) return;
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null) Debug.Log("Selected hotbar slot" + (index + 1) + ": " + item.name);
        }
        else Debug.Log("Selected hotbar slot " + (index + 1) + ":Empty");
    }
    public Item GetSelectedHotbarItem()
    {
        Slot slot = GetHotbarSlot(selectedHotbarIndex);
        if (slot == null || slot.currentItem == null) return null;
        return slot.currentItem.GetComponent<Item>();
    }
    public int GetSelectedHotbarIndex()
    {
        return selectedHotbarIndex;
    }
    public bool AddItem(GameObject itemPrefab)
    {
        if (itemPrefab== null) return false;

        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null) continue;
            Item slotItem = slot.currentItem.GetComponent<Item>();
            if (slotItem != null && slotItem.ID == itemToAdd.ID)
            {
                slotItem.AddToStack();
                return true;
            }
        }
        foreach (Slot slot in slots)
        {
            if (slot.currentItem == null)
            {
                CreateItemInSlot(itemPrefab, slot);
                return true;
            }
        }
        return false;
    }
    public GameObject CreateItemInSlot(GameObject prefab, Slot slot, int quantity = 1)
    {
        if (prefab == null || slot == null) return null;

        GameObject item = Instantiate(prefab, slot.transform);
        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect != null) rect.anchoredPosition = Vector2.zero;
        slot.currentItem = item;
        Item itemScript = item.GetComponent<Item>();
        if (itemScript != null)
        {
            itemScript.Quantity = quantity;
            itemScript.UpdateQuantityDisplay();
        }
        return item;
    }
    public GameObject CreateItemInSlotByName(string itemName, Slot slot,int quantity)
    {
        GameObject prefab = FindPrefabByName(itemName);
        if (prefab != null)
        return CreateItemInSlot(prefab, slot, quantity);
        return null;
    }
    public bool SplitItemInSlot(Slot sourceSlot)
    {
        if (sourceSlot == null || sourceSlot.currentItem == null) return false;
        Item sourceItemScript = sourceSlot.currentItem.GetComponent<Item>();
        if (sourceItemScript == null || sourceItemScript.Quantity <= 1) return false;
        Slot targetSlot = GetFirstEmptySlot();
        if (targetSlot == null)return false;
        int splitAmount = sourceItemScript.Quantity /2;
        int remainingAmount = sourceItemScript.Quantity - splitAmount;
        sourceItemScript.Quantity = remainingAmount;
        sourceItemScript.UpdateQuantityDisplay();
        string cleanName = sourceSlot.currentItem.name.Replace("(Clone)", "").Trim();
        GameObject prefabToSpawn = FindPrefabByName(cleanName);
        if (prefabToSpawn == null) return false;
        CreateItemInSlot(prefabToSpawn, targetSlot, splitAmount);
        return true;
    }
    private Slot GetFirstEmptySlot()
    {
    foreach (Slot slot in slots)
    {
     if (slot.currentItem == null) return slot;       
        }
        return null;
    }
    public List<InventorySaveData> SaveInventoryToList()
    {
        List<InventorySaveData> inventoryData = new List<InventorySaveData>();
        foreach(Slot slot in slots)
        {
            if (slot.currentItem != null)
            {
              string  itemName = slot.currentItem.name.Replace("(Clone)", "").Trim();
              Item itemScript = slot.currentItem.GetComponent<Item>();
              int itemCount = itemScript != null ? itemScript.Quantity : 1;

              inventoryData.Add(new InventorySaveData(itemName, itemCount));
            }
            else inventoryData.Add(new InventorySaveData("", 0));
        }
        return inventoryData;
    }
    public void LoadInventoryFromList(List<InventorySaveData> inventoryData)
    {
        if (inventoryData == null) return;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].currentItem != null)
            {
                Destroy(slots[i].currentItem);
                slots[i].currentItem = null;
            }
            if (i >= inventoryData.Count || string.IsNullOrEmpty(inventoryData[i].itemName)) continue;
            GameObject prefabToSpawn = FindPrefabByName(inventoryData[i].itemName);

            if(prefabToSpawn != null)
            CreateItemInSlot(prefabToSpawn, slots[i], inventoryData[i].Quantity);
        }
    }
    public GameObject FindPrefabByName(string name)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab != null && prefab.name == name) return prefab;
        }
        return null;
    }
}
