using UnityEngine;

public class CurrencyTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private int increaseAmount = 100;
    [SerializeField] private int decreaseAmount = 50;
    [SerializeField] private KeyCode increaseKey = KeyCode.U;
    [SerializeField] private KeyCode decreaseKey = KeyCode.I;

    void Update()
    {
        if (!enableTesting) return;
        
        if (Input.GetKeyDown(increaseKey))
        {
            if (CurrencyManager.instance != null)
            {
                CurrencyManager.instance.AddCurrency(increaseAmount);
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
            }
            else
            {
                Debug.LogError("CurrencyManager instance not found!");
            }
        }
    }
}