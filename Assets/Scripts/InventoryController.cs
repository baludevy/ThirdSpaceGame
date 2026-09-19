using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiteNetLib.Utils;
using UnityEngine;
using UnityEngine.AI;
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
    public GameObject invPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    List<Slot> slots = new List<Slot>();
    List<InventorySaveData> savedInventoryData = new List<InventorySaveData>();

    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, invPanel.transform).GetComponent<Slot>();
            slots.Add(slot);
            if (i < itemPrefabs.Length)
            {
                CreateItemInSlot(itemPrefabs[i], slot);
            }
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null)
            return false;

        foreach (Transform slotTransform in invPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack();
                    return true;
                }
            }
        }

        foreach (Transform slotTransform in invPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        return false;
    }

    public List<InventorySaveData> SaveInventoryToList()
    {
        List<InventorySaveData> inventoryData = new List<InventorySaveData>();

        foreach (Slot slot in slots)
        {
            if (slot.currentItem != null)
            {
                string itemName = slot.currentItem.name.Replace("(Clone)", "").Trim();
                Item itemScript = slot.currentItem.GetComponent<Item>();

                int itemCount = itemScript != null ? itemScript.Quantity : 1;

                inventoryData.Add(new InventorySaveData(itemName, itemCount));
            }
            else
            {
                inventoryData.Add(new InventorySaveData("", 0));
            }
        }
        return inventoryData;
    }

    public void LoadInventoryFromList(List<InventorySaveData> inventoryData)
    {
        if (inventoryData == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].currentItem != null)
            {
                Destroy(slots[i].currentItem);
                slots[i].currentItem = null;
            }

            if (i < inventoryData.Count && !string.IsNullOrEmpty(inventoryData[i].itemName))
            {
                string itemName = inventoryData[i].itemName;
                int itemCount = inventoryData[i].Quantity;

                GameObject prefabToSpawn = FindPrefabByName(itemName);

                if (prefabToSpawn != null)
                {
                    CreateItemInSlot(prefabToSpawn, slots[i]);
                }
            }
        }
    }

    void CreateItemInSlot(GameObject prefab, Slot slot, int quantity = 1)
    {
        GameObject item = Instantiate(prefab, slot.transform);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        slot.currentItem = item;

        Item itemScript = item.GetComponent<Item>();
        if(itemScript != null)
        {
            itemScript.Quantity = quantity;
            itemScript.UpdateQuantityDisplay();
        }
    }

    GameObject FindPrefabByName(string name)
    {
        foreach (GameObject prefab in itemPrefabs)
        {
            if (prefab != null && prefab.name == name)
            {
                return prefab;
            }
        }
        return null;
    }

    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToogleInventory();
        }
    }

    public void ToogleInventory()
    {
        if (invPanel.activeSelf)
        {
            savedInventoryData = SaveInventoryToList();
            invPanel.SetActive(false);
        }
        else
        {
            invPanel.SetActive(true);
            LoadInventoryFromList(savedInventoryData);
        }
    }
}
