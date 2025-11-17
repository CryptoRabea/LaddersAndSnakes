using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using LAS.Core;
using LAS.Events;

namespace LAS.Networking
{
    /// <summary>
    /// Networked game manager using Unity Netcode for GameObjects
    /// Handles network synchronization of game events and state
    /// </summary>
    public class NetworkedGameManager : NetworkBehaviour
    {
        public static NetworkedGameManager Instance { get; private set; }

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private ushort port = 7777;

        private Dictionary<ulong, int> clientIdToPlayerIndex = new Dictionary<ulong, int>();
        private Dictionary<int, PlayerNetworkData> playerDataMap = new Dictionary<int, PlayerNetworkData>();
        private int nextPlayerIndex = 0;

        public int LocalPlayerIndex { get; private set; } = -1;
        public int PlayerCount => playerDataMap.Count;
        public bool IsGameStarted { get; private set; } = false;

        public event System.Action<int> OnPlayerJoined;
        public event System.Action<int> OnPlayerLeft;
        public event System.Action OnGameStarted;
        public event System.Action OnConnectionSuccess;
        public event System.Action<string> OnConnectionFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                Debug.Log("[NetworkedGameManager] Server started");
            }

            if (IsClient)
            {
                Debug.Log($"[NetworkedGameManager] Client connected. IsHost: {IsHost}");
                RequestPlayerIndexServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkedGameManager] Client {clientId} connected");

            if (playerDataMap.Count >= maxPlayers)
            {
                Debug.LogWarning($"[NetworkedGameManager] Server full, disconnecting client {clientId}");
                NetworkManager.Singleton.DisconnectClient(clientId);
                return;
            }

            // Assign player index
            int playerIndex = nextPlayerIndex++;
            clientIdToPlayerIndex[clientId] = playerIndex;

            playerDataMap[playerIndex] = new PlayerNetworkData
            {
                playerIndex = playerIndex,
                clientId = clientId,
                playerName = $"Player {playerIndex + 1}",
                isReady = false
            };

            Debug.Log($"[NetworkedGameManager] Assigned player index {playerIndex} to client {clientId}");

