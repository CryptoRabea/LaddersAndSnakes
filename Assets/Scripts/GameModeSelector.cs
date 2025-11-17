using UnityEngine;

/// <summary>
/// Automatically enables/disables the correct game manager based on game mode
/// Place this in the game scene to coordinate between single player and multiplayer
/// </summary>
public class GameModeSelector : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private ManualGameManager manualGameManager;
    [SerializeField] private NetworkGameManager networkGameManager;

    [Header("Panels (Optional)")]
    [SerializeField] private GameObject singlePlayerPanel;
    [SerializeField] private GameObject multiplayerPanel;

    void Awake()
    {
        ConfigureGameMode();
    }

    void ConfigureGameMode()
    {
        var config = GameConfiguration.Instance;

        if (config == null)
        {
            // No configuration - default to single player
            Debug.LogWarning("No GameConfiguration found, defaulting to single player mode");
            EnableSinglePlayer();
            return;
        }

        if (config.IsMultiplayer)
        {
            Debug.Log("Configuring for MULTIPLAYER mode");
            EnableMultiplayer();
        }
        else
        {
            Debug.Log("Configuring for SINGLE PLAYER mode");
            EnableSinglePlayer();
        }
    }

    void EnableSinglePlayer()
    {
        // Enable single player manager
        if (manualGameManager != null)
        {
            manualGameManager.enabled = true;
            manualGameManager.gameObject.SetActive(true);
        }

        // Disable network manager
        if (networkGameManager != null)
        {
            networkGameManager.enabled = false;
            networkGameManager.gameObject.SetActive(false);
        }

        // Show/hide panels
        if (singlePlayerPanel != null)
        {
            singlePlayerPanel.SetActive(true);
        }

        if (multiplayerPanel != null)
        {
            multiplayerPanel.SetActive(false);
        }
    }

    void EnableMultiplayer()
    {
        // Enable network manager
        if (networkGameManager != null)
        {
            networkGameManager.enabled = true;
            networkGameManager.gameObject.SetActive(true);
        }

        // Keep manual game manager enabled (network will configure it)
        if (manualGameManager != null)
        {
            manualGameManager.enabled = true;
            manualGameManager.gameObject.SetActive(true);
        }

        // Show/hide panels
        if (singlePlayerPanel != null)
        {
            singlePlayerPanel.SetActive(false);
        }

        if (multiplayerPanel != null)
        {
            multiplayerPanel.SetActive(true);
        }
    }
}
