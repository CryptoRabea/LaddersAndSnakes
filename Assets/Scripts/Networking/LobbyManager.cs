using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro;

namespace LAS.Networking
{
    /// <summary>
    /// Manages lobby creation and joining for online multiplayer
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField joinCodeInput;
        [SerializeField] private TextMeshProUGUI lobbyCodeText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button leaveLobbyButton;

        [Header("Player List")]
        [SerializeField] private Transform playerListContent;
        [SerializeField] private GameObject playerListItemPrefab;

        private string playerName = "Player";
        private string currentLobbyCode = "";

        private void Start()
        {
            // Setup button listeners
            if (hostButton != null)
                hostButton.onClick.AddListener(OnHostClicked);

            if (joinButton != null)
                joinButton.onClick.AddListener(OnJoinClicked);

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
                startGameButton.gameObject.SetActive(false);
            }

            if (leaveLobbyButton != null)
            {
                leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
                leaveLobbyButton.gameObject.SetActive(false);
            }

            // Subscribe to network events
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnHostClicked()
        {
            if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            {
                playerName = playerNameInput.text;
            }

            StartHost();
        }

        private void OnJoinClicked()
        {
            if (playerNameInput != null && !string.IsNullOrEmpty(playerNameInput.text))
            {
                playerName = playerNameInput.text;
            }

            string joinCode = joinCodeInput != null ? joinCodeInput.text : "";

            if (string.IsNullOrEmpty(joinCode))
            {
                UpdateStatus("Please enter a lobby code");
                return;
            }

            JoinGame(joinCode);
        }

        private void OnStartGameClicked()
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                UpdateStatus("Only the host can start the game");
                return;
            }

            StartNetworkGame();
        }

        private void OnLeaveLobbyClicked()
        {
            LeaveLobby();
        }

        /// <summary>
        /// Start hosting a game
        /// </summary>
        private void StartHost()
        {
            UpdateStatus("Starting host...");

            if (NetworkManager.Singleton.StartHost())
            {
                currentLobbyCode = GenerateLobbyCode();

                UpdateStatus($"Hosting game! Lobby Code: {currentLobbyCode}");

                if (lobbyCodeText != null)
                    lobbyCodeText.text = $"Lobby Code: {currentLobbyCode}";

                if (startGameButton != null)
                    startGameButton.gameObject.SetActive(true);

                if (leaveLobbyButton != null)
                    leaveLobbyButton.gameObject.SetActive(true);

                if (hostButton != null)
                    hostButton.interactable = false;

                if (joinButton != null)
                    joinButton.interactable = false;
            }
            else
            {
                UpdateStatus("Failed to start host");
            }
        }

        /// <summary>
        /// Join an existing game
        /// </summary>
        private void JoinGame(string lobbyCode)
        {
            UpdateStatus($"Joining game with code: {lobbyCode}...");

            // In a real implementation, you would resolve the lobby code to an IP/port
            // For this example, we'll use direct connection
            // You should integrate with Unity Relay or similar service

            if (NetworkManager.Singleton.StartClient())
            {
                currentLobbyCode = lobbyCode;
                UpdateStatus("Connected to lobby!");

                if (leaveLobbyButton != null)
                    leaveLobbyButton.gameObject.SetActive(true);

                if (hostButton != null)
                    hostButton.interactable = false;

                if (joinButton != null)
                    joinButton.interactable = false;
            }
            else
            {
                UpdateStatus("Failed to join game");
            }
        }

        /// <summary>
        /// Start the networked game
        /// </summary>
        private void StartNetworkGame()
        {
            var networkGameManager = FindAnyObjectByType<NetworkGameManager>();
            if (networkGameManager != null)
            {
                networkGameManager.StartNetworkGameServerRpc();
                UpdateStatus("Starting game...");

                // Hide lobby UI
                if (lobbyPanel != null)
                    lobbyPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Leave current lobby
        /// </summary>
        private void LeaveLobby()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            UpdateStatus("Left lobby");
            ResetUI();
        }

        private void OnClientConnected(ulong clientId)
        {
            UpdateStatus($"Player connected! Total players: {NetworkManager.Singleton.ConnectedClients.Count}");
            UpdatePlayerList();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            UpdateStatus($"Player disconnected. Total players: {NetworkManager.Singleton.ConnectedClients.Count}");
            UpdatePlayerList();
        }

        /// <summary>
        /// Update the player list UI
        /// </summary>
        private void UpdatePlayerList()
        {
            if (playerListContent == null || playerListItemPrefab == null)
                return;

            // Clear existing list
            foreach (Transform child in playerListContent)
            {
                Destroy(child.gameObject);
            }

            // Add connected players
            int index = 1;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                GameObject item = Instantiate(playerListItemPrefab, playerListContent);
                TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"Player {index} {(client.Key == NetworkManager.Singleton.LocalClientId ? "(You)" : "")}";
                }
                index++;
            }
        }

        /// <summary>
        /// Generate a random lobby code
        /// </summary>
        private string GenerateLobbyCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[6];

            for (int i = 0; i < code.Length; i++)
            {
                code[i] = chars[Random.Range(0, chars.Length)];
            }

            return new string(code);
        }

        private void UpdateStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;

            Debug.Log($"Lobby: {message}");
        }

        private void ResetUI()
        {
            if (hostButton != null)
                hostButton.interactable = true;

            if (joinButton != null)
                joinButton.interactable = true;

            if (startGameButton != null)
                startGameButton.gameObject.SetActive(false);

            if (leaveLobbyButton != null)
                leaveLobbyButton.gameObject.SetActive(false);

            if (lobbyCodeText != null)
                lobbyCodeText.text = "";

            currentLobbyCode = "";
        }
    }
}
