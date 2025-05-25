using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_UI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Player player;
    public List<Slots_UI> slots = new List<Slots_UI>();

    [SerializeField] private Canvas canvas; // Reference to the canvas for UI elements
    private Slots_UI draggedSlot;
    private Image draggedIcon;
    private bool dragSingle;
    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    void Start()
    {
        inventoryPanel.SetActive(false);
        InitializeSlots();
    }

    void InitializeSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].slotID = i;
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

        // Fix: Use GetKey() to check if LeftShift is being held
        if (Input.GetKey(KeyCode.LeftShift))
        {
            dragSingle = true;
        }
        else
        {
            dragSingle = false;
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
                if (player.inventory.slots[i].itemName != "")
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
    public void Remove()
    {
        // Add null check for empty slots
        if (string.IsNullOrEmpty(player.inventory.slots[draggedSlot.slotID].itemName))
        {
            return; // Don't try to drop empty slots
        }
        Item itemToDrop = GameManager.instance.itemManager.GetItemByName(player.inventory.slots[draggedSlot.slotID].itemName);
        if (itemToDrop != null)
        {
            if (dragSingle)
            {
                player.DropItem(itemToDrop);
                player.inventory.Remove(draggedSlot.slotID);
            }
            else
            {
                player.DropItem(itemToDrop, player.inventory.slots[draggedSlot.slotID].count);
                player.inventory.Remove(draggedSlot.slotID, player.inventory.slots[draggedSlot.slotID].count);
            }
            Refresh();
        }

        draggedSlot = null; // Clear the dragged slot reference after removing the item
    }
    public void SlotBeginDrag(Slots_UI slot)
    {
        if (slot == null) return;

        draggedSlot = slot;
        draggedIcon = Instantiate(draggedSlot.itemIcon);
        draggedIcon.transform.SetParent(canvas.transform);
        draggedIcon.raycastTarget = false;
        draggedIcon.rectTransform.sizeDelta = new Vector2(50, 50);

        MoveToMousePosition(draggedIcon.gameObject);
        Debug.Log("Start Drag: " + draggedSlot.name);
    }

    public void SlotDrag()
    {
        if (draggedIcon != null)
        {
            MoveToMousePosition(draggedIcon.gameObject); // Update the position of the dragged icon
        }

        if (draggedSlot != null)
        {
            Debug.Log("Dragging slot: " + draggedSlot.name);
        }
    }
    public void SlotEndDrag()
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon.gameObject); // Destroy the dragged icon when the drag ends
            draggedIcon = null; // Clear the reference to the dragged icon
        }

        if (draggedSlot != null)
        {
            Debug.Log("End Drag: " + draggedSlot.name);
            draggedSlot = null; // Clear the dragged slot reference
        }
    }
    public void SlotDrop(Slots_UI slot)
    {
        Debug.Log("Dropped" + draggedSlot.name + "Drop on slot: " + slot.name);

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
