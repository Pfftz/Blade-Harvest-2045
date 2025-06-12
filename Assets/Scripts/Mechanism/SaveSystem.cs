using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public Dictionary<string, SceneTileData> tileDataByScene = new Dictionary<string, SceneTileData>();
    public Dictionary<string, List<Player.InventorySlotData>> inventoryData = new Dictionary<string, List<Player.InventorySlotData>>();
    public int currentDay = 1;
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "gamesave.json");

    public static void SaveGame(GameSaveData saveData)
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved to: {SavePath}");
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
                GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
                Debug.Log("Game loaded successfully");
                return saveData;
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
}