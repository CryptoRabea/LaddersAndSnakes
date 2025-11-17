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
    public void SetMultiplayerMode(bool isHost, string roomName = "LaddersAndSnakes", int maxPlayers = 4)
    {
        IsMultiplayer = true;
        IsHost = isHost;
        RoomName = roomName;
        MaxMultiplayerPlayers = Mathf.Clamp(maxPlayers, 2, 8);

        // In multiplayer, no local AI players
        HumanPlayers = 1; // Local player
        AIPlayers = 0;

        Debug.Log($"Game configured for Multiplayer: {(isHost ? "Host" : "Client")}, Room: {roomName}");
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
    }
}
