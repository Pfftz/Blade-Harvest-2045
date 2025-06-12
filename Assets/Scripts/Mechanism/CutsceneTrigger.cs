using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool isUsable = true;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool hasBeenUsed = false;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.2f;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactPopup;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private string interactMessage = "Press E to Interact";

    [Header("Cutscene Settings")]
    [SerializeField] private string[] dialogueLines;
    [SerializeField] private string speakerName = "";
    [SerializeField] private float textSpeed = 0.05f;

    [Header("Cutscene UI")]
    [SerializeField] private GameObject cutsceneUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private GameObject continuePrompt;

    // State
    private Color originalColor;
    private bool isHighlighted = false;
    private bool isPlayerNearby = false;
    private bool isCutsceneActive = false;
    private bool isTyping = false;
    private int currentLineIndex = 0;
    private Transform player;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        // Set the trigger tag
        if (!gameObject.CompareTag("CutsceneTrigger"))
        {
            gameObject.tag = "CutsceneTrigger";
        }

        // Get sprite renderer if not assigned
        if (triggerSprite == null)
        {
            triggerSprite = GetComponent<SpriteRenderer>();
        }

        // Store original color
        if (triggerSprite != null)
        {
            originalColor = triggerSprite.color;
        }

        // Setup interact popup
        SetupInteractPopup();
        
        // Setup cutscene UI
        SetupCutsceneUI();
    }

    private void Start()
    {
        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Hide UI initially
        if (interactPopup != null)
        {
            interactPopup.SetActive(false);
        }
        
        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(false);
        }
    }

    private void SetupInteractPopup()
    {
        // Create interact popup if not assigned
        if (interactPopup == null)
        {
            // Create popup GameObject
            interactPopup = new GameObject("InteractPopup");
            interactPopup.transform.SetParent(transform);
            interactPopup.transform.localPosition = Vector3.up * 1.5f;

            // Add Canvas component for world space UI
            Canvas canvas = interactPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;

            // Scale down the canvas
            interactPopup.transform.localScale = Vector3.one * 0.01f;

            // Create text GameObject
            GameObject textGO = new GameObject("InteractText");
            textGO.transform.SetParent(interactPopup.transform);
            textGO.transform.localPosition = Vector3.zero;

            // Add TextMeshPro component
            interactText = textGO.AddComponent<TextMeshProUGUI>();
            interactText.text = interactMessage;
            interactText.fontSize = 24;
            interactText.color = Color.white;
            interactText.alignment = TextAlignmentOptions.Center;

            // Setup RectTransform
            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
        }

        // Set text if component exists
        if (interactText != null)
        {
            interactText.text = interactMessage;
        }
    }

    private void SetupCutsceneUI()
    {
        // Create cutscene UI if not assigned
        if (cutsceneUI == null)
        {
            // Create main cutscene UI GameObject
            cutsceneUI = new GameObject("CutsceneUI");
            cutsceneUI.transform.SetParent(transform);

            // Add Canvas component for screen space UI
            Canvas canvas = cutsceneUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // Add CanvasScaler
            CanvasScaler scaler = cutsceneUI.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Create dialogue panel
            GameObject dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(cutsceneUI.transform);
            
            RectTransform panelRect = dialoguePanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.3f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Add background to panel
            Image panelBg = dialoguePanel.AddComponent<Image>();
            panelBg.color = new Color(0, 0, 0, 0.8f);

            // Create speaker name text
            GameObject speakerGO = new GameObject("SpeakerText");
            speakerGO.transform.SetParent(dialoguePanel.transform);
            
            RectTransform speakerRect = speakerGO.AddComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0.05f, 0.7f);
            speakerRect.anchorMax = new Vector2(0.95f, 0.95f);
            speakerRect.offsetMin = Vector2.zero;
            speakerRect.offsetMax = Vector2.zero;

            speakerText = speakerGO.AddComponent<TextMeshProUGUI>();
            speakerText.text = speakerName;
            speakerText.fontSize = 24;
            speakerText.color = Color.yellow;
            speakerText.fontStyle = FontStyles.Bold;

            // Create dialogue text
            GameObject dialogueGO = new GameObject("DialogueText");
            dialogueGO.transform.SetParent(dialoguePanel.transform);
            
            RectTransform dialogueRect = dialogueGO.AddComponent<RectTransform>();
            dialogueRect.anchorMin = new Vector2(0.05f, 0.1f);
            dialogueRect.anchorMax = new Vector2(0.95f, 0.65f);
            dialogueRect.offsetMin = Vector2.zero;
            dialogueRect.offsetMax = Vector2.zero;

            dialogueText = dialogueGO.AddComponent<TextMeshProUGUI>();
            dialogueText.text = "";
            dialogueText.fontSize = 20;
            dialogueText.color = Color.white;

            // Create continue prompt
            continuePrompt = new GameObject("ContinuePrompt");
            continuePrompt.transform.SetParent(dialoguePanel.transform);
            
            RectTransform promptRect = continuePrompt.AddComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.8f, 0.05f);
            promptRect.anchorMax = new Vector2(0.95f, 0.25f);
            promptRect.offsetMin = Vector2.zero;
            promptRect.offsetMax = Vector2.zero;

            TextMeshProUGUI promptText = continuePrompt.AddComponent<TextMeshProUGUI>();
            promptText.text = "Press E";
            promptText.fontSize = 16;
            promptText.color = Color.gray;
            promptText.alignment = TextAlignmentOptions.Center;
        }
    }

    private void Update()
    {
        if (isCutsceneActive)
        {
            HandleCutsceneInput();
            return;
        }

        // Check distance to player
        if (player != null && !hasBeenUsed)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool shouldShowPopup = distance <= interactionRange && isUsable;

            // Update player nearby status
            if (shouldShowPopup != isPlayerNearby)
            {
                isPlayerNearby = shouldShowPopup;
                SetHighlight(shouldShowPopup);
                ShowInteractPopup(shouldShowPopup);
            }

            // Check for interaction input when player is nearby
            if (isPlayerNearby && isUsable && Input.GetKeyDown(KeyCode.E))
            {
                StartCutscene();
            }
        }
    }

    private void HandleCutsceneInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Skip typing animation
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueText.text = dialogueLines[currentLineIndex];
                isTyping = false;
                continuePrompt.SetActive(true);
            }
            else
            {
                // Move to next line or end cutscene
                NextDialogueLine();
            }
        }
    }

    private void ShowInteractPopup(bool show)
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(show);
        }
    }

    private void SetHighlight(bool highlight)
    {
        if (!isUsable || triggerSprite == null || hasBeenUsed) return;

        isHighlighted = highlight;

        if (highlight)
        {
            triggerSprite.color = originalColor * highlightColor * highlightIntensity;
        }
        else
        {
            triggerSprite.color = originalColor;
        }
    }

    private void StartCutscene()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines assigned to cutscene trigger!");
            return;
        }

        // Hide interact popup
        ShowInteractPopup(false);
        SetHighlight(false);

        // Freeze the game
        Time.timeScale = 0f;
        isCutsceneActive = true;
        currentLineIndex = 0;

        // Show cutscene UI
        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(true);
        }

        // Set speaker name
        if (speakerText != null)
        {
            speakerText.text = speakerName;
        }

        // Start first dialogue line
        StartTyping(dialogueLines[currentLineIndex]);

        // Disable player movement if possible
        if (player != null)
        {
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
    }

    private void StartTyping(string text)
    {
        isTyping = true;
        continuePrompt.SetActive(false);
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private System.Collections.IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        
        foreach (char character in text)
        {
            dialogueText.text += character;
            yield return new WaitForSecondsRealtime(textSpeed);
        }
        
        isTyping = false;
        continuePrompt.SetActive(true);
    }

    private void NextDialogueLine()
    {
        currentLineIndex++;
        
        if (currentLineIndex >= dialogueLines.Length)
        {
            EndCutscene();
        }
        else
        {
            StartTyping(dialogueLines[currentLineIndex]);
        }
    }

    private void EndCutscene()
    {
        // Hide cutscene UI
        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(false);
        }

        // Unfreeze the game
        Time.timeScale = 1f;
        isCutsceneActive = false;

        // Re-enable player movement
        if (player != null)
        {
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        // Mark as used if one-time use
        if (oneTimeUse)
        {
            hasBeenUsed = true;
            SetUsable(false);
        }

        // Reset player nearby status
        isPlayerNearby = false;
    }

    public void SetUsable(bool usable)
    {
        isUsable = usable;

        if (!usable || hasBeenUsed)
        {
            SetHighlight(false);
            ShowInteractPopup(false);
        }
        else if (isPlayerNearby)
        {
            SetHighlight(true);
            ShowInteractPopup(true);
        }
    }

    public void ResetTrigger()
    {
        hasBeenUsed = false;
        SetUsable(true);
    }

    // Properties
    public bool IsPlayerNearby => isPlayerNearby;
    public bool IsUsable => isUsable && !hasBeenUsed;
    public bool IsHighlighted => isHighlighted;
    public bool IsCutsceneActive => isCutsceneActive;
    public bool HasBeenUsed => hasBeenUsed;

    // Gizmos for debugging interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}