using UnityEngine;

/// <summary>
/// Singleton that persists game configuration between scenes
/// Stores player setup and game mode settings
/// </summary>
public class GameConfiguration : MonoBehaviour
{
    private static GameConfiguration _instance;
    public static GameConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameConfiguration");
                _instance = go.AddComponent<GameConfiguration>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Game Mode")]
    public bool IsMultiplayer { get; private set; } = false;

    [Header("Single Player Settings")]
    public int HumanPlayers { get; private set; } = 1;
    public int AIPlayers { get; private set; } = 1;
    public int TotalPlayers => HumanPlayers + AIPlayers;

    [Header("Multiplayer Settings")]
    public string RoomName { get; private set; } = "LaddersAndSnakes";
    public int MaxMultiplayerPlayers { get; private set; } = 4;
    public bool IsHost { get; private set; } = false;
    public string PlayerName { get; private set; } = "";

    [Header("Connection Settings")]
    public float LastConnectionTime { get; private set; } = 0f;
    public int ConnectionAttempts { get; private set; } = 0;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configure for single player mode
    /// </summary>
    public void SetSinglePlayerMode(int humanPlayers, int aiPlayers)
    {
        IsMultiplayer = false;
        HumanPlayers = Mathf.Max(0, humanPlayers);
        AIPlayers = Mathf.Max(0, aiPlayers);

        Debug.Log($"Game configured for Single Player: {HumanPlayers} human, {AIPlayers} AI");
    }

    /// <summary>
    /// Configure for multiplayer mode
    /// </summary>
    public void SetMultiplayerMode(bool isHost, string roomName = "LaddersAndSnakes", int maxPlayers = 4, string playerName = "")
    {
        IsMultiplayer = true;
        IsHost = isHost;
        RoomName = SanitizeRoomName(roomName);
        MaxMultiplayerPlayers = Mathf.Clamp(maxPlayers, 2, 8);

        // Set player name from PlayerInfo if not provided
        if (string.IsNullOrEmpty(playerName))
        {
            PlayerName = PlayerInfo.LocalPlayerName;
        }
        else
        {
            PlayerName = PlayerInfo.SanitizePlayerName(playerName);
        }

        // Track connection attempt
        ConnectionAttempts++;
        LastConnectionTime = Time.time;

        // In multiplayer, no local AI players
        HumanPlayers = 1; // Local player
        AIPlayers = 0;

        Debug.Log($"Game configured for Multiplayer: {(isHost ? "Host" : "Client")}, Room: {RoomName}, Player: {PlayerName}");
    }

    /// <summary>
    /// Sanitize room name for safety
    /// </summary>
    string SanitizeRoomName(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            return "LaddersAndSnakes";
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

        if (string.IsNullOrEmpty(sanitized))
        {
            return "LaddersAndSnakes";
        }

        return sanitized;
    }

    /// <summary>
    /// Get AI status array for game manager
    /// </summary>
    public bool[] GetAIPlayerArray()
    {
        bool[] aiArray = new bool[TotalPlayers];

        // First HumanPlayers slots are false (human)
        // Remaining slots are true (AI)
        for (int i = 0; i < TotalPlayers; i++)
        {
            aiArray[i] = i >= HumanPlayers;
        }

        return aiArray;
    }

    /// <summary>
    /// Reset to defaults
    /// </summary>
    public void Reset()
    {
        IsMultiplayer = false;
        HumanPlayers = 1;
        AIPlayers = 1;
        RoomName = "LaddersAndSnakes";
        MaxMultiplayerPlayers = 4;
        IsHost = false;
        PlayerName = "";
        ConnectionAttempts = 0;
        LastConnectionTime = 0f;

        Debug.Log("GameConfiguration reset to defaults");
    }

    /// <summary>
    /// Get connection status summary
    /// </summary>
    public string GetConnectionStatus()
    {
        if (!IsMultiplayer)
        {
            return "Single Player Mode";
        }

        return $"{(IsHost ? "Host" : "Client")} | Room: {RoomName} | Players: {MaxMultiplayerPlayers} | Attempts: {ConnectionAttempts}";
    }
}

