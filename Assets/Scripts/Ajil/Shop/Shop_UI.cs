using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Shop_UI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI shopTitleText;
    
    [Header("Buy Section")]
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private ScrollRect itemScrollView;

    [Header("Sell Section")]
    [SerializeField] private GameObject sellZone;
    [SerializeField] private TextMeshProUGUI sellInstructionText;

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2.5f;

    [Header("Animation")]
    [SerializeField] private bool useAnimations = true;
    [SerializeField] private float animationDuration = 0.3f;

    private List<GameObject> spawnedItems = new List<GameObject>();
    
    // Initialize the shop panel with available items
    public void InitializeShop(List<ItemData> shopInventory)
    {
        ClearShopItems();
        
        foreach (ItemData item in shopInventory)
        {
            AddShopItem(item);
        }
        
        // Reset scroll position to top
        if (itemScrollView != null)
        {
            itemScrollView.normalizedPosition = new Vector2(0, 1);
        }
    }

    private void AddShopItem(ItemData item)
    {
        if (itemListContainer == null || shopItemPrefab == null) return;
        
        GameObject itemObj = Instantiate(shopItemPrefab, itemListContainer);
        ShopItem_UI shopItem = itemObj.GetComponent<ShopItem_UI>();
        
        if (shopItem != null)
        {
            shopItem.Setup(item);
        }
        
        spawnedItems.Add(itemObj);
    }
    
    private void ClearShopItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();
    }

    // Open the shop panel
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            
            // Open inventory too
            if (GameManager.instance?.uiManager != null)
            {
                GameManager.instance.uiManager.ShowInventory(true);
            }
            
            if (useAnimations)
            {
                // Animation for opening
                Transform panelTransform = shopPanel.transform;
                panelTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                LeanTween.scale(shopPanel, Vector3.one, animationDuration)
                    .setEase(LeanTweenType.easeOutBack);
            }
        }
    }

    // Close the shop panel
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            if (useAnimations)
            {
                // Animation for closing
                LeanTween.scale(shopPanel, new Vector3(0.1f, 0.1f, 0.1f), animationDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        shopPanel.SetActive(false);
                        
                        // Close inventory too
                        if (GameManager.instance?.uiManager != null)
                        {
                            GameManager.instance.uiManager.ShowInventory(false);
                        }
                    });
            }
            else
            {
                shopPanel.SetActive(false);
                
                // Close inventory too
                if (GameManager.instance?.uiManager != null)
                {
                    GameManager.instance.uiManager.ShowInventory(false);
                }
            }
        }
    }

    // Show notification message
    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;
        
        // Stop any existing notification coroutine
        StopAllCoroutines();
        
        // Set the notification text
        notificationText.text = message;
        
        // Show the notification
        notificationPanel.SetActive(true);
        
        // Hide after delay
        StartCoroutine(HideNotificationAfterDelay());
    }
    
    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
}