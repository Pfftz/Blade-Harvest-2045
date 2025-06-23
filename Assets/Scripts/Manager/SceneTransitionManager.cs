using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance; [Header("Transition Settings")]
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;

    [Header("Sleep Transition")]
    [SerializeField] private VideoClip sleepTransitionVideo;

    [Header("Restaurant Transition")]
    [SerializeField] private VideoClip restaurantTransitionVideo; [Header("Day Management")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 7;

    // Store names to find references in new scenes
    private string transitionCanvasName = "Transition_Canvas";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure this GameObject stays active across scenes
        gameObject.SetActive(true);

        // Store the names of UI objects for later reference finding
        if (transitionCanvas != null)
            transitionCanvasName = transitionCanvas.name;

        // Setup video player if video is assigned
        if (sleepTransitionVideo != null)
        {
            SetupVideoPlayer();
        }

        // Ensure transition canvas is initially hidden
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);

        // Sync with GameManager if available
        SyncWithGameManager();
    }

    /// <summary>
    /// Sync current day with GameManager's save data
    /// </summary>
    private void SyncWithGameManager()
    {
        if (GameManager.instance != null && GameManager.instance.currentSaveData != null)
        {
            currentDay = GameManager.instance.currentSaveData.currentDay;
            Debug.Log($"SceneTransitionManager synced with GameManager - Current day: {currentDay}");
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
        // Sync with GameManager when scene loads
        SyncWithGameManager();

        // Re-find UI references in the new scene
        StartCoroutine(RefreshUIReferences(scene.name));
    }
    private IEnumerator RefreshUIReferences(string sceneName = "")
    {
        // Wait a frame to ensure all scene objects are loaded
        yield return null;

        // Check if this scene requires a transition canvas
        if (IsSceneRequiringTransitionCanvas(sceneName))
        {
            // Find transition canvas by name (including inactive objects)
            if (transitionCanvas == null)
            {
                // Search for the canvas by name first
                GameObject foundCanvas = GameObject.Find(transitionCanvasName);

                if (foundCanvas == null)
                {
                    // Search through all inactive objects too
                    Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                    foreach (Transform t in allTransforms)
                    {
                        if (t.gameObject.scene.IsValid() && // Only scene objects, not prefabs
                            (t.name == "Transition_Canvas" ||
                             t.name == "TransitionCanvas" ||
                             t.name == "Transition Canvas"))
                        {
                            foundCanvas = t.gameObject;
                            break;
                        }
                    }
                }

                if (foundCanvas != null)
                {
                    transitionCanvas = foundCanvas;
                    Debug.Log($"Found transition canvas: {transitionCanvas.name}");
                }
                else
                {
                    Debug.LogError("Transition canvas not found in scene! Make sure 'Transition_Canvas' exists in the scene.");
                    yield break;
                }
            }            // Find video display within the transition canvas
            if (transitionCanvas != null)
            {
                // Temporarily activate the canvas to search for children
                bool wasActive = transitionCanvas.activeSelf;
                transitionCanvas.SetActive(true);

                // Find video display
                if (videoDisplay == null)
                {
                    RawImage[] rawImages = transitionCanvas.GetComponentsInChildren<RawImage>(true);
                    foreach (RawImage rawImg in rawImages)
                    {
                        if (rawImg.name.ToLower().Contains("video") ||
                            rawImg.name.ToLower().Contains("display"))
                        {
                            videoDisplay = rawImg;
                            Debug.Log($"Found video display: {videoDisplay.name}");
                            break;
                        }
                    }

                    // If not found by name, use the first RawImage
                    if (videoDisplay == null && rawImages.Length > 0)
                    {
                        videoDisplay = rawImages[0];
                        Debug.Log($"Using first RawImage as video display: {videoDisplay.name}");
                    }

                    if (videoDisplay == null)
                    {
                        Debug.LogWarning("Video display not found in transition canvas!");
                    }
                }

                // Restore the original active state
                transitionCanvas.SetActive(wasActive);
            }

            // Re-setup video player with found references
            if (sleepTransitionVideo != null)
            {
                SetupVideoPlayer();
            }
        }
        else
        {
            // Scene doesn't require transition canvas, skip setup
            Debug.Log($"Skipping transition canvas setup for scene: {sceneName}");
        }
    }

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Configure video player settings for better performance
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true; // Allow frame skipping to prevent lag
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // Audio settings
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioMute(0, false);

        // Create render texture if needed
        if (videoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.useMipMap = false; // Disable mipmaps for better performance
            videoPlayer.targetTexture = renderTexture;
        }

        // Assign render texture to video display if available
        if (videoDisplay != null && videoPlayer.targetTexture != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
            Debug.Log("Video player setup complete with video display");
        }
        else
        {
            Debug.LogWarning("Video display not available for video player setup");
        }
    }

    private bool IsSceneRequiringTransitionCanvas(string sceneName)
    {
        // Scenes that don't require transition canvas
        if (string.IsNullOrEmpty(sceneName))
            return true; // Default to requiring canvas if scene name is unknown

        // Convert to lowercase for easier comparison
        string lowerSceneName = sceneName.ToLower();        // Scenes that typically don't need transition canvas
        if (lowerSceneName.Contains("mainmenu") ||
            lowerSceneName.Contains("main_menu") ||
            lowerSceneName.Contains("menu") ||
            lowerSceneName.Contains("goodending") ||
            lowerSceneName.Contains("badending") ||
            lowerSceneName.Contains("ending") ||
            lowerSceneName.Contains("introscene") ||
            lowerSceneName.Contains("intro") ||
            lowerSceneName.Contains("credits"))
        {
            Debug.Log($"Scene '{sceneName}' does not require transition canvas");
            return false;
        }

        // Day scenes and other gameplay scenes typically need transition canvas
        return true;
    }

    public void StartSleepTransition()
    {
        Debug.Log("StartSleepTransition called");

        // Play sleep sound effect
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySleep();
        }

        // Ensure this GameObject is active before starting coroutines
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("SceneTransitionManager GameObject was inactive! Reactivating...");
            gameObject.SetActive(true);
        }

        // Ensure we have valid references before starting transition
        if (transitionCanvas == null)
        {
            Debug.LogError("Cannot start transition - transition canvas is missing! Refreshing references...");
            StartCoroutine(RefreshUIReferencesAndStartTransition());
            return;
        }// Check if player qualifies for restaurant transition
        bool useRestaurantTransition = false;
        GameObject restaurantManagerGO = GameObject.Find("RestaurantManager");
        if (restaurantManagerGO != null)
        {
            var restaurantManager = restaurantManagerGO.GetComponent<MonoBehaviour>();
            if (restaurantManager != null)
            {
                try
                {
                    useRestaurantTransition = (bool)restaurantManager.GetType().GetMethod("QualifiesForRestaurantTransition").Invoke(restaurantManager, null);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to call QualifiesForRestaurantTransition: {e.Message}");
                }
            }
            Debug.Log($"Restaurant transition qualified: {useRestaurantTransition}");
        }

        StartCoroutine(SleepTransitionCoroutine(useRestaurantTransition));
    }
    private IEnumerator DelayedStartTransition()
    {
        // Wait for UI references to be refreshed
        yield return new WaitForSeconds(0.5f);

        if (transitionCanvas != null)
        {            // Check restaurant transition here too
            bool useRestaurantTransition = false;
            GameObject restaurantManagerGO = GameObject.Find("RestaurantManager");
            if (restaurantManagerGO != null)
            {
                var restaurantManager = restaurantManagerGO.GetComponent<MonoBehaviour>();
                if (restaurantManager != null)
                {
                    try
                    {
                        useRestaurantTransition = (bool)restaurantManager.GetType().GetMethod("QualifiesForRestaurantTransition").Invoke(restaurantManager, null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Failed to call QualifiesForRestaurantTransition: {e.Message}");
                    }
                }
            }
            StartCoroutine(SleepTransitionCoroutine(useRestaurantTransition));
        }
        else
        {
            Debug.LogError("Still no transition canvas found after refresh!");
        }
    }
    private IEnumerator SleepTransitionCoroutine(bool useRestaurantTransition = false)
    {
        Debug.Log("Starting sleep transition...");

        // Activate transition canvas
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(true);
            Debug.Log("Transition canvas activated");
        }
        else
        {
            Debug.LogError("Transition canvas is null! Cannot proceed with transition.");
            // Still proceed with scene loading as fallback
            LoadNextDayScene();
            yield break;
        }

        // Disable player movement
        DisablePlayerMovement();

        // Choose video based on transition type
        VideoClip videoToPlay = useRestaurantTransition && restaurantTransitionVideo != null ? restaurantTransitionVideo : sleepTransitionVideo;

        if (videoToPlay != null && videoPlayer != null && videoDisplay != null)
        {
            string transitionType = useRestaurantTransition ? "restaurant video" : "sleep video";
            Debug.Log($"Playing {transitionType} transition");
            yield return StartCoroutine(PlayVideoTransition(videoToPlay));
        }
        else
        {
            Debug.LogWarning("No video transition available! Proceeding with direct scene load.");
            yield return new WaitForSeconds(1f); // Brief pause for user experience
        }

        Debug.Log("Transition complete, loading next day scene...");

        // Load next day scene immediately
        LoadNextDayScene();
    }

    private IEnumerator PlayVideoTransition(VideoClip videoClip)
    {
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            Debug.Log("Video display activated");
        }
        else
        {
            Debug.LogError("Video display is null! Cannot play video transition.");
            yield break;
        }        // Set the video clip
        videoPlayer.clip = videoClip;

        // Prepare the video
        videoPlayer.Prepare();

        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            Debug.Log("Preparing video...");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Video prepared. Duration: {videoPlayer.clip.length} seconds");

        // Play the video
        videoPlayer.Play();

        // Wait for video to actually start playing
        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }

        Debug.Log("Video started playing");

        // Better video completion detection
        float videoDuration = (float)videoPlayer.clip.length;
        float startTime = Time.time;

        // Wait for video to complete using multiple checks
        while (videoPlayer.isPlaying && (Time.time - startTime) < videoDuration + 0.5f)
        {
            yield return null;
        }

        // Additional check: wait until we're near the end of the video
        while (videoPlayer.time < videoDuration - 0.1f && videoPlayer.isPlaying)
        {
            yield return null;
        }

        Debug.Log("Video finished playing");        // Stop and hide video
        videoPlayer.Stop();
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
    }

    private void LoadNextDayScene()
    {
        Debug.Log("=== STARTING NEXT DAY SCENE LOAD ===");
        StartCoroutine(SaveAndLoadNextDay());
    }
    private IEnumerator SaveAndLoadNextDay()
    {
        Debug.Log("=== SAVING DATA BEFORE SCENE TRANSITION ===");        // Reset hasBeenSaved flag to ensure tile data can be saved
        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager != null)
        {
            // Force reset the saved flag to ensure we can save again
            tileManager.ResetSaveFlag();
            Debug.Log("Reset hasBeenSaved flag to ensure tile data can be saved");
        }        // Force save tile data BEFORE any scene operations
        SaveCurrentSceneData();

        // Debug: Check what was actually saved
        TileManager debugTileManager = FindObjectOfType<TileManager>();
        if (debugTileManager != null)
        {
            debugTileManager.DebugCurrentSaveData();
        }

        // Wait a bit to ensure save operations complete
        yield return new WaitForSeconds(0.5f);        // Verify the save was written
        if (GameManager.instance?.currentSaveData != null)
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (GameManager.instance.currentSaveData.HasTileDataForScene(currentScene))
            {
                var tileData = GameManager.instance.currentSaveData.GetTileDataForScene(currentScene);
                int harvestCount = 0;
                foreach (var tile in tileData.tiles)
                {
                    if (tile.tileName.Contains("Harvest"))
                    {
                        harvestCount++;
                    }
                }
                Debug.Log($"VERIFICATION: Save data contains {harvestCount} harvest tiles for {currentScene}");
            }
            else
            {
                Debug.LogWarning($"No tile data found for current scene {currentScene} in save data!");
            }
        }        // Don't increment currentDay here - let GameManager handle day progression
                 // currentDay++; 

        // Reset restaurant tracking for new day
        GameObject restaurantManagerGO = GameObject.Find("RestaurantManager");
        if (restaurantManagerGO != null)
        {
            var restaurantManager = restaurantManagerGO.GetComponent<MonoBehaviour>();
            if (restaurantManager != null)
            {
                restaurantManager.SendMessage("StartNewDay", SendMessageOptions.DontRequireReceiver);
            }
        }        // Sync with GameManager's save data to ensure consistency
        if (GameManager.instance != null && GameManager.instance.currentSaveData != null)
        {
            // Use GameManager's current day as the authoritative source
            int oldDay = currentDay;
            currentDay = GameManager.instance.currentSaveData.currentDay;
            Debug.Log($"Day sync - Old: {oldDay}, New: {currentDay}, GameManager save data: {GameManager.instance.currentSaveData.currentDay}");
        }
        else
        {
            Debug.LogWarning("Cannot sync with GameManager - GameManager or currentSaveData is null!");
        }if (currentDay > maxDays)
        {
            Debug.Log($"Game completed! Current day ({currentDay}) > max days ({maxDays}). Checking ending...");
            // Check for game ending with EndingManager
            GameObject endingManagerGO = GameObject.Find("EndingManager");
            if (endingManagerGO != null)
            {
                Debug.Log("EndingManager found! Triggering game ending...");
                var endingManager = endingManagerGO.GetComponent<MonoBehaviour>();
                if (endingManager != null)
                {
                    endingManager.SendMessage("CheckForGameEnding", currentDay, SendMessageOptions.DontRequireReceiver);
                    yield break; // Stop here, don't load next day scene
                }
                else
                {
                    Debug.LogError("EndingManager GameObject found but has no MonoBehaviour component!");
                }
            }
            else
            {
                Debug.LogWarning("EndingManager not found! Loading main menu as fallback.");
                SceneManager.LoadScene("MainMenu"); // Load main menu by name instead of index
                yield break; // Stop here
            }
        }        else
        {
            // Safety check - if we somehow get here with currentDay > maxDays, force end the game
            if (currentDay > maxDays)
            {
                Debug.LogError($"SAFETY CHECK: Current day ({currentDay}) exceeds max days ({maxDays}) but we're in the else branch! Force ending game.");
                SceneManager.LoadScene("MainMenu");
                yield break;
            }
            
            string nextSceneName = "Day" + currentDay;
            Debug.Log($"Loading scene: {nextSceneName} (Current day: {currentDay}, Max days: {maxDays})");
            yield return StartCoroutine(LoadSceneAsync(nextSceneName));
        }
    }// Add this method to ensure data is saved
    private void SaveCurrentSceneData()
    {
        try
        {
            Debug.Log("=== SAVING ALL GAME DATA BEFORE SCENE TRANSITION ===");

            // Save tile data FIRST with extra safety checks
            TileManager tileManager = FindObjectOfType<TileManager>();
            if (tileManager != null)
            {
                Debug.Log("Found TileManager, saving tile data...");
                tileManager.SaveTileDataForCurrentScene();
            }
            else if (GameManager.instance?.tileManager != null)
            {
                Debug.Log("Using GameManager TileManager reference...");
                GameManager.instance.tileManager.SaveTileDataForCurrentScene();
            }
            else
            {
                Debug.LogWarning("No TileManager found - tile data will not be saved!");
            }

            // Save player inventory
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                Debug.Log("Found Player, saving inventory data...");
                player.SaveInventoryData();
            }
            else if (GameManager.instance?.player != null)
            {
                Debug.Log("Using GameManager Player reference...");
                GameManager.instance.player.SaveInventoryData();
            }
            else
            {
                Debug.LogWarning("No Player found - inventory data will not be saved!");
            }

            // Save currency data explicitly
            if (CurrencyManager.instance != null && GameManager.instance != null)
            {
                int currentCurrency = CurrencyManager.instance.GetCurrentCurrency();
                GameManager.instance.currentSaveData.playerCurrency = currentCurrency;
                Debug.Log($"Saved currency amount: {currentCurrency}");
            }

            // CRITICAL: Save the entire game state to file through GameManager
            if (GameManager.instance != null)
            {
                Debug.Log("Saving complete game state to file...");
                GameManager.instance.SaveGame();
                Debug.Log("Game state saved to file successfully!");
            }
            else
            {
                Debug.LogError("GameManager.instance is null - cannot save game to file!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving scene data: {e.Message}");
        }
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
#if UNITY_EDITOR
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                sceneExists = true;
                break;
            }