            // Notify all clients about the new player
            NotifyPlayerJoinedClientRpc(playerIndex, clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            if (clientIdToPlayerIndex.TryGetValue(clientId, out int playerIndex))
            {
                Debug.Log($"[NetworkedGameManager] Client {clientId} (Player {playerIndex}) disconnected");

                playerDataMap.Remove(playerIndex);
                clientIdToPlayerIndex.Remove(clientId);

                // Notify all clients
                NotifyPlayerLeftClientRpc(playerIndex);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestPlayerIndexServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
        {
            if (clientIdToPlayerIndex.TryGetValue(clientId, out int playerIndex))
            {
                // Send the player index back to the requesting client
                AssignPlayerIndexClientRpc(playerIndex, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                });
            }
        }

        [ClientRpc]
        private void AssignPlayerIndexClientRpc(int playerIndex, ClientRpcParams rpcParams = default)
        {
            LocalPlayerIndex = playerIndex;
            Debug.Log($"[NetworkedGameManager] Local player assigned index: {playerIndex}");
            OnConnectionSuccess?.Invoke();
        }

        [ClientRpc]
        private void NotifyPlayerJoinedClientRpc(int playerIndex, ulong clientId)
        {
            if (!playerDataMap.ContainsKey(playerIndex))
            {
                playerDataMap[playerIndex] = new PlayerNetworkData
                {
                    playerIndex = playerIndex,
                    clientId = clientId,
                    playerName = $"Player {playerIndex + 1}",
                    isReady = false
                };
            }

            Debug.Log($"[NetworkedGameManager] Player {playerIndex} joined");
            OnPlayerJoined?.Invoke(playerIndex);
        }

        [ClientRpc]
        private void NotifyPlayerLeftClientRpc(int playerIndex)
        {
            playerDataMap.Remove(playerIndex);
            Debug.Log($"[NetworkedGameManager] Player {playerIndex} left");
            OnPlayerLeft?.Invoke(playerIndex);
        }

        /// <summary>
        /// Start hosting a game
        /// </summary>
        public bool StartHost()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[NetworkedGameManager] NetworkManager.Singleton is null!");
                OnConnectionFailed?.Invoke("Network Manager not found");
                return false;
            }

            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("0.0.0.0", port);
            }

            bool started = NetworkManager.Singleton.StartHost();
            if (started)
            {
                Debug.Log("[NetworkedGameManager] Started as Host");
                return true;
            }
            else
            {
                Debug.LogError("[NetworkedGameManager] Failed to start as Host");
                OnConnectionFailed?.Invoke("Failed to start host");
                return false;
            }
        }

        /// <summary>
        /// Connect to a hosted game
        /// </summary>
        public bool StartClient(string serverAddress)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[NetworkedGameManager] NetworkManager.Singleton is null!");
                OnConnectionFailed?.Invoke("Network Manager not found");
                return false;
            }

            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData(serverAddress, port);
            }

            bool started = NetworkManager.Singleton.StartClient();
            if (started)
            {
                Debug.Log($"[NetworkedGameManager] Connecting to {serverAddress}:{port}");
                return true;
            }
            else
            {
                Debug.LogError("[NetworkedGameManager] Failed to start as Client");
                OnConnectionFailed?.Invoke("Failed to connect to server");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the network
        /// </summary>
        public void Disconnect()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
                Debug.Log("[NetworkedGameManager] Disconnected");
            }

            playerDataMap.Clear();
            clientIdToPlayerIndex.Clear();
            nextPlayerIndex = 0;
            LocalPlayerIndex = -1;
            IsGameStarted = false;
        }

        /// <summary>
        /// Broadcast dice roll event across network
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void BroadcastDiceRollServerRpc(int result, int rawRoll)
        {
            BroadcastDiceRollClientRpc(result, rawRoll);
        }

        [ClientRpc]
        private void BroadcastDiceRollClientRpc(int result, int rawRoll)
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new DiceRolledEvent { result = result, rawRoll = rawRoll });
        }

        /// <summary>
        /// Broadcast move request across network
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void BroadcastMoveRequestServerRpc(int playerIndex, int steps)
        {
            BroadcastMoveRequestClientRpc(playerIndex, steps);
        }

        [ClientRpc]
        private void BroadcastMoveRequestClientRpc(int playerIndex, int steps)
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new MoveRequestedEvent { playerIndex = playerIndex, steps = steps });
        }

        /// <summary>
        /// Broadcast piece moved event across network
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void BroadcastPieceMovedServerRpc(int playerIndex, int from, int to)
        {
            BroadcastPieceMovedClientRpc(playerIndex, from, to);
        }

        [ClientRpc]
        private void BroadcastPieceMovedClientRpc(int playerIndex, int from, int to)
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new PieceMovedEvent { playerIndex = playerIndex, from = from, to = to });
        }

        /// <summary>
        /// Broadcast turn ended event across network
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void BroadcastTurnEndedServerRpc(int playerIndex)
        {
            BroadcastTurnEndedClientRpc(playerIndex);
        }

        [ClientRpc]
        private void BroadcastTurnEndedClientRpc(int playerIndex)
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new TurnEndedEvent { playerIndex = playerIndex });
        }

        /// <summary>
        /// Broadcast game over event across network
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void BroadcastGameOverServerRpc(int winnerIndex)
        {
            BroadcastGameOverClientRpc(winnerIndex);
        }

        [ClientRpc]
        private void BroadcastGameOverClientRpc(int winnerIndex)
        {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new GameOverEvent { winnerIndex = winnerIndex });
        }

        /// <summary>
        /// Set player ready status
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerReadyServerRpc(int playerIndex, bool ready)
        {
            if (playerDataMap.ContainsKey(playerIndex))
            {
                var data = playerDataMap[playerIndex];
                data.isReady = ready;
                playerDataMap[playerIndex] = data;
                Debug.Log($"[NetworkedGameManager] Player {playerIndex} ready: {ready}");

                // Broadcast to all clients
                UpdatePlayerReadyClientRpc(playerIndex, ready);

                // Check if all players are ready
                if (AllPlayersReady() && playerDataMap.Count >= 2)
                {
                    StartGameClientRpc();
                }
            }
        }

        [ClientRpc]
        private void UpdatePlayerReadyClientRpc(int playerIndex, bool ready)
        {
            if (playerDataMap.ContainsKey(playerIndex))
            {
                var data = playerDataMap[playerIndex];
                data.isReady = ready;
                playerDataMap[playerIndex] = data;
            }
        }

        [ClientRpc]
        private void StartGameClientRpc()
        {
            IsGameStarted = true;
            Debug.Log("[NetworkedGameManager] Game starting!");
            OnGameStarted?.Invoke();
        }

        private bool AllPlayersReady()
        {
            foreach (var kvp in playerDataMap)
            {
                if (!kvp.Value.isReady)
                    return false;
            }
            return playerDataMap.Count > 0;
        }

        public List<int> GetConnectedPlayerIndices()
        {
            return new List<int>(playerDataMap.Keys);
        }

        public PlayerNetworkData GetPlayerData(int playerIndex)
        {
            return playerDataMap.TryGetValue(playerIndex, out var data) ? data : default;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Disconnect();
                Instance = null;
            }
        }
    }

    [System.Serializable]
    public struct PlayerNetworkData
    {
        public int playerIndex;
        public ulong clientId;
        public string playerName;
        public bool isReady;
        public int currentPosition;
    }
}
