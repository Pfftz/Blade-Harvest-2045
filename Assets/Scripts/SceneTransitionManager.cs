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

    // Store names to find references in new scenes
    private string transitionCanvasName = "Transition_Canvas";
    private string fadeGroupName = "FadeGroup";
    private string videoDisplayName = "VideoDisplay";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Store the names of UI objects for later reference finding
        if (transitionCanvas != null)
            transitionCanvasName = transitionCanvas.name;

        // Setup video player if video is assigned
        if (sleepTransitionVideo != null)
        {
            SetupVideoPlayer();
        }

        // Ensure transition canvas is initially hidden
        if (transitionCanvas != null)
            transitionCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find UI references in the new scene
        StartCoroutine(RefreshUIReferences());
    }

    private IEnumerator RefreshUIReferences()
    {
        // Wait a frame to ensure all scene objects are loaded
        yield return null;

        // Find transition canvas by name
        if (transitionCanvas == null || !transitionCanvas.activeInHierarchy)
        {
            GameObject foundCanvas = GameObject.Find(transitionCanvasName);
            if (foundCanvas == null)
            {
                // Try alternative names
                foundCanvas = GameObject.Find("Transition_Canvas") ??
                             GameObject.Find("TransitionCanvas") ??
                             GameObject.Find("Transition Canvas");
            }

            if (foundCanvas != null)
            {
                transitionCanvas = foundCanvas;
                Debug.Log($"Found transition canvas: {transitionCanvas.name}");
            }
            else
            {
                Debug.LogWarning("Transition canvas not found in scene! Looking for any canvas with 'Transition' in name...");
                // Last resort: find any canvas with "Transition" in the name
                Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in allCanvases)
                {
                    if (canvas.name.ToLower().Contains("transition"))
                    {
                        transitionCanvas = canvas.gameObject;
                        Debug.Log($"Found transition canvas by search: {transitionCanvas.name}");
                        break;
                    }
                }
            }
        }

        // Find fade panel and video display within the transition canvas
        if (transitionCanvas != null)
        {
            // Find fade panel - look for Image components that might be fade panels
            Image[] images = transitionCanvas.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                // Look for fade panel by name or by being a solid black/dark image
                if (img.name.ToLower().Contains("fade") ||
                    img.name.ToLower().Contains("black") ||
                    img.name.ToLower().Contains("panel"))
                {
                    fadePanel = img;
                    Debug.Log($"Found fade panel: {fadePanel.name}");
                    break;
                }
            }

            // If not found by name, look for an Image with black color
            if (fadePanel == null)
            {
                foreach (Image img in images)
                {
                    if (img.color == Color.black ||
                        (img.color.r < 0.1f && img.color.g < 0.1f && img.color.b < 0.1f))
                    {
                        fadePanel = img;
                        Debug.Log($"Found fade panel by color: {fadePanel.name}");
                        break;
                    }
                }
            }

            // Find video display - look for RawImage components
            RawImage[] rawImages = transitionCanvas.GetComponentsInChildren<RawImage>(true);
            foreach (RawImage rawImg in rawImages)
            {
                if (rawImg.name.ToLower().Contains("video") ||
                    rawImg.name.ToLower().Contains("display"))
                {
                    videoDisplay = rawImg;
                    Debug.Log($"Found video display: {videoDisplay.name}");
                    break;
                }
            }

            // If not found by name, just use the first RawImage
            if (videoDisplay == null && rawImages.Length > 0)
            {
                videoDisplay = rawImages[0];
                Debug.Log($"Using first RawImage as video display: {videoDisplay.name}");
            }

            // Ensure transition canvas is initially hidden
            transitionCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Could not find transition canvas in the scene! Make sure it exists and is named correctly.");
        }

        // Re-setup video player with new references
        if (sleepTransitionVideo != null)
        {
            SetupVideoPlayer();
        }
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

        // Audio settings
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.SetDirectAudioMute(0, false);

        // Create render texture if needed
        if (videoPlayer.targetTexture == null)
        {
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.useMipMap = false; // Disable mipmaps for better performance
            videoPlayer.targetTexture = renderTexture;
        }

        // Assign render texture to video display if available
        if (videoDisplay != null && videoPlayer.targetTexture != null)
        {
            videoDisplay.texture = videoPlayer.targetTexture;
            Debug.Log("Video player setup complete with video display");
        }
        else
        {
            Debug.LogWarning("Video display not available for video player setup");
        }
    }

    public void StartSleepTransition()
    {
        Debug.Log("StartSleepTransition called");

        // Ensure we have valid references before starting transition
        if (transitionCanvas == null)
        {
            Debug.LogError("Cannot start transition - transition canvas is missing!");
            StartCoroutine(RefreshUIReferences());
            StartCoroutine(DelayedStartTransition());
            return;
        }

        StartCoroutine(SleepTransitionCoroutine());
    }

    private IEnumerator DelayedStartTransition()
    {
        // Wait for UI references to be refreshed
        yield return new WaitForSeconds(0.5f);

        if (transitionCanvas != null)
        {
            StartCoroutine(SleepTransitionCoroutine());
        }
        else
        {
            Debug.LogError("Still no transition canvas found after refresh!");
        }
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
            Debug.LogError("Transition canvas is null! Cannot proceed with transition.");
            yield break;
        }

        // Disable player movement
        DisablePlayerMovement();

        // Choose transition type (video or sprite animation)
        if (sleepTransitionVideo != null && videoPlayer != null && videoDisplay != null)
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
        else
        {
            Debug.LogError("Video display is null! Cannot play video transition.");
            yield break;
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
            Debug.LogError("Fade panel is null! Please assign it in the inspector or ensure it exists in the scene.");
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
        // Save current scene data before switching - with null checks
        if (GameManager.instance != null)
        {
            // Save tile data
            if (GameManager.instance.tileManager != null)
            {
                GameManager.instance.tileManager.SaveTileDataForCurrentScene();
            }

            // Save player inventory
            if (GameManager.instance.player != null)
            {
                GameManager.instance.player.SaveInventoryData();
            }
        }

        currentDay++;

        if (currentDay > maxDays)
        {
            Debug.Log("Game completed!");
            SceneManager.LoadScene(0);
        }
        else
        {
            string nextSceneName = "Day" + currentDay;
            Debug.Log($"Loading scene: {nextSceneName}");
            StartCoroutine(LoadSceneAsync(nextSceneName));
        }
    }

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

        // Delay GameManager reference refresh to avoid null reference errors
        StartCoroutine(DelayedGameManagerRefresh());

        // If we just loaded a new scene and there's a fade panel, fade from black
        if (fadePanel != null && fadePanel.color.a > 0.5f)
        {
            StartCoroutine(FadeFromBlack());
        }
    }

    private IEnumerator DelayedGameManagerRefresh()
    {
        // Wait to ensure all objects are properly initialized
        yield return new WaitForSeconds(0.3f);

        // Refresh GameManager references after scene load
        if (GameManager.instance != null)
        {
            GameManager.instance.RefreshManagerReferences();
        }
    }

    // Public methods for external use
    public int GetCurrentDay() => currentDay;
    public void SetCurrentDay(int day) => currentDay = day;
}
