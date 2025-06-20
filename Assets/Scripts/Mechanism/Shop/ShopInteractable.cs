using UnityEngine;
using TMPro;

public class ShopInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string interactMessage = "Press E to Open Shop";
    [SerializeField] private string exitMessage = "Press E to Close Shop";
    
    [Header("UI Settings")]
    [SerializeField] private GameObject interactPopup;
    [SerializeField] private TextMeshProUGUI interactText;
    
    [Header("Shop References")]
    [SerializeField] private Shop_UI shopUI;
    
    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    
    private void Start()
    {
        // Setup interact popup only if not assigned
        if (interactPopup == null || interactText == null)
        {
            SetupInteractPopup();
        }
        
        // Set initial text if component exists
        if (interactText != null)
        {
            interactText.text = interactMessage;
        }
        
        ShowInteractPopup(false);
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            if (!isShopOpen)
            {
                OpenShop();
                shopUI.InitializeShop(GameManager.instance.shopManager.ShopInventory);
                isShopOpen = true;
            }
            else
            {
                CloseShop();
                isShopOpen = false;
            }
        }
        
        if (isShopOpen && Input.GetKeyDown(KeyCode.Tab))
        {
            CloseShop();
            isShopOpen = false;
        }
    }
    
    private void OpenShop()
    {
        if (shopUI != null)
        {
            shopUI.OpenShop();
            isShopOpen = true;
            if (interactText != null)
            {
                interactText.text = exitMessage;
            }
        }
    }
    
    private void CloseShop()
    {
        if (shopUI != null)
        {
            shopUI.CloseShop();
            isShopOpen = false;
            if (interactText != null)
            {
                interactText.text = interactMessage;
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            ShowInteractPopup(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowInteractPopup(false);
            
            // Auto-close shop when player walks away
            if (isShopOpen)
            {
                CloseShop();
            }
        }
    }
    
    private void SetupInteractPopup()
    {
        // Create interact popup if not assigned (fallback)
        if (interactPopup == null)
        {
            // Create popup GameObject
            interactPopup = new GameObject("InteractPopup");
            interactPopup.transform.SetParent(transform);
            interactPopup.transform.localPosition = Vector3.up * 1.5f;
            
            // Add Canvas component for world space UI
            Canvas canvas = interactPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            
            // Scale down the canvas
            interactPopup.transform.localScale = Vector3.one * 0.01f;
            
            // Create text GameObject
            GameObject textGO = new GameObject("InteractText");
            textGO.transform.SetParent(interactPopup.transform);
            textGO.transform.localPosition = Vector3.zero;
            
            // Add TextMeshPro component
            interactText = textGO.AddComponent<TextMeshProUGUI>();
            interactText.text = interactMessage;
            interactText.fontSize = 24;
            interactText.color = Color.white;
            interactText.alignment = TextAlignmentOptions.Center;
            
            // Setup RectTransform
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
        }
        
        // Set text if component exists
        if (interactText != null)
        {
            interactText.text = interactMessage;
        }
    }
    
    private void ShowInteractPopup(bool show)
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(show);
        }
    }
}