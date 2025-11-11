using System.Collections.Generic;
using UnityEngine;
using LAS.Core;
using LAS.Events;
using LAS.Entities;

namespace LAS.Gameplay
{
    /// <summary>
    /// Manages all players in the game
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        [System.Serializable]
        public class PlayerData
        {
            public string playerName;
            public Color playerColor;
            public GameObject piecePrefab;
            public int currentPosition = 1;
            public bool isAI = false;
            [HideInInspector] public PlayerPiece pieceInstance;
        }

        [Header("Player Configuration")]
        [SerializeField] private List<PlayerData> players = new List<PlayerData>();
        [SerializeField] private Transform spawnPoint;

        private IEventBus eventBus;
        private int maxPlayers = 4;

        private void Awake()
        {
            eventBus = ServiceLocator.Get<IEventBus>();
        }

        private void OnEnable()
        {
            eventBus?.Subscribe<PieceMovedEvent>(OnPieceMoved);
        }

        private void OnDisable()
        {
            eventBus?.Unsubscribe<PieceMovedEvent>(OnPieceMoved);
        }

        /// <summary>
        /// Initialize all players and spawn their pieces
        /// </summary>
        public void InitializePlayers()
        {
            for (int i = 0; i < players.Count; i++)
            {
                SpawnPlayerPiece(i);
                eventBus?.Publish(new PlayerJoinedEvent
                {
                    playerIndex = i,
                    playerName = players[i].playerName
                });
            }
        }

        /// <summary>
        /// Spawn a player piece on the board
        /// </summary>
        private void SpawnPlayerPiece(int playerIndex)
        {
            if (playerIndex >= players.Count) return;

            PlayerData player = players[playerIndex];

            if (player.piecePrefab != null)
            {
                Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
                GameObject pieceObj = Instantiate(player.piecePrefab, spawnPosition, Quaternion.identity);
                pieceObj.name = $"Player_{playerIndex}_{player.playerName}";

                PlayerPiece piece = pieceObj.GetComponent<PlayerPiece>();
                if (piece == null)
                {
                    piece = pieceObj.AddComponent<PlayerPiece>();
                }

                piece.playerIndex = playerIndex;
                piece.currentIndex = 1;
                player.pieceInstance = piece;

                // Apply player color
                Renderer renderer = pieceObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = player.playerColor;
                }
            }
        }

        /// <summary>
        /// Add a new player to the game
        /// </summary>
        public bool AddPlayer(string playerName, Color color, GameObject piecePrefab, bool isAI = false)
        {
            if (players.Count >= maxPlayers)
            {
                Debug.LogWarning("Maximum player count reached!");
                return false;
            }

            PlayerData newPlayer = new PlayerData
            {
                playerName = playerName,
                playerColor = color,
                piecePrefab = piecePrefab,
                isAI = isAI,
                currentPosition = 1
            };

            players.Add(newPlayer);
            return true;
        }

        /// <summary>
        /// Get player data by index
        /// </summary>
        public PlayerData GetPlayer(int index)
        {
            if (index >= 0 && index < players.Count)
                return players[index];
            return null;
        }

        /// <summary>
        /// Get total player count
        /// </summary>
        public int GetPlayerCount()
        {
            return players.Count;
        }

        /// <summary>
        /// Get player piece transform
        /// </summary>
        public Transform GetPlayerPieceTransform(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < players.Count)
            {
                return players[playerIndex].pieceInstance?.transform;
            }
            return null;
        }

        /// <summary>
        /// Get player piece component
        /// </summary>
        public PlayerPiece GetPlayerPiece(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < players.Count)
            {
                return players[playerIndex].pieceInstance;
            }
            return null;
        }

        /// <summary>
        /// Check if player is AI
        /// </summary>
        public bool IsAI(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < players.Count)
                return players[playerIndex].isAI;
            return false;
        }

        /// <summary>
        /// Update player position
        /// </summary>
        private void OnPieceMoved(PieceMovedEvent evt)
        {
            if (evt.playerIndex >= 0 && evt.playerIndex < players.Count)
            {
                players[evt.playerIndex].currentPosition = evt.to;
            }
        }

        /// <summary>
        /// Get player's current position
        /// </summary>
        public int GetPlayerPosition(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < players.Count)
                return players[playerIndex].currentPosition;
            return 1;
        }

        /// <summary>
        /// Clear all players (for restarting game)
        /// </summary>
        public void ClearPlayers()
        {
            foreach (var player in players)
            {
                if (player.pieceInstance != null)
                {
                    Destroy(player.pieceInstance.gameObject);
                }
            }
            players.Clear();
        }
    }
}
