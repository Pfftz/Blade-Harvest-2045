using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI quantityText;
    public GameObject staminaFrame;

    private StaminaManager staminaManager;

    private void Start()
    {
        // Find stamina manager
        if (GameManager.instance?.player != null)
        {
            staminaManager = GameManager.instance.player.GetComponent<StaminaManager>();
        }

        if (staminaManager == null)
        {
            staminaManager = FindObjectOfType<StaminaManager>();
        }

        if (staminaManager != null)
        {
            // Subscribe to stamina changes
            staminaManager.OnStaminaChanged += UpdateStaminaDisplay;

            // Initialize display
            UpdateStaminaDisplay(staminaManager.currentStamina, staminaManager.maxStamina);
        }
        else
        {
            Debug.LogError("StaminaManager not found! Make sure it's attached to the Player.");
        }

        // Auto-find UI elements if not assigned
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void UpdateStaminaDisplay(int currentStamina, int maxStamina)
    {
        // Update text
        if (quantityText != null)
        {
            quantityText.text = currentStamina.ToString();
        }

        // Change color based on stamina level
        if (quantityText != null)
        {
            float staminaPercentage = (float)currentStamina / maxStamina;

            if (staminaPercentage <= 0.2f) // 20% or less - red
            {
                quantityText.color = Color.red;
            }
            else if (staminaPercentage <= 0.5f) // 50% or less - yellow
            {
                quantityText.color = Color.yellow;
            }
            else // Above 50% - white/normal
            {
                quantityText.color = Color.white;
            }
        }
    }

    private void OnDestroy()
    {
        if (staminaManager != null)
        {
            staminaManager.OnStaminaChanged -= UpdateStaminaDisplay;
        }
    }
}