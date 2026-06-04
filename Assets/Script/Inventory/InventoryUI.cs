using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] TMP_Text NameOption;
    [SerializeField] OptionBag[] optionBags;

    private int currentOption = 0;
    private int currentItem = -1;
    private List<InventoryItem> currentItems = new List<InventoryItem>();
    private List<GameObject> currentItemObjects = new List<GameObject>(); // chứa các prefab được tạo

    [Header("Data")]
    [SerializeField] Inventory inventory;
    [SerializeField] GameObject Container;
    [SerializeField] GameObject ItemPrefabs;

    [Header("Info Item")]
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemDescription;
    [SerializeField] Material outlineMaterial;

    private void OnEnable()
    {
        NameOption.text = "";
        ClearContainer();
        UpdateOptionOutline();
        UpdateMenuVisual();
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            currentOption = (currentOption - 1 + optionBags.Length) % optionBags.Length;
            UpdateOptionOutline();
            UpdateMenuVisual();
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            currentOption = (currentOption + 1) % optionBags.Length;
            UpdateOptionOutline();
            UpdateMenuVisual();
        }

        if (currentItems.Count > 0)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame ||
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                currentItem = (currentItem + 1) % currentItems.Count;
                UpdateSelectedItemInfo();
            }
            if (Keyboard.current.zKey.wasPressedThisFrame && BattleSystem.Instance.state == BattleState.BagScreen)
            {
                UseSelectedItem();
            }
        }
    }

    void UpdateMenuVisual()
    {
        if (currentOption < 0 || currentOption >= optionBags.Length)
            return;

        NameOption.text = optionBags[currentOption].type.ToString();
        HandleSelection();
    }

    void HandleSelection()
    {
        currentItem = 0;
        currentItems.Clear();
        currentItemObjects.Clear();
        ClearContainer();

        ItemType selectedType = optionBags[currentOption].type;

        foreach (var item in inventory.Items)
        {
            if (item.Item.Type == selectedType)
            {
                currentItems.Add(item);

                GameObject itemObj = Instantiate(ItemPrefabs, Container.transform);
                currentItemObjects.Add(itemObj);

                Image img = itemObj.GetComponent<Image>();
                if (img != null)
                    img.sprite = item.Item.Icon;

                TMP_Text quantityText = itemObj.GetComponentInChildren<TMP_Text>();
                if (quantityText != null)
                    quantityText.text = $"x{item.Quantity}";
            }
        }

        UpdateSelectedItemInfo();
    }

    void UpdateSelectedItemInfo()
    {
        if (currentItems.Count == 0 || currentItem < 0 || currentItem >= currentItems.Count)
        {
            itemName.text = "";
            itemDescription.text = "";
            return;
        }

        var selected = currentItems[currentItem];
        itemName.text = selected.Item.ItemName;
        itemDescription.text = selected.Item.Description;

        // Cập nhật outline cho item prefab
        for (int i = 0; i < currentItemObjects.Count; i++)
        {
            var img = currentItemObjects[i].GetComponent<Image>();
            if (img != null)
                img.material = (i == currentItem) ? outlineMaterial : null;
        }
    }
    private void UseSelectedItem()
    {
        if (currentItem < 0 || currentItem >= currentItems.Count)
            return;

        var selectedInventoryItem = currentItems[currentItem];
        var selectedItem = selectedInventoryItem.Item;

        // Dùng Pokéball
        if (selectedItem.Type == ItemType.Pokeball)
        {
            var pokeball = selectedItem as PokeballItem;
            if (pokeball != null)
            {
                selectedInventoryItem.RemoveQuantity(1);

                // Nếu số lượng còn 0 thì xóa khỏi inventory
                if (selectedInventoryItem.Quantity <= 0)
                {
                    inventory.RemoveItem(selectedInventoryItem);

                }

                BattleSystem.Instance.StartCoroutine(BattleSystem.Instance.TryToCatchPokemon(pokeball));
                gameObject.SetActive(false);
                return;
            }
        }

        // Dùng medicine
        if (selectedItem.Type == ItemType.medicine)
        {
            selectedInventoryItem.RemoveQuantity(1);

            // Nếu số lượng còn 0 thì xóa khỏi inventory
            if (selectedInventoryItem.Quantity <= 0)
            {
                inventory.RemoveItem(selectedInventoryItem);

            }

            BattleSystem.Instance.UseItemOnPokemon(selectedItem);
            gameObject.SetActive(false);
            return;
        }

        Debug.Log("This item cannot be used in battle.");
    }

    void UpdateOptionOutline()
    {
        for (int i = 0; i < optionBags.Length; i++)
        {
            var img = optionBags[i].optionButton.GetComponent<Image>();
            if (img != null)
                img.material = (i == currentOption) ? outlineMaterial : null;
        }
    }

    public void ClearContainer()
    {
        foreach (Transform child in Container.transform)
            Destroy(child.gameObject);
    }
}

[System.Serializable]
public class OptionBag
{
    public ItemType type;
    public Button optionButton;
}
