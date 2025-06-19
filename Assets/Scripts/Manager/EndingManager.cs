using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    }    /// <summary>
         /// Trigger the good ending cutscene
         /// </summary>
    private void TriggerGoodEnding()
    {
        Debug.Log("Triggering good ending!");

        if (cutsceneController != null)
        {
            // Play good ending cutscene directly
            cutsceneController.RunCutscene(Cutscenes.GoodEnding, OnGoodEndingCutsceneComplete);
        }
        else
        {
            // Fallback: directly load good ending scene with CutsceneManager
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
            // Play bad ending cutscene directly
            cutsceneController.RunCutscene(Cutscenes.BadEnding, OnBadEndingCutsceneComplete);
        }
        else
        {
            // Fallback: directly load bad ending scene with CutsceneManager
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
    }    /// <summary>
         /// Load the good ending scene
         /// </summary>
    private void LoadGoodEndingScene()
    {
        // You can create a dedicated good ending scene, or just return to main menu with a flag
        PlayerPrefs.SetInt("LastEndingWasGood", 1);
        PlayerPrefs.Save();

        // Check if the scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == goodEndingSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (sceneExists)
        {
            SceneManager.LoadScene(goodEndingSceneName);
        }
        else
        {
            // Fallback: display ending message and return to main menu
            Debug.LogWarning($"Good ending scene '{goodEndingSceneName}' not found! Showing message and returning to main menu.");
            StartCoroutine(ShowEndingMessageAndReturnToMenu(true));
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

        // Check if the scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == badEndingSceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (sceneExists)
        {
            SceneManager.LoadScene(badEndingSceneName);
        }
        else
        {
            // Fallback: display ending message and return to main menu
            Debug.LogWarning($"Bad ending scene '{badEndingSceneName}' not found! Showing message and returning to main menu.");
            StartCoroutine(ShowEndingMessageAndReturnToMenu(false));
        }
    }

    /// <summary>
    /// Show ending message and return to main menu when ending scenes don't exist
    /// </summary>
    private IEnumerator ShowEndingMessageAndReturnToMenu(bool isGoodEnding)
    {
        // Create a temporary UI to show the ending message
        GameObject tempCanvas = new GameObject("TempEndingCanvas");
        Canvas canvas = tempCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        tempCanvas.AddComponent<CanvasScaler>();

        // Create ending text
        GameObject textObject = new GameObject("EndingText");
        textObject.transform.SetParent(tempCanvas.transform, false);

        var text = textObject.AddComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = isGoodEnding ?
            "CONGRATULATIONS!\n\nYou reached 1000 currency!\nYou have achieved the good ending!" :
            "GAME OVER\n\nYou didn't reach the goal of 1000 currency.\nBetter luck next time!";

        // Set text position
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Add background
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(tempCanvas.transform, false);
        backgroundObject.transform.SetAsFirstSibling(); // Put behind text

        var backgroundImage = backgroundObject.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = Color.black;

        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        // Show for 5 seconds
        yield return new WaitForSeconds(5f);

        // Clean up and return to main menu
        Destroy(tempCanvas);

        // Check if main menu scene exists
        bool mainMenuExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName == mainMenuSceneName || sceneName == "MainMenu" || sceneName == "Menu")
            {
                mainMenuSceneName = sceneName;
                mainMenuExists = true;
                break;
            }
        }

        if (mainMenuExists)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main menu scene not found! Cannot return to menu.");
            // Could restart the game or quit application here
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
