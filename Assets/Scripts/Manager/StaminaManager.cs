using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaManager : MonoBehaviour
{
    [Header("Stamina Settings")]
    public int maxStamina = 100;
    public int currentStamina;

    [Header("Stamina Costs")]
    public int plowingCost = 5;
    public int seedingCost = 3;
    public int harvestingCost = 4;
    public int shovelingCost = 6;
    public int groQuickLightCost = 2;

    [Header("Stamina Regeneration")]
    public int staminaRegenPerSecond = 1;
    public float regenDelay = 2f; // Delay before regeneration starts after using stamina

    private float lastStaminaUseTime;
    private Coroutine regenCoroutine;

    // Events for UI updates
    public System.Action<int, int> OnStaminaChanged; // current, max

    private void Start()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);

        // Start regeneration coroutine
        StartStaminaRegeneration();
    }

    public bool HasStamina(int cost)
    {
        return currentStamina >= cost;
    }

    public bool UseStamina(int cost)
    {
        if (currentStamina >= cost)
        {
            currentStamina -= cost;
            currentStamina = Mathf.Max(0, currentStamina);
            lastStaminaUseTime = Time.time;

            OnStaminaChanged?.Invoke(currentStamina, maxStamina);

            Debug.Log($"Used {cost} stamina. Current: {currentStamina}/{maxStamina}");
            return true;
        }

        Debug.Log("Not enough stamina!");
        return false;
    }

    public void RestoreStamina(int amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(maxStamina, currentStamina);
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    public void RestoreFullStamina()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void StartStaminaRegeneration()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(StaminaRegenerationCoroutine());
    }

    private IEnumerator StaminaRegenerationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // Only regenerate if enough time has passed since last stamina use
            if (Time.time - lastStaminaUseTime >= regenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenPerSecond;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }
    }

    public int GetStaminaCostForAction(string action)
    {
        switch (action.ToLower())
        {
            case "plowing": return plowingCost;
            case "seeding": return seedingCost;
            case "harvesting": return harvestingCost;
            case "shoveling": return shovelingCost;
            case "gro-quick": return groQuickLightCost;
            default: return 0;
        }
    }
}