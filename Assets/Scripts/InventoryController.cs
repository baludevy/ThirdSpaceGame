using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject invPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    void Start()
    {
        for(int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, invPanel.transform).GetComponent<Slot>();
            if(i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
    }
}