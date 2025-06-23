using UnityEngine;
using TMPro;
using System.Collections;

public class ShopInteractable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private string interactMessage = "Press E to Open Shop";
    [SerializeField] private string exitMessage = "Press E to Close Shop";
    [SerializeField] private float interactionCooldown = 0.5f; // Cooldown baru

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
    private bool isShopOpen = false; // Status apakah UI toko (panel Buy/Sell) sedang terbuka
    private bool isOnCooldown = false; // Flag baru untuk cooldown

    private void Start()
    {
        if (interactPopup == null || interactText == null)
        {
            SetupInteractPopup();
        }

        if (interactText != null)
        {
            interactText.text = interactMessage;
        }

        ShowInteractPopup(false);

        if (cutsceneTrigger == null)
        {
            Debug.LogWarning("CutsceneTrigger not assigned to " + gameObject.name + ". Dialog will not function.");
        }
        else
        {
            cutsceneTrigger.SetCallbackObject(this.gameObject);
            // Inisialisasi status usable CutsceneTrigger dari sini
            bool shouldBeUsable = true;
            if (npcShopMode == Shop_UI.ShopMode.Buy)
            {
                shouldBeUsable = !hasCompletedShopTutorialDay1Buy;
            }
            else // Sell Mode
            {
                shouldBeUsable = !hasCompletedShopTutorialDay1Sell;
            }
            cutsceneTrigger.SetUsableStatus(shouldBeUsable);
        }

        // Idealnya, muat status tutorial dari save game
        // (Ini hanya contoh untuk test)
        //PlayerPrefs.DeleteKey("ShopTutorialDay1Buy"); // Untuk testing
        //PlayerPrefs.DeleteKey("ShopTutorialDay1Sell"); // Untuk testing
        hasCompletedShopTutorialDay1Buy = PlayerPrefs.GetInt("ShopTutorialDay1Buy", 0) == 1;
        hasCompletedShopTutorialDay1Sell = PlayerPrefs.GetInt("ShopTutorialDay1Sell", 0) == 1;

    }

    private void Update()
    {
        // Menggunakan properti IsCutsceneActive dari CutsceneTrigger
        bool isCutsceneCurrentlyActive = (cutsceneTrigger != null && cutsceneTrigger.IsCutsceneActive);

        // Tambahkan cek isOnCooldown di sini
        if (isPlayerInRange && Input.GetKeyDown(interactionKey) && !isOnCooldown)
        {
            if (!isShopOpen && !isCutsceneCurrentlyActive) // Jika toko tidak terbuka DAN tidak ada cutscene aktif
            {
                StartCoroutine(StartCooldown()); // Mulai cooldown
                HandleInteraction();
            }
            else if (isShopOpen && !isCutsceneCurrentlyActive) // Jika toko sudah terbuka DAN tidak ada cutscene aktif
            {
                StartCoroutine(StartCooldown()); // Mulai cooldown
                CloseShop();
            }
            // Jika ada cutscene aktif, input E akan ditangani oleh CutsceneTrigger itu sendiri untuk melanjutkan dialog.
        }

        // Tutup toko dengan tombol Tab (jika toko terbuka dan tidak ada cutscene aktif)
        if (isShopOpen && Input.GetKeyDown(KeyCode.Tab) && !isCutsceneCurrentlyActive)
        {
            CloseShop();
        }
    }

    private void HandleInteraction() // Metode yang akan memicu dialog atau langsung membuka toko
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
        else // Shop_UI.ShopMode.Sell
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
            if (dialogueLinesRepeat == null || dialogueLinesRepeat.Length == 0)
            {
                Debug.LogWarning("No repeat dialogue lines assigned for " + gameObject.name + ". Opening shop directly.");
                OpenShopDirectly();
                return;
            }
            int randomIndex = Random.Range(0, dialogueLinesRepeat.Length);
            linesToDisplay = new string[] { dialogueLinesRepeat[randomIndex] }; // Pastikan ini selalu 1 tips per interaksi
            speakerToDisplay = speakerNameRepeat;
        }

        if (linesToDisplay == null || linesToDisplay.Length == 0)
        {
            OpenShopDirectly();
            return;
        }

        cutsceneTrigger.StartDialogue(linesToDisplay, speakerToDisplay, this.gameObject);
        isShopOpen = true; // <--- PENTING: Set isShopOpen menjadi TRUE di sini
                            // Ini menandakan bahwa alur toko/dialog sudah dimulai.
    }

    // Metode ini akan dipanggil oleh CutsceneTrigger setelah cutscene/dialog selesai
    public void OnCutsceneEnd() // <--- Callback dari CutsceneTrigger
    {
        Debug.Log("ShopInteractable: OnCutsceneEnd received.");

        if (isShopOpen) // Memeriksa isShopOpen agar tidak membuka toko jika sudah ditutup pemain
        {
            Debug.Log("ShopInteractable: isShopOpen is TRUE. Proceeding to open shop directly.");
            OpenShopDirectly(); // Buka toko setelah dialog selesai

            // Set status tutorial selesai untuk NPC ini (hanya jika tutorial Day 1)
            bool isDay1ForThisShop = false; // Tentukan lagi apakah ini tutorial Day 1
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
                    PlayerPrefs.SetInt("ShopTutorialDay1Buy", 1); // Simpan ke PlayerPrefs
                }
                else
                {
                    hasCompletedShopTutorialDay1Sell = true;
                    PlayerPrefs.SetInt("ShopTutorialDay1Sell", 1); // Simpan ke PlayerPrefs
                }
                PlayerPrefs.Save(); // Penting: Simpan perubahan PlayerPrefs

                // Setelah tutorial selesai, nonaktifkan CutsceneTrigger ini untuk dialog selanjutnya
                // Atau, set ulang IsUsableInternal menjadi true jika ingin selalu bisa dialog repeat
                if (cutsceneTrigger != null)
                {
                    // Jika Anda ingin NPC tidak lagi menampilkan dialog tutorial setelah Day1,
                    // dan hanya menampilkan UI Shop, maka SetUsableStatus(false) untuk dialognya.
                    // Namun, jika dialogueLinesRepeat Anda ingin tetap diakses,
                    // maka ini perlu logika yang lebih halus. Untuk saat ini, kita biarkan.
                    // cutsceneTrigger.SetUsableStatus(false); // <-- Hati-hati dengan ini, jika Anda masih ingin dialog repeat
                }
            }

            if (isPlayerInRange && interactText != null)
            {
                interactText.text = exitMessage;
            }
        }
        else // Jika isShopOpen false (berarti pemain menutup toko/menjauh saat dialog aktif)
        {
            Debug.Log("ShopInteractable: isShopOpen is FALSE. NOT opening shop directly. Displaying popup again if in range.");
            if (isPlayerInRange)
            {
                ShowInteractPopup(true);
            }
        }
    }

    private void OpenShopDirectly() // Metode ini sekarang hanya membuka UI toko
    {
        Debug.Log("ShopInteractable: Calling OpenShop on Shop_UI.");
        if (shopUI != null)
        {
            shopUI.OpenShop(npcShopMode); // Memanggil OpenShop di Shop_UI dengan mode yang benar
            if (interactText != null)
            {
                interactText.text = exitMessage;
            }
            isShopOpen = true; // Pastikan status ini diatur di sini
        }
        else
        {
            Debug.LogWarning("ShopInteractable: shopUI reference is NULL in OpenShopDirectly!");
        }
    }

    private void CloseShop()
    {
        Debug.Log("ShopInteractable: Closing shop.");
        if (shopUI != null)
        {
            if (shopUI.UseAnimations)
            {
                LeanTween.scale(shopUI.ShopPanel, new Vector3(0.1f, 0.1f, 0.1f), shopUI.AnimationDuration)
                    .setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
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

            isShopOpen = false; // PENTING: Atur status toko ke tertutup di sini
            if (interactText != null)
            {
                // Kembalikan pesan ke "Press E to Open Shop" hanya jika CutsceneTrigger ini masih bisa digunakan untuk interaksi
                // atau jika ini bukan tutorial Day 1 lagi (sudah selesai).
                bool isUsableAfterClose = (cutsceneTrigger == null || cutsceneTrigger.IsUsableInternal);
                if (npcShopMode == Shop_UI.ShopMode.Buy && hasCompletedShopTutorialDay1Buy) isUsableAfterClose = true;
                if (npcShopMode == Shop_UI.ShopMode.Sell && hasCompletedShopTutorialDay1Sell) isUsableAfterClose = true;

                if (isUsableAfterClose)
                {
                    interactText.text = interactMessage;
                } else {
                    // Jika dialog tutorial one-time use, setelah selesai, popup interaksi mungkin tidak lagi relevan
                    interactText.text = ""; // Sembunyikan pesan jika tidak lagi usable (misal: tutorial one-time use)
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Tampilkan popup interaksi hanya jika toko tidak aktif, tidak ada cutscene aktif,
            // DAN CutsceneTrigger ini sendiri masih usable (untuk tutorial)
            bool canShowPopup = (cutsceneTrigger == null || !cutsceneTrigger.IsCutsceneActive) &&
                                (cutsceneTrigger == null || cutsceneTrigger.IsUsableInternal);

            if (!isShopOpen && canShowPopup)
            {
                ShowInteractPopup(true);
            }
            // else if (isShopOpen) { // Jika toko sudah terbuka, jangan tampilkan popup interaksi lagi
            //    interactText.text = exitMessage; // Pastikan teksnya adalah 'exitMessage'
            // }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            ShowInteractPopup(false); // Sembunyikan popup saat pemain menjauh

            // Auto-close shop dan/atau cutscene jika pemain berjalan pergi saat aktif
            if (isShopOpen)
            {
                CloseShop(); // Menutup toko
                // Paksa cutscene berakhir jika masih aktif saat pemain menjauh
                if (cutsceneTrigger != null && cutsceneTrigger.IsCutsceneActive)
                {
                    cutsceneTrigger.EndCutscene();
                }
            }
        }
    }

    private void SetupInteractPopup()
    {
        // ... (kode ini tidak berubah) ...
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

    // --- Tambahkan metode StartCooldown() ini di sini ---
    private IEnumerator StartCooldown()
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(interactionCooldown);
        isOnCooldown = false;
        Debug.Log("ShopInteractable: Cooldown ended.");
    }
}