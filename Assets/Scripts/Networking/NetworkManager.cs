using UnityEngine;
using System;
using System.Collections.Generic;
using LAS.Core;
using LAS.Events;

namespace LAS.Networking
{
    /// <summary>
    /// Simple multiplayer network manager supporting local and online play
    /// Uses a basic client-server architecture
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        public enum NetworkMode { Offline, Host, Client }
        public NetworkMode Mode { get; private set; } = NetworkMode.Offline;

        public bool IsConnected => Mode != NetworkMode.Offline;
        public bool IsHost => Mode == NetworkMode.Host;
        public int LocalPlayerId { get; private set; } = 0;
        public int PlayerCount => connectedPlayers.Count;
        public bool IsSinglePlayerAI => isSinglePlayerAI;

        private List<int> connectedPlayers = new List<int>();
        private Dictionary<int, PlayerData> playerDataMap = new Dictionary<int, PlayerData>();

        [Header("Network Settings")]
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private string serverAddress = "127.0.0.1";
        [SerializeField] private int serverPort = 7777;
        [SerializeField] private bool isSinglePlayerAI = false;

        public event Action<int> OnPlayerConnected;
        public event Action<int> OnPlayerDisconnected;
        public event Action OnConnectionEstablished;
        public event Action<string> OnConnectionFailed;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Start as host (server + client)
        /// </summary>
        public void StartHost(int localPlayerId = 0)
        {
            Mode = NetworkMode.Host;
            LocalPlayerId = localPlayerId;

            connectedPlayers.Clear();
            connectedPlayers.Add(localPlayerId);

            playerDataMap[localPlayerId] = new PlayerData
            {
                playerId = localPlayerId,
                playerName = $"Player {localPlayerId + 1}",
                isReady = true
            };

            Debug.Log($"[NetworkManager] Started as Host. Local Player ID: {localPlayerId}");
            OnConnectionEstablished?.Invoke();
        }

        /// <summary>
        /// Start as client and connect to host
        /// </summary>
        public void StartClient(string address = null, int playerId = 1)
        {
            Mode = NetworkMode.Client;
            LocalPlayerId = playerId;
            serverAddress = address ?? serverAddress;

            // Simulate connection (in real implementation, this would use Unity Netcode or similar)
            connectedPlayers.Add(playerId);
            playerDataMap[playerId] = new PlayerData
            {
                playerId = playerId,
                playerName = $"Player {playerId + 1}",
                isReady = false
            };

            Debug.Log($"[NetworkManager] Started as Client. Connecting to {serverAddress}:{serverPort}");
            OnConnectionEstablished?.Invoke();
        }

        /// <summary>
        /// Start local multiplayer (hotseat mode)
        /// </summary>
        public void StartLocalMultiplayer(int playerCount = 2)
        {
            Mode = NetworkMode.Host;
            LocalPlayerId = 0;
            isSinglePlayerAI = false;

            connectedPlayers.Clear();
            for (int i = 0; i < Mathf.Min(playerCount, maxPlayers); i++)
            {
                connectedPlayers.Add(i);
                playerDataMap[i] = new PlayerData
                {
                    playerId = i,
                    playerName = $"Player {i + 1}",
                    isReady = true
                };
            }

            Debug.Log($"[NetworkManager] Started Local Multiplayer with {playerCount} players");
            OnConnectionEstablished?.Invoke();
        }

        /// <summary>
        /// Start single player mode with AI opponent
        /// </summary>
        public void StartSinglePlayerWithAI()
        {
            Mode = NetworkMode.Host;
            LocalPlayerId = 0;
            isSinglePlayerAI = true;

            connectedPlayers.Clear();

            // Player 0 is the human player
            connectedPlayers.Add(0);
            playerDataMap[0] = new PlayerData
            {
                playerId = 0,
                playerName = "Player",
                isReady = true
            };

            // Player 1 is the AI
            connectedPlayers.Add(1);
            playerDataMap[1] = new PlayerData
            {
                playerId = 1,
                playerName = "AI",
                isReady = true
            };

            Debug.Log("[NetworkManager] Started Single Player vs AI mode");
            OnConnectionEstablished?.Invoke();
        }

        /// <summary>
        /// Disconnect from network
        /// </summary>
        public void Disconnect()
        {
            Debug.Log("[NetworkManager] Disconnecting...");
            Mode = NetworkMode.Offline;
            isSinglePlayerAI = false;
            connectedPlayers.Clear();
            playerDataMap.Clear();
        }

        /// <summary>
        /// Send network event to all players
        /// </summary>
        public void SendNetworkEvent<T>(T eventData) where T : struct
        {
            if (!IsConnected)
            {
                Debug.LogWarning("[NetworkManager] Cannot send event - not connected");
                return;
            }

            // In local multiplayer or host mode, broadcast immediately
            if (Mode == NetworkMode.Host)
            {
                ServiceLocator.Get<IEventBus>()?.Publish(eventData);
            }
            else
            {
                // In client mode, send to server
                // This would use actual network serialization in production
                ServiceLocator.Get<IEventBus>()?.Publish(eventData);
            }
        }

        /// <summary>
        /// Get list of connected player IDs
        /// </summary>
        public List<int> GetConnectedPlayers()
        {
            return new List<int>(connectedPlayers);
        }

        /// <summary>
        /// Get player data by ID
        /// </summary>
        public PlayerData GetPlayerData(int playerId)
        {
            return playerDataMap.TryGetValue(playerId, out var data) ? data : default;
        }

        /// <summary>
        /// Set player ready status
        /// </summary>
        public void SetPlayerReady(int playerId, bool ready)
        {
            if (playerDataMap.ContainsKey(playerId))
            {
                var data = playerDataMap[playerId];
                data.isReady = ready;
                playerDataMap[playerId] = data;
                Debug.Log($"[NetworkManager] Player {playerId} ready status: {ready}");
            }
        }

        /// <summary>
        /// Check if all players are ready
        /// </summary>
        public bool AllPlayersReady()
        {
            foreach (var player in connectedPlayers)
            {
                if (!playerDataMap[player].isReady)
                    return false;
            }
            return connectedPlayers.Count > 0;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Disconnect();
            }
        }
    }

    [Serializable]
    public struct PlayerData
    {
        public int playerId;
        public string playerName;
        public bool isReady;
        public int score;
    }
}
