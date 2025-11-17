using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Start game
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
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
        if (_runner != null && _runner.LocalPlayer != PlayerRef.None)
        {
            // Convert Fusion PlayerRef (1-based) to game logic player index (0-based)
            LocalPlayerIndex = _runner.LocalPlayer.PlayerId - 1;
            Debug.Log($"Local player index: {LocalPlayerIndex} (PlayerRef: {_runner.LocalPlayer.PlayerId})");

            // Spawn network state (host only)
            if (IsHost)
            {
                SpawnNetworkState();
            }
            else
            {
                // Client - try to find existing network state
                // If not found yet, will be found in OnPlayerJoined
                TryFindNetworkState();
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

    void TryFindNetworkState()
    {
        if (_networkState != null)
        {
            return; // Already found
        }

        // Try to find the network state object
        _networkState = FindFirstObjectByType<NetworkGameState>();

        if (_networkState != null)
        {
            Debug.Log("Client found NetworkGameState!");
            ConfigureGameManager();
        }
        else
        {
            // Network state not spawned yet, try again shortly
            Debug.Log("NetworkGameState not found yet, will retry...");
            StartCoroutine(RetryFindNetworkState());
        }
    }

    System.Collections.IEnumerator RetryFindNetworkState()
    {
        int retries = 0;
        while (_networkState == null && retries < 20) // Try for up to 2 seconds
        {
            yield return new WaitForSeconds(0.1f);
            retries++;

            _networkState = FindFirstObjectByType<NetworkGameState>();
            if (_networkState != null)
            {
                Debug.Log($"Client found NetworkGameState after {retries} retries!");
                ConfigureGameManager();
                yield break;
            }
        }

        if (_networkState == null)
        {
            Debug.LogError("Failed to find NetworkGameState after retries!");
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

        // Only host sets the number of players
        if (IsHost)
        {
            _networkState.SetNumberOfPlayers(maxPlayers);
        }

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
        Debug.Log($"Player {player.PlayerId} joined (0-based index: {player.PlayerId - 1})!");

        // Get actual player count from runner
        PlayerCount = runner.ActivePlayers.Count();

        // If we're a client and don't have network state yet, try to find it
        if (!IsHost && _networkState == null)
        {
            TryFindNetworkState();
        }

        // Update player count (host only)
        if (_networkState != null && IsHost)
        {
            _networkState.SetNumberOfPlayers(PlayerCount);
            Debug.Log($"Updated network player count to: {PlayerCount}");
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
