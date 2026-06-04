using UnityEngine;
public enum ItemType
{
    Item,
    medicine,
    Pokeball,
    HmTm,
    KeyItem,
    Berry
    // Sau này có thể mở rộng
}

[CreateAssetMenu(menuName = "Item/Create New Item")]
public class ItemBase : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private ItemType type;
    [SerializeField] private Sprite icon;

    public string ItemName => itemName;
    public string Description => description;
    public ItemType Type => type;
    public Sprite Icon => icon;
        // Trong ItemBase
    public virtual bool Use(Pokemon target) => false;
}
