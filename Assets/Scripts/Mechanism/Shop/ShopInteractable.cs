using UnityEngine;
using TMPro;
using System.Collections; 

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

    [Header("Shop Mode for this NPC")]
    [SerializeField] private Shop_UI.ShopMode npcShopMode; 

    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    
    private void Start()
    {
        if (interactPopup == null || interactText == null)
        {
            SetupInteractPopup();
        }
        
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
            shopUI.OpenShop(npcShopMode); 
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
            // Menggunakan properti publik (ShopPanel, UseAnimations, AnimationDuration)
            if (shopUI.UseAnimations) // <--- Perbaikan di sini
            {
                LeanTween.scale(shopUI.ShopPanel, new Vector3(0.1f, 0.1f, 0.1f), shopUI.AnimationDuration) // <--- Perbaikan di sini
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        shopUI.ShopPanel.SetActive(false); // <--- Perbaikan di sini
                        
                        if (GameManager.instance?.uiManager != null)
                        {
                            GameManager.instance.uiManager.ShowInventory(false);
                        }
                    });
            }
            else
            {
                shopUI.ShopPanel.SetActive(false); // <--- Perbaikan di sini
                
                if (GameManager.instance?.uiManager != null)
                {
                    GameManager.instance.uiManager.ShowInventory(false);
                }
            }

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
            
            if (isShopOpen)
            {
                CloseShop();
            }
        }
    }
    
    private void SetupInteractPopup()
    {
        if (interactPopup == null)
        {
            interactPopup = new GameObject("InteractPopup");
            interactPopup.transform.SetParent(transform);
            interactPopup.transform.localPosition = Vector3.up * 1.5f;
            
            Canvas canvas = interactPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            
            interactPopup.transform.localScale = Vector3.one * 0.01f;
            
            GameObject textGO = new GameObject("InteractText");
            textGO.transform.SetParent(interactPopup.transform);
            textGO.transform.localPosition = Vector3.zero;
            
            interactText = textGO.AddComponent<TextMeshProUGUI>();
            interactText.text = interactMessage;
            interactText.fontSize = 24;
            interactText.color = Color.white;
            interactText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
        }
        
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