using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manual Game Manager - Full developer control
/// Set up everything yourself in the scene:
/// - Position your own board
/// - Add your player prefabs
/// - Configure dice manually
/// - Adjust camera yourself
/// NO AUTOMATIC GENERATION - You control everything
/// </summary>
public class ManualGameManager : MonoBehaviour
{
    [Header("Manual Board Setup")]
    [Tooltip("Assign the board squares in order 1-100. Leave empty to generate positions logically.")]
    [SerializeField] private Transform[] boardSquares = new Transform[100];
    [SerializeField] private int boardSize = 100;

    [Header("Player Setup")]
    [Tooltip("Assign player piece prefabs you want to use")]
    [SerializeField] private GameObject[] playerPrefabs = new GameObject[4];
    [SerializeField] private int numberOfPlayers = 2;
    [SerializeField] private bool[] isAI = new bool[4]; // Mark which players are AI
    [SerializeField] private float playerHeightOffset = 0.5f;
    [SerializeField] private float playerSpacing = 0.3f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dice Setup")]
    [SerializeField] private ManualDiceRoller diceRoller;
    [SerializeField] private int numberOfDice = 1; // 1 or 2 dice

    [Header("Multiplayer")]
    [SerializeField] private bool isMultiplayer = false;
    [SerializeField] private int localPlayerIndex = 0;
    [SerializeField] private NetworkGameState networkState; // Network state sync

    [Header("UI References")]
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI diceText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Snakes and Ladders")]
    [Tooltip("Define your snakes and ladders here")]
    [SerializeField] private Jump[] jumps = new Jump[]
    {
        // Ladders
        new Jump { from = 4, to = 14, isLadder = true },
        new Jump { from = 9, to = 31, isLadder = true },
        new Jump { from = 21, to = 42, isLadder = true },
        new Jump { from = 28, to = 84, isLadder = true },
        new Jump { from = 51, to = 67, isLadder = true },
        new Jump { from = 72, to = 91, isLadder = true },
        new Jump { from = 80, to = 99, isLadder = true },

        // Snakes
        new Jump { from = 17, to = 7, isLadder = false },
        new Jump { from = 54, to = 34, isLadder = false },
        new Jump { from = 62, to = 19, isLadder = false },
        new Jump { from = 64, to = 60, isLadder = false },
        new Jump { from = 87, to = 36, isLadder = false },
        new Jump { from = 93, to = 73, isLadder = false },
        new Jump { from = 95, to = 75, isLadder = false },
        new Jump { from = 98, to = 79, isLadder = false }
    };

    [System.Serializable]
    public struct Jump
    {
        public int from;
        public int to;
        public bool isLadder;
    }

    // Game state
    private Dictionary<int, int> jumpMap = new Dictionary<int, int>();
    private List<GameObject> players = new List<GameObject>();
    private List<int> playerPositions = new List<int>();
    private int currentPlayer = 0;
    private bool isRolling = false;

    void Start()
    {
        // Check if we should configure from GameConfiguration
        if (GameConfiguration.Instance != null)
        {
            ApplyGameConfiguration();
        }

        InitializeGame();
    }

    /// <summary>
    /// Apply settings from GameConfiguration singleton
    /// </summary>
    void ApplyGameConfiguration()
    {
        var config = GameConfiguration.Instance;

        if (config.IsMultiplayer)
        {
            // Multiplayer mode - will be configured by NetworkGameManager
            Debug.Log("Multiplayer mode detected - waiting for NetworkGameManager configuration");
            isMultiplayer = true;
            numberOfPlayers = config.MaxMultiplayerPlayers;
        }
        else
        {
            // Single player mode
            Debug.Log($"Single player mode: {config.HumanPlayers} human, {config.AIPlayers} AI");
            isMultiplayer = false;
            numberOfPlayers = config.TotalPlayers;

            // Configure AI players
            bool[] aiConfig = config.GetAIPlayerArray();
            for (int i = 0; i < numberOfPlayers && i < isAI.Length; i++)
            {
                isAI[i] = aiConfig[i];
            }
        }
    }

    /// <summary>
    /// Configure game for multiplayer mode
    /// Call this from NetworkGameManager when network is ready
    /// </summary>
    public void ConfigureMultiplayer(NetworkGameState netState, int playerIndex, int totalPlayers)
    {
        isMultiplayer = true;
        localPlayerIndex = playerIndex;
        numberOfPlayers = totalPlayers;
        networkState = netState;

        // Subscribe to network events
        if (networkState != null)
        {
            networkState.OnDiceRolled += OnNetworkDiceRolled;
            networkState.OnTurnChanged += OnNetworkTurnChanged;
            networkState.OnGameEnded += OnNetworkGameEnded;
            networkState.SetGameManager(this);
        }

        Debug.Log($"Multiplayer configured: Player {localPlayerIndex} of {totalPlayers}");
        UpdateUI();
    }

