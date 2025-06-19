using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // Use Lists instead of Dictionaries for proper JSON serialization
    [System.Serializable]
    public class SceneTileDataEntry
    {
        public string sceneName;
        public SceneTileData tileData;
    }

    [System.Serializable]
    public class InventoryDataEntry
    {
        public string sceneName;
        public List<Player.InventorySlotData> inventorySlots;
    }

    public List<SceneTileDataEntry> tileDataByScene = new List<SceneTileDataEntry>();
    public List<InventoryDataEntry> inventoryData = new List<InventoryDataEntry>();
    public int currentDay = 1;
    public string currentScene = "IntroScene"; // Track which scene the player is currently in
    public System.DateTime saveTime; // When the save was created
    public int playerCurrency = 0;

    // Helper methods to access tile data like a dictionary
    public SceneTileData GetTileDataForScene(string sceneName)
    {
        var entry = tileDataByScene.Find(e => e.sceneName == sceneName);
        return entry?.tileData;
    }

    public void SetTileDataForScene(string sceneName, SceneTileData tileData)
    {
        var existingEntry = tileDataByScene.Find(e => e.sceneName == sceneName);
        if (existingEntry != null)
        {
            existingEntry.tileData = tileData;
        }
        else
        {
            tileDataByScene.Add(new SceneTileDataEntry { sceneName = sceneName, tileData = tileData });
        }
    }

    public bool HasTileDataForScene(string sceneName)
    {
        return tileDataByScene.Exists(e => e.sceneName == sceneName);
    }

    // Helper methods to access inventory data like a dictionary
    public List<Player.InventorySlotData> GetInventoryDataForScene(string sceneName)
    {
        var entry = inventoryData.Find(e => e.sceneName == sceneName);
        return entry?.inventorySlots;
    }

    public void SetInventoryDataForScene(string sceneName, List<Player.InventorySlotData> slots)
    {
        var existingEntry = inventoryData.Find(e => e.sceneName == sceneName);
        if (existingEntry != null)
        {
            existingEntry.inventorySlots = slots;
        }
        else
        {
            inventoryData.Add(new InventoryDataEntry { sceneName = sceneName, inventorySlots = slots });
        }
    }
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json"); public static void SaveGame(GameSaveData saveData)
    {
        try
        {
            // Ensure directory exists
            string directory = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to: {SavePath}");
            Debug.Log($"Save file size: {new FileInfo(SavePath).Length} bytes");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public static GameSaveData LoadGame()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                Debug.Log($"Loading save file. Size: {new FileInfo(SavePath).Length} bytes");
                Debug.Log($"JSON preview: {json.Substring(0, Mathf.Min(200, json.Length))}...");

                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

                if (saveData == null)
                {
                    Debug.LogError("JsonUtility.FromJson returned null!");
                    return new GameSaveData();
                }

                Debug.Log($"Game loaded successfully - Day: {saveData.currentDay}, Scene: {saveData.currentScene}");
                return saveData;
            }
            else
            {
                Debug.LogWarning($"Save file not found at: {SavePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }

        return new GameSaveData(); // Return new save data if file doesn't exist
    }

    public static bool SaveExists()
    {
        return File.Exists(SavePath);
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
        }
    }
}