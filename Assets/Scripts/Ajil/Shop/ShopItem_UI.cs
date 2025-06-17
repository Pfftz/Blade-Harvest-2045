using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem_UI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button buyButton;

    private ItemData currentItem;

    public void Setup(ItemData item)
    {
        if (item == null) return;
        
        currentItem = item;
        
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.preserveAspect = true;
        }
        
        if (itemNameText != null)
            itemNameText.text = item.itemName;
            
        if (itemPriceText != null)
            itemPriceText.text = $"{item.buyPrice} coins";
            
        if (itemDescriptionText != null)
            itemDescriptionText.text = item.description;
            
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyItem);
        }
    }
    
    public void BuyItem()
    {
        if (currentItem == null) return;
        
        if (ShopManager.instance != null)
        {
            ShopManager.instance.BuyItem(currentItem);
        }
    }
}