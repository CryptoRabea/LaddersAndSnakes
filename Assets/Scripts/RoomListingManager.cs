using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Manages room creation and listing for multiplayer
/// Handles session discovery via Photon Fusion
/// Optimized for mobile devices with scrollable list and touch-friendly controls
/// </summary>
public class RoomListingManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Mobile Optimization")]
    [SerializeField] private bool enableMobileOptimizations = true;
    [SerializeField] private float mobileButtonScaleMultiplier = 1.2f;
    [SerializeField] private ScrollRect roomListScrollRect;
    [Header("UI References")]
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button refreshButton;

    [Header("Create Room UI")]
    [SerializeField] private TMP_InputField createRoomNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private Button createRoomButton;

    private NetworkRunner _lobbyRunner;
    private List<SessionInfo> _availableRooms = new List<SessionInfo>();
    private List<GameObject> _roomListItems = new List<GameObject>();

    // Auto-refresh settings
    [SerializeField] private bool autoRefreshEnabled = true;
    [SerializeField] private float autoRefreshInterval = 3f;
    private float _lastRefreshTime = 0f;

    // Connection tracking
    private bool _isConnectedToLobby = false;
    private int _connectionRetryCount = 0;
    private const int MAX_CONNECTION_RETRIES = 3;

    // Safety features
    private HashSet<string> _joinedRooms = new HashSet<string>();
    private float _lastJoinAttemptTime = 0f;
    private const float JOIN_COOLDOWN = 2f;

    void Start()
    {
        // Apply mobile optimizations if on mobile device
        if (enableMobileOptimizations && Application.isMobilePlatform)
        {
            ApplyMobileOptimizations();
        }

        SetupUI();
        StartLobbyRunner();
    }

    /// <summary>
    /// Apply mobile-specific optimizations to room listing UI
    /// </summary>
    void ApplyMobileOptimizations()
    {
        Debug.Log("Applying mobile optimizations to room listing...");

        // Enlarge input fields for easier touch interaction
        if (createRoomNameInput != null)
        {
            RectTransform inputRect = createRoomNameInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            createRoomNameInput.textComponent.fontSize = Mathf.Max(createRoomNameInput.textComponent.fontSize, 36f);
        }

        if (maxPlayersInput != null)
        {
            RectTransform inputRect = maxPlayersInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            maxPlayersInput.textComponent.fontSize = Mathf.Max(maxPlayersInput.textComponent.fontSize, 36f);
        }

        // Enlarge buttons for better touch targets
        Button[] buttons = new Button[] { refreshButton, createRoomButton };
        foreach (var button in buttons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.localScale *= mobileButtonScaleMultiplier;
                }

                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 100f;
            }
        }

        // Ensure room list has a scroll rect for mobile
        if (roomListContainer != null && roomListScrollRect == null)
        {
            roomListScrollRect = roomListContainer.GetComponentInParent<ScrollRect>();
            if (roomListScrollRect != null)
            {
                // Optimize scroll for mobile
                roomListScrollRect.scrollSensitivity = 30f; // Increase scroll sensitivity for touch
                roomListScrollRect.inertia = true;
                roomListScrollRect.decelerationRate = 0.135f;
            }
        }

        // Enlarge status text
        if (statusText != null)
        {
            statusText.fontSize *= 1.2f;
        }

        Debug.Log("Mobile optimizations applied to room listing");
    }

    void Update()
    {
        // Auto-refresh room list
        if (autoRefreshEnabled && _isConnectedToLobby)
        {
            if (Time.time - _lastRefreshTime > autoRefreshInterval)
            {
                _lastRefreshTime = Time.time;
                // The session list will update automatically via OnSessionListUpdated callback
                Debug.Log("[Auto-Refresh] Waiting for session list update...");
            }
        }

        // Update connection status indicator
        UpdateConnectionStatusUI();
    }

    void UpdateConnectionStatusUI()
    {
        if (statusText == null) return;

        if (!_isConnectedToLobby)
        {
            statusText.color = Color.red;
        }
        else if (_availableRooms.Count == 0)
        {
            statusText.color = Color.yellow;
        }
        else
        {
            statusText.color = Color.white;
        }
    }

    void SetupUI()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshRoomList);
        }

        if (createRoomButton != null)
        {
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        }

        if (createRoomNameInput != null)
        {
            createRoomNameInput.text = "Room_" + UnityEngine.Random.Range(1000, 9999);
            createRoomNameInput.characterLimit = 20;

            // Validate input in real-time
            createRoomNameInput.onValueChanged.AddListener(ValidateRoomNameInput);
        }

        if (maxPlayersInput != null)
        {
            maxPlayersInput.text = "4";
            maxPlayersInput.characterLimit = 1;
            maxPlayersInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        UpdateStatusText("Connecting to server...");
    }

    void ValidateRoomNameInput(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            createRoomButton.interactable = false;
            UpdateStatusText("Room name cannot be empty!");
        }
        else if (roomName.Length < 3)
        {
            createRoomButton.interactable = false;
            UpdateStatusText("Room name must be at least 3 characters!");
        }
        else if (roomName.Length > 20)
        {
            createRoomButton.interactable = false;
            UpdateStatusText("Room name too long (max 20 characters)!");
        }
        else
        {
            createRoomButton.interactable = true;
            UpdateStatusText(_isConnectedToLobby ?
                $"Found {_availableRooms.Count} room(s)" :
                "Connecting to server...");
        }
    }

    async void StartLobbyRunner()
    {
        if (_lobbyRunner != null)
        {
            Debug.LogWarning("Lobby runner already exists!");
            return;
        }

        Debug.Log("Starting lobby runner for session discovery...");
        UpdateStatusText("Connecting to server...");

        try
        {
            // Create a NetworkRunner for lobby/session list
            _lobbyRunner = gameObject.AddComponent<NetworkRunner>();
            _lobbyRunner.name = "LobbyRunner";
            _lobbyRunner.AddCallbacks(this);

            var result = await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom);

            if (result.Ok)
            {
                _isConnectedToLobby = true;
                _connectionRetryCount = 0;
                Debug.Log("Successfully joined session lobby!");
                UpdateStatusText("Connected. Searching for rooms...");
                RefreshRoomList();
            }
            else
            {
                _isConnectedToLobby = false;
                Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
                HandleConnectionFailure(result.ShutdownReason.ToString());
            }
        }
        catch (System.Exception e)
        {
            _isConnectedToLobby = false;
            Debug.LogError($"Exception while starting lobby runner: {e.Message}");
            HandleConnectionFailure($"Error: {e.Message}");
        }
    }

    void HandleConnectionFailure(string reason)
    {
        _connectionRetryCount++;

        if (_connectionRetryCount < MAX_CONNECTION_RETRIES)
        {
            UpdateStatusText($"Connection failed. Retrying ({_connectionRetryCount}/{MAX_CONNECTION_RETRIES})...");
            Debug.Log($"Retrying connection in 2 seconds... (Attempt {_connectionRetryCount}/{MAX_CONNECTION_RETRIES})");

            // Retry after delay
            Invoke(nameof(RetryConnection), 2f);
        }
        else
        {
            UpdateStatusText($"Connection failed after {MAX_CONNECTION_RETRIES} attempts. Please check your internet connection.");
            Debug.LogError($"Failed to connect after {MAX_CONNECTION_RETRIES} attempts");

            // Enable manual retry button
            if (refreshButton != null)
            {
                refreshButton.interactable = true;
                var buttonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Retry Connection";
                }
            }
        }
    }

    void RetryConnection()
    {
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
            Destroy(_lobbyRunner);
            _lobbyRunner = null;
        }

        StartLobbyRunner();
    }

    void RefreshRoomList()
    {
        Debug.Log("Refreshing room list...");
        UpdateStatusText("Refreshing room list...");

        // The session list will be updated via OnSessionListUpdated callback
        // For now, just clear and wait for the callback
        ClearRoomList();
    }

    void ClearRoomList()
    {
        foreach (var item in _roomListItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _roomListItems.Clear();
    }

    void UpdateRoomListUI()
    {
        ClearRoomList();

        if (_availableRooms.Count == 0)
        {
            UpdateStatusText("No rooms found. Create one to get started!");
            return;
        }

        UpdateStatusText($"Found {_availableRooms.Count} room(s)");

        foreach (var session in _availableRooms)
        {
            CreateRoomListItem(session);
        }
    }

    void CreateRoomListItem(SessionInfo session)
    {
        if (roomListItemPrefab == null || roomListContainer == null)
        {
            Debug.LogWarning("Room list UI not configured!");
            return;
        }

        GameObject item = Instantiate(roomListItemPrefab, roomListContainer);
        _roomListItems.Add(item);

        // Get UI components from prefab
        var roomNameText = item.transform.Find("RoomNameText")?.GetComponent<TextMeshProUGUI>();
        var playerCountText = item.transform.Find("PlayerCountText")?.GetComponent<TextMeshProUGUI>();
        var joinButton = item.transform.Find("JoinButton")?.GetComponent<Button>();

        if (roomNameText != null)
        {
            roomNameText.text = session.Name;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"{session.PlayerCount}/{session.MaxPlayers}";
        }

        if (joinButton != null)
        {
            // Check if room is full
            bool isFull = session.PlayerCount >= session.MaxPlayers;
            joinButton.interactable = !isFull;

            if (isFull)
            {
                var buttonText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Full";
                }
            }

            joinButton.onClick.AddListener(() => JoinRoom(session.Name, session.MaxPlayers));
        }

        Debug.Log($"Added room to list: {session.Name} ({session.PlayerCount}/{session.MaxPlayers})");
    }

    void OnCreateRoomClicked()
    {
        string roomName = createRoomNameInput != null ? createRoomNameInput.text.Trim() : "DefaultRoom";
        int maxPlayers = 4;

        if (maxPlayersInput != null && int.TryParse(maxPlayersInput.text, out int parsedPlayers))
        {
            maxPlayers = Mathf.Clamp(parsedPlayers, 2, 8);
        }

        // Validate room name
        if (string.IsNullOrWhiteSpace(roomName))
        {
            UpdateStatusText("Please enter a room name!");
            return;
        }

        if (roomName.Length < 3)
        {
            UpdateStatusText("Room name must be at least 3 characters!");
            return;
        }

        if (roomName.Length > 20)
        {
            UpdateStatusText("Room name must be 20 characters or less!");
            return;
        }

        // Check for duplicate room names
        if (_availableRooms.Any(r => r.Name.Equals(roomName, System.StringComparison.OrdinalIgnoreCase)))
        {
            UpdateStatusText($"Room '{roomName}' already exists! Please choose a different name.");
            return;
        }

        // Validate max players
        if (maxPlayers < 2 || maxPlayers > 8)
        {
            UpdateStatusText("Max players must be between 2 and 8!");
            return;
        }

        Debug.Log($"Creating room: {roomName} with {maxPlayers} max players");
        CreateRoom(roomName, maxPlayers);
    }

    void CreateRoom(string roomName, int maxPlayers)
    {
        if (!_isConnectedToLobby)
        {
            UpdateStatusText("Not connected to server! Please wait...");
            return;
        }

        UpdateStatusText($"Creating room '{roomName}'...");

        // Sanitize room name
        roomName = SanitizeRoomName(roomName);

        // Set player name if not set
        if (string.IsNullOrEmpty(PlayerInfo.LocalPlayerName))
        {
            PlayerInfo.LocalPlayerName = PlayerInfo.GenerateRandomName();
        }

        // Configure game for multiplayer as host
        GameConfiguration.Instance.SetMultiplayerMode(true, roomName, maxPlayers);

        // Shutdown lobby runner
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
        }

        // Load game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    void JoinRoom(string roomName, int maxPlayers)
    {
        // Check join cooldown to prevent spam
        if (Time.time - _lastJoinAttemptTime < JOIN_COOLDOWN)
        {
            float remainingTime = JOIN_COOLDOWN - (Time.time - _lastJoinAttemptTime);
            UpdateStatusText($"Please wait {remainingTime:F1}s before joining another room...");
            return;
        }

        _lastJoinAttemptTime = Time.time;

        // Check if already joined this room
        if (_joinedRooms.Contains(roomName))
        {
            UpdateStatusText("You've already attempted to join this room!");
            return;
        }

        // Verify room still exists and isn't full
        var roomInfo = _availableRooms.FirstOrDefault(r => r.Name == roomName);
        if (roomInfo == null)
        {
            UpdateStatusText($"Room '{roomName}' no longer exists!");
            RefreshRoomList();
            return;
        }

        if (roomInfo.PlayerCount >= roomInfo.MaxPlayers)
        {
            UpdateStatusText($"Room '{roomName}' is full!");
            RefreshRoomList();
            return;
        }

        if (!roomInfo.IsOpen)
        {
            UpdateStatusText($"Room '{roomName}' is closed!");
            RefreshRoomList();
            return;
        }

        Debug.Log($"Joining room: {roomName}");
        UpdateStatusText($"Joining room '{roomName}'...");

        _joinedRooms.Add(roomName);

        // Set player name if not set
        if (string.IsNullOrEmpty(PlayerInfo.LocalPlayerName))
        {
            PlayerInfo.LocalPlayerName = PlayerInfo.GenerateRandomName();
        }

        // Configure game for multiplayer as client
        GameConfiguration.Instance.SetMultiplayerMode(false, roomName, maxPlayers);

        // Shutdown lobby runner
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
        }

        // Load game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    string SanitizeRoomName(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            return "Room_" + UnityEngine.Random.Range(1000, 9999);
        }

        // Remove invalid characters
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in roomName)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ' ')
            {
                sb.Append(c);
            }
        }

        string sanitized = sb.ToString().Trim();

        // Enforce length limits
        if (sanitized.Length > 20)
        {
            sanitized = sanitized.Substring(0, 20);
        }

        if (sanitized.Length < 3)
        {
            return "Room_" + UnityEngine.Random.Range(1000, 9999);
        }

        return sanitized;
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[RoomListing] {message}");
    }

    #region INetworkRunnerCallbacks

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"Session list updated! Found {sessionList.Count} sessions");

        _availableRooms = sessionList.Where(s => s.IsOpen && s.IsVisible).ToList();

        Debug.Log($"Filtered to {_availableRooms.Count} available rooms");

        UpdateRoomListUI();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Lobby runner connected to server!");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        _isConnectedToLobby = false;
        Debug.LogWarning($"Lobby runner disconnected from server: {reason}");
        UpdateStatusText("Disconnected from server. Retrying...");

        // Attempt to reconnect
        if (_connectionRetryCount < MAX_CONNECTION_RETRIES)
        {
            Invoke(nameof(RetryConnection), 2f);
        }
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Lobby connection failed: {reason}");
        UpdateStatusText($"Connection failed: {reason}");
    }

    // Other required callbacks (empty implementations)
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Lobby runner shutdown: {shutdownReason}");
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    #endregion

    void OnDestroy()
    {
        // Clean up listeners
        if (createRoomNameInput != null)
        {
            createRoomNameInput.onValueChanged.RemoveAllListeners();
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveAllListeners();
        }

        if (createRoomButton != null)
        {
            createRoomButton.onClick.RemoveAllListeners();
        }

        // Shutdown lobby runner
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
        }
    }
}
