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
        // Delay initialization to ensure Player is ready
        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        // Wait a frame to ensure all objects are initialized
        yield return null;

        // Wait until GameManager and Player are available
        while (GameManager.instance == null || GameManager.instance.player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Initialize inventory reference
        if (TryGetInventory())
        {
            SetupSlots();
            Refresh();
        }
    }

    private bool TryGetInventory()
    {
        if (GameManager.instance?.player?.inventoryManager != null)
        {
            inventory = GameManager.instance.player.inventoryManager.GetInventoryByName(inventoryName);
            return inventory != null;
        }
        return false;
    }

    void SetupSlots()
    {
        if (inventory == null) return;

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
        // Add null checks to prevent errors during scene transitions
        if (inventory == null)
        {
            // Try to get inventory reference again
            if (!TryGetInventory())
            {
                Debug.LogWarning($"Cannot refresh {inventoryName} - inventory reference is null");
                return;
            }
        }

        if (slots == null || inventory.slots == null)
        {
            Debug.LogWarning($"Cannot refresh {inventoryName} - slots are null");
            return;
        }

        // Logic to set up the inventory UI, such as populating slots with items
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
            Debug.LogError($"Slots UI count ({slots.Count}) does not match {inventoryName} inventory slots count ({inventory.slots.Count}).");
        }
    }

    public void SlotDrop(Slots_UI slot)
    {
        // Add null checks
        if (UI_Manager.draggedSlot == null || slot == null)
        {
            Debug.LogWarning("Dragged slot or target slot is null");
            return;
        }

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

    public void Remove()
    {
        // Add comprehensive null checks
        if (UI_Manager.draggedSlot == null)
        {
            Debug.LogWarning("No dragged slot to remove");
            return;
        }

        // Use the dragged slot's inventory instead of this UI's inventory
        Inventory sourceInventory = UI_Manager.draggedSlot.inventory;

        if (sourceInventory == null || sourceInventory.slots == null)
        {
            Debug.LogError("Source inventory or slots is null");
            return;
        }

        if (UI_Manager.draggedSlot.slotID >= sourceInventory.slots.Count)
        {
            Debug.LogError("Slot ID out of range");
            return;
        }

        // Add null check for empty slots
        if (string.IsNullOrEmpty(sourceInventory.slots[UI_Manager.draggedSlot.slotID].itemName))
        {
            Debug.LogWarning("Trying to remove empty slot");
            return; // Don't try to drop empty slots
        }

        // Add null checks for GameManager and its components
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager instance is null");
            return;
        }

        if (GameManager.instance.itemManager == null)
        {
            Debug.LogError("ItemManager is null - make sure it's assigned to GameManager");
            return;
        }

        Item itemToDrop = GameManager.instance.itemManager.GetItemByName(sourceInventory.slots[UI_Manager.draggedSlot.slotID].itemName);

        if (itemToDrop != null && GameManager.instance.player != null)
        {
            if (UI_Manager.dragSingle)
            {
                GameManager.instance.player.DropItem(itemToDrop);
                sourceInventory.Remove(UI_Manager.draggedSlot.slotID);
            }
            else
            {
                GameManager.instance.player.DropItem(itemToDrop, sourceInventory.slots[UI_Manager.draggedSlot.slotID].count);
                sourceInventory.Remove(UI_Manager.draggedSlot.slotID, sourceInventory.slots[UI_Manager.draggedSlot.slotID].count);
            }

            // Refresh the correct UI
            if (GameManager.instance.uiManager != null)
            {
                GameManager.instance.uiManager.RefreshAll();
            }
        }
        else
        {
            Debug.LogError($"Could not drop item - itemToDrop: {itemToDrop?.name}, player: {GameManager.instance.player?.name}");
        }

        // Clear dragged references
        UI_Manager.draggedSlot = null;
        if (UI_Manager.draggedIcon != null)
        {
            UI_Manager.draggedIcon.gameObject.SetActive(false);
        }
    }

    public void SlotBeginDrag(Slots_UI slot)
    {
        if (slot == null) return;

        // Check if the slot has an item before starting drag
        if (slot.inventory == null ||
            slot.slotID >= slot.inventory.slots.Count ||
            string.IsNullOrEmpty(slot.inventory.slots[slot.slotID].itemName))
        {
            Debug.LogWarning("Cannot drag empty slot or invalid slot");
            return;
        }

        UI_Manager.draggedSlot = slot;
        UI_Manager.draggedIcon = Instantiate(slot.itemIcon);
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
