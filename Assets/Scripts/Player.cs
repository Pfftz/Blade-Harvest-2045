using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public InventoryManager inventoryManager;
    private TileManager tileManager;
    private StaminaManager staminaManager; // Add stamina manager reference

    // Static inventory data to persist between scenes
    private static Dictionary<string, List<InventorySlotData>> savedInventoryData;

    [System.Serializable]
    public class InventorySlotData
    {
        public string itemName;
        public int count;
        public int maxAllowed;
    }

    private void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
        staminaManager = GetComponent<StaminaManager>(); // Get stamina manager

        // Add StaminaManager if it doesn't exist
        if (staminaManager == null)
        {
            staminaManager = gameObject.AddComponent<StaminaManager>();
        }
    }

    private void Start()
    {
        // Ensure InventoryManager is available
        if (inventoryManager == null)
        {
            inventoryManager = GetComponent<InventoryManager>();
        }

        // Register this player with GameManager first
        if (GameManager.instance != null)
        {
            GameManager.instance.player = this;
        }

        // Load inventory data if available
        LoadInventoryData();

        // Delay getting TileManager and UI refresh to ensure proper initialization
        StartCoroutine(DelayedInitialization());
    }

    private IEnumerator DelayedInitialization()
    {
        // Wait for GameManager to refresh its references
        yield return new WaitForSeconds(0.4f);

        // Try to get TileManager reference
        if (GameManager.instance != null)
        {
            tileManager = GameManager.instance.tileManager;

            // If still null, try to find it directly
            if (tileManager == null)
            {
                tileManager = FindObjectOfType<TileManager>();
                if (tileManager != null)
                {
                    // Update GameManager reference too
                    GameManager.instance.tileManager = tileManager;
                }
            }
        }

        // Refresh UI after loading data and getting references
        if (GameManager.instance != null && GameManager.instance.uiManager != null)
        {
            GameManager.instance.uiManager.RefreshAll();
        }

        // Debug log to confirm initialization
        if (tileManager != null)
        {
            Debug.Log("TileManager successfully found and assigned to Player");
        }
        else
        {
            Debug.LogError("TileManager not found! Make sure TileManager exists in the scene.");
        }
    }

    private void Update()
    {
        // Add additional null check and try to get TileManager if missing
        if (tileManager == null)
        {
            // Try to get TileManager reference again
            if (GameManager.instance != null && GameManager.instance.tileManager != null)
            {
                tileManager = GameManager.instance.tileManager;
            }
            else
            {
                // Try to find it directly in the scene
                tileManager = FindObjectOfType<TileManager>();
                if (tileManager != null && GameManager.instance != null)
                {
                    GameManager.instance.tileManager = tileManager;
                }
            }
        }

        // Only proceed if all references are valid
        if (tileManager != null && inventoryManager != null && inventoryManager.toolbar != null && staminaManager != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
                string tileName = tileManager.GetTileName(position);

                // Debug current state
                Debug.Log($"Position: {position}, Tile: {tileName}, Selected Item: {inventoryManager.toolbar.selectedSlot?.itemName ?? "NULL"}");

                if (!string.IsNullOrWhiteSpace(tileName))
                {
                    // Add null check for selectedSlot
                    if (inventoryManager.toolbar.selectedSlot == null)
                    {
                        Debug.LogWarning("No slot selected in toolbar!");
                        return;
                    }

                    string selectedItem = inventoryManager.toolbar.selectedSlot.itemName;

                    if (tileName == "Interactable" && selectedItem == "Hoe")
                    {
                        // Check stamina before plowing
                        int plowingCost = staminaManager.GetStaminaCostForAction("plowing");
                        if (staminaManager.HasStamina(plowingCost))
                        {
                            Debug.Log("Plowing tile...");
                            tileManager.SetInteracted(position);
                            staminaManager.UseStamina(plowingCost);
                        }
                        else
                        {
                            Debug.Log("Not enough stamina to plow!");
                        }
                    }
                    else if (tileName == "Plowed" && (selectedItem == "TomatoSeed" ||
                                                     selectedItem == "RiceSeed" ||
                                                     selectedItem == "CucumberSeed" ||
                                                     selectedItem == "CabbageSeed"))
                    {
                        // Check stamina before seeding
                        int seedingCost = staminaManager.GetStaminaCostForAction("seeding");
                        if (staminaManager.HasStamina(seedingCost))
                        {
                            Debug.Log($"Planting {selectedItem}...");
                            string seedType = selectedItem;
                            tileManager.SetSeeding(position, seedType);

                            // Remove item from the toolbar slot
                            inventoryManager.toolbar.selectedSlot.RemoveItem();

                            // Use stamina and refresh UI
                            staminaManager.UseStamina(seedingCost);
                            GameManager.instance.uiManager.RefreshInventoryUI("Toolbar");
                        }
                        else
                        {
                            Debug.Log("Not enough stamina to plant!");
                        }
                    }
                    // New: Gro-Quick Light functionality
                    else if (selectedItem == "Gro-Quick Light")
                    {
                        // Check if there's a plant at this position
                        string plantedSeed = tileManager.GetPlantedSeed(position);
                        if (!string.IsNullOrEmpty(plantedSeed))
                        {
                            // Check stamina before using Gro-Quick Light
                            int groQuickCost = staminaManager.GetStaminaCostForAction("gro-quick");
                            if (staminaManager.HasStamina(groQuickCost))
                            {
                                // Grow the plant by one phase
                                bool grown = tileManager.GrowPlant(position);
                                if (grown)
                                {
                                    Debug.Log($"Plant grown! Current phase: {tileManager.GetPlantGrowthPhase(position)}");
                                    staminaManager.UseStamina(groQuickCost);
                                }
                            }
                            else
                            {
                                Debug.Log("Not enough stamina to use Gro-Quick Light!");
                            }
                        }
                    }
                    // Updated: Harvest only fully grown plants
                    else if (selectedItem == "Scythe")
                    {
                        // Check if there's a fully grown plant here
                        if (tileManager.IsPlantFullyGrown(position))
                        {
                            // Check stamina before harvesting
                            int harvestingCost = staminaManager.GetStaminaCostForAction("harvesting");
                            if (staminaManager.HasStamina(harvestingCost))
                            {
                                Debug.Log("Harvesting plant...");
                                tileManager.SetHarvesting(position);
                                staminaManager.UseStamina(harvestingCost);
                            }
                            else
                            {
                                Debug.Log("Not enough stamina to harvest!");
                            }
                        }
                    }
                    // New: Shovel functionality
                    else if (selectedItem == "Shovel")
                    {
                        // Check if there's a plant at this position OR if it's a plowed tile
                        if (tileManager.HasPlantAtPosition(position) || tileName == "Plowed")
                        {
                            // Check stamina before shoveling
                            int shovelingCost = staminaManager.GetStaminaCostForAction("shoveling");
                            if (staminaManager.HasStamina(shovelingCost))
                            {
                                Debug.Log("Using shovel...");
                                tileManager.SetShoveling(position);
                                staminaManager.UseStamina(shovelingCost);
                            }
                            else
                            {
                                Debug.Log("Not enough stamina to use shovel!");
                            }
                        }
                    }
                    else
                    {
                        Debug.Log($"No valid action for tile '{tileName}' with item '{selectedItem}'");
                    }
                }
            }
        }
        else
        {
            // Only log warnings once every few seconds to avoid spam
            if (Time.time % 3f < Time.deltaTime) // Only log every 3 seconds
            {
                if (tileManager == null) Debug.LogWarning("TileManager is null!");
                if (inventoryManager == null) Debug.LogWarning("InventoryManager is null!");
                if (inventoryManager?.toolbar == null) Debug.LogWarning("Toolbar is null!");
                if (staminaManager == null) Debug.LogWarning("StaminaManager is null!");
            }
        }
    }

    private void OnDestroy()
    {
        // Only save if we're actually changing scenes, not just being destroyed
        if (GameManager.instance != null)
        {
            SaveInventoryData();
        }
    }

    // Make this method public so it can be called externally
    public void SaveInventoryData()
    {
        if (savedInventoryData == null)
            savedInventoryData = new Dictionary<string, List<InventorySlotData>>();

        // Ensure inventoryManager exists
        if (inventoryManager == null)
        {
            Debug.LogWarning("Cannot save inventory data - inventoryManager is null");
            return;
        }

        try
        {
            // Save backpack
            List<InventorySlotData> backpackData = new List<InventorySlotData>();
            if (inventoryManager.backpack != null && inventoryManager.backpack.slots != null)
            {
                foreach (var slot in inventoryManager.backpack.slots)
                {
                    backpackData.Add(new InventorySlotData
                    {
                        itemName = slot.itemName,
                        count = slot.count,
                        maxAllowed = slot.maxAllowed
                    });
                }
                savedInventoryData["Backpack"] = backpackData;
            }

            // Save toolbar
            List<InventorySlotData> toolbarData = new List<InventorySlotData>();
            if (inventoryManager.toolbar != null && inventoryManager.toolbar.slots != null)
            {
                foreach (var slot in inventoryManager.toolbar.slots)
                {
                    toolbarData.Add(new InventorySlotData
                    {
                        itemName = slot.itemName,
                        count = slot.count,
                        maxAllowed = slot.maxAllowed
                    });
                }
                savedInventoryData["Toolbar"] = toolbarData;
            }

            Debug.Log("Inventory data saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving inventory data: {e.Message}");
        }
    }

    private void LoadInventoryData()
    {
        if (savedInventoryData == null) return;

        // Load backpack
        if (savedInventoryData.ContainsKey("Backpack"))
        {
            var backpackData = savedInventoryData["Backpack"];
            for (int i = 0; i < backpackData.Count && i < inventoryManager.backpack.slots.Count; i++)
            {
                var slotData = backpackData[i];
                var slot = inventoryManager.backpack.slots[i];

                slot.itemName = slotData.itemName;
                slot.count = slotData.count;
                slot.maxAllowed = slotData.maxAllowed;

                if (!string.IsNullOrEmpty(slotData.itemName))
                {
                    Item item = GameManager.instance.itemManager.GetItemByName(slotData.itemName);
                    if (item != null)
                    {
                        slot.icon = item.data.icon;
                    }
                }
            }
        }

        // Load toolbar
        if (savedInventoryData.ContainsKey("Toolbar"))
        {
            var toolbarData = savedInventoryData["Toolbar"];
            for (int i = 0; i < toolbarData.Count && i < inventoryManager.toolbar.slots.Count; i++)
            {
                var slotData = toolbarData[i];
                var slot = inventoryManager.toolbar.slots[i];

                slot.itemName = slotData.itemName;
                slot.count = slotData.count;
                slot.maxAllowed = slotData.maxAllowed;

                if (!string.IsNullOrEmpty(slotData.itemName))
                {
                    Item item = GameManager.instance.itemManager.GetItemByName(slotData.itemName);
                    if (item != null)
                    {
                        slot.icon = item.data.icon;
                    }
                }
            }
        }
    }

    public void DropItem(Item item)
    {
        if (item == null)
        {
            Debug.LogError("Cannot drop null item");
            return;
        }

        Vector3 spawnLocation = transform.position;
        Vector3 spawnOffset = Random.insideUnitCircle * 1.25f;

        try
        {
            Item droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

            if (droppedItem.rb != null)
            {
                droppedItem.rb.AddForce(spawnOffset * .2f, ForceMode2D.Impulse);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error dropping item: {e.Message}");
        }
    }

    public void DropItem(Item item, int numToDrop)
    {
        for (int i = 0; i < numToDrop; i++)
        {
            DropItem(item);
        }
    }

    // Property to access stamina manager
    public StaminaManager StaminaManager => staminaManager;
}