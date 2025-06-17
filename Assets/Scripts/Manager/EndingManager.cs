using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingManager : MonoBehaviour
{
    public static EndingManager instance;

    [Header("Ending Settings")]
    [SerializeField] private int goodEndingCurrencyGoal = 1000;
    [SerializeField] private int maxDays = 7;

    [Header("Cutscene References")]
    [SerializeField] private CutsceneController cutsceneController;

    [Header("Scene Names")]
    [SerializeField] private string goodEndingSceneName = "GoodEnding";
    [SerializeField] private string badEndingSceneName = "BadEnding";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

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
        // Find cutscene controller if not assigned
        if (cutsceneController == null)
        {
            cutsceneController = FindObjectOfType<CutsceneController>();
        }
    }

    /// <summary>
    /// Check if it's the final day and trigger ending if necessary
    /// </summary>
    /// <param name="currentDay">The current day number</param>
    public void CheckForGameEnding(int currentDay)
    {
        if (currentDay >= maxDays)
        {
            Debug.Log($"Final day reached! Checking ending condition...");
            StartCoroutine(TriggerGameEnding());
        }
    }

    /// <summary>
    /// Trigger the appropriate game ending based on player's currency
    /// </summary>
    private IEnumerator TriggerGameEnding()
    {
        // Wait a moment to ensure all systems are updated
        yield return new WaitForSeconds(1f); int currentCurrency = 0;
        if (CurrencyManager.instance != null)
        {
            currentCurrency = CurrencyManager.instance.GetCurrentCurrency();
        }

        Debug.Log($"Game ending triggered. Current currency: {currentCurrency}, Goal: {goodEndingCurrencyGoal}");

        if (currentCurrency >= goodEndingCurrencyGoal)
        {
            TriggerGoodEnding();
        }
        else
        {
            TriggerBadEnding();
        }
    }

    /// <summary>
    /// Trigger the good ending cutscene
    /// </summary>
    private void TriggerGoodEnding()
    {
        Debug.Log("Triggering good ending!");

        if (cutsceneController != null)
        {
            // Play good ending cutscene
            cutsceneController.RunCutscene(Cutscenes.GoodEndingCutscene, OnGoodEndingCutsceneComplete);
        }
        else
        {
            // Fallback: directly load good ending scene
            Debug.LogWarning("CutsceneController not found! Loading good ending scene directly.");
            LoadGoodEndingScene();
        }
    }

    /// <summary>
    /// Trigger the bad ending cutscene
    /// </summary>
    private void TriggerBadEnding()
    {
        Debug.Log("Triggering bad ending!");

        if (cutsceneController != null)
        {
            // Play bad ending cutscene
            cutsceneController.RunCutscene(Cutscenes.BadEndingCutscene, OnBadEndingCutsceneComplete);
        }
        else
        {
            // Fallback: directly load bad ending scene
            Debug.LogWarning("CutsceneController not found! Loading bad ending scene directly.");
            LoadBadEndingScene();
        }
    }

    /// <summary>
    /// Called when good ending cutscene completes
    /// </summary>
    private void OnGoodEndingCutsceneComplete()
    {
        Debug.Log("Good ending cutscene completed!");
        LoadGoodEndingScene();
    }

    /// <summary>
    /// Called when bad ending cutscene completes
    /// </summary>
    private void OnBadEndingCutsceneComplete()
    {
        Debug.Log("Bad ending cutscene completed!");
        LoadBadEndingScene();
    }

    /// <summary>
    /// Load the good ending scene
    /// </summary>
    private void LoadGoodEndingScene()
    {
        // You can create a dedicated good ending scene, or just return to main menu with a flag
        PlayerPrefs.SetInt("LastEndingWasGood", 1);
        PlayerPrefs.Save();

        if (SceneManager.GetSceneByName(goodEndingSceneName).IsValid())
        {
            SceneManager.LoadScene(goodEndingSceneName);
        }
        else
        {
            // Fallback to main menu
            Debug.LogWarning($"Good ending scene '{goodEndingSceneName}' not found! Loading main menu.");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Load the bad ending scene
    /// </summary>
    private void LoadBadEndingScene()
    {
        // You can create a dedicated bad ending scene, or just return to main menu with a flag
        PlayerPrefs.SetInt("LastEndingWasGood", 0);
        PlayerPrefs.Save();

        if (SceneManager.GetSceneByName(badEndingSceneName).IsValid())
        {
            SceneManager.LoadScene(badEndingSceneName);
        }
        else
        {
            // Fallback to main menu
            Debug.LogWarning($"Bad ending scene '{badEndingSceneName}' not found! Loading main menu.");
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    /// <summary>
    /// Get the currency goal for good ending
    /// </summary>
    /// <returns>Currency goal amount</returns>
    public int GetGoodEndingCurrencyGoal()
    {
        return goodEndingCurrencyGoal;
    }

    /// <summary>
    /// Get the maximum number of days
    /// </summary>
    /// <returns>Maximum days</returns>
    public int GetMaxDays()
    {
        return maxDays;
    }

    /// <summary>
    /// Set the cutscene controller reference
    /// </summary>
    /// <param name="controller">The cutscene controller to use</param>
    public void SetCutsceneController(CutsceneController controller)
    {
        cutsceneController = controller;
    }
}
