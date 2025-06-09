using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        // Check if player has seen intro
        bool hasSeenIntro = PlayerPrefs.GetInt("HasSeenIntro", 0) == 1;

        if (!hasSeenIntro)
        {
            // First time playing - show intro
            SceneManager.LoadScene("IntroScene");
        }
        else
        {
            // Skip intro, go directly to Day1
            SceneManager.LoadScene("Day1");
        }
    }

    public void ResetGameProgress()
    {
        // This method can be called to reset and replay intro
        PlayerPrefs.DeleteKey("HasSeenIntro");
        PlayerPrefs.Save();
        SceneManager.LoadScene("IntroScene");
    }
}