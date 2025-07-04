using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;

    [Header("Shop Items")]
    [SerializeField] private List<ItemData> shopInventory = new List<ItemData>();
    public List<ItemData> ShopInventory => shopInventory;

    [Header("UI References")]
    [SerializeField] private Shop_UI shopUI;

    [Header("Settings")]
    [SerializeField] private float sellValueMultiplier = 1f;

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
        else
        {
            Debug.LogWarning("ShopUI is not assigned in ShopManager! UI notifications will not work.");
        }
    }// Buy an item and add it to player inventory
    public bool BuyItem(ItemData itemData)
    {
        // Validate input
        if (itemData == null)
        {
            Debug.LogError("ItemData is null in BuyItem!");
            return false;
        }

        // Clear any existing drag references at the start of purchase
        ClearDragReferences();

        // Check if player has enough money
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return false;
        }

        if (CurrencyManager.instance.GetCurrentCurrency() < itemData.buyPrice)
        {
            Debug.Log("Not enough money to buy " + itemData.itemName);
            if (shopUI != null)
                shopUI.ShowNotification("Not enough money!");
            return false;
        }

        // Try to add the item to player's inventory
        Item itemToAdd = GameManager.instance.itemManager.GetItemByName(itemData.itemName);
        if (itemToAdd == null)
        {
            Debug.LogError($"Item '{itemData.itemName}' not found in ItemManager");
            return false;
        }        // Check if inventory has space
        bool canAddToInventory = CheckInventorySpace(itemToAdd);
        if (!canAddToInventory)
        {
            Debug.Log("Inventory is full!");
            if (shopUI != null)
                shopUI.ShowNotification("Inventory is full!");
            return false;
        }

        // Remove money and add item to inventory
        CurrencyManager.instance.RemoveCurrency(itemData.buyPrice);
        AddItemToInventory(itemToAdd);

        // Show success notification
        if (shopUI != null)
            shopUI.ShowNotification($"Bought {itemData.itemName} for {itemData.buyPrice} coins");
        else
            Debug.Log($"Bought {itemData.itemName} for {itemData.buyPrice} coins");

        // Clear drag references again after successful purchase
        ClearDragReferences();

        return true;
    }

    // Sell an item from player inventory
    public bool SellItem(string itemName, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemName)) return false;

        if (IsToolItem(itemName))
        {
            // Show notification that tools can't be sold
            if (shopUI != null && shopUI.gameObject.activeInHierarchy)
            {
                shopUI.ShowNotification("Tools cannot be sold!");
            }
            else
            {
                Debug.Log($"Cannot sell {itemName} - Tools are not sellable");
            }
            return false;
        }

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

            // Only show notification if shop UI is active
            if (shopUI != null && shopUI.gameObject.activeInHierarchy)
            {
                shopUI.ShowNotification($"Sold {quantity}x {itemName} for {totalValue} coins");
            }
            else
            {
                // Alternative: Log to console or use a different notification system
                Debug.Log($"Sold {quantity}x {itemName} for {totalValue} coins");
            }

            return true;
        }

        return false;
    }

    private bool CheckInventorySpace(Item item)
    {
        if (item?.data == null || GameManager.instance?.player?.inventoryManager == null)
            return false;

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
        if (item == null)
        {
            Debug.LogError("Item is null in AddItemToInventory!");
            return;
        }

        if (GameManager.instance?.player?.inventoryManager != null)
        {
            GameManager.instance.player.inventoryManager.Add("Backpack", item);

            // Refresh UI
            if (GameManager.instance.uiManager != null)
            {
                GameManager.instance.uiManager.RefreshAll();
            }
        }
        else
        {
            Debug.LogError("GameManager, Player, or InventoryManager is null in AddItemToInventory!");
        }
    }

    private ItemData GetItemDataByName(string itemName)
    {
        Item item = GameManager.instance.itemManager.GetItemByName(itemName);
        return item?.data;
    }

    // Helper method to clear drag references
    private void ClearDragReferences()
    {
        if (UI_Manager.draggedSlot != null)
        {
            UI_Manager.draggedSlot = null;
        }

        if (UI_Manager.draggedIcon != null)
        {
            if (UI_Manager.draggedIcon.gameObject != null)
            {
                Destroy(UI_Manager.draggedIcon.gameObject);
            }
            UI_Manager.draggedIcon = null;
        }
    }
    
    private bool IsToolItem(string itemName)
    {
        // Dapatkan ItemData dari nama item
        ItemData itemData = GetItemDataByName(itemName);
        
        // Periksa apakah item tersebut ada dan kategorinya Tool
        if (itemData != null && itemData.category == ItemData.ItemCategory.Tool)
        {
            return true;
        }
        
        return false;
    }
}