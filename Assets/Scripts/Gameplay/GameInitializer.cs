using UnityEngine;
using LAS.Gameplay;

/// <summary>
/// Helper script to initialize the game automatically when scene loads
/// Useful for testing directly in GameScene without going through MainMenu
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Auto Start Settings")]
    [SerializeField] private bool autoStartGame = true;
    [SerializeField] private int defaultPlayerCount = 2;
    [SerializeField] private float startDelay = 0.5f;

    [Header("References")]
    [SerializeField] private GameController gameController;

    private void Start()
    {
        if (autoStartGame)
        {
            // Check if we have a player count from MainMenu
            int playerCount = PlayerPrefs.GetInt("PlayerCount", defaultPlayerCount);

            Invoke(nameof(InitializeGame), startDelay);
        }
    }

    private void InitializeGame()
    {
        if (gameController == null)
        {
            gameController = FindAnyObjectByType<GameController>();
        }

        if (gameController != null)
        {
            int playerCount = PlayerPrefs.GetInt("PlayerCount", defaultPlayerCount);
            gameController.StartGame(playerCount);
            Debug.Log($"Game initialized with {playerCount} players");
        }
        else
        {
            Debug.LogError("GameController not found! Please assign it in the inspector.");
        }
    }

    /// <summary>
    /// Manually trigger game start (useful for buttons)
    /// </summary>
    public void StartGameWithPlayerCount(int playerCount)
    {
        PlayerPrefs.SetInt("PlayerCount", playerCount);

        if (gameController != null)
        {
            gameController.StartGame(playerCount);
        }
    }
}
