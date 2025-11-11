using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using LAS.Core;
using LAS.Events;
using LAS.Gameplay;

namespace LAS.UI
{
    /// <summary>
    /// Game Over screen UI
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Animation")]
        [SerializeField] private GameObject confettiEffect;

        private IEventBus eventBus;
        private GameController gameController;

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        private void OnEnable()
        {
            eventBus?.Subscribe<GameOverEvent>(OnGameOver);
        }

        private void OnDisable()
        {
            eventBus?.Unsubscribe<GameOverEvent>(OnGameOver);
        }

        private void Start()
        {
            gameController = FindAnyObjectByType<GameController>();

            // Setup button listeners
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnGameOver(GameOverEvent evt)
        {
            ShowGameOver(evt.winnerIndex, evt.winnerName);
        }

        public void ShowGameOver(int winnerIndex, string winnerName)
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // Display winner
            if (winnerText != null)
            {
                winnerText.text = $"🎉 {winnerName} Wins! 🎉";
            }

            // Display game statistics
            if (statsText != null)
            {
                statsText.text = GenerateGameStats(winnerIndex);
            }

            // Show confetti effect
            if (confettiEffect != null)
            {
                confettiEffect.SetActive(true);
            }
        }

        private string GenerateGameStats(int winnerIndex)
        {
            if (gameController == null || gameController.GetPlayerManager() == null)
                return "";

            var playerManager = gameController.GetPlayerManager();
            System.Text.StringBuilder stats = new System.Text.StringBuilder();

            stats.AppendLine("\n=== Final Positions ===\n");

            for (int i = 0; i < playerManager.GetPlayerCount(); i++)
            {
                var playerData = playerManager.GetPlayer(i);
                if (playerData != null)
                {
                    string positionText = i == winnerIndex ? "100 (WINNER!)" : playerData.currentPosition.ToString();
                    stats.AppendLine($"{playerData.playerName}: Position {positionText}");
                }
            }

            return stats.ToString();
        }

        private void OnPlayAgainClicked()
        {
            if (gameController != null)
            {
                gameController.RestartGame();
            }
            else
            {
                // Reload current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnMainMenuClicked()
        {
            // Load main menu scene (assumed to be scene 0)
            SceneManager.LoadScene(0);
        }
    }
}
