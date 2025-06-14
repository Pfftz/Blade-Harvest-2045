using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slots_UI : MonoBehaviour
{
    public int slotID;
    public Inventory inventory; // Reference to the inventory this slot belongs to
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI keyText; // Add this field for the key label

    [SerializeField] private GameObject highlight;
    
    // Add this method to set the key text
    public void SetKeyText(string key)
    {
        if (keyText != null)
        {
            keyText.text = key;
        }
    }
    
    public void SetItem(Inventory.Slot slot)
    {
        if (slot != null && slot.icon != null) // Add icon null check
        {
            itemIcon.sprite = slot.icon;
            itemIcon.color = new Color(1, 1, 1, 1);
            quantityText.text = slot.count.ToString();
        }
        else
        {
            setEmpty(); // Fallback to empty if icon is null
        }
    }
    public void setEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0); // Set icon color to transparent
        quantityText.text = "";
    }

    public void SetHighlight(bool isOn)
    {
        highlight.SetActive(isOn);
    }
}
