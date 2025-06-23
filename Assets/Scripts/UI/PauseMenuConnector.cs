using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to automatically connect pause menu buttons to the persistent GameManager.
/// Place this script on pause menu buttons that need to call GameManager methods.
/// </summary>
public class PauseMenuConnector : MonoBehaviour
{
    [Header("Button Type")]
    [SerializeField] private ButtonType buttonType = ButtonType.Continue;

    private Button button;

    public enum ButtonType
    {
        Continue,
        Exit
    }

    private void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"PauseMenuConnector on {gameObject.name} requires a Button component!");
            return;
        }

        // Connect to GameManager after a short delay to ensure it's initialized
        Invoke(nameof(ConnectToGameManager), 0.1f);
    }

    private void ConnectToGameManager()
    {
        GameManager gameManager = GameManager.GetInstance();

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found! Cannot connect pause menu button.");
            return;
        }

        // Remove any existing listeners to avoid duplicates
        button.onClick.RemoveAllListeners();

        // Connect the appropriate method based on button type
        switch (buttonType)
        {
            case ButtonType.Continue:
                button.onClick.AddListener(gameManager.OnClick_Continue);
                Debug.Log($"Continue button {gameObject.name} connected to GameManager");
                break;
            case ButtonType.Exit:
                button.onClick.AddListener(gameManager.OnClick_Exit);
                Debug.Log($"Exit button {gameObject.name} connected to GameManager");
                break;
        }
    }

    // Public method to manually reconnect (useful if GameManager is recreated)
    public void Reconnect()
    {
        ConnectToGameManager();
    }
}
