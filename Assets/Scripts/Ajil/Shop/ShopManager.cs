using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Shop Items")]
    [SerializeField] private List<ItemData> shopInventory = new List<ItemData>();
    
    [Header("UI References")]
    [SerializeField] private Shop_UI shopUI;

    [Header("Settings")]
    [SerializeField] private float sellValueMultiplier = 0.75f; // Items sell for 75% of buy price

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Initialize the shop UI with available items
        if (shopUI != null)
        {
            shopUI.InitializeShop(shopInventory);
        }
    }

    // Buy an item and add it to player inventory
    public bool BuyItem(ItemData itemData)
    {
        // Check if player has enough money
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return false;
        }

        if (CurrencyManager.instance.GetCurrentCurrency() < itemData.buyPrice)
        {
            Debug.Log("Not enough money to buy " + itemData.itemName);
            shopUI.ShowNotification("Not enough money!");
            return false;
        }

        // Try to add the item to player's inventory
        Item itemToAdd = GameManager.instance.itemManager.GetItemByName(itemData.itemName);
        if (itemToAdd == null)
        {
            Debug.LogError($"Item '{itemData.itemName}' not found in ItemManager");
            return false;
        }

        // Check if inventory has space
        bool canAddToInventory = CheckInventorySpace(itemToAdd);
        if (!canAddToInventory)
        {
            Debug.Log("Inventory is full!");
            shopUI.ShowNotification("Inventory is full!");
            return false;
        }

        // Remove money and add item to inventory
        CurrencyManager.instance.RemoveCurrency(itemData.buyPrice);
        AddItemToInventory(itemToAdd);

        // Show success notification
        shopUI.ShowNotification($"Bought {itemData.itemName} for {itemData.buyPrice} coins");
        
        return true;
    }

    // Sell an item from player inventory
    public bool SellItem(string itemName, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemName)) return false;

        // Find item data to get price
        ItemData itemData = GetItemDataByName(itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"No ItemData found for item: {itemName}");
            return false;
        }

        int sellPrice = Mathf.FloorToInt(itemData.buyPrice * sellValueMultiplier);
        int totalValue = sellPrice * quantity;

        // Add currency
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddCurrency(totalValue);
            shopUI.ShowNotification($"Sold {quantity}x {itemName} for {totalValue} coins");
            return true;
        }
        
        return false;
    }

    // Helper methods
    private bool CheckInventorySpace(Item item)
    {
        if (GameManager.instance?.player?.inventoryManager == null) return false;
        
        // Check backpack first
        Inventory backpack = GameManager.instance.player.inventoryManager.GetInventoryByName("Backpack");
        if (backpack != null)
        {
            // Look for existing stack or empty slot
            foreach (var slot in backpack.slots)
            {
                if (slot.IsEmpty || (slot.itemName == item.data.itemName && slot.count < slot.maxAllowed))
                {
                    return true;
                }
            }
        }
        
        return false;
    }

    private void AddItemToInventory(Item item)
    {
        if (GameManager.instance?.player?.inventoryManager != null)
        {
            GameManager.instance.player.inventoryManager.Add("Backpack", item);
            
            // Refresh UI
            if (GameManager.instance.uiManager != null)
            {
                GameManager.instance.uiManager.RefreshAll();
            }
        }
    }

    private ItemData GetItemDataByName(string itemName)
    {
        Item item = GameManager.instance.itemManager.GetItemByName(itemName);
        return item?.data;
    }
}