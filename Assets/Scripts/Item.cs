using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int Quantity = 1;

    public ItemType itemType;

    TMP_Text quantityText;

    void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    public void AddToStack(int amount = 1)
    {
        Quantity += amount;
        UpdateQuantityDisplay();
    }
    public int RemoveFromStack(int amount = 1)
    {
        int remoevd = Mathf.Min(amount, Quantity);
        Quantity -= remoevd;
        UpdateQuantityDisplay();
        return remoevd;
    }

    public GameObject CloneItem(int newQuantity)
    {
        var clone = Instantiate(gameObject);
        var cloneItem = clone.GetComponent<Item>();
        cloneItem.Quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();
        return clone;
    }

    public void UpdateQuantityDisplay()
    {
        quantityText.text = Quantity > 1 ? Quantity.ToString() : "";
    }
}
