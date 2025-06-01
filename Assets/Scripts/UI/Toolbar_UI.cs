using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar_UI : MonoBehaviour
{
    [SerializeField] private List<Slots_UI> toolbarSlots = new List<Slots_UI>();
    [SerializeField] private Canvas canvas; // Add canvas reference for drag functionality

    private Slots_UI selectedSlot;
    private Inventory toolbarInventory;

    private void Awake()
    {
        // Find canvas for drag functionality
        canvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        // Delay initialization to ensure GameManager and Player are ready
        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        // Wait until GameManager and Player are available
        while (GameManager.instance == null || GameManager.instance.player == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Get toolbar inventory reference
        toolbarInventory = GameManager.instance.player.inventoryManager.toolbar;

        SetupSlots(); // Setup slot references
        SetupKeyLabels(); // Setup key labels
        SelectSlot(0); // Select the first slot by default
    }

    // Add this method to setup slot references (similar to Inventory_UI)
    private void SetupSlots()
    {
        if (toolbarInventory == null) return;

        int counter = 0;
        foreach (Slots_UI slot in toolbarSlots)
        {
            slot.slotID = counter;
            slot.inventory = toolbarInventory; // Set the inventory reference for each slot
            counter++;
        }
    }

    // Add this new method to setup key labels
    private void SetupKeyLabels()
    {
        for (int i = 0; i < toolbarSlots.Count && i < 10; i++)
        {
            if (toolbarSlots[i] != null)
            {
                // Set key text: 1-9 for slots 0-8, 0 for slot 9
                string keyText = (i == 9) ? "0" : (i + 1).ToString();
                toolbarSlots[i].SetKeyText(keyText);
            }
        }
    }

    private void Update()
    {
        // Only check keys if everything is initialized
        if (toolbarSlots.Count == 10 && GameManager.instance?.player != null)
        {
            CheckAlphaNumericKeys();
        }
    }

    public void SelectSlot(Slots_UI slot)
    {
        if (slot != null)
        {
            SelectSlot(slot.slotID);
        }
    }

    public void SelectSlot(int index)
    {
        // Add null checks to prevent errors
        if (toolbarSlots == null || toolbarSlots.Count == 0)
        {
            Debug.LogWarning("Toolbar slots not initialized");
            return;
        }

        if (index < 0 || index >= toolbarSlots.Count)
        {
            Debug.LogWarning($"Invalid slot index: {index}");
            return;
        }

        if (GameManager.instance?.player?.inventoryManager?.toolbar == null)
        {
            Debug.LogWarning("Player inventory not ready for slot selection");
            return;
        }

        if (toolbarSlots.Count == 10)
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetHighlight(false); // Deselect the previous slot
            }

            selectedSlot = toolbarSlots[index];
            selectedSlot.SetHighlight(true); // Highlight the selected slot

            GameManager.instance.player.inventoryManager.toolbar.SelectSlot(index); // Update the player's selected slot

            Debug.Log($"Selected toolbar slot {index}: {GameManager.instance.player.inventoryManager.toolbar.selectedSlot.itemName}");
        }
    }

    // Add drag functionality methods (similar to Inventory_UI)
    public void SlotBeginDrag(Slots_UI slot)
    {
        if (slot == null) return;

        // Check if the slot has an item before starting drag
        if (slot.inventory == null ||
            slot.slotID >= slot.inventory.slots.Count ||
            string.IsNullOrEmpty(slot.inventory.slots[slot.slotID].itemName))
        {
            Debug.LogWarning("Cannot drag empty toolbar slot or invalid slot");
            return;
        }

        UI_Manager.draggedSlot = slot;
        UI_Manager.draggedIcon = Instantiate(slot.itemIcon);
        UI_Manager.draggedIcon.transform.SetParent(canvas.transform);
        UI_Manager.draggedIcon.raycastTarget = false;
        UI_Manager.draggedIcon.rectTransform.sizeDelta = new Vector2(50, 50);

        MoveToMousePosition(UI_Manager.draggedIcon.gameObject);
        Debug.Log("Start Drag Toolbar: " + UI_Manager.draggedSlot.name);
    }

    public void SlotDrag()
    {
        if (UI_Manager.draggedIcon != null)
        {
            MoveToMousePosition(UI_Manager.draggedIcon.gameObject);
        }
    }

    public void SlotEndDrag()
    {
        if (UI_Manager.draggedIcon != null)
        {
            Destroy(UI_Manager.draggedIcon.gameObject);
            UI_Manager.draggedIcon = null;
        }

        if (UI_Manager.draggedSlot != null)
        {
            Debug.Log("End Drag Toolbar: " + UI_Manager.draggedSlot.name);
            UI_Manager.draggedSlot = null;
        }
    }

    public void SlotDrop(Slots_UI slot)
    {
        // Handle dropping items on toolbar slots
        if (UI_Manager.draggedSlot == null || slot == null)
        {
            Debug.LogWarning("Dragged slot or target toolbar slot is null");
            return;
        }

        if (UI_Manager.dragSingle)
        {
            UI_Manager.draggedSlot.inventory.MoveSlot(UI_Manager.draggedSlot.slotID, slot.slotID, slot.inventory);
        }
        else
        {
            UI_Manager.draggedSlot.inventory.MoveSlot(UI_Manager.draggedSlot.slotID, slot.slotID, slot.inventory,
                UI_Manager.draggedSlot.inventory.slots[UI_Manager.draggedSlot.slotID].count);
        }

        // Refresh UI
        if (GameManager.instance.uiManager != null)
        {
            GameManager.instance.uiManager.RefreshAll();
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
                null,
                out position
            );
            toMove.transform.position = canvas.transform.TransformPoint(position);
        }
    }

    private void CheckAlphaNumericKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SelectSlot(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SelectSlot(5);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SelectSlot(6);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SelectSlot(7);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SelectSlot(8);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectSlot(9);
        }
    }
}