    void InitializeGame()
    {
        // Build jump map
        jumpMap.Clear();
        foreach (var jump in jumps)
        {
            jumpMap[jump.from] = jump.to;
        }

        // Setup dice
        if (diceRoller != null)
        {
            diceRoller.SetNumberOfDice(numberOfDice);
        }

        // Spawn players
        SpawnPlayers();

        // Setup UI
        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.AddListener(OnRollButtonPressed);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        UpdateUI();

        // Only check AI turn if NOT in multiplayer mode
        // In multiplayer, this will be called after ConfigureMultiplayer
        if (!isMultiplayer)
        {
            CheckAITurn();
        }
    }

    void SpawnPlayers()
    {
        players.Clear();
        playerPositions.Clear();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerPositions.Add(0);

            if (i < playerPrefabs.Length && playerPrefabs[i] != null)
            {
                Vector3 startPos = GetSquarePosition(0) + new Vector3(i * playerSpacing, playerHeightOffset, 0);
                GameObject player = Instantiate(playerPrefabs[i], startPos, Quaternion.identity);
                player.name = "Player_" + (i + 1);
                players.Add(player);
            }
        }
    }

    Vector3 GetSquarePosition(int index)
    {
        // Use manual board squares if assigned
        if (index > 0 && index <= boardSquares.Length && boardSquares[index - 1] != null)
        {
            return boardSquares[index - 1].position;
        }

        // Fallback: logical grid position
        if (index == 0)
        {
            return new Vector3(-1.5f, 0, 0); // Off-board start
        }

        int row = (index - 1) / 10;
        int col = (index - 1) % 10;

        // Snake pattern
        if (row % 2 == 1)
        {
            col = 9 - col;
        }

        return new Vector3(col * 1.1f, 0, row * 1.1f);
    }

    void OnRollButtonPressed()
    {
        // Check multiplayer
        if (isMultiplayer && currentPlayer != localPlayerIndex)
        {
            ShowMessage("Not your turn!");
            return;
        }

        // Check if AI's turn
        if (currentPlayer < isAI.Length && isAI[currentPlayer])
        {
            ShowMessage("AI is thinking...");
            return;
        }

        if (!isRolling && diceRoller != null)
        {
            isRolling = true;
            if (rollDiceButton != null)
            {
                rollDiceButton.interactable = false;
            }

            // Use network state if multiplayer
            if (isMultiplayer && networkState != null)
            {
                networkState.RequestRollDice();
            }
            else
            {
                // Local single-player roll
                diceRoller.RollDice(OnDiceRolled);
            }
        }
    }

    void OnDiceRolled(int result)
    {
        if (diceText != null)
        {
            diceText.text = "Rolled: " + result;
        }

        ShowMessage($"Player {currentPlayer + 1} rolled {result}!");

        StartCoroutine(MovePlayer(currentPlayer, result));
    }

    IEnumerator MovePlayer(int playerIndex, int steps)
    {
        int currentPos = playerPositions[playerIndex];
        int newPos = currentPos + steps;

        // Exact roll to win
        if (newPos > boardSize)
        {
            ShowMessage($"Player {playerIndex + 1} needs exact roll!");
            yield return new WaitForSeconds(1.5f);

            // Only host advances turn in multiplayer
            if (!isMultiplayer || (networkState != null && networkState.Object.HasStateAuthority))
            {
                NextTurn();
            }
            yield break;
        }

        // Move step by step
        for (int i = currentPos + 1; i <= newPos; i++)
        {
            Vector3 targetPos = GetSquarePosition(i) + new Vector3(playerIndex * playerSpacing, playerHeightOffset, 0);
            yield return StartCoroutine(MoveTo(players[playerIndex], targetPos));
            playerPositions[playerIndex] = i;
        }

        yield return new WaitForSeconds(0.3f);

        // Check for jump (snake/ladder)
        if (jumpMap.ContainsKey(newPos))
        {
            int jumpTo = jumpMap[newPos];
            bool isLadder = jumpTo > newPos;

            ShowMessage(isLadder ?
                $"Player {playerIndex + 1} climbed a ladder!" :
                $"Player {playerIndex + 1} hit a snake!");

            yield return new WaitForSeconds(1f);

            Vector3 jumpPos = GetSquarePosition(jumpTo) + new Vector3(playerIndex * playerSpacing, playerHeightOffset, 0);
            yield return StartCoroutine(MoveTo(players[playerIndex], jumpPos));
            playerPositions[playerIndex] = jumpTo;
            newPos = jumpTo;
        }

        // Check win
        if (newPos >= boardSize)
        {
            GameOver(playerIndex);
            yield break;
        }

        // Only host advances turn in multiplayer
        if (!isMultiplayer || (networkState != null && networkState.Object.HasStateAuthority))
        {
            NextTurn();
        }
    }

    IEnumerator MoveTo(GameObject obj, Vector3 target)
    {
        Vector3 start = obj.transform.position;
        float duration = Vector3.Distance(start, target) / moveSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        obj.transform.position = target;
    }

    void NextTurn()
    {
        // Use network state if multiplayer
        if (isMultiplayer && networkState != null)
        {
            networkState.NextTurn();
            return; // Network will call OnNetworkTurnChanged
        }

        // Local single-player
        currentPlayer = (currentPlayer + 1) % numberOfPlayers;
        isRolling = false;

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = true;
        }

        UpdateUI();
        CheckAITurn();
    }

    void CheckAITurn()
    {
        // Disable AI in multiplayer mode
        if (isMultiplayer)
        {
            return;
        }

        if (currentPlayer < isAI.Length && isAI[currentPlayer])
        {
            // AI turn
            StartCoroutine(AITurn());
        }
    }

    IEnumerator AITurn()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false;
        }

        ShowMessage($"AI Player {currentPlayer + 1} is rolling...");

        if (diceRoller != null)
        {
            isRolling = true;
            diceRoller.RollDice(OnDiceRolled);
        }
    }

    void UpdateUI()
    {
        if (turnText != null)
        {
            string playerType = "";

            if (isMultiplayer)
            {
                // Show if it's the local player's turn
                if (currentPlayer == localPlayerIndex)
                {
                    playerType = " (YOUR TURN)";
                }
                else
                {
                    playerType = " (Waiting...)";
                }
            }
            else if (currentPlayer < isAI.Length && isAI[currentPlayer])
            {
                playerType = " (AI)";
            }

            turnText.text = $"Player {currentPlayer + 1}'s Turn{playerType}";
        }

        // Update roll button state in multiplayer
        if (rollDiceButton != null && isMultiplayer)
        {
            // Only enable button if it's the local player's turn and not currently rolling
            rollDiceButton.interactable = (currentPlayer == localPlayerIndex) && !isRolling;
        }

        // Update message text for multiplayer state
        if (messageText != null)
        {
            // Don't override specific messages, only update default state
            if (messageText.text == "" || messageText.text.Contains("Press and hold") ||
                messageText.text.Contains("Waiting for") || messageText.text.Contains("Your turn"))
            {
                if (isMultiplayer && currentPlayer == localPlayerIndex)
                {
                    messageText.text = "Your turn! Press and hold to shake dice, release to throw!";
                }
                else if (isMultiplayer)
                {
                    messageText.text = "Waiting for other player...";
                }
                else
                {
                    messageText.text = "Press and hold to shake dice, release to throw!";
                }
            }
        }
    }

    void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            messageText.text = msg;
        }
    }

    void GameOver(int winnerIndex)
    {
        // Use network state if multiplayer
        if (isMultiplayer && networkState != null && !networkState.GameEnded)
        {
            networkState.EndGame(winnerIndex);
            return; // Network will call OnNetworkGameEnded
        }

        // Local game over display
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            string playerType = (winnerIndex < isAI.Length && isAI[winnerIndex]) ? " (AI)" : "";
            winnerText.text = $"Player {winnerIndex + 1}{playerType} Wins!";
        }

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false;
        }

        ShowMessage($"Game Over! Player {winnerIndex + 1} reached 100!");
    }

    void PlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Call this to set number of dice (1 or 2)
    /// </summary>
    public void SetNumberOfDice(int count)
    {
        numberOfDice = Mathf.Clamp(count, 1, 2);
        if (diceRoller != null)
        {
            diceRoller.SetNumberOfDice(numberOfDice);
        }
    }

    #region Network Event Handlers

    /// <summary>
    /// Called when network dice roll result is received
    /// </summary>
    void OnNetworkDiceRolled(int result)
    {
        Debug.Log($"Network dice rolled: {result}");

        if (diceText != null)
        {
            diceText.text = "Rolled: " + result;
        }

        ShowMessage($"Player {currentPlayer + 1} rolled {result}!");

        // Show visual dice roll (non-interactive)
        if (diceRoller != null)
        {
            // Visual only - result already determined by network
            StartCoroutine(ShowNetworkDiceRoll(result));
        }
        else
        {
            // No visual roller, just move immediately
            StartCoroutine(MovePlayer(currentPlayer, result));
        }
    }

    /// <summary>
    /// Show dice animation for network result
    /// </summary>
    System.Collections.IEnumerator ShowNetworkDiceRoll(int result)
    {
        // Brief visual shake/animation
        yield return new WaitForSeconds(0.5f);

        if (diceText != null)
        {
            diceText.text = "Rolled: " + result;
        }

        // Start movement
        StartCoroutine(MovePlayer(currentPlayer, result));
    }

    /// <summary>
    /// Called when turn changes on network
    /// </summary>
    void OnNetworkTurnChanged(int newPlayer)
    {
        Debug.Log($"Network turn changed to: {newPlayer}");
        currentPlayer = newPlayer;
        isRolling = false;

        // UpdateUI will handle button state correctly based on whose turn it is
        UpdateUI();
    }

    /// <summary>
    /// Called when game ends on network
    /// </summary>
    void OnNetworkGameEnded(int winnerIndex)
    {
        Debug.Log($"Network game ended. Winner: {winnerIndex}");
        GameOver(winnerIndex);
    }

    #endregion

    void OnDestroy()
    {
        // Unsubscribe from network events
        if (networkState != null)
        {
            networkState.OnDiceRolled -= OnNetworkDiceRolled;
            networkState.OnTurnChanged -= OnNetworkTurnChanged;
            networkState.OnGameEnded -= OnNetworkGameEnded;
        }

        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.RemoveAllListeners();
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.RemoveAllListeners();
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}
