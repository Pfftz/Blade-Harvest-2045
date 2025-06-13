using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainmenuManager : MonoBehaviour
{
    [Header("UI Elements - Main Menu")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] Button startButton;
    [SerializeField] Button loadButton;
    [SerializeField] Button settingButton;
    [SerializeField] Button creditsButton;
    [SerializeField] Button exitButton;

    [Header("UI Elements - Setting")]
    [SerializeField] GameObject settingPanel;
    [SerializeField] Button closeSettingButton;

    [Header("UI Elements - Credits")]
    [SerializeField] GameObject creditsPanel;

    [Header("UI Elements - Transition Effect")]
    [SerializeField] Image transitionEffect;
    [SerializeField] GameObject transitionEffectObject;

    [Header("Audio Clip")]
    [SerializeField] AudioClip bgmSong;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] AudioClip buttonCloseSFX;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.PlayMusic(bgmSong, true);

        // Check if save file exists and enable/disable load button accordingly
        bool saveExists = SaveSystem.SaveExists();
        loadButton.interactable = saveExists;

        Debug.Log($"Save file exists: {saveExists}");
        Debug.Log($"Save file path: {System.IO.Path.Combine(Application.persistentDataPath, "gamesave.json")}");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartGame()
    {
        // Play button click sound
        AudioManager.instance.PlaySound(buttonClickSound);

        // Delete existing save file to start fresh
        if (SaveSystem.SaveExists())
        {
            SaveSystem.DeleteSave();
            Debug.Log("Existing save file deleted for new game");
        }

        // Create new save data for day 1
        GameSaveData newSaveData = new GameSaveData();
        newSaveData.currentDay = 1;
        newSaveData.currentScene = "IntroScene"; // Start with intro, but track Day1 as next
        newSaveData.saveTime = System.DateTime.Now;
        SaveSystem.SaveGame(newSaveData);
        Debug.Log("New save file created for new game");

        transitionEffectObject.SetActive(true);
        LeanTween.alpha(transitionEffect.rectTransform, 1f, 1f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
        {
            // Load the intro scene first
            UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
            // Hide the transition effect
            transitionEffectObject.SetActive(true);
        });
    }

    public void LoadGame()
    {
        // Play button click sound
        AudioManager.instance.PlaySound(buttonClickSound);

        // Check if save file exists
        if (!SaveSystem.SaveExists())
        {
            Debug.LogError("No save file found to load!");
            return;
        }

        try
        {
            // Load the saved game data
            GameSaveData saveData = SaveSystem.LoadGame();

            if (saveData == null)
            {
                Debug.LogError("Failed to load save data - data is null!");
                return;
            }

            // Store the loaded data in a persistent way (using PlayerPrefs temporarily)
            PlayerPrefs.SetString("LoadedSaveData", JsonUtility.ToJson(saveData));
            PlayerPrefs.SetInt("IsLoadingGame", 1);

            Debug.Log($"Loading game from day {saveData.currentDay}, scene: {saveData.currentScene}");

            transitionEffectObject.SetActive(true);
            LeanTween.alpha(transitionEffect.rectTransform, 1f, 1f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
            {            // Load the scene where the player last saved
                string sceneToLoad = !string.IsNullOrEmpty(saveData.currentScene) ? saveData.currentScene : "IntroScene";
                Debug.Log($"Loading scene: {sceneToLoad}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
                transitionEffectObject.SetActive(true);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
        }
    }

    public void Setting()
    {
        // Button Checking
        startButton.interactable = false;
        loadButton.interactable = false;
        settingButton.interactable = false;
        creditsButton.interactable = false;
        exitButton.interactable = false;
        closeSettingButton.interactable = false;
        settingPanel.SetActive(true);

        // Play button click sound
        AudioManager.instance.PlaySound(buttonClickSound);

        // Tweening atau Animation untuk membuka panel setting
        LeanTween.scale(settingPanel, new Vector3(0.49615f, 0.49615f, 0.49615f), 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            // Open the settings panel with a scale animation
            closeSettingButton.interactable = true;
        });

    }

    public void CloseSetting()
    {
        AudioManager.instance.PlaySound(buttonCloseSFX);
        // Close the settings panel        
        LeanTween.scale(settingPanel, new Vector3(0f, 0f, 0f), 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            // Open the settings panel with a scale animation
            settingPanel.SetActive(false);

            // Button Checking
            startButton.interactable = true;
            loadButton.interactable = true;
            settingButton.interactable = true;
            creditsButton.interactable = true;
            exitButton.interactable = true;
        });
    }

    public void Credits()
    {

    }

    public void CloseCredits()
    {
        // Close the credits panel
        creditsPanel.SetActive(false);
    }

    public void Exit()
    {
        // Exit Game
        Application.Quit();
    }
}
