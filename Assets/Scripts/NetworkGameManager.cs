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

    [Header("Game References")]
    [SerializeField] private ManualGameManager gameManager;
    [SerializeField] private GameObject networkStatePrefab; // Prefab with NetworkGameState component

    [Header("Safety Settings")]
    [SerializeField] private float connectionTimeout = 10f;
    [SerializeField] private int maxConnectionRetries = 3;

    private NetworkRunner _runner;
    private NetworkGameState _networkState;

    // Network state
    public bool IsHost => _runner != null && _runner.IsServer;
    public bool IsConnected => _runner != null && _runner.IsRunning;
    public int LocalPlayerIndex { get; private set; } = -1;
    public int PlayerCount { get; private set; } = 0;

    // Connection tracking
    private int _connectionAttempts = 0;
    private float _connectionStartTime = 0f;
    private bool _isConnecting = false;

    // Error tracking
    private string _lastErrorMessage = "";
    public string LastError => _lastErrorMessage;

    void Start()
    {
        // Apply configuration from GameConfiguration if available
        ApplyGameConfiguration();

        SetupUI();

        // Auto-start if we have configuration
        if (GameConfiguration.Instance != null && GameConfiguration.Instance.IsMultiplayer)
        {
            AutoStartFromConfiguration();
        }
    }

    /// <summary>
    /// Apply settings from GameConfiguration singleton
    /// </summary>
    void ApplyGameConfiguration()
    {
        var config = GameConfiguration.Instance;
        if (config != null && config.IsMultiplayer)
        {
            roomName = config.RoomName;
            maxPlayers = config.MaxMultiplayerPlayers;
            Debug.Log($"Network configured from GameConfiguration: Room={roomName}, Max={maxPlayers}, IsHost={config.IsHost}");
        }
    }

    /// <summary>
    /// Auto-start game based on configuration
    /// </summary>
    void AutoStartFromConfiguration()
    {
        var config = GameConfiguration.Instance;
        if (config == null || !config.IsMultiplayer)
        {
            return;
        }

        // Determine game mode based on configuration
        GameMode mode = config.IsHost ? GameMode.Host : GameMode.Client;

        Debug.Log($"Auto-starting multiplayer as {mode}...");

        // Small delay to ensure UI is ready
        StartCoroutine(DelayedStart(mode));
    }

    System.Collections.IEnumerator DelayedStart(GameMode mode)
    {
        yield return new WaitForSeconds(0.5f);
        StartGame(mode);
    }

    void SetupUI()
    {
        // Check if auto-starting from configuration
        bool autoStart = GameConfiguration.Instance != null && GameConfiguration.Instance.IsMultiplayer;

        
    }

    async void StartGame(GameMode mode)
    {
        if (_isConnecting)
        {
            Debug.LogWarning("Connection already in progress!");
            return;
        }

        if (_runner != null)
        {
            Debug.LogWarning("Runner already exists! Cleaning up...");
            await _runner.Shutdown();
            Destroy(_runner);
            _runner = null;
        }

        _isConnecting = true;
        _connectionStartTime = Time.time;
        _connectionAttempts++;

        Debug.Log($"Starting game in {mode} mode (Attempt {_connectionAttempts}/{maxConnectionRetries})...");
        Debug.Log($"Room: {roomName}, Max Players: {maxPlayers}");

        try
        {
            // Create runner
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.name = "GameNetworkRunner";
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);

            // Validate room name
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = "Room_" + UnityEngine.Random.Range(1000, 9999);
                Debug.LogWarning($"Invalid room name, using generated name: {roomName}");
            }

            // Configure session properties for room listing
            var sessionProperties = new Dictionary<string, SessionProperty>();
            sessionProperties["GameVersion"] = gameVersion;
            sessionProperties["MaxPlayers"] = maxPlayers;
            sessionProperties["RoomName"] = roomName;
            sessionProperties["CreatedAt"] = System.DateTime.UtcNow.Ticks.ToString();

            // Add player name if available
            if (!string.IsNullOrEmpty(PlayerInfo.LocalPlayerName))
            {
                sessionProperties["HostName"] = PlayerInfo.LocalPlayerName;
            }

            // Start game with comprehensive settings
            var startArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomName,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
                PlayerCount = maxPlayers,
                SessionProperties = sessionProperties,

                // CRITICAL: Make session visible and open for room discovery
                IsVisible = true,
                IsOpen = true,

                // Custom lobby name for filtering
                CustomLobbyName = "LaddersAndSnakesLobby"
            };

            Debug.Log($"Starting session with IsVisible={startArgs.IsVisible}, IsOpen={startArgs.IsOpen}");

            // Start game with timeout protection
            var result = await _runner.StartGame(startArgs);

            // Check if timed out
            if (Time.time - _connectionStartTime > connectionTimeout)
            {
                _lastErrorMessage = "Connection timed out";
                Debug.LogError($"Connection timeout after {connectionTimeout} seconds");
                HandleConnectionFailure("Connection timeout");
                return;
            }

            if (result.Ok)
            {
                _isConnecting = false;
                _connectionAttempts = 0;
                Debug.Log($"✓ Successfully started in {mode} mode!");
                Debug.Log($"✓ Session '{roomName}' is now visible to other players");
                Debug.Log($"✓ Players can join this room from the room list");
                OnGameStarted();
            }
            else
            {
                _lastErrorMessage = result.ShutdownReason.ToString();
                Debug.LogError($"Failed to start: {result.ShutdownReason}");
                Debug.LogError($"Error details: {result.ErrorMessage}");
                HandleConnectionFailure(result.ShutdownReason.ToString());
            }
        }
        catch (System.Exception e)
        {
            _lastErrorMessage = e.Message;
            Debug.LogError($"Exception while starting game: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            HandleConnectionFailure($"Exception: {e.Message}");
        }
    }

    void HandleConnectionFailure(string reason)
    {
        _isConnecting = false;

        if (_connectionAttempts < maxConnectionRetries)
        {
            Debug.Log($"Retrying connection in 2 seconds... (Attempt {_connectionAttempts + 1}/{maxConnectionRetries})");
            Invoke(nameof(RetryConnection), 2f);
        }
        else
        {
            Debug.LogError($"Failed to connect after {maxConnectionRetries} attempts. Reason: {reason}");
            ShowConnectionError(reason);
        }
    }

    void RetryConnection()
    {
        var config = GameConfiguration.Instance;
        if (config != null && config.IsMultiplayer)
        {
            GameMode mode = config.IsHost ? GameMode.Host : GameMode.Client;
            StartGame(mode);
        }
    }

    void ShowConnectionError(string reason)
    {
        Debug.LogError($"Connection error: {reason}");
        // TODO: Show UI error message to user
        // For now, return to main menu after delay
        Invoke(nameof(ReturnToMainMenu), 5f);
    }

    void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    void OnGameStarted()
    {
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

        // Return to lobby

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
        Debug.Log($"[NetworkGameManager] Session list updated: {sessionList.Count} sessions found");
        // This callback is handled by RoomListingManager for room discovery
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
        

        if (_runner != null)
        {
            _runner.Shutdown();
        }
    }
}
