using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_UI : MonoBehaviour
{
    public string inventoryName;
    public List<Slots_UI> slots = new List<Slots_UI>();

    [SerializeField] private Canvas canvas; // Reference to the canvas for UI elements
    private Inventory inventory;
    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    void Start()
    {
        inventory = GameManager.instance.player.inventory.GetInventoryByName(inventoryName);
        SetupSlots();
        Refresh();
    }

    void SetupSlots()
    {
        int counter = 0;
        foreach (Slots_UI slot in slots)
        {
            slot.slotID = counter;
            counter++;
            slot.inventory = inventory; // Set the inventory reference for each slot
        }
    }
    public void Refresh()
    {
        // Logic to set up the inventory UI, such as populating slots with items
        // This could involve iterating through the player's inventory and updating the UI elements
        if (slots.Count == inventory.slots.Count)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (inventory.slots[i].itemName != "")
                {
                    slots[i].SetItem(inventory.slots[i]);
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
    public void Remove()
    {
        // Add null check for empty slots
        if (string.IsNullOrEmpty(inventory.slots[UI_Manager.draggedSlot.slotID].itemName))
        {
            return; // Don't try to drop empty slots
        }
        Item itemToDrop = GameManager.instance.itemManager.GetItemByName(inventory.slots[UI_Manager.draggedSlot.slotID].itemName);
        if (itemToDrop != null)
        {
            if (UI_Manager.dragSingle)
            {
                GameManager.instance.player.DropItem(itemToDrop);
                inventory.Remove(UI_Manager.draggedSlot.slotID);
            }
            else
            {
                GameManager.instance.player.DropItem(itemToDrop, inventory.slots[UI_Manager.draggedSlot.slotID].count);
                inventory.Remove(UI_Manager.draggedSlot.slotID, inventory.slots[UI_Manager.draggedSlot.slotID].count);
            }
            Refresh();
        }

        UI_Manager.draggedSlot = null; // Clear the dragged slot reference after removing the item
    }
    public void SlotBeginDrag(Slots_UI slot)
    {
        if (slot == null) return;

        UI_Manager.draggedSlot = slot;
        UI_Manager.draggedIcon = Instantiate(UI_Manager.draggedSlot.itemIcon);
        UI_Manager.draggedIcon.transform.SetParent(canvas.transform);
        UI_Manager.draggedIcon.raycastTarget = false;
        UI_Manager.draggedIcon.rectTransform.sizeDelta = new Vector2(50, 50);

        MoveToMousePosition(UI_Manager.draggedIcon.gameObject);
        Debug.Log("Start Drag: " + UI_Manager.draggedSlot.name);
    }

    public void SlotDrag()
    {
        if (UI_Manager.draggedIcon != null)
        {
            MoveToMousePosition(UI_Manager.draggedIcon.gameObject); // Update the position of the dragged icon
        }

        if (UI_Manager.draggedSlot != null)
        {
            Debug.Log("Dragging slot: " + UI_Manager.draggedSlot.name);
        }
    }
    public void SlotEndDrag()
    {
        if (UI_Manager.draggedIcon != null)
        {
            Destroy(UI_Manager.draggedIcon.gameObject); // Destroy the dragged icon when the drag ends
            UI_Manager.draggedIcon = null; // Clear the reference to the dragged icon
        }

        if (UI_Manager.draggedSlot != null)
        {
            Debug.Log("End Drag: " + UI_Manager.draggedSlot.name);
            UI_Manager.draggedSlot = null; // Clear the dragged slot reference
        }
    }
    public void SlotDrop(Slots_UI slot)
    {
        if (UI_Manager.dragSingle)
        {
            UI_Manager.draggedSlot.inventory.MoveSlot(UI_Manager.draggedSlot.slotID, slot.slotID, slot.inventory);
        }
        else
        {
            UI_Manager.draggedSlot.inventory.MoveSlot(UI_Manager.draggedSlot.slotID, slot.slotID, slot.inventory, UI_Manager.draggedSlot.inventory.slots[UI_Manager.draggedSlot.slotID].count);
        }
        GameManager.instance.uiManager.RefreshAll();
    }
    private void MoveToMousePosition(GameObject toMove)
    {
        if (canvas != null)
        {
            Vector2 position;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null, // Use null for the camera if the canvas is in Screen Space - Overlay mode
                out position
            );
            toMove.transform.position = canvas.transform.TransformPoint(position);
        }
    }
}
