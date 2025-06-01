using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public Dictionary<string, Inventory_UI> inventoryUIByName = new Dictionary<string, Inventory_UI>();
    public GameObject inventoryPanel; // Reference to the inventory UI panel
    public List<Inventory_UI> inventoryUIs;
    public static Slots_UI draggedSlot;
    public static Image draggedIcon;
    public static bool dragSingle;

    private void Awake()
    {
        Initialize();
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // Ensure the inventory panel is hidden at start
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.B))
        {
            ToggleInventoryUI();
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            dragSingle = true;
        }
        else
        {
            dragSingle = false;
        }
    }

    public void ToggleInventoryUI()
    {
        // Logic to show/hide the inventory UI
        if (inventoryPanel != null)
        {
            if (!inventoryPanel.activeSelf)
            {
                inventoryPanel.SetActive(true);
                RefreshInventoryUI("Backpack"); // Refresh the backpack inventory UI when opened
            }
            else
            {
                inventoryPanel.SetActive(false);
            }
        }
    }

    public void RefreshInventoryUI(string inventoryName)
    {
        if (inventoryUIByName.ContainsKey(inventoryName) && inventoryUIByName[inventoryName] != null)
        {
            inventoryUIByName[inventoryName].Refresh();
        }
    }

    private void Start()
    {
        // Ensure this UI Manager is registered with GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.uiManager = this;
        }

        // Delay refresh to ensure all systems are initialized
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        // Wait a few frames to ensure all objects are properly initialized
        yield return new WaitForSeconds(0.1f);

        Initialize();
        RefreshAll();
    }

    public void RefreshAll()
    {
        // Only refresh if we have valid references
        if (GameManager.instance?.player?.inventoryManager == null)
        {
            Debug.LogWarning("Cannot refresh UI - Player or InventoryManager not ready");
            return;
        }

        // Clear and reinitialize inventory UI references
        inventoryUIByName.Clear();
        Initialize();

        // Refresh all inventory UIs with null checks
        foreach (KeyValuePair<string, Inventory_UI> keyValuePair in inventoryUIByName)
        {
            if (keyValuePair.Value != null)
            {
                try
                {
                    keyValuePair.Value.Refresh();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error refreshing {keyValuePair.Key}: {e.Message}");
                }
            }
        }
    }

    public Inventory_UI GetInventoryUI(string inventoryName)
    {
        if (inventoryUIByName.ContainsKey(inventoryName))
        {
            return inventoryUIByName[inventoryName];
        }
        return null;
    }

    void Initialize()
    {
        if (inventoryUIs == null) return;

        foreach (Inventory_UI ui in inventoryUIs)
        {
            if (ui != null && !string.IsNullOrEmpty(ui.inventoryName))
            {
                if (!inventoryUIByName.ContainsKey(ui.inventoryName))
                {
                    inventoryUIByName.Add(ui.inventoryName, ui);
                }
            }
        }
    }
}
