using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Cutscene Setup")]
    public CutsceneController cutsceneController;

    [Header("Scene Management")]
    public string nextSceneName = "Day1";
    public float delayBeforeSceneChange = 1f;

    private void Start()
    {
        // Mark that the player has seen the intro
        PlayerPrefs.SetInt("HasSeenIntro", 1);

        // Start the intro cutscene
        StartCoroutine(PlayIntroCutscene());
    }

    private IEnumerator PlayIntroCutscene()
    {
        // Wait a moment to ensure everything is loaded
        yield return new WaitForSeconds(0.5f);

        // Play the intro cutscene
        cutsceneController.RunCutscene(Cutscenes.IntroCutscene, OnIntroCutsceneComplete);
    }

    private void OnIntroCutsceneComplete()
    {
        Debug.Log("Intro cutscene completed!");

        // Wait a moment then transition to Day1
        StartCoroutine(TransitionToGameplay());
    }

    private IEnumerator TransitionToGameplay()
    {
        yield return new WaitForSeconds(delayBeforeSceneChange);

        // Load the first day scene
        SceneManager.LoadScene(nextSceneName);
    }
}