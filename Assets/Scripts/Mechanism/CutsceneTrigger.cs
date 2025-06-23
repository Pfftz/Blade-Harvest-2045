using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool isUsable = true; // Status apakah trigger ini bisa digunakan/aktif
    [SerializeField] private float interactionRange = 2f; // Tetap ada

    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer triggerSprite;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.2f;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactPopup;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private string interactMessage = "Press E to Interact";

    [Header("Cutscene UI")]
    [SerializeField] private GameObject cutsceneUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private GameObject continuePrompt;
    [SerializeField] private float textSpeed = 0.05f; // Kecepatan ketik teks

    // State internal CutsceneTrigger
    private Color originalColor;
    private bool isHighlighted = false;
    private bool isPlayerNearby = false;
    private bool isCutsceneActive = false; // Status apakah dialog sedang aktif
    private bool isTyping = false; // Status apakah teks sedang diketik
    private int currentLineIndex = 0;
    private Transform player;
    private Coroutine typingCoroutine;

    // Tambahan: Variabel untuk callback ke objek yang memicu dialog
    private GameObject callbackTargetObject;
    // Data dialog sementara yang diisi dari StartDialogue()
    private string[] currentDialogueLines;
    private string currentSpeakerName;

    // --- Unity Lifecycle Methods ---
    private void Awake()
    {
        try
        {
            if (!gameObject.CompareTag("CutsceneTrigger"))
            {
                gameObject.tag = "CutsceneTrigger"; // Memastikan tag terpasang
            }
        }
        catch (UnityException)
        {
            Debug.LogWarning("CutsceneTrigger tag not found. Please create it in Project Settings > Tags and Layers");
            gameObject.tag = "Untagged"; // Fallback
        }

        if (triggerSprite == null)
        {
            triggerSprite = GetComponent<SpriteRenderer>();
        }

        if (triggerSprite != null)
        {
            originalColor = triggerSprite.color;
        }

        SetupInteractPopup();
        SetupCutsceneUI();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (interactPopup != null)
        {
            interactPopup.SetActive(false);
        }

        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (isCutsceneActive)
        {
            HandleCutsceneInput();
            return; // Penting: Jangan lakukan pengecekan jarak/interaksi saat cutscene aktif
        }

        // isUsable akan dikelola oleh ShopInteractable, jadi CutsceneTrigger ini hanya perlu memeriksa isUsable dari dirinya sendiri
        if (player != null && isUsable)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool shouldShowPopup = distance <= interactionRange;

            if (shouldShowPopup != isPlayerNearby)
            {
                isPlayerNearby = shouldShowPopup;
                SetHighlight(shouldShowPopup);
                ShowInteractPopup(shouldShowPopup);
            }
        }
        else if (!isUsable) // Jika CutsceneTrigger ini tidak lagi usable, pastikan popup dan highlight hilang
        {
            SetHighlight(false);
            ShowInteractPopup(false);
        }
    }

    // --- UI Setup Methods (Programmatic Fallback) ---
    private void SetupInteractPopup()
    {
        if (interactPopup == null)
        {
            interactPopup = new GameObject("InteractPopup");
            interactPopup.transform.SetParent(transform);
            interactPopup.transform.localPosition = Vector3.up * 1.5f;

            Canvas canvas = interactPopup.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 10;
            interactPopup.transform.localScale = Vector3.one * 0.01f;

            GameObject textGO = new GameObject("InteractText");
            textGO.transform.SetParent(interactPopup.transform);
            textGO.transform.localPosition = Vector3.zero;

            interactText = textGO.AddComponent<TextMeshProUGUI>();
            interactText.text = interactMessage;
            interactText.fontSize = 24;
            interactText.color = Color.white;
            interactText.alignment = TextAlignmentOptions.Center;

            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);
        }

        if (interactText != null)
        {
            interactText.text = interactMessage;
        }
    }

    private void SetupCutsceneUI()
    {
        // Tetap seperti sebelumnya, asumsikan UI diatur di Inspector jika tidak dibuat programatis
    }

    // --- Interaction & Highlight Methods ---
    private void ShowInteractPopup(bool show)
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(show);
        }
    }

    private void SetHighlight(bool highlight)
    {
        if (!isUsable || triggerSprite == null) return;

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

    // --- Cutscene/Dialogue Logic ---
    private void HandleCutsceneInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueText.text = currentDialogueLines[currentLineIndex];
                isTyping = false;
                if (continuePrompt != null) continuePrompt.SetActive(true);
            }
            else
            {
                NextDialogueLine();
            }
        }
    }

    public void StartDialogue(string[] lines, string speaker, GameObject callbackObj)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines provided to CutsceneTrigger's StartDialogue!");
            EndCutscene(); // Jika tidak ada dialog, langsung panggil EndCutscene
            return;
        }

        currentDialogueLines = lines;
        currentSpeakerName = speaker;
        callbackTargetObject = callbackObj;

        ShowInteractPopup(false);
        SetHighlight(false);

        Time.timeScale = 0f;
        isCutsceneActive = true;
        currentLineIndex = 0;

        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = currentSpeakerName;
        }

        StartTyping(currentDialogueLines[currentLineIndex]);

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
        if (continuePrompt != null) continuePrompt.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(text));
    }
    private System.Collections.IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        Debug.Log($"CutsceneTrigger: Typing text: '{text}'"); int charCount = 0;
        foreach (char character in text)
        {
            dialogueText.text += character;

            charCount++;
            yield return new WaitForSecondsRealtime(textSpeed);
        }

        isTyping = false;
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);

            // Add animation to continue prompt
            LeanTween.cancel(continuePrompt);
            LeanTween.scale(continuePrompt, new Vector3(1.2f, 1.2f, 1.2f), 0.5f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong(-1)
                .setIgnoreTimeScale(true);

            Debug.Log("CutsceneTrigger: Continue prompt activated with animation");
        }
    }

    private void NextDialogueLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogueLines.Length)
        {
            EndCutscene();
        }
        else
        {
            StartTyping(currentDialogueLines[currentLineIndex]);
        }
    }

    public void EndCutscene()
    {
        if (cutsceneUI != null)
        {
            cutsceneUI.SetActive(false);
        }

        Time.timeScale = 1f;
        isCutsceneActive = false;

        if (player != null)
        {
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        if (callbackTargetObject != null)
        {
            callbackTargetObject.SendMessage("OnCutsceneEnd", SendMessageOptions.DontRequireReceiver);
            callbackTargetObject = null;
        }
    }

    // --- Public Utility Methods for ShopInteractable ---
    public void SetCallbackObject(GameObject obj)
    {
        callbackTargetObject = obj;
    }

    // Metode untuk mengatur 'isUsable' dari luar (misal oleh ShopInteractable)
    public void SetUsableStatus(bool usable)
    {
        isUsable = usable;
        if (!isUsable && isPlayerNearby)
        {
            SetHighlight(false);
            ShowInteractPopup(false);
        }
    }

    public void ResetTrigger()
    {
        SetUsableStatus(true); // Mengatur kembali ke usable
        currentLineIndex = 0;
        isTyping = false;
        isCutsceneActive = false;
        isPlayerNearby = false;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = "";
        if (cutsceneUI != null) cutsceneUI.SetActive(false);
        if (interactPopup != null) interactPopup.SetActive(false);
        SetHighlight(false);
    }

    // --- Properties ---
    public bool IsPlayerNearby => isPlayerNearby;
    public bool IsUsableInternal => isUsable; // Properti internal untuk CutsceneTrigger itu sendiri
    public bool IsHighlighted => isHighlighted;
    public bool IsCutsceneActive => isCutsceneActive;
}