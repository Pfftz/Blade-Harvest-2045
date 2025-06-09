using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[System.Serializable]
public class TileData
{
    public Vector3Int position;
    public string plantedSeed;
    public int growthPhase;
    public string tileName;
}

[System.Serializable]
public class SceneTileData
{
    public string sceneName;
    public List<TileData> tiles = new List<TileData>();
}

public class TileManager : MonoBehaviour
{
    public Tilemap interactableMap;
    public Tile hiddenInteratableTile;
    public Tile plowedTile;

    // Different seeding tiles for each plant type
    [Header("Tomato Growth Phases")]
    public Tile tomatoSeedingTile;
    public Tile tomatoPhase1Tile;
    public Tile tomatoPhase2Tile;
    public Tile tomatoPhase3Tile;
    public Tile tomatoPhase4Tile;
    public Tile tomatoHarvestTile;

    [Header("Rice Growth Phases")]
    public Tile riceSeedingTile;
    public Tile ricePhase1Tile;
    public Tile ricePhase2Tile;
    public Tile ricePhase3Tile;
    public Tile ricePhase4Tile;
    public Tile riceHarvestTile;

    [Header("Cucumber Growth Phases")]
    public Tile cucumberSeedingTile;
    public Tile cucumberPhase1Tile;
    public Tile cucumberPhase2Tile;
    public Tile cucumberPhase3Tile;
    public Tile cucumberPhase4Tile;
    public Tile cucumberHarvestTile;

    [Header("Cabbage Growth Phases")]
    public Tile cabbageSeedingTile;
    public Tile cabbagePhase1Tile;
    public Tile cabbagePhase2Tile;
    public Tile cabbagePhase3Tile;
    public Tile cabbagePhase4Tile;
    public Tile cabbageHarvestTile;

    // Add references to crop prefabs
    [Header("Crop Collectible Prefabs")]
    public GameObject tomatoCollectablePrefab;
    public GameObject riceCollectablePrefab;
    public GameObject cucumberCollectablePrefab;
    public GameObject cabbageCollectablePrefab;

    // Dictionary to track what seed was planted at each position
    private Dictionary<Vector3Int, string> plantedSeeds = new Dictionary<Vector3Int, string>();
    // Dictionary to track growth phase of each plant
    private Dictionary<Vector3Int, int> plantGrowthPhase = new Dictionary<Vector3Int, int>();

    // Static data to persist between scenes
    private static Dictionary<string, SceneTileData> savedTileDataByScene = new Dictionary<string, SceneTileData>();

    // Add flag to prevent multiple saves
    private bool hasBeenSaved = false;

    void Start()
    {
        // Try to find interactableMap if not assigned
        if (interactableMap == null)
        {
            // Look for Grid parent and find Interactable Map child
            Grid grid = FindObjectOfType<Grid>();
            if (grid != null)
            {
                Transform interactableMapTransform = grid.transform.Find("Interactable Map");
                if (interactableMapTransform != null)
                {
                    interactableMap = interactableMapTransform.GetComponent<Tilemap>();
                    Debug.Log("Found Interactable Map in Grid");
                }
            }
            
            // If still not found, try to find by name
            if (interactableMap == null)
            {
                GameObject interactableMapObj = GameObject.Find("Interactable Map");
                if (interactableMapObj != null)
                {
                    interactableMap = interactableMapObj.GetComponent<Tilemap>();
                    Debug.Log("Found Interactable Map by name");
                }
            }
            
            // Last resort: find any Tilemap with "Interactable" in the name
            if (interactableMap == null)
            {
                Tilemap[] tilemaps = FindObjectsOfType<Tilemap>();
                foreach (Tilemap tilemap in tilemaps)
                {
                    if (tilemap.name.Contains("Interactable"))
                    {
                        interactableMap = tilemap;
                        Debug.Log($"Found tilemap by search: {tilemap.name}");
                        break;
                    }
                }
            }
        }

        if (interactableMap == null)
        {
            Debug.LogError("Interactable Map not found! Make sure it exists as a child of Grid and is named 'Interactable Map'");
            return;
        }

        // Initialize hidden tiles
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);

