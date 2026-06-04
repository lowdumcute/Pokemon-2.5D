
using UnityEngine;
[System.Serializable]

public class InventoryItem
{
    [SerializeField] private ItemBase item;
    [SerializeField] private int quantity;

    public ItemBase Item => item;
    public int Quantity => quantity;

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;
    }
}
