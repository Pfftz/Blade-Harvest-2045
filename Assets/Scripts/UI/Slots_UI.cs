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

    [SerializeField] private GameObject highlight;

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
