using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the lobby waiting room before game starts
/// Handles player list, ready status, and game initialization
/// Includes comprehensive safety features and error handling
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;

    [Header("Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveLobbyButton;

    [Header("Connection Status")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;
    [SerializeField] private Image connectionStatusIndicator;

    [Header("Player Name Input")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button saveNameButton;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float autoRefreshInterval = 2f;
    [SerializeField] private float connectionCheckInterval = 1f;

    // Networked player data
    [Networked, Capacity(8)]
    public NetworkArray<NetworkPlayerInfo> Players => default;

    [Networked]
    public int PlayerCount { get; set; }

    [Networked]
    public NetworkBool GameStarting { get; set; }

    // Local state
    private List<GameObject> _playerListItems = new List<GameObject>();
    private bool _isLocalPlayerReady = false;
    private float _lastRefreshTime = 0f;
    private float _lastConnectionCheckTime = 0f;
    private NetworkRunner _runner;

    // Connection status
    private bool _isConnected = false;
    private bool _isHost = false;
    private int _localPlayerIndex = -1;

    public override void Spawned()
    {
        base.Spawned();

        _isHost = Object.HasStateAuthority;
        Debug.Log($"LobbyManager spawned. IsHost: {_isHost}");

        if (_isHost)
        {
            InitializeHost();
        }

        SetupUI();
        UpdateConnectionStatus();
    }

    void Start()
    {
        // Get network runner reference
        _runner = FindFirstObjectByType<NetworkRunner>();

        SetupUI();
    }

    void SetupUI()
    {
        // Set up player name input
        if (playerNameInput != null)
        {
            playerNameInput.text = PlayerInfo.LocalPlayerName;
            playerNameInput.characterLimit = 15;
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
        }

        // Set up buttons
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
            UpdateReadyButtonText();
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
            startGameButton.gameObject.SetActive(_isHost); // Only show for host
        }

        if (leaveLobbyButton != null)
        {
            leaveLobbyButton.onClick.AddListener(OnLeaveLobbyButtonClicked);
        }

        if (saveNameButton != null)
        {
            saveNameButton.onClick.AddListener(OnSaveNameButtonClicked);
        }

        // Set room name
        if (roomNameText != null && GameConfiguration.Instance != null)
        {
            roomNameText.text = $"Room: {GameConfiguration.Instance.RoomName}";
        }

        UpdatePlayerList();
    }

    void InitializeHost()
    {
        // Initialize empty player slots
        PlayerCount = 0;

        if (Object.HasStateAuthority)
        {
            var players = Players;
            for (int i = 0; i < players.Length; i++)
            {
                players.Set(i, new NetworkPlayerInfo
                {
                    PlayerName = "",
                    PlayerIndex = -1,
                    IsReady = false,
                    ColorIndex = 0,
                    IsConnected = false
                });
            }
        }

        Debug.Log("Lobby initialized as host");
    }

    void Update()
    {
        // Auto-refresh player list
        if (Time.time - _lastRefreshTime > autoRefreshInterval)
        {
            _lastRefreshTime = Time.time;
            UpdatePlayerList();
            UpdateStartGameButton();
        }

        // Check connection status
        if (Time.time - _lastConnectionCheckTime > connectionCheckInterval)
        {
            _lastConnectionCheckTime = Time.time;
            UpdateConnectionStatus();
        }
    }

    void OnPlayerNameChanged(string newName)
    {
        // Validate in real-time
        if (!PlayerInfo.IsValidPlayerName(newName) && !string.IsNullOrEmpty(newName))
        {
            if (statusText != null)
            {
                statusText.text = "Invalid name! Use 2-15 characters (letters, numbers, spaces)";
                statusText.color = Color.yellow;
            }
        }
        else if (statusText != null && statusText.color == Color.yellow)
        {
            statusText.text = "";
        }
    }

    void OnSaveNameButtonClicked()
    {
        if (playerNameInput == null) return;

        string newName = PlayerInfo.SanitizePlayerName(playerNameInput.text);

        if (!PlayerInfo.IsValidPlayerName(newName))
        {
            if (statusText != null)
            {
                statusText.text = "Invalid player name! Please use 2-15 characters.";
                statusText.color = Color.red;
            }
            return;
        }

        // Save name
        PlayerInfo.LocalPlayerName = newName;
        playerNameInput.text = newName;

        // Update in network if joined
        if (_localPlayerIndex >= 0)
        {
            RPC_UpdatePlayerName(newName);
        }

        if (statusText != null)
        {
            statusText.text = "Name saved!";
            statusText.color = Color.green;
        }

        Debug.Log($"Player name saved: {newName}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_UpdatePlayerName(string newName)
    {
        if (_localPlayerIndex < 0 || _localPlayerIndex >= Players.Length) return;

        var players = Players;
        var playerInfo = players[_localPlayerIndex];
        playerInfo.PlayerName = PlayerInfo.SanitizePlayerName(newName);
        players.Set(_localPlayerIndex, playerInfo);

        RPC_BroadcastPlayerListUpdate();
    }

    void OnReadyButtonClicked()
    {
        _isLocalPlayerReady = !_isLocalPlayerReady;
        RPC_SetPlayerReady(_isLocalPlayerReady);
        UpdateReadyButtonText();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void RPC_SetPlayerReady(NetworkBool isReady, RpcInfo info = default)
    {
        // Find player by sender
        int senderIndex = FindPlayerIndexByRef(info.Source);

        if (senderIndex >= 0 && senderIndex < Players.Length)
        {
            var players = Players;
            var playerInfo = players[senderIndex];
            playerInfo.IsReady = isReady;
            players.Set(senderIndex, playerInfo);

            Debug.Log($"Player {senderIndex} ready status: {isReady}");

            // Broadcast update
            RPC_BroadcastPlayerListUpdate();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_BroadcastPlayerListUpdate()
    {
        UpdatePlayerList();
        UpdateStartGameButton();
    }

    void UpdateReadyButtonText()
    {
        if (readyButton != null)
        {
            var buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = _isLocalPlayerReady ? "Not Ready" : "Ready";
            }
            readyButton.GetComponent<Image>().color = _isLocalPlayerReady ? Color.green : Color.grey;
        }
    }

    void OnStartGameButtonClicked()
    {
        if (!_isHost)
        {
            Debug.LogWarning("Only host can start the game!");
            return;
        }

        // Check if all players are ready
        if (!AllPlayersReady())
        {
            if (statusText != null)
            {
                statusText.text = "Waiting for all players to be ready!";
                statusText.color = Color.yellow;
            }
            return;
        }

        // Check minimum players
        if (PlayerCount < 2)
        {
            if (statusText != null)
            {
                statusText.text = "Need at least 2 players to start!";
                statusText.color = Color.red;
            }
            return;
        }

        StartGame();
    }

    void StartGame()
    {
        if (!Object.HasStateAuthority) return;

        GameStarting = true;

        if (statusText != null)
        {
            statusText.text = "Starting game...";
            statusText.color = Color.green;
        }

        // Broadcast game start to all clients
        RPC_LoadGameScene();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_LoadGameScene()
    {
        Debug.Log("Loading game scene...");
        SceneManager.LoadScene(gameSceneName);
    }

    void OnLeaveLobbyButtonClicked()
    {
        if (statusText != null)
        {
            statusText.text = "Leaving lobby...";
        }

        // Clean up and return to main menu
        LeaveLobby();
    }

    void LeaveLobby()
    {
        // Shutdown network runner
        if (_runner != null)
        {
            _runner.Shutdown();
        }

        // Return to main menu
        SceneManager.LoadScene("MainMenu");
    }

    void UpdatePlayerList()
    {
        if (playerListContainer == null || playerListItemPrefab == null) return;

        // Clear existing items
        foreach (var item in _playerListItems)
        {
            if (item != null) Destroy(item);
        }
        _playerListItems.Clear();

        // Get current player count from network runner
        int currentPlayerCount = 0;
        if (_runner != null && _runner.IsRunning)
        {
            currentPlayerCount = _runner.ActivePlayers.Count();
        }

        // Update player count text
        if (playerCountText != null)
        {
            int maxPlayers = GameConfiguration.Instance != null ?
                GameConfiguration.Instance.MaxMultiplayerPlayers : 4;
            playerCountText.text = $"Players: {currentPlayerCount}/{maxPlayers}";
        }

        // Create player list items
        for (int i = 0; i < currentPlayerCount; i++)
        {
            CreatePlayerListItem(i);
        }
    }

    void CreatePlayerListItem(int playerIndex)
    {
        if (playerListItemPrefab == null || playerListContainer == null) return;

        GameObject item = Instantiate(playerListItemPrefab, playerListContainer);
        _playerListItems.Add(item);

        // Get player info
        NetworkPlayerInfo playerInfo = default;
        if (playerIndex < Players.Length)
        {
            playerInfo = Players[playerIndex];
        }

        // Set player name
        var nameText = item.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            string playerName = string.IsNullOrEmpty(playerInfo.PlayerName.ToString()) ?
                $"Player {playerIndex + 1}" : playerInfo.PlayerName.ToString();
            nameText.text = playerName;
            nameText.color = PlayerInfo.GetPlayerColor(playerIndex);
        }

        // Set ready status
        var readyText = item.transform.Find("ReadyStatusText")?.GetComponent<TextMeshProUGUI>();
        if (readyText != null)
        {
            readyText.text = playerInfo.IsReady ? "✓ Ready" : "Waiting...";
            readyText.color = playerInfo.IsReady ? Color.green : Color.grey;
        }

        // Set host indicator
        var hostIndicator = item.transform.Find("HostIndicator");
        if (hostIndicator != null)
        {
            hostIndicator.gameObject.SetActive(playerIndex == 0); // First player is host
        }
    }

    void UpdateStartGameButton()
    {
        if (!_isHost || startGameButton == null) return;

        bool canStart = AllPlayersReady() && PlayerCount >= 2;
        startGameButton.interactable = canStart;
    }

    bool AllPlayersReady()
    {
        if (_runner == null || !_runner.IsRunning) return false;

        int activePlayerCount = _runner.ActivePlayers.Count();

        // Check if all active players are ready
        for (int i = 0; i < activePlayerCount; i++)
        {
            if (i < Players.Length)
            {
                var playerInfo = Players[i];
                if (!playerInfo.IsReady)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void UpdateConnectionStatus()
    {
        bool wasConnected = _isConnected;

        if (_runner != null && _runner.IsRunning)
        {
            _isConnected = true;
            _isHost = _runner.IsServer;
        }
        else
        {
            _isConnected = false;
        }

        // Update UI
        if (connectionStatusText != null)
        {
            if (_isConnected)
            {
                connectionStatusText.text = _isHost ? "Connected (Host)" : "Connected (Client)";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "Disconnected";
                connectionStatusText.color = Color.red;
            }
        }

        if (connectionStatusIndicator != null)
        {
            connectionStatusIndicator.color = _isConnected ? Color.green : Color.red;
        }

        // Handle disconnection
        if (wasConnected && !_isConnected)
        {
            OnDisconnected();
        }
    }

    void OnDisconnected()
    {
        Debug.LogWarning("Lost connection to server!");

        if (statusText != null)
        {
            statusText.text = "Connection lost! Returning to menu...";
            statusText.color = Color.red;
        }

        // Return to menu after delay
        Invoke(nameof(ReturnToMainMenu), 3f);
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    int FindPlayerIndexByRef(PlayerRef playerRef)
    {
        // Fusion PlayerRef is 1-based, convert to 0-based index
        return playerRef.PlayerId - 1;
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (playerNameInput != null)
        {
            playerNameInput.onValueChanged.RemoveAllListeners();
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveAllListeners();
        }

        if (leaveLobbyButton != null)
        {
            leaveLobbyButton.onClick.RemoveAllListeners();
        }

        if (saveNameButton != null)
        {
            saveNameButton.onClick.RemoveAllListeners();
        }
    }
}
