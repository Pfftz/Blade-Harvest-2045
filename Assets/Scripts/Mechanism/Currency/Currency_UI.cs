using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Currency_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private GameObject currencyIcon;

    [Header("Animation Settings")]
    [SerializeField] private bool animateCurrencyChanges = true;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Color positiveChangeColor = Color.green;
    [SerializeField] private Color negativeChangeColor = Color.red;

    private int previousCurrency = 0;

    private void Start()
    {
        // Find currency manager
        if (CurrencyManager.instance == null)
        {
            Debug.LogError("CurrencyManager instance not found! Make sure it exists in your scene.");
        }
        else
        {
            // Subscribe to currency changes
            CurrencyManager.instance.OnCurrencyChanged += UpdateCurrencyDisplay;
            
            // Initialize display with current value
            UpdateCurrencyDisplay(CurrencyManager.instance.GetCurrentCurrency());
        }

        // Auto-find UI elements if not assigned
        if (currencyText == null)
        {
            currencyText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void UpdateCurrencyDisplay(int newCurrency)
    {
        if (currencyText == null) return;

        // Update the display
        currencyText.text = newCurrency.ToString();
        
        // Animate if enabled
        if (animateCurrencyChanges)
        {
            // Determine if this was an increase or decrease
            if (newCurrency > previousCurrency)
            {
                AnimateCurrencyChange(true);
            }
            else if (newCurrency < previousCurrency)
            {
                AnimateCurrencyChange(false);
            }
        }
        
        // Store current value for next comparison
        previousCurrency = newCurrency;
    }

    private void AnimateCurrencyChange(bool isIncrease)
    {
        // Store original color
        Color originalColor = currencyText.color;
        
        // Change color based on increase/decrease
        currencyText.color = isIncrease ? positiveChangeColor : negativeChangeColor;
        
        // Use LeanTween to animate scale up and down
        LeanTween.scale(currencyText.gameObject, Vector3.one * 1.2f, animationDuration/2)
            .setOnComplete(() => {
                LeanTween.scale(currencyText.gameObject, Vector3.one, animationDuration/2);
            });
            
        // Reset color after animation
        LeanTween.value(gameObject, 0f, 1f, animationDuration)
            .setOnComplete(() => {
                currencyText.color = originalColor;
            });
    }

    private void OnDestroy()
    {
        // Unsubscribe when destroyed
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.OnCurrencyChanged -= UpdateCurrencyDisplay;
        }
    }
}