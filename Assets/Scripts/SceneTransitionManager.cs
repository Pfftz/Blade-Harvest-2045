using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Transition Settings")]
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private Image fadePanel;

    [Header("Sleep Transition")]
    [SerializeField] private VideoClip sleepTransitionVideo;
    [SerializeField] private Sprite[] sleepAnimationSprites;
    [SerializeField] private float spriteAnimationSpeed = 0.2f;

    [Header("Day Management")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 7;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup video player if video is assigned
        if (sleepTransitionVideo != null)
        {
            SetupVideoPlayer();
        }

        // Ensure transition canvas is initially hidden
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);
    }

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Configure video player settings for better performance
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.skipOnDrop = true; // Allow frame skipping to prevent lag
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        // In SetupVideoPlayer method, replace the audio line with:
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioMute(0, false);

        // Create render texture if needed
        if (videoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.useMipMap = false; // Disable mipmaps for better performance
            videoPlayer.targetTexture = renderTexture;

            if (videoDisplay != null)
            {
                videoDisplay.texture = renderTexture;
            }
        }
    }

    public void StartSleepTransition()
    {
        Debug.Log("StartSleepTransition called");
        StartCoroutine(SleepTransitionCoroutine());
    }

    private IEnumerator SleepTransitionCoroutine()
    {
        Debug.Log("Starting sleep transition...");

        // Activate transition canvas
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(true);
            Debug.Log("Transition canvas activated");
        }
        else
        {
            Debug.LogError("Transition canvas is null! Please assign it in the inspector.");
        }

        // Disable player movement
        DisablePlayerMovement();

        // Choose transition type (video or sprite animation)
        if (sleepTransitionVideo != null && videoPlayer != null)
        {
            Debug.Log("Playing video transition");
            yield return StartCoroutine(PlayVideoTransition());
        }
        else if (sleepAnimationSprites != null && sleepAnimationSprites.Length > 0)
        {
            Debug.Log("Playing sprite animation transition");
            yield return StartCoroutine(PlaySpriteAnimation());
        }
        else
        {
            Debug.Log("Playing simple fade transition");
            // Fallback to simple fade
            yield return StartCoroutine(SimpleFadeTransition());
        }

        Debug.Log("Transition complete, loading next day scene...");

        // Load next day scene
        LoadNextDayScene();
    }

    private IEnumerator PlayVideoTransition()
    {
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            Debug.Log("Video display activated");
        }

        // Set the video clip
        videoPlayer.clip = sleepTransitionVideo;

        // Prepare the video
        videoPlayer.Prepare();

        // Wait for video to be prepared
        while (!videoPlayer.isPrepared)
        {
            Debug.Log("Preparing video...");
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Video prepared. Duration: {videoPlayer.clip.length} seconds");

        // Play the video
        videoPlayer.Play();

        // Wait for video to actually start playing
        while (!videoPlayer.isPlaying)
        {
            yield return null;
        }

        Debug.Log("Video started playing");

        // Better video completion detection
        float videoDuration = (float)videoPlayer.clip.length;
        float startTime = Time.time;

        // Wait for video to complete using multiple checks
        while (videoPlayer.isPlaying && (Time.time - startTime) < videoDuration + 0.5f)
        {
            yield return null;
        }

        // Additional check: wait until we're near the end of the video
        while (videoPlayer.time < videoDuration - 0.1f && videoPlayer.isPlaying)
        {
            yield return null;
        }

        Debug.Log("Video finished playing");

        // Smooth transition: fade out video while stopping it
        if (videoDisplay != null)
        {
            yield return StartCoroutine(FadeOutVideo());
        }

        // Stop and hide video
        videoPlayer.Stop();
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }
    }

    // New method for smooth video fade out
    private IEnumerator FadeOutVideo()
    {
        if (videoDisplay == null) yield break;

        float fadeTime = 1f;
        float elapsedTime = 0f;
        Color startColor = videoDisplay.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            videoDisplay.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            yield return null;
        }

        // Reset color for next use
        videoDisplay.color = startColor;
    }

    private IEnumerator PlaySpriteAnimation()
    {
        Image animationImage = transitionCanvas.GetComponentInChildren<Image>();
        if (animationImage == null)
        {
            Debug.LogError("No Image component found in transition canvas for sprite animation");
            yield break;
        }

        // Play sprite animation
        foreach (Sprite sprite in sleepAnimationSprites)
        {
            animationImage.sprite = sprite;
            yield return new WaitForSeconds(spriteAnimationSpeed);
        }

        // Fade out
        float fadeTime = 1f;
        float elapsedTime = 0f;
        Color startColor = animationImage.color;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            animationImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
    }

    private IEnumerator SimpleFadeTransition()
    {
        Debug.Log("Starting simple fade transition");
        // Fade to black
        yield return StartCoroutine(FadeToBlack());

        // Wait a moment
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator FadeToBlack()
    {
        if (fadePanel == null)
        {
            Debug.LogError("Fade panel is null! Please assign it in the inspector.");
            yield break;
        }

        Debug.Log("Fading to black");

        float fadeTime = 1f;
        float elapsedTime = 0f;
        Color startColor = new Color(0, 0, 0, 0);
        Color endColor = new Color(0, 0, 0, 1);

        fadePanel.gameObject.SetActive(true);

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            yield return null;
        }

        Debug.Log("Fade to black complete");
    }

    public IEnumerator FadeFromBlack()
    {
        if (fadePanel == null) yield break;

        float fadeTime = 1f;
        float elapsedTime = 0f;
        Color startColor = new Color(0, 0, 0, 1);
        Color endColor = new Color(0, 0, 0, 0);

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, elapsedTime / fadeTime);
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
        if (transitionCanvas != null)
        {
            transitionCanvas.SetActive(false);
        }
    }

    private void LoadNextDayScene()
    {
        currentDay++;

        if (currentDay > maxDays)
        {
            Debug.Log("Game completed!");
            // Game completed, load ending scene or restart
            SceneManager.LoadScene(0); // Go back to first scene or create a game complete scene
        }
        else
        {
            string nextSceneName = "Day" + currentDay;
            Debug.Log($"Loading scene: {nextSceneName}");

            // Preload the scene for smoother transition
            StartCoroutine(LoadSceneAsync(nextSceneName));
        }
    }

    // New method for async scene loading
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Check if scene exists in build settings
        bool sceneExists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameInBuild = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameInBuild == sceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"Scene '{sceneName}' not found in build settings!");
            yield break;
        }

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait for the scene to be 90% loaded
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("Scene loaded, activating...");

        // Activate the scene
        asyncLoad.allowSceneActivation = true;

        // Wait for scene to be fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("Scene activation complete");
    }

    private void DisablePlayerMovement()
    {
        // Disable player movement during transition
        Movement playerMovement = FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Also disable player script
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        // Re-enable player movement after transition
        Movement playerMovement = FindObjectOfType<Movement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Also enable player script
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.enabled = true;
        }
    }

    private void Start()
    {
        EnablePlayerMovement();

        // If we just loaded a new scene and there's a fade panel, fade from black
        if (fadePanel != null && fadePanel.color.a > 0.5f)
        {
            StartCoroutine(FadeFromBlack());
        }
    }

    // Public methods for external use
    public int GetCurrentDay() => currentDay;
    public void SetCurrentDay(int day) => currentDay = day;
}
