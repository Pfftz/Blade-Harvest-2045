using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slots_UI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Button removeButton; // Add this reference

    private int slotID;
    private Inventory_UI inventoryUI;

    public void Initialize(int id, Inventory_UI invUI)
    {
        slotID = id;
        inventoryUI = invUI;
        
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => inventoryUI.Remove(slotID));
        }
    }
    public void SetItem(Inventory.Slot slot)
    {
        if (slot != null)
        {
            itemIcon.sprite = slot.icon;
            itemIcon.color = new Color(1, 1, 1, 1); // Set icon color to white (fully visible)}
            quantityText.text = slot.count.ToString();
        }
    }
    public void setEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0); // Set icon color to transparent
        quantityText.text = "";
    }
}
