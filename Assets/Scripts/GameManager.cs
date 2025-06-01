using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public ItemManager itemManager;
    public TileManager tileManager;
    public UI_Manager uiManager;
    public Player player;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Initial setup
        RefreshReferences();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"GameManager: Scene {scene.name} loaded, refreshing references");
        StartCoroutine(DelayedRefreshReferences());
    }

    private IEnumerator DelayedRefreshReferences()
    {
        // Wait a frame for everything to initialize
        yield return null;
        RefreshReferences();
    }

    private void RefreshReferences()
    {
        // ItemManager stays with GameManager (DontDestroyOnLoad)
        if (itemManager == null)
        {
            itemManager = GetComponent<ItemManager>();
        }

        // Find scene-specific components
        tileManager = FindObjectOfType<TileManager>();
        uiManager = FindObjectOfType<UI_Manager>();
        player = FindObjectOfType<Player>();

        // Debug log to check what was found
        Debug.Log($"GameManager References Refreshed:");
        Debug.Log($"- ItemManager: {(itemManager != null ? "Found" : "Missing")}");
        Debug.Log($"- TileManager: {(tileManager != null ? "Found" : "Missing")}");
        Debug.Log($"- UI_Manager: {(uiManager != null ? "Found" : "Missing")}");
        Debug.Log($"- Player: {(player != null ? "Found" : "Missing")}");

        // Check TileManager's interactableMap specifically
        if (tileManager != null && tileManager.interactableMap != null)
        {
            Debug.Log("- TileManager.interactableMap: Found");
        }
        else if (tileManager != null)
        {
            Debug.LogError("- TileManager.interactableMap: Missing! Please assign it in the inspector.");
        }
    }

    private void Start()
    {
        // Ensure SceneTransitionManager exists
        if (SceneTransitionManager.Instance == null)
        {
            GameObject transitionManager = new GameObject("SceneTransitionManager");
            transitionManager.AddComponent<SceneTransitionManager>();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Public method to manually refresh references if needed
    public void ForceRefreshReferences()
    {
        RefreshReferences();
    }
}
