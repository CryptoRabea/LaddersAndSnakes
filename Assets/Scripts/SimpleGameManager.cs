using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Main game manager - handles everything for Snakes and Ladders
/// </summary>
public class SimpleGameManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardSize = 100;
    [SerializeField] private int boardWidth = 10;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private Transform boardParent;

    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int numberOfPlayers = 2;
    [SerializeField] private Color[] playerColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };

    [Header("UI")]
    [SerializeField] private Button rollDiceButton;
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI diceText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Game Settings")]
    [SerializeField] private float moveSpeed = 5f;

    // Game state
    private List<Vector3> squarePositions = new List<Vector3>();
    private Dictionary<int, int> snakesAndLadders = new Dictionary<int, int>();
    private List<GameObject> players = new List<GameObject>();
    private List<int> playerPositions = new List<int>();
    private int currentPlayer = 0;
    private bool isMoving = false;

    void Start()
    {
        // Get player count from PlayerPrefs (set by main menu)
        if (PlayerPrefs.HasKey("PlayerCount"))
        {
            numberOfPlayers = PlayerPrefs.GetInt("PlayerCount");
            numberOfPlayers = Mathf.Clamp(numberOfPlayers, 2, 4);
        }

        SetupSnakesAndLadders();
        GenerateBoard();
        SetupPlayers();
        SetupUI();
        UpdateTurnDisplay();
    }

    void SetupSnakesAndLadders()
    {
        // Ladders (going up)
        snakesAndLadders[4] = 14;
        snakesAndLadders[9] = 31;
        snakesAndLadders[21] = 42;
        snakesAndLadders[28] = 84;
        snakesAndLadders[51] = 67;
        snakesAndLadders[72] = 91;
        snakesAndLadders[80] = 99;

        // Snakes (going down)
        snakesAndLadders[17] = 7;
        snakesAndLadders[54] = 34;
        snakesAndLadders[62] = 19;
        snakesAndLadders[64] = 60;
        snakesAndLadders[87] = 36;
        snakesAndLadders[93] = 73;
        snakesAndLadders[95] = 75;
        snakesAndLadders[98] = 79;
    }

    void GenerateBoard()
    {
        // Create board parent if not assigned
        if (boardParent == null)
        {
            boardParent = new GameObject("Board").transform;
        }

        // Clear existing board
        foreach (Transform child in boardParent)
        {
            Destroy(child.gameObject);
        }
        squarePositions.Clear();

        // Add position 0 (off-board starting position)
        squarePositions.Add(new Vector3(-1.5f, 0, 0));

        // Generate 100 squares in snake pattern (bottom-left to top-right)
        for (int i = 0; i < boardSize; i++)
        {
            int row = i / boardWidth;
            int col = i % boardWidth;

            // Snake pattern - reverse every other row
            if (row % 2 == 1)
            {
                col = boardWidth - 1 - col;
            }

            Vector3 position = new Vector3(col * 1.1f, 0, row * 1.1f);
            squarePositions.Add(position);

            // Create visual square
            if (squarePrefab != null)
            {
                GameObject square = Instantiate(squarePrefab, position, Quaternion.identity, boardParent);
                square.name = "Square_" + (i + 1);

                // Color special squares
                Renderer renderer = square.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (snakesAndLadders.ContainsKey(i + 1))
                    {
                        int target = snakesAndLadders[i + 1];
                        if (target > i + 1)
                            renderer.material.color = Color.green; // Ladder
                        else
                            renderer.material.color = Color.red; // Snake
                    }
                    else if (i == 0)
                    {
                        renderer.material.color = Color.cyan; // Start
                    }
                    else if (i == boardSize - 1)
                    {
                        renderer.material.color = Color.yellow; // Win
                    }
                }

                // Add number text
                TextMeshPro numberText = square.GetComponentInChildren<TextMeshPro>();
                if (numberText != null)
                {
                    numberText.text = (i + 1).ToString();
                }
            }
        }

        // Center camera on board
        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(boardWidth * 0.55f, 15f, boardWidth * 0.55f);
            Camera.main.transform.rotation = Quaternion.Euler(60, 0, 0);
        }
    }

    void SetupPlayers()
    {
        players.Clear();
        playerPositions.Clear();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            playerPositions.Add(0); // Start at position 0

            if (playerPrefab != null)
            {
                Vector3 startPos = squarePositions[0] + new Vector3(i * 0.3f, 0.5f, 0);
                GameObject player = Instantiate(playerPrefab, startPos, Quaternion.identity);
                player.name = "Player_" + (i + 1);

                // Set player color
                Renderer renderer = player.GetComponent<Renderer>();
                if (renderer != null && i < playerColors.Length)
                {
                    renderer.material.color = playerColors[i];
                }

                players.Add(player);
            }
        }
    }

    void SetupUI()
    {
        if (rollDiceButton != null)
        {
            rollDiceButton.onClick.AddListener(RollDice);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (messageText != null)
        {
            messageText.text = "Click Roll Dice to start!";
        }
    }

    public void RollDice()
    {
        if (isMoving) return;

        int diceRoll = Random.Range(1, 7);

        if (diceText != null)
        {
            diceText.text = "Dice: " + diceRoll;
        }

        if (messageText != null)
        {
            messageText.text = $"Player {currentPlayer + 1} rolled a {diceRoll}!";
        }

        StartCoroutine(MovePlayer(currentPlayer, diceRoll));
    }

    IEnumerator MovePlayer(int playerIndex, int steps)
    {
        isMoving = true;

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false;
        }

        int currentPos = playerPositions[playerIndex];
        int newPos = currentPos + steps;

        // Don't move past the end
        if (newPos > boardSize)
        {
            if (messageText != null)
            {
                messageText.text = $"Player {playerIndex + 1} needs exact roll to win!";
            }
            yield return new WaitForSeconds(1f);
            NextTurn();
            yield break;
        }

        // Move step by step
        for (int i = currentPos + 1; i <= newPos; i++)
        {
            Vector3 targetPos = squarePositions[i] + new Vector3(playerIndex * 0.3f, 0.5f, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], targetPos, 0.3f));
            playerPositions[playerIndex] = i;
        }

        // Check for snake or ladder
        if (snakesAndLadders.ContainsKey(newPos))
        {
            int jumpTo = snakesAndLadders[newPos];
            bool isLadder = jumpTo > newPos;

            if (messageText != null)
            {
                messageText.text = isLadder ?
                    $"Player {playerIndex + 1} found a ladder! Going up!" :
                    $"Player {playerIndex + 1} hit a snake! Going down!";
            }

            yield return new WaitForSeconds(1f);

            Vector3 jumpPos = squarePositions[jumpTo] + new Vector3(playerIndex * 0.3f, 0.5f, 0);
            yield return StartCoroutine(MoveToPosition(players[playerIndex], jumpPos, 0.5f));
            playerPositions[playerIndex] = jumpTo;
            newPos = jumpTo;
        }

        // Check for win
        if (newPos >= boardSize)
        {
            GameOver(playerIndex);
            yield break;
        }

        NextTurn();
    }

    IEnumerator MoveToPosition(GameObject obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        obj.transform.position = targetPos;
    }

    void NextTurn()
    {
        currentPlayer = (currentPlayer + 1) % numberOfPlayers;
        UpdateTurnDisplay();
        isMoving = false;

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = true;
        }
    }

    void UpdateTurnDisplay()
    {
        if (turnText != null)
        {
            turnText.text = $"Player {currentPlayer + 1}'s Turn";
        }
    }

    void GameOver(int winnerIndex)
    {
        isMoving = false;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = $"Player {winnerIndex + 1} Wins!";
        }

        if (rollDiceButton != null)
        {
            rollDiceButton.interactable = false;
        }

        if (messageText != null)
        {
            messageText.text = $"Game Over! Player {winnerIndex + 1} reached square 100!";
        }
    }

    void PlayAgain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
