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
    private static Dictionary<string, List<TileData>> savedTileData = new Dictionary<string, List<TileData>>();

    void Start()
    {
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
    }

    private void OnDestroy()
    {
        // Only save if the tilemap still exists
        if (interactableMap != null)
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

    public void SaveTileDataForCurrentScene()
    {
        // Add null checks to prevent errors during scene destruction
        if (interactableMap == null)
        {
            Debug.LogWarning("Cannot save tile data - interactableMap is null");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        List<TileData> currentTileData = new List<TileData>();

        try
        {
            // Save all plowed tiles and planted seeds
            foreach (var position in interactableMap.cellBounds.allPositionsWithin)
            {
                TileBase tile = interactableMap.GetTile(position);
                if (tile != null)
                {
                    string tileName = tile.name;

                    // Save if it's not the default hidden tile
                    if (tileName != "Interactable")
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

            savedTileData[currentScene] = currentTileData;
            Debug.Log($"Saved {currentTileData.Count} tiles for scene {currentScene}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving tile data: {e.Message}");
        }
    }

    public void LoadTileDataForCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedTileData.ContainsKey(currentScene))
        {
            List<TileData> tileDataList = savedTileData[currentScene];

            foreach (TileData tileData in tileDataList)
            {
                // Restore tile
                Tile tileToSet = GetTileByName(tileData.tileName);
                if (tileToSet != null)
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

            Debug.Log($"Loaded {tileDataList.Count} tiles for scene {currentScene}");
        }
    }

    private Tile GetTileByName(string tileName)
    {
        // Map tile names to actual tile references
        switch (tileName)
        {
            case "Plowed": return plowedTile;
            case "TomatoSeeding": return tomatoSeedingTile;
            case "TomatoPhase1": return tomatoPhase1Tile;
            case "TomatoPhase2": return tomatoPhase2Tile;
            case "TomatoPhase3": return tomatoPhase3Tile;
            case "TomatoPhase4": return tomatoPhase4Tile;
            case "TomatoHarvest": return tomatoHarvestTile;
            // Add cases for all your tiles...
            default: return hiddenInteratableTile;
        }
    }
}
