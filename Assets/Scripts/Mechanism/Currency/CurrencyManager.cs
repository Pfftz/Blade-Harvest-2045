using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [SerializeField] private int currentCurrency = 0;

    // Event for currency changes
    public event Action<int> OnCurrencyChanged;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Get the current currency amount
    public int GetCurrentCurrency()
    {
        return currentCurrency;
    }

    // Set currency to a specific value (used when loading save data)
    public void SetCurrency(int amount)
    {
        currentCurrency = amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        Debug.Log($"Currency set to: {currentCurrency}");
    }

    // Add currency
    public void AddCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempting to add negative currency. Use RemoveCurrency instead.");
            return;
        }

        currentCurrency += amount;
        OnCurrencyChanged?.Invoke(currentCurrency);
        Debug.Log($"Added {amount} currency. New balance: {currentCurrency}");
    }

    // Remove currency
    public bool RemoveCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempting to remove negative currency. Use AddCurrency instead.");
            return false;
        }

        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            OnCurrencyChanged?.Invoke(currentCurrency);
            Debug.Log($"Removed {amount} currency. New balance: {currentCurrency}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough currency. Current: {currentCurrency}, Attempting to remove: {amount}");
            return false;
        }
    }
}