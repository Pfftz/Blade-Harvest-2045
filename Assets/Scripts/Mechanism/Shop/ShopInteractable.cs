using UnityEngine;
using TMPro;
using System.Collections;

public class ShopInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string interactMessage = "Press E to Open Shop";
    [SerializeField] private string exitMessage = "Press E to Close Shop";
    [SerializeField] private float interactionCooldown = 0.5f;

    [Header("UI Settings")]
    [SerializeField] private GameObject interactPopup;
    [SerializeField] private TextMeshProUGUI interactText;

    [Header("Shop References")]
    [SerializeField] private Shop_UI shopUI;

    [Header("Shop Mode for this NPC")]
    [SerializeField] private Shop_UI.ShopMode npcShopMode;

    [Header("Dialogue Settings")]
    [SerializeField] private CutsceneTrigger cutsceneTrigger;
    [SerializeField] private string[] dialogueLinesDay1;
    [SerializeField] private string speakerNameDay1;
    [SerializeField] private string[] dialogueLinesRepeat;
    [SerializeField] private string speakerNameRepeat;

    // Status apakah tutorial toko untuk mode ini sudah selesai (IDEALNYA DARI SAVE GAME)
    private static bool hasCompletedShopTutorialDay1Buy = false;
    private static bool hasCompletedShopTutorialDay1Sell = false;

    private bool isPlayerInRange = false;
    private bool isShopOpen = false;
    private bool isOnCooldown = false;

    private void Start()
    {
        if (interactPopup == null || interactText == null)
        {
            SetupInteractPopup(); // Pastikan popup dibuat jika belum ada di Inspector
        }

        // Muat status tutorial dari PlayerPrefs saat Start
        hasCompletedShopTutorialDay1Buy = PlayerPrefs.GetInt("ShopTutorialDay1Buy", 0) == 1;
        hasCompletedShopTutorialDay1Sell = PlayerPrefs.GetInt("ShopTutorialDay1Sell", 0) == 1;
        Debug.Log($"[ShopInteractable-{gameObject.name}] Loaded tutorial status: Buy={hasCompletedShopTutorialDay1Buy}, Sell={hasCompletedShopTutorialDay1Sell}");

        // Inisialisasi teks popup default
        if (interactText != null)
        {
            interactText.text = interactMessage;
        }

        ShowInteractPopup(false); // Pastikan popup tersembunyi di awal

        if (cutsceneTrigger == null)
        {
            Debug.LogWarning("CutsceneTrigger not assigned to " + gameObject.name + ". Dialog will not function.");
        }
        else
        {
            cutsceneTrigger.SetCallbackObject(this.gameObject);
            
            // Logika untuk mengatur isUsable di CutsceneTrigger:
            // CT harus usable jika:
            // 1. Tutorial belum selesai (untuk Day1 dialog)
            // ATAU
            // 2. Tutorial sudah selesai DAN ada dialog repeat (untuk dialog repeat)
            bool shouldCutsceneBeUsableForDialogue = false;
            if (npcShopMode == Shop_UI.ShopMode.Buy)
            {
                shouldCutsceneBeUsableForDialogue = !hasCompletedShopTutorialDay1Buy || (dialogueLinesRepeat != null && dialogueLinesRepeat.Length > 0);
            }
            else // Sell Mode
            {
                shouldCutsceneBeUsableForDialogue = !hasCompletedShopTutorialDay1Sell || (dialogueLinesRepeat != null && dialogueLinesRepeat.Length > 0);
            }
            cutsceneTrigger.SetUsableStatus(shouldCutsceneBeUsableForDialogue);
            Debug.Log($"[ShopInteractable-{gameObject.name}] CT.SetUsableStatus({shouldCutsceneBeUsableForDialogue}). CT.IsUsableInternal: {cutsceneTrigger.IsUsableInternal}");
        }
    }

    private void Update()
    {
        bool isCutsceneCurrentlyActive = (cutsceneTrigger != null && cutsceneTrigger.IsCutsceneActive);

        if (isPlayerInRange && Input.GetKeyDown(interactionKey) && !isOnCooldown)
        {
            StartCoroutine(StartCooldown()); // Mulai cooldown segera

            if (isShopOpen) // Jika toko sudah terbuka
            {
                if (!isCutsceneCurrentlyActive) // Pastikan tidak ada cutscene aktif
                {
                    CloseShop();
                }
            }
            else // Jika toko belum terbuka
            {
                if (!isCutsceneCurrentlyActive) // Pastikan tidak ada cutscene aktif
                {
                    HandleInteraction();
                }
            }
        }

        if (isShopOpen && Input.GetKeyDown(KeyCode.Tab) && !isCutsceneCurrentlyActive)
        {
            CloseShop();
        }
    }

    private void HandleInteraction()
    {
        if (cutsceneTrigger == null)
        {
            Debug.LogWarning("CutsceneTrigger is null. Opening shop directly.");
            OpenShopDirectly();
            return;
        }

        ShowInteractPopup(false); // Sembunyikan popup interaksi saat memulai interaksi

        string[] linesToDisplay;
        string speakerToDisplay;
        bool isDay1ForThisShop;

        if (npcShopMode == Shop_UI.ShopMode.Buy)
        {
            isDay1ForThisShop = !hasCompletedShopTutorialDay1Buy;
        }
        else
        {
            isDay1ForThisShop = !hasCompletedShopTutorialDay1Sell;
        }

        if (isDay1ForThisShop)
        {
            linesToDisplay = dialogueLinesDay1;
            speakerToDisplay = speakerNameDay1;
        }
        else
        {
            // Jika tutorial sudah selesai, gunakan dialog repeat
            if (dialogueLinesRepeat != null && dialogueLinesRepeat.Length > 0)
            {
                int randomIndex = Random.Range(0, dialogueLinesRepeat.Length);
                linesToDisplay = new string[] { dialogueLinesRepeat[randomIndex] };
                speakerToDisplay = speakerNameRepeat;
            }
            else
            {
                // Jika tidak ada dialog repeat, langsung buka toko
                Debug.Log($"[ShopInteractable-{gameObject.name}] No repeat dialogue lines assigned. Opening shop directly.");
                OpenShopDirectly();
                return;
            }
        }

        if (linesToDisplay == null || linesToDisplay.Length == 0)
        {
            Debug.LogWarning($"[ShopInteractable-{gameObject.name}] Dialogue lines are empty. Opening shop directly.");
            OpenShopDirectly();
            return;
        }

        cutsceneTrigger.StartDialogue(linesToDisplay, speakerToDisplay, this.gameObject);
        isShopOpen = true; // Menandakan alur toko/dialog sudah dimulai
    }

    public void OnCutsceneEnd()
    {
        Debug.Log($"[ShopInteractable-{gameObject.name}] OnCutsceneEnd received. isShopOpen: {isShopOpen}");

        if (isShopOpen) // Hanya buka toko jika belum ditutup pemain
        {
            OpenShopDirectly(); // Buka toko setelah dialog selesai

            // Tandai tutorial selesai jika ini adalah Day1 dan belum selesai
            bool isDay1ForThisShop = false;
            if (npcShopMode == Shop_UI.ShopMode.Buy)
            {
                isDay1ForThisShop = !hasCompletedShopTutorialDay1Buy;
            }
            else
            {
                isDay1ForThisShop = !hasCompletedShopTutorialDay1Sell;
            }

            if (isDay1ForThisShop)
            {
                if (npcShopMode == Shop_UI.ShopMode.Buy)
                {
                    hasCompletedShopTutorialDay1Buy = true;
                    PlayerPrefs.SetInt("ShopTutorialDay1Buy", 1);
                }
                else
                {
                    hasCompletedShopTutorialDay1Sell = true;
                    PlayerPrefs.SetInt("ShopTutorialDay1Sell", 1);
                }
                PlayerPrefs.Save();
                Debug.Log($"[ShopInteractable-{gameObject.name}] ShopTutorial for {npcShopMode} completed and saved.");

                // Setelah tutorial selesai, pastikan CutsceneTrigger tetap usable untuk interaksi di masa depan
                // terutama jika ada dialog repeat atau hanya ingin popup muncul.
                if (cutsceneTrigger != null)
                {
                    bool shouldCutsceneBeUsableForDialogue = (dialogueLinesRepeat != null && dialogueLinesRepeat.Length > 0);
                    cutsceneTrigger.SetUsableStatus(shouldCutsceneBeUsableForDialogue);
                    Debug.Log($"[ShopInteractable-{gameObject.name}] CT.SetUsableStatus({shouldCutsceneBeUsableForDialogue}) after tutorial. Current CT.IsUsableInternal: {cutsceneTrigger.IsUsableInternal}");
                }
            }

            // Atur teks popup ke exitMessage jika pemain masih dalam jangkauan dan toko terbuka
            if (isPlayerInRange && interactText != null)
            {
                interactText.text = exitMessage;
                ShowInteractPopup(true); // Pastikan popup terlihat
            }
        }
        else // Jika isShopOpen false (pemain sudah menutup toko/menjauh saat dialog aktif)
        {
            Debug.Log($"[ShopInteractable-{gameObject.name}] Shop was closed during cutscene. Not opening shop directly.");
            if (isPlayerInRange)
            {
                ShowInteractPopup(true); // Tampilkan popup lagi jika pemain masih dalam jangkauan
            }
        }
    }

    private void OpenShopDirectly()
    {
        Debug.Log($"[ShopInteractable-{gameObject.name}] Calling OpenShop on Shop_UI. Mode: {npcShopMode}");
        if (shopUI != null)
        {
            shopUI.OpenShop(npcShopMode);
            if (interactText != null)
            {
                interactText.text = exitMessage;
            }
            isShopOpen = true;
        }
        else
        {
            Debug.LogWarning($"[ShopInteractable-{gameObject.name}] shopUI reference is NULL in OpenShopDirectly!");
        }
    }

    private void CloseShop()
    {
        Debug.Log($"[ShopInteractable-{gameObject.name}] Closing shop. isShopOpen was: {isShopOpen}");
        if (shopUI != null)
        {
            // ... (Kode animasi LeanTween tetap sama) ...
            if (shopUI.UseAnimations)
            {
                LeanTween.scale(shopUI.ShopPanel, new Vector3(0.1f, 0.1f, 0.1f), shopUI.AnimationDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() =>
                    {
                        shopUI.ShopPanel.SetActive(false);
                        if (GameManager.instance?.uiManager != null)
                        {
                            GameManager.instance.uiManager.ShowInventory(false);
                        }
                    });
            }
            else
            {
                shopUI.ShopPanel.SetActive(false);
                if (GameManager.instance?.uiManager != null)
                {
                    GameManager.instance.uiManager.ShowInventory(false);
                }
            }
            
            isShopOpen = false; // Set status toko ke tertutup di sini
            
            if (interactText != null)
            {
                // Teks popup kembali ke pesan interaksi awal jika pemain masih dalam jangkauan
                if (isPlayerInRange) // Hanya set teks jika pemain masih dalam jangkauan
                {
                    // Tampilkan pesan "Open Shop" jika CutsceneTrigger masih bisa memicu dialog (repeat)
                    // atau jika tutorial sudah selesai (artinya langsung buka toko)
                    bool canReopenWithPopup = (cutsceneTrigger != null && cutsceneTrigger.IsUsableInternal) || 
                                              (npcShopMode == Shop_UI.ShopMode.Buy && hasCompletedShopTutorialDay1Buy) || 
                                              (npcShopMode == Shop_UI.ShopMode.Sell && hasCompletedShopTutorialDay1Sell);
                    
                    if (canReopenWithPopup)
                    {
                        interactText.text = interactMessage;
                        ShowInteractPopup(true); // Pastikan popup aktif kembali
                    }
                    else
                    {
                        // Jika tidak lagi usable (misal one-time tutorial tanpa repeat dialog), sembunyikan popup
                        interactText.text = ""; 
                        ShowInteractPopup(false);
                    }
                }
                else // Jika pemain tidak lagi dalam jangkauan, sembunyikan popup sepenuhnya
                {
                    ShowInteractPopup(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Ini akan memuat ulang status dari PlayerPrefs setiap kali player masuk trigger
            // Ini REDUNDAN jika Start() sudah memuatnya, tapi tidak merusak.
            // Sebaiknya pastikan GameManager mengelola PlayerPrefs.Save() dan Load() di awal/akhir hari
            // atau saat save game. Untuk sementara, ini tidak apa-apa.
            hasCompletedShopTutorialDay1Buy = PlayerPrefs.GetInt("ShopTutorialDay1Buy", 0) == 1;
            hasCompletedShopTutorialDay1Sell = PlayerPrefs.GetInt("ShopTutorialDay1Sell", 0) == 1;

            // Logika untuk menampilkan popup:
            // Popup harus muncul jika:
            // 1. Toko tidak terbuka (isShopOpen == false)
            // DAN
            // 2. (CutsceneTrigger tidak aktif DAN CutsceneTrigger bisa digunakan untuk dialog)
            // ATAU (Tutorial untuk mode ini sudah selesai -- artinya langsung buka toko tanpa dialog lagi, tapi popup tetap perlu)
            bool tutorialForThisModeCompleted = (npcShopMode == Shop_UI.ShopMode.Buy && hasCompletedShopTutorialDay1Buy) ||
                                                (npcShopMode == Shop_UI.ShopMode.Sell && hasCompletedShopTutorialDay1Sell);
            
            bool canShowPopup = (!isShopOpen && 
                                 (tutorialForThisModeCompleted || 
                                  (cutsceneTrigger != null && !cutsceneTrigger.IsCutsceneActive && cutsceneTrigger.IsUsableInternal)));

            Debug.Log($"[ShopInteractable-{gameObject.name}-OnTriggerEnter] Player in range. isShopOpen: {isShopOpen}, tutorialCompleted: {tutorialForThisModeCompleted}, CT.IsCutsceneActive: {(cutsceneTrigger != null ? cutsceneTrigger.IsCutsceneActive.ToString() : "N/A")}, CT.IsUsableInternal: {(cutsceneTrigger != null ? cutsceneTrigger.IsUsableInternal.ToString() : "N/A")}. Final canShowPopup: {canShowPopup}");


            if (canShowPopup)
            {
                ShowInteractPopup(true);
                // Selalu set text ke pesan interaksi awal saat masuk range dan toko belum terbuka
                if (interactText != null)
                {
                    interactText.text = interactMessage;
                }
            }
            else
            {
                ShowInteractPopup(false); // Sembunyikan jika tidak memenuhi kondisi
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowInteractPopup(false); // Sembunyikan popup saat pemain menjauh

            if (isShopOpen)
            {
                CloseShop();
            }
            // Paksa cutscene berakhir jika masih aktif saat pemain menjauh
            if (cutsceneTrigger != null && cutsceneTrigger.IsCutsceneActive)
            {
                cutsceneTrigger.EndCutscene();
            }
        }
    }

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

    private void ShowInteractPopup(bool show)
    {
        if (interactPopup != null)
        {
            interactPopup.SetActive(show);
        }
    }

    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        isOnCooldown = false;
        Debug.Log("ShopInteractable: Cooldown ended.");
    }
    
    public static void ResetAllTutorialFlags()
    {
        hasCompletedShopTutorialDay1Buy = false;
        hasCompletedShopTutorialDay1Sell = false;
        PlayerPrefs.DeleteKey("ShopTutorialDay1Buy");
        PlayerPrefs.DeleteKey("ShopTutorialDay1Sell");
        PlayerPrefs.Save(); // Penting: Simpan perubahan setelah menghapus
        Debug.Log("Shop tutorial flags have been reset in PlayerPrefs and static variables.");
    }
}