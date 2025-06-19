using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Setup")]
    public CutsceneController cutsceneController;

    [Header("Cutscene Type")]
    [SerializeField] private CutsceneType cutsceneType = CutsceneType.Intro;

    [Header("Scene Management")]
    public string nextSceneName = "Day1";
    public float delayBeforeSceneChange = 1f;

    [Header("Ending Scene Settings")]
    public string mainMenuSceneName = "MainMenu";
    public bool returnToMainMenuAfterEnding = true;

    public enum CutsceneType
    {
        Intro,
        GoodEnding,
        BadEnding
    }

    private void Start()
    {
        // Start the appropriate cutscene based on type
        StartCoroutine(PlayCutscene());
    }

    private IEnumerator PlayCutscene()
    {
        // Wait a moment to ensure everything is loaded
        yield return new WaitForSeconds(0.5f);

        // Play the appropriate cutscene based on type
        switch (cutsceneType)
        {
            case CutsceneType.Intro:
                PlayIntroCutscene();
                break;
            case CutsceneType.GoodEnding:
                PlayGoodEndingCutscene();
                break;
            case CutsceneType.BadEnding:
                PlayBadEndingCutscene();
                break;
        }
    }

    private void PlayIntroCutscene()
    {
        // Mark that the player has seen the intro
        PlayerPrefs.SetInt("HasSeenIntro", 1);
        cutsceneController.RunCutscene(Cutscenes.IntroCutscene, OnIntroCutsceneComplete);
    }

    private void PlayGoodEndingCutscene()
    {
        // Mark the ending type
        PlayerPrefs.SetInt("LastEndingWasGood", 1);
        cutsceneController.RunCutscene(Cutscenes.GoodEnding, OnEndingCutsceneComplete);
    }

    private void PlayBadEndingCutscene()
    {
        // Mark the ending type
        PlayerPrefs.SetInt("LastEndingWasGood", 0);
        cutsceneController.RunCutscene(Cutscenes.BadEnding, OnEndingCutsceneComplete);
    }

    private void OnIntroCutsceneComplete()
    {
        Debug.Log("Intro cutscene completed!");
        StartCoroutine(TransitionToNextScene());
    }

    private void OnEndingCutsceneComplete()
    {
        Debug.Log($"{cutsceneType} cutscene completed!");

        if (returnToMainMenuAfterEnding)
        {
            // Return to main menu after ending cutscenes
            nextSceneName = mainMenuSceneName;
        }

        StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator TransitionToNextScene()
    {
        yield return new WaitForSeconds(delayBeforeSceneChange);

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    /// <summary>
    /// Set the cutscene type programmatically
    /// </summary>
    /// <param name="type">The type of cutscene to play</param>
    public void SetCutsceneType(CutsceneType type)
    {
        cutsceneType = type;
    }

    /// <summary>
    /// Play a specific cutscene type immediately
    /// </summary>
    /// <param name="type">The type of cutscene to play</param>
    public void PlaySpecificCutscene(CutsceneType type)
    {
        cutsceneType = type;
        StartCoroutine(PlayCutscene());
    }
}