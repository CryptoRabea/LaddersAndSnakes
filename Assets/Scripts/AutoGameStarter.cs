using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime script to automatically start the game when the scene loads.
/// Attach this to the GameManager or any persistent object in the scene.
/// </summary>
public class AutoGameStarter : MonoBehaviour
{
    [Header("Auto-Start Settings")]
    [Tooltip("Automatically start the game when this scene loads")]
    public bool autoStartGame = true;

    [Tooltip("Number of players (2-4)")]
    [Range(2, 4)]
    public int numberOfPlayers = 2;

    [Tooltip("Delay before auto-starting the game (in seconds)")]
    public float startDelay = 0.5f;

    [Tooltip("Automatically generate the board if not already generated")]
    public bool autoGenerateBoard = true;

    private void Start()
    {
        if (autoStartGame)
        {
            Invoke(nameof(StartGame), startDelay);
        }

        if (autoGenerateBoard)
        {
            GenerateBoardIfNeeded();
        }
    }

    private void StartGame()
    {
        // Find GameController
        GameController gameController = FindObjectOfType<GameController>();
        if (gameController == null)
        {
            Debug.LogError("AutoGameStarter: GameController not found in scene!");
            return;
        }

        // Find PlayerManager
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogError("AutoGameStarter: PlayerManager not found in scene!");
            return;
        }

        // Initialize players
        playerManager.InitializePlayers(numberOfPlayers);

        // Publish GameStartedEvent
        IEventBus eventBus = ServiceLocator.Get<IEventBus>();
        if (eventBus != null)
        {
            eventBus.Publish(new GameEvents.GameStartedEvent { playerCount = numberOfPlayers });
            Debug.Log($"AutoGameStarter: Game started with {numberOfPlayers} players");
        }
        else
        {
            Debug.LogWarning("AutoGameStarter: EventBus not found, game may not start correctly");
        }
    }

    private void GenerateBoardIfNeeded()
    {
        BoardGenerator boardGenerator = FindObjectOfType<BoardGenerator>();
        if (boardGenerator == null)
        {
            Debug.LogWarning("AutoGameStarter: BoardGenerator not found in scene");
            return;
        }

        // Check if board already has children (already generated)
        if (boardGenerator.transform.childCount > 0)
        {
            Debug.Log("AutoGameStarter: Board already generated, skipping auto-generation");
            return;
        }

        Debug.Log("AutoGameStarter: Auto-generating board...");
        // Note: Board generation is handled by the BoardGenerator component
        // In editor, this is done via the custom editor button
        // At runtime, you may need to implement a runtime generation method
    }

    /// <summary>
    /// Public method to manually start the game (can be called from UI buttons)
    /// </summary>
    public void ManualStartGame()
    {
        StartGame();
    }

    /// <summary>
    /// Restart the current scene
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Load a specific scene by name
    /// </summary>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
