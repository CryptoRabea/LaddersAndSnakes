using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Handles Photon Fusion networking for multiplayer Ladders and Snakes
/// Works alongside ManualGameManager to sync game state
/// </summary>
public class NetworkGameManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Settings")]
    [SerializeField] private string roomName = "LaddersAndSnakes";
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string gameVersion = "1.0";

    [Header("UI References")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject gamePanel;

    [Header("Game References")]
    [SerializeField] private ManualGameManager gameManager;
    [SerializeField] private GameObject networkStatePrefab; // Prefab with NetworkGameState component

    private NetworkRunner _runner;
    private bool _isInitialized = false;
    private NetworkGameState _networkState;

    // Network state
    public bool IsHost => _runner != null && _runner.IsServer;
    public bool IsConnected => _runner != null && _runner.IsRunning;
    public int LocalPlayerIndex { get; private set; } = -1;
    public int PlayerCount { get; private set; } = 0;

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        // Show lobby, hide game initially
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);

        // Setup buttons
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() => StartGame(GameMode.Host));
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(() => StartGame(GameMode.Client));
        }
    }

    async void StartGame(GameMode mode)
    {
        Debug.Log($"Starting game in {mode} mode...");

        // Create runner
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // Setup scene info
        var sceneInfo = new NetworkSceneInfo();

        // Start game
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers
        });

        if (result.Ok)
        {
            Debug.Log($"Successfully started in {mode} mode!");
            OnGameStarted();
        }
        else
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
        }
    }

    void OnGameStarted()
    {
        _isInitialized = true;

        // Hide lobby, show game
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(true);

        // Assign player index based on join order
        if (_runner != null && _runner.LocalPlayer.IsValid)
        {
            LocalPlayerIndex = _runner.LocalPlayer.PlayerId;
            Debug.Log($"Local player index: {LocalPlayerIndex}");

            // Spawn network state (host only)
            if (IsHost)
            {
                SpawnNetworkState();
            }
        }
    }

    void SpawnNetworkState()
    {
        if (!IsHost || _networkState != null)
        {
            Debug.LogWarning("Cannot spawn network state - not host or already spawned!");
            return;
        }

        if (networkStatePrefab != null)
        {
            // Spawn the network state object
            var stateObj = _runner.Spawn(networkStatePrefab, Vector3.zero, Quaternion.identity);
            _networkState = stateObj.GetComponent<NetworkGameState>();

            if (_networkState != null)
            {
                Debug.Log("NetworkGameState spawned successfully!");
                ConfigureGameManager();
            }
            else
            {
                Debug.LogError("NetworkGameState component not found on spawned prefab!");
            }
        }
        else
        {
            Debug.LogError("NetworkStatePrefab is not assigned!");
        }
    }

    void ConfigureGameManager()
    {
        if (gameManager == null)
        {
            Debug.LogError("ManualGameManager reference not set!");
            return;
        }

        if (_networkState == null)
        {
            Debug.LogError("NetworkGameState not available!");
            return;
        }

        // Configure the game manager for multiplayer
        gameManager.ConfigureMultiplayer(_networkState, LocalPlayerIndex, maxPlayers);
        _networkState.SetNumberOfPlayers(maxPlayers);

        Debug.Log($"Game manager configured for multiplayer. Local player: {LocalPlayerIndex}");
    }

    // Request dice roll (called by local player)
    public void RequestDiceRoll()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Not connected to network!");
            return;
        }

        // This will be called by the game manager when player wants to roll
        Debug.Log("Requesting dice roll from network...");
    }

    // Broadcast dice result to all players
    public void BroadcastDiceResult(int result)
    {
        if (!IsConnected) return;

        Debug.Log($"Broadcasting dice result: {result}");
        // This will sync the dice roll result to all clients
    }

    #region INetworkRunnerCallbacks

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} joined!");
        PlayerCount++;

        // If we're a client and don't have network state yet, find it
        if (!IsHost && _networkState == null)
        {
            // Find the network state object in the scene
            _networkState = FindObjectOfType<NetworkGameState>();

            if (_networkState != null)
            {
                Debug.Log("Client found NetworkGameState!");
                ConfigureGameManager();
            }
        }

        // Update player count
        if (_networkState != null && IsHost)
        {
            _networkState.SetNumberOfPlayers(PlayerCount);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left!");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Handle input if needed
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");
        _isInitialized = false;

        // Return to lobby
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server!");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Connect failed: {reason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason disconnectReason)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason failedReason)
    {
    }

    #endregion

    void OnDestroy()
    {
        if (hostButton != null) hostButton.onClick.RemoveAllListeners();
        if (joinButton != null) joinButton.onClick.RemoveAllListeners();

        if (_runner != null)
        {
            _runner.Shutdown();
        }
    }
}
