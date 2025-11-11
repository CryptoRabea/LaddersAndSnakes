using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LAS.Core;
using LAS.Events;
using LAS.Gameplay;

namespace LAS.UI
{
    /// <summary>
    /// In-game HUD displaying player info, current turn, and game status
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI currentPlayerText;
        [SerializeField] private TextMeshProUGUI playerPositionText;
        [SerializeField] private Image currentPlayerColorIndicator;

        [Header("Dice Info")]
        [SerializeField] private TextMeshProUGUI diceResultText;
        [SerializeField] private Button rollDiceButton;

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI turnNumberText;
        [SerializeField] private TextMeshProUGUI gameStatusText;

        [Header("Action Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Event Log")]
        [SerializeField] private TextMeshProUGUI eventLogText;
        [SerializeField] private int maxLogLines = 5;

        private IEventBus eventBus;
        private GameController gameController;
        private int turnNumber = 1;
        private System.Collections.Generic.Queue<string> eventLog = new System.Collections.Generic.Queue<string>();

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void OnEnable()
        {
            eventBus?.Subscribe<GameStartedEvent>(OnGameStarted);
            eventBus?.Subscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);
            eventBus?.Subscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus?.Subscribe<SnakeHitEvent>(OnSnakeHit);
            eventBus?.Subscribe<LadderHitEvent>(OnLadderHit);
        }

        private void OnDisable()
        {
            eventBus?.Unsubscribe<GameStartedEvent>(OnGameStarted);
            eventBus?.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
            eventBus?.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
            eventBus?.Unsubscribe<SnakeHitEvent>(OnSnakeHit);
            eventBus?.Unsubscribe<LadderHitEvent>(OnLadderHit);
        }

        private void Start()
        {
            gameController = FindAnyObjectByType<GameController>();

            // Setup button listeners
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            if (pausePanel != null)
                pausePanel.SetActive(false);

            UpdateHUD();
        }

        private void OnGameStarted(GameStartedEvent evt)
        {
            turnNumber = 1;
            AddToEventLog($"Game started with {evt.playerCount} players!");
            UpdateHUD();
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            turnNumber++;
            UpdateHUD();
        }

        private void OnDiceRolled(DiceRolledEvent evt)
        {
            if (diceResultText != null)
                diceResultText.text = $"Rolled: {evt.result}";

            AddToEventLog($"Player {gameController.CurrentPlayer + 1} rolled {evt.result}");
        }

        private void OnPieceMoved(PieceMovedEvent evt)
        {
            AddToEventLog($"Player {evt.playerIndex + 1} moved from {evt.from} to {evt.to}");
            UpdateHUD();
        }

        private void OnSnakeHit(SnakeHitEvent evt)
        {
            AddToEventLog($"Player {evt.playerIndex + 1} hit a SNAKE! {evt.from} → {evt.to}");
            if (gameStatusText != null)
                gameStatusText.text = "Oh no! Snake bite!";
        }

        private void OnLadderHit(LadderHitEvent evt)
        {
            AddToEventLog($"Player {evt.playerIndex + 1} climbed a LADDER! {evt.from} → {evt.to}");
            if (gameStatusText != null)
                gameStatusText.text = "Nice! Climbed a ladder!";
        }

        private void UpdateHUD()
        {
            if (gameController == null) return;

            // Update current player
            if (currentPlayerText != null)
            {
                int currentPlayer = gameController.CurrentPlayer;
                var playerManager = gameController.GetPlayerManager();

                if (playerManager != null)
                {
                    var playerData = playerManager.GetPlayer(currentPlayer);
                    if (playerData != null)
                    {
                        currentPlayerText.text = $"Current: {playerData.playerName}";

                        // Update color indicator
                        if (currentPlayerColorIndicator != null)
                            currentPlayerColorIndicator.color = playerData.playerColor;
                    }
                }
                else
                {
                    currentPlayerText.text = $"Current: Player {currentPlayer + 1}";
                }
            }

            // Update player position
            if (playerPositionText != null && gameController.GetPlayerManager() != null)
            {
                int position = gameController.GetPlayerManager().GetPlayerPosition(gameController.CurrentPlayer);
                playerPositionText.text = $"Position: {position}";
            }

            // Update turn number
            if (turnNumberText != null)
                turnNumberText.text = $"Turn: {turnNumber}";

            // Clear status text after showing snake/ladder message
            if (gameStatusText != null && !gameStatusText.text.Contains("Snake") && !gameStatusText.text.Contains("Ladder"))
                gameStatusText.text = "Roll the dice!";
        }

        private void AddToEventLog(string message)
        {
            eventLog.Enqueue(message);

            if (eventLog.Count > maxLogLines)
                eventLog.Dequeue();

            UpdateEventLog();
        }

        private void UpdateEventLog()
        {
            if (eventLogText != null)
            {
                eventLogText.text = string.Join("\n", eventLog.ToArray());
            }
        }

        private void OnPauseClicked()
        {
            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        private void OnResumeClicked()
        {
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Load main menu
        }
    }
}
