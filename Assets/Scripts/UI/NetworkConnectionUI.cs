using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using LAS.Networking;

namespace LAS.UI
{
    /// <summary>
    /// UI component for displaying network connection status
    /// Shows connection state, player count, and current player turn
    /// </summary>
    public class NetworkConnectionUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject connectionPanel;
        [SerializeField] private TextMeshProUGUI connectionStatusText;
        [SerializeField] private TextMeshProUGUI playerCountText;
        [SerializeField] private TextMeshProUGUI currentTurnText;
        [SerializeField] private TextMeshProUGUI localPlayerIndexText;
        [SerializeField] private Button disconnectButton;

        [Header("Settings")]
        [SerializeField] private bool showInLocalGames = false;
        [SerializeField] private bool autoHide = true;
        [SerializeField] private float hideDelay = 3f;

        private NetworkedGameManager networkManager;
        private NetworkedMultiplayerGameController gameController;
        private float hideTimer = 0f;
        private bool shouldHide = false;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Get references
            networkManager = NetworkedGameManager.Instance;
            gameController = FindObjectOfType<NetworkedMultiplayerGameController>();

            // Setup disconnect button
            if (disconnectButton != null)
            {
                disconnectButton.onClick.AddListener(OnDisconnectClicked);
            }

            // Subscribe to network events
            if (networkManager != null)
            {
                networkManager.OnPlayerJoined += OnPlayerJoined;
                networkManager.OnPlayerLeft += OnPlayerLeft;
                networkManager.OnConnectionSuccess += OnConnectionSuccess;
                networkManager.OnConnectionFailed += OnConnectionFailed;
                networkManager.OnGameStarted += OnGameStarted;
            }

            // Initial update
            UpdateUI();

            // Hide panel if not in networked game
            if (connectionPanel != null && !IsNetworkedGame())
            {
                if (!showInLocalGames)
                {
                    connectionPanel.SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (networkManager != null)
            {
                networkManager.OnPlayerJoined -= OnPlayerJoined;
                networkManager.OnPlayerLeft -= OnPlayerLeft;
                networkManager.OnConnectionSuccess -= OnConnectionSuccess;
                networkManager.OnConnectionFailed -= OnConnectionFailed;
                networkManager.OnGameStarted -= OnGameStarted;
            }

            if (disconnectButton != null)
            {
                disconnectButton.onClick.RemoveListener(OnDisconnectClicked);
            }
        }

        private void Update()
        {
            UpdateUI();

            // Auto-hide logic
            if (autoHide && shouldHide)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= hideDelay && connectionPanel != null)
                {
                    connectionPanel.SetActive(false);
                    shouldHide = false;
                }
            }
        }

        private void UpdateUI()
        {
            if (!IsNetworkedGame())
            {
                UpdateLocalGameUI();
                return;
            }

            UpdateNetworkedGameUI();
        }

        private void UpdateLocalGameUI()
        {
            if (!showInLocalGames && connectionPanel != null)
            {
                connectionPanel.SetActive(false);
                return;
            }

            if (connectionStatusText != null)
                connectionStatusText.text = "Local Game";

            if (playerCountText != null && gameController != null)
                playerCountText.text = $"Players: {gameController.GetTotalPlayers()}";

            if (currentTurnText != null && gameController != null)
                currentTurnText.text = $"Current Turn: Player {gameController.CurrentPlayer + 1}";

            if (localPlayerIndexText != null)
                localPlayerIndexText.text = "";

            if (disconnectButton != null)
                disconnectButton.gameObject.SetActive(false);
        }

        private void UpdateNetworkedGameUI()
        {
            if (connectionPanel != null && !connectionPanel.activeSelf)
                connectionPanel.SetActive(true);

            // Connection status
            if (connectionStatusText != null)
            {
                if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost)
                {
                    connectionStatusText.text = "Hosting";
                    connectionStatusText.color = Color.green;
                }
                else if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient)
                {
                    connectionStatusText.text = "Connected";
                    connectionStatusText.color = Color.green;
                }
                else
                {
                    connectionStatusText.text = "Disconnected";
                    connectionStatusText.color = Color.red;
                }
            }

            // Player count
            if (playerCountText != null && networkManager != null)
            {
                playerCountText.text = $"Players: {networkManager.PlayerCount}";
            }

            // Current turn
            if (currentTurnText != null && gameController != null)
            {
                int currentPlayer = gameController.CurrentPlayer;
                bool isLocalTurn = gameController.IsLocalPlayerTurn();
                currentTurnText.text = $"Turn: Player {currentPlayer + 1}";
                currentTurnText.color = isLocalTurn ? Color.yellow : Color.white;
            }

            // Local player index
            if (localPlayerIndexText != null && networkManager != null)
            {
                localPlayerIndexText.text = $"You are Player {networkManager.LocalPlayerIndex + 1}";
            }

            // Disconnect button
            if (disconnectButton != null)
                disconnectButton.gameObject.SetActive(true);
        }

        private bool IsNetworkedGame()
        {
            return gameController != null && gameController.IsNetworkedGame();
        }

        private void OnPlayerJoined(int playerIndex)
        {
            Debug.Log($"[NetworkConnectionUI] Player {playerIndex} joined");
            ShowPanel();
        }

        private void OnPlayerLeft(int playerIndex)
        {
            Debug.Log($"[NetworkConnectionUI] Player {playerIndex} left");
            ShowPanel();
        }

        private void OnConnectionSuccess()
        {
            Debug.Log("[NetworkConnectionUI] Connection successful");
            ShowPanel();

            if (autoHide)
            {
                shouldHide = true;
                hideTimer = 0f;
            }
        }

        private void OnConnectionFailed(string reason)
        {
            Debug.LogError($"[NetworkConnectionUI] Connection failed: {reason}");
            if (connectionStatusText != null)
            {
                connectionStatusText.text = $"Connection Failed: {reason}";
                connectionStatusText.color = Color.red;
            }
            ShowPanel();
        }

        private void OnGameStarted()
        {
            Debug.Log("[NetworkConnectionUI] Game started");
            ShowPanel();

            if (autoHide)
            {
                shouldHide = true;
                hideTimer = 0f;
            }
        }

        private void OnDisconnectClicked()
        {
            Debug.Log("[NetworkConnectionUI] Disconnect button clicked");

            if (networkManager != null)
            {
                networkManager.Disconnect();
            }

            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void ShowPanel()
        {
            if (connectionPanel != null)
            {
                connectionPanel.SetActive(true);
                shouldHide = false;
                hideTimer = 0f;
            }
        }

        public void TogglePanel()
        {
            if (connectionPanel != null)
            {
                connectionPanel.SetActive(!connectionPanel.activeSelf);
            }
        }
    }
}
