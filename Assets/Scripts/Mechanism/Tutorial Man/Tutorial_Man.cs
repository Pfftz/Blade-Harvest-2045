using UnityEngine;
using System.Collections; // Tambahkan ini untuk IEnumerator

public class Tutorial_Man : MonoBehaviour
{
    [Header("Speaker Settings")]
    [SerializeField] private string speakerName = "Tutorial Man";

    [Header("First Interaction Dialogue")]
    [SerializeField] private string[] introDialogueLines;

    [Header("Random Tips")]
    [SerializeField] private string[] farmingTips;
    [SerializeField] private string[] combatTips;
    [SerializeField] private string[] generalTips;

    [Header("Settings")]
    [SerializeField] private bool showRandomTipsAfterIntro = true;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private float interactionCooldown = 0.5f; // Cooldown baru

    private CutsceneTrigger cutsceneTrigger;
    private bool hasShownIntroDialogue = false;
    private bool isOnCooldown = false; // Flag baru untuk cooldown

    private void Awake()
    {
        cutsceneTrigger = GetComponent<CutsceneTrigger>();
        if (cutsceneTrigger == null)
        {
            Debug.LogError("No CutsceneTrigger component found on " + gameObject.name);
        }
    }

    private void Start()
    {
        // FOR TESTING
        // PlayerPrefs.DeleteKey("TutorialMan_IntroShown"); // Komen atau hapus ini untuk build final

        // Check if we've shown the intro dialogue before
        hasShownIntroDialogue = PlayerPrefs.GetInt("TutorialMan_IntroShown", 0) == 1;
        Debug.Log($"Tutorial_Man: hasShownIntroDialogue = {hasShownIntroDialogue}");

        // Set cutscene trigger usability
        if (cutsceneTrigger != null)
        {
            cutsceneTrigger.SetUsableStatus(true);
        }
    }

    private void Update()
    {
        // Pastikan tidak dalam cooldown dan tidak ada cutscene aktif
        if (cutsceneTrigger != null &&
            cutsceneTrigger.IsPlayerNearby &&
            Input.GetKeyDown(interactionKey) &&
            !cutsceneTrigger.IsCutsceneActive &&
            !isOnCooldown) // Cek cooldown di sini
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // Aktifkan cooldown segera setelah interaksi dipicu
        StartCoroutine(StartCooldown());

        string[] dialogueToShow;

        Debug.Log($"Tutorial_Man: Interaction started, hasShownIntroDialogue: {hasShownIntroDialogue}");

        // Cek apakah ini interaksi pertama
        if (!hasShownIntroDialogue)
        {
            Debug.Log("Tutorial_Man: Using introduction dialogue lines");
            dialogueToShow = introDialogueLines;
            hasShownIntroDialogue = true;

            // Simpan status bahwa intro sudah ditampilkan
            PlayerPrefs.SetInt("TutorialMan_IntroShown", 1);
            PlayerPrefs.Save();
        }
        else if (showRandomTipsAfterIntro)
        {
            Debug.Log("Tutorial_Man: Using random tips");
            // GetRandomTip() sudah mengembalikan array berisi satu string, ini sudah benar
            dialogueToShow = GetRandomTip();
        }
        else
        {
            Debug.Log("Tutorial_Man: No dialogue to show after intro");
            return;
        }

        // Jangan memulai jika tidak ada baris dialog
        if (dialogueToShow == null || dialogueToShow.Length == 0)
        {
            Debug.LogWarning("Tutorial_Man: No dialogue lines available to show");
            return;
        }

        // Mulai dialog
        cutsceneTrigger.StartDialogue(dialogueToShow, speakerName, gameObject);
    }

    private string[] GetRandomTip()
    {
        // Choose which type of tip to show
        string[] selectedTipPool;
        float randomValue = Random.value;

        if (randomValue < 0.33f && farmingTips != null && farmingTips.Length > 0)
            selectedTipPool = farmingTips;
        else if (randomValue < 0.66f && combatTips != null && combatTips.Length > 0)
            selectedTipPool = combatTips;
        else if (generalTips != null && generalTips.Length > 0)
            selectedTipPool = generalTips;
        else
            return new string[] { "Remember to save your game regularly!" };

        // Select a single random tip
        int randomIndex = Random.Range(0, selectedTipPool.Length);
        return new string[] { selectedTipPool[randomIndex] };
    }

    // Coroutine untuk mengelola cooldown interaksi
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        isOnCooldown = false;
        Debug.Log("Tutorial_Man: Cooldown ended.");
    }

    // Callback ketika dialog berakhir (dipanggil dari CutsceneTrigger)
    public void OnCutsceneEnd()
    {
        Debug.Log("Tutorial dialogue ended");
        // Tambahkan tindakan setelah dialog berakhir di sini
        // Misalnya, jika ada animasi karakter yang perlu berhenti berbicara
    }
}