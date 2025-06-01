using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Managers")]
    public ItemManager itemManager;
    public TileManager tileManager;
    public UI_Manager uiManager;
    public Player player;

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
    }

    private IEnumerator DelayedRefreshManagerReferences()
    {
        // Wait a bit to ensure all scene objects are properly instantiated
        yield return new WaitForSeconds(0.2f);
        RefreshManagerReferences();
    }

    public void RefreshManagerReferences()
    {
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

        // Find managers in the new scene
        if (tileManager == null)
            tileManager = FindObjectOfType<TileManager>();

        if (uiManager == null)
            uiManager = FindObjectOfType<UI_Manager>();

        if (player == null)
            player = FindObjectOfType<Player>();

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
}