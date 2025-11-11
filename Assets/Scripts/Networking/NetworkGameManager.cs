using UnityEngine;
using Unity.Netcode;
using LAS.Core;
using LAS.Events;
using LAS.Gameplay;

namespace LAS.Networking
{
    /// <summary>
    /// Network-enabled game manager for online multiplayer
    /// Requires Unity Netcode for GameObjects package
    /// </summary>
    public class NetworkGameManager : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private GameController gameController;
        [SerializeField] private PlayerManager playerManager;

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 4;

        private NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0);
        private NetworkVariable<bool> isGameActive = new NetworkVariable<bool>(false);

        private IEventBus eventBus;

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            // Subscribe to game events
            eventBus?.Subscribe<DiceRolledEvent>(OnDiceRolled);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            eventBus?.Unsubscribe<DiceRolledEvent>(OnDiceRolled);
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");

            if (IsServer && NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                // Start game when we have at least 2 players
                StartNetworkGame();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
        }

        /// <summary>
        /// Start the networked game
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void StartNetworkGameServerRpc()
        {
            StartNetworkGame();
        }

        private void StartNetworkGame()
        {
            if (!IsServer) return;

            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            isGameActive.Value = true;
            currentPlayerIndex.Value = 0;

            // Initialize game on all clients
            InitializeGameClientRpc(playerCount);
        }

        [ClientRpc]
        private void InitializeGameClientRpc(int playerCount)
        {
            if (gameController != null)
            {
                gameController.StartGame(playerCount);
            }
        }

        /// <summary>
        /// Request to roll dice
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RollDiceServerRpc(ulong clientId)
        {
            if (!IsServer || !isGameActive.Value) return;

            // Verify it's this player's turn
            if (clientId != GetCurrentPlayerClientId())
            {
                Debug.LogWarning($"Client {clientId} tried to roll dice but it's not their turn");
                return;
            }

            // Generate dice roll on server
            int result = Random.Range(1, 7);

            // Send result to all clients
            BroadcastDiceRollClientRpc(result);
        }

        [ClientRpc]
        private void BroadcastDiceRollClientRpc(int result)
        {
            // Publish dice roll event locally
            eventBus?.Publish(new DiceRolledEvent
            {
                result = result,
                rawRoll = result
            });
        }

        private void OnDiceRolled(DiceRolledEvent evt)
        {
            // This is handled by the local GameController
        }

        /// <summary>
        /// Move player piece
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void MovePieceServerRpc(int playerIndex, int steps)
        {
            if (!IsServer) return;

            MovePieceClientRpc(playerIndex, steps);
        }

        [ClientRpc]
        private void MovePieceClientRpc(int playerIndex, int steps)
        {
            eventBus?.Publish(new MoveRequestedEvent
            {
                playerIndex = playerIndex,
                steps = steps
            });
        }

        /// <summary>
        /// End turn on server
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void EndTurnServerRpc()
        {
            if (!IsServer) return;

            currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % NetworkManager.Singleton.ConnectedClients.Count;

            EndTurnClientRpc(currentPlayerIndex.Value);
        }

        [ClientRpc]
        private void EndTurnClientRpc(int nextPlayerIndex)
        {
            eventBus?.Publish(new TurnEndedEvent
            {
                playerIndex = nextPlayerIndex
            });
        }

        /// <summary>
        /// Game over on server
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void GameOverServerRpc(int winnerIndex, string winnerName)
        {
            if (!IsServer) return;

            isGameActive.Value = false;
            GameOverClientRpc(winnerIndex, winnerName);
        }

        [ClientRpc]
        private void GameOverClientRpc(int winnerIndex, string winnerName)
        {
            eventBus?.Publish(new GameOverEvent
            {
                winnerIndex = winnerIndex,
                winnerName = winnerName
            });
        }

        private ulong GetCurrentPlayerClientId()
        {
            int index = 0;
            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {
                if (index == currentPlayerIndex.Value)
                    return client.Key;
                index++;
            }
            return 0;
        }
    }
}
