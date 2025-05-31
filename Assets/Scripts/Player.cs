using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public InventoryManager inventoryManager;
    private TileManager tileManager;

    private void Start()
    {
        tileManager = GameManager.instance.tileManager;
    }
    private void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
    }
    private void Update()
    {
        if (tileManager != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Vector3Int position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
                string tileName = tileManager.GetTileName(position);

                if (!string.IsNullOrWhiteSpace(tileName))
                {
                    if (tileName == "Interactable" && inventoryManager.toolbar.selectedSlot.itemName == "Hoe")
                    {
                        tileManager.SetInteracted(position);
                    }
                    else if (tileName == "Plowed" && (inventoryManager.toolbar.selectedSlot.itemName == "TomatoSeed" ||
                                                     inventoryManager.toolbar.selectedSlot.itemName == "RiceSeed" ||
                                                     inventoryManager.toolbar.selectedSlot.itemName == "CucumberSeed" ||
                                                     inventoryManager.toolbar.selectedSlot.itemName == "CabbageSeed"))
                    {
                        string seedType = inventoryManager.toolbar.selectedSlot.itemName;
                        tileManager.SetSeeding(position, seedType);

                        // Remove item from the toolbar slot
                        inventoryManager.toolbar.selectedSlot.RemoveItem();

                        // Refresh the toolbar UI specifically
                        GameManager.instance.uiManager.RefreshInventoryUI("Toolbar");
                    }
                    // New: Gro-Quick Light functionality
                    else if (inventoryManager.toolbar.selectedSlot.itemName == "Gro-Quick Light")
                    {
                        // Check if there's a plant at this position
                        string plantedSeed = tileManager.GetPlantedSeed(position);
                        if (!string.IsNullOrEmpty(plantedSeed))
                        {
                            // Grow the plant by one phase
                            bool grown = tileManager.GrowPlant(position);
                            if (grown)
                            {
                                Debug.Log($"Plant grown! Current phase: {tileManager.GetPlantGrowthPhase(position)}");
                            }
                        }
                    }
                    // Updated: Harvest only fully grown plants
                    else if (inventoryManager.toolbar.selectedSlot.itemName == "Scythe")
                    {
                        // Check if there's a fully grown plant here
                        if (tileManager.IsPlantFullyGrown(position))
                        {
                            tileManager.SetHarvesting(position);
                        }
                    }
                    // New: Shovel functionality
                    else if (inventoryManager.toolbar.selectedSlot.itemName == "Shovel")
                    {
                        // Check if there's a plant at this position OR if it's a plowed tile
                        if (tileManager.HasPlantAtPosition(position) || tileName == "Plowed")
                        {
                            tileManager.SetShoveling(position);
                        }
                    }
                }
            }
        }
    }
    public void DropItem(Item item)
    {
        Vector3 spawnLocation = transform.position;

        Vector3 spawnOffset = Random.insideUnitCircle * 1.25f;

        Item droppedItem = Instantiate(item, spawnLocation + spawnOffset, Quaternion.identity);

        droppedItem.rb.AddForce(spawnOffset * .2f, ForceMode2D.Impulse);
    }
    public void DropItem(Item item, int numToDrop)
    {
        for (int i = 0; i < numToDrop; i++)
        {
            DropItem(item);
        }
    }
}
