using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    public Dictionary<string, SceneTileData> tileDataByScene = new Dictionary<string, SceneTileData>();
    public Dictionary<string, List<Player.InventorySlotData>> inventoryData = new Dictionary<string, List<Player.InventorySlotData>>();
    public int currentDay = 1;
    public string currentScene = "IntroScene"; // Track which scene the player is currently in
    public System.DateTime saveTime; // When the save was created
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