            if (tile != null && tile.name == "Interactable_Visible")
            {
                interactableMap.SetTile(position, hiddenInteratableTile);
            }
        }

        // Load tile data for current scene
        LoadTileDataForCurrentScene();
        
        // Register with GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.tileManager = this;
        }
    }

    private void OnDestroy()
    {
        // Only save if we haven't saved yet and interactableMap is still valid
        if (!hasBeenSaved && interactableMap != null)
        {
            SaveTileDataForCurrentScene();
        }
    }

    // Add this method to be called before scene changes
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !hasBeenSaved)
        {
            SaveTileDataForCurrentScene();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !hasBeenSaved)
        {
            SaveTileDataForCurrentScene();
        }
    }

    public void SetInteracted(Vector3Int position)
    {
        interactableMap.SetTile(position, plowedTile);
    }

    public void SetSeeding(Vector3Int position, string seedType)
    {
        if (!string.IsNullOrEmpty(seedType))
        {
            plantedSeeds[position] = seedType;
            plantGrowthPhase[position] = 0; // Start at phase 0 (seeding)

            // Set appropriate seeding tile based on plant type
            Tile seedingTile = GetSeedingTileForPlant(seedType);
            if (seedingTile != null)
            {
                interactableMap.SetTile(position, seedingTile);
            }
        }
    }

    public void SetHarvesting(Vector3Int position)
    {
        // Get what was planted here before clearing data
        string plantType = GetPlantedSeed(position);

        // Spawn the appropriate crop collectable
        SpawnCropCollectable(position, plantType);

        // Set tile to harvesting state (or back to plowed)
        interactableMap.SetTile(position, plowedTile);

        // Clear plant data when harvested
        plantedSeeds.Remove(position);
        plantGrowthPhase.Remove(position);
    }

    // New method to spawn crop collectables
    private void SpawnCropCollectable(Vector3Int position, string plantType)
    {
        GameObject prefabToSpawn = GetCropPrefab(plantType);

        if (prefabToSpawn != null)
        {
            // Convert tile position to world position
            Vector3 worldPosition = interactableMap.CellToWorld(position);

            // Add small random offset like the item drop system
            Vector3 spawnOffset = Random.insideUnitCircle * 0.5f;
            Vector3 spawnPosition = worldPosition + spawnOffset;

            // Spawn the crop collectable
            GameObject spawnedCrop = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            // Add force to make it move a bit (similar to dropped items)
            Rigidbody2D cropRb = spawnedCrop.GetComponent<Rigidbody2D>();
            if (cropRb != null)
            {
                cropRb.AddForce(spawnOffset * 2f, ForceMode2D.Impulse);
            }

            Debug.Log($"Spawned {plantType} crop at {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"No prefab found for plant type: {plantType}");
        }
    }

    // Get the appropriate crop prefab based on plant type
    private GameObject GetCropPrefab(string plantType)
    {
        switch (plantType)
        {
            case "TomatoSeed": return tomatoCollectablePrefab;
            case "RiceSeed": return riceCollectablePrefab;
            case "CucumberSeed": return cucumberCollectablePrefab;
            case "CabbageSeed": return cabbageCollectablePrefab;
            default: return null;
        }
    }

    // New: Shovel functionality - reset tiles
    public void SetShoveling(Vector3Int position)
    {
        string tileName = GetTileName(position);

        // If there's a plant (any growth phase), return to plowed
        if (HasPlantAtPosition(position))
        {
            interactableMap.SetTile(position, plowedTile);
            // Clear plant data
            plantedSeeds.Remove(position);
            plantGrowthPhase.Remove(position);
        }
        // If it's already plowed, return to normal interactable tile
        else if (tileName == "Plowed")
        {
            interactableMap.SetTile(position, hiddenInteratableTile);
        }
    }

    // Helper method to check if there's a plant at a position
    public bool HasPlantAtPosition(Vector3Int position)
    {
        return plantedSeeds.ContainsKey(position);
    }

    // New method to grow plants with Gro-Quick Light
    public bool GrowPlant(Vector3Int position)
    {
        if (plantedSeeds.ContainsKey(position) && plantGrowthPhase.ContainsKey(position))
        {
            string plantType = plantedSeeds[position];
            int currentPhase = plantGrowthPhase[position];

            // If not at max growth phase (5), grow the plant
            if (currentPhase < 5)
            {
                plantGrowthPhase[position] = currentPhase + 1;
                UpdatePlantTile(position, plantType, plantGrowthPhase[position]);
                return true;
            }
        }
        return false;
    }

    private void UpdatePlantTile(Vector3Int position, string plantType, int phase)
    {
        Tile tileToSet = GetPlantTileForPhase(plantType, phase);
        if (tileToSet != null)
        {
            interactableMap.SetTile(position, tileToSet);
        }
    }

    private Tile GetSeedingTileForPlant(string seedType)
    {
        switch (seedType)
        {
            case "TomatoSeed": return tomatoSeedingTile;
            case "RiceSeed": return riceSeedingTile;
            case "CucumberSeed": return cucumberSeedingTile;
            case "CabbageSeed": return cabbageSeedingTile;
            default: return null;
        }
    }

    private Tile GetPlantTileForPhase(string plantType, int phase)
    {
        switch (plantType)
        {
            case "TomatoSeed":
                switch (phase)
                {
                    case 0: return tomatoSeedingTile;
                    case 1: return tomatoPhase1Tile;
                    case 2: return tomatoPhase2Tile;
                    case 3: return tomatoPhase3Tile;
                    case 4: return tomatoPhase4Tile;
                    case 5: return tomatoHarvestTile;
                }
                break;
            case "RiceSeed":
                switch (phase)
                {
                    case 0: return riceSeedingTile;
                    case 1: return ricePhase1Tile;
                    case 2: return ricePhase2Tile;
                    case 3: return ricePhase3Tile;
                    case 4: return ricePhase4Tile;
                    case 5: return riceHarvestTile;
                }
                break;
            case "CucumberSeed":
                switch (phase)
                {
                    case 0: return cucumberSeedingTile;
                    case 1: return cucumberPhase1Tile;
                    case 2: return cucumberPhase2Tile;
                    case 3: return cucumberPhase3Tile;
                    case 4: return cucumberPhase4Tile;
                    case 5: return cucumberHarvestTile;
                }
                break;
            case "CabbageSeed":
                switch (phase)
                {
                    case 0: return cabbageSeedingTile;
                    case 1: return cabbagePhase1Tile;
                    case 2: return cabbagePhase2Tile;
                    case 3: return cabbagePhase3Tile;
                    case 4: return cabbagePhase4Tile;
                    case 5: return cabbageHarvestTile;
                }
                break;
        }
        return null;
    }

    public string GetTileName(Vector3Int position)
    {
        if (interactableMap != null)
        {
            TileBase tile = interactableMap.GetTile(position);
            if (tile != null)
            {
                return tile.name;
            }
        }
        return "";
    }

    public string GetPlantedSeed(Vector3Int position)
    {
        return plantedSeeds.ContainsKey(position) ? plantedSeeds[position] : "";
    }

    public int GetPlantGrowthPhase(Vector3Int position)
    {
        return plantGrowthPhase.ContainsKey(position) ? plantGrowthPhase[position] : -1;
    }

    public bool IsPlantFullyGrown(Vector3Int position)
    {
        return GetPlantGrowthPhase(position) == 5;
    }

    // Add this method to be called externally
    public void SaveTileDataForCurrentScene()
    {
        // Check if already saved or if interactableMap is null
        if (hasBeenSaved)
        {
            Debug.Log("Tile data already saved for this scene");
            return;
        }

        if (interactableMap == null)
        {
            Debug.LogWarning("Cannot save tile data - interactableMap is null");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        List<TileData> currentTileData = new List<TileData>();

        try
        {
            Debug.Log($"Saving tile data for scene: {currentScene}");
            
            // Save all modified tiles
            foreach (var position in interactableMap.cellBounds.allPositionsWithin)
            {
                TileBase tile = interactableMap.GetTile(position);
                if (tile != null)
                {
                    string tileName = tile.name;

                    // Save if it's not the default hidden tile or empty
                    // Updated condition to match your tile names
                    if (tileName != "Interactable" && 
                        tileName != "Interactable_Visible" && 
                        tileName != hiddenInteratableTile?.name)
                    {
                        TileData tileData = new TileData
                        {
                            position = position,
                            plantedSeed = GetPlantedSeed(position),
                            growthPhase = GetPlantGrowthPhase(position),
                            tileName = tileName
                        };
                        currentTileData.Add(tileData);
                    }
                }
            }

            // Store in static dictionary
            SceneTileData sceneData = new SceneTileData
            {
                sceneName = currentScene,
                tiles = currentTileData
            };
            savedTileDataByScene[currentScene] = sceneData;
            
            hasBeenSaved = true; // Mark as saved
            Debug.Log($"Saved {currentTileData.Count} tiles for scene {currentScene}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving tile data: {e.Message}");
        }
    }

    public void LoadTileDataForCurrentScene()
    {
        // Reset the saved flag when loading
        hasBeenSaved = false;
        
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"Loading tile data for scene: {currentScene}");

        // Clear existing plant data
        plantedSeeds.Clear();
        plantGrowthPhase.Clear();

        // First, try to load data for the current scene
        if (savedTileDataByScene.ContainsKey(currentScene))
        {
            LoadSceneData(currentScene);
            return;
        }

        // If no data for current scene, try to carry forward from previous day
        string previousScene = GetPreviousSceneName(currentScene);
        if (!string.IsNullOrEmpty(previousScene) && savedTileDataByScene.ContainsKey(previousScene))
        {
            Debug.Log($"No data for {currentScene}, carrying forward from {previousScene}");
            
            // Copy previous day's data to current day
            SceneTileData previousData = savedTileDataByScene[previousScene];
            SceneTileData currentData = new SceneTileData
            {
                sceneName = currentScene,
                tiles = new List<TileData>(previousData.tiles) // Create a copy
            };
            
            // Update scene name in the copied data
            foreach (TileData tile in currentData.tiles)
            {
                // Advance plant growth by 1 phase for the new day (plants grow overnight)
                if (!string.IsNullOrEmpty(tile.plantedSeed) && tile.growthPhase < 5)
                {
                    tile.growthPhase++;
                    // Update tile name to match new growth phase
                    tile.tileName = GetTileNameForGrowthPhase(tile.plantedSeed, tile.growthPhase);
                }
            }
            
            savedTileDataByScene[currentScene] = currentData;
            LoadSceneData(currentScene);
            return;
        }

        Debug.Log($"No saved data found for scene {currentScene} or previous scenes");
    }

    private void LoadSceneData(string sceneName)
    {
        SceneTileData sceneData = savedTileDataByScene[sceneName];
        List<TileData> tileDataList = sceneData.tiles;

        foreach (TileData tileData in tileDataList)
        {
            // Restore tile
            Tile tileToSet = GetTileByName(tileData.tileName);
            if (tileToSet != null && interactableMap != null)
            {
                interactableMap.SetTile(tileData.position, tileToSet);
            }

            // Restore plant data
            if (!string.IsNullOrEmpty(tileData.plantedSeed))
            {
                plantedSeeds[tileData.position] = tileData.plantedSeed;
                plantGrowthPhase[tileData.position] = tileData.growthPhase;
            }
        }

        Debug.Log($"Loaded {tileDataList.Count} tiles for scene {sceneName}");
    }

    private string GetPreviousSceneName(string currentScene)
    {
        // Extract day number from scene name (e.g., "Day2" -> 2)
        if (currentScene.StartsWith("Day"))
        {
            string dayNumberStr = currentScene.Substring(3);
            if (int.TryParse(dayNumberStr, out int dayNumber))
            {
                if (dayNumber > 1)
                {
                    return "Day" + (dayNumber - 1);
                }
            }
        }
        return null;
    }

    private string GetTileNameForGrowthPhase(string plantType, int phase)
    {
        switch (plantType)
        {
            case "TomatoSeed":
                switch (phase)
                {
                    case 0: return "TomatoSeeding";
                    case 1: return "TomatoPhase1";
                    case 2: return "TomatoPhase2";
                    case 3: return "TomatoPhase3";
                    case 4: return "TomatoPhase4";
                    case 5: return "TomatoHarvest";
                }
                break;
            case "RiceSeed":
                switch (phase)
                {
                    case 0: return "RiceSeeding";
                    case 1: return "RicePhase1";
                    case 2: return "RicePhase2";
                    case 3: return "RicePhase3";
                    case 4: return "RicePhase4";
                    case 5: return "RiceHarvest";
                }
                break;
            case "CucumberSeed":
                switch (phase)
                {
                    case 0: return "CucumberSeeding";
                    case 1: return "CucumberPhase1";
                    case 2: return "CucumberPhase2";
                    case 3: return "CucumberPhase3";
                    case 4: return "CucumberPhase4";
                    case 5: return "CucumberHarvest";
                }
                break;
            case "CabbageSeed":
                switch (phase)
                {
                    case 0: return "CabbageSeeding";
                    case 1: return "CabbagePhase1";
                    case 2: return "CabbagePhase2";
                    case 3: return "CabbagePhase3";
                    case 4: return "CabbagePhase4";
                    case 5: return "CabbageHarvest";
                }
                break;
        }
        return "Plowed"; // Default fallback
    }

    private Tile GetTileByName(string tileName)
    {
        // Expanded tile name mapping
        switch (tileName)
        {
            case "Plowed": return plowedTile;
            
            // Tomato tiles
            case "TomatoSeeding": return tomatoSeedingTile;
            case "TomatoPhase1": return tomatoPhase1Tile;
            case "TomatoPhase2": return tomatoPhase2Tile;
            case "TomatoPhase3": return tomatoPhase3Tile;
            case "TomatoPhase4": return tomatoPhase4Tile;
            case "TomatoHarvest": return tomatoHarvestTile;
            
            // Rice tiles
            case "RiceSeeding": return riceSeedingTile;
            case "RicePhase1": return ricePhase1Tile;
            case "RicePhase2": return ricePhase2Tile;
            case "RicePhase3": return ricePhase3Tile;
            case "RicePhase4": return ricePhase4Tile;
            case "RiceHarvest": return riceHarvestTile;
            
            // Cucumber tiles
            case "CucumberSeeding": return cucumberSeedingTile;
            case "CucumberPhase1": return cucumberPhase1Tile;
            case "CucumberPhase2": return cucumberPhase2Tile;
            case "CucumberPhase3": return cucumberPhase3Tile;
            case "CucumberPhase4": return cucumberPhase4Tile;
            case "CucumberHarvest": return cucumberHarvestTile;
            
            // Cabbage tiles
            case "CabbageSeeding": return cabbageSeedingTile;
            case "CabbagePhase1": return cabbagePhase1Tile;
            case "CabbagePhase2": return cabbagePhase2Tile;
            case "CabbagePhase3": return cabbagePhase3Tile;
            case "CabbagePhase4": return cabbagePhase4Tile;
            case "CabbageHarvest": return cabbageHarvestTile;
            
            default: return hiddenInteratableTile;
        }
    }
}
