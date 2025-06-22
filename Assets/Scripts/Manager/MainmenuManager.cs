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
    [SerializeField] Button closeCreditsButton;

    [Header("UI Elements - Transition Effect")]
    [SerializeField] Image transitionEffect;
    [SerializeField] GameObject transitionEffectObject;

    [Header("Audio Clip")]
    [SerializeField] AudioClip bgmSong;
    [SerializeField] AudioClip buttonClickSound;
    [SerializeField] AudioClip buttonCloseSFX;

    void Start()
    {
        if (bgmSong != null)
            AudioManager.instance.PlayMusic(bgmSong, true);

        // Aktifkan tombol "Load" hanya jika ada save file
        bool saveExists = SaveSystem.SaveExists();
        loadButton.interactable = saveExists;

        Debug.Log($"Save file exists: {saveExists}");
        Debug.Log($"Save file path: {System.IO.Path.Combine(Application.persistentDataPath, "gamesave.json")}");
    }

    public void StartGame()
    {
        if (buttonClickSound != null)
            AudioManager.instance.PlaySound(buttonClickSound);

        if (SaveSystem.SaveExists())
        {
            SaveSystem.DeleteSave();
            Debug.Log("Existing save file deleted for new game");
        }

        GameSaveData newSaveData = new GameSaveData
        {
            currentDay = 1,
            currentScene = "IntroScene",
            saveTime = System.DateTime.Now
        };

        SaveSystem.SaveGame(newSaveData);
        Debug.Log("New save file created for new game");

        transitionEffectObject.SetActive(true);
        LeanTween.alpha(transitionEffect.rectTransform, 1f, 1f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
        });
    }

    public void LoadGame()
    {
        if (buttonClickSound != null)
            AudioManager.instance.PlaySound(buttonClickSound);

        if (!SaveSystem.SaveExists())
        {
            Debug.LogError("No save file found to load!");
            return;
        }

        try
        {
            GameSaveData saveData = SaveSystem.LoadGame();

            if (saveData == null)
            {
                Debug.LogError("Failed to load save data - data is null!");
                return;
            }

            PlayerPrefs.SetString("LoadedSaveData", JsonUtility.ToJson(saveData));
            PlayerPrefs.SetInt("IsLoadingGame", 1);

            Debug.Log($"Loading game from day {saveData.currentDay}, scene: {saveData.currentScene}");

            transitionEffectObject.SetActive(true);
            LeanTween.alpha(transitionEffect.rectTransform, 1f, 1f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
            {
                string sceneToLoad = !string.IsNullOrEmpty(saveData.currentScene) ? saveData.currentScene : "IntroScene";
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
        }
    }

    public void Setting()
    {
        DisableAllButtons();
        settingPanel.SetActive(true);
        settingPanel.transform.localScale = Vector3.zero;

        if (buttonClickSound != null)
            AudioManager.instance.PlaySound(buttonClickSound);

        LeanTween.scale(settingPanel, Vector3.one, 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            closeSettingButton.interactable = true;
        });
    }

    public void CloseSetting()
    {
        if (buttonCloseSFX != null)
            AudioManager.instance.PlaySound(buttonCloseSFX);

        LeanTween.scale(settingPanel, Vector3.zero, 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            settingPanel.SetActive(false);
            EnableAllButtons();
        });
    }

    public void Credits()
    {
        DisableAllButtons();
        creditsPanel.SetActive(true);
        creditsPanel.transform.localScale = Vector3.zero;

        if (buttonClickSound != null)
            AudioManager.instance.PlaySound(buttonClickSound);

        LeanTween.scale(creditsPanel, Vector3.one, 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            closeCreditsButton.interactable = true;
        });
    }

    public void CloseCredits()
    {
        if (buttonCloseSFX != null)
            AudioManager.instance.PlaySound(buttonCloseSFX);

        LeanTween.scale(creditsPanel, Vector3.zero, 1f).setEase(LeanTweenType.easeOutSine).setOnComplete(() =>
        {
            creditsPanel.SetActive(false);
            EnableAllButtons();
        });
    }

    public void Exit()
    {
        Application.Quit();
    }
    
    private void DisableAllButtons()
    {
        startButton.interactable = false;
        loadButton.interactable = false;
        settingButton.interactable = false;
        creditsButton.interactable = false;
        exitButton.interactable = false;
    }

    private void EnableAllButtons()
    {
        startButton.interactable = true;
        loadButton.interactable = true;
        settingButton.interactable = true;
        creditsButton.interactable = true;
        exitButton.interactable = true;
    }
}
