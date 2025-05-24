using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_UI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Player player;
    public List<Slots_UI> slots = new List<Slots_UI>();

    void Start()
    {
        inventoryPanel.SetActive(false);
        InitializeSlots();
    }

    void InitializeSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Initialize(i, this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Toggle the inventory UI
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        // Logic to show/hide the inventory UI
        // This could involve enabling/disabling a UI panel or changing its visibility
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
            Refresh(); // Call Refresh to populate the inventory UI
        }
        else
        {
            inventoryPanel.SetActive(false);
        }
    }
    void Refresh()
    {
        // Logic to set up the inventory UI, such as populating slots with items
        // This could involve iterating through the player's inventory and updating the UI elements
        if (slots.Count == player.inventory.slots.Count)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (player.inventory.slots[i].type != CollectableType.NONE)
                {
                    slots[i].SetItem(player.inventory.slots[i]);
                }
                else
                {
                    slots[i].setEmpty();
                }
            }
        }
        else
        {
            Debug.LogError("Slots UI count does not match player's inventory slots count.");
        }
    }
    public void Remove(int slotID)
    {
        Collectable itemToDrop = GameManager.instance.itemManager.GetItemByType(player.inventory.slots[slotID].type);
        if (itemToDrop != null)
        {
            player.DropItem(itemToDrop);
            player.inventory.Remove(slotID);
            Refresh();
        }
    }
}
