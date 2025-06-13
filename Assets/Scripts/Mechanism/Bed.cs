using UnityEngine;
using TMPro;

public class Bed : MonoBehaviour
{
    [Header("Bed Settings")]
    [SerializeField] private Transform sleepPosition;
    [SerializeField] private Vector2 sleepDirection = Vector2.up;
    [SerializeField] private bool isUsable = true;
    [SerializeField] private float interactionRange = 2f;

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer bedSprite;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.2f;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactPopup;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private string interactMessage = "Press E to Sleep";

    // State
    private Color originalColor;
    private bool isHighlighted = false;
    private bool isPlayerNearby = false;
    private Transform player;

    private void Awake()
    {
        // Set the bed tag
        if (!gameObject.CompareTag("Bed"))
        {
            gameObject.tag = "Bed";
        }

        // Get bed sprite renderer if not assigned
        if (bedSprite == null)
        {
            bedSprite = GetComponent<SpriteRenderer>();
        }

        // Store original color
        if (bedSprite != null)
        {
            originalColor = bedSprite.color;
        }

        // Set sleep position to bed center if not assigned
        if (sleepPosition == null)
        {
            GameObject sleepPosGO = new GameObject("SleepPosition");
            sleepPosGO.transform.SetParent(transform);
            sleepPosGO.transform.localPosition = Vector3.zero;
            sleepPosition = sleepPosGO.transform;
        }

        // Setup interact popup
        SetupInteractPopup();
    }

    private void Start()
    {
        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Hide popup initially
        if (interactPopup != null)
        {
            interactPopup.SetActive(false);
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
            interactPopup.transform.localPosition = Vector3.up * 1.5f; // Position above bed

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

    private void Update()
    {
        // Check distance to player
        if (player != null)
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
                StartSleep();
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
        if (!isUsable || bedSprite == null) return;

        isHighlighted = highlight;

        if (highlight)
        {
            bedSprite.color = originalColor * highlightColor * highlightIntensity;
        }
        else
        {
            bedSprite.color = originalColor;
        }
    }
    private void StartSleep()
    {
        // Hide popup immediately
        ShowInteractPopup(false);

        // Restore player's stamina when sleeping
        if (player != null)
        {
            Player playerComponent = player.GetComponent<Player>();
            if (playerComponent != null && playerComponent.StaminaManager != null)
            {
                playerComponent.StaminaManager.RestoreFullStamina();
                Debug.Log("Stamina fully restored from sleeping!");
            }
        }

        // Save the game progress (integrate with save system)
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPlayerSleep();
            Debug.Log("Game saved on sleep - day advanced!");
        }
        else
        {
            Debug.LogError("GameManager instance not found for saving!");
        }

        // Trigger the sleep transition
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.StartSleepTransition();
        }
        else
        {
            Debug.LogError("SceneTransitionManager not found!");
        }
    }

    public Vector3 GetSleepPosition()
    {
        return sleepPosition != null ? sleepPosition.position : transform.position;
    }

    public Vector2 GetSleepDirection()
    {
        return sleepDirection.normalized;
    }

    public bool CanUse()
    {
        return isUsable && isPlayerNearby;
    }

    public void SetUsable(bool usable)
    {
        isUsable = usable;

        if (!usable)
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

    // Properties
    public bool IsPlayerNearby => isPlayerNearby;
    public bool IsUsable => isUsable;
    public bool IsHighlighted => isHighlighted;

    // Gizmos for debugging interaction range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
