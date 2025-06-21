using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Shop_UI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject shopPanel;
    // PENTING: Tambahkan properti public untuk shopPanel agar bisa diakses dari luar
    public GameObject ShopPanel => shopPanel; // <-- Perbaikan di sini

    [SerializeField] private TextMeshProUGUI shopTitleText;

    [Header("Buy Section")]
    [SerializeField] private Transform itemListContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private ScrollRect itemScrollView;
    [SerializeField] private GameObject buySectionGameObject;

    [Header("Sell Section")]
    [SerializeField] private GameObject sellZone;
    [SerializeField] private TextMeshProUGUI sellInstructionText;
    [SerializeField] private GameObject sellSectionGameObject;

    [Header("Notifications")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2.5f;

    [Header("Animation")]
    [SerializeField] private bool useAnimations; // Tetap private field
    // PENTING: Tambahkan properti public untuk useAnimations dan animationDuration
    public bool UseAnimations => useAnimations; // <-- Perbaikan di sini
    [SerializeField] private float animationDuration; // Tetap private field
    public float AnimationDuration => animationDuration; // <-- Perbaikan di sini

    private List<GameObject> spawnedItems = new List<GameObject>();

    public enum ShopMode
    {
        Buy,
        Sell
    }
    private ShopMode currentShopMode; 

    public void InitializeShop(List<ItemData> shopInventory)
    {
        ClearShopItems();
        foreach (ItemData item in shopInventory)
        {
            AddShopItem(item);
        }
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

    public void OpenShop(ShopMode mode)
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            if (GameManager.instance?.uiManager != null)
            {
                GameManager.instance.uiManager.ShowInventory(true);
            }
            if (useAnimations)
            {
                Transform panelTransform = shopPanel.transform;
                panelTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                LeanTween.scale(shopPanel, Vector3.one, animationDuration)
                    .setEase(LeanTweenType.easeOutBack);
            }
            SetShopMode(mode); 
        }
    }

    public void CloseShop()
    {
        // Perhatikan: Dalam metode ini, akses langsung ke 'shopPanel' masih OK karena Anda berada di dalam script yang sama.
        // Tapi di ShopInteractable, kita harus pakai 'ShopPanel' yang public.
        if (shopPanel != null)
        {
            if (useAnimations)
            {
                LeanTween.scale(shopPanel, new Vector3(0.1f, 0.1f, 0.1f), animationDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        shopPanel.SetActive(false);
                        if (GameManager.instance?.uiManager != null)
                        {
                            GameManager.instance.uiManager.ShowInventory(false);
                        }
                    });
            }
            else
            {
                shopPanel.SetActive(false);
                if (GameManager.instance?.uiManager != null)
                {
                    GameManager.instance.uiManager.ShowInventory(false);
                }
            }
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationPanel == null || notificationText == null) return;
        StopAllCoroutines();
        notificationText.text = message;
        notificationPanel.SetActive(true);
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

    public void SetShopMode(ShopMode mode)
    {
        currentShopMode = mode; 
        if (buySectionGameObject != null)
        {
            buySectionGameObject.SetActive(mode == ShopMode.Buy);
        }
        else
        {
            Debug.LogWarning("Buy Section GameObject not assigned in Shop_UI!");
        }
        if (sellSectionGameObject != null)
        {
            sellSectionGameObject.SetActive(mode == ShopMode.Sell);
        }
        else
        {
            Debug.LogWarning("Sell Section GameObject not assigned in Shop_UI!");
        }
        if (shopTitleText != null)
        {
            if (mode == ShopMode.Buy)
            {
                shopTitleText.text = "Buy Items";
                InitializeShop(GameManager.instance.shopManager.ShopInventory); 
            }
            else if (mode == ShopMode.Sell)
            {
                shopTitleText.text = "Sell Items";
            }
        }
    }

    public void ShowBuySection()
    {
        SetShopMode(ShopMode.Buy);
    }

    public void ShowSellSection()
    {
        SetShopMode(ShopMode.Sell);
    }
}