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
/// </summary>
public class RoomListingManager : MonoBehaviour, INetworkRunnerCallbacks
{
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

    public event Action<string, int> OnJoinRoomRequested;

    void Start()
    {
        SetupUI();
        StartLobbyRunner();
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
        }

        if (maxPlayersInput != null)
        {
            maxPlayersInput.text = "4";
        }

        UpdateStatusText("Connecting to server...");
    }

    async void StartLobbyRunner()
    {
        if (_lobbyRunner != null)
        {
            return;
        }

        Debug.Log("Starting lobby runner for session discovery...");

        // Create a NetworkRunner for lobby/session list
        _lobbyRunner = gameObject.AddComponent<NetworkRunner>();
        _lobbyRunner.name = "LobbyRunner";
        _lobbyRunner.AddCallbacks(this);

        var result = await _lobbyRunner.JoinSessionLobby(SessionLobby.Custom);

        if (result.Ok)
        {
            Debug.Log("Successfully joined session lobby!");
            UpdateStatusText("Connected. Searching for rooms...");
            RefreshRoomList();
        }
        else
        {
            Debug.LogError($"Failed to join lobby: {result.ShutdownReason}");
            UpdateStatusText($"Connection failed: {result.ShutdownReason}");
        }
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
        string roomName = createRoomNameInput != null ? createRoomNameInput.text : "DefaultRoom";
        int maxPlayers = 4;

        if (maxPlayersInput != null && int.TryParse(maxPlayersInput.text, out int parsedPlayers))
        {
            maxPlayers = Mathf.Clamp(parsedPlayers, 2, 8);
        }

        if (string.IsNullOrWhiteSpace(roomName))
        {
            UpdateStatusText("Please enter a room name!");
            return;
        }

        Debug.Log($"Creating room: {roomName} with {maxPlayers} max players");
        CreateRoom(roomName, maxPlayers);
    }

    void CreateRoom(string roomName, int maxPlayers)
    {
        UpdateStatusText($"Creating room '{roomName}'...");

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
        Debug.Log($"Joining room: {roomName}");
        UpdateStatusText($"Joining room '{roomName}'...");

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

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("Lobby runner disconnected from server");
        UpdateStatusText("Disconnected from server");
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
        if (_lobbyRunner != null)
        {
            _lobbyRunner.Shutdown();
        }
    }
}
