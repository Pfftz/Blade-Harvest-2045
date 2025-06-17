using UnityEngine;

public class RestaurantManager : MonoBehaviour
{
    public static RestaurantManager instance;

    [Header("Restaurant Settings")]
    [SerializeField] private int requiredPlantsForBonus = 3;
    [SerializeField] private int restaurantBonus = 150;

    [Header("Tracking")]
    [SerializeField] private int plantsSubmittedToday = 0;
    [SerializeField] private bool hasReceivedRestaurantBonus = false;

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
        // Reset daily tracking when starting the game
        ResetDailyTracking();
    }

    /// <summary>
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

            // Check if we should award restaurant bonus
            CheckRestaurantBonus();
        }
    }

    /// <summary>
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
    }

    /// <summary>
    /// Reset daily tracking (call this when a new day starts)
    /// </summary>
    public void ResetDailyTracking()
    {
        plantsSubmittedToday = 0;
        hasReceivedRestaurantBonus = false;
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
}
