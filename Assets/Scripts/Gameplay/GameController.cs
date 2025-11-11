using UnityEngine;
using LAS.Core;
using LAS.Config;
using LAS.Events;


namespace LAS.Gameplay
{
    /// <summary>
    /// Main game controller that manages game flow, turns, and win conditions
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private GameConfig gameConfig;

        [Header("Game Settings")]
        [SerializeField] private int winningPosition = 100;
        [SerializeField] private bool exactRollToWin = true; // Must roll exact number to win

        public int CurrentPlayer { get; private set; } = 0;
        public bool IsGameActive { get; private set; } = false;
        public int PlayerCount { get; private set; } = 2;

        private GameState state;
        private IEventBus eventBus;

        void Awake()
        {
            // bootstrap core services
            ServiceLocator.Register<IEventBus>(new EventBus());
            ServiceLocator.Register(new PoolManager());
            eventBus = ServiceLocator.Get<IEventBus>();
        }

        void Start()
        {
            state = new IdleState();
            state.Enter(this);
            eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);
            eventBus?.Subscribe<PieceMovedEvent>(OnPieceMoved);
        }

        void OnDestroy()
        {
            eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
            eventBus?.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
            ServiceLocator.Clear();
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartGame(int playerCount = 2)
        {
            PlayerCount = playerCount;
            CurrentPlayer = 0;
            IsGameActive = true;

            if (playerManager != null)
            {
                playerManager.InitializePlayers();
            }

            eventBus?.Publish(new GameStartedEvent { playerCount = playerCount });
            TransitionTo(new IdleState());

            Debug.Log($"Game started with {playerCount} players!");
        }

        /// <summary>
        /// Handle dice rolled event
        /// </summary>
        void OnDiceRolled(DiceRolledEvent evt)
        {
            if (!IsGameActive) return;
            state.OnDiceRolled(evt.result);
        }

        /// <summary>
        /// Handle piece moved event - check for win condition
        /// </summary>
        void OnPieceMoved(PieceMovedEvent evt)
        {
            if (!IsGameActive) return;

            // Check win condition
            if (evt.to >= winningPosition)
            {
                WinGame(evt.playerIndex);
            }
        }

        /// <summary>
        /// Transition to a new game state
        /// </summary>
        public void TransitionTo(GameState newState)
        {
            state?.Exit();
            state = newState;
            state.Enter(this);
        }

        /// <summary>
        /// End current player's turn
        /// </summary>
        public void EndTurn()
        {
            if (!IsGameActive) return;

            eventBus?.Publish(new TurnEndedEvent { playerIndex = CurrentPlayer });

            CurrentPlayer = (CurrentPlayer + 1) % PlayerCount;

            Debug.Log($"Turn ended. Next player: {CurrentPlayer}");

            TransitionTo(new IdleState());

            // If next player is AI, trigger AI turn
            if (playerManager != null && playerManager.IsAI(CurrentPlayer))
            {
                Invoke(nameof(TriggerAITurn), 1f);
            }
        }

        /// <summary>
        /// Trigger AI player turn
        /// </summary>
        private void TriggerAITurn()
        {
            // AI automatically rolls after a delay
            var diceController = FindAnyObjectByType<Entities.DiceController>();
            if (diceController != null)
            {
                diceController.RollDice();
            }
        }

        /// <summary>
        /// Handle game win
        /// </summary>
        private void WinGame(int winnerIndex)
        {
            IsGameActive = false;

            string winnerName = "Player " + (winnerIndex + 1);
            if (playerManager != null)
            {
                var playerData = playerManager.GetPlayer(winnerIndex);
                if (playerData != null)
                {
                    winnerName = playerData.playerName;
                }
            }

            eventBus?.Publish(new GameOverEvent
            {
                winnerIndex = winnerIndex,
                winnerName = winnerName
            });

            Debug.Log($"Game Over! {winnerName} wins!");

            TransitionTo(new GameOverState());
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            CurrentPlayer = 0;
            IsGameActive = false;

            if (playerManager != null)
            {
                playerManager.ClearPlayers();
            }

            // Reload the scene or restart
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }

        /// <summary>
        /// Get player manager reference
        /// </summary>
        public PlayerManager GetPlayerManager()
        {
            return playerManager;
        }

        /// <summary>
        /// Check if exact roll is required to win
        /// </summary>
        public bool RequiresExactRollToWin()
        {
            return exactRollToWin;
        }

        /// <summary>
        /// Get winning position
        /// </summary>
        public int GetWinningPosition()
        {
            return winningPosition;
        }
    }
}