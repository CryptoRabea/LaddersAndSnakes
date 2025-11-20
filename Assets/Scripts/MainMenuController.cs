using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using Fusion.Sockets;
using System;

/// <summary>
/// Main menu controller for game setup and multiplayer options
/// Optimized for mobile devices with responsive UI and touch-friendly controls
/// Includes room listing and selection for multiplayer
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Mobile Optimization")]
    [SerializeField] private bool enableMobileOptimizations = true;
    [SerializeField] private float mobileButtonScaleMultiplier = 1.2f;
    [SerializeField] private SafeAreaHandler safeAreaHandler;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playLocalButton;
    [SerializeField] private Button playAIButton;
    [SerializeField] private Button playOnlineButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Multiplayer Options")]
    [SerializeField] private Button hostGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshRoomListButton;
    [SerializeField] private TMP_Dropdown playerCountDropdown;

    [Header("Room Creation")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private Button showCreateRoomButton;
    [SerializeField] private Button showRoomListButton;

    [Header("Room Listing")]
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListItemPrefab;
    [SerializeField] private TextMeshProUGUI roomListStatusText;
    [SerializeField] private ScrollRect roomListScrollRect;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private Button settingsBackButton;

    [Header("Room List Settings")]
    [SerializeField] private float autoRefreshInterval = 3f;
    [SerializeField] private int maxConnectionRetries = 3;

    // Room listing state
    private NetworkRunner _lobbyRunner;
    private List<SessionInfo> _availableRooms = new List<SessionInfo>();
    private List<GameObject> _roomListItems = new List<GameObject>();
    private SessionInfo _selectedRoom;
    private bool _hasSelectedRoom = false;
    private bool _isConnectedToLobby = false;
    private float _lastRefreshTime = 0f;
    private int _connectionRetryCount = 0;

    private void Start()
    {
        // Apply mobile optimizations if on mobile device
        if (enableMobileOptimizations && Application.isMobilePlatform)
        {
            ApplyMobileOptimizations();
        }

        InitializeUI();
        SetupEventListeners();
        ShowMainPanel();
    }

    private void Update()
    {
        // Auto-refresh room list when in multiplayer panel
        if (_isConnectedToLobby && multiplayerPanel != null && multiplayerPanel.activeSelf)
        {
            if (Time.time - _lastRefreshTime > autoRefreshInterval)
            {
                _lastRefreshTime = Time.time;
                // Room list updates automatically via callback
            }
        }
    }

    /// <summary>
    /// Apply mobile-specific optimizations to menu buttons and UI
    /// </summary>
    private void ApplyMobileOptimizations()
    {
        Debug.Log("Applying mobile optimizations to main menu...");

        // Enlarge all buttons for touch-friendly interaction
        Button[] allButtons = new Button[]
        {
            playLocalButton, playAIButton, playOnlineButton, settingsButton, quitButton,
            hostGameButton, joinGameButton, backButton, settingsBackButton,
            refreshRoomListButton, showCreateRoomButton, showRoomListButton
        };

        foreach (var button in allButtons)
        {
            if (button != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.localScale *= mobileButtonScaleMultiplier;
                }

                // Increase button minimum size for better touch targets
                LayoutElement layoutElement = button.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = button.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 100f; // Minimum 100 pixels for touch
            }
        }

        // Enlarge input fields for easier typing on mobile
        if (roomNameInput != null)
        {
            RectTransform inputRect = roomNameInput.GetComponent<RectTransform>();
            if (inputRect != null)
            {
                inputRect.sizeDelta = new Vector2(inputRect.sizeDelta.x, Mathf.Max(inputRect.sizeDelta.y, 80f));
            }
            roomNameInput.textComponent.fontSize = Mathf.Max(roomNameInput.textComponent.fontSize, 32f);
        }

        // Enlarge dropdown for easier selection
        if (playerCountDropdown != null)
        {
            RectTransform dropdownRect = playerCountDropdown.GetComponent<RectTransform>();
            if (dropdownRect != null)
            {
                dropdownRect.sizeDelta = new Vector2(dropdownRect.sizeDelta.x, Mathf.Max(dropdownRect.sizeDelta.y, 80f));
            }
        }

        // Optimize scroll rect for mobile
        if (roomListScrollRect != null)
        {
            roomListScrollRect.scrollSensitivity = 30f;
            roomListScrollRect.inertia = true;
            roomListScrollRect.decelerationRate = 0.135f;
        }

        Debug.Log("Mobile optimizations applied to main menu");
    }

    private void InitializeUI()
    {
        // Initialize dropdowns
        if (playerCountDropdown != null)
        {
            playerCountDropdown.ClearOptions();
            playerCountDropdown.AddOptions(new System.Collections.Generic.List<string> { "2 Players", "3 Players", "4 Players", "6 Players", "8 Players" });
            playerCountDropdown.value = 1; // Default to 3 players
        }

        // Initialize room name with random value
        if (roomNameInput != null)
        {
            roomNameInput.text = "Room_" + UnityEngine.Random.Range(1000, 9999);
            roomNameInput.characterLimit = 20;
        }

        // Initially disable join button until a room is selected
        if (joinGameButton != null)
        {
            joinGameButton.interactable = false;
        }

        // Show room list by default
        ShowRoomListPanel();
    }

    private void SetupEventListeners()
    {
        if (playLocalButton != null)
            playLocalButton.onClick.AddListener(OnPlayLocal);

        if (playAIButton != null)
            playAIButton.onClick.AddListener(OnPlayAI);

        if (playOnlineButton != null)
            playOnlineButton.onClick.AddListener(OnPlayOnline);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);

        if (hostGameButton != null)
            hostGameButton.onClick.AddListener(OnHostGame);

        if (joinGameButton != null)
            joinGameButton.onClick.AddListener(OnJoinGame);

        if (backButton != null)
            backButton.onClick.AddListener(OnBack);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnBack);

        if (refreshRoomListButton != null)
            refreshRoomListButton.onClick.AddListener(RefreshRoomList);

        if (showCreateRoomButton != null)
            showCreateRoomButton.onClick.AddListener(ShowCreateRoomPanel);

        if (showRoomListButton != null)
            showRoomListButton.onClick.AddListener(ShowRoomListPanel);

        if (roomNameInput != null)
            roomNameInput.onValueChanged.AddListener(ValidateRoomName);
    }

    private void ShowMainPanel()
    {
        SetPanelActive(mainPanel, true);
        SetPanelActive(multiplayerPanel, false);
        SetPanelActive(settingsPanel, false);

        // Disconnect from lobby when returning to main menu
        DisconnectFromLobby();
    }

    private void ShowMultiplayerPanel()
    {
        SetPanelActive(mainPanel, false);
        SetPanelActive(multiplayerPanel, true);
        SetPanelActive(settingsPanel, false);

        // Start lobby connection when showing multiplayer panel
        StartLobbyConnection();
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private void ShowCreateRoomPanel()
    {
        SetPanelActive(createRoomPanel, true);
        SetPanelActive(roomListPanel, false);
    }

    private void ShowRoomListPanel()
    {
        SetPanelActive(createRoomPanel, false);
        SetPanelActive(roomListPanel, true);
    }

        private void OnPlayLocal()
        {
            Debug.Log("[MainMenu] Starting local multiplayer");
            int playerCount = playerCountDropdown != null ? playerCountDropdown.value + 2 : 2;

            // Configure for local multiplayer (all human players)
            GameConfiguration.Instance.SetSinglePlayerMode(playerCount, 0);
            LoadGameScene();
        }

        private void OnPlayAI()
        {
            Debug.Log("[MainMenu] Starting single player vs AI");

            // Configure for single player with 1 human and 1 AI
            GameConfiguration.Instance.SetSinglePlayerMode(1, 1);
            LoadGameScene();
        }

        private void OnPlayOnline()
        {
            Debug.Log("[MainMenu] Opening online multiplayer menu");
            ShowMultiplayerPanel();
        }

    private void OnHostGame()
    {
        // Validate room name
        if (roomNameInput == null || string.IsNullOrWhiteSpace(roomNameInput.text))
        {
            UpdateRoomListStatus("Please enter a room name!");
            return;
        }

        string roomName = roomNameInput.text.Trim();
        if (roomName.Length < 3)
        {
            UpdateRoomListStatus("Room name must be at least 3 characters!");
            return;
        }

        // Check for duplicate room names
        if (_availableRooms.Any(r => r.Name.Equals(roomName, System.StringComparison.OrdinalIgnoreCase)))
        {
            UpdateRoomListStatus($"Room '{roomName}' already exists! Please choose a different name.");
            return;
        }

        Debug.Log($"[MainMenu] Hosting game: {roomName}");

        // Calculate max players from dropdown (2, 3, 4, 6, 8)
        int maxPlayers = GetMaxPlayersFromDropdown();

        // Set player name if not set
        if (string.IsNullOrEmpty(PlayerInfo.LocalPlayerName))
        {
            PlayerInfo.LocalPlayerName = PlayerInfo.GenerateRandomName();
        }

        // Disconnect from lobby
        DisconnectFromLobby();

        // Configure for multiplayer as host
        GameConfiguration.Instance.SetMultiplayerMode(isHost: true, roomName: roomName, maxPlayers: maxPlayers);

        LoadGameScene();
    }

    private void OnJoinGame()
    {
        // Validate room selection
        if (!_hasSelectedRoom)
        {
            UpdateRoomListStatus("Please select a room to join!");
            return;
        }

        if (_selectedRoom == null)
        {
            UpdateRoomListStatus("Selected room is no longer available!");
            _hasSelectedRoom = false;
            joinGameButton.interactable = false;
            RefreshRoomList();
            return;
        }

        // Check if room is full
        if (_selectedRoom.PlayerCount >= _selectedRoom.MaxPlayers)
        {
            UpdateRoomListStatus("Room is full!");
            _hasSelectedRoom = false;
            joinGameButton.interactable = false;
            RefreshRoomList();
            return;
        }

        // Check if room is open
        if (!_selectedRoom.IsOpen)
        {
            UpdateRoomListStatus("Room is closed!");
            _hasSelectedRoom = false;
            joinGameButton.interactable = false;
            RefreshRoomList();
            return;
        }

        Debug.Log($"[MainMenu] Joining game: {_selectedRoom.Name}");

        // Set player name if not set
        if (string.IsNullOrEmpty(PlayerInfo.LocalPlayerName))
        {
            PlayerInfo.LocalPlayerName = PlayerInfo.GenerateRandomName();
        }

        // Disconnect from lobby
        DisconnectFromLobby();

        // Configure for multiplayer as client
        GameConfiguration.Instance.SetMultiplayerMode(isHost: false, roomName: _selectedRoom.Name, maxPlayers: _selectedRoom.MaxPlayers);

        LoadGameScene();
    }

    private int GetMaxPlayersFromDropdown()
    {
        if (playerCountDropdown == null) return 4;

        // Dropdown options: "2 Players", "3 Players", "4 Players", "6 Players", "8 Players"
        switch (playerCountDropdown.value)
        {
            case 0: return 2;
            case 1: return 3;
            case 2: return 4;
            case 3: return 6;
            case 4: return 8;
            default: return 4;
        }
    }

    public void ReturnToMainMenu()
    {
        Debug.Log($"[MainMenu] Loading scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

        private void OnSettings()
        {
            Debug.Log("[MainMenu] Opening settings");
            SetPanelActive(mainPanel, false);
            SetPanelActive(multiplayerPanel, false);
            SetPanelActive(settingsPanel, true);
        }

        private void OnBack()
        {
            Debug.Log("[MainMenu] Back to main menu");
            ShowMainPanel();
        }

        private void OnQuit()
        {
            Debug.Log("[MainMenu] Quitting game");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    private void LoadGameScene()
    {
        Debug.Log($"[MainMenu] Loading scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }

    // ===== ROOM LISTING FUNCTIONALITY =====

    private async void StartLobbyConnection()
    {
        if (_lobbyRunner != null)
        {
            Debug.Log("[MainMenu] Already connected to lobby");
            return;
        }

        Debug.Log("[MainMenu] Connecting to lobby for room discovery...");
        UpdateRoomListStatus("Connecting to server...");

        try
        {
            _lobbyRunner = gameObject.AddComponent<NetworkRunner>();
            _lobbyRunner.name = "MainMenuLobbyRunner";
            _lobbyRunner.AddCallbacks(new RoomListCallbacks(this));

            var result = await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom, "LaddersAndSnakesLobby");

            if (result.Ok)
            {
                _isConnectedToLobby = true;
                _connectionRetryCount = 0;
                Debug.Log("[MainMenu] Connected to lobby successfully!");
                UpdateRoomListStatus("Connected. Searching for rooms...");
                _lastRefreshTime = Time.time;
            }
            else
            {
                Debug.LogError($"[MainMenu] Failed to connect to lobby: {result.ShutdownReason}");
                UpdateRoomListStatus($"Connection failed: {result.ShutdownReason}");
                HandleConnectionFailure();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MainMenu] Exception connecting to lobby: {e.Message}");
            UpdateRoomListStatus($"Connection error: {e.Message}");
            HandleConnectionFailure();
        }
    }

    private void HandleConnectionFailure()
    {
        _isConnectedToLobby = false;
        _connectionRetryCount++;

        if (_connectionRetryCount < maxConnectionRetries)
        {
            UpdateRoomListStatus($"Retrying connection... ({_connectionRetryCount}/{maxConnectionRetries})");
            Invoke(nameof(RetryLobbyConnection), 2f);
        }
        else
        {
            UpdateRoomListStatus("Connection failed. Please check your internet connection.");
        }
    }

    private void RetryLobbyConnection()
    {
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
            Destroy(_lobbyRunner);
            _lobbyRunner = null;
        }
        StartLobbyConnection();
    }

    private void DisconnectFromLobby()
    {
        if (_lobbyRunner != null)
        {
            Debug.Log("[MainMenu] Disconnecting from lobby");
            _lobbyRunner.Shutdown();
            Destroy(_lobbyRunner);
            _lobbyRunner = null;
        }
        _isConnectedToLobby = false;
    }

    private void RefreshRoomList()
    {
        Debug.Log("[MainMenu] Refreshing room list...");
        UpdateRoomListStatus("Refreshing...");

        // Clear current selection when refreshing
        _hasSelectedRoom = false;
        _selectedRoom = null;
        if (joinGameButton != null)
        {
            joinGameButton.interactable = false;
        }

        // Room list will update via callback
    }

    public void OnSessionListUpdated(List<SessionInfo> sessionList)
    {
        Debug.Log($"[MainMenu] Session list updated: {sessionList.Count} rooms found");

        _availableRooms = sessionList.Where(s => s.IsOpen && s.IsVisible).ToList();

        Debug.Log($"[MainMenu] Filtered to {_availableRooms.Count} available rooms");

        UpdateRoomListUI();
    }

    private void UpdateRoomListUI()
    {
        // Clear existing room list items
        foreach (var item in _roomListItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _roomListItems.Clear();

        if (_availableRooms.Count == 0)
        {
            UpdateRoomListStatus("No rooms available. Create one to get started!");
            return;
        }

        UpdateRoomListStatus($"Found {_availableRooms.Count} room(s)");

        // Create UI items for each room
        foreach (var session in _availableRooms)
        {
            CreateRoomListItem(session);
        }
    }

    private void CreateRoomListItem(SessionInfo session)
    {
        if (roomListItemPrefab == null || roomListContainer == null)
        {
            Debug.LogWarning("[MainMenu] Room list UI not configured!");
            return;
        }

        GameObject item = Instantiate(roomListItemPrefab, roomListContainer);
        _roomListItems.Add(item);

        // Get UI components
        var roomNameText = item.transform.Find("RoomNameText")?.GetComponent<TextMeshProUGUI>();
        var playerCountText = item.transform.Find("PlayerCountText")?.GetComponent<TextMeshProUGUI>();
        var selectButton = item.transform.Find("SelectButton")?.GetComponent<Button>();

        if (roomNameText != null)
        {
            roomNameText.text = session.Name;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"{session.PlayerCount}/{session.MaxPlayers} Players";

            // Color code based on availability
            if (session.PlayerCount >= session.MaxPlayers)
            {
                playerCountText.color = Color.red;
            }
            else if (session.PlayerCount >= session.MaxPlayers * 0.75f)
            {
                playerCountText.color = Color.yellow;
            }
            else
            {
                playerCountText.color = Color.green;
            }
        }

        if (selectButton != null)
        {
            bool isFull = session.PlayerCount >= session.MaxPlayers;
            selectButton.interactable = !isFull;

            var buttonText = selectButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isFull ? "Full" : "Select";
            }

            selectButton.onClick.AddListener(() => OnRoomSelected(session));
        }

        Debug.Log($"[MainMenu] Added room to list: {session.Name} ({session.PlayerCount}/{session.MaxPlayers})");
    }

    private void OnRoomSelected(SessionInfo room)
    {
        _selectedRoom = room;
        _hasSelectedRoom = true;

        Debug.Log($"[MainMenu] Room selected: {room.Name}");

        // Enable join button
        if (joinGameButton != null)
        {
            joinGameButton.interactable = true;
        }

        // Update status
        UpdateRoomListStatus($"Selected: {room.Name} ({room.PlayerCount}/{room.MaxPlayers} players)");

        // Highlight selected room
        HighlightSelectedRoom(room);
    }

    private void HighlightSelectedRoom(SessionInfo selectedRoom)
    {
        foreach (var item in _roomListItems)
        {
            if (item == null) continue;

            var roomNameText = item.transform.Find("RoomNameText")?.GetComponent<TextMeshProUGUI>();
            if (roomNameText != null)
            {
                // Check if this is the selected room
                bool isSelected = roomNameText.text == selectedRoom.Name;

                var bg = item.GetComponent<Image>();
                if (bg != null)
                {
                    bg.color = isSelected ? new Color(0.3f, 0.6f, 1f, 0.3f) : new Color(1f, 1f, 1f, 0.1f);
                }
            }
        }
    }

    private void ValidateRoomName(string roomName)
    {
        if (hostGameButton == null) return;

        bool isValid = !string.IsNullOrWhiteSpace(roomName) && roomName.Length >= 3 && roomName.Length <= 20;
        hostGameButton.interactable = isValid;

        if (!isValid && !string.IsNullOrEmpty(roomName))
        {
            if (roomName.Length < 3)
            {
                UpdateRoomListStatus("Room name must be at least 3 characters");
            }
            else if (roomName.Length > 20)
            {
                UpdateRoomListStatus("Room name must be 20 characters or less");
            }
        }
        else if (_isConnectedToLobby)
        {
            UpdateRoomListStatus($"Found {_availableRooms.Count} room(s)");
        }
    }

    private void UpdateRoomListStatus(string message)
    {
        if (roomListStatusText != null)
        {
            roomListStatusText.text = message;
        }
        Debug.Log($"[MainMenu] {message}");
    }

    // Callback handler class for Fusion
    private class RoomListCallbacks : INetworkRunnerCallbacks
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
=======

>>>>>>> Stashed changes
=======

>>>>>>> Stashed changes
    {
        private MainMenuController _controller;

        public RoomListCallbacks(MainMenuController controller)
        {
            _controller = controller;
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _controller.OnSessionListUpdated(sessionList);
        }

        // Required empty callbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }

    private void OnDestroy()
    {
        // Disconnect from lobby
        DisconnectFromLobby();

        // Clean up listeners
        if (playLocalButton != null)
            playLocalButton.onClick.RemoveListener(OnPlayLocal);

        if (playAIButton != null)
            playAIButton.onClick.RemoveListener(OnPlayAI);

        if (playOnlineButton != null)
            playOnlineButton.onClick.RemoveListener(OnPlayOnline);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettings);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuit);

        if (hostGameButton != null)
            hostGameButton.onClick.RemoveListener(OnHostGame);

        if (joinGameButton != null)
            joinGameButton.onClick.RemoveListener(OnJoinGame);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBack);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnBack);

        if (refreshRoomListButton != null)
            refreshRoomListButton.onClick.RemoveListener(RefreshRoomList);

        if (showCreateRoomButton != null)
            showCreateRoomButton.onClick.RemoveListener(ShowCreateRoomPanel);

        if (showRoomListButton != null)
            showRoomListButton.onClick.RemoveListener(ShowRoomListPanel);

        if (roomNameInput != null)
            roomNameInput.onValueChanged.RemoveListener(ValidateRoomName);

        // Clean up room list items
        foreach (var item in _roomListItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _roomListItems.Clear();
    }
}

