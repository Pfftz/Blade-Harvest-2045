using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SellZone : MonoBehaviour, IDropHandler
{
    [Header("Visual Settings")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.92f, 0.016f, 0.7f);
    [SerializeField] private TextMeshProUGUI dropHereText;
    
    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseAmount = 0.1f;

    private void Start()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }

        // Start pulsing animation
        if (dropHereText != null)
        {
            LeanTween.scale(dropHereText.gameObject, Vector3.one * (1 + pulseAmount), pulseSpeed)
                .setLoopPingPong()
                .setEase(LeanTweenType.easeInOutSine);
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // Check if an item is being dragged and dropped
        if (UI_Manager.draggedSlot != null && !string.IsNullOrEmpty(UI_Manager.draggedSlot.inventory.slots[UI_Manager.draggedSlot.slotID].itemName))
        {
            SellDraggedItem();
        }
    }
    
    private void SellDraggedItem()
    {
        if (UI_Manager.draggedSlot == null || UI_Manager.draggedSlot.inventory == null) return;
        
        int slotId = UI_Manager.draggedSlot.slotID;
        Inventory inventory = UI_Manager.draggedSlot.inventory;
        
        if (slotId >= 0 && slotId < inventory.slots.Count)
        {
            string itemName = inventory.slots[slotId].itemName;
            int itemCount = UI_Manager.dragSingle ? 1 : inventory.slots[slotId].count;
            
            // Sell the item
            if (!string.IsNullOrEmpty(itemName) && itemCount > 0 && ShopManager.instance != null)
            {
                // Process the sale
                bool success = ShopManager.instance.SellItem(itemName, itemCount);
                
                if (success)
                {
                    // Remove from inventory
                    if (UI_Manager.dragSingle)
                    {
                        inventory.Remove(slotId);
                    }
                    else
                    {
                        inventory.Remove(slotId, itemCount);
                    }
                    
                    // Update UI
                    if (GameManager.instance?.uiManager != null)
                    {
                        GameManager.instance.uiManager.RefreshAll();
                    }
                    
                    // Flash effect
                    StartCoroutine(FlashEffect());
                }
            }
        }
        
        // Clear drag references
        if (UI_Manager.draggedIcon != null)
        {
            Destroy(UI_Manager.draggedIcon.gameObject);
            UI_Manager.draggedIcon = null;
        }
        UI_Manager.draggedSlot = null;
    }
    
    private System.Collections.IEnumerator FlashEffect()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
            yield return new WaitForSeconds(0.2f);
            backgroundImage.color = normalColor;
        }
    }
    
    // Called by event trigger when item is dragged over
    public void OnDragEnter()
    {
        if (backgroundImage != null && UI_Manager.draggedSlot != null)
        {
            backgroundImage.color = highlightColor;
        }
    }
    
    // Called by event trigger when item is dragged out
    public void OnDragExit()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
}