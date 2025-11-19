using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Fusion;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Button References")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private Button closePanelButton;

    [Header("Manager References")]
    private GameConfiguration gameConfig;
    private NetworkGameManager networkGameManager;
    private NetworkRunner networkRunner;

    private void Start()
    {
        // Get references
        gameConfig = GameConfiguration.Instance;
        networkGameManager = FindObjectOfType<NetworkGameManager>();

        // Setup button listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettingsPanel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (surrenderButton != null)
        {
            surrenderButton.onClick.AddListener(OnSurrenderClicked);
        }

        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(CloseSettingsPanel);
        }

        // Make sure panel is hidden at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // Apply mobile optimizations
        ApplyMobileOptimizations();
    }

    private void ApplyMobileOptimizations()
    {
        if (Application.isMobilePlatform)
        {
            // Scale buttons for better touch targets
            if (settingsButton != null)
            {
                settingsButton.transform.localScale *= 1.2f;
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.transform.localScale *= 1.2f;
            }

            if (surrenderButton != null)
            {
                surrenderButton.transform.localScale *= 1.2f;
            }

            if (closePanelButton != null)
            {
                closePanelButton.transform.localScale *= 1.2f;
            }
        }
    }

    public void OpenSettingsPanel()
    {
        Debug.Log("[SettingsPanel] Opening settings panel");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettingsPanel()
    {
        Debug.Log("[SettingsPanel] Closing settings panel");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void GoToMainMenu()
    {
        Debug.Log("[SettingsPanel] Returning to main menu");

        // If in multiplayer, cleanup network connection
        if (gameConfig != null && gameConfig.IsMultiplayer)
        {
            CleanupNetworkConnection();
        }

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }

    public void OnSurrenderClicked()
    {
        Debug.Log("[SettingsPanel] Surrender button clicked");

        if (gameConfig == null)
        {
            Debug.LogError("[SettingsPanel] GameConfiguration not found!");
            GoToMainMenu();
            return;
        }

        // Check if multiplayer
        if (gameConfig.IsMultiplayer)
        {
            HandleMultiplayerSurrender();
        }
        else
        {
            // Local or AI game - just quit to main menu
            Debug.Log("[SettingsPanel] Surrendering local/AI game");
            GoToMainMenu();
        }
    }

    private void HandleMultiplayerSurrender()
    {
        Debug.Log("[SettingsPanel] Handling multiplayer surrender");

        // Get network runner
        if (networkGameManager != null)
        {
            networkRunner = networkGameManager.GetComponent<NetworkRunner>();
            if (networkRunner == null)
            {
                networkRunner = FindObjectOfType<NetworkRunner>();
            }
        }
        else
        {
            networkRunner = FindObjectOfType<NetworkRunner>();
        }

        if (networkRunner == null)
        {
            Debug.LogWarning("[SettingsPanel] NetworkRunner not found, treating as disconnected");
            GoToMainMenu();
            return;
        }

        // Check player count
        int playerCount = networkRunner.SessionInfo.PlayerCount;
        Debug.Log($"[SettingsPanel] Current player count: {playerCount}");

        // If only 1 player left (us), close the room
        if (playerCount <= 1)
        {
            Debug.Log("[SettingsPanel] Only 1 player remaining, closing room");
            CloseRoom();
        }
        else
        {
            Debug.Log("[SettingsPanel] Multiple players remaining, leaving room");
            LeaveRoom();
        }
    }

    private void CloseRoom()
    {
        Debug.Log("[SettingsPanel] Closing room and shutting down network");

        if (networkRunner != null)
        {
            // Shutdown with ShutdownReason.Ok to close the session
            networkRunner.Shutdown(true, ShutdownReason.Ok);
        }

        // Return to main menu after short delay
        Invoke(nameof(GoToMainMenu), 0.5f);
    }

    private void LeaveRoom()
    {
        Debug.Log("[SettingsPanel] Leaving room");

        if (networkRunner != null)
        {
            // Shutdown without destroying the session (other players can continue)
            networkRunner.Shutdown(false, ShutdownReason.Ok);
        }

        // Return to main menu after short delay
        Invoke(nameof(GoToMainMenu), 0.5f);
    }

    private void CleanupNetworkConnection()
    {
        Debug.Log("[SettingsPanel] Cleaning up network connection");

        if (networkRunner != null && networkRunner.IsRunning)
        {
            // Gracefully shutdown
            networkRunner.Shutdown(false, ShutdownReason.Ok);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettingsPanel);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);
        }

        if (surrenderButton != null)
        {
            surrenderButton.onClick.RemoveListener(OnSurrenderClicked);
        }

        if (closePanelButton != null)
        {
            closePanelButton.onClick.RemoveListener(CloseSettingsPanel);
        }
    }
}
