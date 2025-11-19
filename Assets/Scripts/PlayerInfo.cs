using UnityEngine;
using Fusion;
using System;

/// <summary>
/// Holds player information for multiplayer games
/// Includes player name, color, ready status, and network data
/// </summary>
[Serializable]
public struct NetworkPlayerInfo : INetworkStruct
{
    [Networked, Capacity(20)]
    public NetworkString<_16> PlayerName { get; set; }

    [Networked]
    public int PlayerIndex { get; set; }

    [Networked]
    public NetworkBool IsReady { get; set; }

    [Networked]
    public int ColorIndex { get; set; }

    [Networked]
    public NetworkBool IsConnected { get; set; }

    public static NetworkPlayerInfo Create(string name, int index, int colorIndex = 0)
    {
        return new NetworkPlayerInfo
        {
            PlayerName = name,
            PlayerIndex = index,
            IsReady = false,
            ColorIndex = colorIndex,
            IsConnected = true
        };
    }
}

/// <summary>
/// Manages player information in the lobby
/// Handles player names, ready status, and player list management
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    private static string _localPlayerName = "";

    /// <summary>
    /// Get or set the local player's name
    /// Persists across scenes
    /// </summary>
    public static string LocalPlayerName
    {
        get
        {
            if (string.IsNullOrEmpty(_localPlayerName))
            {
                // Try to load from PlayerPrefs
                _localPlayerName = PlayerPrefs.GetString("PlayerName", "");

                if (string.IsNullOrEmpty(_localPlayerName))
                {
                    // Generate random name if none exists
                    _localPlayerName = GenerateRandomName();
                    SaveLocalPlayerName(_localPlayerName);
                }
            }
            return _localPlayerName;
        }
        set
        {
            _localPlayerName = SanitizePlayerName(value);
            SaveLocalPlayerName(_localPlayerName);
        }
    }

    /// <summary>
    /// Save player name to PlayerPrefs
    /// </summary>
    static void SaveLocalPlayerName(string name)
    {
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Generate a random player name
    /// </summary>
    public static string GenerateRandomName()
    {
        string[] adjectives = { "Swift", "Brave", "Clever", "Lucky", "Mighty", "Noble", "Quick", "Wise", "Bold", "Fierce" };
        string[] nouns = { "Tiger", "Eagle", "Dragon", "Wolf", "Lion", "Hawk", "Bear", "Fox", "Falcon", "Panther" };

        string adjective = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
        string noun = nouns[UnityEngine.Random.Range(0, nouns.Length)];
        int number = UnityEngine.Random.Range(10, 99);

        return $"{adjective}{noun}{number}";
    }

    /// <summary>
    /// Sanitize player name for safety
    /// Removes invalid characters and enforces length limits
    /// </summary>
    public static string SanitizePlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return GenerateRandomName();
        }

        // Remove leading/trailing whitespace
        name = name.Trim();

        // Remove invalid characters (keep alphanumeric, spaces, and basic punctuation)
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-')
            {
                sb.Append(c);
            }
        }

        name = sb.ToString();

        // Enforce length limits
        if (name.Length > 15)
        {
            name = name.Substring(0, 15);
        }

        if (name.Length < 2)
        {
            return GenerateRandomName();
        }

        return name;
    }

    /// <summary>
    /// Validate player name
    /// </summary>
    public static bool IsValidPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.Length < 2 || name.Length > 15) return false;

        // Check for valid characters
        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ' && c != '_' && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get player color based on index
    /// </summary>
    public static Color GetPlayerColor(int colorIndex)
    {
        Color[] colors = new Color[]
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.5f, 0f, 1f), // Purple
            Color.cyan,
            Color.magenta
        };

        return colors[colorIndex % colors.Length];
    }

    /// <summary>
    /// Get player color name based on index
    /// </summary>
    public static string GetPlayerColorName(int colorIndex)
    {
        string[] colorNames = { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Cyan", "Magenta" };
        return colorNames[colorIndex % colorNames.Length];
    }
}