#else
            // In build, just try to load and handle the exception
            sceneExists = true;
            break;
#endif
        }

        if (!sceneExists)
        {
            Debug.LogError($"Scene '{sceneName}' not found in build settings!");
            yield break;
        }

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait for the scene to be 90% loaded
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("Scene loaded, activating...");

        // Activate the scene
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to be fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Update the save data with the new scene name after successful load
        if (GameManager.instance != null && GameManager.instance.currentSaveData != null)
        {
            GameManager.instance.currentSaveData.currentScene = sceneName;
            Debug.Log($"Updated save data currentScene to: {sceneName}");
        }
    }

    private void DisablePlayerMovement()
    {
        // Disable player movement during transition
        Movement playerMovement = FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Also disable player script
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        // Re-enable player movement after transition
        Movement playerMovement = FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Also enable player script
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = true;
        }
    }
    private void Start()
    {
        EnablePlayerMovement();

        // Delay GameManager reference refresh to avoid null reference errors
        StartCoroutine(DelayedGameManagerRefresh());
    }

    private IEnumerator DelayedGameManagerRefresh()
    {
        // Wait to ensure all objects are properly initialized
        yield return new WaitForSeconds(0.3f);

        // Refresh GameManager references after scene load
        if (GameManager.instance != null)
        {
            GameManager.instance.RefreshManagerReferences();
        }
    }

    // Public methods for external use
    public int GetCurrentDay() => currentDay;
    public void SetCurrentDay(int day) => currentDay = day;    /// <summary>
                                                               /// Reset game state for new game - call this when starting a new game
                                                               /// </summary>
    public void ResetGameState()
    {
        Debug.Log("=== RESETTING GAME STATE FOR NEW GAME ===");
        currentDay = 1;

        // Clear any cached UI references so they get refreshed for the new game
        transitionCanvas = null;
        videoDisplay = null;

        // Reset any other persistent state if needed
        Debug.Log($"Game state reset - Current day set to: {currentDay}");
    }    /// <summary>
         /// Initialize game state from save data - call this when loading a game
         /// </summary>
    public void InitializeFromSaveData(GameSaveData saveData)
    {
        if (saveData != null)
        {
            currentDay = saveData.currentDay;
            Debug.Log($"SceneTransitionManager initialized from save data - Current day: {currentDay}");
        }
        else
        {
            // Fallback to sync with GameManager
            SyncWithGameManager();
        }
    }

    /// <summary>
    /// Force refresh of UI references - useful for debugging or when canvas is lost
    /// </summary>
    [ContextMenu("Force Refresh UI References")]
    public void ForceRefreshUIReferences()
    {
        Debug.Log("Forcing refresh of UI references...");
        transitionCanvas = null;
        videoDisplay = null;
        StartCoroutine(RefreshUIReferences());
    }

    private IEnumerator RefreshUIReferencesAndStartTransition()
    {
        yield return StartCoroutine(RefreshUIReferences());

        // Try starting transition again after refresh
        if (transitionCanvas != null)
        {
            StartSleepTransition();
        }
        else
        {
            Debug.LogError("Failed to find transition canvas after refresh!");
        }
    }
}
