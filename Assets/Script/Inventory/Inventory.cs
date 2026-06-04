using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<InventoryItem> items;

    public IReadOnlyList<InventoryItem> Items => items;

    public void AddItem(ItemBase item, int quantity)
    {
        var existingItem = items.Find(i => i.Item == item);
        if (existingItem != null)
        {
            existingItem.AddQuantity(quantity);
        }
        else
        {
            items.Add(InventoryItemWrapper(item, quantity));
        }
    }

    public void RemoveItem(InventoryItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
        }
    }

    // Wrapper để tạo InventoryItem vì constructor không thể Serialize
    private InventoryItem InventoryItemWrapper(ItemBase item, int qty)
    {
        InventoryItem inv = new InventoryItem();
        typeof(InventoryItem).GetField("item", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(inv, item);
        typeof(InventoryItem).GetField("quantity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(inv, qty);
        return inv;
    }
}
