using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LAS.Core;
using LAS.Events;
using LAS.Entities;
using LAS.Gameplay;

namespace LAS.UI
{
    /// <summary>
    /// Manages all in-game UI elements including dice button, turn indicator, and game over screen
    /// </summary>
    public class GameUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button rollDiceButton;
        [SerializeField] private TextMeshProUGUI turnIndicatorText;
        [SerializeField] private TextMeshProUGUI diceResultText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Game References")]
        [SerializeField] private DiceModel diceModel;
        [SerializeField] private GameController gameController;

        private IEventBus eventBus;
        private bool isRolling = false;

        private void Awake()
        {
            // Setup button listeners
            if (rollDiceButton != null)
                rollDiceButton.onClick.AddListener(OnRollDiceClicked);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Hide game over panel initially
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void Start()
        {
            eventBus = ServiceLocator.Get<IEventBus>();
            if (eventBus == null)
            {
                Debug.LogWarning("[GameUIManager] EventBus not found in ServiceLocator");
            }

            // Auto-find references if not assigned
            if (diceModel == null)
            {
                diceModel = FindObjectOfType<DiceModel>();
                if (diceModel == null)
                {
                    Debug.LogWarning("[GameUIManager] DiceModel not found in scene");
                }
            }

            if (gameController == null)
            {
                gameController = FindObjectOfType<GameController>();
                if (gameController == null)
                {
                    Debug.LogWarning("[GameUIManager] GameController not found in scene");
                }
            }

            // Subscribe to events
            if (eventBus != null)
            {
                eventBus.Subscribe<DiceRolledEvent>(OnDiceRolled);
                eventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
                eventBus.Subscribe<GameOverEvent>(OnGameOver);
            }

            UpdateTurnIndicator();
            UpdateDiceButton();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
            eventBus?.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);

            // Remove button listeners
            if (rollDiceButton != null)
                rollDiceButton.onClick.RemoveListener(OnRollDiceClicked);

            if (playAgainButton != null)
                playAgainButton.onClick.RemoveListener(OnPlayAgainClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        private void OnRollDiceClicked()
        {
            if (isRolling || diceModel == null)
                return;

            // Check if it's the local player's turn (for multiplayer)
            var mpController = gameController as Networking.MultiplayerGameController;
            if (mpController != null && !mpController.IsLocalPlayerTurn())
            {
                Debug.Log("Not your turn!");
                return;
            }

            isRolling = true;
            UpdateDiceButton();
            diceModel.Roll();
        }

        private void OnDiceRolled(DiceRolledEvent evt)
        {
            isRolling = false;

            // Update dice result text
            if (diceResultText != null)
                diceResultText.text = $"Rolled: {evt.result}";

            UpdateDiceButton();
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            UpdateTurnIndicator();
            UpdateDiceButton();
        }

        private void OnGameOver(GameOverEvent evt)
        {
            // Show game over panel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // Update winner text
            if (winnerText != null)
                winnerText.text = $"Player {evt.winnerIndex + 1} Wins!";

            // Disable dice button
            if (rollDiceButton != null)
                rollDiceButton.interactable = false;
        }

        private void UpdateTurnIndicator()
        {
            if (turnIndicatorText == null || gameController == null)
                return;

            int currentPlayer = gameController.CurrentPlayer;
            turnIndicatorText.text = $"Player {currentPlayer + 1}'s Turn";
        }

        private void UpdateDiceButton()
        {
            if (rollDiceButton == null)
                return;

            // Disable button while rolling or if not local player's turn
            bool canRoll = !isRolling;

            var mpController = gameController as Networking.MultiplayerGameController;
            if (mpController != null)
                canRoll = canRoll && mpController.IsLocalPlayerTurn();

            rollDiceButton.interactable = canRoll;

            // Update button text
            var buttonText = rollDiceButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = isRolling ? "Rolling..." : "Roll Dice";
        }

        private void OnPlayAgainClicked()
        {
            // Reload the game scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnMainMenuClicked()
        {
            // Load main menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
