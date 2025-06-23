using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; [Header("Managers")]
    public ItemManager itemManager;
    public TileManager tileManager;
    public UI_Manager uiManager;
    public Player player;
    public CurrencyManager currencyManager;
    public ShopManager shopManager;

    [Header("UI Elements - Paused")]
    [SerializeField] GameObject pausedPanel;

    [Header("Player Reference - Scripts")]
    [SerializeField] Movement movement;

    [Header("Save System")]
    public GameSaveData currentSaveData;
    public bool isLoadingFromSave = false;

    private void Awake()
    {
        // Singleton pattern with scene persistence
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Check if we're loading from a save file
        CheckForLoadedSaveData();

        // Ensure ItemManager is assigned (it should persist with GameManager)
        if (itemManager == null)
        {
            itemManager = GetComponent<ItemManager>();
            if (itemManager == null)
            {
                itemManager = FindObjectOfType<ItemManager>();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Delay reference refresh to ensure all objects are initialized
        StartCoroutine(DelayedRefreshManagerReferences());

        // Apply save data if loading from save
        if (isLoadingFromSave && currentSaveData != null)
        {
            StartCoroutine(DelayedApplySaveData(scene.name));
        }
    }

    private IEnumerator DelayedRefreshManagerReferences()
    {
        // Wait a bit to ensure all scene objects are properly instantiated
        yield return new WaitForSeconds(0.2f);
        RefreshManagerReferences();
    }

    public void RefreshManagerReferences()
    {
        Debug.Log("Refreshing GameManager references...");

        // ItemManager should persist with GameManager, but double-check
        if (itemManager == null)
        {
            itemManager = GetComponent<ItemManager>();
            if (itemManager == null)
            {
                itemManager = FindObjectOfType<ItemManager>();
            }

            if (itemManager == null)
            {
                Debug.LogError("ItemManager not found! Make sure it's on the GameManager GameObject or in the scene.");
            }
        }

        // Find managers in the new scene - always refresh these
        tileManager = FindObjectOfType<TileManager>();
        uiManager = FindObjectOfType<UI_Manager>();
        player = FindObjectOfType<Player>();

        // Find paused panel - look for it in the scene, including as child of HUD
        if (pausedPanel == null)
        {
            GameObject pausedPanelObj = GameObject.Find("PausedPanel");
            if (pausedPanelObj == null)
            {
                // Try alternative names
                pausedPanelObj = GameObject.Find("Paused Panel");
                if (pausedPanelObj == null)
                {
                    pausedPanelObj = GameObject.Find("PausePanel");
                }
            }

            // If still not found, search within HUD canvas
            if (pausedPanelObj == null)
            {
                GameObject hudCanvas = GameObject.Find("HUD");
                if (hudCanvas != null)
                {
                    Transform pausedPanelTransform = hudCanvas.transform.Find("PausedPanel");
                    if (pausedPanelTransform == null)
                    {
                        pausedPanelTransform = hudCanvas.transform.Find("Paused Panel");
                        if (pausedPanelTransform == null)
                        {
                            pausedPanelTransform = hudCanvas.transform.Find("PausePanel");
                        }
                    }
                    if (pausedPanelTransform != null)
                    {
                        pausedPanelObj = pausedPanelTransform.gameObject;
                    }
                }
            }

            if (pausedPanelObj != null)
            {
                pausedPanel = pausedPanelObj;
                Debug.Log("Paused panel found and assigned");
            }
            else
            {
                Debug.LogWarning("Paused panel not found in scene! Looking for 'PausedPanel', 'Paused Panel', or 'PausePanel' (also checked under HUD canvas)");
            }
        }

        // Find movement component for pause functionality
        if (movement == null && player != null)
        {
            movement = player.GetComponent<Movement>();
            if (movement == null)
            {
                Debug.LogWarning("Movement component not found on player!");
            }
        }

        // Find CurrencyManager
        if (currencyManager == null)
        {
            currencyManager = FindObjectOfType<CurrencyManager>();
            if (currencyManager == null)
            {
                GameObject currencyObj = new GameObject("CurrencyManager");
                currencyManager = currencyObj.AddComponent<CurrencyManager>();
                Debug.Log("Created new CurrencyManager");
            }
        }

        if (shopManager == null)
        {
            shopManager = FindObjectOfType<ShopManager>();
            if (shopManager == null)
            {
                GameObject shopObj = new GameObject("ShopManager");
                shopManager = shopObj.AddComponent<ShopManager>();
                Debug.Log("Created new ShopManager");
            }
        }

        // Ensure player has stamina manager
        if (player != null)
        {
            StaminaManager staminaManager = player.GetComponent<StaminaManager>();
            if (staminaManager == null)
            {
                staminaManager = player.gameObject.AddComponent<StaminaManager>();
            }
        }

        // Debug log all references
        Debug.Log($"GameManager References - TileManager: {(tileManager != null ? "Found" : "Missing")}, " +
                 $"UIManager: {(uiManager != null ? "Found" : "Missing")}, " +
                 $"Player: {(player != null ? "Found" : "Missing")}, " +
                 $"PausedPanel: {(pausedPanel != null ? "Found" : "Missing")}, " +
                 $"Movement: {(movement != null ? "Found" : "Missing")}");

        // Only refresh UI if all references are available
        if (uiManager != null && player != null)
        {
            // Add a small delay to ensure Player's Start() has been called
            StartCoroutine(DelayedUIRefresh());
        }
    }

    private IEnumerator DelayedUIRefresh()
    {
        // Wait for Player and InventoryManager to be fully initialized
        yield return new WaitForSeconds(0.1f);

        if (uiManager != null)
        {
            uiManager.RefreshAll();
        }
    }

    public void TogglePause()
    {
        if (pausedPanel != null)
        {
            bool isPaused = pausedPanel.activeSelf;
            pausedPanel.SetActive(!isPaused);
            Time.timeScale = isPaused ? 1f : 0f; // Pause or resume time
            movement.enabled = isPaused; // Disable player movement when paused
        }
        else
        {
            Debug.LogWarning("Paused panel not assigned in GameManager!");
        }
    }

    // Save System Methods
    private void CheckForLoadedSaveData()
    {
        Debug.Log("Checking for loaded save data...");

        if (PlayerPrefs.GetInt("IsLoadingGame", 0) == 1)
        {
            string savedDataJson = PlayerPrefs.GetString("LoadedSaveData", "");
            if (!string.IsNullOrEmpty(savedDataJson))
            {
                try
                {
                    currentSaveData = JsonUtility.FromJson<GameSaveData>(savedDataJson);
                    isLoadingFromSave = true;

                    // Clear the temporary data
                    PlayerPrefs.DeleteKey("LoadedSaveData");
                    PlayerPrefs.DeleteKey("IsLoadingGame");

                    Debug.Log($"Loading game from PlayerPrefs save data - Day {currentSaveData.currentDay}");
                    return;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse save data from PlayerPrefs: {e.Message}");
                }
            }
        }

        // Check if there's an existing save file we should load
        if (SaveSystem.SaveExists())
        {
            Debug.Log("Found existing save file, loading it automatically");
            currentSaveData = SaveSystem.LoadGame();
            if (currentSaveData != null)
            {
                isLoadingFromSave = true;
                Debug.Log($"Loaded save data from file - Day {currentSaveData.currentDay}, Scene: {currentSaveData.currentScene}");
            }
            else
            {
                Debug.LogError("Failed to load save data from file!");
                currentSaveData = new GameSaveData();
                isLoadingFromSave = false;
            }
        }
        else
        {
            // If not loading from save, create new save data
            currentSaveData = new GameSaveData();
            Debug.Log("No save file found, creating new save data");
            isLoadingFromSave = false;
        }
    }

    private IEnumerator DelayedApplySaveData(string sceneName)
    {
        // Wait for all managers to be properly initialized
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"Applying save data to scene: {sceneName}");

        // Force TileManager to reload with our save data
        if (tileManager != null && currentSaveData != null)
        {
            Debug.Log("Forcing TileManager to use new save data...");
            // The TileManager should now pick up the save data from GameManager.currentSaveData
            // Force it to reload by calling LoadTileDataForCurrentScene again
            yield return new WaitForSeconds(0.1f);
            tileManager.LoadTileDataForCurrentScene();
        }        // Apply inventory data if it exists
        if (currentSaveData.GetInventoryDataForScene(sceneName) != null && player != null)
        {
            // Apply inventory data through Player
            // You'll need to add a LoadInventoryData method to your Player
            Debug.Log("Inventory data found for scene, applying...");
            // player.LoadInventoryData(currentSaveData.GetInventoryDataForScene(sceneName));
        }

        // Apply currency data
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.SetCurrency(currentSaveData.playerCurrency);
            Debug.Log($"Currency loaded: {currentSaveData.playerCurrency}");
        }

        // Set current day (you might need to implement day system)
        Debug.Log($"Current day set to: {currentSaveData.currentDay}");

        isLoadingFromSave = false; // Reset after applying once
    }

    public void SaveGame()
    {
        if (currentSaveData == null)
        {
            currentSaveData = new GameSaveData();
        }

        // Update current save data with current game state
        UpdateSaveDataFromCurrentState();

        // Save to file
        SaveSystem.SaveGame(currentSaveData);
        Debug.Log($"Game saved successfully - Day: {currentSaveData.currentDay}, Scene: {currentSaveData.currentScene}");

        // Verify the save worked
        if (SaveSystem.SaveExists())
        {
            Debug.Log("Save file verified to exist after saving");
        }
        else
        {
            Debug.LogError("Save file does not exist after saving! Something went wrong.");
        }
    }

    private void UpdateSaveDataFromCurrentState()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Update current scene and save time
        if (currentSaveData != null)
        {
            currentSaveData.currentScene = currentScene;
            currentSaveData.saveTime = System.DateTime.Now;
            Debug.Log($"Save data updated for scene: {currentScene}");
        }

        // Update tile data for current scene
        if (tileManager != null)
        {
            // Trigger TileManager to save its data (which will also update our save data)
            tileManager.SaveTileDataForCurrentScene();
            Debug.Log("TileManager data updated for current scene");
        }

        // Update inventory data
        if (player != null)
        {
            // Get current inventory data from Player
            // You'll need to add a GetInventoryData method to your Player
            Debug.Log("Updating inventory data");
            // currentSaveData.inventoryData[currentScene] = player.GetInventoryData();
        }

        // Save currency data
        if (CurrencyManager.instance != null)
        {
            currentSaveData.playerCurrency = CurrencyManager.instance.GetCurrentCurrency();
            Debug.Log($"Currency saved: {currentSaveData.playerCurrency}");
        }

        // Update current day if you have a day system
        // currentSaveData.currentDay = GetCurrentDay();
    }

    public void OnPlayerSleep()
    {
        // Call this method when the player goes to sleep (end of day)
        if (currentSaveData != null)
        {
            currentSaveData.currentDay++;
        }
        SaveGame();
        Debug.Log("Game saved on sleep, advancing to next day");
    }

    public void ReturnToMainMenu()
    {
        // Save current progress before returning to menu
        SaveGame();

        // Load main menu scene
        SceneManager.LoadScene("MainMenu"); // Adjust scene name as needed
    }

    // Public method to force apply current save data (useful for debugging)
    [ContextMenu("Force Apply Save Data")]
    public void ForceApplySaveData()
    {
        if (currentSaveData != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            StartCoroutine(DelayedApplySaveData(currentScene));
            Debug.Log("Forcing save data application...");
        }
        else
        {
            Debug.LogWarning("No save data to apply!");
        }
    }

    // Public method to force refresh references (useful for debugging)
    [ContextMenu("Force Refresh References")]
    public void ForceRefreshReferences()
    {
        RefreshManagerReferences();
    }

    public void OnClick_Continue()
    {
    if (GameManager.instance != null)
    {
        GameManager.instance.TogglePause();
    }
    }

    public void OnClick_Exit()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.ReturnToMainMenu();
        }
    }
}