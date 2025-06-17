using UnityEngine;

public class CurrencyTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private int increaseAmount = 100;
    [SerializeField] private int decreaseAmount = 50;
    [SerializeField] private KeyCode increaseKey = KeyCode.U;
    [SerializeField] private KeyCode decreaseKey = KeyCode.I;
    [SerializeField] private bool enableTesting = true;

    void Update()
    {
        if (!enableTesting) return;

        if (Input.GetKeyDown(increaseKey))
        {
            if (CurrencyManager.instance != null)
            {
                CurrencyManager.instance.AddCurrency(increaseAmount);
                Debug.Log($"Added {increaseAmount} coins. New balance: {CurrencyManager.instance.GetCurrentCurrency()}");
            }
            else
            {
                Debug.LogError("CurrencyManager instance not found!");
            }
        }
        
        if (Input.GetKeyDown(decreaseKey))
        {
            if (CurrencyManager.instance != null)
            {
                CurrencyManager.instance.RemoveCurrency(decreaseAmount);
                Debug.Log($"Removed {decreaseAmount} coins. New balance: {CurrencyManager.instance.GetCurrentCurrency()}");
            }
            else
            {
                Debug.LogError("CurrencyManager instance not found!");
            }
        }
    }
}