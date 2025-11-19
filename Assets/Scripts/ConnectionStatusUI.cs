using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

/// <summary>
/// Displays real-time connection status for multiplayer games
/// Shows connection state, player count, ping, and error messages
/// Can be added to any scene to monitor network health
/// </summary>
public class ConnectionStatusUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private Image connectionIndicator;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private Button reconnectButton;

    [Header("Settings")]
    [SerializeField] private bool autoHideWhenConnected = false;
    [SerializeField] private float updateInterval = 1f;

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color connectingColor = Color.yellow;
    [SerializeField] private Color disconnectedColor = Color.red;

    private NetworkRunner _runner;
    private float _lastUpdateTime = 0f;
    private bool _wasConnected = false;

    void Start()
    {
        if (reconnectButton != null)
        {
            reconnectButton.onClick.AddListener(OnReconnectClicked);
        }

        UpdateConnectionStatus();
    }

    void Update()
    {
        if (Time.time - _lastUpdateTime > updateInterval)
        {
            _lastUpdateTime = Time.time;
            UpdateConnectionStatus();
        }
    }

    void UpdateConnectionStatus()
    {
        // Find network runner if not cached
        if (_runner == null)
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
        }

        if (_runner == null || !_runner.IsRunning)
        {
            ShowDisconnected();
            return;
        }

        // Connected
        bool isConnected = _runner.IsRunning;
        bool isHost = _runner.IsServer;

        if (isConnected)
        {
            ShowConnected(isHost);

            // Check if just connected
            if (!_wasConnected)
            {
                OnConnected();
            }

            _wasConnected = true;
        }
        else
        {
            ShowDisconnected();

            // Check if just disconnected
            if (_wasConnected)
            {
                OnDisconnected();
            }

            _wasConnected = false;
        }
    }

    void ShowConnected(bool isHost)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = isHost ? "Connected (Host)" : "Connected (Client)";
            connectionStatusText.color = connectedColor;
        }

        if (connectionIndicator != null)
        {
            connectionIndicator.color = connectedColor;
        }

        // Update player count
        if (playerCountText != null && _runner != null)
        {
            int playerCount = _runner.ActivePlayers.Count();
            int maxPlayers = GameConfiguration.Instance != null ?
                GameConfiguration.Instance.MaxMultiplayerPlayers : 4;

            playerCountText.text = $"Players: {playerCount}/{maxPlayers}";
        }

        // Update ping
        if (pingText != null && _runner != null)
        {
            // Photon Fusion doesn't directly expose ping, but we can show connection quality
            pingText.text = $"Status: {(_runner.IsRunning ? "Good" : "Poor")}";
        }

        // Hide error panel
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }

        // Auto-hide if enabled
        if (autoHideWhenConnected && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void ShowDisconnected()
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = "Disconnected";
            connectionStatusText.color = disconnectedColor;
        }

        if (connectionIndicator != null)
        {
            connectionIndicator.color = disconnectedColor;
        }

        if (playerCountText != null)
        {
            playerCountText.text = "Players: 0/0";
        }

        if (pingText != null)
        {
            pingText.text = "Status: -";
        }

        // Show reconnect option
        if (reconnectButton != null)
        {
            reconnectButton.gameObject.SetActive(true);
        }
    }

    void OnConnected()
    {
        Debug.Log("[ConnectionStatus] Connected to server");

        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }

    void OnDisconnected()
    {
        Debug.LogWarning("[ConnectionStatus] Disconnected from server");

        ShowError("Connection lost. You have been disconnected from the server.");
    }

    public void ShowError(string message)
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
        }

        if (errorMessageText != null)
        {
            errorMessageText.text = message;
        }

        Debug.LogError($"[ConnectionStatus] Error: {message}");
    }

    void OnReconnectClicked()
    {
        Debug.Log("[ConnectionStatus] Reconnect requested");

        // Attempt to reconnect
        var networkManager = FindFirstObjectByType<NetworkGameManager>();
        if (networkManager != null)
        {
            // Restart the game session
            var config = GameConfiguration.Instance;
            if (config != null && config.IsMultiplayer)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
        else
        {
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    void OnDestroy()
    {
        if (reconnectButton != null)
        {
            reconnectButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Get connection quality as a percentage (0-100)
    /// </summary>
    public int GetConnectionQuality()
    {
        if (_runner == null || !_runner.IsRunning)
        {
            return 0;
        }

        // Simplified quality metric
        return 100;
    }

    /// <summary>
    /// Check if currently connected
    /// </summary>
    public bool IsConnected()
    {
        return _runner != null && _runner.IsRunning;
    }

    /// <summary>
    /// Check if local player is host
    /// </summary>
    public bool IsHost()
    {
        return _runner != null && _runner.IsServer;
    }

    /// <summary>
    /// Get current player count
    /// </summary>
    public int GetPlayerCount()
    {
        if (_runner == null || !_runner.IsRunning)
        {
            return 0;
        }

        return _runner.ActivePlayers.Count();
    }
}
