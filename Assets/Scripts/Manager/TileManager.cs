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
            Debug.Log("TileManager: interactableMap not assigned, searching...");

            // Look for Grid parent and find Interactable Map child
            Grid grid = FindObjectOfType<Grid>();
            if (grid != null)
            {
                Debug.Log("Found Grid object, searching for Interactable Map child...");
                Transform interactableMapTransform = grid.transform.Find("Interactable Map");
                if (interactableMapTransform != null)
                {
                    interactableMap = interactableMapTransform.GetComponent<Tilemap>();
                    Debug.Log("Found Interactable Map in Grid");
                }
                else
                {
                    Debug.LogWarning("Could not find 'Interactable Map' child in Grid");
                }
            }
            else
            {
                Debug.LogWarning("Could not find Grid object");
            }

            // If still not found, try to find by name
            if (interactableMap == null)
            {
                Debug.Log("Searching for Interactable Map by name...");
                GameObject interactableMapObj = GameObject.Find("Interactable Map");
                if (interactableMapObj != null)
                {
                    interactableMap = interactableMapObj.GetComponent<Tilemap>();
                    Debug.Log("Found Interactable Map by name");
                }
                else
                {
                    Debug.LogWarning("Could not find 'Interactable Map' GameObject by name");
                }
            }

            // Last resort: find any Tilemap with "Interactable" in the name
            if (interactableMap == null)
            {
                Debug.Log("Last resort: searching for any Tilemap with 'Interactable' in name...");
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

        // Final check
        if (interactableMap == null)
        {
            Debug.LogError("CRITICAL: TileManager could not find interactableMap! Tile saving/loading will not work!");
            return;
        }
        else
        {
            Debug.Log($"TileManager: Successfully found interactableMap: {interactableMap.name}");
        }

        // Initialize hidden tiles
        Debug.Log("TileManager: Initializing hidden tiles...");
        int hiddenTileCount = 0;
        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(position);

            if (tile != null && tile.name == "Interactable_Visible")
            {
                interactableMap.SetTile(position, hiddenInteratableTile);
                hiddenTileCount++;
            }
        }
        Debug.Log($"TileManager: Initialized {hiddenTileCount} hidden tiles");

        // Register with GameManager
        if (GameManager.instance != null)
        {
            Debug.Log("TileManager: Registering with GameManager");
            GameManager.instance.tileManager = this;
        }
        else
        {
            Debug.LogWarning("TileManager: GameManager.instance is null at Start!");
        }

        // Load tile data for current scene (delayed to ensure everything is set up)
        StartCoroutine(DelayedLoadTileData());
    }

    private IEnumerator DelayedLoadTileData()
    {
        // Wait a bit to ensure everything is properly initialized
        yield return new WaitForSeconds(0.2f);

        Debug.Log("TileManager: Loading tile data for current scene...");
        LoadTileDataForCurrentScene();
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
                    string actualTileName = tile.name;

                    // Save if it's not the default hidden tile or empty
                    // Updated condition to match your tile names
                    if (actualTileName != "Interactable" &&
                        actualTileName != "Interactable_Visible" &&
                        actualTileName != hiddenInteratableTile?.name)
                    {
                        // Get the logical tile name instead of the actual asset name
                        string logicalTileName = GetLogicalTileName(tile, position);

                        TileData tileData = new TileData
                        {
                            position = position,
                            plantedSeed = GetPlantedSeed(position),
                            growthPhase = GetPlantGrowthPhase(position),
                            tileName = logicalTileName // Use logical name for consistency
                        };
                        currentTileData.Add(tileData);

                        // Debug log for harvest tiles
                        if (logicalTileName.Contains("Harvest"))
                        {
                            Debug.Log($"Saving harvest tile: {logicalTileName} (actual: {actualTileName}) at {position} with phase {tileData.growthPhase}");
                        }
                        else if (actualTileName != logicalTileName)
                        {
                            Debug.Log($"Tile name mapping: {actualTileName} -> {logicalTileName} at {position}");
                        }
                    }
                }
            }

            // Store in static dictionary (old system)
            SceneTileData sceneData = new SceneTileData
            {
                sceneName = currentScene,
                tiles = currentTileData
            };
            savedTileDataByScene[currentScene] = sceneData;

            // NEW: Also save to GameManager's new SaveSystem
            if (GameManager.instance != null && GameManager.instance.currentSaveData != null)
            {
                GameManager.instance.currentSaveData.tileDataByScene[currentScene] = sceneData;
                Debug.Log($"Tile data also saved to new SaveSystem for scene: {currentScene}");
            }

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

        // NEW: First check if GameManager has save data from new SaveSystem
        if (GameManager.instance != null && GameManager.instance.currentSaveData != null)
        {
            var saveData = GameManager.instance.currentSaveData;
            if (saveData.tileDataByScene.ContainsKey(currentScene))
            {
                Debug.Log($"Loading tile data from new SaveSystem for scene: {currentScene}");
                // Convert from new save format to old format and load
                SceneTileData sceneData = saveData.tileDataByScene[currentScene];
                savedTileDataByScene[currentScene] = sceneData;
                LoadSceneData(currentScene);
                return;
            }
        }

        // EXISTING: Fall back to old save system
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
                // BUT don't advance if already at harvest phase (5)
                if (!string.IsNullOrEmpty(tile.plantedSeed) && tile.growthPhase < 5)
                {
                    tile.growthPhase++;
                    // Update tile name to match new growth phase
                    tile.tileName = GetTileNameForGrowthPhase(tile.plantedSeed, tile.growthPhase);
                }
                // If already at harvest phase (5), keep it at harvest phase
                else if (tile.growthPhase == 5)
                {
                    // Keep harvest tiles as they are - don't advance further
                    Debug.Log($"Keeping harvest tile {tile.tileName} at phase {tile.growthPhase} (no advancement)");
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
        if (!savedTileDataByScene.ContainsKey(sceneName))
        {
            Debug.LogError($"No saved tile data for scene: {sceneName}");
            return;
        }

        if (interactableMap == null)
        {
            Debug.LogError("Cannot load tile data - interactableMap is null!");
            return;
        }

        SceneTileData sceneData = savedTileDataByScene[sceneName];
        List<TileData> tileDataList = sceneData.tiles;

        Debug.Log($"Loading {tileDataList.Count} tiles for scene {sceneName}");

        int harvestTilesLoaded = 0;
        int failedTileLoads = 0;

        foreach (TileData tileData in tileDataList)
        {
            try
            {
                // Debug log for harvest tiles specifically
                if (tileData.tileName.Contains("Harvest"))
                {
                    Debug.Log($"Loading harvest tile: {tileData.tileName} at {tileData.position} with phase {tileData.growthPhase}");
                    harvestTilesLoaded++;
                }

                // Restore tile
                Tile tileToSet = GetTileByName(tileData.tileName);
                if (tileToSet != null && interactableMap != null)
                {
                    interactableMap.SetTile(tileData.position, tileToSet);

                    // Verify the tile was actually set
                    var verifyTile = interactableMap.GetTile(tileData.position);
                    if (verifyTile == null)
                    {
                        Debug.LogError($"Failed to set tile {tileData.tileName} at {tileData.position} - tile is null after SetTile");
                        failedTileLoads++;
                    }
                    else if (verifyTile.name != tileToSet.name)
                    {
                        Debug.LogError($"Tile mismatch at {tileData.position}: Expected {tileToSet.name}, got {verifyTile.name}");
                        failedTileLoads++;
                    }
                    else if (tileData.tileName.Contains("Harvest"))
                    {
                        Debug.Log($"Successfully verified harvest tile {tileData.tileName} at {tileData.position}");
                    }
                }
                else
                {
                    if (tileToSet == null)
                    {
                        Debug.LogError($"Failed to get tile for: {tileData.tileName}");
                    }
                    if (interactableMap == null)
                    {
                        Debug.LogError("interactableMap became null during loading!");
                    }
                    failedTileLoads++;
                }

                // Restore plant data
                if (!string.IsNullOrEmpty(tileData.plantedSeed))
                {
                    plantedSeeds[tileData.position] = tileData.plantedSeed;
                    plantGrowthPhase[tileData.position] = tileData.growthPhase;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception loading tile {tileData.tileName} at {tileData.position}: {e.Message}");
                failedTileLoads++;
            }
        }

        Debug.Log($"Loaded {tileDataList.Count} tiles for scene {sceneName}. Harvest tiles loaded: {harvestTilesLoaded}. Failed loads: {failedTileLoads}");

        if (harvestTilesLoaded > 0)
        {
            Debug.Log($"SUCCESS: {harvestTilesLoaded} harvest tiles loaded for scene {sceneName}");
        }

        if (failedTileLoads > 0)
        {
            Debug.LogError($"WARNING: {failedTileLoads} tiles failed to load properly for scene {sceneName}");
        }
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
        // Safety check: if phase is beyond 5 (harvest), keep it at 5
        if (phase > 5)
        {
            Debug.LogWarning($"Plant {plantType} phase {phase} is beyond harvest phase! Clamping to 5.");
            phase = 5;
        }

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

        // If we get here, something went wrong - return a safe default
        Debug.LogError($"Unknown plant type {plantType} or invalid phase {phase}! Returning Plowed as fallback.");
        return "Plowed"; // Default fallback
    }

    private Tile GetTileByName(string tileName)
    {
        // Expanded tile name mapping with null checks
        switch (tileName)
        {
            case "Plowed": return plowedTile;

            // Tomato tiles
            case "TomatoSeeding": return tomatoSeedingTile;
            case "TomatoPhase1": return tomatoPhase1Tile;
            case "TomatoPhase2": return tomatoPhase2Tile;
            case "TomatoPhase3": return tomatoPhase3Tile;
            case "TomatoPhase4": return tomatoPhase4Tile;
            case "TomatoHarvest":
                if (tomatoHarvestTile == null)
                {
                    Debug.LogError("TomatoHarvest tile is null! Check TileManager inspector.");
                    return null;
                }
                return tomatoHarvestTile;

            // Rice tiles
            case "RiceSeeding": return riceSeedingTile;
            case "RicePhase1": return ricePhase1Tile;
            case "RicePhase2": return ricePhase2Tile;
            case "RicePhase3": return ricePhase3Tile;
            case "RicePhase4": return ricePhase4Tile;
            case "RiceHarvest":
                if (riceHarvestTile == null)
                {
                    Debug.LogError("RiceHarvest tile is null! Check TileManager inspector.");
                    return null;
                }
                return riceHarvestTile;

            // Cucumber tiles
            case "CucumberSeeding": return cucumberSeedingTile;
            case "CucumberPhase1": return cucumberPhase1Tile;
            case "CucumberPhase2": return cucumberPhase2Tile;
            case "CucumberPhase3": return cucumberPhase3Tile;
            case "CucumberPhase4": return cucumberPhase4Tile;
            case "CucumberHarvest":
                if (cucumberHarvestTile == null)
                {
                    Debug.LogError("CucumberHarvest tile is null! Check TileManager inspector.");
                    return null;
                }
                return cucumberHarvestTile;

            // Cabbage tiles
            case "CabbageSeeding": return cabbageSeedingTile;
            case "CabbagePhase1": return cabbagePhase1Tile;
            case "CabbagePhase2": return cabbagePhase2Tile;
            case "CabbagePhase3": return cabbagePhase3Tile;
            case "CabbagePhase4": return cabbagePhase4Tile;
            case "CabbageHarvest":
                if (cabbageHarvestTile == null)
                {
                    Debug.LogError("CabbageHarvest tile is null! Check TileManager inspector.");
                    return null;
                }
                return cabbageHarvestTile;

            default:
                // Handle legacy/unknown tile names - try to find a matching tile
                Debug.LogWarning($"Unknown tile name: {tileName} - attempting to find matching tile");

                // Try to find a tile with this actual name
                Tile[] allTiles = {
                    tomatoHarvestTile, riceHarvestTile, cucumberHarvestTile, cabbageHarvestTile,
                    tomatoSeedingTile, tomatoPhase1Tile, tomatoPhase2Tile, tomatoPhase3Tile, tomatoPhase4Tile,
                    riceSeedingTile, ricePhase1Tile, ricePhase2Tile, ricePhase3Tile, ricePhase4Tile,
                    cucumberSeedingTile, cucumberPhase1Tile, cucumberPhase2Tile, cucumberPhase3Tile, cucumberPhase4Tile,
                    cabbageSeedingTile, cabbagePhase1Tile, cabbagePhase2Tile, cabbagePhase3Tile, cabbagePhase4Tile,
                    plowedTile, hiddenInteratableTile
                };

                foreach (Tile tile in allTiles)
                {
                    if (tile != null && tile.name == tileName)
                    {
                        Debug.Log($"Found matching tile for {tileName}");
                        return tile;
                    }
                }

                Debug.LogError($"Could not find any tile matching name: {tileName}");
                return hiddenInteratableTile; // Fallback to hidden tile instead of null
        }
    }

    // Method to get logical tile name from actual tile
    private string GetLogicalTileName(TileBase tile, Vector3Int position)
    {
        if (tile == null) return null;

        string actualName = tile.name;

        // Check if we have plant data for this position to determine the logical name
        if (plantedSeeds.ContainsKey(position) && plantGrowthPhase.ContainsKey(position))
        {
            string plantType = plantedSeeds[position];
            int phase = plantGrowthPhase[position];

            // Return the logical name based on plant type and phase
            return GetTileNameForGrowthPhase(plantType, phase);
        }

        // Handle known tile mappings
        if (tile == plowedTile) return "Plowed";
        if (tile == hiddenInteratableTile) return "Interactable";

        // Check against our known harvest tiles
        if (tile == tomatoHarvestTile) return "TomatoHarvest";
        if (tile == riceHarvestTile) return "RiceHarvest";
        if (tile == cucumberHarvestTile) return "CucumberHarvest";
        if (tile == cabbageHarvestTile) return "CabbageHarvest";

        // Check against all other known tiles
        if (tile == tomatoSeedingTile) return "TomatoSeeding";
        if (tile == tomatoPhase1Tile) return "TomatoPhase1";
        if (tile == tomatoPhase2Tile) return "TomatoPhase2";
        if (tile == tomatoPhase3Tile) return "TomatoPhase3";
        if (tile == tomatoPhase4Tile) return "TomatoPhase4";

        if (tile == riceSeedingTile) return "RiceSeeding";
        if (tile == ricePhase1Tile) return "RicePhase1";
        if (tile == ricePhase2Tile) return "RicePhase2";
        if (tile == ricePhase3Tile) return "RicePhase3";
        if (tile == ricePhase4Tile) return "RicePhase4";

        if (tile == cucumberSeedingTile) return "CucumberSeeding";
        if (tile == cucumberPhase1Tile) return "CucumberPhase1";
        if (tile == cucumberPhase2Tile) return "CucumberPhase2";
        if (tile == cucumberPhase3Tile) return "CucumberPhase3";
        if (tile == cucumberPhase4Tile) return "CucumberPhase4";

        if (tile == cabbageSeedingTile) return "CabbageSeeding";
        if (tile == cabbagePhase1Tile) return "CabbagePhase1";
        if (tile == cabbagePhase2Tile) return "CabbagePhase2";
        if (tile == cabbagePhase3Tile) return "CabbagePhase3";
        if (tile == cabbagePhase4Tile) return "CabbagePhase4";

        // If we can't map it, return the actual name but log a warning
        Debug.LogWarning($"Could not map tile to logical name: {actualName} at {position}");
        return actualName;
    }

    // Debug method to check current harvest tiles
    [ContextMenu("Debug Current Harvest Tiles")]
    public void DebugCurrentHarvestTiles()
    {
        if (interactableMap == null)
        {
            Debug.LogError("Cannot debug - interactableMap is null!");
            return;
        }

        List<Vector3Int> harvestPositions = new List<Vector3Int>();

        foreach (var position in interactableMap.cellBounds.allPositionsWithin)
        {
            var tile = interactableMap.GetTile(position);
            if (tile != null && tile.name.Contains("Harvest"))
            {
                harvestPositions.Add(position);
                Debug.Log($"[HARVEST DEBUG] Found harvest tile: {tile.name} at {position}");

                // Check if this position has plant data
                if (plantedSeeds.ContainsKey(position))
                {
                    Debug.Log($"[HARVEST DEBUG] - Planted seed: {plantedSeeds[position]}, Phase: {GetPlantGrowthPhase(position)}");
                }
                else
                {
                    Debug.LogWarning($"[HARVEST DEBUG] - No plant data for harvest tile at {position}");
                }
            }
        }

        Debug.Log($"[HARVEST DEBUG] Total harvest tiles found: {harvestPositions.Count}");
    }

    // Debug method to check save data
    [ContextMenu("Debug Save Data")]
    public void DebugSaveData()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Check static save data
        if (savedTileDataByScene.ContainsKey(currentScene))
        {
            var staticData = savedTileDataByScene[currentScene];
            int harvestCount = 0;
            foreach (var tile in staticData.tiles)
            {
                if (tile.tileName.Contains("Harvest"))
                {
                    harvestCount++;
                    Debug.Log($"[SAVE DEBUG] Static data has harvest: {tile.tileName} at {tile.position}");
                }
            }
            Debug.Log($"[SAVE DEBUG] Static save data has {harvestCount} harvest tiles for {currentScene}");
        }
        else
        {
            Debug.LogWarning($"[SAVE DEBUG] No static save data for {currentScene}");
        }

        // Check GameManager save data
        if (GameManager.instance?.currentSaveData?.tileDataByScene.ContainsKey(currentScene) == true)
        {
            var gameManagerData = GameManager.instance.currentSaveData.tileDataByScene[currentScene];
            int harvestCount = 0;
            foreach (var tile in gameManagerData.tiles)
            {
                if (tile.tileName.Contains("Harvest"))
                {
                    harvestCount++;
                    Debug.Log($"[SAVE DEBUG] GameManager data has harvest: {tile.tileName} at {tile.position}");
                }
            }
            Debug.Log($"[SAVE DEBUG] GameManager save data has {harvestCount} harvest tiles for {currentScene}");
        }
        else
        {
            Debug.LogWarning($"[SAVE DEBUG] No GameManager save data for {currentScene}");
        }
    }

    // Method to force save and immediately reload (for testing)
    [ContextMenu("Test Save-Load Cycle")]
    public void TestSaveLoadCycle()
    {
        Debug.Log("[TEST] Starting save-load cycle test...");

        // Count current harvest tiles
        DebugCurrentHarvestTiles();

        // Force save
        Debug.Log("[TEST] Forcing save...");
        SaveTileDataForCurrentScene();

        // Check save data
        DebugSaveData();

        // Clear current state and reload
        Debug.Log("[TEST] Clearing and reloading...");
        plantedSeeds.Clear();
        plantGrowthPhase.Clear();
        hasBeenSaved = false;

        LoadTileDataForCurrentScene();

        // Check result
        Debug.Log("[TEST] After reload:");
        DebugCurrentHarvestTiles();
    }

    // Method to fix tile data with wrong names (for debugging and fixing existing saves)
    [ContextMenu("Fix Save Data Tile Names")]
    public void FixSaveDataTileNames()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Fix GameManager save data
        if (GameManager.instance?.currentSaveData?.tileDataByScene.ContainsKey(currentScene) == true)
        {
            var tileData = GameManager.instance.currentSaveData.tileDataByScene[currentScene];
            int fixedCount = 0;

            for (int i = 0; i < tileData.tiles.Count; i++)
            {
                var tile = tileData.tiles[i];

                // If the tile name doesn't match our expected logical names, try to fix it
                if (!IsLogicalTileName(tile.tileName))
                {
                    // Try to find the actual tile at this position
                    var actualTile = interactableMap?.GetTile(tile.position);
                    if (actualTile != null)
                    {
                        string logicalName = GetLogicalTileName(actualTile, tile.position);
                        if (logicalName != tile.tileName)
                        {
                            Debug.Log($"Fixing tile name: {tile.tileName} -> {logicalName} at {tile.position}");
                            tile.tileName = logicalName;
                            fixedCount++;
                        }
                    }
                }
            }

            Debug.Log($"Fixed {fixedCount} tile names in save data for {currentScene}");

            if (fixedCount > 0)
            {
                // Save the corrected data
                GameManager.instance.SaveGame();
                Debug.Log("Corrected save data has been saved to file");
            }
        }
    }

    // Helper method to check if a tile name is a logical name we recognize
    private bool IsLogicalTileName(string tileName)
    {
        string[] knownLogicalNames = {
            "Plowed",
            "TomatoSeeding", "TomatoPhase1", "TomatoPhase2", "TomatoPhase3", "TomatoPhase4", "TomatoHarvest",
            "RiceSeeding", "RicePhase1", "RicePhase2", "RicePhase3", "RicePhase4", "RiceHarvest",
            "CucumberSeeding", "CucumberPhase1", "CucumberPhase2", "CucumberPhase3", "CucumberPhase4", "CucumberHarvest",
            "CabbageSeeding", "CabbagePhase1", "CabbagePhase2", "CabbagePhase3", "CabbagePhase4", "CabbageHarvest"
        };

        return System.Array.Exists(knownLogicalNames, name => name == tileName);
    }
}
