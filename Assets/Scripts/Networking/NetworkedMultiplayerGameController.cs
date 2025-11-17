using UnityEngine;
using Unity.Netcode;
using LAS.Core;
using LAS.Events;
using LAS.Gameplay;

namespace LAS.Networking
{
    /// <summary>
    /// Networked game controller using Unity Netcode for GameObjects
    /// Extends GameController with true network synchronization
    /// </summary>
    public class NetworkedMultiplayerGameController : GameController
    {
        [Header("Network Settings")]
        [SerializeField] private bool requireNetworking = true;

        private NetworkedGameManager networkManager;
        private bool isNetworkedGame = false;

        protected override void Awake()
        {
            base.Awake();

            // Get network manager
            networkManager = NetworkedGameManager.Instance;

            if (networkManager == null && requireNetworking)
            {
                Debug.LogError("[NetworkedMultiplayerGameController] NetworkedGameManager not found!");
            }
            else if (networkManager != null)
            {
                isNetworkedGame = true;

                // Subscribe to network events
                networkManager.OnPlayerJoined += OnNetworkPlayerJoined;
                networkManager.OnPlayerLeft += OnNetworkPlayerLeft;
                networkManager.OnGameStarted += OnNetworkGameStarted;
            }
            else
            {
                Debug.LogWarning("[NetworkedMultiplayerGameController] Running without networking");
            }
        }

        protected override void Start()
        {
            base.Start();

            if (isNetworkedGame)
            {
                // Wait for all players to be ready before starting
                if (!networkManager.IsGameStarted)
                {
                    Debug.Log("[NetworkedMultiplayerGameController] Waiting for all players to be ready...");

                    // Auto-ready the local player
                    if (networkManager.LocalPlayerIndex >= 0)
                    {
                        networkManager.SetPlayerReadyServerRpc(networkManager.LocalPlayerIndex, true);
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (networkManager != null)
            {
                networkManager.OnPlayerJoined -= OnNetworkPlayerJoined;
                networkManager.OnPlayerLeft -= OnNetworkPlayerLeft;
                networkManager.OnGameStarted -= OnNetworkGameStarted;
            }
        }

        private void OnNetworkGameStarted()
        {
            Debug.Log($"[NetworkedMultiplayerGameController] Game started with {networkManager.PlayerCount} players");
            // Game is ready to start - the base Start() will initialize everything
        }

        private void OnNetworkPlayerJoined(int playerIndex)
        {
            Debug.Log($"[NetworkedMultiplayerGameController] Player {playerIndex} joined");
        }

        private void OnNetworkPlayerLeft(int playerIndex)
        {
            Debug.Log($"[NetworkedMultiplayerGameController] Player {playerIndex} left");

            // Handle player leaving during game
            // You might want to pause the game or end it
        }

        public override void EndTurn()
        {
            if (!isNetworkedGame)
            {
                // Local game - use base behavior
                base.EndTurn();
                return;
            }

            // Networked game - broadcast turn end
            ServiceLocator.Get<IEventBus>()?.Publish(new TurnEndedEvent { playerIndex = CurrentPlayer });

            // Move to next player
            int totalPlayers = networkManager.PlayerCount;
            CurrentPlayer = (CurrentPlayer + 1) % totalPlayers;

            // Broadcast to network (server will relay to all clients)
            if (networkManager != null)
            {
                networkManager.BroadcastTurnEndedServerRpc(CurrentPlayer);
            }

            TransitionTo(new IdleState());
        }

        /// <summary>
        /// Check if it's the local player's turn (for networked games)
        /// </summary>
        public bool IsLocalPlayerTurn()
        {
            if (!isNetworkedGame || networkManager == null)
                return true; // In offline mode, always allow

            return CurrentPlayer == networkManager.LocalPlayerIndex;
        }

        /// <summary>
        /// Check if player can take action (used by input handlers)
        /// </summary>
        public bool CanPlayerAct()
        {
            if (!isNetworkedGame)
                return true;

            return IsLocalPlayerTurn() && networkManager.IsGameStarted;
        }

        /// <summary>
        /// Broadcast dice roll to all players
        /// </summary>
        public void BroadcastDiceRoll(int result, int rawRoll)
        {
            if (isNetworkedGame && networkManager != null)
            {
                networkManager.BroadcastDiceRollServerRpc(result, rawRoll);
            }
            else
            {
                // Local game - publish directly
                ServiceLocator.Get<IEventBus>()?.Publish(new DiceRolledEvent { result = result, rawRoll = rawRoll });
            }
        }

        /// <summary>
        /// Broadcast move request to all players
        /// </summary>
        public void BroadcastMoveRequest(int playerIndex, int steps)
        {
            if (isNetworkedGame && networkManager != null)
            {
                networkManager.BroadcastMoveRequestServerRpc(playerIndex, steps);
            }
            else
            {
                // Local game - publish directly
                ServiceLocator.Get<IEventBus>()?.Publish(new MoveRequestedEvent { playerIndex = playerIndex, steps = steps });
            }
        }

        /// <summary>
        /// Broadcast piece moved to all players
        /// </summary>
        public void BroadcastPieceMoved(int playerIndex, int from, int to)
        {
            if (isNetworkedGame && networkManager != null)
            {
                networkManager.BroadcastPieceMovedServerRpc(playerIndex, from, to);
            }
            else
            {
                // Local game - publish directly
                ServiceLocator.Get<IEventBus>()?.Publish(new PieceMovedEvent { playerIndex = playerIndex, from = from, to = to });
            }
        }

        /// <summary>
        /// Broadcast game over to all players
        /// </summary>
        public void BroadcastGameOver(int winnerIndex)
        {
            if (isNetworkedGame && networkManager != null)
            {
                networkManager.BroadcastGameOverServerRpc(winnerIndex);
            }
            else
            {
                // Local game - publish directly
                ServiceLocator.Get<IEventBus>()?.Publish(new GameOverEvent { winnerIndex = winnerIndex });
            }
        }

        /// <summary>
        /// Get the total number of players in the game
        /// </summary>
        public int GetTotalPlayers()
        {
            if (isNetworkedGame && networkManager != null)
            {
                return networkManager.PlayerCount;
            }

            // Fallback for local games
            return 2;
        }

        /// <summary>
        /// Check if this is a networked game
        /// </summary>
        public bool IsNetworkedGame()
        {
            return isNetworkedGame;
        }

        /// <summary>
        /// Get the local player index
        /// </summary>
        public int GetLocalPlayerIndex()
        {
            if (isNetworkedGame && networkManager != null)
            {
                return networkManager.LocalPlayerIndex;
            }

            return 0;
        }
    }
}
