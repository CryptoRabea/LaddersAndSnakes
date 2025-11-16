using UnityEngine;

namespace LAS.Config
{
    /// <summary>
    /// Configuration for game setup
    /// </summary>
    [CreateAssetMenu(fileName = "GameSetupConfig", menuName = "LAS/Game Setup Config")]
    public class GameSetupConfig : ScriptableObject
    {
        [Header("Player Settings")]
        [Tooltip("Number of players in the game (2-4)")]
        [Range(2, 4)]
        public int playerCount = 2;

        [Header("Game Mode")]
        [Tooltip("Enable AI opponents")]
        public bool enableAI = false;

        [Tooltip("Number of AI players")]
        [Range(0, 3)]
        public int aiPlayerCount = 0;

        [Header("Networking")]
        [Tooltip("Enable multiplayer networking")]
        public bool enableNetworking = false;

        [Tooltip("Network mode (Offline, Host, Client)")]
        public NetworkMode networkMode = NetworkMode.Offline;

        [Tooltip("Server address for client mode")]
        public string serverAddress = "localhost";

        [Header("Board Settings")]
        [Tooltip("Use procedurally generated board")]
        public bool useProceduralBoard = false;

        [Tooltip("Difficulty level for procedural generation")]
        public DifficultyLevel difficultyLevel = DifficultyLevel.Medium;

        [Tooltip("Random seed for board generation (0 = random)")]
        public int randomSeed = 0;

        [Header("Visual Settings")]
        [Tooltip("Camera distance from board")]
        public float cameraDistance = 15f;

        [Tooltip("Camera angle")]
        public float cameraAngle = 60f;

        [Tooltip("Animation speed multiplier")]
        public float animationSpeed = 1f;
    }

    public enum NetworkMode
    {
        Offline,
        Host,
        Client
    }
}
