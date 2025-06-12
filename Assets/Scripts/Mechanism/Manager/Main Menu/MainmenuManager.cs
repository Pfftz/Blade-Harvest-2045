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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        // Play button click sound
        AudioManager.instance.PlaySound(buttonClickSound);

        transitionEffectObject.SetActive(true);
        LeanTween.alpha(transitionEffect.rectTransform, 1f, 1f).setEase(LeanTweenType.easeInSine).setOnComplete(() =>
        {
            // Load the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
            // Hide the transition effect
            transitionEffectObject.SetActive(true);
        });
    }

    public void LoadGame()
    {
        // Play button click sound
        AudioManager.instance.PlaySound(buttonClickSound);

        // Load the saved game data
        // This is a placeholder; actual implementation will depend on how you save and load game data
        Debug.Log("Load Game functionality not implemented yet.");
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
