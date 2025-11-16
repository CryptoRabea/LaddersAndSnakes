using UnityEngine;
using LAS.Core;
using LAS.Config;
using LAS.Events;
using LAS.Gameplay;

namespace LAS.Networking
{
    /// <summary>
    /// Extended game controller with multiplayer support
    /// Manages game state across multiple players
    /// </summary>
    public class MultiplayerGameController : GameController
    {
        [Header("Multiplayer Settings")]
        [SerializeField] private bool useNetworking = true;

        private NetworkManager networkManager;
        private int totalPlayers = 2;

        protected virtual void Awake()
        {
            base.Awake();

            // Get or create network manager
            networkManager = NetworkManager.Instance;
            if (networkManager == null && useNetworking)
            {
                Debug.LogWarning("[MultiplayerGameController] No NetworkManager found. Creating one...");
                var nmGO = new GameObject("NetworkManager");
                networkManager = nmGO.AddComponent<NetworkManager>();
            }

            // Subscribe to network events
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected += OnPlayerJoined;
                networkManager.OnPlayerDisconnected += OnPlayerLeft;
                totalPlayers = networkManager.PlayerCount;
            }
        }

        protected override void Start()
        {
            base.Start();

            if (networkManager != null && networkManager.IsConnected)
            {
                totalPlayers = networkManager.PlayerCount;
                Debug.Log($"[MultiplayerGameController] Game started with {totalPlayers} players");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from network events
            if (networkManager != null)
            {
                networkManager.OnPlayerConnected -= OnPlayerJoined;
                networkManager.OnPlayerDisconnected -= OnPlayerLeft;
            }
        }

        public override void EndTurn()
        {
            ServiceLocator.Get<IEventBus>()?.Publish(new TurnEndedEvent { playerIndex = CurrentPlayer });

            // Move to next player
            CurrentPlayer = (CurrentPlayer + 1) % totalPlayers;

            // Sync turn end across network
            if (networkManager != null && networkManager.IsHost)
            {
                networkManager.SendNetworkEvent(new TurnEndedEvent { playerIndex = CurrentPlayer });
            }

            TransitionTo(new IdleState());
        }

        private void OnPlayerJoined(int playerId)
        {
            Debug.Log($"[MultiplayerGameController] Player {playerId} joined");
            totalPlayers = networkManager.PlayerCount;
        }

        private void OnPlayerLeft(int playerId)
        {
            Debug.Log($"[MultiplayerGameController] Player {playerId} left");
            totalPlayers = networkManager.PlayerCount;

            // Handle player leaving during game
            if (CurrentPlayer >= totalPlayers)
            {
                CurrentPlayer = 0;
            }
        }

        /// <summary>
        /// Check if it's the local player's turn
        /// </summary>
        public bool IsLocalPlayerTurn()
        {
            if (networkManager == null || !networkManager.IsConnected)
                return true; // In offline mode, always allow

            return CurrentPlayer == networkManager.LocalPlayerId;
        }

        /// <summary>
        /// Get total number of players in the game
        /// </summary>
        public int GetTotalPlayers()
        {
            return totalPlayers;
        }
    }
}
