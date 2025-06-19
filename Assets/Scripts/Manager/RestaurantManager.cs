using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager instance;

    [Header("Restaurant Settings")]
    [SerializeField] private int requiredPlantsForBonus = 3;
    [SerializeField] private int restaurantBonus = 150; [Header("Tracking")]
    [SerializeField] private int plantsSubmittedToday = 0;
    [SerializeField] private bool hasReceivedRestaurantBonus = false;
    [SerializeField] private bool qualifiedForBonusYesterday = false; // Track if player qualified yesterday

    // Events for UI updates
    public delegate void PlantSubmittedDelegate(int totalSubmitted);
    public static event PlantSubmittedDelegate OnPlantSubmitted;

    public delegate void RestaurantBonusDelegate(int bonusAmount);
    public static event RestaurantBonusDelegate OnRestaurantBonusAwarded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        // Only reset daily tracking at game startup, not on scene transitions
        // Scene transitions are handled by StartNewDay()
        Debug.Log("RestaurantManager initialized");
    }/// <summary>
     /// Call this method when a plant is sold/submitted
     /// </summary>
     /// <param name="itemName">Name of the item being sold</param>
     /// <param name="quantity">Number of items being sold</param>
    public void OnPlantSold(string itemName, int quantity)
    {
        // Check if the sold item is a plant (you can customize this logic)
        if (IsPlantItem(itemName))
        {
            plantsSubmittedToday += quantity;
            Debug.Log($"Plants submitted today: {plantsSubmittedToday}");

            // Trigger event for UI updates
            OnPlantSubmitted?.Invoke(plantsSubmittedToday);

            // Just check if qualified, but don't award bonus yet
            if (plantsSubmittedToday >= requiredPlantsForBonus)
            {
                Debug.Log($"Player has submitted {plantsSubmittedToday} plants - qualified for restaurant bonus tomorrow!");
            }
        }
    }    /// <summary>
         /// Check if the player qualifies for restaurant bonus and award it
         /// </summary>
    private void CheckRestaurantBonus()
    {
        if (!hasReceivedRestaurantBonus && plantsSubmittedToday >= requiredPlantsForBonus)
        {
            AwardRestaurantBonus();
        }
    }

    /// <summary>
    /// Check if player qualified yesterday and award the bonus
    /// </summary>
    private void CheckAndAwardPendingBonus()
    {
        if (qualifiedForBonusYesterday && !hasReceivedRestaurantBonus)
        {
            AwardRestaurantBonus();
            qualifiedForBonusYesterday = false; // Reset after awarding
        }
    }

    /// <summary>
    /// Award the restaurant bonus to the player
    /// </summary>
    private void AwardRestaurantBonus()
    {
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddCurrency(restaurantBonus);
            hasReceivedRestaurantBonus = true;

            Debug.Log($"Restaurant bonus awarded: {restaurantBonus} currency!");

            // Trigger event for UI feedback
            OnRestaurantBonusAwarded?.Invoke(restaurantBonus);
        }
    }

    /// <summary>
    /// Check if an item is considered a plant/crop
    /// </summary>
    /// <param name="itemName">Name of the item to check</param>
    /// <returns>True if the item is a plant</returns>
    private bool IsPlantItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return false;

        // Convert to lowercase for case-insensitive comparison
        string lowerItemName = itemName.ToLower();

        // List of plant/crop keywords - you can expand this list
        string[] plantKeywords = {
            "carrot", "potato", "tomato", "corn", "wheat", "rice", "bean", "pea",
            "lettuce", "cabbage", "onion", "garlic", "pepper", "cucumber", "radish",
            "spinach", "broccoli", "cauliflower", "eggplant", "squash", "pumpkin",
            "crop", "plant", "vegetable", "fruit", "seed", "harvest"
        };

        // Check if the item name contains any plant keywords
        foreach (string keyword in plantKeywords)
        {
            if (lowerItemName.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }    /// <summary>
         /// Reset daily tracking (call this when a new day starts)
         /// </summary>
    public void ResetDailyTracking()
    {        // Store whether player qualified for bonus before resetting
        bool wasQualifiedForBonus = plantsSubmittedToday >= requiredPlantsForBonus;

        // Only set the flag if player qualified but hasn't received bonus yet
        // This prevents overwriting the flag during mid-game resets
        if (wasQualifiedForBonus && !hasReceivedRestaurantBonus)
        {
            qualifiedForBonusYesterday = true;
        }

        // Reset daily counters
        plantsSubmittedToday = 0;
        hasReceivedRestaurantBonus = false; Debug.Log($"Restaurant tracking reset. Qualified for bonus yesterday: {qualifiedForBonusYesterday}");
    }

    /// <summary>
    /// Call this method at the start of a new day to handle restaurant bonus
    /// </summary>
    public void StartNewDay()
    {
        Debug.Log("=== STARTING NEW DAY ===");

        // First, check if player qualified yesterday based on current state
        bool qualifiedYesterday = plantsSubmittedToday >= requiredPlantsForBonus;

        Debug.Log($"Plants submitted yesterday: {plantsSubmittedToday}, Required: {requiredPlantsForBonus}, Qualified: {qualifiedYesterday}");

        // Award bonus if qualified and hasn't received it yet
        if (qualifiedYesterday && !hasReceivedRestaurantBonus)
        {
            Debug.Log("Player qualified for bonus yesterday, awarding now...");
            AwardRestaurantBonus();
        }
        else if (qualifiedYesterday && hasReceivedRestaurantBonus)
        {
            Debug.Log("Player qualified yesterday but already received bonus");
        }
        else
        {
            Debug.Log("Player did not qualify for bonus yesterday");
        }        // Now reset tracking for the new day
        plantsSubmittedToday = 0;
        hasReceivedRestaurantBonus = false;
        qualifiedForBonusYesterday = false; // Reset after bonus is awarded

        Debug.Log("Restaurant tracking reset for new day");
    }

    /// <summary>
    /// Check if player qualifies for restaurant transition
    /// </summary>
    /// <returns>True if player has submitted enough plants for restaurant transition</returns>
    public bool QualifiesForRestaurantTransition()
    {
        return plantsSubmittedToday >= requiredPlantsForBonus;
    }

    /// <summary>
    /// Get the current number of plants submitted today
    /// </summary>
    /// <returns>Number of plants submitted today</returns>
    public int GetPlantsSubmittedToday()
    {
        return plantsSubmittedToday;
    }

    /// <summary>
    /// Get the required number of plants for restaurant bonus
    /// </summary>
    /// <returns>Required number of plants</returns>
    public int GetRequiredPlantsForBonus()
    {
        return requiredPlantsForBonus;
    }

    /// <summary>
    /// Check if restaurant bonus has been received today
    /// </summary>
    /// <returns>True if bonus has been received</returns>
    public bool HasReceivedRestaurantBonus()
    {
        return hasReceivedRestaurantBonus;
    }

    /// <summary>
    /// Check if player qualified for bonus yesterday (useful for UI)
    /// </summary>
    /// <returns>True if qualified for bonus yesterday</returns>
    public bool QualifiedForBonusYesterday()
    {
        return qualifiedForBonusYesterday;
    }
}
